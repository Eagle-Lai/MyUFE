using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 默认暂停界面（DefaultPauseScreen）。
/// <para>用途：暂停菜单界面——管理多个子屏幕（Tab）切换与"返回主菜单"确认对话框的显示/隐藏，</para>
/// <para>将输入与选项转发到当前活动的子屏幕。</para>
/// </summary>
public class DefaultPauseScreen : PauseScreen{
	#region public instance fields
	/// <summary>返回主菜单确认对话框（弹窗屏幕）。</summary>
	public UFEScreen backToMenuConfirmationDialog;
	/// <summary>暂停菜单包含的子屏幕列表（Tab）。</summary>
	public UFEScreen[] screens;
	#endregion

	#region protected instance fields
	/// <summary>当前活动子屏幕索引。</summary>
	protected int currentScreen;
	/// <summary>确认对话框是否可见。</summary>
	protected bool confirmationDialogVisible = false;
	#endregion

	#region public instance methods
	/// <summary>
	/// 隐藏返回主菜单确认对话框（触发当前屏幕的 OnShow 事件）。
	/// </summary>
	public virtual void HideBackToMenuConfirmationDialog(){
		this.HideBackToMenuConfirmationDialog(true);
	}

	/// <summary>
	/// 隐藏确认对话框：恢复子屏幕交互性并（可选）重新触发当前屏幕的 OnShow。
	/// </summary>
	/// <param name="triggerOnShowScreenEvent">是否重新显示当前子屏幕。</param>
	public virtual void HideBackToMenuConfirmationDialog(bool triggerOnShowScreenEvent){
		if (this.backToMenuConfirmationDialog != null){
			for (int i = 0; i < this.screens.Length; ++i){
				if (this.screens[i] != null){
					CanvasGroup canvasGroup = this.screens[i].GetComponent<CanvasGroup>();
					
					if (canvasGroup != null){
						canvasGroup.interactable = true;
					}
				}
			}

			this.HideScreen(this.backToMenuConfirmationDialog);
			this.confirmationDialogVisible = false;

			if (triggerOnShowScreenEvent){
				this.ShowScreen(this.screens[this.currentScreen]);
			}
		}
	}

	/// <summary>
	/// 切换到指定索引的子屏幕（其余子屏幕隐藏）。
	/// </summary>
	/// <param name="index">目标子屏幕索引。</param>
	public virtual void GoToScreen(int index){
		for (int i = 0; i < this.screens.Length; ++i){
			if (i != index){
				this.HideScreen(this.screens[i]);
			}else{
				this.ShowScreen(this.screens[i]);
			}
		}

		this.currentScreen = index;
	}

	/// <summary>
	/// 显示返回主菜单确认对话框（禁用子屏幕交互）。
	/// </summary>
	public virtual void ShowBackToMenuConfirmationDialog(){
		if (this.backToMenuConfirmationDialog != null){
			for (int i = 0; i < this.screens.Length; ++i){
				if (this.screens[i] != null){
					CanvasGroup canvasGroup = this.screens[i].GetComponent<CanvasGroup>();
					
					if (canvasGroup != null){
						canvasGroup.interactable = false;
					}else{
						this.HideScreen(this.screens[i]);
					}
				}
			}

			this.ShowScreen(this.backToMenuConfirmationDialog);
			this.confirmationDialogVisible = true;
		}
	}
	#endregion

	#region public override methods
	/// <summary>
	/// 固定帧更新：将输入转发给当前活动对象（确认对话框或当前子屏幕）。
	/// </summary>
	public override void DoFixedUpdate(
		IDictionary<InputReferences, InputEvents> player1PreviousInputs,
		IDictionary<InputReferences, InputEvents> player1CurrentInputs,
		IDictionary<InputReferences, InputEvents> player2PreviousInputs,
		IDictionary<InputReferences, InputEvents> player2CurrentInputs
	){
		base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);

		if (this.confirmationDialogVisible){
			if (this.backToMenuConfirmationDialog != null){
				this.backToMenuConfirmationDialog.DoFixedUpdate(
					player1PreviousInputs,
					player1CurrentInputs,
					player2PreviousInputs,
					player2CurrentInputs
				);
			}
		}else{
			if(this.currentScreen >= 0 && this.currentScreen < this.screens.Length && this.screens[this.currentScreen] != null){
				this.screens[this.currentScreen].DoFixedUpdate(
					player1PreviousInputs,
					player1CurrentInputs,
					player2PreviousInputs,
					player2CurrentInputs
				);
			}
		}
	}

	/// <summary>
	/// 暂停界面隐藏时：隐藏确认对话框与当前子屏幕。
	/// </summary>
	public override void OnHide (){
		this.confirmationDialogVisible = false;
		this.HideBackToMenuConfirmationDialog(false);
		if (this.currentScreen >= 0 && this.currentScreen < this.screens.Length){
			this.HideScreen(this.screens[this.currentScreen]);
		}
		base.OnHide ();
	}

	/// <summary>
	/// 暂停界面显示时：重置确认对话框并进入第一个子屏幕。
	/// </summary>
	public override void OnShow (){
		base.OnShow ();

		this.confirmationDialogVisible = false;
		this.HideBackToMenuConfirmationDialog(false);
		if (this.screens.Length > 0){
			this.GoToScreen(0);
		}
	}

	/// <summary>
	/// 处理菜单选项：转发给当前活动子屏幕。
	/// </summary>
	/// <param name="option">选项索引。</param>
	/// <param name="player">操作玩家。</param>
	public override void SelectOption(int option, int player){
		// TODO: select the correct option manually.
		if(this.currentScreen >= 0 && this.currentScreen < this.screens.Length && this.screens[this.currentScreen] != null){
			this.screens[this.currentScreen].SelectOption(option, player);
		}else{

		}
	}
	#endregion

	#region protected instance methods
	/// <summary>
	/// 隐藏屏幕（触发 OnHide 并停用 GameObject）。
	/// </summary>
	/// <param name="screen">要隐藏的屏幕。</param>
	protected virtual void HideScreen(UFEScreen screen){
		if (screen != null){
			screen.OnHide();
			screen.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// 判断屏幕是否可见。
	/// </summary>
	/// <param name="screen">目标屏幕。</param>
	/// <returns>可见返回 true。</returns>
	protected virtual bool IsVisible(UFEScreen screen){
		return screen != null ? screen.IsVisible() : false;
	}
	
	/// <summary>
	/// 显示屏幕（激活 GameObject 并触发 OnShow）。
	/// </summary>
	/// <param name="screen">要显示的屏幕。</param>
	protected virtual void ShowScreen(UFEScreen screen){
		if (screen != null){
			screen.gameObject.SetActive(true);
			screen.OnShow();
		}
	}
	#endregion
}
