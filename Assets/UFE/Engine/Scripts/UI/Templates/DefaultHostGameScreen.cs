using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 默认建房界面（DefaultHostGameScreen）。
/// <para>用途：局域网建房界面——提供默认导航系统与连接状态文本，开始建房时显示"等待玩家加入"。</para>
/// </summary>
public class DefaultHostGameScreen : HostGameScreen {
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
	/// 开始建房：显示等待状态并调用基类建房。
	/// </summary>
    public override void StartHostGame() {
        connectionStatus.text = "Waiting for players...";
        base.StartHostGame();
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
	#endregion
}
