using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 默认故事模式继续界面（DefaultContinueScreen）。
/// <para>用途：故事模式战败后的"继续"界面——提供默认导航系统（重打/返回）与音乐/倒计时音效播放。</para>
/// </summary>
public class DefaultContinueScreen : StoryModeContinueScreen{
	#region public instance properties
	/// <summary>背景音乐。</summary>
	public AudioClip music;
	/// <summary>倒计时音效。</summary>
	public AudioClip countdownSound;
	/// <summary>选择音效。</summary>
	public AudioClip selectSound;
	/// <summary>取消音效。</summary>
	public AudioClip cancelSound;
	/// <summary>移动光标音效。</summary>
	public AudioClip moveCursorSound;
	/// <summary>延迟播放音乐的时间。</summary>
	public float delayBeforePlayingMusic = 0.1f;
	#endregion

	#region public override methods
	/// <summary>
	/// 固定帧更新：调用默认导航系统。
	/// </summary>
	public override void DoFixedUpdate(
		IDictionary<InputReferences, InputEvents> player1PreviousInputs,
		IDictionary<InputReferences, InputEvents> player1CurrentInputs,
		IDictionary<InputReferences, InputEvents> player2PreviousInputs,
		IDictionary<InputReferences, InputEvents> player2CurrentInputs
	){
		base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);

		this.DefaultNavigationSystem(
			player1PreviousInputs,
			player1CurrentInputs,
			player2PreviousInputs,
			player2CurrentInputs,
			this.moveCursorSound,
			this.selectSound,
			this.cancelSound
		);
	}

	/// <summary>
	/// 屏幕显示时：高亮首个物体并播放音乐/倒计时音效。
	/// </summary>
	public override void OnShow (){
		base.OnShow ();
		this.HighlightOption(this.FindFirstSelectable());

		if (this.music != null){
			UFE.DelayLocalAction(delegate(){UFE.PlayMusic(this.music);}, this.delayBeforePlayingMusic);
		}
		
		if (this.countdownSound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.countdownSound);}, this.delayBeforePlayingMusic);
		}
	}
	#endregion
}
