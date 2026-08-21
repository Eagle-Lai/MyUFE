using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UFE3D;

/// <summary>
/// 挑战模式（ChallengeMode）。
/// <para>用途：驱动挑战模式玩法——监听玩家1的事件（出招/基础动作/按键），与当前挑战的动作序列比对，</para>
/// <para>依次完成所有要求动作即完成挑战；支持"自动进入下一挑战"序列并显示挑战描述与跳过按钮。</para>
/// </summary>
public class ChallengeMode : MonoBehaviour {

	/// <summary>当前挑战要求的动作序列列表。</summary>
    private List<ActionSequence> challengeActions;
	/// <summary>玩家1的角色控制脚本引用（用于判断当前状态）。</summary>
    public ControlsScript cScript;
	/// <summary>当前挑战索引。</summary>
    public int currentChallenge = 0;
	/// <summary>当前完成到第几个要求动作。</summary>
    public int currentAction = 0;
	/// <summary>当前挑战是否已完成。</summary>
    public bool complete;
	/// <summary>完成后是否自动进入下一挑战。</summary>
    public bool moveToNext;
	/// <summary>下一挑战开始时是否重置回合。</summary>
    public bool resetRound;
	/// <summary>挑战描述 GUI 样式。</summary>
    public GUIStyle style = new GUIStyle();
	/// <summary>挑战描述使用的字体。</summary>
    public Font font;

	/// <summary>
	/// 启动：初始化 GUI 样式、定位当前挑战并开始执行。
	/// </summary>
    public void Start() {
        //style.fontSize = 30;
        style.font = (Font)Resources.Load("Robustik");
        style.alignment = TextAnchor.MiddleCenter;
        currentChallenge = UFE.config.selectedChallenge;
        challengeActions = new List<ActionSequence>(UFE.GetChallenge(currentChallenge).actionSequence);

        Run();
	}

	/// <summary>
	/// 开始运行挑战：订阅 UFE 事件并重置进度。
	/// </summary>
    public void Run() {
        UFE.OnMove += this.OnMove;
        UFE.OnBasicMove += this.OnBasicMove;
        UFE.OnButton += this.OnButtonPress;
        complete = false;
        moveToNext = false;
        resetRound = false;
        currentAction = 0;
    }

	/// <summary>
	/// 停止挑战：取消订阅 UFE 事件。
	/// </summary>
    public void Stop() {
        UFE.OnMove -= this.OnMove;
        UFE.OnBasicMove -= this.OnBasicMove;
        UFE.OnButton -= this.OnButtonPress;
    }
    
	/// <summary>
	/// 招式触发回调：玩家1成功使出必杀技时校验是否匹配当前要求动作。
	/// </summary>
	/// <param name="move">使出的招式。</param>
	/// <param name="player">使出的玩家角色。</param>
    protected virtual void OnMove(MoveInfo move, UFE3D.CharacterInfo player) {
        // Fires when a player successfully executes a move
        // player.playerNum = 1 or 2
        if (player.playerNum == 1
            && !complete
            && !UFE.config.lockInputs
            && UFE.gameMode == GameMode.ChallengeMode
            && challengeActions[currentAction].actionType == ActionType.SpecialMove
            && challengeActions[currentAction].specialMove == move) {
            currentAction++;
            testChallenge();
        } else {
            currentAction = 0;
        }
    }

	/// <summary>
	/// 基础动作触发回调：玩家1成功执行基础动作时校验是否匹配当前要求动作。
	/// </summary>
	/// <param name="basicMove">执行的基础动作。</param>
	/// <param name="player">执行的玩家角色。</param>
    protected virtual void OnBasicMove(BasicMoveReference basicMove, UFE3D.CharacterInfo player) {
        // Fires when a player successfully executes a move
        // player.playerNum = 1 or 2
        if (player.playerNum == 1
            && !complete
            && !UFE.config.lockInputs
            && UFE.gameMode == GameMode.ChallengeMode
            && challengeActions[currentAction].actionType == ActionType.BasicMove
            && challengeActions[currentAction].basicMove == basicMove) {
            currentAction++;
            testChallenge();
        } else {
            currentAction = 0;
        }
    }

	/// <summary>
	/// 按键触发回调：玩家1按下按钮时校验是否匹配当前要求动作。
	/// </summary>
	/// <param name="buttonPress">按下的按钮。</param>
	/// <param name="player">按下的玩家角色。</param>
    protected virtual void OnButtonPress(ButtonPress buttonPress, UFE3D.CharacterInfo player) {
        // Fires when a player successfully executes a move
        // player.playerNum = 1 or 2
        if (player.playerNum == 1
            && !complete
            && !UFE.config.lockInputs
            && UFE.gameMode == GameMode.ChallengeMode
            && challengeActions[currentAction].actionType == ActionType.ButtonPress
            && challengeActions[currentAction].button == buttonPress) {
            currentAction++;
            testChallenge();
        } else {
            currentAction = 0;
        }
    }

	/// <summary>
	/// 校验挑战进度：全部要求动作完成则标记挑战完成，并按配置决定是否进入下一挑战。
	/// </summary>
    private void testChallenge() {
        if (currentAction == challengeActions.Count) {
            if (UFE.GetChallenge(currentChallenge).challengeSequence == ChallengeAutoSequence.MoveToNext) {
                moveToNext = true;
                if (UFE.GetChallenge(currentChallenge).resetData) resetRound = true;

                currentChallenge++;
                challengeActions = new List<ActionSequence>(UFE.GetChallenge(currentChallenge).actionSequence);
            } else {
                moveToNext = false;
            }

            complete = true;
        } 
    }

	/// <summary>
	/// 渲染挑战 GUI：显示挑战描述文本与"跳过"按钮。
	/// </summary>
    public void OnGUI() {
        if (UFE.GetChallenge(currentChallenge).description != "" 
            && !complete
            && !UFE.config.lockInputs
            && !UFE.config.lockMovements) {

            if (GUI.Button(new Rect(Screen.width - 120, 50, 70, 30), "Skip")) {
                moveToNext = false;
                complete = true;
                UFE.fluxCapacitor.EndRound();
            }

            GUI.Box(new Rect(0, 150, Screen.width, 40), UFE.GetChallenge(currentChallenge).description, style);
            //GUI.Box(new Rect(0, Screen.height - 60, Screen.width, 40), UFE.GetChallenge(currentChallenge).description);
            /*GUI.BeginGroup(new Rect(0, Screen.height - 100, Screen.width, 100));
            {
                GUILayout.Label(UFE.GetChallenge(currentChallenge).description);
            } GUI.EndGroup();*/
        }
    }
}
