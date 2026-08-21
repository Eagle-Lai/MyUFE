using UnityEngine;
using System.Collections;

/// <summary>
/// 故事模式继续界面（StoryModeContinueScreen）。
/// <para>用途：故事模式中战斗失败后的"继续"选择界面基类，提供重打本场战斗或进入游戏结束界面的方法。</para>
/// </summary>
public class StoryModeContinueScreen : UFEScreen {
	/// <summary>
	/// 重打当前故事战斗。
	/// </summary>
	public virtual void RepeatBattle(){
		UFE.StartStoryModeBattle();
	}

	/// <summary>
	/// 进入故事模式游戏结束界面。
	/// </summary>
	public virtual void GoToGameOverScreen(){
		UFE.StartStoryModeGameOverScreen();
	}
}
