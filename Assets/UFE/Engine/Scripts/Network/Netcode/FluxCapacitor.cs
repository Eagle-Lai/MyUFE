using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UFENetcode;
using FPLibrary;
using UFE3D;

/// <summary>
/// 帧同步核心（FluxCapacitor）。
/// <para>用途：实现 UFE 网络对战的帧同步（Rollback Netcode）算法——管理输入预测/确认、帧延迟、回滚、</para>
/// <para>反同步检测与恢复、游戏历史快照（FluxGameHistory）、网络消息收发与调试信息。</para>
/// <para>支持"回滚"（Rollback，需 UFE_PRO）与"帧延迟"（Frame Delay）两种同步策略，</para>
/// <para>保证双方客户端在帧号一致的前提下按确定性规则推进游戏模拟。</para>
/// </summary>
public class FluxCapacitor {
	#region public class properties
	/// <summary>玩家索引越界提示消息模板。</summary>
	public static string PlayerIndexOutOfRangeMessage = 
	"The Player Index is {0}, but it should be in the [{1}, {2}] range.";

	/// <summary>收到意外玩家网络消息的提示消息模板。</summary>
	public static string NetworkMessageFromUnexpectedPlayerMessage = 
	"The Network Message was sent by {0}, but it was expected to be sent by {1}.";
	#endregion

	#region public instance properties
	/// <summary>
	/// 是否允许回滚（Rollback）——需配置启用回滚、游戏运行中且为网络对战。
	/// <para>菜单界面禁用回滚（按帧延迟算法行为，避免菜单状态机不支持回滚）。</para>
	/// </summary>
	public bool AllowRollbacks{
		get{
            //---------------------------------------------------------------------------------------------------------
            // Take into account that we will disable the remote player input prediction
            // in menu screens because we want this algorithm to behave as the frame-delay
            // algorithm in those screens (they aren't ready for dealing with rollbacks).
            //---------------------------------------------------------------------------------------------------------
            // FIXME: The current code will probably fail at "pause screen" and "after battle screens".
            //
            // Because when we try to disable rollbacks again, it's possible we already have some predicted inputs 
            // from the other player. A possible hack would be reseting the UFE.currentNetworkFrame and the input 
            // buffer when we detect one of these events, but we aren't completely sure about the undesirable 
            // side-effects which can appear.
            //---------------------------------------------------------------------------------------------------------
#if UFE_LITE || UFE_BASIC || UFE_STANDARD
            return false;
#else
            return UFE.config.networkOptions.allowRollBacks && UFE.gameRunning && this.IsNetworkGame();
#endif
        }
	}

	/// <summary>游戏历史（帧状态快照缓冲）。</summary>
	public FluxGameHistory History{
		get{
			return this._history;
		}
	}

	/// <summary>
	/// 当前网络帧延迟（帧数）——自动模式按网络延迟计算最优值，固定模式用默认值。
	/// </summary>
	public int NetworkFrameDelay{
		get{
			int frameDelay = 0;

			if (UFE.multiplayerAPI.Connections > 0){
				if (UFE.config.networkOptions.frameDelayType == global::NetworkFrameDelay.Auto){
					frameDelay = this.GetOptimalFrameDelay();

					if (this.AllowRollbacks){
						//---------------------------------------------------------------------------------------------
						// TODO: if one of the players get consistently more rollbacks than the other player, 
						// then we should increase the frame delay for that player in 1 or 2 frames because
						// using a greater frame-delay means having more input lag, but also less rollbacks.
						//---------------------------------------------------------------------------------------------
						// Another solution would be pausing the client which is receiving more rollbacks 
						// for a single frame in order to give the other client some time to catch up.
						//---------------------------------------------------------------------------------------------
					}
				}else{
					frameDelay = UFE.config.networkOptions.defaultFrameDelay;
				}
			}else if (UFE.config.networkOptions.applyFrameDelayOffline){
                if (UFE.config.networkOptions.frameDelayType == global::NetworkFrameDelay.Auto) {
					frameDelay = UFE.config.networkOptions.minFrameDelay;
				}else{
					frameDelay = UFE.config.networkOptions.defaultFrameDelay;
				}
			}

			return frameDelay;
		}
	}

	/// <summary>玩家管理器（双方输入缓冲）。</summary>
	public FluxPlayerManager PlayerManager{
		get{
			return this._playerManager;
		}
	}
#endregion

#region public instance fields
	/// <summary>保存的游戏状态（状态追踪器测试用）。</summary>
	public FluxStates? savedState = null;
#endregion

#region protected instance fields
	/// <summary>调试文本组件（帧同步调试信息显示）。</summary>
    protected Text debugger;
	/// <summary>调试信息字符串缓存。</summary>
	protected StringBuilder _debugInfo = new StringBuilder();
	/// <summary>游戏历史（帧状态快照）。</summary>
	protected FluxGameHistory _history = new FluxGameHistory();
	/// <summary>当前最大帧号记录。</summary>
	protected long _maxCurrentFrameValue = long.MinValue;
	/// <summary>玩家管理器。</summary>
	protected FluxPlayerManager _playerManager = new FluxPlayerManager();
	/// <summary>已收到的网络消息列表。</summary>
	protected List<byte[]> _receivedNetworkMessages = new List<byte[]>();
	/// <summary>双方当前选中的菜单选项。</summary>
	protected sbyte?[] _selectedOptions = new sbyte?[2];

	/// <summary>本地同步状态列表（反同步检测用）。</summary>
	protected List<FluxSimpleState> _localSynchronizationStates = new List<FluxSimpleState>();
	/// <summary>远端同步状态列表（反同步检测用）。</summary>
	protected List<FluxSimpleState> _remoteSynchronizationStates = new List<FluxSimpleState>();

	/// <summary>已发生反同步次数。</summary>
	protected int _desynchronizations = 0;
	/// <summary>远端玩家下一期望帧号。</summary>
	protected long _remotePlayerNextExpectedFrame;
	/// <summary>是否已应用回滚平衡。</summary>
	protected bool _rollbackBalancingApplied;
	/// <summary>距离下次发送网络消息的剩余时间。</summary>
	protected long _timeToNetworkMessage;
#endregion

#region public instance constructors
	public FluxCapacitor() : this(0){}
	public FluxCapacitor(long currentFrame) : this(currentFrame, -1){}
	public FluxCapacitor(long currentFrame, int maxHistoryLength){
		this.Initialize(currentFrame, maxHistoryLength);
	}
#endregion


#region public instance methods
	public void DoFixedUpdate(){
		bool allowRollbacks = this.AllowRollbacks;
		long currentFrame = UFE.currentFrame;
		long frameDelay = this.NetworkFrameDelay;
		long remotePlayerLastFrameReceived = this._remotePlayerNextExpectedFrame - 1;
		long remotePlayerExpectedFrame = remotePlayerLastFrameReceived + frameDelay;


		//-------------------------------------------------------------------------------------------------------------
		// Check if it's a network game...
		//-------------------------------------------------------------------------------------------------------------
		bool isNetworkGame = this.IsNetworkGame();
		if (isNetworkGame){
			//---------------------------------------------------------------------------------------------------------
			// In that case, process the received the network messages...
			//---------------------------------------------------------------------------------------------------------
			this.ProcessReceivedNetworkMessages();
			remotePlayerLastFrameReceived = this._remotePlayerNextExpectedFrame - 1;
			remotePlayerExpectedFrame = remotePlayerLastFrameReceived + frameDelay;

			//---------------------------------------------------------------------------------------------------------
			// If rollback balancing is enabled and it hasn't been applied in the current frame,
			// check if we need to apply the rollback balancing on this client.
			//
			// In order to avoid visual glitches, we want apply the rollback balancing at most one frame every second,
			// but we can become more aggressive if the desynchronization between clients is very big. If one client 
			// simulation is far ahead of the other client simulation (1 second or more), we pause that simulation
			// until the other client has time to catch up.
			//---------------------------------------------------------------------------------------------------------
			long rollbackBalancingFrameDelay = System.Math.Max(frameDelay, (long)this.GetOptimalFrameDelay());
			if(
				currentFrame > remotePlayerExpectedFrame + (long)(UFE.config.fps)
				||
				(
					!this._rollbackBalancingApplied 
					&&
					(
						UFE.config.networkOptions.rollbackBalancing != NetworkRollbackBalancing.Disabled &&
						UFE.currentFrame % UFE.config.fps == 0 &&
						currentFrame > remotePlayerExpectedFrame + rollbackBalancingFrameDelay / 2
						||
						UFE.config.networkOptions.rollbackBalancing == NetworkRollbackBalancing.Aggressive &&
						(
							UFE.currentFrame % (UFE.config.fps / 4) == 0 &&
							currentFrame > remotePlayerExpectedFrame + rollbackBalancingFrameDelay * 2
							||
							UFE.currentFrame % (UFE.config.fps / 2) == 0 &&
							currentFrame > remotePlayerExpectedFrame + rollbackBalancingFrameDelay
							||
							UFE.currentFrame % UFE.config.fps == 0 &&
							currentFrame > remotePlayerExpectedFrame + rollbackBalancingFrameDelay / 2
						)
					)
				)
			){
				//-----------------------------------------------------------------------------------------------------
				// If the game simulation on this client is far ahead in front of the simulation on the other client,
				// we will pause this client for a single frame in order to give the other simulation some time to 
				// catch up.
				//-----------------------------------------------------------------------------------------------------
				if (UFE.config.debugOptions.desyncErrorLog){
					this._debugInfo.Append("\n\nGame paused for one frame (Rollback Balancing Algorithm)\n\n");
				}

				this._rollbackBalancingApplied = true;
				this.CheckOutgoingNetworkMessages(currentFrame);
				return;
			}else{
				this.ReadInputs(frameDelay, allowRollbacks);
				this.CheckOutgoingNetworkMessages(currentFrame);
			}
		}else{
			this.ReadInputs(frameDelay, allowRollbacks);
		}

		long firstFrameWhereRollbackIsRequired = this.PlayerManager.GetFirstFrameWhereRollbackIsRequired();
#if UFE_LITE || UFE_BASIC || UFE_STANDARD
        bool rollback = false;
#else
        bool rollback = firstFrameWhereRollbackIsRequired >= 0 && firstFrameWhereRollbackIsRequired < UFE.currentFrame;
#endif
        long lastFrameWithConfirmedInput = this.PlayerManager.GetLastFrameWithConfirmedInput();
		long lastFrameWithSynchronizationMessage = Math.Min(this.GetFirstLocalSynchronizationFrame(),this.GetFirstRemoteSynchronizationFrame());
		long lastFrameWithSynchronizedInput = firstFrameWhereRollbackIsRequired >= 0 ? firstFrameWhereRollbackIsRequired - 1L : lastFrameWithConfirmedInput;

		//-------------------------------------------------------------------------------------------------------------
		// Remove the information which is no longer necessary:
		//-------------------------------------------------------------------------------------------------------------
		// We need to leave the confirmed information for a few extra frames
		// because we may need them later during a rollback.
		//-------------------------------------------------------------------------------------------------------------
		while(
			this.PlayerManager.player1.inputBuffer.FirstFrame < currentFrame - 1L 
			&&
			this.PlayerManager.player1.inputBuffer.FirstFrame < lastFrameWithSynchronizedInput - 1L 
			&&
			this.PlayerManager.player1.inputBuffer.FirstFrame < this._remotePlayerNextExpectedFrame
			&&
			(
				!UFE.config.networkOptions.desynchronizationRecovery || true
				||
				this.PlayerManager.player1.inputBuffer.FirstFrame < lastFrameWithSynchronizationMessage - 1L
				||
				this.PlayerManager.player1.inputBuffer.MaxBufferSize > 0 && 
				this.PlayerManager.player1.inputBuffer.Count > this.PlayerManager.player1.inputBuffer.MaxBufferSize * 3/4
			)
		){
			this.PlayerManager.player1.inputBuffer.RemoveNextInput();
		}

		while(
			this.PlayerManager.player2.inputBuffer.FirstFrame < currentFrame - 1L 
			&&
			this.PlayerManager.player2.inputBuffer.FirstFrame < lastFrameWithSynchronizedInput - 1L 
			&&
			this.PlayerManager.player2.inputBuffer.FirstFrame < this._remotePlayerNextExpectedFrame
			&&
			(
				!UFE.config.networkOptions.desynchronizationRecovery || true
				||
				this.PlayerManager.player2.inputBuffer.FirstFrame < lastFrameWithSynchronizationMessage - 1L
				||
				this.PlayerManager.player2.inputBuffer.MaxBufferSize > 0 && 
				this.PlayerManager.player2.inputBuffer.Count > this.PlayerManager.player2.inputBuffer.MaxBufferSize * 3/4
			)
		){
			this.PlayerManager.player2.inputBuffer.RemoveNextInput();
		}

		while(
			this._history.FirstStoredFrame < currentFrame - 1L 
			&&
			this._history.FirstStoredFrame < lastFrameWithSynchronizedInput - 1L 
			&&
			this._history.FirstStoredFrame < this._remotePlayerNextExpectedFrame
			&&
			(
				!UFE.config.networkOptions.desynchronizationRecovery || true
				||
				this._history.FirstStoredFrame < lastFrameWithSynchronizationMessage - 1L 
				||
				this._history.MaxBufferSize > 0 && 
				this._history.Count > this._history.MaxBufferSize * 3/4
			)
		){
			this._history.RemoveNextFrame();
		}
	
		//-------------------------------------------------------------------------------------------------------------
		// Check if it's a network game and we need to apply a rollback...
		//-------------------------------------------------------------------------------------------------------------
		if (isNetworkGame){
			// Check if we need to rollback to a previous frame...
			if (rollback){
				if (allowRollbacks){
                    // In that case, execute the rollback...
                    this.Rollback(currentFrame, firstFrameWhereRollbackIsRequired, lastFrameWithConfirmedInput);
                }
                else{
					// If a desynchronization has happened and we don't allow rollbacks, 
					// show a log message and go to the "Connection Lost" screen.
					if (UFE.config.debugOptions.desyncErrorLog){
						this._debugInfo
							.Append("\n\nCurrent Frame: ").Append(UFE.currentFrame)
							.Append(" | Rollback Frame: ").Append(firstFrameWhereRollbackIsRequired).AppendLine();
					}

					this.ForceDisconnection("Game Desynchronized because a rollback was required, but not allowed.");
				}
			}

            if (UFE.config.debugOptions.debugMode && UFE.config.debugOptions.networkToggle) {
                debugger.enabled = true;
                debugger.text = "";
                //if (UFE.config.debugOptions.ping) debugger.text += "Ping:" + UFE.multiplayerAPI.GetLastPing() + " ms\n";
                if (UFE.config.debugOptions.frameDelay) debugger.text += "Frame Delay:" + frameDelay + "\n";
                if (UFE.config.debugOptions.currentLocalFrame) debugger.text += "Current Frame:" + UFE.currentFrame;
            } else {
                debugger.enabled = false;
            }
		}else {
			this._remotePlayerNextExpectedFrame = lastFrameWithSynchronizedInput + 1L;
		}


		//-------------------------------------------------------------------------------------------------------------
		// We need to update these values again because they may have changed during the rollback and fast-foward
		//-------------------------------------------------------------------------------------------------------------
		firstFrameWhereRollbackIsRequired = this.PlayerManager.GetFirstFrameWhereRollbackIsRequired();
		lastFrameWithConfirmedInput = this.PlayerManager.GetLastFrameWithConfirmedInput();
		lastFrameWithSynchronizedInput = firstFrameWhereRollbackIsRequired >= 0 ? firstFrameWhereRollbackIsRequired - 1L : lastFrameWithConfirmedInput;
		currentFrame = UFE.currentFrame;

		//-------------------------------------------------------------------------------------------------------------
		// If the game isn't paused and all players have entered their input for the current frame...
		//-------------------------------------------------------------------------------------------------------------
		bool isInputReady;
		if (this.PlayerManager.TryCheckIfInputIsReady(UFE.currentFrame, out isInputReady) && isInputReady){
			this.ApplyInputs(currentFrame, lastFrameWithSynchronizedInput);
			this._rollbackBalancingApplied = false;
        }
	}

	/// <summary>
	/// 初始化帧同步（帧号0）。
	/// </summary>
	public virtual void Initialize(){
		this.Initialize(0);
	}

	/// <summary>
	/// 初始化帧同步（指定当前帧号）。
	/// </summary>
	/// <param name="currentFrame">当前帧号。</param>
	public virtual void Initialize(long currentFrame){
		this.Initialize(currentFrame, -1);
	}

	/// <summary>
	/// 初始化帧同步（完整参数）：重置历史/同步状态/缓冲，订阅回合与消息事件并创建调试文本。
	/// </summary>
	/// <param name="currentFrame">当前帧号。</param>
	/// <param name="maxHistoryLength">最大历史长度（-1 使用配置值）。</param>
	public virtual void Initialize(long currentFrame, int maxHistoryLength){
		this._debugInfo.Length = 0;
		this._debugInfo.Append("PLAYER ").Append(UFE.GetLocalPlayer()).Append(" - SYNCHRONIZATION LOG\n\n\n");

        if (maxHistoryLength == -1) maxHistoryLength = UFE.config.networkOptions.maxBufferSize;
		this.savedState = null;
		this._maxCurrentFrameValue = long.MinValue;
		this._localSynchronizationStates.Clear();
		this._remoteSynchronizationStates.Clear();
		this._history.Initialize(currentFrame, maxHistoryLength);
		this._remotePlayerNextExpectedFrame = currentFrame;
		this._rollbackBalancingApplied = false;
		this._timeToNetworkMessage = 0L;
		this._desynchronizations = 0;

        int maxBufferSize = UFE.config.networkOptions.maxBufferSize;
//		if (!UFE.config.networkOptions.sendNetworkMessagesEveryFrame){
//			maxBufferSize = Mathf.Max(maxBufferSize, UFE.config.networkOptions.defaultFrameDelay * 4);
//		}

        this.PlayerManager.Initialize(0, maxBufferSize);
		//this.PlayerManager.Initialize(0, -1);

		UFE.currentFrame = currentFrame;
		UFE.OnRoundEnds -= this.OnRoundEnds;
		UFE.OnRoundEnds += this.OnRoundEnds;
		UFE.OnRoundBegins -= this.OnRoundBegin;
		UFE.OnRoundBegins += this.OnRoundBegin;
		UFE.multiplayerAPI.OnMessageReceived -= this.OnMessageReceived;
		UFE.multiplayerAPI.OnMessageReceived += this.OnMessageReceived;

        // DEBUGGER
        debugger = UFE.DebuggerText("Debugger", "", new Vector2(-Screen.width + 50, Screen.height - 180), TextAnchor.UpperLeft);
	}

	/// <summary>
	/// 获取最优帧延迟（按当前网络 Ping 计算）。
	/// </summary>
	/// <returns>最优帧延迟。</returns>
	public virtual int GetOptimalFrameDelay(){
		return this.GetOptimalFrameDelay(UFE.multiplayerAPI.GetLastPing());
	}

	/// <summary>
	/// 获取最优帧延迟：按单程延迟与帧时长换算应延迟的帧数，并夹取到配置的最小/最大帧延迟。
	/// </summary>
	/// <param name="ping">当前网络 Ping 值（毫秒）。</param>
	/// <returns>最优帧延迟（帧数）。</returns>
	public virtual int GetOptimalFrameDelay(int ping){
		//-------------------------------------------------------------------------------------------------------------
		// Measure the time that a message needs to arrive at the other client and  calculate the duration
		// of each frame in seconds, so we can calculate the number of frames that will pass before the
		// network message arrives at the other client: that value will be the frame-delay.
		//-------------------------------------------------------------------------------------------------------------
		Fix64 latency = 0.001 * 0.5 * (Fix64)(ping);
		Fix64 frameDuration = (Fix64)1 / (Fix64)(UFE.config.fps);

		//-------------------------------------------------------------------------------------------------------------
		// Add one additional frame to the frame-delay, to compensate that messages could not being sent
		// until the next frame.
		//-------------------------------------------------------------------------------------------------------------
		int frameDelay = (int)FPMath.Ceiling(latency / frameDuration) + 1;
		return Mathf.Clamp(frameDelay,UFE.config.networkOptions.minFrameDelay,UFE.config.networkOptions.maxFrameDelay);
	}

	/// <summary>
	/// 请求记录某玩家的菜单选中选项（帧同步传递，用于选人/选场等界面）。
	/// </summary>
	/// <param name="player">玩家编号。</param>
	/// <param name="option">选中的选项（sbyte，-1 表示取消）。</param>
	public virtual void RequestOptionSelection(int player, sbyte option){
		if (player == 1 || player == 2){
			this._selectedOptions[player-1] = option;
		}
	}


	/// <summary>
	/// 开始回放录像：加载初始游戏状态并恢复双方输入缓冲。
	/// </summary>
	/// <param name="replay">录像数据。</param>
	public virtual void StartReplay(FluxGameReplay replay){
		if (replay != null && replay.Player1InputBuffer != null && replay.Player2InputBuffer != null){
            FluxStateTracker.LoadGameState(replay.InitialState);
			this.PlayerManager.GetPlayer(1)._inputBuffer = replay.Player1InputBuffer;
			this.PlayerManager.GetPlayer(2)._inputBuffer = replay.Player2InputBuffer;
		}
	}
#endregion

#region protected instance mehtods
	/// <summary>
	/// 应用当前帧输入：读取双方上一帧/当前帧输入转换为 UFE 输入字典，注入到双方控制器，
	/// 更新 GUI 调试信息，并应用已确认的菜单选中选项。
	/// </summary>
	/// <param name="currentFrame">当前帧号。</param>
	/// <param name="lastSynchronizedFrame">最后同步帧号。</param>
	protected virtual void ApplyInputs(long currentFrame, long lastSynchronizedFrame){
		bool synchronized = currentFrame <= lastSynchronizedFrame;

		//-------------------------------------------------------------------------------------------------------------
		// Retrieve the player 1 input in the previous frame
		//-------------------------------------------------------------------------------------------------------------
		UFEController player1Controller = this.PlayerManager.player1.inputController;

		FrameInput? player1PreviousFrameInput;
		bool foundPlayer1PreviousFrameInput = 
			this.PlayerManager.TryGetInput(1, currentFrame - 1, out player1PreviousFrameInput) &&
			player1PreviousFrameInput != null;

		if (!foundPlayer1PreviousFrameInput) player1PreviousFrameInput = new FrameInput(FrameInput.NullSelectedOption);

		Tuple<Dictionary<InputReferences, InputEvents>, sbyte?> player1PreviousTuple = 
			player1Controller.inputReferences.GetInputEvents(player1PreviousFrameInput.Value);

		IDictionary<InputReferences, InputEvents> player1PreviousInputs = player1PreviousTuple.Item1;
		sbyte? player1PreviousSelectedOption = player1PreviousTuple.Item2;

		//-------------------------------------------------------------------------------------------------------------
		// Retrieve the player 1 input in the current frame
		//-------------------------------------------------------------------------------------------------------------
		FrameInput? player1CurrentFrameInput;
		bool foundPlayer1CurrentFrameInput = 
			this.PlayerManager.TryGetInput(1, currentFrame, out player1CurrentFrameInput) &&
			player1CurrentFrameInput != null;

		if (!foundPlayer1CurrentFrameInput) player1CurrentFrameInput = new FrameInput(FrameInput.NullSelectedOption);

		Tuple<Dictionary<InputReferences, InputEvents>, sbyte?> player1CurrentTuple = 
			player1Controller.inputReferences.GetInputEvents(player1CurrentFrameInput.Value);

		IDictionary<InputReferences, InputEvents> player1CurrentInputs = player1CurrentTuple.Item1;
		sbyte? player1CurrentSelectedOption = player1CurrentTuple.Item2;

		int? player1SelectedOptions = null;
		if (player1CurrentSelectedOption != null && player1CurrentSelectedOption != player1PreviousSelectedOption){
			player1SelectedOptions = player1CurrentSelectedOption;
		}

		//-------------------------------------------------------------------------------------------------------------
		// Retrieve the player 2 input in the previous frame
		//-------------------------------------------------------------------------------------------------------------
		UFEController player2Controller = this.PlayerManager.player2.inputController;

		FrameInput? player2PreviousFrameInput;
		bool foundPlayer2PreviousFrameInput = 
			this.PlayerManager.TryGetInput(2, currentFrame - 1, out player2PreviousFrameInput) && 
			player2PreviousFrameInput != null ;
		
		if (!foundPlayer2PreviousFrameInput) player2PreviousFrameInput = new FrameInput(FrameInput.NullSelectedOption);

		Tuple<Dictionary<InputReferences, InputEvents>, sbyte?> player2PreviousTuple = 
			player2Controller.inputReferences.GetInputEvents(player2PreviousFrameInput.Value);

		IDictionary<InputReferences, InputEvents> player2PreviousInputs = player2PreviousTuple.Item1;
		sbyte? player2PreviousSelectedOption = player2PreviousTuple.Item2;


		//-------------------------------------------------------------------------------------------------------------
		// Retrieve the player 2 input in the current frame
		//-------------------------------------------------------------------------------------------------------------
		FrameInput? player2CurrentFrameInput;
		bool foundPlayer2CurrentFrameInput = 
			this.PlayerManager.TryGetInput(2, currentFrame, out player2CurrentFrameInput) &&
			player2CurrentFrameInput != null;

		if (!foundPlayer2CurrentFrameInput) player2CurrentFrameInput = new FrameInput(FrameInput.NullSelectedOption);

		Tuple<Dictionary<InputReferences, InputEvents>, sbyte?> player2CurrentTuple = 
			player2Controller.inputReferences.GetInputEvents(player2CurrentFrameInput.Value);

		IDictionary<InputReferences, InputEvents> player2CurrentInputs = player2CurrentTuple.Item1;
		sbyte? player2CurrentSelectedOption = player2CurrentTuple.Item2;

		int? player2SelectedOptions = null;
		if (player2CurrentSelectedOption != null && player2CurrentSelectedOption != player2PreviousSelectedOption){
			player2SelectedOptions = player2CurrentSelectedOption;
		}

		//-------------------------------------------------------------------------------------------------------------
		// Set the Random Seed
		//-------------------------------------------------------------------------------------------------------------
        UnityEngine.Random.InitState((int)currentFrame);

		//-------------------------------------------------------------------------------------------------------------
		// If the inputs are confirmed, send a synchronization message with the player positions at the current frame,
		// so the other client can check if the game remains synchronized across clients.
		//-------------------------------------------------------------------------------------------------------------
		FluxStates currentState = FluxStateTracker.SaveGameState(currentFrame);

        if (synchronized && this.IsNetworkGame()){
			int player = UFE.GetLocalPlayer();

			//---------------------------------------------------------------------------------------------------------
			// Check if we should send a synchronization message
			//---------------------------------------------------------------------------------------------------------
			if(
				UFE.config.networkOptions.synchronizationMessageFrequency == NetworkSynchronizationMessageFrequency.EveryFrame 
				||
				UFE.config.networkOptions.synchronizationMessageFrequency == NetworkSynchronizationMessageFrequency.EverySecond && 
				UFE.currentFrame % UFE.config.fps == 0
			){
				FluxSimpleState? receivedState = this.GetRemoteSynchronizationState(currentFrame);
				FluxSimpleState expectedState = new FluxSimpleState(currentState);

				UFE.multiplayerAPI.SendNetworkMessage(new SynchronizationMessage(player, currentFrame, expectedState));

				//-----------------------------------------------------------------------------------------------------
				// After sending the network message, check if we already have a "received state" for that frame
				//-----------------------------------------------------------------------------------------------------
				if (receivedState != null){
					//-------------------------------------------------------------------------------------------------
					// In that case, check if the current and the received value match.
					//-------------------------------------------------------------------------------------------------
					if (!this.SynchronizationCheck(expectedState, receivedState.Value, currentFrame)){
						return;
					}
				}else{
					//-------------------------------------------------------------------------------------------------
					// Otherwise, save the current value so we can try it again later.
					//-------------------------------------------------------------------------------------------------
					this._localSynchronizationStates.Add(expectedState);
				}
			}
		}

		//-------------------------------------------------------------------------------------------------------------
		// Before updating the state of the game, save the current state and the input that will be applied 
		// to reach the next frame state
		//-------------------------------------------------------------------------------------------------------------
		this._history.TrySetState(
			currentState,
			new FluxFrameInput(
				player1PreviousFrameInput.Value,
				player1CurrentFrameInput.Value,
				player2PreviousFrameInput.Value,
				player2CurrentFrameInput.Value
			)
		);

        //-------------------------------------------------------------------------------------------------------------
        // Write Debug Information
        //-------------------------------------------------------------------------------------------------------------
        if (!synchronized) {
            GenerateDebugLog(
                currentFrame,
                lastSynchronizedFrame,
                new FluxFrameInput(
                    player1PreviousFrameInput.Value,
                    player1CurrentFrameInput.Value,
                    player2PreviousFrameInput.Value,
                    player2CurrentFrameInput.Value
                ));
        }

		//-------------------------------------------------------------------------------------------------------------
		// Update the game state
		//-------------------------------------------------------------------------------------------------------------
        //if (UFE.gameRunning && !UFE.isPaused()) {
        if (!UFE.isPaused()) {
            this.UpdateTimer();
			this.UpdatePlayer(1, currentFrame, lastSynchronizedFrame, player1PreviousInputs, player1CurrentInputs);
			this.UpdatePlayer(2, currentFrame, lastSynchronizedFrame, player2PreviousInputs, player2CurrentInputs);
            this.UpdateInstantiatedObjects(currentFrame, lastSynchronizedFrame);
            if (UFE.cameraScript != null) UFE.cameraScript.DoFixedUpdate();
            if (UFE.gameRunning && !UFE.IsTimerPaused()) CheckEndRoundConditions();

            this.ExecuteSynchronizedDelayedActions();
        }

        this.ExecuteLocalDelayedActions();

		this.UpdateGUI(
			player1PreviousInputs, 
			player1CurrentInputs, 
			player1SelectedOptions,
			player2PreviousInputs, 
			player2CurrentInputs,
			player2SelectedOptions
		);

		this.PlayerManager.player1.inputController.DoFixedUpdate();
		this.PlayerManager.player2.inputController.DoFixedUpdate();

		//-------------------------------------------------------------------------------------------------------------
		// Finally, increment the frame count
		//-------------------------------------------------------------------------------------------------------------
		this._maxCurrentFrameValue = Math.Max(this._maxCurrentFrameValue, currentFrame);
		UFE.currentFrame = currentFrame + 1;
	}

    protected void CheckEndRoundConditions() {
        if (UFE.GetControlsScript(1).myInfo.currentLifePoints == 0 || UFE.GetControlsScript(2).myInfo.currentLifePoints == 0) {
            UFE.FireAlert(UFE.config.selectedLanguage.ko, null);

            if (UFE.GetControlsScript(1).myInfo.currentLifePoints == 0) UFE.PlaySound(UFE.GetControlsScript(1).myInfo.deathSound);
            if (UFE.GetControlsScript(2).myInfo.currentLifePoints == 0) UFE.PlaySound(UFE.GetControlsScript(2).myInfo.deathSound);

            UFE.PauseTimer();
            if (!UFE.config.roundOptions.allowMovementEnd) {
                UFE.config.lockMovements = true;
                UFE.config.lockInputs = true;
            }

            if (UFE.config.roundOptions.slowMotionKO) {
                UFE.timeScale = UFE.timeScale * UFE.config.roundOptions._slowMoSpeed;
                UFE.DelaySynchronizedAction(this.ReturnTimeScale, UFE.config.roundOptions._slowMoTimer);
                UFE.DelaySynchronizedAction(this.EndRound, 1 / UFE.config.roundOptions._slowMoSpeed);
            } else {
                UFE.DelaySynchronizedAction(this.EndRound, (Fix64)1);
            }
        }
    }

    public void ReturnTimeScale() {
        UFE.timeScale = UFE.config._gameSpeed;
    } 

    public void EndRound() {
        ControlsScript p1ControlScript = UFE.GetControlsScript(1);
        ControlsScript p2ControlScript = UFE.GetControlsScript(2);

        // Make sure both characters are grounded
        if (!p1ControlScript.Physics.IsGrounded() || !p2ControlScript.Physics.IsGrounded()) {
            UFE.DelaySynchronizedAction(this.EndRound, .5);
            return;
        }

        UFE.config.lockMovements = true;
        UFE.config.lockInputs = true;

        // Reset Stats
        p1ControlScript.KillCurrentMove();
        p2ControlScript.KillCurrentMove();

        p1ControlScript.ResetDrainStatus(true);
        p2ControlScript.ResetDrainStatus(true);

        // Clear All Projectiles
        foreach (ProjectileMoveScript projectileMoveScript in p1ControlScript.projectiles) {
            if (projectileMoveScript != null) projectileMoveScript.destroyMe = true;
        }
        foreach (ProjectileMoveScript projectileMoveScript in p2ControlScript.projectiles) {
            if (projectileMoveScript != null) projectileMoveScript.destroyMe = true;
        }

        // Check Winner
        if (p1ControlScript.myInfo.currentLifePoints == 0 && p2ControlScript.myInfo.currentLifePoints == 0) {
            UFE.FireAlert(UFE.config.selectedLanguage.draw, null);
            UFE.DelaySynchronizedAction(this.NewRound, UFE.config.roundOptions._newRoundDelay);
        } else {
            if (p1ControlScript.myInfo.currentLifePoints == 0) {
                SetWinner(p2ControlScript);
            } else if (p2ControlScript.myInfo.currentLifePoints == 0) {
                SetWinner(p1ControlScript);
            }
        }
    }

    protected void SetWinner(ControlsScript winner) {
        ++winner.roundsWon;
        UFE.FireRoundEnds(winner.myInfo, winner.opInfo);

        // Start New Round or End Game
        if (winner.roundsWon > Mathf.Ceil(UFE.config.roundOptions.totalRounds / 2) || winner.challengeMode != null) {
            winner.SetMoveToOutro();
            UFE.DelaySynchronizedAction(this.KillCam, UFE.config.roundOptions._endGameDelay);
            UFE.FireGameEnds(winner.myInfo, winner.opInfo);
        } else {
            UFE.DelaySynchronizedAction(this.NewRound, UFE.config.roundOptions._newRoundDelay);
        }
    }

    protected void NewRound() {
        ControlsScript p1ControlScript = UFE.GetControlsScript(1);
        ControlsScript p2ControlScript = UFE.GetControlsScript(2);

        p1ControlScript.potentialBlock = false;
        p2ControlScript.potentialBlock = false;
        if (UFE.config.roundOptions.resetPositions) {
            CameraFade.StartAlphaFade(UFE.config.gameGUI.roundFadeColor, false, (float)UFE.config.gameGUI.roundFadeDuration / 2);
            UFE.DelaySynchronizedAction(this.StartNewRound, UFE.config.gameGUI.roundFadeDuration / 2);
        } else {
            UFE.DelaySynchronizedAction(this.StartNewRound, (Fix64)2);
        }

        if (p1ControlScript.challengeMode != null) p1ControlScript.challengeMode.Run();
    }
    

	/// <summary>
	/// 开始新回合（帧同步内执行）：递增回合数、重置计时器/双方数据/位置、锁定输入并广播新回合。
	/// </summary>
	protected void StartNewRound(){
        ControlsScript p1ControlScript = UFE.GetControlsScript(1);
        ControlsScript p2ControlScript = UFE.GetControlsScript(2);

        UFE.config.currentRound ++;
		UFE.ResetTimer();

        p1ControlScript.ResetData(true);
        p2ControlScript.ResetData(false);
        if (UFE.config.roundOptions.resetPositions) {
            CameraFade.StartAlphaFade(UFE.config.gameGUI.roundFadeColor, true, (float)UFE.config.gameGUI.roundFadeDuration / 2);
            p1ControlScript.cameraScript.ResetCam();
        }

		UFE.config.lockInputs = true;
		UFE.ResetRoundCast();
		UFE.CastNewRound();

		if (UFE.config.roundOptions.allowMovementStart) {
			UFE.config.lockMovements = false;
		}else{
			UFE.config.lockMovements = true;
		}
	}

	/// <summary>
	/// 停止摄像机移动（K.O. 演出后冻结镜头）。
	/// </summary>
    protected void KillCam() {
        UFE.GetControlsScript(1).cameraScript.killCamMove = true;
    }

	/// <summary>
	/// 生成同步调试日志（反同步日志模式启用时输出双方角色/物理/招式状态对比）。
	/// </summary>
	/// <param name="currentFrame">当前帧号。</param>
	/// <param name="lastSynchronizedFrame">最后同步帧号。</param>
	/// <param name="frameInput">当前帧输入。</param>
    protected virtual void GenerateDebugLog(long currentFrame, long lastSynchronizedFrame, FluxFrameInput frameInput) {
        if (UFE.config.debugOptions.desyncErrorLog) {
            FluxStates state;
            bool historyRetrieved =
                this._history.TryGetState(currentFrame, out state) &&
                state.player1.controlsScript &&
                state.player2.controlsScript;

            this._debugInfo.Append("\nDesync detected at frame ").Append(currentFrame).AppendLine();

            if (frameInput.Player1PreviousInput.Equals(null)) {
                this._debugInfo
                    .Append("\nPrevious Player 1 Input not found at frame: ").Append(currentFrame)
                    .Append("\nLast Synchronized Frame: ").Append(lastSynchronizedFrame)
                    .Append("\nFirst Input at Player 1 Input Buffer: ").Append(this.PlayerManager.player1.inputBuffer.FirstFrame)
                    .Append("\nLast Input at Player 1 Input Buffer: ").Append(this.PlayerManager.player1.inputBuffer.LastFrame)
                    .Append("\nLast Confirmed Input for Player 1: ").Append(this.PlayerManager.player1.inputBuffer.GetLastFrameWithConfirmedInput())
                    .AppendLine();
            }
            
            if (frameInput.Player2PreviousInput.Equals(null)) {
                this._debugInfo
                    .Append("\nPrevious Player 2 Input not found at frame: ").Append(currentFrame)
                    .Append("\nLast Synchronized Frame: ").Append(lastSynchronizedFrame)
                    .Append("\nFirst Input at Player 2 Input Buffer: ").Append(this.PlayerManager.player2.inputBuffer.FirstFrame)
                    .Append("\nLast Input at Player 2 Input Buffer: ").Append(this.PlayerManager.player2.inputBuffer.LastFrame)
                    .Append("\nLast Confirmed Input for Player 2: ").Append(this.PlayerManager.player2.inputBuffer.GetLastFrameWithConfirmedInput())
                    .AppendLine();
            }

            this._debugInfo
                .Append("\nPlayer1 Previous Input: ").Append(frameInput.Player1PreviousInput)
                .Append("\nPlayer1 Current Input: ").Append(frameInput.Player1CurrentInput);

            if (historyRetrieved) {
                this._debugInfo
                    .Append("\nPlayer1 Position: (")
                    .Append(state.player1.shellTransform.fpPosition.x)
                    .Append(", ")
                    .Append(state.player1.shellTransform.fpPosition.y)
                    .Append(", ")
                    .Append(state.player1.shellTransform.fpPosition.z)
                    .Append(")");

                if (state.player1.moveSet.animator.currentAnimationData.mecanimAnimationData != null) {
                    this._debugInfo
                        .Append("\nPlayer1 Animation: " + state.player1.moveSet.animator.currentAnimationData.mecanimAnimationData.clipName)
                        .Append("\nPlayer1 Animation Time: " + state.player1.moveSet.animator.currentAnimationData.mecanimAnimationData.secondsPlayed);
                }
            }

            this._debugInfo
                .Append("\nPlayer2 Previous Input: ").Append(frameInput.Player2PreviousInput)
                .Append("\nPlayer2 Current Input: ").Append(frameInput.Player2CurrentInput);

            if (historyRetrieved) {
                this._debugInfo
                    .Append("\nPlayer2 Position: (")
                    .Append(state.player2.shellTransform.fpPosition.x)
                    .Append(", ")
                    .Append(state.player2.shellTransform.fpPosition.y)
                    .Append(", ")
                    .Append(state.player2.shellTransform.fpPosition.z)
                    .Append(")")
                    .Append("\nPlayer2 Basic Move: " + state.player2.currentBasicMove)
                    .Append("\nPlayer2 Move: " + (state.player2.currentMove.move != null ? state.player2.currentMove.move.moveName : string.Empty));

                if (state.player2.moveSet.animator.currentAnimationData.mecanimAnimationData != null) {
                    this._debugInfo
                        .Append("\nPlayer2 Animation: " + state.player2.moveSet.animator.currentAnimationData.mecanimAnimationData.clipName)
                        .Append("\nPlayer2 Animation Time: " + state.player2.moveSet.animator.currentAnimationData.mecanimAnimationData.secondsPlayed);
                }
            }

            this._debugInfo.AppendLine();
        }
    }

	/// <summary>
	/// 检查并发送出站网络消息：按输入消息频率决定每帧发送或隔帧发送，
	/// 且当本地输入变化时立即发送以避免"超级回滚"（mega-rollbacks）。
	/// </summary>
	/// <param name="currentFrame">当前帧号。</param>
	protected virtual void CheckOutgoingNetworkMessages(long currentFrame){
		//---------------------------------------------------------------------------------------------------------
		// Check if we need to send a network message
		//---------------------------------------------------------------------------------------------------------
		if (UFE.config.networkOptions.inputMessageFrequency == NetworkInputMessageFrequency.EveryFrame){
			//-----------------------------------------------------------------------------------------------------
			// We may want to send a network message every frame...
			//-----------------------------------------------------------------------------------------------------
			this.SendNetworkMessages();
		}else{
			//-----------------------------------------------------------------------------------------------------
			// Or we may want to send a network message every few frames...
			//-----------------------------------------------------------------------------------------------------
			if (this._timeToNetworkMessage <= 0L){
				this.SendNetworkMessages();
			}else{
				int localPlayer = UFE.GetLocalPlayer();
				if (localPlayer > 0){
					FrameInput? previousFrameInput;
					FrameInput? currentFrameInput;

					if(
						this.PlayerManager.TryGetInput(localPlayer, currentFrame - 1, out previousFrameInput) &&
						previousFrameInput != null &&
						this.PlayerManager.TryGetInput(localPlayer, currentFrame, out currentFrameInput) &&
						currentFrameInput != null &&
						!previousFrameInput.Value.Equals(currentFrameInput.Value)
					){
						//-----------------------------------------------------------------------------------------
						// Even if we want to send the network message every few frames, 
						// we send the network message immediately if the local player
						// input has changed since the previous frame.
						//
						// We do this to avoid "mega-rollbacks" which can kill the game
						// performance during the "fast-forward" phase.
						//-----------------------------------------------------------------------------------------
						this.SendNetworkMessages();
					}
				}
			}

			--this._timeToNetworkMessage;
		}
	}

    protected virtual void ExecuteLocalDelayedActions() {
        // Check if we need to execute any delayed "local action" (such as playing a sound or GUI)
        for (int i = UFE.delayedLocalActions.Count - 1; i >= 0; --i) {
            DelayedAction action = UFE.delayedLocalActions[i];
            --action.steps;

            if (action.steps <= 0) {
                action.action();
                UFE.delayedLocalActions.RemoveAt(i);
            }
        }
    }

	/// <summary>
	/// 执行同步延迟动作队列（每帧递减步数，到期执行并移除）。
	/// </summary>
    protected virtual void ExecuteSynchronizedDelayedActions() {
        // Check if we need to execute any delayed "synchronized action" (game actions)
        for (int i = UFE.delayedSynchronizedActions.Count - 1; i >= 0; --i) {
            DelayedAction action = UFE.delayedSynchronizedActions[i];
            --action.steps;

            if (action.steps <= 0) {
                action.action();
                UFE.delayedSynchronizedActions.RemoveAt(i);
            }
        }
    }

	/// <summary>
	/// 强制断开连接（反同步处理）：累计反同步次数，超过允许次数时输出错误日志并断开。
	/// </summary>
	/// <param name="disconnectionCause">断开原因文本。</param>
	protected virtual void ForceDisconnection(string disconnectionCause){
		if (UFE.config.networkOptions.disconnectOnDesynchronization){
			++this._desynchronizations;

			if (this._desynchronizations > UFE.config.networkOptions.allowedDesynchronizations){
				if (!string.IsNullOrEmpty(disconnectionCause)){
					Debug.LogError(disconnectionCause);
				}
				Debug.LogError(this._debugInfo.ToString());

				this._debugInfo.Length = 0;
				this._debugInfo.Append("PLAYER ").Append(UFE.GetLocalPlayer()).Append(" - SYNCHRONIZATION LOG\n\n\n");


				if (UFE.multiplayerAPI.IsClient()){
					UFE.multiplayerAPI.DisconnectFromMatch();
				}else if (UFE.multiplayerAPI.IsServer()){
					UFE.multiplayerAPI.DestroyMatch();
				}
			}else{
				Debug.LogWarning(disconnectionCause);
			}
		}else{
			if (!string.IsNullOrEmpty(disconnectionCause)){
				Debug.LogError(disconnectionCause);
			}
			Debug.LogError(this._debugInfo.ToString());

			this._debugInfo.Length = 0;
			this._debugInfo.Append("PLAYER ").Append(UFE.GetLocalPlayer()).Append(" - SYNCHRONIZATION LOG\n\n\n");
		}

	}

	/// <summary>
	/// 获取本地指定帧的同步状态（反同步检测用）。
	/// </summary>
	/// <param name="frame">帧号。</param>
	/// <returns>同步状态；未找到返回 null。</returns>
	protected virtual FluxSimpleState? GetLocalSynchronizationState(long frame){
		for (int i = 0; i < this._localSynchronizationStates.Count; ++i){
			if (this._localSynchronizationStates[i].frame == frame){
				return this._localSynchronizationStates[i];
			}
		}

		return null;
	}

	/// <summary>
	/// 获取远端指定帧的同步状态（反同步检测用）。
	/// </summary>
	/// <param name="frame">帧号。</param>
	/// <returns>同步状态；未找到返回 null。</returns>
	protected virtual FluxSimpleState? GetRemoteSynchronizationState(long frame){
		for (int i = 0; i < this._remoteSynchronizationStates.Count; ++i){
			if (this._remoteSynchronizationStates[i].frame == frame){
				return this._remoteSynchronizationStates[i];
			}
		}

		return null;
	}

	/// <summary>
	/// 获取本地同步状态中最小的帧号。
	/// </summary>
	/// <returns>最小帧号；无记录返回 -1。</returns>
	protected virtual long GetFirstLocalSynchronizationFrame(){
		long frame = -1L;

		for (int i = this._localSynchronizationStates.Count - 1; i >= 0; --i){
			if (frame < 0 || frame > this._localSynchronizationStates[i].frame){
				frame = this._localSynchronizationStates[i].frame;
			}
		}

		return frame;
	}

	/// <summary>
	/// 获取远端同步状态中最小的帧号。
	/// </summary>
	/// <returns>最小帧号；无记录返回 -1。</returns>
	protected virtual long GetFirstRemoteSynchronizationFrame(){
		long frame = -1L;

		for (int i = this._remoteSynchronizationStates.Count - 1; i >= 0; --i){
			if (frame < 0 || frame > this._remoteSynchronizationStates[i].frame){
				frame = this._remoteSynchronizationStates[i].frame;
			}
		}

		return frame;
	}

	/// <summary>
	/// 获取本地同步状态中最大的帧号。
	/// </summary>
	/// <returns>最大帧号；无记录返回 -1。</returns>
	protected virtual long GetLastLocalSynchronizationFrame(){
		long frame = -1L;

		for (int i = this._localSynchronizationStates.Count - 1; i >= 0; --i){
			frame = Math.Max(frame, this._localSynchronizationStates[i].frame);
		}

		return frame;
	}

	/// <summary>
	/// 获取远端同步状态中最大的帧号。
	/// </summary>
	/// <returns>最大帧号；无记录返回 -1。</returns>
	protected virtual long GetLastRemoteSynchronizationFrame(){
		long frame = -1L;

		for (int i = this._remoteSynchronizationStates.Count - 1; i >= 0; --i){
			frame = Math.Max(frame, this._remoteSynchronizationStates[i].frame);
		}

		return frame;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Determines whether this instance is network game.
	/// </summary>
	/// <remarks>
	/// If there is at least one remote player, then it's a network player; otherwise, it's a local game.
	/// </remarks>
	/// <returns><c>true</c> if this instance is network game; otherwise, <c>false</c>.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>判断当前是否为网络对战（存在远端玩家）。</summary>
	/// <returns>网络对战返回 true。</returns>
	protected virtual bool IsNetworkGame(){
		return this.PlayerManager.AreThereRemoteCharacters();
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This method is invoked remotely to update the player inputs.
	/// </summary>
	/// <param name="serializedMessage">Serialized message.</param>
	/// <param name="msgInfo">Message info.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>网络消息接收回调：将收到的消息字节存入待处理列表。</summary>
	/// <param name="bytes">收到的消息字节。</param>
	protected virtual void OnMessageReceived(byte[] bytes){
		this._receivedNetworkMessages.Add(bytes);
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This method is invoked by the engine at the start of the round.
	/// </summary>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>回合开始回调：重置反同步计数。</summary>
	/// <param name="currentRound">回合编号。</param>
	protected virtual void OnRoundBegin(int currentRound){
		// We set the desynchronizations count to zero at the start of each round
		this._desynchronizations = 0;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Raises the round ends event.
	/// </summary>
	/// <param name="winner">Winner.</param>
	/// <param name="loser">Loser.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>回合结束回调：输出反同步日志（如启用）并重置反同步计数。</summary>
	/// <param name="winner">获胜角色。</param>
	/// <param name="loser">失败角色。</param>
	protected virtual void OnRoundEnds(UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser){
		if (UFE.config.debugOptions.desyncErrorLog && this._desynchronizations > 0 && this._debugInfo.Length > 0){
			Debug.LogWarning(this._debugInfo.ToString());
		}
		this._desynchronizations = 0;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Processes the pending network messages.
	/// </summary>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>处理待处理的网络消息：按消息类型分发到输入缓冲或同步消息处理，处理完清空队列。</summary>
	protected virtual void ProcessReceivedNetworkMessages(){
		foreach (byte[] serializedMessage in this._receivedNetworkMessages){
			if (serializedMessage != null && serializedMessage.Length > 0){
				NetworkMessageType messageType = (NetworkMessageType)serializedMessage[0];
				if (messageType == NetworkMessageType.InputBuffer){
					this.ProcessInputBufferMessage(new InputBufferMessage(serializedMessage));
				}else if (messageType == NetworkMessageType.Syncronization){
					this.ProcessSynchronizationMessage(new SynchronizationMessage(serializedMessage));
				}
			}
		}
		this._receivedNetworkMessages.Clear();
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Processes the specified input network package.
	/// </summary>
	/// <param name="package">Network package.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	protected virtual void ProcessInputBufferMessage(InputBufferMessage package){
		// Check if the player number included in the package is valid...
		int playerIndex = package.PlayerIndex;
		if (playerIndex <= 0 || playerIndex > FluxPlayerManager.NumberOfPlayers){
			throw new IndexOutOfRangeException(string.Format(
				FluxCapacitor.PlayerIndexOutOfRangeMessage, 
				playerIndex, 
				1, 
				FluxPlayerManager.NumberOfPlayers
			));
		}


		// TODO: check if the client that sent the message is the same client which controls that player...
//		FluxPlayer player = this.PlayerManager.GetPlayer(playerIndex);
//		if (player.NetworkPlayer != msgInfo.sender){
//			throw new Exception(string.Format(
//				FluxGameManager.NetworkMessageFromUnexpectedPlayerMessage,
//				msgInfo.sender,
//				player.NetworkPlayer
//			));
//		}

		long previousGetLastFrameWithConfirmedInput = this.PlayerManager.GetLastFrameWithConfirmedInput();

		this._remotePlayerNextExpectedFrame = Math.Max(
			this._remotePlayerNextExpectedFrame,
			package.Data.NextExpectedFrame
		);

		// If we want to send only the input changes, we need to remove repeated inputs from the buffer...
		if (UFE.config.networkOptions.onlySendInputChanges){
			int count = package.Data.InputBuffer.Count;

			if (count > 0){
				// First, process the inputs of the first frame in the list...
				this.ProcessInput(playerIndex, package.Data.InputBuffer[0], previousGetLastFrameWithConfirmedInput);

				// Iterate over the rest of the items of the list except the last one...
				for (int i = 1; i < package.Data.InputBuffer.Count; ++i){
					Tuple<long, FrameInput> previousInput = package.Data.InputBuffer[i - 1];
					Tuple<long, FrameInput> currentInput = package.Data.InputBuffer[i];

					if (previousInput != null && currentInput != null){
						// Repeat the previous input from the last updated frame to the frame before the new input
						for (long j = previousInput.Item1 + 1L; j < currentInput.Item1; ++j){
							this.ProcessInput(
								playerIndex, 
								new Tuple<long, FrameInput>(j, new FrameInput(previousInput.Item2)), 
								previousGetLastFrameWithConfirmedInput
							);
						}

						// Now process the new input
						this.ProcessInput(playerIndex, currentInput, previousGetLastFrameWithConfirmedInput);
					}
				}
			}
		}else{
			for (int i = 0; i < package.Data.InputBuffer.Count; ++i){
				this.ProcessInput(playerIndex, package.Data.InputBuffer[i], previousGetLastFrameWithConfirmedInput);
			}
		}
	}

	/// <summary>
	/// 处理收到的单帧输入：设置该玩家该帧的确认输入。
	/// </summary>
	/// <param name="playerIndex">玩家索引。</param>
	/// <param name="frame">（帧号, 输入）对。</param>
	/// <param name="lastFrameWithConfirmedInput">最后确认帧号。</param>
	protected virtual void ProcessInput(int playerIndex, Tuple<long, FrameInput> frame, long lastFrameWithConfirmedInput){
		long currentFrame = frame.Item1;
		this.PlayerManager.TrySetConfirmedInput(playerIndex, currentFrame, frame.Item2);

        //long firstFrameWhereRollbackIsRequired = this.PlayerManager.GetFirstFrameWhereRollbackIsRequired();
        //bool rollbackRequired = firstFrameWhereRollbackIsRequired>=0 && firstFrameWhereRollbackIsRequired<currentFrame;
    }

	/// <summary>
	/// 处理同步消息：与本地对应帧的期望状态比较以检测反同步；无本地期望状态时先保存远端状态待后续比对。
	/// </summary>
	/// <param name="msg">同步消息。</param>
	protected virtual void ProcessSynchronizationMessage(SynchronizationMessage msg){
		if(UFE.config.networkOptions.synchronizationMessageFrequency!=NetworkSynchronizationMessageFrequency.Disabled){
			FluxSimpleState? expectedState = this.GetLocalSynchronizationState(msg.CurrentFrame);
			FluxSimpleState receivedState = msg.Data;

			//-------------------------------------------------------------------------------------------------------------
			// When we receive a synchronization message, check if we already have an "expected state" for that frame
			//-------------------------------------------------------------------------------------------------------------
			if (expectedState != null){
				//---------------------------------------------------------------------------------------------------------
				// In that case, check if the expected and the received value match.
				//---------------------------------------------------------------------------------------------------------
				this.SynchronizationCheck(expectedState.Value, receivedState, msg.CurrentFrame);
			}else{
				//---------------------------------------------------------------------------------------------------------
				// Otherwise, save the received value so we can try it again later.
				//---------------------------------------------------------------------------------------------------------
				this._remoteSynchronizationStates.Add(receivedState);
			}
		}
	}

	/// <summary>
	/// 发送网络消息：将本地玩家的已确认输入缓冲（可压缩为仅输入变化）打包为 InputBufferMessage 发送，
	/// 并按同步频率在需要时附加同步状态消息。
	/// </summary>
	protected virtual void SendNetworkMessages(){
		int localPlayer = UFE.GetLocalPlayer();

		if (localPlayer > 0){
			FluxPlayer local = this.PlayerManager.GetPlayer(localPlayer);

			// And send a message with their current "confirmed input" buffer.
			if (local != null && local.inputBuffer != null){
				IList<Tuple<long, FrameInput>> confirmedInputBuffer = 
					local.inputBuffer.GetConfirmedInputBuffer(this._remotePlayerNextExpectedFrame);

				// If we want to send only the input changes, we need to remove repeated inputs from the buffer...
				if (UFE.config.networkOptions.onlySendInputChanges && confirmedInputBuffer.Count > 1){
					IList<Tuple<long, FrameInput>> tempInputBuffer = confirmedInputBuffer;

					// So copy the first item of the list
					confirmedInputBuffer = new List<Tuple<long, FrameInput>>();
					confirmedInputBuffer.Add(tempInputBuffer[0]);

					// Iterate over the rest of the items in the list, except the last one
					for (int i = 1; i < tempInputBuffer.Count - 1; ++i){
						// If the player inputs has changed since the last frame, add the item to the list
						Tuple<long, FrameInput> currentInput = tempInputBuffer[i];
						Tuple<long, FrameInput> lastInput = confirmedInputBuffer[confirmedInputBuffer.Count - 1];

						if (lastInput != null && currentInput != null && !currentInput.Item2.Equals(lastInput.Item2)){
							confirmedInputBuffer.Add(currentInput);
						}
					}

					// Copy the last item of the list
					confirmedInputBuffer.Add(tempInputBuffer[tempInputBuffer.Count - 1]);
				}

				if (confirmedInputBuffer.Count > 0){
					//Debug.Log("Message Sent: " + confirmedInputBuffer.Count + " frames");

					InputBufferMessage msg = new InputBufferMessage(
						localPlayer, 
						local.inputBuffer.FirstFrame, 
						new InputBufferMessageContent(this.PlayerManager.GetNextExpectedFrame(), confirmedInputBuffer)
					);

					UFE.multiplayerAPI.SendNetworkMessage(msg);


					if (UFE.config.networkOptions.inputMessageFrequency == NetworkInputMessageFrequency.EveryFrame){
						this._timeToNetworkMessage = 1L;
					}else if (UFE.config.networkOptions.inputMessageFrequency == NetworkInputMessageFrequency.EveryOtherFrame){
						this._timeToNetworkMessage = 2L;
					}else{
						//this._timeToNetworkMessage = (long)(this.NetworkFrameDelay) / 2L;
						this._timeToNetworkMessage = (long)(this.NetworkFrameDelay) / 4L;
					}
				}
			}
		}
	}

	/// <summary>
	/// 回滚（无覆盖状态版本）。
	/// </summary>
	/// <param name="currentFrame">当前帧号。</param>
	/// <param name="rollbackFrame">回滚目标帧号。</param>
	/// <param name="lastFrameWithConfirmedInputs">最后确认输入帧号。</param>
	protected virtual void Rollback(long currentFrame, long rollbackFrame, long lastFrameWithConfirmedInputs){
		this.Rollback(currentFrame, rollbackFrame, lastFrameWithConfirmedInputs, null);
	}

	/// <summary>
	/// 回滚（Rollback）核心逻辑：将预测输入校正为确认输入（可选覆盖状态以反同步恢复），
	/// 把游戏状态恢复到最后一致帧并快进模拟后续所有帧回到当前帧，同时快进同步状态与延迟动作。
	/// </summary>
	/// <param name="currentFrame">当前帧号。</param>
	/// <param name="rollbackFrame">回滚目标帧号。</param>
	/// <param name="lastFrameWithConfirmedInputs">最后确认输入帧号。</param>
	/// <param name="overriddenGameState">可选的覆盖状态（反同步恢复）。</param>
	protected virtual void Rollback(
		long currentFrame, 
		long rollbackFrame, 
		long lastFrameWithConfirmedInputs,
		FluxSimpleState? overriddenGameState
	){
#if UFE_LITE || UFE_BASIC || UFE_STANDARD
        Debug.LogError("Rollback not installed.");
#else
        // Retrieve the first stored frame and check if we can rollback to the specified frame...
        long firstStoredFrame = Math.Max(this.PlayerManager.player1.inputBuffer.FirstFrame, this.PlayerManager.player2.inputBuffer.FirstFrame);
		if (rollbackFrame > firstStoredFrame){
			// Show the debug information to help us understand what has happened
			FluxPlayerInputBuffer p1Buffer = this.PlayerManager.player1.inputBuffer;
			FluxPlayerInputBuffer p2Buffer = this.PlayerManager.player2.inputBuffer;
			FluxPlayerInput p1Input = p1Buffer[p1Buffer.GetIndex(rollbackFrame)];
			FluxPlayerInput p2Input = p2Buffer[p2Buffer.GetIndex(rollbackFrame)];

			if (UFE.config.debugOptions.desyncErrorLog){
				this._debugInfo.AppendLine().AppendLine()
					.Append("Rollback from frame ").Append(currentFrame).Append(" to frame ").Append(rollbackFrame)
					.Append("\n(First Stored Input: ").Append(firstStoredFrame).Append(")")
					.Append("\n(Last Confirmed Input: ").Append(lastFrameWithConfirmedInputs).Append(")")
					.Append("\n\nPlayer 1 Predicted:   ").Append(p1Input.PredictedInput)
					.Append("\nPlayer 1 Confirmed:   ").Append(p1Input.ConfirmedInput)
					.Append("\nPlayer 1 Requires Rollback: ").Append(!p1Input.ArePredictedAndConfirmedInputsEqual())
					.Append("\n\nPlayer 2 Predicted:   ").Append(p2Input.PredictedInput)
					.Append("\nPlayer 2 Confirmed:   ").Append(p2Input.ConfirmedInput)
					.Append("\nPlayer 2 Requires Rollback: ").Append(!p2Input.ArePredictedAndConfirmedInputsEqual())
					.AppendLine().AppendLine();
			}

			// Update the predicted inputs with the inputs which have been already confirmed
			for (long i = rollbackFrame; i <= lastFrameWithConfirmedInputs; ++i){
				this.PlayerManager.TryOverridePredictionWithConfirmedInput(1, i);
				this.PlayerManager.TryOverridePredictionWithConfirmedInput(2, i);
			}

			// Check if we should override the current game state
			if (overriddenGameState != null){
				KeyValuePair<FluxStates, FluxFrameInput> pair;

				if (this._history.TryGetStateAndInput(rollbackFrame, out pair)){
					// Override partially the GameState in the history 
					// so we have a chance of resynchronization...
                    FluxStateTracker.LoadGameState(pair.Key);
					pair.Key.Override(overriddenGameState.Value);
					this._history.TrySetState(pair);

					// Store the new state into the local synchronization states 
					// to try to pass the next synchronization check...
					FluxSimpleState simpleState = new FluxSimpleState(pair.Key);
					for (int i = 0; i < this._localSynchronizationStates.Count; ++i){
						if (this._localSynchronizationStates[i].frame == simpleState.frame){
							this._localSynchronizationStates[i] = simpleState;
						}
					}
				}
			}

			// Reset the game to the state it had on the last consistent frame...
            this._history = FluxStateTracker.LoadGameState(this._history, rollbackFrame);

			// And simulate all the frames after that fast-forward, so we return to the previous frame again...
			long fastForwardTarget = Math.Min(UFE.currentFrame, this._remotePlayerNextExpectedFrame - 1);
			long maxFastForwards = Math.Max((long)(UFE.config.networkOptions.maxFastForwards), (currentFrame - fastForwardTarget)/2L);
			long currentFastForwards = 0L;

			while (UFE.currentFrame < currentFrame && currentFastForwards < maxFastForwards){
				this.ApplyInputs(UFE.currentFrame, lastFrameWithConfirmedInputs);
				++currentFastForwards;
			}
		}else{
			this._debugInfo.AppendLine().AppendLine()
				.Append("Rollback from frame ").Append(currentFrame).Append(" to frame ").Append(rollbackFrame)
				.Append("\nFailed because the specified frame is no longer stored in the Game History.")
				.AppendLine().AppendLine();
			
		}
#endif
    }

	/// <summary>
	/// 读取双方当前帧输入：为双方生成预测输入（本地确认，远端可预测），
	/// 并清理已消费的菜单选中选项（ReadInputs 每帧调用）。
	/// </summary>
	/// <param name="frameDelay">当前帧延迟。</param>
	/// <param name="allowRollbacks">是否允许回滚。</param>
    protected virtual void ReadInputs(long frameDelay, bool allowRollbacks) {
        //-------------------------------------------------------------------------------------------------------------
        // Read the player inputs (ensuring that there aren't any "holes" created by variable frame-delay).
        //-------------------------------------------------------------------------------------------------------------
        for (int i = 0; i <= frameDelay * 2; ++i) {
            long frame = UFE.currentFrame + (long)(i);

            for (int j = 1; j <= FluxPlayerManager.NumberOfPlayers; ++j) {
                if (this.PlayerManager.ReadInputs(j, frame, this._selectedOptions[j - 1], allowRollbacks)) {
                    this._selectedOptions[j - 1] = null;
                }
            }
        }
    }

	/// <summary>
	/// 同步检查（默认允许反同步恢复）。
	/// </summary>
	/// <param name="expectedState">本地期望状态。</param>
	/// <param name="receivedState">远端收到的状态。</param>
	/// <param name="frame">帧号。</param>
	/// <returns>状态一致返回 true。</returns>
	protected virtual bool SynchronizationCheck(
		FluxSimpleState expectedState, 
		FluxSimpleState receivedState, 
		long frame
	){
		return this.SynchronizationCheck(expectedState, receivedState, frame, true);
	}

	/// <summary>
	/// 同步检查（反同步检测）：比较双方生命/能量/相对位置（含帧号），
	/// 误差在阈值内视为一致；否则触发反同步处理（按配置强制断开或覆盖状态恢复）。
	/// </summary>
	/// <param name="expectedState">本地期望状态。</param>
	/// <param name="receivedState">远端收到的状态。</param>
	/// <param name="frame">帧号。</param>
	/// <param name="allowRecoveryFromDesynchronizations">是否允许反同步恢复。</param>
	/// <returns>状态一致返回 true。</returns>
	protected virtual bool SynchronizationCheck(
		FluxSimpleState expectedState, 
		FluxSimpleState receivedState, 
		long frame,
		bool allowRecoveryFromDesynchronizations
	){
		float distanceThreshold = UFE.config.networkOptions.floatDesynchronizationThreshold;

		// As we want to be as permissive as possible as long as we can recover from the desynchronization,
		// we aren't as interested in comparing players absolute positions as we are interesting in comparing
		// their positions relative each other.
		Vector3 expectedRelativePosition = (expectedState.p1.position - expectedState.p2.position);
		Vector3 receivedRelativePosition = (receivedState.p1.position - receivedState.p2.position);

		if(
			expectedState.frame == receivedState.frame 
			&&
			(
				!UFE.config.networkOptions.desynchronizationRecovery &&
				FPLibrary.FPMath.Abs(expectedState.p1.life - receivedState.p1.life ) <= distanceThreshold &&
                FPLibrary.FPMath.Abs(expectedState.p2.life - receivedState.p2.life) <= distanceThreshold
				||
				UFE.config.networkOptions.desynchronizationRecovery &&
                FPLibrary.FPMath.Abs(expectedState.p1.life - receivedState.p1.life) <= distanceThreshold &&
                FPLibrary.FPMath.Abs(expectedState.p1.gauge - receivedState.p1.gauge) <= distanceThreshold &&
                FPLibrary.FPMath.Abs(expectedState.p2.life - receivedState.p2.life) <= distanceThreshold &&
                FPLibrary.FPMath.Abs(expectedState.p2.gauge - receivedState.p2.gauge) <= distanceThreshold &&
				Mathf.Abs(expectedRelativePosition.x - receivedRelativePosition.x) <= distanceThreshold	&&
				Mathf.Abs(expectedRelativePosition.y - receivedRelativePosition.y) <= distanceThreshold	&&
				Mathf.Abs(expectedRelativePosition.z - receivedRelativePosition.z) <= distanceThreshold
			)
		){
			//---------------------------------------------------------------------------------------------------------
			// If the game state received from the network message is equal to the stored state,
			// everything is ok, we can delete previous messages.
			//---------------------------------------------------------------------------------------------------------
			// Debug.Log("Synchroned!\tFrame = " + msg.CurrentFrame + "\nExpected State: " + state +  "\nReceived State: " + msg.Data);

			for (int i = this._localSynchronizationStates.Count - 1; i >= 0; --i){
				if (this._localSynchronizationStates[i].frame <= receivedState.frame){
					this._localSynchronizationStates.RemoveAt(i);
				}
			}

			for (int i = this._remoteSynchronizationStates.Count - 1; i >= 0; --i){
				if (this._remoteSynchronizationStates[i].frame <= receivedState.frame){
					this._remoteSynchronizationStates.RemoveAt(i);
				}
			}

			return true;
		}else{ 
			//---------------------------------------------------------------------------------------------------------
			// If a desynchronization has happened, check if we should try to recover from the desynchronization
			// so show a log message and check if we should exit from the network game.
			//---------------------------------------------------------------------------------------------------------
			long firstStoredInput = Math.Max(
				this.PlayerManager.player1.inputBuffer.FirstFrame, 
				this.PlayerManager.player2.inputBuffer.FirstFrame
			);

			int localPlayer = UFE.GetLocalPlayer();
			long rollbackFrame = localPlayer == 1 ? frame - 1L : frame;

			if(
				allowRecoveryFromDesynchronizations &&
				rollbackFrame > firstStoredInput &&
				rollbackFrame >= this._history.FirstStoredFrame &&
				UFE.config.networkOptions.desynchronizationRecovery &&
				this.AllowRollbacks
			){
				
				long lastFrameWithConfirmedInput = this.PlayerManager.GetLastFrameWithConfirmedInput();
				long currentFrame = UFE.currentFrame;

				if (localPlayer == 1){
					this._debugInfo.Append("\n\n\nDesynchronization detected, expecting the other client will recover from the desynchronization.\n\n");
					this.Rollback(currentFrame, rollbackFrame, lastFrameWithConfirmedInput);
					return true;
				}else{
					this._debugInfo.Append("\n\n\nDesynchronization detected, trying to recover from the desynchronization.\n\n");
					this.Rollback(currentFrame, rollbackFrame, lastFrameWithConfirmedInput, receivedState);
					return this.SynchronizationCheck(expectedState, receivedState, frame, false);
				}
			}else{
				//-----------------------------------------------------------------------------------------------------
				// If a desynchronization has happened and we can't or don't want to recover from the 
				// desynchronization, show a log message and check if we should exit from the network game.
				//-----------------------------------------------------------------------------------------------------
				string expectedStateString = expectedState.ToString();
				string receivedStateString = receivedState.ToString();

				this._debugInfo
					.Append("\n\n\nSYNCHRONIZATION LOST!!!")
					.Append("\nFrame: ").Append(frame)
					.Append("\nExpected State: ").Append(expectedStateString)
					.Append("\nReceived State: ").Append(receivedStateString).AppendLine().AppendLine();

				this._localSynchronizationStates.Clear();
				this._remoteSynchronizationStates.Clear();

				this.ForceDisconnection(string.Format(
					"SYNCHRONIZATION LOST!!!\nFrame: {0}\nExpected State: {1}\nReceived State: {2}",
					frame,
					expectedStateString,
					receivedStateString
				));

				return false;
			}
		}
	}

	/// <summary>
	/// 更新 GUI：推进摄像机淡入淡出、向战斗 HUD 转发双方输入与选中选项，并更新触屏控件与调试文本。
	/// </summary>
	/// <param name="player1PreviousInputs">玩家1上一帧输入。</param>
	/// <param name="player1CurrentInputs">玩家1当前帧输入。</param>
	/// <param name="player1SelectedOptions">玩家1选中选项。</param>
	/// <param name="player2PreviousInputs">玩家2上一帧输入。</param>
	/// <param name="player2CurrentInputs">玩家2当前帧输入。</param>
	/// <param name="player2SelectedOptions">玩家2选中选项。</param>
	protected virtual void UpdateGUI(
		IDictionary<InputReferences, InputEvents> player1PreviousInputs,
		IDictionary<InputReferences, InputEvents> player1CurrentInputs,
		int? player1SelectedOptions,
		IDictionary<InputReferences, InputEvents> player2PreviousInputs,
		IDictionary<InputReferences, InputEvents> player2CurrentInputs,
		int? player2SelectedOptions
	){

		if (CameraFade.instance.enabled){
			CameraFade.instance.DoFixedUpdate();
		}

        if (UFE.battleGUI != null) {
			if (player1SelectedOptions != null){
				UFE.battleGUI.SelectOption(player1SelectedOptions.Value, 1);
			}

			if (player2SelectedOptions != null){
				UFE.battleGUI.SelectOption(player2SelectedOptions.Value, 2);
			}

			UFE.battleGUI.DoFixedUpdate(
				player1PreviousInputs,
				player1CurrentInputs,
				player2PreviousInputs,
				player2CurrentInputs
            );

		}

        if (UFE.isControlFreak2Installed && UFE.touchControllerBridge != null) {
            UFE.touchControllerBridge.DoFixedUpdate();
        } else if (UFE.isControlFreak1Installed) {
            if (UFE.gameRunning && UFE.controlFreakPrefab != null && !UFE.controlFreakPrefab.activeSelf) {
                UFE.controlFreakPrefab.SetActive(true);
            } else if (!UFE.gameRunning && UFE.controlFreakPrefab != null && UFE.controlFreakPrefab.activeSelf) {
                UFE.controlFreakPrefab.SetActive(false);
            }
        }

		if (UFE.currentScreen != null){
			if (player1SelectedOptions != null){
				UFE.currentScreen.SelectOption(player1SelectedOptions.Value, 1);
			}

			if (player2SelectedOptions != null){
				UFE.currentScreen.SelectOption(player2SelectedOptions.Value, 2);
			}

			UFE.currentScreen.DoFixedUpdate(
				player1PreviousInputs,
				player1CurrentInputs,
				player2PreviousInputs,
				player2CurrentInputs
			);
		}

		if (UFE.canvasGroup.alpha == 0){
			UFE.canvasGroup.alpha = 1;
		}
	}

	/// <summary>
	/// 更新回合计时器：按配置计时速度递减剩余时间（训练/挑战冻结时间模式除外），
	/// 每秒触发一次计时事件，归零后触发时间到事件。
	/// </summary>
	protected virtual void UpdateTimer(){
		if (UFE.config.roundOptions.hasTimer && UFE.timer > 0 && !UFE.IsTimerPaused()) {
			if (UFE.gameMode != GameMode.TrainingRoom 
                && UFE.gameMode != GameMode.ChallengeMode
                && !UFE.config.trainingModeOptions.freezeTime){
                UFE.timer -= UFE.fixedDeltaTime * (UFE.config.roundOptions._timerSpeed * .01);
			}

			if (UFE.timer < UFE.intTimer) {
				UFE.intTimer --;
				UFE.FireTimer((float)UFE.timer);
			}
		}
		if (UFE.timer < 0){
			UFE.timer = 0;
		}
		if (UFE.intTimer < 0){
			UFE.intTimer = 0;
		}
        
		ControlsScript p1ControlsScript = UFE.GetControlsScript(1);
		ControlsScript p2ControlsScript = UFE.GetControlsScript(2);

		if (UFE.timer == 0 && p1ControlsScript != null && !UFE.config.lockMovements){
			Fix64 p1LifePercentage = p1ControlsScript.myInfo.currentLifePoints/(Fix64)p1ControlsScript.myInfo.lifePoints;
            Fix64 p2LifePercentage = p2ControlsScript.myInfo.currentLifePoints / (Fix64)p2ControlsScript.myInfo.lifePoints;
			UFE.PauseTimer();
			UFE.config.lockMovements = true;
			UFE.config.lockInputs = true;

			UFE.FireTimeOver();

            
            // Check Winner
            if (p1LifePercentage == p2LifePercentage) {
                UFE.FireAlert(UFE.config.selectedLanguage.draw, null);
                UFE.DelaySynchronizedAction(this.NewRound, 1);
            } else {
                if (p1LifePercentage > p2LifePercentage) SetWinner((p1LifePercentage > p2LifePercentage) ? p1ControlsScript : p2ControlsScript);
            }
		}
	}

    protected virtual void UpdateInstantiatedObjects(long currentFrame, long lastSynchronizedFrame)
    {
        foreach (InstantiatedGameObject entry in UFE.instantiatedObjects.ToArray())
        {
            if (entry.gameObject == null) continue;
            if (entry.mrFusion != null && entry.gameObject.activeInHierarchy) entry.mrFusion.UpdateBehaviours();
            if (entry.destructionFrame != null) entry.gameObject.SetActive(currentFrame > entry.creationFrame && currentFrame < entry.destructionFrame);
        }

        // Memory Cleaner
        if (UFE.instantiatedObjects.Count > 0 && (UFE.instantiatedObjects.Count > UFE.config.networkOptions.spawnBuffer || !UFE.gameRunning)) {
            if (!UFE.instantiatedObjects[0].gameObject.activeInHierarchy)
            {
                UnityEngine.Object.Destroy(UFE.instantiatedObjects[0].gameObject);
                UFE.instantiatedObjects.RemoveAt(0);
            }
        }
    }

    protected virtual void UpdatePlayer(int player, long currentFrame, long lastSynchronizedFrame, IDictionary<InputReferences, InputEvents> previousInputs, IDictionary<InputReferences, InputEvents> currentInputs) {
		ControlsScript controlsScript = UFE.GetControlsScript(player);

		if (controlsScript != null) {
			if (controlsScript.MoveSet != null && controlsScript.MoveSet.MecanimControl != null) {
				controlsScript.MoveSet.MecanimControl.DoFixedUpdate();
			}
			if (controlsScript.MoveSet != null && controlsScript.MoveSet.LegacyControl != null) {
				controlsScript.MoveSet.LegacyControl.DoFixedUpdate();
            }
            controlsScript.DoFixedUpdate(previousInputs, currentInputs);

            if (controlsScript.projectiles.Count > 0) {
				controlsScript.projectiles.RemoveAll(item => item.IsDestroyed() || item == null);
			}
		}
	}
#endregion
}
