using UnityEngine;
using System;
using System.Reflection;

/// <summary>
/// 对战结算界面（VersusModeAfterBattleScreen）。
/// <para>用途：对战结束后展示胜负并提供"再战/返回选人/换场地/返回主菜单"选项的界面基类，</para>
/// <para>支持本地即时切换与网络对战下的帧同步选项选择（通过 FluxCapacitor.RequestOptionSelection 同步）。</para>
/// </summary>
public class VersusModeAfterBattleScreen : UFEScreen {
	#region protected enum definitions
	/// <summary>结算界面可选操作。</summary>
	protected enum Option{
		/// <summary>再战。</summary>
		RepeatBattle = 0,
		/// <summary>返回角色选择。</summary>
		CharacterSelectionScreen = 1,
		/// <summary>进入场地选择。</summary>
		StageSelectionScreen = 2,
		/// <summary>返回主菜单。</summary>
		MainMenu = 3,
	}
	#endregion

	#region public instance methods
	/// <summary>
	/// 返回角色选择界面。
	/// </summary>
	public virtual void GoToCharacterSelectionScreen(){
		this.TrySelectOption((int)VersusModeAfterBattleScreen.Option.CharacterSelectionScreen, UFE.GetLocalPlayer());
	}

	/// <summary>
	/// 返回主菜单。
	/// </summary>
	public virtual void GoToMainMenu(){
		this.TrySelectOption((int)VersusModeAfterBattleScreen.Option.MainMenu, UFE.GetLocalPlayer());
	}

	/// <summary>
	/// 进入场地选择界面。
	/// </summary>
	public virtual void GoToStageSelectionScreen(){
		this.TrySelectOption((int)VersusModeAfterBattleScreen.Option.StageSelectionScreen, UFE.GetLocalPlayer());
	}

	/// <summary>
	/// 再战一次。
	/// </summary>
	public virtual void RepeatBattle(){
		this.TrySelectOption((int)VersusModeAfterBattleScreen.Option.RepeatBattle, UFE.GetLocalPlayer());
	}
	#endregion

	#region public override methods
	/// <summary>
	/// 处理菜单选项选择：按选项跳转到对应界面。
	/// </summary>
	/// <param name="option">选项索引。</param>
	/// <param name="player">操作玩家。</param>
	public override void SelectOption(int option, int player){
		VersusModeAfterBattleScreen.Option selectedOption = (VersusModeAfterBattleScreen.Option)option;
		if (selectedOption == VersusModeAfterBattleScreen.Option.CharacterSelectionScreen){
			UFE.StartCharacterSelectionScreen();
		}else if (selectedOption == VersusModeAfterBattleScreen.Option.MainMenu){
			UFE.StartMainMenuScreen();
		}else if (selectedOption == VersusModeAfterBattleScreen.Option.StageSelectionScreen){
			UFE.StartStageSelectionScreen();
		}else if (selectedOption == VersusModeAfterBattleScreen.Option.RepeatBattle){
			UFE.StartLoadingBattleScreen();
		}
	}
	#endregion

	#region protected virtual methods
	/// <summary>
	/// 尝试选择选项：本地游戏立即执行；网络对战由本机玩家通过帧同步请求同步给对手。
	/// </summary>
	/// <param name="option">选项索引。</param>
	/// <param name="player">操作玩家。</param>
	protected virtual void TrySelectOption(int option, int player){
		// Check if he was playing online or not...
		if (!UFE.isConnected){
			// If it's a local game, go to the selected screen immediately...
			this.SelectOption(option, player);
		}else{
			// If it's an online game, we need to inform the other client about the screen we want to go...
			int localPlayer = UFE.GetLocalPlayer();
			if (localPlayer == player){
				// We don't invoke the SelectOption() method immediately because we are using the frame-delay 
				// algorithm to keep players synchronized, so we can't invoke the SelectOption() method
				// until the other player has received the message with our choice.
				UFE.fluxCapacitor.RequestOptionSelection(player, (sbyte)option);
			}
		}
	}
	#endregion
}
