using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 默认加入游戏界面（DefaultJoinGameScreen）。
/// <para>用途：局域网加入游戏界面——提供默认导航系统与连接状态文本，搜索时显示"正在搜索"、未找到时提示。</para>
/// </summary>
public class DefaultJoinGameScreen : JoinGameScreen {
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
	/// <summary>延迟播放音乐的时间。</summary>
	public float delayBeforePlayingMusic = 0.1f;
	/// <summary>连接状态文本。</summary>
	public Text connectionStatus;
	#endregion

	#region public override methods
	/// <summary>
	/// 固定帧更新：调用默认导航系统（取消返回网络界面）。
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
			this.GoToNetworkGameScreen
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
		
		if (this.onLoadSound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.onLoadSound);}, this.delayBeforePlayingMusic);
		}
	}

	/// <summary>
	/// 加入首个 LAN 游戏：显示搜索状态并调用基类。
	/// </summary>
	public override void JoinFirstLanGame (){
		base.JoinFirstLanGame ();

		if (this.connectionStatus != null){
			this.connectionStatus.text = "Searching LAN Games...";
		}
	}
	#endregion

	#region protected override methods
	/// <summary>
	/// 未找到 LAN 游戏回调：更新状态文本。
	/// </summary>
	protected override void OnLanGameNotFound (){
		if (this.connectionStatus != null){
			this.connectionStatus.text = "LAN Game not found";
		}
	}
	#endregion
}
