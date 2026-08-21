using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FPLibrary;

/// <summary>
/// 默认场地选择界面（DefaultStageSelectionScreen）。
/// <para>用途：选场界面——显示双方角色头像/名称与场地预览截图，支持上/下方向循环浏览场地，</para>
/// <para>使用 SpecialNavigationSystem 处理方向键导航与确认/取消操作。</para>
/// </summary>
public class DefaultStageSelectionScreen : StageSelectionScreen{
	#region public instance properties
	/// <summary>移动光标音效。</summary>
	public AudioClip moveCursorSound;
	/// <summary>加载音效。</summary>
	public AudioClip onLoadSound;
	/// <summary>背景音乐。</summary>
	public AudioClip music;
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
	/// <summary>延迟播放音乐的时间。</summary>
	public float delayBeforePlayingMusic = 0.1f;
	#endregion

	#region public instance methods
	/// <summary>
	/// 浏览下一个场地（循环）。
	/// </summary>
	public virtual void NextStage(){
		if (this.moveCursorSound != null) UFE.PlaySound(this.moveCursorSound);
		this.SetHoverIndex((this.stageHoverIndex + 1) % UFE.config.stages.Length);
	}

	/// <summary>
	/// 浏览上一个场地（循环）。
	/// </summary>
	public virtual void PreviousStage(){
		int length = UFE.config.stages.Length;
		if (this.moveCursorSound != null) UFE.PlaySound(this.moveCursorSound);
		this.SetHoverIndex((this.stageHoverIndex + length - 1) % length);
	}

	/// <summary>
	/// 设置悬停索引并更新场地名称与预览截图。
	/// </summary>
	/// <param name="stageIndex">场地索引。</param>
	public override void SetHoverIndex(int stageIndex){
		int length = UFE.config.stages.Length;

		if (!this.closing && stageIndex >= 0 && stageIndex < length){
			StageOptions stage = UFE.config.stages[stageIndex];
			base.SetHoverIndex(stageIndex);

			if (this.nameStage != null) this.nameStage.text = stage.stageName;
			if (this.screenshotStage != null){
				this.screenshotStage.sprite = Sprite.Create(
					stage.screenshot,
					new Rect(0f, 0f, stage.screenshot.width, stage.screenshot.height),
					new Vector2(0.5f * stage.screenshot.width, 0.5f * stage.screenshot.height)
				);
			}
		}
	}
	#endregion

	#region public override methods
	/// <summary>
	/// 固定帧更新：使用特殊导航系统处理方向键与确认/取消。
	/// </summary>
	public override void DoFixedUpdate(
		IDictionary<InputReferences, InputEvents> player1PreviousInputs,
		IDictionary<InputReferences, InputEvents> player1CurrentInputs,
		IDictionary<InputReferences, InputEvents> player2PreviousInputs,
		IDictionary<InputReferences, InputEvents> player2CurrentInputs
	){
		base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);

		this.SpecialNavigationSystem(
			player1PreviousInputs,
			player1CurrentInputs,
			player2PreviousInputs,
			player2CurrentInputs,
			new UFEScreenExtensions.MoveCursorCallback(this.HighlightStage),
			new UFEScreenExtensions.ActionCallback(this.TrySelectStage),
			new UFEScreenExtensions.ActionCallback(this.TryDeselectStage)
		);
	}

	/// <summary>
	/// 屏幕显示时：播放音乐/音效、填充角色信息并初始化场地预览。
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

		this.stageHoverIndex = 0;
		StageOptions stage = UFE.config.stages[this.stageHoverIndex];

		if (stage != null){
			if (this.screenshotStage != null){
				this.screenshotStage.sprite = Sprite.Create(
					stage.screenshot,
					new Rect(0f, 0f, stage.screenshot.width, stage.screenshot.height),
					new Vector2(0.5f * stage.screenshot.width, 0.5f * stage.screenshot.height)
				);
			}

			if (this.nameStage != null){
				this.nameStage.text = stage.stageName;
			}
		}
	}
	#endregion

	#region protected instance methods: methods required by the Special Navigation System (GUI)
	/// <summary>
	/// 光标移动回调（特殊导航系统）：垂直方向键切换场地。
	/// </summary>
	protected virtual void HighlightStage(
		Fix64 horizontalAxis, 
		Fix64 verticalAxis, 
		bool horizontalAxisDown, 
		bool verticalAxisDown, 
		bool confirmButtonDown, 
		bool cancelButtonDown, 
		AudioClip sound
	){
		if (verticalAxisDown){
			if (verticalAxis > 0){
				this.PreviousStage();
			}else if (verticalAxis < 0){
				this.NextStage();
			}
		}
	}

	/// <summary>
	/// 取消选择回调（特殊导航系统）。
	/// </summary>
	protected virtual void TryDeselectStage(AudioClip sound){
		this.TryDeselectStage();
	}

	/// <summary>
	/// 确认选择回调（特殊导航系统）。
	/// </summary>
	protected virtual void TrySelectStage(AudioClip sound){
		this.TrySelectStage();
	}
	#endregion
}
