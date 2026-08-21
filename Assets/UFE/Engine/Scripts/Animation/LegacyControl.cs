using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FPLibrary;

/// <summary>
/// 旧版动画系统控制（LegacyControl）。
/// <para>用途：封装 Unity 旧版 Animation 组件，为 UFE 提供统一的多片段动画注册、播放、速度控制、</para>
/// <para>位置追踪（根位移）与帧同步驱动（overrideAnimatorUpdate 手动采样）能力。</para>
/// </summary>

/// <summary>
/// 旧版动画数据：一个已注册动画片段的数据记录。
/// </summary>
[System.Serializable]
public class LegacyAnimationData {
	/// <summary>动画片段实例。</summary>
	public AnimationClip clip;
	/// <summary>动画注册名。</summary>
	public string clipName;
	/// <summary>播放模式（循环/单次/夹紧）。</summary>
	public WrapMode wrapMode;
	/// <summary>片段时长（定点数）。</summary>
	public Fix64 length = 0;
	/// <summary>原始播放速度。</summary>
	public Fix64 originalSpeed = 1;
	/// <summary>归一化播放速度（相对原始速度）。</summary>
	public Fix64 normalizedSpeed = 1;

    #region trackable definitions
	/// <summary>归一化播放时间（运行时跟踪）。</summary>
    public Fix64 normalizedTime = 1;
	/// <summary>已播放秒数（游戏时间，运行时跟踪）。</summary>
    public Fix64 secondsPlayed = 0;
	/// <summary>已播放真实秒数（运行时跟踪）。</summary>
    public Fix64 realSecondsPlayed = 0;
	/// <summary>已播放次数（运行时跟踪）。</summary>
    public int timesPlayed = 0;
	/// <summary>当前播放速度（运行时跟踪）。</summary>
    public Fix64 speed = 1;
    #endregion
	/// <summary>关联的 AnimationState（Inspector 隐藏）。</summary>
    [HideInInspector] public AnimationState animState;
}

/// <summary>
/// 旧版动画控制脚本（LegacyControl）。
/// </summary>
[RequireComponent(typeof(Animation))]
public class LegacyControl : MonoBehaviour {

	/// <summary>已注册的动画数据列表。</summary>
    public LegacyAnimationData[] animations = new LegacyAnimationData[0];
	/// <summary>是否显示调试 GUI。</summary>
    public bool debugMode = false;
	/// <summary>是否由 UFE 引擎驱动动画更新（手动采样）。</summary>
    public bool overrideAnimatorUpdate = false;
	/// <summary>Unity Animation 组件。</summary>
    public Animation animator;

    #region trackable definitions
	/// <summary>当前动画数据（Inspector 隐藏）。</summary>
    [HideInInspector] public LegacyAnimationData currentAnimationData;
	/// <summary>当前是否镜像（Inspector 隐藏）。</summary>
	[HideInInspector] public bool currentMirror;
	/// <summary>全局播放速度（Inspector 隐藏）。</summary>
	[HideInInspector] public Fix64 globalSpeed = 1;
	/// <summary>上一帧位置（Inspector 隐藏，用于位移计算）。</summary>
	[HideInInspector] public Vector3 lastPosition;
	/// <summary>累积位移增量。</summary>
    public Vector3 deltaDisplacement;
    #endregion

	/// <summary>
	/// 唤醒：获取 Animation 组件并记录初始位置。
	/// </summary>
    void Awake() {
        animator = gameObject.GetComponent<Animation>();
        lastPosition = transform.position;
    }

	/// <summary>
	/// 启动：选择第一个动画为当前动画；引擎驱动模式下将所有片段速度置 0。
	/// </summary>
    void Start() {
        if (animations[0] == null) Debug.LogWarning("No animation found!");
        currentAnimationData = animations[0];

        if (overrideAnimatorUpdate) {
            foreach (AnimationState animState in animator) {
                animState.speed = 0;
            }
        }
    }

	/// <summary>
	/// 固定帧更新：引擎驱动模式下手动推进当前动画时间并采样。
	/// </summary>
    public void DoFixedUpdate() {
		if (animator == null || currentAnimationData == null || !animator.isPlaying || !overrideAnimatorUpdate) return;
        
        currentAnimationData.secondsPlayed += (UFE.fixedDeltaTime * globalSpeed);
        currentAnimationData.realSecondsPlayed += UFE.fixedDeltaTime;
        currentAnimationData.animState.time = (float)currentAnimationData.secondsPlayed;
        if (currentAnimationData.secondsPlayed >= currentAnimationData.length && currentAnimationData.clip.wrapMode == WrapMode.Loop) SetCurrentClipPosition(0);
        animator.Sample();
    }

	/// <summary>
	/// 调试 GUI：显示当前动画数据。
	/// </summary>
    void OnGUI() {
        //Toggle debug mode to see the live data in action
        if (debugMode) {
            GUI.Box(new Rect(Screen.width - 340, 40, 340, 300), "Animation Data");
            GUI.BeginGroup(new Rect(Screen.width - 330, 60, 400, 300));
            {
                GUILayout.Label("Global Speed: " + globalSpeed);
                GUILayout.Label("Current Animation Data");
                GUILayout.Label("-Clip Name: " + currentAnimationData.clipName);
                GUILayout.Label("-Speed: " + currentAnimationData.speed);
                GUILayout.Label("-Normalized Speed: " + currentAnimationData.normalizedSpeed);
                GUILayout.Label("Animation State");
                GUILayout.Label("-Time: " + currentAnimationData.animState.time);
                GUILayout.Label("-Normalized Time: " + currentAnimationData.animState.normalizedTime);
                GUILayout.Label("-Lengh: " + currentAnimationData.animState.length);
                GUILayout.Label("-Speed: " + currentAnimationData.animState.speed);
            } GUI.EndGroup();
        }
    }



    // LEGACY CONTROL METHODS
	/// <summary>
	/// 按注册名移除动画。
	/// </summary>
	/// <param name="name">动画注册名。</param>
    public void RemoveClip(string name) {
        List<LegacyAnimationData> animationDataList = new List<LegacyAnimationData>(animations);
        animationDataList.Remove(GetAnimationData(name));
        animations = animationDataList.ToArray();
    }

	/// <summary>
	/// 按片段移除动画。
	/// </summary>
	/// <param name="clip">动画片段。</param>
    public void RemoveClip(AnimationClip clip) {
        List<LegacyAnimationData> animationDataList = new List<LegacyAnimationData>(animations);
        animationDataList.Remove(GetAnimationData(clip));
        animations = animationDataList.ToArray();
    }

	/// <summary>
	/// 移除全部动画。
	/// </summary>
    public void RemoveAllClips() {
        animations = new LegacyAnimationData[0];
    }

	/// <summary>
	/// 注册动画片段（使用组件默认播放模式）。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <param name="newName">注册名。</param>
    public void AddClip(AnimationClip clip, string newName) {
        AddClip(clip, newName, 1, animator.wrapMode);
    }

	/// <summary>
	/// 注册动画片段（指定速度与播放模式）。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <param name="newName">注册名。</param>
	/// <param name="speed">播放速度。</param>
	/// <param name="wrapMode">播放模式。</param>
    public void AddClip(AnimationClip clip, string newName, Fix64 speed, WrapMode wrapMode) {
        AddClip(clip, newName, speed, wrapMode, clip.length);
    }

	/// <summary>
	/// 注册动画片段（完整参数）：克隆片段、设置速度/模式/时长并添加到 Animation 组件。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <param name="newName">注册名。</param>
	/// <param name="speed">播放速度。</param>
	/// <param name="wrapMode">播放模式。</param>
	/// <param name="length">片段时长。</param>
    public void AddClip(AnimationClip clip, string newName, Fix64 speed, WrapMode wrapMode, Fix64 length) {
        if (GetAnimationData(newName) != null) Debug.LogWarning("An animation with the name '" + newName + "' already exists.");
        LegacyAnimationData animData = new LegacyAnimationData();
        animData.clip = (AnimationClip)Instantiate(clip);
        if (wrapMode == WrapMode.Default) wrapMode = animator.wrapMode;
        animData.clip.wrapMode = wrapMode;
        animData.clip.name = newName;
        animData.clipName = newName;
        animData.speed = speed;
        animData.originalSpeed = speed;
        animData.length = length;
        animData.wrapMode = wrapMode;

        List<LegacyAnimationData> animationDataList = new List<LegacyAnimationData>(animations);
        animationDataList.Add(animData);
        animations = animationDataList.ToArray();

        animator.AddClip(clip, newName);
        animator[newName].speed = (float)speed;
        animator[newName].wrapMode = wrapMode;


        foreach (AnimationState animState in animator) {
            if (animState.name == newName) animData.animState = animState;
        }
    }

	/// <summary>
	/// 按注册名获取动画数据。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <returns>动画数据；未找到返回 null。</returns>
    public LegacyAnimationData GetAnimationData(string clipName) {
        foreach (LegacyAnimationData animData in animations) {
            if (animData.clipName == clipName) {
                return animData;
            }
        }
        return null;
    }

	/// <summary>
	/// 按片段获取动画数据。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <returns>动画数据；未找到返回 null。</returns>
    public LegacyAnimationData GetAnimationData(AnimationClip clip) {
        foreach (LegacyAnimationData animData in animations) {
            if (animData.clip == clip) {
                return animData;
            }
        }
        return null;
    }

	/// <summary>
	/// 判断指定动画是否在播放（ClampForever 片段视为常驻播放）。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <returns>播放中返回 true。</returns>
    public bool IsPlaying(string clipName) {
        if (currentAnimationData == GetAnimationData(clipName) && currentAnimationData.wrapMode == WrapMode.ClampForever) return true;
        return (animator.IsPlaying(clipName));
    }

	/// <summary>
	/// 判断指定动画数据是否当前播放。
	/// </summary>
	/// <param name="animData">动画数据。</param>
	/// <returns>播放中返回 true。</returns>
    public bool IsPlaying(LegacyAnimationData animData) {
        return (currentAnimationData == animData);
    }

	/// <summary>
	/// 按注册名播放动画。
	/// </summary>
	/// <param name="animationName">注册名。</param>
	/// <param name="blendingTime">融合时间。</param>
	/// <param name="normalizedTime">起始归一化时间。</param>
    public void Play(string animationName, Fix64 blendingTime, Fix64 normalizedTime) {
        Play(GetAnimationData(animationName), blendingTime, normalizedTime);
    }

	/// <summary>
	/// 播放第一个动画。
	/// </summary>
    public void Play() {
        if (animations[0] == null) {
            Debug.LogError("No animation found.");
            return;
        }
        Play(animations[0], 0, 0);
    }

	/// <summary>
	/// 播放指定动画：切换当前动画、按融合时间播放/交叉淡化并设置起始位置。
	/// </summary>
	/// <param name="animData">动画数据。</param>
	/// <param name="blendingTime">融合时间。</param>
	/// <param name="normalizedTime">起始归一化时间。</param>
    public void Play(LegacyAnimationData animData, Fix64 blendingTime, Fix64 normalizedTime) {
        if (animData == null) return;

        if (currentAnimationData != null) {
            currentAnimationData.speed = currentAnimationData.originalSpeed;
            currentAnimationData.normalizedSpeed = 1;
        }

        currentAnimationData = animData;

        if (blendingTime == 0 || 
            ((UFE.isConnected || UFE.config.debugOptions.emulateNetwork) && UFE.config.networkOptions.disableBlending)) {
            animator.Play(currentAnimationData.clipName);
        } else {
            animator.CrossFade(currentAnimationData.clipName, (float)blendingTime);
        }
        
        SetSpeed(currentAnimationData.speed);
        deltaDisplacement = new Vector3();

        SetCurrentClipPosition(normalizedTime);
    }

	/// <summary>
	/// 设置当前动画播放位置（归一化时间）。
	/// </summary>
	/// <param name="normalizedTime">归一化时间。</param>
    public void SetCurrentClipPosition(Fix64 normalizedTime) {
        SetCurrentClipPosition(normalizedTime, false);
    }

	/// <summary>
	/// 设置当前动画播放位置（可暂停）。
	/// </summary>
	/// <param name="normalizedTime">归一化时间。</param>
	/// <param name="pause">是否暂停。</param>
    public void SetCurrentClipPosition(Fix64 normalizedTime, bool pause) {
        normalizedTime = FPMath.Clamp(normalizedTime, 0, 1);
        currentAnimationData.secondsPlayed = normalizedTime * currentAnimationData.length;
        currentAnimationData.normalizedTime = normalizedTime;
        currentAnimationData.animState.normalizedTime = (float)normalizedTime;
        animator.Sample();

        if (pause) Pause();
    }

	/// <summary>
	/// 获取当前动画归一化播放位置。
	/// </summary>
	/// <returns>归一化时间。</returns>
    public Fix64 GetCurrentClipPosition() {
        return currentAnimationData.animState.normalizedTime;
    }
	
	/// <summary>
	/// 获取当前动画播放秒数。
	/// </summary>
	/// <param name="realSeconds">true 返回真实秒数，false 返回游戏时间秒数。</param>
	/// <returns>播放秒数。</returns>
	public Fix64 GetCurrentClipTime(bool realSeconds = false){
        if (realSeconds) return currentAnimationData.realSecondsPlayed;
        return currentAnimationData.secondsPlayed;
	}
	
	/// <summary>
	/// 获取当前播放帧号。
	/// </summary>
	/// <returns>帧号。</returns>
	public int GetCurrentClipFrame(){
        return (int)FPLibrary.FPMath.Round(currentAnimationData.animState.time * UFE.config.fps);
    }

	/// <summary>
	/// 获取当前动画注册名。
	/// </summary>
	/// <returns>注册名；无当前动画返回 null。</returns>
    public string GetCurrentClipName() {
        if (currentAnimationData == null) return null;
        return currentAnimationData.clipName;
    }
    
	/// <summary>
	/// 获取累积位移增量（根位移）。
	/// </summary>
	/// <returns>累积位移。</returns>
    public Vector3 GetDeltaDisplacement() {
        deltaDisplacement += GetDeltaPosition();
        return deltaDisplacement;
    }

	/// <summary>
	/// 获取本帧位移增量。
	/// </summary>
	/// <returns>本帧位移。</returns>
    public Vector3 GetDeltaPosition() {
        Vector3 deltaPosition = transform.position - lastPosition;
        lastPosition = transform.position;
        return deltaPosition;
    }

	/// <summary>
	/// 停止全部动画。
	/// </summary>
    public void Stop() {
        animator.Stop();
    }

	/// <summary>
	/// 停止指定动画。
	/// </summary>
	/// <param name="animName">注册名。</param>
    public void Stop(string animName) {
        animator.Stop(animName);
    }

	/// <summary>
	/// 暂停动画（全局速度置 0）。
	/// </summary>
    public void Pause() {
        globalSpeed = 0;
    }

	/// <summary>
	/// 按片段设置播放速度。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <param name="speed">播放速度。</param>
    public void SetSpeed(AnimationClip clip, Fix64 speed) {
        SetSpeed(GetAnimationData(clip), speed);
    }

	/// <summary>
	/// 按注册名设置播放速度。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <param name="speed">播放速度。</param>
    public void SetSpeed(string clipName, Fix64 speed) {
        SetSpeed(GetAnimationData(clipName), speed);
    }

	/// <summary>
	/// 按动画数据设置播放速度（同步归一化速度，播放中则立即应用）。
	/// </summary>
	/// <param name="animData">动画数据。</param>
	/// <param name="speed">播放速度。</param>
    public void SetSpeed(LegacyAnimationData animData, Fix64 speed) {
        if (animData != null) {
            animData.speed = speed;
            animData.normalizedSpeed = speed / animData.originalSpeed;
            if (IsPlaying(animData)) SetSpeed(speed);
        }
    }

	/// <summary>
	/// 设置全局播放速度。
	/// </summary>
	/// <param name="speed">播放速度。</param>
    public void SetSpeed(Fix64 speed) {
        globalSpeed = speed;

        if (!overrideAnimatorUpdate) {
			foreach(AnimationState animState in animator) {
                animState.speed = (float)speed;
            }
        }
    }

	/// <summary>
	/// 按片段设置归一化速度。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <param name="normalizedSpeed">归一化速度。</param>
    public void SetNormalizedSpeed(AnimationClip clip, Fix64 normalizedSpeed) {
        SetNormalizedSpeed(GetAnimationData(clip), normalizedSpeed);
    }

	/// <summary>
	/// 按注册名设置归一化速度。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <param name="normalizedSpeed">归一化速度。</param>
    public void SetNormalizedSpeed(string clipName, Fix64 normalizedSpeed) {
        SetNormalizedSpeed(GetAnimationData(clipName), normalizedSpeed);
    }

	/// <summary>
	/// 按动画数据设置归一化速度（速度 = 原始速度 × 归一化速度）。
	/// </summary>
	/// <param name="animData">动画数据。</param>
	/// <param name="normalizedSpeed">归一化速度。</param>
    public void SetNormalizedSpeed(LegacyAnimationData animData, Fix64 normalizedSpeed) {
        animData.normalizedSpeed = normalizedSpeed;
        animData.speed = animData.originalSpeed * animData.normalizedSpeed;
        if (IsPlaying(animData)) SetSpeed(animData.speed);
    }

	/// <summary>
	/// 按片段获取播放速度。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <returns>播放速度。</returns>
    public Fix64 GetSpeed(AnimationClip clip) {
        return GetSpeed(GetAnimationData(clip));
    }

	/// <summary>
	/// 按注册名获取播放速度。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <returns>播放速度。</returns>
    public Fix64 GetSpeed(string clipName) {
        return GetSpeed(GetAnimationData(clipName));
    }

	/// <summary>
	/// 按动画数据获取播放速度。
	/// </summary>
	/// <param name="animData">动画数据。</param>
	/// <returns>播放速度。</returns>
    public Fix64 GetSpeed(LegacyAnimationData animData) {
        return animData.speed;
    }

	/// <summary>
	/// 获取全局播放速度。
	/// </summary>
	/// <returns>全局速度。</returns>
    public Fix64 GetSpeed() {
        return globalSpeed;
    }

	/// <summary>
	/// 按片段获取归一化速度。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <returns>归一化速度。</returns>
    public Fix64 GetNormalizedSpeed(AnimationClip clip) {
        return GetNormalizedSpeed(GetAnimationData(clip));
    }

	/// <summary>
	/// 按注册名获取归一化速度。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <returns>归一化速度。</returns>
    public Fix64 GetNormalizedSpeed(string clipName) {
        return GetNormalizedSpeed(GetAnimationData(clipName));
    }

	/// <summary>
	/// 按动画数据获取归一化速度。
	/// </summary>
	/// <param name="animData">动画数据。</param>
	/// <returns>归一化速度。</returns>
    public Fix64 GetNormalizedSpeed(LegacyAnimationData animData) {
        return animData.normalizedSpeed;
    }

	/// <summary>
	/// 恢复动画播放速度（还原为当前动画数据速度）。
	/// </summary>
    public void RestoreSpeed() {
        SetSpeed(currentAnimationData.speed);

        if (!overrideAnimatorUpdate) {
            foreach (AnimationState animState in animator) {
                animState.speed = (float)GetAnimationData(animState.name).speed;
            }
        }
    }
}
