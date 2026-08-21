using UnityEngine;
using System.Collections;

/// <summary>
/// 对战模式界面（VersusModeScreen）。
/// <para>用途：对战模式选择界面基类，提供 PVP/PvC/CvC 三种对战方式与返回主菜单的导航方法。</para>
/// </summary>
public class VersusModeScreen : UFEScreen{
	/// <summary>
	/// 选择玩家 vs 玩家对战。
	/// </summary>
	public virtual void SelectPlayerVersusPlayer(){
		UFE.StartPlayerVersusPlayer();
	}

	/// <summary>
	/// 选择玩家 vs CPU 对战。
	/// </summary>
	public virtual void SelectPlayerVersusCpu(){
		UFE.StartPlayerVersusCpu();
	}

	/// <summary>
	/// 选择 CPU vs CPU 对战。
	/// </summary>
	public virtual void SelectCpuVersusCpu(){
		UFE.StartCpuVersusCpu();
	}

	/// <summary>
	/// 返回主菜单。
	/// </summary>
	public virtual void GoToMainMenu(){
		UFE.StartMainMenuScreen();
	}
}
