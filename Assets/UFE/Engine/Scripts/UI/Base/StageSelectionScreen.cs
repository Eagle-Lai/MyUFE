using UnityEngine;
using System;
using System.Reflection;

/// <summary>
/// 场地选择界面（StageSelectionScreen）。
/// <para>用途：对战/故事模式的场地选择界面基类——维护悬停索引，处理场地确认/取消选择，</para>
/// <para>支持本地即时选择与网络对战下的帧同步选择（通过 FluxCapacitor.RequestOptionSelection 同步给对方）。</para>
/// </summary>
public class StageSelectionScreen : UFEScreen{
	#region public instance properties
	/// <summary>选择场地时播放的音效。</summary>
	public AudioClip selectSound;
	/// <summary>取消选择时播放的音效。</summary>
	public AudioClip cancelSound;
	/// <summary>进入加载界面之前是否播放淡出动画。</summary>
	public bool fadeBeforeGoingToLoadingBattleScreen = false;
	#endregion

	#region protected instance fields
	/// <summary>界面是否正在关闭。</summary>
	protected bool closing = false;
	/// <summary>当前悬停的场地索引。</summary>
	protected int stageHoverIndex = 0;
	#endregion

	#region public instance methods
	/// <summary>
	/// 返回角色选择界面（网络对战下清除已选场地并请求返回；本地对战直接进入角色选择）。
	/// </summary>
	public virtual void GoToCharacterSelectionScreen(){
		if (UFE.gameMode == GameMode.NetworkGame){
			UFE.config.selectedStage = null;
			this.TrySelectStage(-1);
		}else{
			this.StartLoadingCharacterSelectionScreen();
		}
	}

	/// <summary>
	/// 进入战斗加载界面。
	/// </summary>
	public virtual void GoToLoadingBattleScreen(){
		this.StartLoadingBattleScreen();
	}

	/// <summary>
	/// 设置悬停的场地索引（仅在未关闭且索引有效时更新）。
	/// </summary>
	/// <param name="stageIndex">场地索引。</param>
	public virtual void SetHoverIndex(int stageIndex){
		if (!this.closing && stageIndex >= 0 && stageIndex < UFE.config.stages.Length){
			this.stageHoverIndex = stageIndex;
		}
	}

	/// <summary>
	/// 场地选择确认回调：有效索引则选中场地并进入加载；负索引表示取消/返回。
	/// </summary>
	/// <param name="stageIndex">场地索引。</param>
	public void OnStageSelectionAllowed(int stageIndex){
		if (!this.closing){
			if (stageIndex >= 0 && stageIndex < UFE.config.stages.Length){
				if (this.selectSound != null)UFE.PlaySound(this.selectSound);
				this.SetHoverIndex(stageIndex);

				UFE.config.selectedStage = UFE.config.stages[stageIndex];
				this.StartLoadingBattleScreen();
			}else if (stageIndex < 0){
				if (UFE.config.selectedStage != null){
					if (this.cancelSound != null) UFE.PlaySound(this.cancelSound);
					UFE.config.selectedStage = null;
				}else{
					if (this.cancelSound != null) UFE.PlaySound(this.cancelSound);
					this.StartLoadingCharacterSelectionScreen();
				}
			}
		}
	}

	/// <summary>
	/// 尝试取消场地选择。
	/// </summary>
	public void TryDeselectStage(){
		this.TrySelectStage(-1);
	}

	/// <summary>
	/// 尝试确认当前悬停的场地。
	/// </summary>
	public void TrySelectStage(){
		this.TrySelectStage(this.stageHoverIndex);
	}

	/// <summary>
	/// 尝试选择指定场地：本地游戏立即生效；网络对战仅由玩家1请求选择（帧同步同步给对手）。
	/// </summary>
	/// <param name="stageIndex">场地索引。</param>
	public void TrySelectStage(int stageIndex){
		// Check if he was playing online or not...
		if (!UFE.isConnected){
			// If it's a local game, update the corresponding stage immediately...
			this.OnStageSelectionAllowed(stageIndex);
		}else{
			// If it's an online game, we only select the stage if it has been requested by Player 1...
			// But if player 2 wants to come back to character selection screen, we also allow that...
			int localPlayer = UFE.GetLocalPlayer();
			if (localPlayer == 1 || stageIndex < 0){
				// We don't invoke the OnstageSelected() method immediately because we are using the frame-delay 
				// algorithm to keep players synchronized, so we can't invoke the OnstageSelected() method
				// until the other player has received the message with our choice.
				UFE.fluxCapacitor.RequestOptionSelection(localPlayer, (sbyte)stageIndex);
			}
		}
	}
	#endregion

	#region public override methods
	/// <summary>
	/// 界面显示时清空已选场地并复位关闭标志。
	/// </summary>
	public override void OnShow (){
		UFE.config.selectedStage = null;
		this.closing = false;
	}

	/// <summary>
	/// 处理菜单选项选择（确认场地）。
	/// </summary>
	/// <param name="option">场地索引。</param>
	/// <param name="player">操作玩家。</param>
	public override void SelectOption (int option, int player){
		this.OnStageSelectionAllowed(option);
	}
	#endregion

	#region protected instance method
	/// <summary>
	/// 进入角色选择界面（标记关闭）。
	/// </summary>
	protected virtual void StartLoadingCharacterSelectionScreen(){
		this.closing = true;
		UFE.StartCharacterSelectionScreen();
	}

	/// <summary>
	/// 进入战斗加载界面（按配置决定是否淡出）。
	/// </summary>
	protected virtual void StartLoadingBattleScreen(){
		this.closing = true;
		if (this.fadeBeforeGoingToLoadingBattleScreen){
			UFE.StartLoadingBattleScreen();
		}else{
			UFE.StartLoadingBattleScreen(0f);
		}
	}
	#endregion
}
