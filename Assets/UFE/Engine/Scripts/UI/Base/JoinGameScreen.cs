using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.UI;

/// <summary>
/// 加入游戏界面（JoinGameScreen）。
/// <para>用途：局域网联机模式下的"搜索并加入房间"界面基类——自动搜索比赛、维护找到的服务器列表、</para>
/// <para>依次尝试连接；无可用比赛或连接失败时跳转到连接丢失界面。</para>
/// </summary>
public class JoinGameScreen : UFEScreen {
	#region protected instance fields
	/// <summary>是否正在连接中。</summary>
	protected bool _connecting = false;
	/// <summary>已发现的服务器列表。</summary>
	protected IList<MultiplayerAPI.MatchInformation> _foundServers = new List<MultiplayerAPI.MatchInformation>();
	#endregion

	#region public override methods
	/// <summary>
	/// 界面显示时停止搜索 LAN 游戏。
	/// </summary>
	public override void OnShow(){
		base.OnShow ();
		this.StopSearchingLanGames();
	}
	#endregion

	#region public instance methods
	/// <summary>
	/// 返回网络游戏界面（停止搜索）。
	/// </summary>
	public virtual void GoToNetworkGameScreen(){
		this.StopSearchingLanGames();
		UFE.StartNetworkGameScreen();
	}

	/// <summary>
	/// 进入连接丢失界面（停止搜索）。
	/// </summary>
	public virtual void GoToConnectionLostScreen(){
		this.StopSearchingLanGames();
		UFE.StartConnectionLostScreen();
	}

	/// <summary>
	/// 刷新游戏列表（虚方法，默认空实现）。
	/// </summary>
	public virtual void RefreshGameList() {

	}

	/// <summary>
	/// 通过 UI 文本中的房间名加入游戏。
	/// </summary>
	/// <param name="textUI">包含房间名的 UI 文本。</param>
	public virtual void JoinGame(Text textUI) {
		this.StopSearchingLanGames();
		UFE.JoinGame(new MultiplayerAPI.MatchInformation(textUI.text, UFE.config.networkOptions.port));
	}

	/// <summary>
	/// 自动加入发现的第一个 LAN 游戏（注册发现/错误回调后开始搜索）。
	/// </summary>
	public virtual void JoinFirstLanGame(){
		UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

		UFE.multiplayerAPI.OnMatchesDiscovered += this.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError += this.OnMatchDiscoveryError;

		UFE.multiplayerAPI.StartSearchingMatches();
	}

	/// <summary>
	/// 停止搜索 LAN 游戏并清空发现列表。
	/// </summary>
	public virtual void StopSearchingLanGames(){
		UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

		UFE.multiplayerAPI.StopSearchingMatches();
		this._foundServers.Clear();
		this._connecting = false;
	}
	#endregion

	#region protected instance methods
	/// <summary>
	/// 加入成功回调：取消注册连接事件。
	/// </summary>
	/// <param name="match">加入的比赛信息。</param>
	protected virtual void OnJoined(MultiplayerAPI.JoinedMatchInformation match){
		UFE.multiplayerAPI.OnJoined -= this.OnJoined;
		UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;
	}

	/// <summary>
	/// 加入失败回调：尝试连接发现的下一场比赛。
	/// </summary>
	protected virtual void OnJoinError(){
		UFE.multiplayerAPI.OnJoined -= this.OnJoined;
		UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;

		// Try to connect to other found matches
		this._connecting = false;
		this.TryConnect();
	}

	/// <summary>
	/// 发现比赛回调：保存发现的比赛并尝试连接；无比赛则进入连接丢失界面。
	/// </summary>
	/// <param name="matches">发现的比赛列表。</param>
	protected virtual void OnMatchesDiscovered(ReadOnlyCollection<MultiplayerAPI.MatchInformation> matches){
		this.StopSearchingLanGames();

		if (matches != null && matches.Count > 0){
			for (int i = 0; i < matches.Count; ++i){
				if (matches[i] != null){
					this._foundServers.Add(matches[i]);
				}
			}

			this.TryConnect();
		}else{
			this.GoToConnectionLostScreen();
		}
	}

	/// <summary>
	/// 比赛发现错误回调：进入连接丢失界面。
	/// </summary>
	protected virtual void OnMatchDiscoveryError(){
		this.StopSearchingLanGames();
		this.GoToConnectionLostScreen();
	}

	/// <summary>
	/// 未找到 LAN 游戏回调：进入连接丢失界面。
	/// </summary>
	protected virtual void OnLanGameNotFound(){
		this.GoToConnectionLostScreen();
	}

	/// <summary>
	/// 尝试连接：若未连接且无连接请求，则从发现列表取出比赛并加入；列表为空则报告未找到游戏。
	/// </summary>
	protected virtual void TryConnect(){
		// First, we check that we aren't already connected to a client or a server...
		if (!UFE.multiplayerAPI.IsConnected() && !this._connecting){
			MultiplayerAPI.MatchInformation match = null;

			// After that, check if we have found one match with at least one player which isn't already full...
			while (match == null && this._foundServers.Count > 0){
				match = this._foundServers[0];
				this._foundServers.RemoveAt(0);
			}


			if (match != null){
				// In that case, try connecting to that match
				this._connecting = true;

				UFE.multiplayerAPI.OnJoined += this.OnJoined;
				UFE.multiplayerAPI.OnJoinError += this.OnJoinError;
				UFE.JoinGame(match);
			}else{
				// Otherwise, return a net a new match
				this.OnLanGameNotFound();
			}
		}
	}
	#endregion
}
