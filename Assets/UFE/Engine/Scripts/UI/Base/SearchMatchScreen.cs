using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.UI;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

/// <summary>
/// 搜索比赛界面（SearchMatchScreen）。
/// <para>用途：在线比赛搜索界面基类——按页搜索并去重收集比赛，允许重复搜索直至找到可用比赛或达到搜索次数上限，</para>
/// <para>优先加入未满比赛，无可用比赛时创建新比赛；搜索失败/对手断开时进入连接丢失界面。</para>
/// </summary>
public class SearchMatchScreen : UFEScreen {
	#region public instance fields
	/// <summary>每页比赛数量。</summary>
	public int pageSize = 20;
	/// <summary>搜索延迟（帧数）。</summary>
	public int searchDelay = 180;
	/// <summary>最大搜索次数。</summary>
	public int maxSearchTimes = 3;
	/// <summary>使用的多人模式。</summary>
	public UFE.MultiplayerMode multiplayerMode;
	#endregion

	#region protected instance field
	/// <summary>是否正在连接中。</summary>
	protected bool _connecting = false;
	/// <summary>当前搜索页索引。</summary>
	protected int _currentPage = 0;
	/// <summary>当前搜索次数。</summary>
	protected int _currentSearchTime = 0;
	/// <summary>已发现的比赛列表。</summary>
	protected IList<MultiplayerAPI.MatchInformation> _foundMatches = new List<MultiplayerAPI.MatchInformation>();
	/// <summary>已尝试过的比赛列表（去重用）。</summary>
    protected IList<MultiplayerAPI.MatchInformation> _triedMatches = new List<MultiplayerAPI.MatchInformation>();
	/// <summary>当前比赛信息。</summary>
    protected MultiplayerAPI.MatchInformation current_match = null;
    #endregion

    #region public override methods
	/// <summary>
	/// 界面显示时：设置多人模式并开始搜索比赛。
	/// </summary>
    public override void OnShow(){
		base.OnShow ();

		//this.StopSearchingMatchGames();

        UFE.multiplayerMode = multiplayerMode;
        this._currentPage = 0;
        this._currentSearchTime = 0;
        this.StartSearchingGames();
	}
	#endregion

	#region public instance methods
	/// <summary>
	/// 返回主菜单（停止搜索）。
	/// </summary>
	public virtual void GoToMainMenuScreen(){
		this.StopSearchingMatchGames();
        //UFE.EnsureNetworkDisconnection();
        UFE.StartMainMenuScreen();
	}

	/// <summary>
	/// 进入连接丢失界面（停止搜索）。
	/// </summary>
	public virtual void GoToConnectionLostScreen(){
		this.StopSearchingMatchGames();
        //UFE.EnsureNetworkDisconnection();
        UFE.StartConnectionLostScreen();
    }

	/// <summary>
	/// 开始搜索比赛（注册发现/错误回调）。
	/// </summary>
	public virtual void StartSearchingGames()
    {
        UFE.multiplayerAPI.OnMatchesDiscovered += this.OnMatchesDiscovered;
        UFE.multiplayerAPI.OnMatchDiscoveryError += this.OnMatchDiscoveryError;
        
        UFE.multiplayerAPI.StartSearchingMatches(this._currentPage, this.pageSize, null);
    }

	/// <summary>
	/// 停止搜索比赛并清空发现列表（可取消延迟搜索）。
	/// </summary>
	/// <param name="enforce">是否同时取消延迟的搜索动作。</param>
	public virtual void StopSearchingMatchGames(bool enforce = true)
    {
        this._connecting = false;
        UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

        if (enforce) UFE.FindAndRemoveDelayLocalAction(StartSearchingGames);
        UFE.multiplayerAPI.StopSearchingMatches();
        this._foundMatches.Clear();
	}
    #endregion

    #region protected instance methods
	/// <summary>
	/// 比赛创建成功回调：记录当前比赛并等待对手加入。
	/// </summary>
	/// <param name="match">创建的比赛信息。</param>
    protected virtual void OnMatchCreated(MultiplayerAPI.CreatedMatchInformation match){
		UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
		UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;
        UFE.multiplayerAPI.OnPlayerConnectedToMatch += this.OnPlayerConnectedToMatch;

        this.StopSearchingMatchGames();
        this.current_match = new MultiplayerAPI.MatchInformation(match);
        this._triedMatches.Add(this.current_match);
        
		if (UFE.config.networkOptions.networkService == NetworkService.Unity){
			if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Created: "+ match.unityNetworkId);
		}else{
			if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Created: "+ match.matchName);
		}
        if (UFE.config.debugOptions.connectionLog) Debug.Log("Waiting for opponent...");
    }

	/// <summary>
	/// 比赛创建失败回调（记录日志）。
	/// </summary>
	protected virtual void OnMatchCreationError(){
		UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
		UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;

		//this.GoToConnectionLostScreen();
        if (UFE.config.debugOptions.connectionLog) Debug.Log("OnMatchCreationError");
    }

	/// <summary>
	/// 发现比赛回调：去重收集比赛；有可用比赛或达到搜索上限时尝试连接，否则延迟再次搜索。
	/// </summary>
	/// <param name="matches">发现的比赛列表。</param>
    protected virtual void OnMatchesDiscovered(ReadOnlyCollection<MultiplayerAPI.MatchInformation> matches)
    {
        int unique = 0;
        if (matches != null)
        {
            for (int i = 0; i < matches.Count; ++i)
            {
                if (matches[i] != null)
                {
                    bool duplicate = false;
                    for (int f = 0; f < _foundMatches.Count; f++)
                    {
                        if (_foundMatches[f].unityNetworkId == matches[i].unityNetworkId)
                            duplicate = true;
                    }
                    for (int t = 0; t < _triedMatches.Count; t++)
                    {
                        if (_triedMatches[t].unityNetworkId == matches[i].unityNetworkId)
                            duplicate = true;
                    }

                    if (UFE.config.networkOptions.networkService == NetworkService.Photon)
                    {
                        duplicate = false;
                    }


                    if (duplicate)
                    {
                        if (UFE.config.networkOptions.networkService == NetworkService.Unity)
                        {
                            if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Found: " + matches[i].unityNetworkId + " [duplicate]");
                        }
                        else
                        {
                            if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Found: " + matches[i].matchName + " [duplicate]");
                        }
                    }
                    else
                    {
                        if (UFE.config.networkOptions.networkService == NetworkService.Unity)
                        {
                            if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Found: " + matches[i].unityNetworkId);
                        }
                        else
                        {
                            if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Found: " + matches[i].matchName);
                        }

                        this._foundMatches.Add(matches[i]);
                        unique++;
                    }
                }
            }
            if (UFE.config.debugOptions.connectionLog) Debug.Log("Matches Found (available/total): " + unique + "/" + matches.Count);
        }

        if (unique > 0 || _currentSearchTime >= maxSearchTimes)
        {
            this.TryConnect();
        }
        else
        {
            UFE.DelayLocalAction(StartSearchingGames, searchDelay);
            _currentSearchTime++;
        }
        this.StopSearchingMatchGames(false);
    }

	/// <summary>
	/// 比赛发现错误回调：进入连接丢失界面。
	/// </summary>
	protected virtual void OnMatchDiscoveryError() {
		UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

		this.GoToConnectionLostScreen();
        if (UFE.config.debugOptions.connectionLog) Debug.Log("OnMatchDiscoveryError");
    }

	/// <summary>
	/// 加入成功回调：切换在线模式并启动网络对战。
	/// </summary>
	/// <param name="match">加入的比赛信息。</param>
	protected virtual void OnJoined(MultiplayerAPI.JoinedMatchInformation match){
		UFE.multiplayerAPI.OnJoined -= this.OnJoined;
		UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;

		UFE.multiplayerMode = UFE.MultiplayerMode.Online;
		this.StopSearchingMatchGames();

        if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Starting...");
        UFE.StartNetworkGame(0.5f, 2, false);
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
	/// 玩家加入比赛回调：作为主机启动网络对战（本地玩家为玩家1）。
	/// </summary>
	/// <param name="player">加入的玩家信息。</param>
    protected virtual void OnPlayerConnectedToMatch(MultiplayerAPI.PlayerInformation player)
    {
        UFE.multiplayerAPI.OnPlayerConnectedToMatch -= this.OnPlayerConnectedToMatch;
        UFE.multiplayerAPI.OnPlayerDisconnectedFromMatch += this.OnPlayerDisconnectedFromMatch;

        if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Starting...");
        UFE.StartNetworkGame(0.5f, 1, false);
    }

	/// <summary>
	/// 玩家离开比赛回调：进入连接丢失界面。
	/// </summary>
	/// <param name="player">离开的玩家信息。</param>
    protected virtual void OnPlayerDisconnectedFromMatch(MultiplayerAPI.PlayerInformation player)
    {
        UFE.multiplayerAPI.OnPlayerDisconnectedFromMatch -= this.OnPlayerDisconnectedFromMatch;

        this.GoToConnectionLostScreen();
        if (UFE.config.debugOptions.connectionLog) Debug.Log("OnPlayerDisconnectedFromMatch");
    }

	/// <summary>
	/// 尝试连接：依次尝试加入未满的比赛，无可用比赛则创建新比赛。
	/// </summary>
    protected virtual void TryConnect(){
		// First, we check that we aren't already connected to a client or a server...
		if (!UFE.multiplayerAPI.IsConnected() && !this._connecting){

            if (UFE.config.debugOptions.connectionLog) Debug.Log("Connecting...");
            MultiplayerAPI.MatchInformation match = null;

			// After that, check if we have found one match with at least one player which isn't already full...
			while(this._foundMatches.Count > 0){
				match = this._foundMatches[0];
				this._foundMatches.RemoveAt(0);
                this._triedMatches.Add(match);

                if (match != null && match.currentPlayers > 0 && match.currentPlayers < match.maxPlayers){
					// In that case, try connecting to that match
					this._connecting = true;
                    
					UFE.multiplayerAPI.OnJoined += this.OnJoined;
					UFE.multiplayerAPI.OnJoinError += this.OnJoinError;
                    if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Found! Joining Match...");
                    UFE.multiplayerAPI.JoinMatch(match);

					return;
				}
			}

			// Otherwise, create a new match
			this._connecting = true;
            UFE.multiplayerAPI.OnMatchCreated += this.OnMatchCreated;
			UFE.multiplayerAPI.OnMatchCreationError += this.OnMatchCreationError;
			UFE.multiplayerAPI.CreateMatch(new MultiplayerAPI.MatchCreationRequest());

            if (UFE.config.debugOptions.connectionLog) Debug.Log("No Matches Found. Creating Match...");

        }
	}
	#endregion
}
