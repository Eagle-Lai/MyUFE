using UnityEngine;
using System.Collections;

/// <summary>
/// 网络状态（NetworkState）：当前网络连接的角色状态。
/// </summary>
public enum NetworkState{
	/// <summary>未连接。</summary>
	Disconnected,
	/// <summary>客户端。</summary>
	Client,
	/// <summary>服务器（主机）。</summary>
	Server
}
