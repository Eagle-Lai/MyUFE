using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 默认对战模式界面（DefaultVersusModeScreen）。
/// <para>用途：对战模式选择界面——提供默认导航系统（取消返回主菜单）与音乐/音效播放。</para>
/// </summary>
public class DefaultVersusModeScreen : VersusModeScreen{
	#region public instance properties
	/// <summary>加载音效。</summary>
	public AudioClip onLoadSound;
	/// <summary>背景音乐。</summary>
	public AudioClip music;
	/// <summary>选择音效。</summary>
	public AudioClip selectSound;
	/// <summary>取消音效。</summary>
	public AudioClip cancelSound;
	/// <summary>移动光标音效。</summary>
	public AudioClip moveCursorSound;
	/// <summary>加载时是否停止之前的音效。</summary>
	public bool stopPreviousSoundEffectsOnLoad = false;
	/// <summary>延迟播放音乐的时间。</summary>
	public float delayBeforePlayingMusic = 0.1f;
	#endregion

	#region public override methods
	/// <summary>
	/// 固定帧更新：调用默认导航系统（取消返回主菜单）。
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
			this.cancelSound,
			this.GoToMainMenu
		);
	}

	/// <summary>
	/// 屏幕显示时：高亮首个物体并播放音乐/音效。
	/// </summary>
	public override void OnShow (){
		base.OnShow ();
		this.HighlightOption(this.FindFirstSelectable());
		
		if (this.music != null){
			UFE.DelayLocalAction(delegate(){UFE.PlayMusic(this.music);}, this.delayBeforePlayingMusic);
		}

		if (this.stopPreviousSoundEffectsOnLoad){
			UFE.StopSounds();
		}
		
		if (this.onLoadSound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.onLoadSound);}, this.delayBeforePlayingMusic);
		}
	}
	#endregion
}
