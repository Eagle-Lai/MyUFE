using UnityEngine;
using System.Collections;

/// <summary>
/// 贴图片头界面（TextureIntroScreen）。
/// <para>用途：以静态贴图展示片头/Logo 的界面——播放音乐/音效，</para>
/// <para>等待指定时长或玩家按键（可跳过）后进入主菜单。</para>
/// </summary>
public class TextureIntroScreen : IntroScreen{
	#region public instance properties
	/// <summary>加载音效。</summary>
	public AudioClip onLoadSound;
	/// <summary>背景音乐。</summary>
	public AudioClip music;
	/// <summary>是否可跳过（按任意键跳过）。</summary>
	public bool skippable = true;
	/// <summary>加载时是否停止之前的音效。</summary>
	public bool stopPreviousSoundEffectsOnLoad = false;
	/// <summary>延迟播放音乐的时间。</summary>
	public float delayBeforePlayingMusic = 0.1f;
	/// <summary>进入主菜单的延迟（秒）。</summary>
	public float delayBeforeGoingToMenu = 3f;
	/// <summary>可跳过前的最小等待时间（秒）。</summary>
	public float minDelayBeforeSkipping = 0.1f;
	#endregion

	#region public override methods
	/// <summary>
	/// 屏幕显示时：播放音乐/音效并启动片头计时协程。
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

		this.StartCoroutine(this.ShowScreen());
	}

	/// <summary>
	/// 片头计时协程：等待指定时长或玩家按键跳过，然后进入主菜单。
	/// </summary>
	/// <returns>协程枚举器。</returns>
	public virtual IEnumerator ShowScreen(){
		float startTime = Time.realtimeSinceStartup;
		float time = 0f;
		
		while(
			time < this.delayBeforeGoingToMenu && 
			!(skippable && Input.anyKeyDown && time > this.minDelayBeforeSkipping)
		){
			yield return null;
			time = Time.realtimeSinceStartup - startTime;
		}

		this.GoToMainMenu();
	}
	#endregion
}
