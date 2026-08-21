using UnityEngine;
using System;
using System.Collections.Generic;
using UFE3D;

/// <summary>
/// 战斗 HUD 基类（BattleGUI）。
/// <para>用途：战斗中血条/能量条/回合/提示等 HUD 的基类——维护双方角色信息（PlayerInfo）、</para>
/// <para>订阅 UFE 全部战斗事件（生命/命中/格挡/弹反/出招/回合/计时等）以便更新 UI，</para>
/// <para>并在游戏/回合结束时按模式跳转到结算界面。</para>
/// <para>子类（DefaultBattleGUI 等）覆写事件回调实现具体 HUD 表现。</para>
/// </summary>
public class BattleGUI : UFEScreen {
	#region public class definitions
	/// <summary>
	/// 玩家信息（PlayerInfo）：HUD 展示用的一名玩家数据。
	/// </summary>
	[Serializable]
	public class PlayerInfo{
		/// <summary>玩家角色信息。</summary>
		public UFE3D.CharacterInfo character;
		/// <summary>目标生命值。</summary>
		public float targetLife;
		/// <summary>总生命值。</summary>
		public float totalLife;
		/// <summary>已获胜回合数。</summary>
		public int wonRounds;
		/// <summary>是否为最终胜利者。</summary>
        public bool winner;
    }
    #endregion

    #region protected instance properties
	/// <summary>玩家1信息。</summary>
    protected PlayerInfo player1 = new PlayerInfo();
	/// <summary>玩家2信息。</summary>
	protected PlayerInfo player2 = new PlayerInfo();
	/// <summary>战斗是否正在运行。</summary>
	protected bool isRunning;
	#endregion
    
    //GameObject leftZoneRegular;
    //GameObject rightZoneRegular;
    //GameObject leftZoneMirror;
    //GameObject rightZoneMirror;
    //int trycount;


	#region public override methods
	/// <summary>
	/// 固定帧更新：处理双方输入（基类实现，当前仅转发）。
	/// </summary>
	/// <param name="player1PreviousInputs">玩家1上一帧输入。</param>
	/// <param name="player1CurrentInputs">玩家1当前帧输入。</param>
	/// <param name="player2PreviousInputs">玩家2上一帧输入。</param>
	/// <param name="player2CurrentInputs">玩家2当前帧输入。</param>
	public override void DoFixedUpdate(
		IDictionary<InputReferences, InputEvents> player1PreviousInputs,
		IDictionary<InputReferences, InputEvents> player1CurrentInputs,
		IDictionary<InputReferences, InputEvents> player2PreviousInputs,
		IDictionary<InputReferences, InputEvents> player2CurrentInputs
	){
        /*if (leftZoneRegular == null && UFE.controlFreakPrefab != null && trycount < 10) {
                //leftZoneRegular = GameObject.Find("/CF2 Swipe(Clone)/CF2-Canvas/CF2-Panel/TouchZone-Left-Tap");
                leftZoneRegular = GameObject.Find("/CF2-Panel/TouchZone-Left-Tap");
                rightZoneRegular = GameObject.Find("/CF2-Panel/TouchZone-Right-Tap");
                leftZoneMirror = GameObject.Find("/CF2-Panel/TouchZone-Left-Tap-Mirror");
                rightZoneMirror = GameObject.Find("/CF2-Panel/TouchZone-Right-Tap-Mirror");
                Debug.Log(leftZoneRegular);

                leftZoneRegular.SetActive(true);
                rightZoneRegular.SetActive(true);
                leftZoneMirror.SetActive(false);
                rightZoneMirror.SetActive(false);
                trycount ++;
            }
        }*/
        base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
	}

	/// <summary>
	/// 界面显示时订阅 UFE 全部战斗事件。
	/// </summary>
	public override void OnShow (){
		base.OnShow ();

		/* Subscribe to UFE events:
		/* Possible Events:
		 * OnLifePointsChange(float newLifePoints, UFE3D.CharacterInfo player)
		 * OnNewAlert(string alertMessage, UFE3D.CharacterInfo player)
		 * OnHit(MoveInfo move, UFE3D.CharacterInfo hitter)
		 * OnMove(MoveInfo move, UFE3D.CharacterInfo player)
		 * OnRoundEnds(UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser)
		 * OnRoundBegins(int roundNumber)
		 * OnGameEnds(UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser)
		 * OnGameBegins(UFE3D.CharacterInfo player1, UFE3D.CharacterInfo player2, StageOptions stage)
		 * 
		 * usage:
		 * UFE.OnMove += YourFunctionHere;
		 * .
		 * .
		 * void YourFunctionHere(T param1, T param2){...}
		 * 
		 * The following code bellow show more usage examples
		 */

        // Global Events
		UFE.OnGameBegin += this.OnGameBegin;
		UFE.OnGameEnds += this.OnGameEnd;
		UFE.OnGamePaused += this.OnGamePaused;
		UFE.OnRoundBegins += this.OnRoundBegin;
		UFE.OnRoundEnds += this.OnRoundEnd;
		UFE.OnLifePointsChange += this.OnLifePointsChange;
		UFE.OnNewAlert += this.OnNewAlert;
        UFE.OnHit += this.OnHit;
        UFE.OnBlock += this.OnBlock;
		UFE.OnParry += this.OnParry;
		UFE.OnMove += this.OnMove;
		UFE.OnBasicMove += this.OnBasicMove;
		UFE.OnButton += this.OnButtonPress;
		UFE.OnTimer += this.OnTimer;
		UFE.OnTimeOver += this.OnTimeOver;
		UFE.OnInput += this.OnInput;

        // Move Events
        UFE.OnBodyVisibilityChange += this.OnBodyVisibilityChange;
        UFE.OnParticleEffects += this.OnParticleEffects;
        UFE.OnSideSwitch += this.OnSideSwitch;
	}

	/// <summary>
	/// 界面隐藏时取消订阅 UFE 全部战斗事件。
	/// </summary>
	public override void OnHide (){
		UFE.OnGameBegin -= this.OnGameBegin;
		UFE.OnGameEnds -= this.OnGameEnd;
		UFE.OnGamePaused -= this.OnGamePaused;
		UFE.OnRoundBegins -= this.OnRoundBegin;
		UFE.OnRoundEnds -= this.OnRoundEnd;
		UFE.OnLifePointsChange -= this.OnLifePointsChange;
		UFE.OnNewAlert -= this.OnNewAlert;
        UFE.OnHit -= this.OnHit;
        UFE.OnBlock -= this.OnBlock;
        UFE.OnParry -= this.OnParry;
        UFE.OnMove -= this.OnMove;
        UFE.OnBasicMove -= this.OnBasicMove;
        UFE.OnButton -= this.OnButtonPress;
		UFE.OnTimer -= this.OnTimer;
		UFE.OnTimeOver -= this.OnTimeOver;
		UFE.OnInput -= this.OnInput;

        UFE.OnBodyVisibilityChange -= this.OnBodyVisibilityChange;
        UFE.OnParticleEffects -= this.OnParticleEffects;
        UFE.OnSideSwitch -= this.OnSideSwitch;

		base.OnHide ();
	}
	#endregion

	#region protected instance methods
	/// <summary>
	/// 游戏开始回调：初始化双方玩家信息并播放场地音乐。
	/// </summary>
	/// <param name="player1">玩家1角色。</param>
	/// <param name="player2">玩家2角色。</param>
	/// <param name="stage">场地。</param>
	protected virtual void OnGameBegin(UFE3D.CharacterInfo player1, UFE3D.CharacterInfo player2, StageOptions stage){
		this.player1.character = player1;
		this.player1.targetLife = player1.lifePoints;
		this.player1.totalLife = player1.lifePoints;
		this.player1.wonRounds = 0;

		this.player2.character = player2;
		this.player2.targetLife = player2.lifePoints;
		this.player2.totalLife = player2.lifePoints;
		this.player2.wonRounds = 0;

		UFE.PlayMusic(stage.music);
		this.isRunning = true;
	}

	/// <summary>
	/// 游戏结束回调：标记胜利者并延迟 3.5 秒打开结算界面。
	/// </summary>
	/// <param name="winner">获胜角色。</param>
	/// <param name="loser">失败角色。</param>
	protected virtual void OnGameEnd(UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser){
		this.isRunning = false;
        if (winner == this.player1.character) this.player1.winner = true;
        if (winner == this.player2.character) this.player2.winner = true;

        UFE.DelaySynchronizedAction(this.OpenMenuAfterBattle, 3.5);
	}

	/// <summary>
	/// 战斗结束后打开结算界面（按游戏模式选择：对战→结算界面，故事→胜利推进/失败继续，其他→主菜单）。
	/// </summary>
    protected void OpenMenuAfterBattle() {
        if (UFE.gameMode == GameMode.VersusMode || UFE.gameMode == GameMode.ChallengeMode || UFE.gameMode == GameMode.NetworkGame) {
			UFE.StartVersusModeAfterBattleScreen();
		}else if (UFE.gameMode == GameMode.StoryMode) {
			if (this.player1.winner) {
				UFE.WonStoryModeBattle();
			}else {
				UFE.StartStoryModeContinueScreen();
			}
		}else {
			UFE.StartMainMenuScreen();
		}
    }

	/// <summary>
	/// 游戏暂停回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="isPaused">是否暂停。</param>
	protected virtual void OnGamePaused(bool isPaused){

	}

	/// <summary>
	/// 回合开始回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="roundNumber">回合编号。</param>
	protected virtual void OnRoundBegin(int roundNumber){
		
	}

	/// <summary>
	/// 回合结束回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="winner">获胜角色。</param>
	/// <param name="loser">失败角色。</param>
	protected virtual void OnRoundEnd(UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser){
		//++this.player1WonRounds;
		//++this.playe21WonRounds;
	}

	/// <summary>
	/// 生命值变化回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="newFloat">新生命值。</param>
	/// <param name="player">所属角色。</param>
    protected virtual void OnLifePointsChange(float newFloat, UFE3D.CharacterInfo player) {
        // You can use this to have your own custom events when a player's life points changes
        // player.playerNum = 1 or 2
	}

	/// <summary>
	/// 文字提示回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="msg">提示文本。</param>
	/// <param name="player">所属角色。</param>
    protected virtual void OnNewAlert(string msg, UFE3D.CharacterInfo player) {
        // You can use this to have your own custom events when a new text alert is fired from the engine
        // player.playerNum = 1 or 2
	}

	/// <summary>
	/// 命中事件回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="strokeHitBox">打击判定盒。</param>
	/// <param name="move">招式。</param>
	/// <param name="player">攻击角色。</param>
	protected virtual void OnHit(HitBox strokeHitBox, MoveInfo move, UFE3D.CharacterInfo player){
        // player.playerNum = 1 or 2
		// You can use this to have your own custom events when a character gets hit
	}

	/// <summary>
	/// 格挡事件回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="strokeHitBox">打击判定盒。</param>
	/// <param name="move">招式。</param>
	/// <param name="player">格挡角色。</param>
    protected virtual void OnBlock(HitBox strokeHitBox, MoveInfo move, UFE3D.CharacterInfo player) {
        // You can use this to have your own custom events when a player blocks.
        // player.playerNum = 1 or 2
        // player = character blocking
    }

	/// <summary>
	/// 弹反事件回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="strokeHitBox">打击判定盒。</param>
	/// <param name="move">招式。</param>
	/// <param name="player">弹反角色。</param>
    protected virtual void OnParry(HitBox strokeHitBox, MoveInfo move, UFE3D.CharacterInfo player) {
        // You can use this to have your own custom events when a character parries an attack
        // player.playerNum = 1 or 2
        // player = character parrying
    }

	/// <summary>
	/// 出招事件回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="move">招式。</param>
	/// <param name="player">出招角色。</param>
    protected virtual void OnMove(MoveInfo move, UFE3D.CharacterInfo player) {
        // Fires when a player successfully executes a move
        // player.playerNum = 1 or 2
	}

	/// <summary>
	/// 基础动作事件回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="basicMove">基础动作。</param>
	/// <param name="player">执行角色。</param>
    protected virtual void OnBasicMove(BasicMoveReference basicMove, UFE3D.CharacterInfo player) {
        // Fires when a player successfully executes a move
        // player.playerNum = 1 or 2
    }

	/// <summary>
	/// 按键事件回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="buttonPress">按下的按钮。</param>
	/// <param name="player">按下角色。</param>
    protected virtual void OnButtonPress(ButtonPress buttonPress, UFE3D.CharacterInfo player) {
        // Fires when a player successfully executes a move
        // player.playerNum = 1 or 2
    }

	/// <summary>
	/// 身体部位可见性变化回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="move">招式。</param>
	/// <param name="player">所属角色。</param>
	/// <param name="bodyPartVisibilityChange">可见性变化数据。</param>
	/// <param name="hitBox">关联判定盒。</param>
    protected virtual void OnBodyVisibilityChange(MoveInfo move, UFE3D.CharacterInfo player, BodyPartVisibilityChange bodyPartVisibilityChange, HitBox hitBox) {
        // Fires when a move casts a body part visibility change
        // player.playerNum = 1 or 2
    }

	/// <summary>
	/// 粒子特效事件回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="move">招式。</param>
	/// <param name="player">所属角色。</param>
	/// <param name="particleEffects">粒子效果数据。</param>
    protected virtual void OnParticleEffects(MoveInfo move, UFE3D.CharacterInfo player, MoveParticleEffect particleEffects) {
        // Fires when a move casts a particle effect
        // player.playerNum = 1 or 2
    }

	/// <summary>
	/// 换边事件回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="side">新朝向侧。</param>
	/// <param name="player">换边角色。</param>
    protected virtual void OnSideSwitch(int side, UFE3D.CharacterInfo player) {
        // Fires when a character switches orientation
        // player.playerNum = 1 or 2
        /*if (player.playerNum == 1) {
            leftZoneRegular.SetActive(false);
            rightZoneRegular.SetActive(false);
            leftZoneMirror.SetActive(false);
            rightZoneMirror.SetActive(false);

            if (side == -1) {
                leftZoneMirror.SetActive(true);
                rightZoneMirror.SetActive(true);
            } else {
                leftZoneRegular.SetActive(true);
                rightZoneRegular.SetActive(true);
            }
        }*/
    }

	/// <summary>
	/// 计时器更新回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="time">剩余时间。</param>
	protected virtual void OnTimer(FPLibrary.Fix64 time){

	}

	/// <summary>
	/// 时间到回调（虚方法，默认空实现）。
	/// </summary>
	protected virtual void OnTimeOver(){
		
	}

	/// <summary>
	/// 输入更新回调（虚方法，默认空实现）。
	/// </summary>
	/// <param name="inputReferences">输入引用列表。</param>
	/// <param name="player">玩家编号。</param>
	protected virtual void OnInput(InputReferences[] inputReferences, int player){

	}
	#endregion
}
