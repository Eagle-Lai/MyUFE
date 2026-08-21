using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using FPLibrary;

/// <summary>
/// 视频片头界面（VideoIntroScreen）。
/// <para>用途：以视频（VideoClip 或 URL）展示游戏片头/Logo 的界面——使用 Unity VideoPlayer 渲染到材质，</para>
/// <para>支持跳过（按键或播完自动跳转），结束后进入主菜单。</para>
/// </summary>
public class VideoIntroScreen : IntroScreen {
    #region public class properties
	/// <summary>路径占位符：%Data%（游戏数据目录）。</summary>
    public static readonly string Data = "%Data%";
	/// <summary>路径占位符：%Persistent%（持久化数据目录）。</summary>
    public static readonly string Persistent = "%Persistent%";
	/// <summary>路径占位符：%StreamingAssets%（流式资源目录）。</summary>
    public static readonly string StreamingAssets = "%StreamingAssets%";
	/// <summary>路径占位符：%Temp%（临时目录）。</summary>
    public static readonly string Temp = "%Temp%";
    #endregion

    #region public instance properties
	/// <summary>视频片段（loadFromUrl 为 false 时使用）。</summary>
    public VideoClip videoClip;
    // The name of the video file in the StreamingAssets folder
	/// <summary>是否从 URL 加载视频。</summary>
    public bool loadFromUrl = false;
	/// <summary>视频路径或 URL（支持占位符替换）。</summary>
    public string pathOrUrl = "file://" + VideoIntroScreen.StreamingAssets + "/video.ogv";
	/// <summary>是否可跳过（按任意键跳过）。</summary>
    public bool skippable = true;
	/// <summary>延迟播放视频的时间。</summary>
    public float delayBeforePlayingVideo = 0.05f;
	/// <summary>跳过视频后的延迟。</summary>
    public float delayAfterSkippingVideo = 0.05f;
    #endregion

	/// <summary>视频播放器组件。</summary>
    private VideoPlayer videoPlayer;
	/// <summary>音频源（视频音频输出）。</summary>
    private AudioSource audioSource;

	/// <summary>
	/// 屏幕显示时：脱离父级并延迟加载视频。
	/// </summary>
    public override void OnShow()
    {
        base.OnShow();

        this.transform.parent = null;
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
        this.transform.localScale = Vector3.one;
        UFE.DelayLocalAction(LoadMovie, (Fix64)delayBeforePlayingVideo);
    }

	/// <summary>
	/// 加载并播放视频：获取/创建 VideoPlayer 与 AudioSource，设置视频源（片段或 URL）、渲染模式与音频输出。
	/// </summary>
    public void LoadMovie()
    {
        videoPlayer = this.GetComponent<VideoPlayer>();
        if (videoPlayer == null) videoPlayer = this.gameObject.AddComponent<VideoPlayer>();

        audioSource = this.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = this.gameObject.AddComponent<AudioSource>();


        if (loadFromUrl)
        {
            string url = this.pathOrUrl
                    .Replace(VideoIntroScreen.Data, Application.dataPath)
                    .Replace(VideoIntroScreen.Persistent, Application.persistentDataPath)
                    .Replace(VideoIntroScreen.StreamingAssets, Application.streamingAssetsPath)
                    .Replace(VideoIntroScreen.Temp, Application.temporaryCachePath);

            videoPlayer.url = url;
        }
        else
        {
            videoPlayer.clip = videoClip;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.targetMaterialRenderer = GetComponent<Renderer>();
        videoPlayer.targetMaterialProperty = "_MainTex";
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
    }

	/// <summary>
	/// 每帧更新：按键跳过或视频播放完毕后停止视频并进入主菜单。
	/// </summary>
    public void Update()
    {
        if ((skippable && Input.anyKeyDown) || videoPlayer.frame >= (long)videoPlayer.frameCount)
        {
            videoPlayer.Stop();
            videoPlayer = null;
            UFE.DelayLocalAction(this.GoToMainMenu, (Fix64)delayAfterSkippingVideo);
        }
    }
}
