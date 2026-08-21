using UnityEngine;
using System.Collections;

/// <summary>
/// 建房界面（HostGameScreen）。
/// <para>用途：局域网联机模式下的"创建房间"界面基类，提供开始建房、返回网络界面或连接丢失界面的方法。</para>
/// </summary>
public class HostGameScreen : UFEScreen{
	/// <summary>
	/// 返回网络游戏界面。
	/// </summary>
	public virtual void GoToNetworkGameScreen(){
		UFE.StartNetworkGameScreen();
	}

	/// <summary>
	/// 进入连接丢失界面。
	/// </summary>
	public virtual void GoToConnectionLostScreen(){
		UFE.StartConnectionLostScreen();
	}

	/// <summary>
	/// 开始创建主机房间。
	/// </summary>
	public virtual void StartHostGame() {
		UFE.HostGame();
	}
}
