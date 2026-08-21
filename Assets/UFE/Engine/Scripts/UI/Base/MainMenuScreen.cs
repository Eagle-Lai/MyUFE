using UnityEngine;
using System.Collections;

/// <summary>
/// 主菜单界面（MainMenuScreen）。
/// <para>用途：游戏主菜单基类，提供进入各玩法模式（故事/对战/训练/网络/蓝牙/随机匹配）与选项/制作人员的导航方法。</para>
/// </summary>
public class MainMenuScreen : UFEScreen {
	/// <summary>
	/// 退出游戏。
	/// </summary>
	public virtual void Quit(){
		UFE.Quit();
	}

	/// <summary>
	/// 进入蓝牙对战界面。
	/// </summary>
	public virtual void GoToBluetoothPlayScreen(){
		UFE.StartBluetoothGameScreen();
	}

	/// <summary>
	/// 进入搜索匹配界面。
	/// </summary>
	public virtual void GoToSearchMatchScreen(){
		UFE.StartSearchMatchScreen();
	}

	/// <summary>
	/// 进入故事模式。
	/// </summary>
	public virtual void GoToStoryModeScreen(){
		UFE.StartStoryMode();
	}

	/// <summary>
	/// 进入对战模式选择界面。
	/// </summary>
	public virtual void GoToVersusModeScreen(){
		UFE.StartVersusModeScreen();
	}

	/// <summary>
	/// 进入训练模式。
	/// </summary>
	public virtual void GoToTrainingModeScreen(){
		UFE.StartTrainingMode();
	}

	/// <summary>
	/// 进入网络对战界面。
	/// </summary>
	public virtual void GoToNetworkPlayScreen(){
		UFE.StartNetworkGameScreen();
	}

	/// <summary>
	/// 进入选项界面。
	/// </summary>
	public virtual void GoToOptionsScreen(){
		UFE.StartOptionsScreen();
	}

	/// <summary>
	/// 进入制作人员界面。
	/// </summary>
	public virtual void GoToCreditsScreen(){
		UFE.StartCreditsScreen();
	}
}
