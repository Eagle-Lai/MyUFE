using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

/// <summary>
/// 空多人 API（NullMultiplayerAPI）。
/// <para>用途：未安装网络插件时使用的"空实现"——所有网络操作均失败并触发对应的错误事件，连接状态恒为断开。</para>
/// <para>保证 UFE 引擎在无网络插件时也能正常编译与运行（网络功能不可用）。</para>
/// </summary>
public class NullMultiplayerAPI : MultiplayerAPI{
	#region public override properties
	/// <summary>连接数（恒为 0）。</summary>
	public override int Connections{
		get{
			return 0;
		}
	}

	/// <summary>当前玩家信息（恒为空）。</summary>
	public override MultiplayerAPI.PlayerInformation Player{
		get{
			return new MultiplayerAPI.PlayerInformation(null);
		}
	}

	/// <summary>发送速率（读写无效果）。</summary>
	public override float SendRate{get; set;}
	#endregion

	#region public override methods
	// Client
	/// <summary>断开比赛（触发断开事件）。</summary>
	public override void DisconnectFromMatch(){
		this.RaiseOnDisconnection();
	}

	/// <summary>加入比赛（触发加入错误事件）。</summary>
	public override void JoinMatch(MatchInformation match, string password = null){
		this.RaiseOnJoinError();
	}

	/// <summary>随机加入比赛（触发加入错误事件）。</summary>
	public override void JoinRandomMatch(){
		this.RaiseOnJoinError();
	}

	/// <summary>开始搜索比赛（触发搜索错误事件）。</summary>
	public override void StartSearchingMatches(int startPage = 0, int pageSize = 20, string filter = null){
		this.RaiseOnMatchDiscoveryError();
	}

	/// <summary>停止搜索比赛（空实现）。</summary>
	public override void StopSearchingMatches(){}

	// Common
	/// <summary>获取连接状态（恒为断开）。</summary>
	public override NetworkState GetConnectionState(){
		return NetworkState.Disconnected;
	}

	/// <summary>获取最后 Ping 值（恒为 0）。</summary>
	public override int GetLastPing(){
		return 0;
	}

	// Server
	/// <summary>创建比赛（触发创建错误事件）。</summary>
	public override void CreateMatch(MatchCreationRequest request){
		this.RaiseOnMatchCreationError();
	}

	/// <summary>销毁比赛（触发销毁事件）。</summary>
	public override void DestroyMatch(){
		this.RaiseOnMatchDestroyed();
	}
	#endregion

	#region protected override methods
	/// <summary>发送网络消息（恒失败）。</summary>
	/// <param name="bytes">消息字节。</param>
	/// <returns>恒 false。</returns>
	protected override bool SendNetworkMessage(byte[] bytes){
		return false;
	}
	#endregion
}
