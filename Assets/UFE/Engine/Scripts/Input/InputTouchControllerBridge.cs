using UnityEngine;
///--------------------------------------------------------------------------------------------------------------------
/// <summary>
/// This class is used by Control Freak 2.
/// https://www.assetstore.unity3d.com/#/content/11562
/// </summary>
///--------------------------------------------------------------------------------------------------------------------
/// <summary>
/// 触屏输入桥接器（InputTouchControllerBridge）。
/// <para>用途：为 Control Freak 2（移动端虚拟摇杆插件）提供输入读取与战斗控件显隐的抽象基类。</para>
/// <para>订阅 UFE 游戏事件，在战斗开始/结束/暂停/界面切换时自动控制触屏战斗控件显示。</para>
/// </summary>

// [DGT]

/// <summary>
/// 触屏输入桥接抽象基类：派生类实现具体的摇杆读取与控件显示逻辑。
/// </summary>
public abstract class InputTouchControllerBridge : MonoBehaviour 
	{
	/// <summary>
	/// 初始化桥接器（虚方法，默认空实现）。
	/// </summary>
	virtual public void		Init()		{ }

	/// <summary>
	/// 读取指定轴的当前值。
	/// </summary>
	/// <param name="axisName">轴名称。</param>
	/// <returns>轴当前值。</returns>
	abstract public float	GetAxis		(string axisName);
	/// <summary>
	/// 读取指定轴的原始值。
	/// </summary>
	/// <param name="axisName">轴名称。</param>
	/// <returns>轴原始值。</returns>
	abstract public float	GetAxisRaw	(string axisName);
	/// <summary>
	/// 读取指定按钮的按下状态。
	/// </summary>
	/// <param name="axisName">按钮名称。</param>
	/// <returns>按钮是否被按下。</returns>
	abstract public bool		GetButton	(string axisName);

	/// <summary>
	/// 显示/隐藏战斗触屏控件。
	/// </summary>
	/// <param name="visible">是否可见。</param>
	/// <param name="animate">是否带动画。</param>
	abstract public void		ShowBattleControls	(bool visible, bool animate);

	/// <summary>
	/// 上一帧是否处于战斗 GUI 状态（用于检测变化）。
	/// </summary>
	private bool
		prevBattleGUI,
	/// <summary>
	/// 上一帧游戏是否暂停（用于检测变化）。
	/// </summary>
		prevGamePaused;


	// ----------------
	/// <summary>
	/// 启用时订阅 UFE 游戏事件并初始化桥接器。
	/// </summary>
	void OnEnable()
		{
		UFE.OnGameEnds		+= this.OnGameEnds;
		UFE.OnRoundBegins	+= this.OnRoundBegins;
		UFE.OnRoundEnds	+= this.OnRoundEnds;
		UFE.OnGamePaused	+= this.OnGamePaused;
		UFE.OnScreenChanged += this.OnScreenChanged;

		this.prevBattleGUI = false;
		this.prevGamePaused = false;

		this.Init();
		}

	
	// ---------------
	/// <summary>
	/// 禁用时取消订阅 UFE 游戏事件。
	/// </summary>
	void OnDisable()
		{		
		UFE.OnGameEnds		-= this.OnGameEnds;
		UFE.OnRoundBegins	-= this.OnRoundBegins;
		UFE.OnRoundEnds	-= this.OnRoundEnds;
		UFE.OnGamePaused	-= this.OnGamePaused;
		UFE.OnScreenChanged -= this.OnScreenChanged;

		}


	// ----------------
	/// <summary>
	/// 固定帧率更新：根据战斗 GUI/暂停状态变化自动显隐战斗触屏控件。
	/// </summary>
	public void DoFixedUpdate()
		{
		bool battleGUI = (UFE.battleGUI != null);
		bool gamePaused = UFE.isPaused();

		if (battleGUI != this.prevBattleGUI)
			{
			this.ShowBattleControls(battleGUI && !gamePaused, battleGUI);
			}
		
		else if (gamePaused != this.prevGamePaused)
			{
			if (battleGUI)
				{
				this.ShowBattleControls(!gamePaused, true);
				}
			}

		this.prevBattleGUI	= battleGUI;
		this.prevGamePaused	= gamePaused;
		}



	// ---------------
	/// <summary>
	/// 游戏结束回调：隐藏战斗触屏控件。
	/// </summary>
	/// <param name="winner">获胜角色。</param>
	/// <param name="loser">失败角色。</param>
	private void OnGameEnds(UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser)
		{
//Debug.Log(ControlFreak2.CFUtils.LogPrefix() + "OnGameEnds");
	    this.ShowBattleControls(false, false);
		}

	// -------------
	/// <summary>
	/// 回合结束回调（当前为空操作）。
	/// </summary>
	/// <param name="winner">获胜角色。</param>
	/// <param name="loser">失败角色。</param>
	private void OnRoundEnds(UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser)
		{
//Debug.Log(ControlFreak2.CFUtils.LogPrefix() + "Round Ends");
	//	this.ShowBattleControls(false, true);
		}

	// ---------------
	/// <summary>
	/// 回合开始回调（当前为空操作）。
	/// </summary>
	/// <param name="roundNum">回合编号。</param>
	private void OnRoundBegins(int roundNum)
		{
//Debug.Log(ControlFreak2.CFUtils.LogPrefix() + "Round Begin : " + roundNum);
	//	this.ShowBattleControls(true, true);
		
		}

	// -------------------
	/// <summary>
	/// 游戏暂停回调（当前为空操作）。
	/// </summary>
	/// <param name="paused">是否暂停。</param>
	private void OnGamePaused(bool paused)
		{
//Debug.Log(ControlFreak2.CFUtils.LogPrefix() + "GamePaused : " + paused);
//this.ShowBattleControls(!paused, true);
		}

	// -----------------
	/// <summary>
	/// 界面切换回调（当前为空操作）。
	/// </summary>
	/// <param name="old">切换前的界面。</param>
	/// <param name="newScreen">切换后的界面。</param>
	private void OnScreenChanged(UFEScreen old, UFEScreen newScreen)
		{
//Debug.Log(ControlFreak2.CFUtils.LogPrefix() + "Screen change:" + (old != null ? old.GetType().Name : "NULL") + 
//		" new:" + (newScreen != null ? newScreen.GetType().Name : "NULL"));
		}

	
	
	}
