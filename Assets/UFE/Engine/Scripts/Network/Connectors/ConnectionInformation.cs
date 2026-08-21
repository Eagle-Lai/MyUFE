using UnityEngine;
using System.Collections;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

/// <summary>
/// 连接信息（ConnectionInformation）。
/// <para>用途：描述一条网络连接的主机信息（端口、公网/内网地址、Unity 节点 ID），支持从 MatchInfoSnapshot 直接构造。</para>
/// </summary>
public class ConnectionInformation{
	/// <summary>连接端口。</summary>
	public int port = 0;
	/// <summary>公网地址。</summary>
	public string publicAddress = null;
	/// <summary>内网地址。</summary>
	public string privateAddress = null;
	/// <summary>Unity 网络节点 ID。</summary>
	public NodeID unityNodeId = NodeID.Invalid;

	/// <summary>默认构造函数。</summary>
	public ConnectionInformation(){}

	/// <summary>构造函数（地址与端口，公网=内网）。</summary>
	/// <param name="address">地址。</param>
	/// <param name="port">端口。</param>
	public ConnectionInformation(string address, int port) : this(address, address, port){}

	/// <summary>构造函数（内网/公网地址与端口）。</summary>
	/// <param name="privateAddress">内网地址。</param>
	/// <param name="publicAddress">公网地址。</param>
	/// <param name="port">端口。</param>
	public ConnectionInformation(string privateAddress, string publicAddress, int port) : 
	this (privateAddress, publicAddress, port, NodeID.Invalid){}

	/// <summary>构造函数（完整参数）。</summary>
	/// <param name="privateAddress">内网地址。</param>
	/// <param name="publicAddress">公网地址。</param>
	/// <param name="port">端口。</param>
	/// <param name="unityNodeId">Unity 节点 ID。</param>
	public ConnectionInformation(
		string privateAddress, 
		string publicAddress, 
		int port, 
		NodeID unityNodeId
	){
		this.privateAddress = privateAddress;
		this.publicAddress = publicAddress;
		this.port = port;
		this.unityNodeId = unityNodeId;
	}

	/// <summary>从 MatchInfoSnapshot 的直连信息构造连接信息。</summary>
	/// <param name="info">Unity 比赛快照的直连信息。</param>
	public ConnectionInformation(MatchInfoSnapshot.MatchInfoDirectConnectSnapshot info) : this(
		info.privateAddress,
		info.publicAddress,
		UFE.config.networkOptions.port,
		info.nodeId
	){}
}
