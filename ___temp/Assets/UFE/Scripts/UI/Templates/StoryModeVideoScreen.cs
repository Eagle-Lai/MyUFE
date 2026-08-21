using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class StoryModeVideoScreen : StoryModeScreen {
	#region public class properties
	public static readonly string Data = "%Data%";
	public static readonly string Persistent = "%Persistent%";
	public static readonly string StreamingAssets = "%StreamingAssets%";
	public static readonly string Temp = "%Temp%";
	#endregion
	
	#region public instance properties
	// The name of the video file in the StreamingAssets folder
	public string mobilePlatformsPath = "video.mp4";
	public string otherPlatformsPath = "file://" + VideoIntroScreen.StreamingAssets + "/video.ogv";
	public bool skippable = true;
	public bool stopMusicImmediatelyAfterSkippingVideo = true;
	public bool stopVideoImmediatelyAfterSkippingVideo = true;
	public float delayBeforePlayingVideo = 0.05f;
	public float delayAfterSkippingVideo = 0.05f;
	#endregion
	
	#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_WP8 || UNITY_IOS)
	public override void OnShow (){
		base.OnShow ();
		Handheld.PlayFullScreenMovie(
			this.mobilePlatformsPath, 
			Color.black, 
			FullScreenMovieControlMode.Hidden,
			FullScreenMovieScalingMode.AspectFill
		);
		this.GoToNextScreen();
	}
	#elif !UNITY_EDITOR && UNITY_WEBGL
	public override void OnShow (){
		base.OnShow ();
		this.GoToNextScreen();
	}
	#else
	public override void OnShow (){
		base.OnShow ();
		
		this.transform.parent = null;
		this.transform.localPosition = Vector3.zero;
		this.transform.localRotation = Quaternion.identity;
		this.transform.localScale = Vector3.one;
		this.StartCoroutine(this.LoadMovie());
	}
	
	protected virtual IEnumerator LoadMovie(){
		// Start loading movie from disk...
		string url = 
			this.otherPlatformsPath
				.Replace(VideoIntroScreen.Data, Application.dataPath)
				.Replace(VideoIntroScreen.Persistent, Application.persistentDataPath)
				.Replace(VideoIntroScreen.StreamingAssets, Application.streamingAssetsPath)
				.Replace(VideoIntroScreen.Temp, Application.temporaryCachePath);
		
		// 使用 VideoPlayer 替代已移除的 MovieTexture / GUITexture
		VideoPlayer videoPlayer = this.GetComponent<VideoPlayer>();
		if (videoPlayer == null){
			videoPlayer = this.gameObject.AddComponent<VideoPlayer>();
		}
		videoPlayer.playOnAwake = false;
		videoPlayer.isLooping = false;
		videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
		if (Camera.main != null) videoPlayer.targetCamera = Camera.main;
		videoPlayer.url = url;
		videoPlayer.Prepare();
		
		// Wait the min delay before starting playing the video
		if (this.delayBeforePlayingVideo > 0){
			yield return new WaitForSeconds(this.delayBeforePlayingVideo);
		}
		
		// Wait until we have enough information to start playing the movie...
		while (!videoPlayer.isPrepared){
			yield return null;
		}
		
		// Start playing the movie
		videoPlayer.Play();
		
		//-------------------------------------------------------------------------------------------------------------
		// We need to enable the sound during the duration of the movie because it doesn't matter the user settings,
		// we want him to hear the AudioClip. However, we need to play this sound as music because Unity won't allow
		// us to play streamed audio as SoundFX (Unity doesn't throw an error, but the AudioClip isn't played).
		//-------------------------------------------------------------------------------------------------------------
		bool musicEnabled = UFE.GetMusic();
		bool musicLooped = UFE.IsMusicLooped();
		float musicVolume = UFE.GetMusicVolume();
		AudioClip clip = UFE.GetMusicClip();
		
		//UFE.SetMusic(true);
		UFE.LoopMusic(false);
		//UFE.SetMusicVolume(1f);
		
		//-------------------------------------------------------------------------------------------------------------
		// After the video finish, restore the original audio settings.
		//-------------------------------------------------------------------------------------------------------------
		while(videoPlayer.isPlaying && !(skippable && Input.anyKeyDown)){ yield return null; }
		
		//-------------------------------------------------------------------------------------------------------------
		// Check if the video has been skipped. In that case, wait for the "delay after skipping video" time.
		//-------------------------------------------------------------------------------------------------------------
		if (videoPlayer.isPlaying && this.delayAfterSkippingVideo > 0f){
			if (this.stopMusicImmediatelyAfterSkippingVideo){
				UFE.StopMusic();
			}
			
			if (this.stopVideoImmediatelyAfterSkippingVideo){
				videoPlayer.Stop();
			}
			
			yield return new WaitForSeconds(this.delayAfterSkippingVideo);
		}
		
		//-------------------------------------------------------------------------------------------------------------
		// If we haven't done it yet, stop the video, the audio and free the used memory
		//-------------------------------------------------------------------------------------------------------------
		UFE.StopMusic();
		videoPlayer.Stop();
		GameObject.Destroy(clip);
		
		//-------------------------------------------------------------------------------------------------------------
		// Finally, restore the original audio settings...
		//-------------------------------------------------------------------------------------------------------------
		UFE.SetMusic(musicEnabled);
		UFE.LoopMusic(musicLooped);
		UFE.SetMusicVolume(musicVolume);
		UFE.PlayMusic(clip);
		
		//-------------------------------------------------------------------------------------------------------------
		// And go to the next screen
		//-------------------------------------------------------------------------------------------------------------
		this.GoToNextScreen();
	}
	#endif
}
