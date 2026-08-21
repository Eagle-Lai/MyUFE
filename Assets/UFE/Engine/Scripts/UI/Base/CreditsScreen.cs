using UnityEngine;
using System.Collections;

/// <summary>
/// 制作人员界面（CreditsScreen）。
/// <para>用途：游戏制作人员/版权信息展示界面基类，提供返回主菜单的方法。</para>
/// </summary>
public class CreditsScreen : UFEScreen{
	/// <summary>
	/// 返回主菜单。
	/// </summary>
	public virtual void GoToMainMenuScreen(){
		UFE.StartMainMenuScreen();
	}
}
