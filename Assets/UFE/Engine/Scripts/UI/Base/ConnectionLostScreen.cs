using UnityEngine;
using System.Collections;

/// <summary>
/// 连接丢失界面（ConnectionLostScreen）。
/// <para>用途：网络连接断开时显示的界面基类，提供返回主菜单或重新进入网络游戏界面（重连）的方法。</para>
/// </summary>
public class ConnectionLostScreen : UFEScreen{
	/// <summary>
	/// 返回主菜单。
	/// </summary>
	public virtual void GoToMainMenu(){
		UFE.StartMainMenuScreen();
	}

	/// <summary>
	/// 重新进入网络游戏界面（尝试重连）。
	/// </summary>
	public virtual void GoToNetworkGameScreen(){
		UFE.GoToNetworkGameScreen();
	}
}
