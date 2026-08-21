using UnityEngine;
using System.Collections;
using System;
using System.Net;

/// <summary>
/// 网络游戏界面（NetworkGameScreen）。
/// <para>用途：网络联机模式的主界面基类，提供建房/加入游戏导航与获取本机 IP 地址的方法。</para>
/// </summary>
public class NetworkGameScreen : UFEScreen{
	/// <summary>
	/// 返回主菜单。
	/// </summary>
	public virtual void GoToMainMenu(){
		UFE.StartMainMenuScreen();
	}

	/// <summary>
	/// 进入建房界面。
	/// </summary>
	public virtual void GoToHostGameScreen(){
		UFE.StartHostGameScreen();
	}

	/// <summary>
	/// 进入加入游戏界面。
	/// </summary>
	public virtual void GoToJoinGameScreen(){
		UFE.StartJoinGameScreen();
	}

	/// <summary>
	/// 获取本机 IPv4 地址。
	/// </summary>
	/// <returns>本机 IP 地址字符串。</returns>
	public virtual string GetIp() {
		string hostName = System.Net.Dns.GetHostName();
		IPHostEntry ipHostEntry = System.Net.Dns.GetHostEntry(hostName);
		IPAddress[] ipAddresses = ipHostEntry.AddressList;
		
		return ipAddresses[ipAddresses.Length - 1].ToString();
	}
}
