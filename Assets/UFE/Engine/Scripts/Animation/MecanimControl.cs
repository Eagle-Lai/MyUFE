using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FPLibrary;
//using UnityEngine.Experimental.Director;

/// <summary>
/// Mecanim 动画系统控制（MecanimControl）。
/// <para>用途：封装 Unity Mecanim（Animator）动画系统，为 UFE 提供多片段动画注册、状态切换（正向/反向/镜像）、</para>
/// <para>播放速度控制、融合过渡（CrossFade）、根位移追踪、循环/单次模式仿真与动画事件（开始/结束/循环）驱动。</para>
/// <para>通过 AnimatorOverrideController 动态替换动画片段实现无状态机动画切换。</para>
/// </summary>

/// <summary>
/// Mecanim 动画数据：一个已注册动画片段的数据记录。
/// </summary>
[System.Serializable]
public class MecanimAnimationData {
	/// <summary>动画片段实例。</summary>
	public AnimationClip clip;
	/// <summary>动画注册名。</summary>
	public string clipName;
	/// <summary>播放模式。</summary>
	public WrapMode wrapMode;
	/// <summary>是否应用根骨骼运动。</summary>
    public bool applyRootMotion;
	/// <summary>片段时长（定点数）。</summary>
    public Fix64 length = 1;
	/// <summary>原始播放速度。</summary>
    public Fix64 originalSpeed = 1;

	/// <summary>切换过渡时长（-1 表示使用默认值）。</summary>
    public Fix64 transitionDuration = -1;
	/// <summary>归一化播放速度。</summary>
    public Fix64 normalizedSpeed = 1;
	/// <summary>Animator 状态名。</summary>
    public string stateName;

    #region trackable definitions
	/// <summary>归一化播放时间（运行时跟踪）。</summary>
    public Fix64 normalizedTime = 1;
	/// <summary>已播放秒数（运行时跟踪）。</summary>
    public Fix64 secondsPlayed = 0;
	/// <summary>已播放次数（运行时跟踪）。</summary>
    public int timesPlayed = 0;
	/// <summary>当前播放速度（运行时跟踪）。</summary>
    public Fix64 speed = 1;
    #endregion
}

/// <summary>
/// Mecanim 动画控制脚本（MecanimControl）。
/// </summary>
[RequireComponent (typeof (Animator))]
public class MecanimControl : MonoBehaviour {

	/// <summary>默认动画数据（待机）。</summary>
	public MecanimAnimationData defaultAnimation = new MecanimAnimationData();
	/// <summary>已注册的动画数据列表。</summary>
	public MecanimAnimationData[] animations = new MecanimAnimationData[0];
    
	/// <summary>是否显示调试 GUI。</summary>
	public bool debugMode = false;

	/// <summary>是否由 UFE 引擎驱动动画更新（手动采样）。</summary>
    public bool overrideAnimatorUpdate = false;
	/// <summary>默认过渡时长。</summary>
    public Fix64 defaultTransitionDuration = 0.15;
	/// <summary>默认播放模式。</summary>
	public WrapMode defaultWrapMode = WrapMode.Loop;





	/// <summary>Animator 组件。</summary>
    public Animator animator;

    #region trackable definitions
	/// <summary>运行时动画控制器属性。</summary>
    public RuntimeAnimatorController runtimeAnimatorController { get { return this.animator.runtimeAnimatorController; } set { this.animator.runtimeAnimatorController = value; } }
	/// <summary>是否应用根骨骼运动属性。</summary>
    public bool applyRootMotion { get { return this.animator.applyRootMotion; } set { this.animator.applyRootMotion = value; } }
	/// <summary>动画覆盖控制器（动态替换片段用）。</summary>
    public AnimatorOverrideController overrideController;
	/// <summary>当前动画数据。</summary>
    public MecanimAnimationData currentAnimationData;
	/// <summary>当前是否镜像。</summary>
    public bool currentMirror;
	/// <summary>当前归一化时间。</summary>
    public Fix64 currentNormalizedTime;
	/// <summary>当前 Animator 状态名。</summary>
    public string currentState;
	/// <summary>当前播放速度。</summary>
    public Fix64 currentSpeed;
	/// <summary>累积位移增量。</summary>
    public Vector3 deltaDisplacement;
    #endregion

	/// <summary>上一帧位置（位移计算用）。</summary>
    public Vector3 lastPosition;

	/// <summary>动画事件委托。</summary>
	public delegate void AnimEvent(MecanimAnimationData animationData);
	/// <summary>动画开始事件。</summary>
	public static event AnimEvent OnAnimationBegin;
	/// <summary>动画结束事件。</summary>
	public static event AnimEvent OnAnimationEnd;
	/// <summary>动画循环事件。</summary>
	public static event AnimEvent OnAnimationLoop;


	/// <summary>是否总是播放（单次动画结束后自动回到默认动画）。</summary>
    public bool alwaysPlay = false;
	/// <summary>是否覆盖根骨骼运动设置。</summary>
    public bool overrideRootMotion = false;

	/// <summary>基础运行时动画控制器（MC_Controller 资产）。</summary>
    private RuntimeAnimatorController controller;

	// UNITY METHODS
	/// <summary>
	/// 唤醒：获取 Animator、设置物理更新模式、加载基础动画控制器并规范化已注册动画的播放模式。
	/// </summary>
	void Awake () {
        animator = gameObject.GetComponent<Animator>();
        animator.logWarnings = false;
        animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        controller = (RuntimeAnimatorController)Instantiate(Resources.Load("MC_Controller"));

        foreach (MecanimAnimationData animData in animations) {
			if (animData.wrapMode == WrapMode.Default) animData.wrapMode = defaultWrapMode;
			animData.clip.wrapMode = animData.wrapMode;
		}

	}
	
	/// <summary>
	/// 启动：设置默认动画并创建动画覆盖控制器，播放默认状态。
	/// </summary>
	void Start(){
		if (defaultAnimation.clip == null && animations.Length > 0){
			SetDefaultClip(animations[0].clip, "State1", animations[0].speed, animations[0].wrapMode, false);
		}

        if (defaultAnimation.clip != null && currentAnimationData == null) {
			foreach(MecanimAnimationData animData in animations) {
				if (animData.clip == defaultAnimation.clip)
					defaultAnimation.clip = (AnimationClip) Instantiate(defaultAnimation.clip);
			}
			
			currentAnimationData = defaultAnimation;
			currentAnimationData.stateName = "State1";
			currentAnimationData.length = currentAnimationData.clip.length;

            AnimatorOverrideController overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = controller;
            overrideController["Default"] = currentAnimationData.clip;
            overrideController["State1"] = currentAnimationData.clip;

			animator.runtimeAnimatorController = overrideController;
			animator.Play("State1", 0, 0);

			if (overrideRootMotion) animator.applyRootMotion = currentAnimationData.applyRootMotion;
			SetSpeed(currentAnimationData.speed);
		}
	}
	
	/// <summary>
	/// 固定帧更新：引擎驱动下手动更新 Animator；推进当前动画计时，处理循环/乒乓/单次结束事件。
	/// </summary>
	public void DoFixedUpdate(){
        //WrapMode emulator
        if (overrideAnimatorUpdate) {
            animator.enabled = false;
            animator.Update((float)UFE.fixedDeltaTime);
        }

        if (currentAnimationData == null || currentAnimationData.clip == null) return;
        
        deltaDisplacement += animator.deltaPosition;
        currentAnimationData.secondsPlayed += FPMath.Abs(UFE.fixedDeltaTime * GetSpeed());
        if (currentAnimationData.secondsPlayed > currentAnimationData.length) currentAnimationData.secondsPlayed = currentAnimationData.length;
        currentAnimationData.normalizedTime = currentAnimationData.secondsPlayed / currentAnimationData.length;

        if (currentAnimationData.secondsPlayed == currentAnimationData.length)
        {
            if (currentAnimationData.clip.wrapMode == WrapMode.Loop || currentAnimationData.clip.wrapMode == WrapMode.PingPong) {
				if (MecanimControl.OnAnimationLoop != null) MecanimControl.OnAnimationLoop(currentAnimationData);
				currentAnimationData.timesPlayed ++;
				
				if (currentAnimationData.clip.wrapMode == WrapMode.Loop) {
					SetCurrentClipPosition(0);
				}
				
				if (currentAnimationData.clip.wrapMode == WrapMode.PingPong) {
					SetSpeed(currentAnimationData.clipName, -currentAnimationData.speed);
					SetCurrentClipPosition(0);
				}
				
			}else if (currentAnimationData.timesPlayed == 0) {
				if (MecanimControl.OnAnimationEnd != null) MecanimControl.OnAnimationEnd(currentAnimationData);
				currentAnimationData.timesPlayed = 1;
				
				if ((currentAnimationData.clip.wrapMode == WrapMode.Once ||
                    currentAnimationData.clip.wrapMode == WrapMode.Clamp) 
                    && alwaysPlay) {
					Play(defaultAnimation, currentMirror);
                } else if (!alwaysPlay) {
                    SetSpeed(0);
				}
			}
		}
	}
	
	/// <summary>
	/// 调试 GUI：显示当前动画数据。
	/// </summary>
	void OnGUI(){
		//Toggle debug mode to see the live data in action
		if (debugMode) {
			GUI.Box (new Rect (Screen.width - 340,40,340,420), "Animation Data");
			GUI.BeginGroup(new Rect (Screen.width - 330,60,400,420));{
				
				AnimatorClipInfo[] animationInfoArray = animator.GetCurrentAnimatorClipInfo(0);
				foreach (AnimatorClipInfo animationInfo in animationInfoArray){
					AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
					GUILayout.Label(animationInfo.clip.name);
					GUILayout.Label("-Wrap Mode: "+ animationInfo.clip.wrapMode);
					GUILayout.Label("-Is Playing: "+ IsPlaying(animationInfo.clip));
					GUILayout.Label("-Blend Weight: "+ animationInfo.weight);
					GUILayout.Label("-Normalized Time: "+ animatorStateInfo.normalizedTime);
					GUILayout.Label("-True Length: "+ animationInfo.clip.length);
					GUILayout.Label("----");
				}

                GUILayout.Label("Global Speed: " + GetSpeed().ToString());

				GUILayout.Label("--Current Animation Data--");
                GUILayout.Label("-Clip Name: " + currentAnimationData.clipName);
                GUILayout.Label("-Animation Speed: " + GetSpeed(currentAnimationData).ToString());
				GUILayout.Label("-Normalized Speed: "+ GetNormalizedSpeed(currentAnimationData));
				GUILayout.Label("-Times Played: "+ currentAnimationData.timesPlayed);
				GUILayout.Label("-Seconds Played: "+ currentAnimationData.secondsPlayed);
                GUILayout.Label("-Emulated Length: " + currentAnimationData.length);
                GUILayout.Label("-Normalized Time: " + currentAnimationData.normalizedTime);
			}GUI.EndGroup();
		}
	}
	

	// MECANIM CONTROL METHODS
	/// <summary>
	/// 按注册名移除动画。
	/// </summary>
	/// <param name="name">注册名。</param>
	public void RemoveClip(string name) {
		List<MecanimAnimationData> animationDataList = new List<MecanimAnimationData>(animations);
		animationDataList.Remove(GetAnimationData(name));
		animations = animationDataList.ToArray();
	}

	/// <summary>
	/// 按片段移除动画。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	public void RemoveClip(AnimationClip clip) {
		List<MecanimAnimationData> animationDataList = new List<MecanimAnimationData>(animations);
		animationDataList.Remove(GetAnimationData(clip));
		animations = animationDataList.ToArray();
    }

	/// <summary>
	/// 清空全部已注册动画。
	/// </summary>
    public void Clear() {
        animations = new MecanimAnimationData[0];
    }

	/// <summary>
	/// 设置默认动画（待机片段）。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <param name="name">注册名。</param>
	/// <param name="speed">播放速度。</param>
	/// <param name="wrapMode">播放模式。</param>
	/// <param name="mirror">是否镜像。</param>
    public void SetDefaultClip(AnimationClip clip, string name, Fix64 speed, WrapMode wrapMode, bool mirror) {
		defaultAnimation.clip = (AnimationClip) Instantiate(clip);
		defaultAnimation.clip.wrapMode = wrapMode;
		defaultAnimation.clipName = name;
		defaultAnimation.speed = speed;
		defaultAnimation.originalSpeed = speed;
		defaultAnimation.transitionDuration = -1;
		defaultAnimation.wrapMode = wrapMode;
	}
	
	/// <summary>
	/// 注册动画片段（使用默认播放模式）。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <param name="newName">注册名。</param>
	public void AddClip(AnimationClip clip, string newName) {
		AddClip(clip, newName, 1, defaultWrapMode);
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
	/// 注册动画片段（完整参数）：克隆片段并记录速度/模式/时长。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <param name="newName">注册名。</param>
	/// <param name="speed">播放速度。</param>
	/// <param name="wrapMode">播放模式。</param>
	/// <param name="length">片段时长。</param>
    public void AddClip(AnimationClip clip, string newName, Fix64 speed, WrapMode wrapMode, Fix64 length) {
		if (GetAnimationData(newName) != null) Debug.LogWarning("An animation with the name '"+ newName +"' already exists.");
		MecanimAnimationData animData = new MecanimAnimationData();
		animData.clip = (AnimationClip) Instantiate(clip);
		//if (wrapMode == WrapMode.Default) wrapMode = defaultWrapMode;
		animData.clip.wrapMode = wrapMode;
		animData.clip.name = newName;
		animData.clipName = newName;
        animData.speed = speed;
        animData.originalSpeed = speed;
        animData.length = length;
		animData.wrapMode = wrapMode;

		List<MecanimAnimationData> animationDataList = new List<MecanimAnimationData>(animations);
		animationDataList.Add(animData);
		animations = animationDataList.ToArray();
	}

	/// <summary>
	/// 按注册名获取动画数据。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <returns>动画数据；未找到返回 null。</returns>
	public MecanimAnimationData GetAnimationData(string clipName){
		foreach(MecanimAnimationData animData in animations){
			if (animData.clipName == clipName){
				return animData;
			}
		}
		if (clipName == defaultAnimation.clipName) return defaultAnimation;
		return null;
	}

	/// <summary>
	/// 按片段获取动画数据。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <returns>动画数据；未找到返回 null。</returns>
	public MecanimAnimationData GetAnimationData(AnimationClip clip){
		foreach(MecanimAnimationData animData in animations){
			if (animData.clip == clip){
				return animData;
			}
		}
		if (clip == defaultAnimation.clip) return defaultAnimation;
		return null;
	}

	/// <summary>
	/// 复制动画数据（从 from 到 to，克隆片段）。
	/// </summary>
	/// <param name="from">源数据。</param>
	/// <param name="to">目标数据（引用传递）。</param>
    public void CopyAnimationData(MecanimAnimationData from, ref MecanimAnimationData to) {
        if (from == null || from.clip == null) return;
        to.clip = (AnimationClip)Instantiate(from.clip);
        to.clip.wrapMode = from.clip.wrapMode;
        to.clip.name = from.clip.name;
        to.clipName = from.clipName;
        to.speed = from.speed;
        to.transitionDuration = from.transitionDuration;
        to.wrapMode = from.wrapMode;
        to.applyRootMotion = from.applyRootMotion;
        to.timesPlayed = from.timesPlayed;
        to.secondsPlayed = from.secondsPlayed;
        to.length = from.length;
        to.originalSpeed = from.originalSpeed;
        to.normalizedSpeed = from.normalizedSpeed;
        to.normalizedTime = from.normalizedTime;
        to.stateName = from.stateName;
    }
	
	/// <summary>
	/// 交叉淡化到指定动画（当前镜像状态）。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <param name="blendingTime">融合时间。</param>
	public void CrossFade(string clipName, Fix64 blendingTime){
		CrossFade(clipName, blendingTime, 0, currentMirror);
	}

	/// <summary>
	/// 交叉淡化到指定动画（完整参数）。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <param name="blendingTime">融合时间。</param>
	/// <param name="normalizedTime">起始归一化时间。</param>
	/// <param name="mirror">是否镜像。</param>
    public void CrossFade(string clipName, Fix64 blendingTime, Fix64 normalizedTime, bool mirror) {
        Play(GetAnimationData(clipName), blendingTime, normalizedTime, mirror);
	}

	/// <summary>
	/// 交叉淡化到指定动画数据。
	/// </summary>
	/// <param name="animationData">动画数据。</param>
	/// <param name="blendingTime">融合时间。</param>
	/// <param name="normalizedTime">起始归一化时间。</param>
	/// <param name="mirror">是否镜像。</param>
    public void CrossFade(MecanimAnimationData animationData, Fix64 blendingTime, Fix64 normalizedTime, bool mirror) {
        Play(animationData, blendingTime, normalizedTime, mirror);
	}

	/// <summary>
	/// 播放指定动画（按注册名）。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <param name="blendingTime">融合时间。</param>
	/// <param name="normalizedTime">起始归一化时间。</param>
	/// <param name="mirror">是否镜像。</param>
    public void Play(string clipName, Fix64 blendingTime, Fix64 normalizedTime, bool mirror) {
        Play(GetAnimationData(clipName), blendingTime, normalizedTime, mirror);
	}

	/// <summary>
	/// 播放指定动画（按片段）。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <param name="blendingTime">融合时间。</param>
	/// <param name="normalizedTime">起始归一化时间。</param>
	/// <param name="mirror">是否镜像。</param>
    public void Play(AnimationClip clip, Fix64 blendingTime, Fix64 normalizedTime, bool mirror) {
        Play(GetAnimationData(clip), blendingTime, normalizedTime, mirror);
	}

	/// <summary>
	/// 播放指定动画（按注册名，指定镜像）。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <param name="mirror">是否镜像。</param>
	public void Play(string clipName, bool mirror){
        Play(GetAnimationData(clipName), 0, 0, mirror);
	}

	/// <summary>
	/// 播放指定动画（按注册名，当前镜像状态）。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	public void Play(string clipName){
        Play(GetAnimationData(clipName), 0, 0, currentMirror);
	}
	
	/// <summary>
	/// 播放指定动画（按片段，指定镜像）。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <param name="mirror">是否镜像。</param>
	public void Play(AnimationClip clip, bool mirror){
        Play(GetAnimationData(clip), 0, 0, mirror);
	}

	/// <summary>
	/// 播放指定动画（按片段，当前镜像状态）。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	public void Play(AnimationClip clip){
        Play(GetAnimationData(clip), 0, 0, currentMirror);
	}

	/// <summary>
	/// 播放指定动画数据（使用数据配置的过渡时长）。
	/// </summary>
	/// <param name="animationData">动画数据。</param>
	/// <param name="mirror">是否镜像。</param>
	public void Play(MecanimAnimationData animationData, bool mirror){
        Play(animationData, animationData.transitionDuration, 0, mirror);
	}

	/// <summary>
	/// 播放指定动画数据（当前镜像状态）。
	/// </summary>
	/// <param name="animationData">动画数据。</param>
	public void Play(MecanimAnimationData animationData){
        Play(animationData, animationData.transitionDuration, 0, currentMirror);
	}

	/// <summary>
	/// 播放指定动画数据（完整参数）。
	/// </summary>
	/// <param name="animationData">动画数据。</param>
	/// <param name="blendingTime">融合时间。</param>
	/// <param name="normalizedTime">起始归一化时间。</param>
	/// <param name="mirror">是否镜像。</param>
    public void Play(MecanimAnimationData animationData, Fix64 blendingTime, Fix64 normalizedTime, bool mirror) {
		_playAnimation(animationData, blendingTime, normalizedTime, mirror);
        //DirectorPlay(animationData, blendingTime, normalizedTime, mirror);
	}

	/// <summary>
	/// 恢复当前动画播放速度。
	/// </summary>
	public void Play(){
        SetSpeed(currentAnimationData.speed);
	}

	/// <summary>
	/// 刷新动画：重播当前状态到记录的归一化时间并恢复速度（帧同步回滚用）。
	/// </summary>
    public void Refresh()
    {
        //overrideController.runtimeAnimatorController = controller;
        //animator.runtimeAnimatorController = overrideController;
        animator.Play(currentState, 0, (float)currentAnimationData.normalizedTime);
        animator.applyRootMotion = currentAnimationData.applyRootMotion;
        animator.Update(0);
        SetSpeed(currentSpeed);
    }

	/// <summary>
	/// 实际播放动画：通过 AnimatorOverrideController 替换状态片段，
	/// 根据镜像/正反方向选择 State1~4 状态，支持融合过渡与根骨骼运动/速度设置，触发动画开始事件。
	/// </summary>
	/// <param name="targetAnimationData">目标动画数据。</param>
	/// <param name="blendingTime">融合时间（-1 使用数据或默认值）。</param>
	/// <param name="normalizedTime">起始归一化时间。</param>
	/// <param name="mirror">是否镜像。</param>
    private void _playAnimation(MecanimAnimationData targetAnimationData, Fix64 blendingTime, Fix64 normalizedTime, bool mirror) {
		//The overrite machine. Creates an overrideController, replace its core animations and restate it back in
		if (targetAnimationData == null || targetAnimationData.clip == null) return;

        bool prevMirror = currentMirror;
        currentMirror = mirror;

        Fix64 animSpeed = targetAnimationData.originalSpeed * (targetAnimationData.originalSpeed < 0? - 1 : 1);

		currentNormalizedTime = GetCurrentClipPosition();
        currentState = "State1";

        if (!mirror){
            if (targetAnimationData.originalSpeed >= 0){
                currentState = "State1";
			}else{
                currentState = "State2";
			}
		}else{
            if (targetAnimationData.originalSpeed >= 0){
                currentState = "State3";
			}else{
                currentState = "State4";
			}
		}

        overrideController = new AnimatorOverrideController();
        overrideController.runtimeAnimatorController = controller;

        if (currentAnimationData != null && currentAnimationData.clip != null) 
            overrideController["Default"] = currentAnimationData.clip;

        overrideController[currentState] = targetAnimationData.clip;

        if (blendingTime == -1) blendingTime = currentAnimationData.transitionDuration;
        if (blendingTime == -1) blendingTime = defaultTransitionDuration;

        if (blendingTime <= 0 || currentAnimationData == null) {
			animator.runtimeAnimatorController = overrideController;
            animator.Play(currentState, 0, (float)normalizedTime);
		}else{
			animator.runtimeAnimatorController = overrideController;
            
			currentAnimationData.stateName = "Default";
            SetCurrentClipPosition(currentNormalizedTime);

            animator.Play("Default", 0, (float)normalizedTime);
            animator.CrossFade(currentState, (float)(blendingTime / animSpeed), 0, (float)normalizedTime);
        }

        // Update Previous Mirror
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("Default")) {
            if (animator.GetBool("Mirror") != prevMirror) {
                animator.SetBool("Mirror", prevMirror);
            }
        }
        animator.Update(0);
        deltaDisplacement = new Vector3();

        targetAnimationData.timesPlayed = 0;
        targetAnimationData.secondsPlayed = (normalizedTime * targetAnimationData.length) / animSpeed;
        //targetAnimationData.secondsPlayed = normalizedTime * targetAnimationData.length;
        targetAnimationData.normalizedTime = normalizedTime;
        targetAnimationData.speed = targetAnimationData.originalSpeed;

        if (overrideRootMotion) animator.applyRootMotion = targetAnimationData.applyRootMotion;
        SetSpeed(targetAnimationData.originalSpeed);
		
        if (currentAnimationData != null) {
            currentAnimationData.speed = currentAnimationData.originalSpeed;
            currentAnimationData.normalizedSpeed = 1;
            currentAnimationData.timesPlayed = 0;
        }

		currentAnimationData = targetAnimationData;
        currentAnimationData.stateName = currentState;

		if (MecanimControl.OnAnimationBegin != null) MecanimControl.OnAnimationBegin(currentAnimationData);
	}
	
	/// <summary>
	/// 判断指定动画是否在播放（按注册名）。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <returns>播放中返回 true。</returns>
	public bool IsPlaying(string clipName){
		return IsPlaying(GetAnimationData(clipName));
	}
	
	/// <summary>
	/// 判断指定动画是否在播放（按注册名，指定混合权重阈值）。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <param name="weight">混合权重阈值。</param>
	/// <returns>播放中返回 true。</returns>
	public bool IsPlaying(string clipName, float weight){
		return IsPlaying(GetAnimationData(clipName), weight);
	}
	
	/// <summary>
	/// 判断指定动画是否在播放（按片段）。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <returns>播放中返回 true。</returns>
	public bool IsPlaying(AnimationClip clip){
		return IsPlaying(GetAnimationData(clip));
	}
	
	/// <summary>
	/// 判断指定动画是否在播放（按片段，指定混合权重阈值）。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <param name="weight">混合权重阈值。</param>
	/// <returns>播放中返回 true。</returns>
	public bool IsPlaying(AnimationClip clip, float weight){
		return IsPlaying(GetAnimationData(clip), weight);
	}
	
	/// <summary>
	/// 判断指定动画数据是否在播放（考虑播放模式与混合权重）。
	/// </summary>
	/// <param name="animData">动画数据。</param>
	/// <param name="weight">混合权重阈值（默认 1）。</param>
	/// <returns>播放中返回 true。</returns>
	public bool IsPlaying(MecanimAnimationData animData, float weight = 1){
		if (animData == null) return false;
		if (currentAnimationData == null) return false;
		if (currentAnimationData == animData && animData.wrapMode == WrapMode.Once && animData.timesPlayed > 0) return false;
        if (currentAnimationData == animData && animData.wrapMode == WrapMode.Clamp && animData.timesPlayed > 0) return false;
		if (currentAnimationData == animData && animData.wrapMode == WrapMode.ClampForever) return true;
		if (currentAnimationData == animData) return true;

		AnimatorClipInfo[] animationInfoArray = animator.GetCurrentAnimatorClipInfo(0);
		foreach (AnimatorClipInfo animationInfo in animationInfoArray){
			if (animData.clip == animationInfo.clip && animationInfo.weight >= weight) return true;
		}
		return false;
	}
	
	/// <summary>
	/// 获取当前动画注册名。
	/// </summary>
	/// <returns>注册名。</returns>
	public string GetCurrentClipName(){
		return currentAnimationData.clipName;
	}
	
	/// <summary>
	/// 获取当前动画数据。
	/// </summary>
	/// <returns>当前动画数据。</returns>
	public MecanimAnimationData GetCurrentAnimationData(){
		return currentAnimationData;
	}
	
	/// <summary>
	/// 获取当前动画已播放次数。
	/// </summary>
	/// <returns>播放次数。</returns>
	public int GetCurrentClipPlayCount(){
		return currentAnimationData.timesPlayed;
	}
	
	/// <summary>
	/// 获取当前动画播放秒数。
	/// </summary>
	/// <param name="realSeconds">true 按真实秒（除以速度），false 按游戏秒。</param>
	/// <returns>播放秒数。</returns>
	public Fix64 GetCurrentClipTime(bool realSeconds = false){
        if (realSeconds) return currentAnimationData.secondsPlayed / currentAnimationData.speed;
        return currentAnimationData.secondsPlayed;
    }

	/// <summary>
	/// 获取当前动画归一化时间。
	/// </summary>
	/// <returns>归一化时间。</returns>
    public Fix64 GetCurrentClipNormalizedTime() {
        return currentAnimationData.normalizedTime;
    }

	/// <summary>
	/// 获取当前动画时长。
	/// </summary>
	/// <returns>动画时长。</returns>
    public Fix64 GetCurrentClipLength() {
		return currentAnimationData.length;
	}

	/// <summary>
	/// 获取累积位移增量。
	/// </summary>
	/// <returns>累积位移。</returns>
    public Vector3 GetDeltaDisplacement() {
        return deltaDisplacement;
    }

	/// <summary>
	/// 获取本帧位移增量（网络禁用根骨骼运动时返回零）。
	/// </summary>
	/// <returns>本帧位移。</returns>
    public Vector3 GetDeltaPosition() {
        if ((UFE.isConnected || UFE.config.debugOptions.emulateNetwork) 
            && UFE.config.networkOptions.disableRootMotion) return new Vector3();
        return animator.deltaPosition;
    }

	/// <summary>
	/// 设置当前动画播放位置（归一化时间）。
	/// </summary>
	/// <param name="normalizedTime">归一化时间。</param>
	public void SetCurrentClipPosition(Fix64 normalizedTime){
		SetCurrentClipPosition(normalizedTime, false);
	}

	/// <summary>
	/// 设置当前动画播放位置（可暂停）。
	/// </summary>
	/// <param name="normalizedTime">归一化时间。</param>
	/// <param name="pause">是否暂停。</param>
    public void SetCurrentClipPosition(Fix64 normalizedTime, bool pause) {
        if (normalizedTime > 1) normalizedTime = 1;
        if (normalizedTime < 0) normalizedTime = 0;

        currentAnimationData.secondsPlayed = normalizedTime * currentAnimationData.length;
        currentAnimationData.normalizedTime = normalizedTime;

        animator.Play(currentAnimationData.stateName, 0, (float)normalizedTime);
        animator.Update(0);

        if (pause) Pause();
    }

	/// <summary>
	/// 获取当前动画归一化播放位置。
	/// </summary>
	/// <returns>归一化时间。</returns>
    public Fix64 GetCurrentClipPosition() {
        if (currentAnimationData == null) return 0;
		return currentAnimationData.secondsPlayed/currentAnimationData.length;
	}
	
	/// <summary>
	/// 停止当前动画（回到默认动画）。
	/// </summary>
	public void Stop(){
		Play(defaultAnimation.clip, defaultTransitionDuration, 0, currentMirror);
	}

	/// <summary>
	/// 暂停动画（速度置 0）。
	/// </summary>
    public void Pause() {
        SetSpeed(0);
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
	/// 按动画数据设置播放速度（同步归一化速度，播放中立即应用）。
	/// </summary>
	/// <param name="animData">动画数据。</param>
	/// <param name="speed">播放速度。</param>
    public void SetSpeed(MecanimAnimationData animData, Fix64 speed) {
        if (animData != null) {
            animData.normalizedSpeed = speed / animData.originalSpeed;

            animData.speed = speed;
            if (IsPlaying(animData)) SetSpeed(speed);
        }
    }

	/// <summary>
	/// 设置全局播放速度。
	/// </summary>
	/// <param name="speed">播放速度。</param>
    public void SetSpeed(Fix64 speed) {
        animator.speed = Mathf.Abs((float)speed);
        currentSpeed = speed;
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
    public void SetNormalizedSpeed(MecanimAnimationData animData, Fix64 normalizedSpeed) {
        if (animData == null) return;
        animData.normalizedSpeed = normalizedSpeed;
        animData.speed = animData.originalSpeed * animData.normalizedSpeed;
        if (IsPlaying(animData)) SetSpeed(animData.speed);
    }
	
	/// <summary>
	/// 恢复当前动画播放速度。
	/// </summary>
	public void RestoreSpeed(){
		SetSpeed(currentAnimationData.speed);
	}
	
	/// <summary>
	/// 倒放（速度取反）。
	/// </summary>
	public void Rewind(){
		SetSpeed(-currentAnimationData.speed);
	}

	/// <summary>
	/// 设置默认播放模式。
	/// </summary>
	/// <param name="wrapMode">播放模式。</param>
	public void SetWrapMode(WrapMode wrapMode){
		defaultWrapMode = wrapMode;
	}
	
	/// <summary>
	/// 按动画数据设置播放模式。
	/// </summary>
	/// <param name="animationData">动画数据。</param>
	/// <param name="wrapMode">播放模式。</param>
	public void SetWrapMode(MecanimAnimationData animationData, WrapMode wrapMode){
		animationData.wrapMode = wrapMode;
		animationData.clip.wrapMode = wrapMode;
	}

	/// <summary>
	/// 按片段设置播放模式。
	/// </summary>
	/// <param name="clip">动画片段。</param>
	/// <param name="wrapMode">播放模式。</param>
	public void SetWrapMode(AnimationClip clip, WrapMode wrapMode){
		MecanimAnimationData animData = GetAnimationData(clip);
		animData.wrapMode = wrapMode;
		animData.clip.wrapMode = wrapMode;
	}

	/// <summary>
	/// 按注册名设置播放模式。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <param name="wrapMode">播放模式。</param>
	public void SetWrapMode(string clipName, WrapMode wrapMode){
		MecanimAnimationData animData = GetAnimationData(clipName);
		animData.wrapMode = wrapMode;
		animData.clip.wrapMode = wrapMode;
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
	/// 按注册名获取原始播放速度。
	/// </summary>
	/// <param name="clipName">注册名。</param>
	/// <returns>原始速度。</returns>
    public Fix64 GetOriginalSpeed(string clipName) {
        return GetAnimationData(clipName).originalSpeed;
    }

	/// <summary>
	/// 按动画数据获取播放速度。
	/// </summary>
	/// <param name="animData">动画数据。</param>
	/// <returns>播放速度。</returns>
    public Fix64 GetSpeed(MecanimAnimationData animData) {
        return animData.speed;
    }

	/// <summary>
	/// 获取全局播放速度。
	/// </summary>
	/// <returns>全局速度。</returns>
    public Fix64 GetSpeed() {
		return currentSpeed;
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
    public Fix64 GetNormalizedSpeed(MecanimAnimationData animData) {
        return animData.normalizedSpeed;
    }
	
	/// <summary>
	/// 获取当前是否镜像。
	/// </summary>
	/// <returns>镜像状态。</returns>
	public bool GetMirror(){
		return currentMirror;
	}

	/// <summary>
	/// 设置镜像（无融合）。
	/// </summary>
	/// <param name="toggle">是否镜像。</param>
	public void SetMirror(bool toggle){
		SetMirror(toggle, 0, false);
	}

	/// <summary>
	/// 设置镜像（指定融合时间）。
	/// </summary>
	/// <param name="toggle">是否镜像。</param>
	/// <param name="blendingTime">融合时间。</param>
    public void SetMirror(bool toggle, Fix64 blendingTime) {
		SetMirror(toggle, blendingTime, false);
	}

	/// <summary>
	/// 设置镜像（完整参数）：镜像变化时重播当前动画到对应镜像状态。
	/// </summary>
	/// <param name="toggle">是否镜像。</param>
	/// <param name="blendingTime">融合时间。</param>
	/// <param name="forceMirror">是否强制重播。</param>
    public void SetMirror(bool toggle, Fix64 blendingTime, bool forceMirror) {
		if (currentMirror == toggle && !forceMirror) return;
		
		if (blendingTime == 0) blendingTime = defaultTransitionDuration;
		_playAnimation(currentAnimationData, blendingTime, GetCurrentClipPosition(), toggle);
	}
}
