using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// 默认选项界面（DefaultOptionsScreen）。
/// <para>用途：选项菜单界面——通过 UI 控件（音乐/音效开关与音量滑杆、AI 难度/引擎、调试开关、改键按钮）</para>
/// <para>读取与设置游戏配置；仅在界面可见（visible）时允许 GUI 回调修改配置，避免隐藏时误改。</para>
/// </summary>
public class DefaultOptionsScreen : OptionsScreen{
	#region public instance properties
	/// <summary>选择音效。</summary>
	public AudioClip selectSound;
	/// <summary>取消音效。</summary>
	public AudioClip cancelSound;
	/// <summary>移动光标音效。</summary>
	public AudioClip moveCursorSound;
	/// <summary>加载音效。</summary>
	public AudioClip onLoadSound;
	/// <summary>背景音乐。</summary>
	public AudioClip music;
	/// <summary>音乐开关。</summary>
	public Toggle musicToggle;
	/// <summary>音乐音量滑杆。</summary>
	public Slider musicSlider;
	/// <summary>音效开关。</summary>
	public Toggle soundToggle;
	/// <summary>音效音量滑杆。</summary>
	public Slider soundSlider;
	/// <summary>AI 难度滑杆。</summary>
	public Slider difficultySlider;
	/// <summary>AI 难度名称文本。</summary>
	public Text difficultyName;
	/// <summary>AI 引擎名称文本。</summary>
	public Text aiEngineName;
	/// <summary>调试模式开关。</summary>
	public Toggle debugModeToggle;
	/// <summary>改键按钮。</summary>
	public Button changeControlsButton;
	/// <summary>取消按钮。</summary>
	public Button cancelButton;
	/// <summary>滑杆调整速度。</summary>
	public float sliderSpeed = 0.1f;
	/// <summary>加载时是否停止之前的音效。</summary>
	public bool stopPreviousSoundEffectsOnLoad = false;
	/// <summary>延迟播放音乐的时间。</summary>
	public float delayBeforePlayingMusic = 0.1f;
	#endregion
	
	#region protected instance properties
	// This property is used for preventing the Unity GUI from updating 
	// the values of certain variables when the screen isn't visible
	/// <summary>界面是否可见（防止隐藏时 GUI 误改配置）。</summary>
	protected bool visible = false;
	#endregion
	
	#region public override methods
	/// <summary>
	/// 固定帧更新：调用默认导航系统（取消执行 CancelAction）。
	/// </summary>
	public override void DoFixedUpdate (
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
			this.CancelAction
		);
	}
	
	/// <summary>
	/// 界面隐藏时标记不可见。
	/// </summary>
	public override void OnHide (){
		base.OnHide ();
		this.visible = false;
	}
	
	/// <summary>
	/// 界面显示时：播放音乐/音效并同步所有 UI 控件到当前配置。
	/// </summary>
	public override void OnShow (){
		base.OnShow ();
		this.visible = true;
		
		if (this.music != null){
			UFE.DelayLocalAction(delegate(){UFE.PlayMusic(this.music);}, this.delayBeforePlayingMusic);
		}
		
		if (this.stopPreviousSoundEffectsOnLoad){
			UFE.StopSounds();
		}
		
		if (this.onLoadSound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.onLoadSound);}, this.delayBeforePlayingMusic);
		}
		
		if (this.musicToggle != null){
			this.musicToggle.isOn = UFE.config.music;
		}
		
		if (this.musicSlider != null){
			this.musicSlider.value = UFE.config.musicVolume;
		}
		
		if (this.soundToggle != null){
			this.soundToggle.isOn = UFE.config.soundfx;
		}
		
		if (this.soundSlider != null){
			this.soundSlider.value = UFE.config.soundfxVolume;
		}
		
		int difficultySettingsLength = UFE.config.aiOptions.difficultySettings.Length;
		AIDifficultySettings difficulty = UFE.GetAIDifficulty();
		
		if (this.difficultySlider != null){
			this.difficultySlider.minValue = 0;
			this.difficultySlider.maxValue = difficultySettingsLength - 1;
			this.difficultySlider.wholeNumbers = true;
			this.difficultySlider.value = this.GetDifficultyIndex(difficulty);
		}
		
		if (this.difficultyName != null){
			this.difficultyName.text = difficulty.difficultyLevel.ToString();
		}
		
		if (this.aiEngineName != null){
			AIEngine aiEngine = UFE.GetAIEngine();
			
			if (aiEngine == AIEngine.RandomAI){
				this.aiEngineName.text = "Random";
			}else{
				this.aiEngineName.text = "Fuzzy";
			}
		}
		
		if (this.debugModeToggle != null){
			this.debugModeToggle.isOn = UFE.config.debugOptions.debugMode;
		}
		
		
		if (this.changeControlsButton != null){
            this.changeControlsButton.gameObject.SetActive(
                (UFE.isCInputInstalled && UFE.config.inputOptions.inputManagerType == InputManagerType.cInput) ||
                (UFE.isRewiredInstalled && UFE.config.inputOptions.inputManagerType == InputManagerType.Rewired)
            );
		}
		
		this.HighlightOption(this.FindFirstSelectable());
	}
	#endregion
	
	#region public instance methods
	/// <summary>
	/// 按滑杆值设置 AI 难度。
	/// </summary>
	/// <param name="slider">难度滑杆。</param>
	public virtual void SetAIDifficulty(Slider slider){
		if (this.visible && slider != null){
			this.SetAIDifficulty(UFE.config.aiOptions.difficultySettings[Mathf.RoundToInt(slider.value)]);
		}
	}
	
	/// <summary>
	/// 按滑杆值设置音乐音量。
	/// </summary>
	/// <param name="slider">音量滑杆。</param>
	public virtual void SetMusicVolume(Slider slider){
		if (this.visible && slider != null){
			this.SetMusicVolume(slider.value);
		}
	}
	
	/// <summary>
	/// 按滑杆值设置音效音量。
	/// </summary>
	/// <param name="slider">音量滑杆。</param>
	public virtual void SetSoundFXVolume(Slider slider){
		if (this.visible && slider != null){
			this.SetSoundFXVolume(slider.value);
		}
	}
	#endregion
	
	#region public override methods
	/// <summary>
	/// 设置 AI 难度并同步滑杆与名称文本。
	/// </summary>
	/// <param name="difficulty">难度设置。</param>
	public override void SetAIDifficulty (AIDifficultySettings difficulty){
		if (this.visible){
			base.SetAIDifficulty (difficulty);
			
			if (this.difficultySlider != null){
				this.difficultySlider.value = this.GetDifficultyIndex(difficulty);
			}
			
			if (this.difficultyName != null){
				this.difficultyName.text = difficulty.difficultyLevel.ToString();
			}
		}
	}
	
	/// <summary>
	/// 设置 AI 引擎并同步名称文本。
	/// </summary>
	/// <param name="aiEngine">AI 引擎类型。</param>
	public override void SetAIEngine (AIEngine aiEngine){
		if (this.visible){
			base.SetAIEngine(aiEngine);
			
			if (this.aiEngineName != null){
				aiEngine = UFE.GetAIEngine();
				
				if (aiEngine == AIEngine.RandomAI){
					this.aiEngineName.text = "Random";
				}else{
					this.aiEngineName.text = "Fuzzy";
				}
			}
		}
	}
	
	/// <summary>
	/// 设置调试模式并同步开关状态。
	/// </summary>
	/// <param name="enabled">是否启用。</param>
	public override void SetDebugMode (bool enabled){
		if (this.visible){
			base.SetDebugMode(enabled);
			
			if (this.debugModeToggle != null){
				this.debugModeToggle.isOn = UFE.config.debugOptions.debugMode;
			}
		}
	}
	
	/// <summary>
	/// 设置音乐开关并同步开关状态。
	/// </summary>
	/// <param name="enabled">是否启用。</param>
	public override void SetMusic(bool enabled){
		if (this.visible){
			base.SetMusic(enabled);
			
			if (this.musicToggle != null){
				this.musicToggle.isOn = !this.IsMusicMuted();
			}
		}
	}
	
	/// <summary>
	/// 设置音效开关并同步开关状态。
	/// </summary>
	/// <param name="enabled">是否启用。</param>
	public override void SetSoundFX(bool enabled){
		if (this.visible){
			base.SetSoundFX(enabled);
			
			if (this.soundToggle != null){
				this.soundToggle.isOn = !this.IsSoundMuted();
			}
		}
	}
	
	/// <summary>
	/// 设置音乐音量并同步滑杆。
	/// </summary>
	/// <param name="volume">音量值。</param>
	public override void SetMusicVolume(float volume){
		if (this.visible){
			base.SetMusicVolume(volume);
			
			if (this.musicSlider != null){
				this.musicSlider.value = this.GetMusicVolume();
			}
		}
	}
	
	/// <summary>
	/// 设置音效音量并同步滑杆。
	/// </summary>
	/// <param name="volume">音量值。</param>
	public override void SetSoundFXVolume(float volume){
		if (this.visible){
			base.SetSoundFXVolume(volume);
			
			if (this.soundSlider != null){
				this.soundSlider.value = this.GetSoundFXVolume();
			}
		}
	}
	
	/// <summary>
	/// 切换 AI 引擎（可见时）。
	/// </summary>
	public override void ToggleAIEngine (){
		if (this.visible){
			base.ToggleAIEngine();
		}
	}
	
	/// <summary>
	/// 切换调试模式（通过开关控件）。
	/// </summary>
	public override void ToggleDebugMode (){
		if (this.visible){
			if (this.debugModeToggle != null){
				this.SetDebugMode(this.debugModeToggle.isOn);
			}else{
				base.ToggleDebugMode ();
			}
		}
	}
	
	/// <summary>
	/// 切换音乐开关（通过开关控件）。
	/// </summary>
	public override void ToggleMusic(){
		if (this.visible){
			if (this.musicToggle != null){
				this.SetMusic(this.musicToggle.isOn);
			}else{
				base.ToggleMusic();
			}
		}
	}
	
	/// <summary>
	/// 切换音效开关（通过开关控件）。
	/// </summary>
	public override void ToggleSoundFX(){
		if (this.visible){
			if (this.soundToggle != null){
				this.SetSoundFX(this.soundToggle.isOn);
			}else{
				base.ToggleSoundFX();
			}
		}
	}
	#endregion
	
	#region protected instance methods
	/// <summary>
	/// 取消动作：调用取消按钮点击事件。
	/// </summary>
	protected virtual void CancelAction(){
		if (this.cancelButton != null && this.cancelButton.onClick != null){
			this.cancelButton.onClick.Invoke();
		}
	}

	/// <summary>
	/// 获取指定难度设置对应的列表索引。
	/// </summary>
	/// <param name="difficulty">难度设置。</param>
	/// <returns>索引；未找到返回 -1。</returns>
	protected virtual int GetDifficultyIndex(AIDifficultySettings difficulty){
		AIDifficultySettings[] difficultySettings = UFE.config.aiOptions.difficultySettings;
		int count = difficultySettings.Length;
		
		for (int i = 0; i < count; ++i){
			if (difficulty == difficultySettings[i]){
				return i;
			}
		}
		
		return -1;
	}
	#endregion
}
