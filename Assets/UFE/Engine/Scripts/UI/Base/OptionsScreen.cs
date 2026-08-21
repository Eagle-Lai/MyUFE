using UnityEngine;
using System;
using System.Reflection;

/// <summary>
/// 选项界面（OptionsScreen）。
/// <para>用途：游戏选项菜单基类——提供音乐/音效开关与音量、AI 引擎/难度、调试模式等设置的读取与设置方法，</para>
/// <para>并支持进入按键配置界面（cInput 的 cGUI 或 Rewired 的输入配置 UI）。</para>
/// </summary>
public class OptionsScreen : UFEScreen{
	/// <summary>
	/// 获取当前音乐音量。
	/// </summary>
	/// <returns>音乐音量值。</returns>
	public virtual float GetMusicVolume(){
		return UFE.GetMusicVolume();
	}

	/// <summary>
	/// 获取当前音效音量。
	/// </summary>
	/// <returns>音效音量值。</returns>
	public virtual float GetSoundFXVolume(){
		return UFE.GetSoundFXVolume();
	}

	/// <summary>
	/// 进入按键配置界面（cInput 或 Rewired 的输入配置 UI）。
	/// </summary>
	public virtual void GoToControlsScreen(){
        if (UFE.config.inputOptions.inputManagerType == InputManagerType.cInput && UFE.isCInputInstalled)
        {
            UFE.SearchClass("cGUI").GetMethod(
                "ToggleGUI",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
                null,
                new Type[] { },
                null
            ).Invoke(null, null);
        }
        else if (UFE.config.inputOptions.inputManagerType == InputManagerType.Rewired && UFE.isRewiredInstalled)
        {
            if (RewiredInputController.inputConfiguration != null)
            {
                RewiredInputController.inputConfiguration.ShowInputConfigurationUI(() => UFE.StartOptionsScreen(0.1f)); // show the config screen and return to Options screen when screen is closed
                Destroy(this.gameObject); // close options screen when control config UI is opened to prevent UFEScreen navigation system from interfering
            }
        }
    }

	/// <summary>
	/// 返回主菜单。
	/// </summary>
	public virtual void GoToMainMenuScreen(){
		UFE.StartMainMenuScreen();
	}

	/// <summary>
	/// 音乐当前是否静音。
	/// </summary>
	/// <returns>静音返回 true。</returns>
	public virtual bool IsMusicMuted(){
		return !UFE.config.music;
	}
	
	/// <summary>
	/// 音效当前是否静音。
	/// </summary>
	/// <returns>静音返回 true。</returns>
	public virtual bool IsSoundMuted(){
		return !UFE.config.soundfx;
	}

	/// <summary>
	/// 设置音乐静音。
	/// </summary>
	/// <param name="mute">是否静音。</param>
	public void MuteMusic(bool mute){
		this.SetMusic(!mute);
	}

	/// <summary>
	/// 设置音效静音。
	/// </summary>
	/// <param name="mute">是否静音。</param>
	public void MuteSoundFX(bool mute){
		this.SetSoundFX(!mute);
	}

	/// <summary>
	/// 设置 AI 难度（按难度参数）。
	/// </summary>
	/// <param name="difficulty">难度设置。</param>
	public virtual void SetAIDifficulty(AIDifficultySettings difficulty){
		if (difficulty != null){
			UFE.SetAIDifficulty(difficulty.difficultyLevel);
		}
	}

	/// <summary>
	/// 设置 AI 引擎（未安装 AI 插件时回退随机 AI）。
	/// </summary>
	/// <param name="aiEngine">AI 引擎类型。</param>
	public virtual void SetAIEngine(AIEngine aiEngine){
		if (UFE.isAiAddonInstalled){
			UFE.SetAIEngine(aiEngine);
		}else{
			UFE.SetAIEngine(AIEngine.RandomAI);
		}
	}

	/// <summary>
	/// 设置调试模式。
	/// </summary>
	/// <param name="enabled">是否启用。</param>
	public virtual void SetDebugMode(bool enabled){
		UFE.SetDebugMode(enabled);
	}
	
	/// <summary>
	/// 设置音乐开关。
	/// </summary>
	/// <param name="enabled">是否启用。</param>
	public virtual void SetMusic(bool enabled){
		UFE.SetMusic(enabled);
	}

	/// <summary>
	/// 设置音效开关。
	/// </summary>
	/// <param name="enabled">是否启用。</param>
	public virtual void SetSoundFX(bool enabled){
		UFE.SetSoundFX(enabled);
	}

	/// <summary>
	/// 设置音乐音量。
	/// </summary>
	/// <param name="volume">音量值。</param>
	public virtual void SetMusicVolume(float volume){
		UFE.SetMusicVolume(volume);
	}

	/// <summary>
	/// 设置音效音量。
	/// </summary>
	/// <param name="volume">音量值。</param>
	public virtual void SetSoundFXVolume(float volume){
		UFE.SetSoundFXVolume(volume);
	}

	/// <summary>
	/// 切换 AI 引擎（随机↔模糊）。
	/// </summary>
	public virtual void ToggleAIEngine(){
		if (UFE.GetAIEngine() == AIEngine.RandomAI){
			this.SetAIEngine(AIEngine.FuzzyAI);
		}else{
			this.SetAIEngine(AIEngine.RandomAI);
		}
	}

	/// <summary>
	/// 切换调试模式。
	/// </summary>
	public virtual void ToggleDebugMode(){
		this.SetDebugMode(!UFE.config.debugOptions.debugMode);
	}
	
	/// <summary>
	/// 切换音乐开关。
	/// </summary>
	public virtual void ToggleMusic(){
		UFE.SetMusic(!UFE.GetMusic());
	}

	/// <summary>
	/// 切换音效开关。
	/// </summary>
	public virtual void ToggleSoundFX(){
		UFE.SetSoundFX(!UFE.GetSoundFX());
	}
}
