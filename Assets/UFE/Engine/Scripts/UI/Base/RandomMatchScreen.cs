using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.UI;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

/// <summary>
/// 随机匹配界面（RandomMatchScreen）。
/// <para>用途：在线快速匹配界面基类——按页搜索在线比赛，优先加入未满的比赛，无可用比赛时自动创建新比赛。</para>
/// <para>匹配失败/无比赛时跳转到连接丢失界面。</para>
/// </summary>
public class RandomMatchScreen : UFEScreen {
	#region public instance fields
	/// <summary>每页比赛数量。</summary>
	public int pageSize = 20;
	#endregion

	#region protected instance field
	/// <summary>是否正在连接中。</summary>
	protected bool _connecting = false;
	/// <summary>当前搜索页索引。</summary>
	protected int _currentPage = 0;
	/// <summary>已发现的比赛列表。</summary>
	protected IList<MultiplayerAPI.MatchInformation> _foundMatches = new List<MultiplayerAPI.MatchInformation>();
	#endregion

	#region public override methods
	/// <summary>
	/// 界面显示时：停止旧搜索、切换到在线模式并开始加入或创建比赛。
	/// </summary>
	public override void OnShow(){
		base.OnShow ();

		this.StopSearchingMatchGames();

        UFE.multiplayerMode = UFE.MultiplayerMode.Online;
        this._currentPage = 0;
        this.JoinOrCreateMatchGame();
	}
	#endregion

	#region public instance methods
	/// <summary>
	/// 返回主菜单（停止搜索）。
	/// </summary>
	public virtual void GoToMainMenuScreen(){
		this.StopSearchingMatchGames();
		UFE.StartMainMenuScreen();
	}

	/// <summary>
	/// 进入连接丢失界面（停止搜索）。
	/// </summary>
	public virtual void GoToConnectionLostScreen(){
		this.StopSearchingMatchGames();
		UFE.StartConnectionLostScreen();
	}

	/// <summary>
	/// 开始搜索在线比赛（注册发现/错误回调）。
	/// </summary>
	public virtual void JoinOrCreateMatchGame()
    {
        this._connecting = true;
        UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
		UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;

		UFE.multiplayerAPI.OnMatchesDiscovered += this.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError += this.OnMatchDiscoveryError;
        
        UFE.multiplayerAPI.StartSearchingMatches(this._currentPage, this.pageSize, null);
    }

	/// <summary>
	/// 停止搜索在线比赛并清空发现列表。
	/// </summary>
	public virtual void StopSearchingMatchGames(){
		UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

		this._foundMatches.Clear();
		this._connecting = false;
	}
	#endregion

	#region protected instance methods
	/// <summary>
	/// 比赛创建成功回调：取消注册并停止搜索。
	/// </summary>
	/// <param name="match">创建的比赛信息。</param>
	protected virtual void OnMatchCreated(MultiplayerAPI.CreatedMatchInformation match){
		UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
		UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;

		this.StopSearchingMatchGames();
	}

	/// <summary>
	/// 比赛创建失败回调：进入连接丢失界面。
	/// </summary>
	protected virtual void OnMatchCreationError(){
		UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
		UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;

		this.GoToConnectionLostScreen();
	}

	/// <summary>
	/// 发现比赛回调：保存发现列表并尝试连接。
	/// </summary>
	/// <param name="matches">发现的比赛列表。</param>
	protected virtual void OnMatchesDiscovered(ReadOnlyCollection<MultiplayerAPI.MatchInformation> matches) {
		UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

		if (matches != null){
			for (int i = 0; i < matches.Count; ++i){
				if (matches[i] != null){
					this._foundMatches.Add(matches[i]);

					//					if (matches[i].directConnectInfos != null){
					//						for (int j = 0; j < matches[i].directConnectInfos.Count; ++j){
					//							MatchDirectConnectInfo connectionInfo = matches[i].directConnectInfos[j];
					//
					//							if (connectionInfo != null){
					//								Debug.Log(connectionInfo.privateAddress + "\n" + connectionInfo.publicAddress);
					//							}
					//						}
					//					}
				}
			}
		}

		this.TryConnect();
	}

	/// <summary>
	/// 比赛发现错误回调：进入连接丢失界面。
	/// </summary>
	protected virtual void OnMatchDiscoveryError() {
		UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

		this.GoToConnectionLostScreen();
	}

	/// <summary>
	/// 加入成功回调：切换在线模式并停止搜索。
	/// </summary>
	/// <param name="match">加入的比赛信息。</param>
	protected virtual void OnJoined(MultiplayerAPI.JoinedMatchInformation match){
		UFE.multiplayerAPI.OnJoined -= this.OnJoined;
		UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;

		UFE.multiplayerMode = UFE.MultiplayerMode.Online;
		this.StopSearchingMatchGames();
	}

	/// <summary>
	/// 加入失败回调：尝试连接其他发现的比赛。
	/// </summary>
	protected virtual void OnJoinError(){
		UFE.multiplayerAPI.OnJoined -= this.OnJoined;
		UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;

		// Try to connect to other found matches
		this._connecting = false;
		this.TryConnect();
	}

	/// <summary>
	/// 未找到 LAN 游戏回调：进入连接丢失界面。
	/// </summary>
	protected virtual void OnLanGameNotFound(){
		this.GoToConnectionLostScreen();
	}

	/// <summary>
	/// 尝试连接：依次尝试加入未满的比赛，无可用比赛则创建新比赛。
	/// </summary>
	protected virtual void TryConnect(){
		// First, we check that we aren't already connected to a client or a server...
		if (!UFE.multiplayerAPI.IsConnected() && !this._connecting){
			MultiplayerAPI.MatchInformation match = null;

			// After that, check if we have found one match with at least one player which isn't already full...
			while(
				this._foundMatches.Count > 0 && 
				(match == null || match.currentPlayers == 0 || match.currentPlayers >= match.maxPlayers)
			){
				match = this._foundMatches[0];
				this._foundMatches.RemoveAt(0);

				if (match != null && match.currentPlayers > 0 && match.currentPlayers < match.maxPlayers){
					// In that case, try connecting to that match
					this._connecting = true;

					UFE.multiplayerAPI.OnJoined += this.OnJoined;
					UFE.multiplayerAPI.OnJoinError += this.OnJoinError;
					UFE.multiplayerAPI.JoinMatch(match);

					return;
				}
			}





			// Otherwise, create a new match
			this._connecting = true;
			UFE.multiplayerAPI.OnMatchCreated += this.OnMatchCreated;
			UFE.multiplayerAPI.OnMatchCreationError += this.OnMatchCreationError;
			UFE.multiplayerAPI.CreateMatch(new MultiplayerAPI.MatchCreationRequest());

		}
	}
	#endregion
}
