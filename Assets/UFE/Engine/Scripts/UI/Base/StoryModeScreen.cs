using UnityEngine;
using System;

/// <summary>
/// 故事模式演出界面（StoryModeScreen）。
/// <para>用途：故事模式的开场/结尾/对话演出界面基类，通过 nextScreenAction 回调在演出结束后进入下一步。</para>
/// </summary>
public class StoryModeScreen : UFEScreen {
	#region public instance properties
	/// <summary>下一步动作回调（演出结束后执行）。</summary>
	public Action nextScreenAction{get; set;}
	#endregion


	#region public instance methods
	/// <summary>
	/// 前往下一个界面：执行配置的下一步回调。
	/// </summary>
	public virtual void GoToNextScreen(){
		if (this.nextScreenAction != null){
			this.nextScreenAction();
		}
	}
	#endregion
}
