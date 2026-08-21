using UnityEngine;
using System.Collections;

/// <summary>
/// 默认故事模式通关祝贺界面（DefaultCongratulationsScreen）。
/// <para>用途：展示通关祝贺并自动播放音效/音乐，延迟数秒后自动进入下一界面（通常为结尾演出）。</para>
/// </summary>
public class DefaultCongratulationsScreen : StoryModeScreen{
	#region public instance properties
	/// <summary>播放的庆祝音效。</summary>
	public AudioClip sound;
	/// <summary>播放的背景音乐。</summary>
	public AudioClip music;
	/// <summary>延迟播放音乐的时间。</summary>
	public float delayBeforePlayingMusic = 0.1f;
	/// <summary>延迟进入下一界面的时间（秒）。</summary>
	public float delayBeforeLoadingNextScreen = 3f;
	#endregion

	#region public override methods
	/// <summary>
	/// 屏幕显示时：播放音效与音乐，并延迟进入下一界面。
	/// </summary>
	public override void OnShow (){
		base.OnShow ();

		if (this.music != null){
			UFE.DelayLocalAction(delegate(){UFE.PlayMusic(this.music);}, this.delayBeforePlayingMusic);
		}
		
		if (this.sound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.sound);}, this.delayBeforePlayingMusic);
		}

		UFE.DelaySynchronizedAction(this.GoToNextScreen, this.delayBeforeLoadingNextScreen);
	}
	#endregion
}
