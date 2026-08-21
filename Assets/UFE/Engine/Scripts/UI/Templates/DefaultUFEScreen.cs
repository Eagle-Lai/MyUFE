using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 默认屏幕基类（DefaultUFEScreen）。
/// <para>用途：所有默认 UI 屏幕的公共基类——提供屏幕通用音效（加载/背景音乐/选择/取消/移动光标）、</para>
/// <para>默认按键导航系统（DefaultNavigationSystem）与取消按钮回调（CancelAction）。</para>
/// </summary>
public class DefaultUFEScreen : UFEScreen{
	#region public instance properties
	/// <summary>屏幕加载时播放的音效。</summary>
	public AudioClip onLoadSound;
	/// <summary>屏幕的背景音乐。</summary>
	public AudioClip music;
	/// <summary>选择选项时的音效。</summary>
	public AudioClip selectSound;
	/// <summary>取消时的音效。</summary>
	public AudioClip cancelSound;
	/// <summary>移动光标时的音效。</summary>
	public AudioClip moveCursorSound;
	/// <summary>取消按钮引用。</summary>
	public Button cancelButton;
	/// <summary>加载时是否停止之前的音效。</summary>
	public bool stopPreviousSoundEffectsOnLoad = false;
	/// <summary>延迟播放音乐的时间。</summary>
	public float delayBeforePlayingMusic = 0.1f;
	#endregion

	#region public override methods
	/// <summary>
	/// 固定帧更新：调用默认按键导航系统处理双方输入。
	/// </summary>
	/// <param name="player1PreviousInputs">玩家1上一帧输入。</param>
	/// <param name="player1CurrentInputs">玩家1当前帧输入。</param>
	/// <param name="player2PreviousInputs">玩家2上一帧输入。</param>
	/// <param name="player2CurrentInputs">玩家2当前帧输入。</param>
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
			this.CancelAction
		);
	}

	/// <summary>
	/// 屏幕显示时：高亮首个可交互物体、播放背景音乐与加载音效。
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

	#region protected methods
	/// <summary>
	/// 取消动作：调用取消按钮的点击事件。
	/// </summary>
	protected virtual void CancelAction(){
		if (this.cancelButton != null && this.cancelButton.onClick != null){
			this.cancelButton.onClick.Invoke();
		}
	}
	#endregion
}
