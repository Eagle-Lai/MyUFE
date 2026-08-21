using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 默认主菜单界面（DefaultMainMenuScreen）。
/// <para>用途：主菜单界面——提供默认导航系统与音效/音乐播放，并根据插件安装情况启用/禁用网络与蓝牙按钮。</para>
/// </summary>
public class DefaultMainMenuScreen : MainMenuScreen{
	#region public instance fields
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

	/// <summary>网络对战按钮。</summary>
	public Button buttonNetwork;
	/// <summary>蓝牙对战按钮。</summary>
	public Button buttonBluetooth;
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
	/// 屏幕显示时：高亮首个物体、播放音乐/音效，并按插件安装情况启用网络与蓝牙按钮。
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

		if (buttonNetwork != null){
			buttonNetwork.interactable = UFE.isNetworkAddonInstalled;
		}

		if (buttonBluetooth != null){
			buttonBluetooth.interactable = UFE.isNetworkAddonInstalled && Application.isMobilePlatform;
		}
	}
	#endregion
}
