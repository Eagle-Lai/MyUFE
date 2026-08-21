using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 默认战斗加载界面（DefaultLoadingBattleScreen）。
/// <para>用途：战斗开始前的加载界面——显示双方角色头像/名称与场地截图，</para>
/// <para>播放音乐/音效，延迟执行资源预加载（PreloadBattle）后自动开始战斗。</para>
/// </summary>
public class DefaultLoadingBattleScreen : LoadingBattleScreen{
	#region public instance properties
	/// <summary>加载音效。</summary>
	public AudioClip onLoadSound;
	/// <summary>背景音乐。</summary>
    public AudioClip music;
	/// <summary>延迟播放音乐的时间。</summary>
    public float delayBeforeMusic = .1f;
	/// <summary>延迟预加载的时间。</summary>
    public float delayBeforePreload = .5f;
	/// <summary>预加载后的延迟。</summary>
    public float delayAfterPreload = .5f;
	/// <summary>玩家1名称文本。</summary>
	public Text namePlayer1;
	/// <summary>玩家2名称文本。</summary>
	public Text namePlayer2;
	/// <summary>场地名称文本。</summary>
	public Text nameStage;
	/// <summary>玩家1头像。</summary>
	public Image portraitPlayer1;
	/// <summary>玩家2头像。</summary>
	public Image portraitPlayer2;
	/// <summary>场地截图。</summary>
	public Image screenshotStage;
	/// <summary>加载时是否停止之前的音效。</summary>
    public bool stopPreviousSoundEffectsOnLoad = false;
	#endregion
	
	#region public override methods
	/// <summary>
	/// 屏幕显示时：播放音乐/音效、填充角色与场地信息、延迟执行资源预加载并开始战斗。
	/// </summary>
	public override void OnShow (){
		base.OnShow ();

		if (this.music != null){
			UFE.DelayLocalAction(delegate(){UFE.PlayMusic(this.music);}, this.delayBeforeMusic);
		}
		
		if (this.stopPreviousSoundEffectsOnLoad){
			UFE.StopSounds();
		}
		
		if (this.onLoadSound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.onLoadSound);}, this.delayBeforeMusic);
		}

		if (UFE.config.player1Character != null){
			if (this.portraitPlayer1 != null){
				this.portraitPlayer1.sprite = Sprite.Create(
					UFE.config.player1Character.profilePictureBig,
					new Rect(0f, 0f, UFE.config.player1Character.profilePictureBig.width, UFE.config.player1Character.profilePictureBig.height),
					new Vector2(0.5f * UFE.config.player1Character.profilePictureBig.width, 0.5f * UFE.config.player1Character.profilePictureBig.height)
				);
			}

			if (this.namePlayer1 != null){
				this.namePlayer1.text = UFE.config.player1Character.characterName;
			}
		}

		if (UFE.config.player2Character != null){
			if (this.portraitPlayer2 != null){
				this.portraitPlayer2.sprite = Sprite.Create(
					UFE.config.player2Character.profilePictureBig,
					new Rect(0f, 0f, UFE.config.player2Character.profilePictureBig.width, UFE.config.player2Character.profilePictureBig.height),
					new Vector2(0.5f * UFE.config.player2Character.profilePictureBig.width, 0.5f * UFE.config.player2Character.profilePictureBig.height)
				);
			}

			if (this.namePlayer2 != null){
				this.namePlayer2.text = UFE.config.player2Character.characterName;
			}
		}

		if (UFE.config.selectedStage != null){
			if (this.screenshotStage != null){
				this.screenshotStage.sprite = Sprite.Create(
					UFE.config.selectedStage.screenshot,
					new Rect(0f, 0f, UFE.config.selectedStage.screenshot.width, UFE.config.selectedStage.screenshot.height),
					new Vector2(0.5f * UFE.config.selectedStage.screenshot.width, 0.5f * UFE.config.selectedStage.screenshot.height)
				);

				Animator anim = this.screenshotStage.GetComponent<Animator>();
				if (anim != null){
					anim.enabled = UFE.gameMode != GameMode.StoryMode;
				}
			}

			/*if (this.nameStage != null){
				this.nameStage.text = UFE.config.selectedStage.stageName;
			}*/
		}

        UFE.DelayLocalAction(UFE.PreloadBattle, this.delayBeforePreload);
        UFE.DelayLocalAction(this.StartBattle, UFE.config._preloadingTime);

        // If network synchornization is needed in this screen, use this instead
        //UFE.DelaySynchronizedAction(UFE.PreloadBattle, this.delayBeforePreload);
        //UFE.DelaySynchronizedAction(this.StartBattle, this.delayBeforePreload + UFE.config.preloadingTime + this.delayAfterPreload);
	}
	#endregion
}
