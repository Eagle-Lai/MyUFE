using UnityEngine;
using System.Collections;

/// <summary>
/// 默认故事模式游戏结束界面（DefaultGameOverScreen）。
/// <para>用途：展示游戏结束信息并播放音效/音乐，延迟数秒后自动进入下一界面（通常返回主菜单）。</para>
/// </summary>
public class DefaultGameOverScreen : StoryModeScreen{
	#region public instance properties
	/// <summary>加载音效。</summary>
	public AudioClip onLoadSound;
	/// <summary>背景音乐。</summary>
	public AudioClip music;
	/// <summary>加载时是否停止之前的音效。</summary>
	public bool stopPreviousSoundEffectsOnLoad = false;
	/// <summary>延迟播放音乐的时间。</summary>
	public float delayBeforePlayingMusic = 0.1f;
	/// <summary>延迟进入下一界面的时间（秒）。</summary>
	public float delayBeforeLoadingNextScreen = 3f;
	#endregion

	#region public override methods
	/// <summary>
	/// 屏幕显示时：播放音乐/音效并延迟进入下一界面。
	/// </summary>
	public override void OnShow (){
		base.OnShow ();

		if (this.music != null){
			UFE.DelayLocalAction(delegate(){UFE.PlayMusic(this.music);}, this.delayBeforePlayingMusic);
		}
		
		if (this.stopPreviousSoundEffectsOnLoad){
			UFE.StopSounds();
		}
		
		if (this.onLoadSound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.onLoadSound);}, this.delayBeforePlayingMusic);
		}

		UFE.DelaySynchronizedAction(this.GoToNextScreen, delayBeforeLoadingNextScreen);
	}
	#endregion
}
