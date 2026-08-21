using UnityEngine;
using System.Collections;

/// <summary>
/// 片头界面（IntroScreen）。
/// <para>用途：游戏启动时的片头/Logo 展示界面基类，提供跳转到主菜单的方法。</para>
/// </summary>
public class IntroScreen : UFEScreen {
	/// <summary>
	/// 跳转到主菜单。
	/// </summary>
	public virtual void GoToMainMenu(){
		UFE.StartMainMenuScreen(0f);
	}
}
