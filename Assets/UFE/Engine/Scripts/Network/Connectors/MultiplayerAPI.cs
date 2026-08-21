using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
// UFE Legacy API Compatibility alias (Unity HLAPI NetworkIdentity 已移除，见 "UFE Addons/Compatibility/UFELegacyAPICompat.cs")
using NetworkIdentity = NetworkIdentityCompat;

/// <summary>
/// 多人 API 抽象基类（MultiplayerAPI）。
/// <para>用途：定义联机对战（创建/加入/搜索/断开比赛、连接事件、消息收发）的统一抽象接口，</para>
/// <para>供 UFE 引擎调用；具体实现（Unity 网络 / Photon / 蓝牙 / Null）通过继承该类提供。</para>
/// <para>本文件同时定义了网络响应结构体与比赛/玩家信息数据类。</para>
/// </summary>

/// <summary>
/// 基础响应（BasicResponse）：携带成功标志的通用响应。
/// </summary>
public struct BasicResponse{
	/// <summary>是否成功。</summary>
	public bool success{
		get{
			return this._success;
		}
	}

	/// <summary>成功标志内部存储。</summary>
	private bool _success;

	/// <summary>
	/// 构造函数。
	/// </summary>
	/// <param name="success">是否成功。</param>
	public BasicResponse(bool success){
		this._success = success;
	}
}

/// <summary>
/// 创建比赛响应（CreateMatchResponse）：创建比赛结果的响应数据。
/// </summary>
public struct CreateMatchResponse{
	/// <summary>访问令牌字符串。</summary>
	public NetworkAccessToken accessTokenString{
		get{
			return this._accessTokenString;
		}
	}

	/// <summary>网络 ID。</summary>
	public NetworkID networkId {
		get {
			return this._networkId;
		}
	}

	/// <summary>节点 ID。</summary>
	public NodeID nodeId {
		get {
			return this._nodeId;
		}
	}

	/// <summary>是否成功。</summary>
	public bool success {
		get {
			return this._success;
		}
	}

	/// <summary>访问令牌内部存储。</summary>
	private NetworkAccessToken _accessTokenString;
	/// <summary>网络 ID 内部存储。</summary>
	private NetworkID _networkId;
	/// <summary>节点 ID 内部存储。</summary>
	private NodeID _nodeId;
	/// <summary>成功标志内部存储。</summary>
	private bool _success;

	/// <summary>
	/// 构造函数。
	/// </summary>
	/// <param name="success">是否成功。</param>
	/// <param name="networkId">网络 ID。</param>
	/// <param name="nodeId">节点 ID。</param>
	/// <param name="accessTokenString">访问令牌。</param>
	public CreateMatchResponse(bool success, NetworkID networkId, NodeID nodeId, NetworkAccessToken accessTokenString){
		this._success = success;
		this._networkId = networkId;
		this._nodeId = nodeId;
		this._accessTokenString = accessTokenString;
	}
}

/// <summary>
/// 加入比赛响应（JoinMatchResponse）：加入比赛结果的响应数据。
/// </summary>
public struct JoinMatchResponse{
	/// <summary>访问令牌字符串。</summary>
	public string accessTokenString{
		get{
			return this._accessTokenString;
		}
	}

	/// <summary>网络 ID（来自比赛信息）。</summary>
	public NetworkID networkId{
		get{
			return this._matchInfo.networkId;
		}
	}

	/// <summary>节点 ID（来自比赛信息）。</summary>
	public NodeID nodeId{
		get{
			return this._matchInfo.nodeId;
		}
	}

	/// <summary>是否成功。</summary>
	public bool success{
		get {
			return this._success;
		}
	}


	/// <summary>访问令牌内部存储。</summary>
	private string _accessTokenString;
	/// <summary>比赛信息内部存储。</summary>
	private MatchInfo _matchInfo;
	/// <summary>成功标志内部存储。</summary>
	private bool _success;

	/// <summary>
	/// 构造函数。
	/// </summary>
	/// <param name="success">是否成功。</param>
	/// <param name="accessTokenString">访问令牌。</param>
	/// <param name="matchInfo">比赛信息。</param>
	public JoinMatchResponse(bool success, string accessTokenString, MatchInfo matchInfo){
		this._success = success;
		this._accessTokenString = accessTokenString;
		this._matchInfo = matchInfo;
	}
}

/// <summary>
/// 比赛列表响应（ListMatchResponse）：搜索比赛结果的响应数据。
/// </summary>
public class ListMatchResponse{
	/// <summary>是否成功。</summary>
	public bool success;
	/// <summary>比赛信息数组。</summary>
	public MatchInfo[] matches;	
}

/// <summary>
/// 多人 API 抽象基类（MultiplayerAPI）。
/// </summary>
public abstract class MultiplayerAPI : MonoBehaviour{
	#region class definitions
	/// <summary>
	/// 比赛创建请求：创建比赛所需的参数。
	/// </summary>
	public class MatchCreationRequest{
		/// <summary>比赛名称。</summary>
		public string matchName = null;
		/// <summary>最大玩家数。</summary>
		public int maxPlayers = 2;
		/// <summary>房间密码（null 表示公开房）。</summary>
		public string password = null;
		/// <summary>端口。</summary>
		public int port = 0;
		/// <summary>是否公开比赛。</summary>
		public bool publicMatch = true;

		/// <summary>
		/// 构造函数（使用配置的端口）。
		/// </summary>
		/// <param name="matchName">比赛名称。</param>
		/// <param name="maxPlayers">最大玩家数。</param>
		/// <param name="publicMatch">是否公开。</param>
		/// <param name="password">密码。</param>
		public MatchCreationRequest(
			string matchName = null, 
			int maxPlayers = 2, 
			bool publicMatch = true, 
			string password = null
		) : this(UFE.config.networkOptions.port, matchName, maxPlayers, publicMatch, password){}

		/// <summary>
		/// 构造函数（指定端口）。
		/// </summary>
		/// <param name="port">端口。</param>
		/// <param name="matchName">比赛名称。</param>
		/// <param name="maxPlayers">最大玩家数。</param>
		/// <param name="publicMatch">是否公开。</param>
		/// <param name="password">密码。</param>
		public MatchCreationRequest(
			int port, 
			string matchName = null, 
			int maxPlayers = 2, 
			bool publicMatch = true, 
			string password = null
		){
			this.matchName = matchName;
			this.maxPlayers = maxPlayers;
			this.password = password;
			this.port = port;
			this.publicMatch = publicMatch;
		}
	}

	/// <summary>
	/// 已创建比赛信息：主机创建比赛后的结果信息。
	/// </summary>
	public class CreatedMatchInformation{
		/// <summary>比赛名称。</summary>
		public string matchName = null;
		/// <summary>Unity 主机节点 ID。</summary>
		public NodeID unityHostNodeId = NodeID.Invalid;
		/// <summary>Unity 网络 ID。</summary>
		public NetworkID unityNetworkId = NetworkID.Invalid;

		/// <summary>默认构造函数。</summary>
		public CreatedMatchInformation() : this(null, NetworkID.Invalid, NodeID.Invalid){}

		/// <summary>构造函数（指定名称）。</summary>
		/// <param name="matchName">比赛名称。</param>
		public CreatedMatchInformation(string matchName) : this(matchName, NetworkID.Invalid, NodeID.Invalid){}

		/// <summary>构造函数（从响应构建）。</summary>
		/// <param name="response">创建比赛响应。</param>
		public CreatedMatchInformation(CreateMatchResponse response) : this(
			response.accessTokenString.GetByteString(),
			response.networkId,
			response.nodeId
		){}

		/// <summary>构造函数（完整参数）。</summary>
		/// <param name="matchName">比赛名称。</param>
		/// <param name="unityNetworkId">网络 ID。</param>
		/// <param name="unityHostNodeId">主机节点 ID。</param>
		public CreatedMatchInformation(string matchName, NetworkID unityNetworkId, NodeID unityHostNodeId){
			this.matchName = matchName;
			this.unityHostNodeId = unityHostNodeId;
			this.unityNetworkId = unityNetworkId;
		}
	}

	/// <summary>
	/// 已加入比赛信息：客户端加入比赛后的结果信息。
	/// </summary>
	public class JoinedMatchInformation{
		/// <summary>比赛名称。</summary>
		public string matchName = null;
		/// <summary>Unity 主机节点 ID。</summary>
		public NodeID unityHostNodeId = NodeID.Invalid;
		/// <summary>Unity 网络 ID。</summary>
		public NetworkID unityNetworkId = NetworkID.Invalid;

		/// <summary>默认构造函数。</summary>
		public JoinedMatchInformation() : this(null, NetworkID.Invalid, NodeID.Invalid){}

		/// <summary>构造函数（指定名称）。</summary>
		/// <param name="matchName">比赛名称。</param>
		public JoinedMatchInformation(string matchName) : this(matchName, NetworkID.Invalid, NodeID.Invalid){}

		/// <summary>构造函数（从响应构建）。</summary>
		/// <param name="response">加入比赛响应。</param>
		public JoinedMatchInformation(JoinMatchResponse response) : this(
			response.accessTokenString,
			response.networkId,
			response.nodeId
		){}

		/// <summary>构造函数（完整参数）。</summary>
		/// <param name="matchName">比赛名称。</param>
		/// <param name="unityNetworkId">网络 ID。</param>
		/// <param name="unityHostNodeId">主机节点 ID。</param>
		public JoinedMatchInformation(string matchName, NetworkID unityNetworkId, NodeID unityHostNodeId){
			this.matchName = matchName;
			this.unityHostNodeId = unityHostNodeId;
			this.unityNetworkId = unityNetworkId;
		}
	}

	/// <summary>
	/// 比赛信息：一个可加入的比赛描述（含连接信息与玩家数等）。
	/// </summary>
	public class MatchInformation{
		/// <summary>连接信息列表（内部存储）。</summary>
		private List<ConnectionInformation> _connections = new List<ConnectionInformation>();
		/// <summary>连接信息列表（只读）。</summary>
		public IList<ConnectionInformation> connections{
			get{
				return this._connections;
			}
		}

		/// <summary>平均 ELO 评分。</summary>
		public int averageEloScore = 0;
		/// <summary>当前玩家数。</summary>
		public int currentPlayers = 0;
		/// <summary>是否公开比赛。</summary>
		public bool isPublic = true;
		/// <summary>比赛名称。</summary>
		public string matchName = null;
		/// <summary>最大玩家数。</summary>
		public int maxPlayers = 2;
		/// <summary>Unity 主机节点 ID。</summary>
		public NodeID unityHostNodeId = NodeID.Invalid;
		/// <summary>Unity 网络 ID。</summary>
		public NetworkID unityNetworkId = NetworkID.Invalid;


		/// <summary>默认构造函数。</summary>
		public MatchInformation(){}

		/// <summary>构造函数（地址，端口取配置）。</summary>
		/// <param name="address">连接地址。</param>
		public MatchInformation(string address) : this(address, UFE.config.networkOptions.port){}

		/// <summary>构造函数（地址与端口）。</summary>
		/// <param name="address">连接地址。</param>
		/// <param name="port">端口。</param>
		public MatchInformation(string address, int port){
			this.connections.Add(new ConnectionInformation(address, port));
		}

		/// <summary>构造函数（从 Unity 比赛快照构建）。</summary>
		/// <param name="match">Unity 比赛快照。</param>
		public MatchInformation(MatchInfoSnapshot match){
			this.averageEloScore = match.averageEloScore;
			this.currentPlayers = match.currentSize;
			this.isPublic = !match.isPrivate;
			this.matchName = match.name;
			this.maxPlayers = match.maxSize;
			this.unityHostNodeId = match.hostNodeId;
			this.unityNetworkId = match.networkId;


			if (match.directConnectInfos != null){
				for (int i = 0; i < match.directConnectInfos.Count; ++i){
					if (match.directConnectInfos[i] != null){
						this.connections.Add(new ConnectionInformation(match.directConnectInfos[i]));
					}
				}
			}

			//m.matchAttributes
		}

		/// <summary>构造函数（从已创建比赛信息构建）。</summary>
		/// <param name="match">已创建比赛信息。</param>
		public MatchInformation(MultiplayerAPI.CreatedMatchInformation match){
			this.unityHostNodeId = match.unityHostNodeId;
			this.unityNetworkId = match.unityNetworkId;
		}
	}
    
	/// <summary>
	/// 玩家信息：一名连接玩家的信息（Unity 网络身份或 Photon 玩家对象）。
	/// </summary>
	public class PlayerInformation{
		/// <summary>Unity 网络身份（networkIdentity）。</summary>
        public NetworkIdentity networkIdentity { get; private set; }
		/// <summary>Photon 玩家对象。</summary>
        public object photonPlayer{get; private set;}

		/// <summary>构造函数（Unity 网络身份）。</summary>
		/// <param name="networkIdentity">网络身份。</param>
        public PlayerInformation(NetworkIdentity networkIdentity){
            this.networkIdentity = networkIdentity;
        }

		/// <summary>构造函数（Photon 玩家对象）。</summary>
		/// <param name="photonPlayer">Photon 玩家对象。</param>
        public PlayerInformation(object photonPlayer){
			if (photonPlayer == null){
				throw new ArgumentNullException();
			}

			this.photonPlayer = photonPlayer;
		}
	}
	#endregion


	#region public delegate definitions: Common Delegates
	/// <summary>初始化错误委托。</summary>
	public delegate void OnInitializationErrorDelegate();
	/// <summary>初始化成功委托。</summary>
	public delegate void OnInitializationSuccessfulDelegate();
	/// <summary>消息接收委托（参数：消息字节）。</summary>
	public delegate void OnMessageReceivedDelegate(byte[] bytes);
	#endregion

	#region public delegate definitions: Client Delegates
	/// <summary>断开连接委托。</summary>
	public delegate void OnDisconnectionDelegate();
	/// <summary>加入成功委托（参数：加入的比赛信息）。</summary>
	public delegate void OnJoinedDelegate(JoinedMatchInformation match);
	/// <summary>加入错误委托。</summary>
	public delegate void OnJoinErrorDelegate();
	/// <summary>比赛发现委托（参数：发现的比赛列表）。</summary>
	public delegate void OnMatchesDiscoveredDelegate(ReadOnlyCollection<MatchInformation> matches);
	/// <summary>比赛发现错误委托。</summary>
	public delegate void OnMatchDiscoveryErrorDelegate();
    #endregion

    #region public delegate definitions: Server Delegates
	/// <summary>比赛创建成功委托（参数：创建的比赛信息）。</summary>
	public delegate void OnMatchCreatedDelegate(CreatedMatchInformation match);
	/// <summary>比赛创建错误委托。</summary>
	public delegate void OnMatchCreationErrorDelegate();
	/// <summary>比赛销毁委托。</summary>
	public delegate void OnMatchDestroyedDelegate();
	/// <summary>玩家连接比赛委托（参数：玩家信息）。</summary>
	public delegate void OnPlayerConnectedToMatchDelegate(PlayerInformation player);
	/// <summary>玩家离开比赛委托（参数：玩家信息）。</summary>
	public delegate void OnPlayerDisconnectedFromMatchDelegate(PlayerInformation player);
	#endregion

	#region public event definitions: Common Events
	/// <summary>初始化错误事件。</summary>
	public event OnInitializationErrorDelegate OnInitializationError;
	/// <summary>初始化成功事件。</summary>
	public event OnInitializationSuccessfulDelegate OnInitializationSuccessful;
	/// <summary>消息接收事件。</summary>
	public event OnMessageReceivedDelegate OnMessageReceived;
	#endregion

	#region public class event definitions: Client Events
	/// <summary>断开连接事件。</summary>
	public event OnDisconnectionDelegate OnDisconnection;
	/// <summary>加入成功事件。</summary>
	public event OnJoinedDelegate OnJoined;
	/// <summary>加入错误事件。</summary>
	public event OnJoinErrorDelegate OnJoinError;
	/// <summary>比赛发现事件。</summary>
	public event OnMatchesDiscoveredDelegate OnMatchesDiscovered;
	/// <summary>比赛发现错误事件。</summary>
	public event OnMatchDiscoveryErrorDelegate OnMatchDiscoveryError;
	#endregion

	#region public event definitions: Server Events
	/// <summary>比赛创建成功事件。</summary>
	public event OnMatchCreatedDelegate OnMatchCreated;
	/// <summary>比赛创建错误事件。</summary>
	public event OnMatchCreationErrorDelegate OnMatchCreationError;
	/// <summary>比赛销毁事件。</summary>
	public event OnMatchDestroyedDelegate OnMatchDestroyed;
	/// <summary>玩家连接比赛事件。</summary>
	public event OnPlayerConnectedToMatchDelegate OnPlayerConnectedToMatch;
	/// <summary>玩家离开比赛事件。</summary>
	public event OnPlayerDisconnectedFromMatchDelegate OnPlayerDisconnectedFromMatch;
	#endregion

	#region public abstract properties
	/// <summary>当前连接数。</summary>
	public abstract int Connections{get;}
	/// <summary>当前玩家信息。</summary>
	public abstract PlayerInformation Player{get;}
	/// <summary>发送速率。</summary>
	public abstract float SendRate{get; set;}
	#endregion

	#region private instance fields
	/// <summary>初始化用的唯一 ID（UUID）。</summary>
	protected string _uuid = null;
	#endregion

	#region public instance methods
	/// <summary>
	/// 初始化多人 API（UUID 非空则触发成功事件，否则触发错误事件）。
	/// </summary>
	/// <param name="uuid">唯一 ID。</param>
	public virtual void Initialize(string uuid){
		if (uuid != null){
			this._uuid = uuid;
			this.RaiseOnInitializationSuccessful();	
		}else{
			this.RaiseOnInitializationError();
		}
	}
	#endregion

	#region public abstract methods
	// Client
	/// <summary>断开与比赛的连接。</summary>
	public abstract void DisconnectFromMatch();
	/// <summary>加入指定比赛。</summary>
	/// <param name="match">目标比赛。</param>
	/// <param name="password">可选密码。</param>
	public abstract void JoinMatch(MatchInformation match, string password = null);
	/// <summary>随机加入比赛。</summary>
	public abstract void JoinRandomMatch();
	/// <summary>开始搜索比赛。</summary>
	/// <param name="startPage">起始页。</param>
	/// <param name="pageSize">每页数量。</param>
	/// <param name="filter">过滤条件。</param>
	public abstract void StartSearchingMatches(int startPage = 0, int pageSize = 20, string filter = null);
	/// <summary>停止搜索比赛。</summary>
	public abstract void StopSearchingMatches();

	// Common
	/// <summary>获取当前连接状态。</summary>
	/// <returns>网络状态。</returns>
	public abstract NetworkState GetConnectionState();
	/// <summary>获取最后 Ping 值。</summary>
	/// <returns>Ping 值。</returns>
	public abstract int GetLastPing();

	// Server
	/// <summary>创建比赛。</summary>
	/// <param name="request">创建请求。</param>
	public abstract void CreateMatch(MatchCreationRequest request);
	/// <summary>销毁比赛。</summary>
	public abstract void DestroyMatch();
	#endregion

	#region public instance methods
	/// <summary>是否处于客户端连接状态。</summary>
	/// <returns>客户端返回 true。</returns>
	public bool IsClient(){
		return this.GetConnectionState() == NetworkState.Client;
	}

	/// <summary>是否已连接（非断开状态）。</summary>
	/// <returns>已连接返回 true。</returns>
	public bool IsConnected(){
		return this.GetConnectionState() != NetworkState.Disconnected;
	}

	/// <summary>是否处于服务器状态。</summary>
	/// <returns>服务器返回 true。</returns>
	public bool IsServer(){
		return this.GetConnectionState() == NetworkState.Server;
	}

	/// <summary>发送网络消息（泛型消息，自动序列化）。</summary>
	/// <typeparam name="T">消息数据类型。</typeparam>
	/// <param name="message">网络消息。</param>
	/// <returns>发送成功返回 true。</returns>
	public bool SendNetworkMessage<T>(NetworkMessage<T> message){
		return this.SendNetworkMessage(message.Serialize());
	}
	#endregion

	#region protected abstract methods
	/// <summary>发送原始字节消息（由子类实现具体传输）。</summary>
	/// <param name="bytes">消息字节。</param>
	/// <returns>发送成功返回 true。</returns>
	protected abstract bool SendNetworkMessage(byte[] bytes);
	#endregion

    #region protected instance methods: Common Events
	/// <summary>触发初始化错误事件。</summary>
	protected virtual void RaiseOnInitializationError(){
		if (this.OnInitializationError != null){
			this.OnInitializationError();
		}
	}

	/// <summary>触发初始化成功事件。</summary>
	protected virtual void RaiseOnInitializationSuccessful(){
		if (this.OnInitializationSuccessful != null){
			this.OnInitializationSuccessful();
		}
	}

	/// <summary>触发消息接收事件。</summary>
	/// <param name="bytes">消息字节。</param>
	protected virtual void RaiseOnMessageReceived(byte[] bytes){
		if (this.OnMessageReceived != null){
			this.OnMessageReceived(bytes);
		}
	}
    #endregion

    #region protected instance methods: Client Events
	/// <summary>触发断开连接事件。</summary>
    protected virtual void RaiseOnDisconnection(){
		if (this.OnDisconnection != null){
			this.OnDisconnection();
		}
	}

	/// <summary>触发比赛发现事件。</summary>
	/// <param name="matches">发现的比赛列表。</param>
	protected virtual void RaiseOnMatchesDiscovered(ReadOnlyCollection<MatchInformation> matches){
		if (this.OnMatchesDiscovered != null){
			this.OnMatchesDiscovered(matches);
		}
	}

	/// <summary>触发比赛发现错误事件。</summary>
	protected virtual void RaiseOnMatchDiscoveryError(){
		if (this.OnMatchDiscoveryError != null){
			this.OnMatchDiscoveryError();
		}
	}

	/// <summary>触发加入成功事件。</summary>
	/// <param name="match">加入的比赛信息。</param>
	protected virtual void RaiseOnJoined(JoinedMatchInformation match){
		if (this.OnJoined != null){
			this.OnJoined(match);
		}
	}

	/// <summary>触发加入错误事件。</summary>
	protected virtual void RaiseOnJoinError(){
		if (this.OnJoinError != null){
			this.OnJoinError();
		}
	}
    #endregion

    #region protected instance methods: Server Events
	/// <summary>触发比赛创建成功事件。</summary>
	/// <param name="match">创建的比赛信息。</param>
	protected virtual void RaiseOnMatchCreated(CreatedMatchInformation match){
		if (this.OnMatchCreated != null){
			this.OnMatchCreated(match);
		}
	}

	/// <summary>触发比赛创建错误事件。</summary>
	protected virtual void RaiseOnMatchCreationError(){
		if (this.OnMatchCreationError != null){
			this.OnMatchCreationError();
		}
	}

	/// <summary>触发比赛销毁事件。</summary>
	protected virtual void RaiseOnMatchDestroyed(){
		if (this.OnMatchDestroyed != null){
			this.OnMatchDestroyed();
		}
	}

	/// <summary>触发玩家连接比赛事件。</summary>
	/// <param name="player">玩家信息。</param>
	protected virtual void RaiseOnPlayerConnectedToMatch(PlayerInformation player){
		if (this.OnPlayerConnectedToMatch != null){
			this.OnPlayerConnectedToMatch(player);
		}
	}

	/// <summary>触发玩家离开比赛事件。</summary>
	/// <param name="player">玩家信息。</param>
	protected virtual void RaiseOnPlayerDisconnectedFromMatch(PlayerInformation player){
		if (this.OnPlayerDisconnectedFromMatch != null){
			this.OnPlayerDisconnectedFromMatch(player);
		}
	}
    #endregion
}
