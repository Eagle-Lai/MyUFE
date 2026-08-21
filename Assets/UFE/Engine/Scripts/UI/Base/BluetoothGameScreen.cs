using UnityEngine;
using System.Collections;
using System;
using System.Net;

/// <summary>
/// 蓝牙对战界面（BluetoothGameScreen）。
/// <para>用途：蓝牙联机模式的主界面基类，提供创建蓝牙房间（主机）与加入蓝牙房间（客户端）的方法。</para>
/// </summary>
public class BluetoothGameScreen : UFEScreen{
	/// <summary>
	/// 返回主菜单。
	/// </summary>
	public virtual void GoToMainMenu(){
		UFE.StartMainMenuScreen();
	}

	/// <summary>
	/// 作为主机创建蓝牙对战房间。
	/// </summary>
	public virtual void HostGame() {
		UFE.HostBluetoothGame();
	}

	/// <summary>
	/// 作为客户端搜索并加入蓝牙对战房间。
	/// </summary>
	public virtual void JoinGame() {
		UFE.JoinBluetoothGame();
	}
}
