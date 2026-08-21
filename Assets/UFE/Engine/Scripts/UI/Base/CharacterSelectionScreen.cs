using UnityEngine;
using System;
using System.Reflection;
using UFE3D;

/// <summary>
/// 角色选择界面（CharacterSelectionScreen）。
/// <para>用途：选人界面的基类——维护双方悬停索引与可选角色列表，处理角色的选择/取消选择，</para>
/// <para>支持本地即时选择与网络对战下的帧同步选择（通过 FluxCapacitor.RequestOptionSelection 同步），</para>
/// <para>双人均选好后按游戏模式进入场地选择或故事开场。</para>
/// </summary>
public class CharacterSelectionScreen : UFEScreen {
	#region public instance properties
	/// <summary>选择角色时播放的音效。</summary>
	public AudioClip selectSound;
	/// <summary>取消选择时播放的音效。</summary>
	public AudioClip cancelSound;
	#endregion

	#region protected instance fields
	/// <summary>玩家1当前悬停的角色索引。</summary>
	protected int p1HoverIndex = 0;
	/// <summary>玩家2当前悬停的角色索引。</summary>
	protected int p2HoverIndex = 0;
	/// <summary>界面是否正在关闭。</summary>
	protected bool closing = false;
	/// <summary>当前模式可选角色列表。</summary>
	protected UFE3D.CharacterInfo[] selectableCharacters = new UFE3D.CharacterInfo[0];
	#endregion

	#region public instance methods
	/// <summary>
	/// 获取指定玩家的悬停角色索引。
	/// </summary>
	/// <param name="player">玩家编号（1 或 2）。</param>
	/// <returns>悬停索引。</returns>
	public virtual int GetHoverIndex(int player){
		if (player == 1){
			return this.p1HoverIndex;
		}else if (player == 2){
			return this.p2HoverIndex;
		}

		throw new ArgumentOutOfRangeException("player");
	}

	/// <summary>
	/// 返回上一个界面（对战→对战模式界面，网络→网络界面，其他→主菜单）。
	/// </summary>
	public virtual void GoToPreviousScreen(){
		this.closing = true;

		if (UFE.gameMode == GameMode.VersusMode && UFE.GetVersusModeScreen() != null){
			UFE.DelaySynchronizedAction(this.GoToVersusModeScreen, 0.8);
		}else if (UFE.gameMode == GameMode.NetworkGame){
			UFE.DelaySynchronizedAction(this.GoToNetworkGameScreen, 0.8);
		}else{
			UFE.DelaySynchronizedAction(this.GoToMainMenuScreen, 0.8);
		}
	}

	/// <summary>
	/// 角色选择确认回调：处理选中（设置角色、播放音效、双方选齐后进入下一界面）与取消（取消选择/返回上一界面）。
	/// </summary>
	/// <param name="characterIndex">角色索引。</param>
	/// <param name="player">操作玩家。</param>
	public virtual void OnCharacterSelectionAllowed(int characterIndex, int player){
		// If we haven't started loading a different screen....
		if (!this.closing){
			// Check if we are trying to select or deselect a character...
			if (characterIndex >= 0 && characterIndex <= this.GetMaxCharacterIndex()){
				// If we are selecting a character, check if the player has already selected a character...
				if(
					player == 1 && UFE.config.player1Character == null ||
					player == 2 && UFE.config.player2Character == null
				){
					// If the player hasn't selected any character yet, process the request...
					this.SetHoverIndex(player, characterIndex);
					UFE3D.CharacterInfo character = this.selectableCharacters[characterIndex];
					if (this.selectSound != null) UFE.PlaySound(this.selectSound);
					if (character != null && character.selectionSound != null) UFE.PlaySound(character.selectionSound);
					UFE.SetPlayer(player, character);


					// And check if we should start loading the next screen...
					if(
						UFE.config.player1Character != null && 
						(UFE.config.player2Character != null || UFE.gameMode == GameMode.StoryMode)
					){
						this.GoToNextScreen();
					}
				}
			}else if (characterIndex < 0){
				if(
					// If we are trying to deselect a character, check if at least one player has selected a character
					UFE.config.player1Character != null || UFE.config.player2Character != null 
					||
					// In network games, we also allow to return to the previous screen if the one of the player 
					// doesn't have a character selected and he presses the back button. We want to return to the 
					// previous screen even if the other player has a character selected.
					UFE.gameMode == GameMode.NetworkGame 
					&&
					(
						player == 1 && UFE.config.player1Character != null ||
						player == 2 && UFE.config.player2Character != null
					)
				){
					// In that case, check if the player that wants to deselect his current character has already
					// selected a character and try to deselect that character.
					if(
						player == 1 && UFE.config.player1Character != null ||
						player == 2 && UFE.config.player2Character != null
					){
						if (this.cancelSound != null) UFE.PlaySound(this.cancelSound);
						UFE.SetPlayer(player, null);
					}
				}else{
					// If none of the players has selected a character and one of the player wanted to deselect
					// his current character, that means that the player wants to return to the previous menu instead.
					this.GoToPreviousScreen();
				}
			}
		}
	}

	/// <summary>
	/// 设置指定玩家的悬停角色索引。
	/// </summary>
	/// <param name="player">玩家编号。</param>
	/// <param name="characterIndex">角色索引。</param>
	public virtual void SetHoverIndex(int player, int characterIndex){
		if (!this.closing){
			if (characterIndex >= 0 && characterIndex <= this.GetMaxCharacterIndex()){
				if (player == 1){
					p1HoverIndex = characterIndex;
				}else if (player == 2){
					p2HoverIndex = characterIndex;
				}
			}
		}
	}

	/// <summary>
	/// 尝试取消选择角色（自动选择当前应取消的玩家）。
	/// </summary>
	public void TryDeselectCharacter(){
		if (!UFE.isConnected){
			// If it's a local game, update the corresponding character immediately...
			if (UFE.config.player2Character != null && UFE.gameMode != GameMode.StoryMode && !UFE.GetCPU(2)){
				this.TryDeselectCharacter(2);
			}else{
				this.TryDeselectCharacter(1);
			}
		}else{
			// If it's an online game, find out if the local player is Player1 or Player2
			// and update the selection only for the local player...
			this.TryDeselectCharacter(UFE.GetLocalPlayer());
		}
	}

	/// <summary>
	/// 尝试取消指定玩家的角色选择。
	/// </summary>
	/// <param name="player">玩家编号。</param>
	public void TryDeselectCharacter(int player){
		this.TrySelectCharacter(-1, player);
	}

	/// <summary>
	/// 尝试选择当前悬停的角色（本地/网络按玩家分配）。
	/// </summary>
	public void TrySelectCharacter(){
		// If it's a local game, update the corresponding character immediately...
		if (!UFE.isConnected){
			if (UFE.config.player1Character == null){
				this.TrySelectCharacter(this.p1HoverIndex, 1);
			}else if (UFE.config.player2Character == null){
				this.TrySelectCharacter(this.p2HoverIndex, 2);
			}
		}else{
			// If it's an online game, find out if the local player is Player1 or Player2
			// and update the selection only for the local player...
			int localPlayer = UFE.GetLocalPlayer();

			if (localPlayer == 1){
				this.TrySelectCharacter(this.p1HoverIndex, localPlayer);
			}else if (localPlayer == 2){
				this.TrySelectCharacter(this.p2HoverIndex, localPlayer);
			}
		}
	}

	/// <summary>
	/// 尝试选择指定角色（自动分配到未选角色的玩家）。
	/// </summary>
	/// <param name="characterIndex">角色索引。</param>
	public void TrySelectCharacter(int characterIndex){
		if (!UFE.isConnected){
			// If it's a local game, update the corresponding character immediately...
			if (UFE.config.player1Character == null){
				this.TrySelectCharacter(characterIndex, 1);
			}else if (UFE.config.player2Character == null && UFE.gameMode != GameMode.StoryMode){
				this.TrySelectCharacter(characterIndex, 2);
			}
		}else{
			// If it's an online game, find out if the local player is Player1 or Player2
			// and update the selection only for the local player...
			this.TrySelectCharacter(characterIndex, UFE.GetLocalPlayer());
		}
	}
	
	/// <summary>
	/// 尝试选择角色（本地立即生效；网络对战仅本机玩家通过帧同步请求同步）。
	/// </summary>
	/// <param name="characterIndex">角色索引。</param>
	/// <param name="player">操作玩家。</param>
	public virtual void TrySelectCharacter(int characterIndex, int player){
		// Check if he was playing online or not...
		if (!UFE.isConnected){
			// If it's a local game, update the corresponding character immediately...
			this.OnCharacterSelectionAllowed(characterIndex, player);
		}else{
			// If it's an online game, find out if the requesting player is the local player
			// because we will only accept requests for the local player...
			int localPlayer = UFE.GetLocalPlayer();
			if (player == localPlayer){
				// We don't invoke the OnCharacterSelected() method immediately because we are using the frame-delay 
				// algorithm to keep players synchronized, so we can't invoke the OnCharacterSelected() method
				// until the other player has received the message with our choice.
				UFE.fluxCapacitor.RequestOptionSelection(localPlayer, (sbyte)characterIndex);
			}
		}
	}
	#endregion

	#region public override methods
	/// <summary>
	/// 界面显示时：按游戏模式加载可选角色列表并清空双方已选角色。
	/// </summary>
	public override void OnShow (){
		base.OnShow();

		if (UFE.gameMode == GameMode.StoryMode){
			this.selectableCharacters = UFE.GetStoryModeSelectableCharacters();
		}else if (UFE.gameMode == GameMode.TrainingRoom){
			this.selectableCharacters = UFE.GetTrainingRoomSelectableCharacters();
		}else{
			this.selectableCharacters = UFE.GetVersusModeSelectableCharacters();
		}

		UFE.SetPlayer1(null);
		UFE.SetPlayer2(null);
		this.SetHoverIndex(1, 0);
		this.SetHoverIndex(2, this.GetMaxCharacterIndex());
	}

	/// <summary>
	/// 处理菜单选项选择（确认角色）。
	/// </summary>
	/// <param name="option">角色索引。</param>
	/// <param name="player">操作玩家。</param>
	public override void SelectOption (int option, int player){
		this.OnCharacterSelectionAllowed(option, player);
	}
	#endregion

	#region protected instance methods
	/// <summary>
	/// 获取最大角色索引（列表末尾）。
	/// </summary>
	/// <returns>最大索引。</returns>
	protected virtual int GetMaxCharacterIndex(){
		return this.selectableCharacters.Length - 1;
	}

	/// <summary>
	/// 返回主菜单（标记关闭）。
	/// </summary>
	protected void GoToMainMenuScreen(){
		this.closing = true;
		UFE.StartMainMenuScreen();
	}

	/// <summary>
	/// 进入网络游戏界面（标记关闭）。
	/// </summary>
	protected void GoToNetworkGameScreen(){
		this.closing = true;
		UFE.StartNetworkGameScreen();
	}

	/// <summary>
	/// 进入下一界面：故事模式→开场演出，其他→场地选择（延迟0.8秒）。
	/// </summary>
	protected virtual void GoToNextScreen(){
		this.closing = true;

		if (UFE.gameMode == GameMode.StoryMode){
			UFE.DelaySynchronizedAction(this.StartStoryMode, 0.8);
		}else{
			UFE.DelaySynchronizedAction(this.GoToStageSelectionScreen, 0.8);
		}
	}

	/// <summary>
	/// 进入场地选择界面（标记关闭）。
	/// </summary>
	protected void GoToStageSelectionScreen(){
		this.closing = true;
		UFE.StartStageSelectionScreen();
	}
	
	/// <summary>
	/// 进入对战模式界面（标记关闭）。
	/// </summary>
	protected void GoToVersusModeScreen(){
		this.closing = true;
		UFE.StartVersusModeScreen();
	}

	/// <summary>
	/// 进入故事模式开场演出（标记关闭）。
	/// </summary>
	protected void StartStoryMode(){
		this.closing = true;
		UFE.StartStoryModeOpeningScreen();
	}
	#endregion
}
