using UnityEngine;
using System.Collections;

/// <summary>
/// 暂停界面（PauseScreen）。
/// <para>用途：游戏暂停菜单基类，提供恢复游戏与返回主菜单的方法。</para>
/// </summary>
public class PauseScreen : UFEScreen {
	/// <summary>
	/// 恢复游戏并返回主菜单。
	/// </summary>
	public virtual void GoToMainMenu(){
        UFE.PauseGame(false);
		UFE.StartMainMenuScreen();
	}

	/// <summary>
	/// 恢复游戏。
	/// </summary>
	public virtual void ResumeGame(){
		UFE.PauseGame(false);
	}
}
