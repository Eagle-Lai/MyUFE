using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FPLibrary;
using UFENetcode;
using UFE3D;

/// <summary>
/// 招式集合脚本（MoveSetScript）。
/// <para>用途：管理角色的全部动画与招式数据——加载基础动作/攻击招式、切换战斗姿态（ChangeMoveStances）、</para>
/// <para>驱动动画播放（Legacy/Mecanim 双系统）、执行输入指令匹配（必杀技判定）、招式条件校验与实例化。</para>
/// </summary>

/// <summary>
/// 基础动作集合（BasicMoves）：角色全部基础状态动作（待机/移动/跳跃/受击/倒地/格挡/弹反等）的容器。
/// </summary>
[System.Serializable]
public class BasicMoves:ICloneable {
	/// <summary>待机动作。</summary>
	public BasicMoveInfo idle = new BasicMoveInfo();
	/// <summary>前进动作。</summary>
	public BasicMoveInfo moveForward = new BasicMoveInfo();
	/// <summary>后退动作。</summary>
	public BasicMoveInfo moveBack = new BasicMoveInfo();
	/// <summary>下蹲动作。</summary>
    public BasicMoveInfo crouching = new BasicMoveInfo();
	/// <summary>起跳动作。</summary>
    public BasicMoveInfo takeOff = new BasicMoveInfo();
	/// <summary>垂直跳跃动作。</summary>
	public BasicMoveInfo jumpStraight = new BasicMoveInfo();
	/// <summary>向后跳跃动作。</summary>
	public BasicMoveInfo jumpBack = new BasicMoveInfo();
	/// <summary>向前跳跃动作。</summary>
	public BasicMoveInfo jumpForward = new BasicMoveInfo();
	/// <summary>垂直下落动作。</summary>
	public BasicMoveInfo fallStraight = new BasicMoveInfo();
	/// <summary>向后下落动作。</summary>
	public BasicMoveInfo fallBack = new BasicMoveInfo();
	/// <summary>向前下落动作。</summary>
	public BasicMoveInfo fallForward = new BasicMoveInfo();
	/// <summary>落地动作。</summary>
	public BasicMoveInfo landing = new BasicMoveInfo();
	/// <summary>下蹲格挡姿态动作。</summary>
	public BasicMoveInfo blockingCrouchingPose = new BasicMoveInfo();
	/// <summary>下蹲格挡受击动作。</summary>
	public BasicMoveInfo blockingCrouchingHit = new BasicMoveInfo();
	/// <summary>站立格挡姿态动作。</summary>
	public BasicMoveInfo blockingHighPose = new BasicMoveInfo();
	/// <summary>站立格挡受击动作。</summary>
	public BasicMoveInfo blockingHighHit = new BasicMoveInfo();
	/// <summary>低位格挡受击动作。</summary>
	public BasicMoveInfo blockingLowHit = new BasicMoveInfo();
	/// <summary>空中格挡姿态动作。</summary>
	public BasicMoveInfo blockingAirPose = new BasicMoveInfo();
	/// <summary>空中格挡受击动作。</summary>
	public BasicMoveInfo blockingAirHit = new BasicMoveInfo();
	/// <summary>下蹲弹反动作。</summary>
	public BasicMoveInfo parryCrouching = new BasicMoveInfo();
	/// <summary>站立弹反动作。</summary>
	public BasicMoveInfo parryHigh = new BasicMoveInfo();
	/// <summary>低位弹反动作。</summary>
	public BasicMoveInfo parryLow = new BasicMoveInfo();
	/// <summary>空中弹反动作。</summary>
	public BasicMoveInfo parryAir = new BasicMoveInfo();
	/// <summary>地面弹跳动作。</summary>
	public BasicMoveInfo groundBounce = new BasicMoveInfo();
	/// <summary>站立墙壁弹跳动作。</summary>
	public BasicMoveInfo standingWallBounce = new BasicMoveInfo();
	/// <summary>站立墙壁弹跳击倒动作。</summary>
	public BasicMoveInfo standingWallBounceKnockdown = new BasicMoveInfo();
	/// <summary>空中墙壁弹跳动作。</summary>
	public BasicMoveInfo airWallBounce = new BasicMoveInfo();
	/// <summary>地面弹跳后下落动作。</summary>
	public BasicMoveInfo fallingFromGroundBounce = new BasicMoveInfo();
	/// <summary>空中受击后下落动作。</summary>
	public BasicMoveInfo fallingFromAirHit = new BasicMoveInfo();
	/// <summary>默认倒地动作。</summary>
    public BasicMoveInfo fallDown = new BasicMoveInfo();
	/// <summary>空中受身恢复动作。</summary>
    public BasicMoveInfo airRecovery = new BasicMoveInfo();
	/// <summary>下蹲受击动作。</summary>
	public BasicMoveInfo getHitCrouching = new BasicMoveInfo();
	/// <summary>站立高位受击动作。</summary>
	public BasicMoveInfo getHitHigh = new BasicMoveInfo();
	/// <summary>站立低位受击动作。</summary>
	public BasicMoveInfo getHitLow = new BasicMoveInfo();
	/// <summary>高位击倒受击动作。</summary>
	public BasicMoveInfo getHitHighKnockdown = new BasicMoveInfo();
	/// <summary>中位击倒受击动作。</summary>
	public BasicMoveInfo getHitMidKnockdown = new BasicMoveInfo();
	/// <summary>空中连击受击动作。</summary>
	public BasicMoveInfo getHitAir = new BasicMoveInfo();
	/// <summary>破防受击动作。</summary>
	public BasicMoveInfo getHitCrumple = new BasicMoveInfo();
	/// <summary>击退受击动作。</summary>
	public BasicMoveInfo getHitKnockBack = new BasicMoveInfo();
	/// <summary>扫腿击倒受击动作。</summary>
	public BasicMoveInfo getHitSweep = new BasicMoveInfo();
	/// <summary>默认起身动作。</summary>
	public BasicMoveInfo standUp = new BasicMoveInfo();
	/// <summary>空中连击后起身动作。</summary>
	public BasicMoveInfo standUpFromAirHit = new BasicMoveInfo();
	/// <summary>击退后起身动作。</summary>
    public BasicMoveInfo standUpFromKnockBack = new BasicMoveInfo();
	/// <summary>站立高位受击后起身动作。</summary>
    public BasicMoveInfo standUpFromStandingHighHit = new BasicMoveInfo();
	/// <summary>站立中位受击后起身动作。</summary>
    public BasicMoveInfo standUpFromStandingMidHit = new BasicMoveInfo();
	/// <summary>破防后起身动作。</summary>
    public BasicMoveInfo standUpFromCrumple = new BasicMoveInfo();
	/// <summary>扫腿后起身动作。</summary>
    public BasicMoveInfo standUpFromSweep = new BasicMoveInfo();
	/// <summary>站立墙壁弹跳后起身动作。</summary>
    public BasicMoveInfo standUpFromStandingWallBounce = new BasicMoveInfo();
	/// <summary>空中墙壁弹跳后起身动作。</summary>
    public BasicMoveInfo standUpFromAirWallBounce = new BasicMoveInfo();
	/// <summary>地面弹跳后起身动作。</summary>
    public BasicMoveInfo standUpFromGroundBounce = new BasicMoveInfo();

	/// <summary>是否启用移动。</summary>
    public bool moveEnabled = true;
	/// <summary>是否启用跳跃。</summary>
    public bool jumpEnabled = true;
	/// <summary>是否启用下蹲。</summary>
    public bool crouchEnabled = true;
	/// <summary>是否启用格挡。</summary>
    public bool blockEnabled = true;
	/// <summary>是否启用弹反。</summary>
    public bool parryEnabled = true;

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 按钮序列记录：记录一次按键事件及其蓄力时间（用于必杀技指令匹配）。
/// </summary>
public class ButtonSequenceRecord {
    #region trackable definitions
	/// <summary>按下的按钮。</summary>
    public ButtonPress buttonPress;
	/// <summary>该按钮的蓄力时间。</summary>
    public Fix64 chargeTime;
    #endregion

	/// <summary>
	/// 构造函数。
	/// </summary>
	/// <param name="buttonPress">按下的按钮。</param>
	/// <param name="chargeTime">蓄力时间。</param>
    public ButtonSequenceRecord(ButtonPress buttonPress, Fix64 chargeTime) {
        this.buttonPress = buttonPress;
        this.chargeTime = chargeTime;
    }
}

/// <summary>
/// 招式集合脚本（MoveSetScript）：角色的动画与招式管理器（挂在角色预制体上）。
/// <para>加载并切换姿态的招式数据、注册动画、播放基础动作、执行输入指令匹配与招式实例化。</para>
/// </summary>
public class MoveSetScript : MonoBehaviour {
	/// <summary>基础动作集合（当前姿态）。</summary>
	public BasicMoves basicMoves;
	/// <summary>攻击招式列表（当前姿态）。</summary>
	public MoveInfo[] attackMoves;
	/// <summary>招式列表（= attackMoves，已按条件排序）。</summary>
	public MoveInfo[] moves;
	/// <summary>姿态进入演出招式。</summary>
	public MoveInfo intro;
	/// <summary>姿态退出演出招式。</summary>
	public MoveInfo outro;

    #region trackable definitions
	/// <summary>Mecanim 动画控制器属性。</summary>
    public MecanimControl MecanimControl { get { return this.mecanimControl; } set { mecanimControl = value; } }
	/// <summary>Legacy 动画控制器属性。</summary>
    public LegacyControl LegacyControl { get { return this.legacyControl; } set { legacyControl = value; } }
	/// <summary>当前空中已用招式次数（限制空中连段数）。</summary>
    public int totalAirMoves;
	/// <summary>动画是否暂停（速度 < 1 时）。</summary>
    public bool animationPaused;
	/// <summary>覆盖下一次融合值（-1 表示不使用）。</summary>
    public Fix64 overrideNextBlendingValue = -1;
	/// <summary>上次按键时间（秒，用于指令序列计时）。</summary>
    public Fix64 lastTimePress;
	/// <summary>最近按键序列记录列表。</summary>
    public List<ButtonSequenceRecord> lastButtonPresses = new List<ButtonSequenceRecord>();
    #endregion


	/// <summary>角色控制脚本引用。</summary>
    public ControlsScript controlsScript;
	/// <summary>判定盒脚本引用。</summary>
    public HitBoxesScript hitBoxesScript;
	/// <summary>Mecanim 动画控制器（内部引用）。</summary>
    private MecanimControl mecanimControl;
	/// <summary>Legacy 动画控制器（内部引用）。</summary>
    private LegacyControl legacyControl;
	/// <summary>已注册的基础动作列表。</summary>
    private List<BasicMoveInfo> basicMoveList = new List<BasicMoveInfo>();

    // Deprecated
	/// <summary>蓄力值字典（已弃用）。</summary>
    public Dictionary<ButtonPress, Fix64> chargeValues = new Dictionary<ButtonPress, Fix64>();

	/// <summary>
	/// 唤醒：获取控件/判定盒脚本、加载全部姿态招式数据并切换到姿态1。
	/// </summary>
    void Awake(){
		controlsScript = transform.parent.gameObject.GetComponent<ControlsScript>();
		hitBoxesScript = GetComponent<HitBoxesScript>();

		foreach(ButtonPress bp in Enum.GetValues(typeof(ButtonPress))){
			chargeValues.Add (bp, 0);
		}


        List<MoveSetData> loadedMoveSets = new List<MoveSetData>();
        foreach (MoveSetData moveSetData in controlsScript.myInfo.moves)
        {
            loadedMoveSets.Add(moveSetData);
        }
        foreach (string path in controlsScript.myInfo.stanceResourcePath)
        {
            loadedMoveSets.Add(Resources.Load<StanceInfo>(path).ConvertData());
        }
        controlsScript.myInfo.loadedMoves = loadedMoveSets.ToArray();

        controlsScript.myInfo.currentCombatStance = CombatStances.Stance10;
		ChangeMoveStances(CombatStances.Stance1);
	}


	/// <summary>
	/// 启动：Mecanim 动画系统下根据朝向设置镜像。
	/// </summary>
    void Start() {
		if (controlsScript.myInfo.animationType == AnimationType.Mecanim){
			mecanimControl.SetMirror(controlsScript.mirror > 0);
        }
	}


    /// <summary>
    /// 切换战斗姿态：加载新姿态的基础动作与攻击招式，重置动画组件，恢复当前动画播放状态。
    /// <para>同时校验蓄力技计时与重名招式并发出警告，对招式列表按多种条件排序以优化输入匹配。</para>
    /// </summary>
    /// <param name="newStance">要切换到的姿态。</param>
    public void ChangeMoveStances(CombatStances newStance) {
        if (controlsScript.myInfo.currentCombatStance == newStance) return;
        foreach (MoveSetData moveSetData in controlsScript.myInfo.loadedMoves) {
            if (moveSetData.combatStance == newStance) {
                string currentClip = basicMoves != null ? GetCurrentClipName() : null;
                Fix64 currentNormalizedTime = basicMoves != null ? GetCurrentClipPosition() : 0;
                Fix64 currentSpeed = 0;

                if (controlsScript.myInfo.animationType == AnimationType.Legacy && legacyControl != null) {
                    currentSpeed = legacyControl.globalSpeed;
                }
                string currentState = null;
                bool currentMirror = false;
                MecanimAnimationData currentMecanimData = new MecanimAnimationData();
                AnimatorOverrideController overrideController = new AnimatorOverrideController();

                if (controlsScript.myInfo.animationType == AnimationType.Mecanim && mecanimControl != null) {
                    currentState = mecanimControl.currentState;
                    currentMirror = mecanimControl.currentMirror;
                    currentSpeed = mecanimControl.currentSpeed;
                    overrideController = mecanimControl.overrideController;

                    mecanimControl.CopyAnimationData(mecanimControl.currentAnimationData, ref currentMecanimData);
                }
                
                basicMoves = moveSetData.basicMoves;
                attackMoves = moveSetData.attackMoves;
                moves = attackMoves;

                foreach (MoveInfo move1 in moves) {
                    if (move1.defaultInputs.chargeMove && move1.defaultInputs._chargeTiming <= controlsScript.myInfo._executionTiming) {
                        Debug.LogWarning("Warning: " + move1.name + " (" + move1.moveName + ") charge timing must be higher then the character's execution timing.");
                    }

                    foreach (MoveInfo move2 in moves) {
                        if (move1.name != move2.name && move1.moveName == move2.moveName) {
                            Debug.LogWarning("Warning: " + move1.name + " (" + move1.moveName + ") has the same name as " + move2.name + " (" + move2.moveName + ")");
                        }
                    }
                }

                // Reset Animation Components
                fillMoves();

                if (moveSetData.cinematicIntro != null) {
                    intro = Instantiate(moveSetData.cinematicIntro) as MoveInfo;
                    intro.name = "Intro";
                    attachAnimation(intro.animMap.clip, intro.name, intro._animationSpeed, intro.wrapMode, intro.animMap.length);
                }
                if (moveSetData.cinematicOutro != null) {
                    outro = Instantiate(moveSetData.cinematicOutro) as MoveInfo;
                    outro.name = "Outro";
                    attachAnimation(outro.animMap.clip, outro.name, outro._animationSpeed, outro.wrapMode, outro.animMap.length);
                }

                controlsScript.myInfo.currentCombatStance = newStance;

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    return move1.defaultInputs.buttonExecution.Length.CompareTo(move2.defaultInputs.buttonExecution.Length);
                });

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    if (move1.defaultInputs.buttonExecution.Length > 1 && move1.defaultInputs.buttonExecution.Contains(ButtonPress.Back)) return 0;
                    if (move1.defaultInputs.buttonExecution.Length > 1 && move1.defaultInputs.buttonExecution.Contains(ButtonPress.Forward)) return 0;
                    if (move1.defaultInputs.buttonExecution.Length > 1) return 1;
                    return 0;
                });

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    return move1.selfConditions.basicMoveLimitation.Length.CompareTo(move2.selfConditions.basicMoveLimitation.Length);
                });

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    return move1.opponentConditions.basicMoveLimitation.Length.CompareTo(move2.opponentConditions.basicMoveLimitation.Length);
                });

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    return move1.opponentConditions.possibleMoveStates.Length.CompareTo(move2.opponentConditions.possibleMoveStates.Length);
                });

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    return move1.previousMoves.Length.CompareTo(move2.previousMoves.Length);
                });

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    return move1.defaultInputs.buttonSequence.Length.CompareTo(move2.defaultInputs.buttonSequence.Length);
                });

                System.Array.Reverse(moves);

                if (currentClip != null) {
                    // Restore animation
                    if (controlsScript.myInfo.animationType == AnimationType.Mecanim) {
                        mecanimControl.currentState = currentState;
                        mecanimControl.currentMirror = currentMirror;
                        mecanimControl.currentSpeed = currentSpeed;
                        mecanimControl.overrideController = overrideController;

                        mecanimControl.currentAnimationData = new MecanimAnimationData();
                        mecanimControl.CopyAnimationData(currentMecanimData, ref mecanimControl.currentAnimationData);

                        mecanimControl.animator.runtimeAnimatorController = overrideController;
                        mecanimControl.animator.Play(currentState, 0, (float)currentNormalizedTime);
                        mecanimControl.animator.applyRootMotion = currentMecanimData.applyRootMotion;
                        mecanimControl.animator.Update(0);
                        mecanimControl.SetSpeed(currentSpeed);

                    } else {
                        legacyControl.globalSpeed = currentSpeed;
                        PlayAnimation(currentClip, 0, currentNormalizedTime);
                    }

                } else {
                    PlayBasicMove(basicMoves.idle);
                    controlsScript.currentState = PossibleStates.Stand;
                    controlsScript.currentSubState = SubStates.Resting;
                }
                return;
            }
        }
    }

    /// <summary>
    /// 重建动画系统：销毁旧的动画组件，按动画类型（Legacy/Mecanim）创建新的动画控制器，
    /// 并注册全部攻击招式与基础动作动画片段。
    /// </summary>
    private void fillMoves() {
        DestroyImmediate(gameObject.GetComponent(typeof(MecanimControl)));
        DestroyImmediate(gameObject.GetComponent(typeof(LegacyControl)));
		DestroyImmediate(gameObject.GetComponent(typeof(Animation)));
		DestroyImmediate(gameObject.GetComponent(typeof(Animator)));
		DestroyImmediate(gameObject.GetComponent("MecanimControl"));

        if ((UFE.isConnected || UFE.config.debugOptions.emulateNetwork)
            && UFE.config.networkOptions.forceAnimationControl) {
            controlsScript.myInfo.animationFlow = AnimationFlow.UFEEngine;
        }

		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
			gameObject.AddComponent(typeof(Animation));
            gameObject.GetComponent<Animation>().clip = basicMoves.idle.animMap[0].clip;
            gameObject.GetComponent<Animation>().wrapMode = WrapMode.Once;

            legacyControl = gameObject.AddComponent<LegacyControl>();
            if (controlsScript.myInfo.animationFlow == AnimationFlow.UFEEngine) legacyControl.overrideAnimatorUpdate = true;

		}else{
            Animator animator = (Animator) gameObject.AddComponent(typeof(Animator));
			animator.avatar = controlsScript.myInfo.avatar;
			//animator.applyRootMotion = true;

            //mecanimControl = gameObject.AddComponent<MC3>();
            mecanimControl = gameObject.AddComponent<MecanimControl>();
			mecanimControl.defaultTransitionDuration = controlsScript.myInfo._blendingTime;
			mecanimControl.SetDefaultClip(basicMoves.idle.animMap[0].clip, "default", basicMoves.idle._animationSpeed, WrapMode.Loop, 
			                              (controlsScript.mirror > 0 && UFE.config.characterRotationOptions.autoMirror));

            mecanimControl.defaultWrapMode = WrapMode.Once;
            if (controlsScript.myInfo.animationFlow == AnimationFlow.UFEEngine) mecanimControl.overrideAnimatorUpdate = true;
		}


        foreach (MoveInfo move in moves) {
            if (move == null) {
                Debug.LogWarning("You have empty entries in your move list. Check your special moves under Character Editor.");
                continue;
            }
			if (move.animMap.clip != null) {
                attachAnimation(move.animMap.clip, move.name, move._animationSpeed, move.wrapMode, move.animMap.length);
			}
		}

        setBasicMoveAnimation(basicMoves.idle, "idle", BasicMoveReference.Idle);
		setBasicMoveAnimation(basicMoves.moveForward, "moveForward", BasicMoveReference.MoveForward);
		setBasicMoveAnimation(basicMoves.moveBack, "moveBack", BasicMoveReference.MoveBack);
        setBasicMoveAnimation(basicMoves.crouching, "crouching", BasicMoveReference.Crouching);
        setBasicMoveAnimation(basicMoves.takeOff, "takeOff", BasicMoveReference.TakeOff);
		setBasicMoveAnimation(basicMoves.jumpStraight, "jumpStraight", BasicMoveReference.JumpStraight);
		setBasicMoveAnimation(basicMoves.jumpBack, "jumpBack", BasicMoveReference.JumpBack);
		setBasicMoveAnimation(basicMoves.jumpForward, "jumpForward", BasicMoveReference.JumpForward);
		setBasicMoveAnimation(basicMoves.fallStraight, "fallStraight", BasicMoveReference.FallStraight);
		setBasicMoveAnimation(basicMoves.fallBack, "fallBack", BasicMoveReference.FallBack);
		setBasicMoveAnimation(basicMoves.fallForward, "fallForward", BasicMoveReference.FallForward);
		setBasicMoveAnimation(basicMoves.landing, "landing", BasicMoveReference.Landing);

        setBasicMoveAnimation(basicMoves.blockingCrouchingPose, "blockingCrouchingPose", BasicMoveReference.BlockingCrouchingPose);
        setBasicMoveAnimation(basicMoves.blockingCrouchingHit, "blockingCrouchingHit", BasicMoveReference.BlockingCrouchingHit);
        setBasicMoveAnimation(basicMoves.blockingHighPose, "blockingHighPose", BasicMoveReference.BlockingHighPose);
        setBasicMoveAnimation(basicMoves.blockingHighHit, "blockingHighHit", BasicMoveReference.BlockingHighHit);
        setBasicMoveAnimation(basicMoves.blockingLowHit, "blockingLowHit", BasicMoveReference.BlockingLowHit);
        setBasicMoveAnimation(basicMoves.blockingAirPose, "blockingAirPose", BasicMoveReference.BlockingAirPose);
        setBasicMoveAnimation(basicMoves.blockingAirHit, "blockingAirHit", BasicMoveReference.BlockingAirHit);
        setBasicMoveAnimation(basicMoves.parryCrouching, "parryCrouching", BasicMoveReference.ParryCrouching);
		setBasicMoveAnimation(basicMoves.parryHigh, "parryHigh", BasicMoveReference.ParryHigh);
		setBasicMoveAnimation(basicMoves.parryLow, "parryLow", BasicMoveReference.ParryLow);
		setBasicMoveAnimation(basicMoves.parryAir, "parryAir", BasicMoveReference.ParryAir);

        setBasicMoveAnimation(basicMoves.getHitHigh, "getHitHigh", BasicMoveReference.HitStandingHigh);
        setBasicMoveAnimation(basicMoves.getHitLow, "getHitLow", BasicMoveReference.HitStandingLow);
        setBasicMoveAnimation(basicMoves.getHitCrouching, "getHitCrouching", BasicMoveReference.HitStandingCrouching);
        setBasicMoveAnimation(basicMoves.getHitAir, "getHitAir", BasicMoveReference.HitAirJuggle);
        setBasicMoveAnimation(basicMoves.getHitKnockBack, "getHitKnockBack", BasicMoveReference.HitKnockBack);
		setBasicMoveAnimation(basicMoves.getHitHighKnockdown, "getHitHighKnockdown", BasicMoveReference.HitStandingHighKnockdown);
		setBasicMoveAnimation(basicMoves.getHitMidKnockdown, "getHitMidKnockdown", BasicMoveReference.HitStandingMidKnockdown);
		setBasicMoveAnimation(basicMoves.getHitSweep, "getHitSweep", BasicMoveReference.HitSweep);
		setBasicMoveAnimation(basicMoves.getHitCrumple, "getHitCrumple", BasicMoveReference.HitCrumple);

        setBasicMoveAnimation(basicMoves.groundBounce, "groundBounce", BasicMoveReference.StageGroundBounce);
        setBasicMoveAnimation(basicMoves.standingWallBounce, "standingWallBounce", BasicMoveReference.StageStandingWallBounce);
        setBasicMoveAnimation(basicMoves.standingWallBounceKnockdown, "standingWallBounceKnockdown", BasicMoveReference.StageStandingWallBounceKnockdown);
        setBasicMoveAnimation(basicMoves.airWallBounce, "airWallBounce", BasicMoveReference.StageAirWallBounce);

        setBasicMoveAnimation(basicMoves.fallDown, "fallDown", BasicMoveReference.FallDownDefault);
        setBasicMoveAnimation(basicMoves.fallingFromAirHit, "fallingFromAirHit", BasicMoveReference.FallDownFromAirJuggle);
        setBasicMoveAnimation(basicMoves.fallingFromGroundBounce, "fallingFromBounce", BasicMoveReference.FallDownFromGroundBounce);
        setBasicMoveAnimation(basicMoves.airRecovery, "airRecovery", BasicMoveReference.AirRecovery);

		setBasicMoveAnimation(basicMoves.standUp, "standUp", BasicMoveReference.StandUpDefault);
        setBasicMoveAnimation(basicMoves.standUpFromAirHit, "standUpFromAirHit", BasicMoveReference.StandUpFromAirJuggle);
        setBasicMoveAnimation(basicMoves.standUpFromKnockBack, "standUpFromKnockBack", BasicMoveReference.StandUpFromKnockBack);
        setBasicMoveAnimation(basicMoves.standUpFromStandingHighHit, "standUpFromStandingHighHit", BasicMoveReference.StandUpFromStandingHighHit);
        setBasicMoveAnimation(basicMoves.standUpFromStandingMidHit, "standUpFromStandingMidHit", BasicMoveReference.StandUpFromStandingMidHit);
        setBasicMoveAnimation(basicMoves.standUpFromSweep, "standUpFromSweep", BasicMoveReference.StandUpFromSweep);
        setBasicMoveAnimation(basicMoves.standUpFromCrumple, "standUpFromCrumple", BasicMoveReference.StandUpFromCrumple);
        setBasicMoveAnimation(basicMoves.standUpFromStandingWallBounce, "standUpFromStandingWallBounce", BasicMoveReference.StandUpFromStandingWallBounce);
        setBasicMoveAnimation(basicMoves.standUpFromAirWallBounce, "standUpFromAirWallBounce", BasicMoveReference.StandUpFromAirWallBounce);
        setBasicMoveAnimation(basicMoves.standUpFromGroundBounce, "standUpFromGroundBounce", BasicMoveReference.StandUpFromGroundBounce);
	}
	
	/// <summary>
	/// 注册单个基础动作：设置名称/引用、加入基础动作列表，并注册其全部动画片段（含多片段 _2~_6）。
	/// </summary>
	/// <param name="basicMove">基础动作数据。</param>
	/// <param name="animName">动画注册名。</param>
	/// <param name="basicMoveReference">基础动作引用（枚举标识）。</param>
	private void setBasicMoveAnimation(BasicMoveInfo basicMove, string animName, BasicMoveReference basicMoveReference){
		if (basicMove.animMap[0].clip == null) {
			return;
		}
		basicMove.name = animName;
		basicMove.reference = basicMoveReference;

        basicMoveList.Add(basicMove);

        attachAnimation(basicMove.animMap[0].clip, animName, basicMove._animationSpeed, basicMove.wrapMode, basicMove.animMap[0].length);
        WrapMode newWrapMode = basicMove.wrapMode;
        if (basicMoveReference == BasicMoveReference.Idle) {
            newWrapMode = WrapMode.Once;
        } else if (basicMove.downClip) {
            newWrapMode = WrapMode.Loop;
        }

        if (basicMove.animMap[1].clip != null) attachAnimation(basicMove.animMap[1].clip, animName + "_2", basicMove._animationSpeed, newWrapMode, basicMove.animMap[1].length);
        if (basicMove.animMap[2].clip != null) attachAnimation(basicMove.animMap[2].clip, animName + "_3", basicMove._animationSpeed, newWrapMode, basicMove.animMap[2].length);
        if (basicMove.animMap[3].clip != null) attachAnimation(basicMove.animMap[3].clip, animName + "_4", basicMove._animationSpeed, newWrapMode, basicMove.animMap[3].length);
        if (basicMove.animMap[4].clip != null) attachAnimation(basicMove.animMap[4].clip, animName + "_5", basicMove._animationSpeed, newWrapMode, basicMove.animMap[4].length);
        if (basicMove.animMap[5].clip != null) attachAnimation(basicMove.animMap[5].clip, animName + "_6", basicMove._animationSpeed, newWrapMode, basicMove.animMap[5].length);
	}

    /// <summary>
    /// 注册一个动画片段到当前动画系统（Legacy/Mecanim）。
    /// </summary>
    /// <param name="clip">动画片段。</param>
    /// <param name="animName">注册名。</param>
    /// <param name="speed">播放速度。</param>
    /// <param name="wrapMode">播放模式。</param>
    /// <param name="length">片段时长（使用动画映射时用映射时长）。</param>
    private void attachAnimation(AnimationClip clip, string animName, Fix64 speed, WrapMode wrapMode, Fix64 length) {
        if (!controlsScript.myInfo.useAnimationMaps) length = clip.length;
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            legacyControl.AddClip(clip, animName, speed, wrapMode, length);
        } else {
            mecanimControl.AddClip(clip, animName, speed, wrapMode, length);
        }
    }

	/// <summary>
	/// 按基础动作引用（枚举）查找基础动作数据。
	/// </summary>
	/// <param name="reference">基础动作引用。</param>
	/// <returns>匹配的 BasicMoveInfo；未找到返回 null。</returns>
    public BasicMoveInfo GetBasicAnimationInfo(BasicMoveReference reference) {
        foreach(BasicMoveInfo basicMove in basicMoveList){
            if (basicMove.reference == reference) return basicMove;
        }
        return null;
    }

	/// <summary>
	/// 获取基础动作指定片段编号（1~6）对应的动画注册名（含 _2~_6 后缀）。
	/// </summary>
	/// <param name="basicMove">基础动作。</param>
	/// <param name="clipNum">片段编号。</param>
	/// <returns>动画注册名。</returns>
	public string GetAnimationString(BasicMoveInfo basicMove, int clipNum){
		if (clipNum == 1) return basicMove.name;
		if (clipNum == 2 && basicMove.animMap[1].clip != null) return basicMove.name + "_2";
		if (clipNum == 3 && basicMove.animMap[2].clip != null) return basicMove.name + "_3";
		if (clipNum == 4 && basicMove.animMap[3].clip != null) return basicMove.name + "_4";
		if (clipNum == 5 && basicMove.animMap[4].clip != null) return basicMove.name + "_5";
		if (clipNum == 6 && basicMove.animMap[5].clip != null) return basicMove.name + "_6";
		return basicMove.name;
	}


	/// <summary>
	/// 判断指定基础动作的任一动画片段是否正在播放。
	/// </summary>
	/// <param name="basicMove">基础动作。</param>
	/// <returns>任一片段在播放返回 true。</returns>
    public bool IsBasicMovePlaying(BasicMoveInfo basicMove) {
        if (basicMove.animMap[0].clip != null && IsAnimationPlaying(basicMove.name)) return true;
        if (basicMove.animMap[1].clip != null && IsAnimationPlaying(basicMove.name + "_2")) return true;
        if (basicMove.animMap[2].clip != null && IsAnimationPlaying(basicMove.name + "_3")) return true;
        if (basicMove.animMap[3].clip != null && IsAnimationPlaying(basicMove.name + "_4")) return true;
        if (basicMove.animMap[4].clip != null && IsAnimationPlaying(basicMove.name + "_5")) return true;
        if (basicMove.animMap[5].clip != null && IsAnimationPlaying(basicMove.name + "_6")) return true;
        return false;
    }
	
	/// <summary>
	/// 判断指定动画名是否正在播放。
	/// </summary>
	/// <param name="animationName">动画名。</param>
	/// <returns>播放中返回 true。</returns>
	public bool IsAnimationPlaying(string animationName){
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            return legacyControl.IsPlaying(animationName);
		}else{
			return mecanimControl.IsPlaying(animationName);
		}
	}

	/// <summary>
	/// 获取指定动画的时长。
	/// </summary>
	/// <param name="animationName">动画名。</param>
	/// <returns>动画时长（定点数）。</returns>
    public Fix64 GetAnimationLength(string animationName) {
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            return legacyControl.GetAnimationData(animationName).length;
		}else{
			return mecanimControl.GetAnimationData(animationName).length;
		}
	}

	/// <summary>
	/// 判断指定动画是否已注册。
	/// </summary>
	/// <param name="animationName">动画名。</param>
	/// <returns>已注册返回 true。</returns>
	public bool AnimationExists(string animationName){
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            return (legacyControl.GetAnimationData(animationName) != null);
		}else{
			return (mecanimControl.GetAnimationData(animationName) != null);
		}
	}

	/// <summary>
	/// 播放指定动画（从时间0开始）。
	/// </summary>
	/// <param name="animationName">动画名。</param>
	/// <param name="blendingTime">融合时间。</param>
    public void PlayAnimation(string animationName, Fix64 blendingTime) {
		PlayAnimation(animationName, blendingTime, 0);
	}

	/// <summary>
	/// 播放指定动画（从指定归一化时间开始，网络模式下可禁用融合）。
	/// </summary>
	/// <param name="animationName">动画名。</param>
	/// <param name="blendingTime">融合时间。</param>
	/// <param name="normalizedTime">起始归一化时间。</param>
    public void PlayAnimation(string animationName, Fix64 blendingTime, Fix64 normalizedTime) {
        if ((UFE.isConnected || UFE.config.debugOptions.emulateNetwork) &&
            UFE.config.networkOptions.disableBlending) blendingTime = 0;

        if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            legacyControl.Play(animationName, blendingTime, normalizedTime);
		}else{
			mecanimControl.Play(animationName, blendingTime, normalizedTime, (controlsScript.mirror > 0 && UFE.config.characterRotationOptions.autoMirror));
		}
	}

	/// <summary>
	/// 停止指定动画。
	/// </summary>
	/// <param name="animationName">动画名。</param>
	public void StopAnimation(string animationName){
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            legacyControl.Stop(animationName);
		}else{
			mecanimControl.Stop();
		}
	}

	/// <summary>
	/// 设置当前动画播放速度（速度 <1 时标记动画暂停）。
	/// </summary>
	/// <param name="speed">播放速度。</param>
    public void SetAnimationSpeed(Fix64 speed) {
        if (speed < 1) animationPaused = true;
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            legacyControl.SetSpeed(speed);
		}else{
			mecanimControl.SetSpeed(speed);
		}
	}

	/// <summary>
	/// 设置指定动画的播放速度。
	/// </summary>
	/// <param name="animationName">动画名。</param>
	/// <param name="speed">播放速度。</param>
    public void SetAnimationSpeed(string animationName, Fix64 speed) {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            legacyControl.SetSpeed(animationName, speed);
		}else{
			mecanimControl.SetSpeed(animationName, speed);
		}
	}

	/// <summary>
	/// 设置指定动画的归一化速度。
	/// </summary>
	/// <param name="animationName">动画名。</param>
	/// <param name="normalizedSpeed">归一化速度。</param>
    public void SetAnimationNormalizedSpeed(string animationName, Fix64 normalizedSpeed) {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            legacyControl.SetNormalizedSpeed(animationName, normalizedSpeed);
        } else {
            mecanimControl.SetNormalizedSpeed(animationName, normalizedSpeed);
        }
    }

	/// <summary>
	/// 获取当前动画播放速度。
	/// </summary>
	/// <returns>播放速度。</returns>
    public Fix64 GetAnimationSpeed() {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            return legacyControl.GetSpeed();
        } else {
            return mecanimControl.GetSpeed();
        }
    }

	/// <summary>
	/// 获取指定动画的当前播放速度。
	/// </summary>
	/// <param name="animationName">动画名。</param>
	/// <returns>播放速度。</returns>
    public Fix64 GetAnimationSpeed(string animationName) {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            return legacyControl.GetSpeed(animationName);
        } else {
            return mecanimControl.GetSpeed(animationName);
        }
    }

	/// <summary>
	/// 获取指定动画的原始播放速度。
	/// </summary>
	/// <param name="animationName">动画名。</param>
	/// <returns>原始播放速度。</returns>
    public Fix64 GetOriginalAnimationSpeed(string animationName) {
        return mecanimControl.GetOriginalSpeed(animationName);
    }

	/// <summary>
	/// 恢复动画原始播放速度并复位暂停标志。
	/// </summary>
	public void RestoreAnimationSpeed(){
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            legacyControl.RestoreSpeed();
		}else{
			mecanimControl.RestoreSpeed();
		}
		animationPaused = false;
	}
	
	/// <summary>
	/// 播放基础动作（使用动作默认片段名）。
	/// </summary>
	/// <param name="basicMove">基础动作。</param>
	public void PlayBasicMove(BasicMoveInfo basicMove){
		PlayBasicMove(basicMove, basicMove.name);
	}
	
	/// <summary>
	/// 播放基础动作（可指定是否允许重播）。
	/// </summary>
	/// <param name="basicMove">基础动作。</param>
	/// <param name="replay">是否允许重播。</param>
	public void PlayBasicMove(BasicMoveInfo basicMove, bool replay){
		PlayBasicMove(basicMove, basicMove.name, replay);
	}

	/// <summary>
	/// 播放基础动作（指定片段名，允许重播）。
	/// </summary>
	/// <param name="basicMove">基础动作。</param>
	/// <param name="clipName">动画片段名。</param>
	public void PlayBasicMove(BasicMoveInfo basicMove, string clipName){
		PlayBasicMove(basicMove, clipName, true);
	}

	/// <summary>
	/// 播放基础动作（指定片段名与重播标志，使用覆盖或默认融合时间）。
	/// </summary>
	/// <param name="basicMove">基础动作。</param>
	/// <param name="clipName">动画片段名。</param>
	/// <param name="replay">是否允许重播。</param>
	public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, bool replay){
		if (overrideNextBlendingValue > -1){
			PlayBasicMove(basicMove, clipName, overrideNextBlendingValue);
			overrideNextBlendingValue = -1;
		}else if (basicMove.overrideBlendingIn){
            PlayBasicMove(basicMove, clipName, basicMove._blendingIn, replay, basicMove.invincible);
		}else {
            PlayBasicMove(basicMove, clipName, controlsScript.myInfo._blendingTime, replay, basicMove.invincible);
		}
		
		if (basicMove.overrideBlendingOut) overrideNextBlendingValue = basicMove._blendingOut;
	}

	/// <summary>
	/// 播放基础动作（指定片段名与融合时间）。
	/// </summary>
	/// <param name="basicMove">基础动作。</param>
	/// <param name="clipName">动画片段名。</param>
	/// <param name="blendingTime">融合时间。</param>
    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, Fix64 blendingTime) {
        PlayBasicMove(basicMove, clipName, blendingTime, true, basicMove.invincible);
	}

	/// <summary>
	/// 播放基础动作（指定片段名、融合时间与重播标志）。
	/// </summary>
	/// <param name="basicMove">基础动作。</param>
	/// <param name="clipName">动画片段名。</param>
	/// <param name="blendingTime">融合时间。</param>
	/// <param name="replay">是否允许重播。</param>
    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, Fix64 blendingTime, bool replay) {
        PlayBasicMove(basicMove, clipName, blendingTime, replay, basicMove.invincible);
    }

	/// <summary>
	/// 播放基础动作（完整参数）：播放动画、切换头部注视/根骨骼运动、设置判定盒可见性并更新动画映射。
	/// </summary>
	/// <param name="basicMove">基础动作。</param>
	/// <param name="clipName">动画片段名。</param>
	/// <param name="blendingTime">融合时间。</param>
	/// <param name="replay">是否允许重播。</param>
	/// <param name="hideHitBoxes">是否隐藏判定盒（无敌基础动作）。</param>
    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, Fix64 blendingTime, bool replay, bool hideHitBoxes) {
		if (IsAnimationPlaying(clipName) && !replay) return;
		PlayAnimation(clipName, blendingTime);

        controlsScript.ToggleHeadLook(!basicMove.disableHeadLook);
        controlsScript.applyRootMotion = basicMove.applyRootMotion;

        _playBasicMove(basicMove);
        hitBoxesScript.HideHitBoxes(hideHitBoxes);

        for (int i = 0; i < 6; i ++){
            if (clipName == GetAnimationString(basicMove, i + 1)) {
                hitBoxesScript.bakeSpeed = basicMove.animMap[i].bakeSpeed;
                hitBoxesScript.animationMaps = basicMove.animMap[i].animationMaps;
                hitBoxesScript.UpdateMap(0);
                break;
            };
        }
	}
	
	/// <summary>
	/// 执行基础动作的附加逻辑：播放音效、记录当前基础动作、恢复判定盒默认可见性、生成粒子特效、触发基础动作事件。
	/// </summary>
	/// <param name="basicMove">基础动作。</param>
	private void _playBasicMove(BasicMoveInfo basicMove){
		UFE.PlaySound(basicMove.soundEffects);
		controlsScript.currentBasicMove = basicMove.reference;
		
		HitBoxesScript hitBoxes = controlsScript.character.GetComponent<HitBoxesScript>();
		if (hitBoxes != null){
			foreach (HitBox hitBox in hitBoxes.hitBoxes){
				if (hitBox != null && hitBox.bodyPart != BodyPart.none && hitBox.position != null){
					hitBox.position.gameObject.SetActive(hitBox.defaultVisibility);
				}
			}
		}
		
		if (basicMove.particleEffect.prefab != null) {
            Vector3 newPosition = hitBoxesScript.GetPosition(basicMove.particleEffect.bodyPart).ToVector();
			newPosition.x += basicMove.particleEffect.positionOffSet.x * -controlsScript.mirror;
			newPosition.y += basicMove.particleEffect.positionOffSet.y;
			newPosition.z += basicMove.particleEffect.positionOffSet.z;
            GameObject pTemp = UFE.SpawnGameObject(basicMove.particleEffect.prefab, newPosition, Quaternion.identity, Mathf.RoundToInt(basicMove.particleEffect.duration * UFE.config.fps));

            if (basicMove.particleEffect.mirrorOn2PSide && controlsScript.mirror > 0) {
                pTemp.transform.localEulerAngles = new Vector3(pTemp.transform.localEulerAngles.x, pTemp.transform.localEulerAngles.y + 180, pTemp.transform.localEulerAngles.z);
            }
			if (basicMove.particleEffect.stick) pTemp.transform.parent = transform;
		}

        UFE.FireBasicMove(basicMove.reference, controlsScript.myInfo);
	}

	/// <summary>
	/// 设置当前动画的播放位置（归一化时间）。
	/// </summary>
	/// <param name="animationName">动画名。</param>
	/// <param name="normalizedTime">归一化时间。</param>
	public void SetAnimationPosition(string animationName, Fix64 normalizedTime){
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            legacyControl.SetCurrentClipPosition(normalizedTime);
		}else{
			mecanimControl.SetCurrentClipPosition(normalizedTime);
		}
	}

	/// <summary>
	/// 获取动画帧位移增量（根骨骼运动位移）。
	/// </summary>
	/// <returns>位移增量（Unity Vector3）。</returns>
    public Vector3 GetDeltaDisplacement() {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            return legacyControl.GetDeltaDisplacement();
        } else {
            return mecanimControl.GetDeltaDisplacement();
        }
    }

	/// <summary>
	/// 获取动画位移增量（动画位移，与 DeltaDisplacement 不同）。
	/// </summary>
	/// <returns>位移增量（Unity Vector3）。</returns>
    public Vector3 GetDeltaPosition() {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            return legacyControl.GetDeltaPosition();

        } else {
            return mecanimControl.GetDeltaPosition();
        }
    }

	/// <summary>
	/// 获取当前播放的动画片段名。
	/// </summary>
	/// <returns>片段名。</returns>
    public string GetCurrentClipName() {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            return legacyControl.GetCurrentClipName();
        } else {
            return mecanimControl.GetCurrentClipName();
        }
    }

	/// <summary>
	/// 获取当前播放位置（秒）。
	/// </summary>
	/// <returns>当前播放时间。</returns>
    public Fix64 GetCurrentClipPosition() {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            return legacyControl.GetCurrentClipPosition();
		}else{
			return mecanimControl.GetCurrentClipPosition();
		}
	}

	/// <summary>
	/// 获取当前播放位置（归一化时间）。
	/// </summary>
	/// <returns>归一化时间。</returns>
    public Fix64 GetCurrentClipNormalizedTime() {
        return mecanimControl.GetCurrentClipNormalizedTime();
    }

	/// <summary>
	/// 获取当前播放帧号。
	/// </summary>
	/// <param name="realSeconds">true 按真实秒计算，false 按动画时间计算。</param>
	/// <returns>当前帧号。</returns>
    public int GetCurrentClipFrame(bool realSeconds = false) {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            return (int)FPMath.Abs(FPMath.Round(legacyControl.GetCurrentClipTime(realSeconds) * UFE.config.fps));
		}else{
            return (int)FPMath.Abs(FPMath.Round(mecanimControl.GetCurrentClipTime(realSeconds) * UFE.config.fps));
        }
    }

	/// <summary>
	/// 将招式帧号转换为动画归一化时间（反向播放时返回 >1 的值）。
	/// </summary>
	/// <param name="animFrame">招式帧号。</param>
	/// <param name="move">招式。</param>
	/// <returns>归一化时间。</returns>
    public Fix64 GetAnimationNormalizedTime(int animFrame, MoveInfo move){
		if (move == null) return 0;
		if (move._animationSpeed < 0){
			return ((Fix64)animFrame/ (Fix64)move.totalFrames) + 1;
		}else{
			return (Fix64)animFrame/ (Fix64)move.totalFrames;
		}
	}
	
	/// <summary>
	/// 设置 Mecanim 镜像。
	/// </summary>
	/// <param name="toggle">是否镜像。</param>
	public void SetMecanimMirror(bool toggle){
		mecanimControl.SetMirror(toggle, UFE.config.characterRotationOptions._mirrorBlending, true);
	}

	/// <summary>
	/// 判断指定按钮是否匹配当前格挡类型（HoldButton 类格挡）。
	/// </summary>
	/// <param name="button">按钮。</param>
	/// <returns>匹配返回 true。</returns>
	public bool CompareBlockButtons(ButtonPress button){
		if (button == ButtonPress.Button1 && UFE.config.blockOptions.blockType == BlockType.HoldButton1) return true;
		if (button == ButtonPress.Button2 && UFE.config.blockOptions.blockType == BlockType.HoldButton2) return true;
		if (button == ButtonPress.Button3 && UFE.config.blockOptions.blockType == BlockType.HoldButton3) return true;
		if (button == ButtonPress.Button4 && UFE.config.blockOptions.blockType == BlockType.HoldButton4) return true;
		if (button == ButtonPress.Button5 && UFE.config.blockOptions.blockType == BlockType.HoldButton5) return true;
		if (button == ButtonPress.Button6 && UFE.config.blockOptions.blockType == BlockType.HoldButton6) return true;
		if (button == ButtonPress.Button7 && UFE.config.blockOptions.blockType == BlockType.HoldButton7) return true;
		if (button == ButtonPress.Button8 && UFE.config.blockOptions.blockType == BlockType.HoldButton8) return true;
		if (button == ButtonPress.Button9 && UFE.config.blockOptions.blockType == BlockType.HoldButton9) return true;
		if (button == ButtonPress.Button10 && UFE.config.blockOptions.blockType == BlockType.HoldButton10) return true;
		if (button == ButtonPress.Button11 && UFE.config.blockOptions.blockType == BlockType.HoldButton11) return true;
		if (button == ButtonPress.Button12 && UFE.config.blockOptions.blockType == BlockType.HoldButton12) return true;
		return false;
	}
	
	/// <summary>
	/// 判断指定按钮是否匹配当前弹反类型（TapButton 类弹反）。
	/// </summary>
	/// <param name="button">按钮。</param>
	/// <returns>匹配返回 true。</returns>
	public bool CompareParryButtons(ButtonPress button){
		if (button == ButtonPress.Button1 && UFE.config.blockOptions.parryType == ParryType.TapButton1) return true;
		if (button == ButtonPress.Button2 && UFE.config.blockOptions.parryType == ParryType.TapButton2) return true;
		if (button == ButtonPress.Button3 && UFE.config.blockOptions.parryType == ParryType.TapButton3) return true;
		if (button == ButtonPress.Button4 && UFE.config.blockOptions.parryType == ParryType.TapButton4) return true;
		if (button == ButtonPress.Button5 && UFE.config.blockOptions.parryType == ParryType.TapButton5) return true;
		if (button == ButtonPress.Button6 && UFE.config.blockOptions.parryType == ParryType.TapButton6) return true;
		if (button == ButtonPress.Button7 && UFE.config.blockOptions.parryType == ParryType.TapButton7) return true;
		if (button == ButtonPress.Button8 && UFE.config.blockOptions.parryType == ParryType.TapButton8) return true;
		if (button == ButtonPress.Button9 && UFE.config.blockOptions.parryType == ParryType.TapButton9) return true;
		if (button == ButtonPress.Button10 && UFE.config.blockOptions.parryType == ParryType.TapButton10) return true;
		if (button == ButtonPress.Button11 && UFE.config.blockOptions.parryType == ParryType.TapButton11) return true;
		if (button == ButtonPress.Button12 && UFE.config.blockOptions.parryType == ParryType.TapButton12) return true;
		return false;
	}

	/// <summary>
	/// 判断当前能量是否足够（无能量槽时总是满足）。
	/// </summary>
	/// <param name="gaugeNeeded">所需能量百分比。</param>
	/// <returns>足够返回 true。</returns>
	private bool hasEnoughGauge(Fix64 gaugeNeeded){
		if (!UFE.config.gameGUI.hasGauge) return true;
        if (controlsScript.myInfo.currentGaugePoints < ((Fix64)controlsScript.myInfo.maxGaugePoints * (gaugeNeeded / 100))) return false;
		return true;
	}

	/// <summary>
	/// 获取姿态进入演出招式的实例。
	/// </summary>
	/// <returns>演出招式实例。</returns>
	public MoveInfo GetIntro(){
		return InstantiateMove(intro);
	}
	
	/// <summary>
	/// 获取姿态退出演出招式的实例。
	/// </summary>
	/// <returns>演出招式实例。</returns>
	public MoveInfo GetOutro(){
		return InstantiateMove(outro);
	}
	
	/// <summary>
	/// 实例化招式（独立副本，避免修改资产数据）。
	/// </summary>
	/// <param name="move">招式资产。</param>
	/// <returns>招式实例；参数为 null 返回 null。</returns>
	public MoveInfo InstantiateMove(MoveInfo move){
		if (move == null) return null;
		MoveInfo newMove = Instantiate(move) as MoveInfo;
		newMove.name = move.name;
		return newMove;
	}

	/// <summary>
	/// 根据当前招式的取消链获取下一个应自动执行的招式（无需输入条件的派生技）。
	/// </summary>
	/// <param name="currentMove">当前招式。</param>
	/// <returns>下一招式实例；无可用招式返回 null。</returns>
	public MoveInfo GetNextMove(MoveInfo currentMove){
		if (currentMove.frameLinks.Length == 0) return null;

		foreach(FrameLink frameLink in currentMove.frameLinks){
			if (frameLink.linkableMoves.Length == 0) continue;
			if (frameLink.cancelable){
                foreach (MoveInfo move in frameLink.linkableMoves) {
                    if (move == null) continue;
					if (move.defaultInputs.buttonExecution.Length == 0 || frameLink.ignoreInputs ||
                        (move.defaultInputs.onReleaseExecution && 
                        !move.defaultInputs.requireButtonPress && 
                        controlsScript.inputHeldDown[move.defaultInputs.buttonExecution[0]] == 0)
                        ) {
						    return InstantiateMove(move);
					}
				}
			}
		}
		return null;
	}

	/// <summary>
	/// 清空最近按键序列与蓄力记录。
	/// </summary>
	public void ClearLastButtonSequence(){
		lastButtonPresses.Clear();
        lastTimePress = 0;
		
		foreach(ButtonPress bp in Enum.GetValues(typeof(ButtonPress))){
			chargeValues[bp] = 0;
		}
	}

	/// <summary>
	/// 校验执行状态（防止松键触发时重复记录同一次按键）。
	/// </summary>
	/// <param name="buttonPress">按钮数组。</param>
	/// <param name="inputUp">是否松键。</param>
	/// <returns>允许执行返回 true。</returns>
	private bool checkExecutionState(ButtonPress[] buttonPress, bool inputUp){
		if (inputUp 
		    && lastButtonPresses.Count > 0 
			&& buttonPress[0].Equals(lastButtonPresses.ToArray()[lastButtonPresses.Count - 1])) return false;

		return true;
	}

	/// <summary>
	/// 根据当前输入匹配可执行的招式（非强制执行）。
	/// </summary>
	/// <param name="buttonPress">当前按下的按钮。</param>
	/// <param name="charge">当前按钮蓄力时间。</param>
	/// <param name="currentMove">当前招式。</param>
	/// <param name="inputUp">是否松键。</param>
	/// <returns>匹配的招式实例；无则返回 null。</returns>
	public MoveInfo GetMove(ButtonPress[] buttonPress, Fix64 charge, MoveInfo currentMove, bool inputUp){
		return GetMove(buttonPress, charge, currentMove, inputUp, false);
	}

	/// <summary>
	/// 根据当前输入匹配可执行的招式（必杀技指令匹配核心方法）。
	/// <para>先尝试用按键序列匹配（含执行时间窗口校验），记录按键序列后再用完整输入匹配一次。</para>
	/// </summary>
	/// <param name="buttonPress">当前按下的按钮。</param>
	/// <param name="charge">当前按钮蓄力时间。</param>
	/// <param name="currentMove">当前招式。</param>
	/// <param name="inputUp">是否松键。</param>
	/// <param name="forceExecution">是否强制执行（跳过按键序列匹配）。</param>
	/// <returns>匹配的招式实例；无则返回 null。</returns>
    public MoveInfo GetMove(ButtonPress[] buttonPress, Fix64 charge, MoveInfo currentMove, bool inputUp, bool forceExecution) {
		if (buttonPress.Length > 0
            && (UFE.currentFrame / (Fix64) UFE.config.fps) - lastTimePress <= controlsScript.myInfo._executionTiming) {

			if (controlsScript.debugInfo.buttonSequence){
                string allbp = "";
                foreach (ButtonSequenceRecord bp in lastButtonPresses) {
                    allbp += bp.buttonPress.ToString() + " [" + bp.chargeTime + "]" + ", ";
                }
                string allbp2 = "";

                string inputUpStr = inputUp ? "[Up]" : "[Down]";
				foreach(ButtonPress bp in buttonPress) allbp2 += bp.ToString() + " ";
                Debug.Log(" Sequence: ( " + allbp + ") | " + inputUpStr + " Execution: " + "( " + allbp2 + ")");
			}

			// Attempt execution
            foreach (MoveInfo move in moves) {
                if (move == null) continue;
				MoveInfo newMove = TestMoveExecution(move, currentMove, buttonPress, inputUp, true);
				if (newMove != null) return newMove;
			}
		}

		if (buttonPress.Length > 0) {
            if ((UFE.currentFrame / (Fix64) UFE.config.fps) - lastTimePress > controlsScript.myInfo._executionTiming) {
				ClearLastButtonSequence();
			}

			if (!forceExecution){
                lastTimePress = UFE.currentFrame / (Fix64) UFE.config.fps;
				// Store sequence
                if (!inputUp || charge > controlsScript.myInfo._executionTiming) {
                    lastButtonPresses.Add(new ButtonSequenceRecord(buttonPress[0], charge));
                }

				/*if (!inputUp || (inputUp && lastButtonPresses.Count == 0)){
					lastButtonPresses.Add(buttonPress[0]);
					if (charge > 0) chargeValues[buttonPress[0]] = charge;
				}*/
            }

            // Attempt execution one more time
            foreach (MoveInfo move in moves) {
                MoveInfo newMove = TestMoveExecution(move, currentMove, buttonPress, inputUp, false, forceExecution);
                if (newMove != null) return newMove;
            }
		}

		return null;
	}
    
	/// <summary>
	/// 在招式的取消链（含出招缓冲窗口）中查找指定招式名。
	/// </summary>
	/// <param name="moveName">招式名。</param>
	/// <param name="frameLinks">取消链列表。</param>
	/// <param name="currentFrame">当前招式帧。</param>
	/// <returns>存在返回 true。</returns>
    private bool searchMoveBuffer(string moveName, FrameLink[] frameLinks, int currentFrame) {
        foreach (FrameLink frameLink in frameLinks) {
            if ((currentFrame >= frameLink.activeFramesBegins && currentFrame <= frameLink.activeFramesEnds)
                || (currentFrame >= (frameLink.activeFramesBegins - UFE.config.executionBufferTime)
                && currentFrame <= frameLink.activeFramesEnds) && frameLink.allowBuffer) {

                foreach (MoveInfo move in frameLink.linkableMoves) {
                    if (move == null) continue;
                    if (moveName == move.moveName) return true;
                }
            }
        }

        return false;
    }

	/// <summary>
	/// 在招式的取消链中查找指定招式名（仅可取消的链接，可忽略玩家条件）。
	/// </summary>
	/// <param name="moveName">招式名。</param>
	/// <param name="frameLinks">取消链列表。</param>
	/// <param name="ignoreConditions">是否忽略玩家条件过滤。</param>
	/// <returns>存在返回 true。</returns>
	public bool SearchMove(string moveName, FrameLink[] frameLinks, bool ignoreConditions = false) {
		foreach(FrameLink frameLink in frameLinks){
			if (frameLink.cancelable){
				if (ignoreConditions && !frameLink.ignorePlayerConditions) continue;

                foreach (MoveInfo move in frameLink.linkableMoves) {
                    if (move == null) continue;
					if (moveName == move.moveName) return true;
				}
			}
		}
		
		return false;
	}

	/// <summary>
	/// 在指定招式数组中查找指定招式名。
	/// </summary>
	/// <param name="moveName">招式名。</param>
	/// <param name="moves">招式数组。</param>
	/// <returns>存在返回 true。</returns>
	private bool searchMove(string moveName, MoveInfo[] moves){
        foreach (MoveInfo move in moves) {
            if (move == null) continue;
            if (moveName == move.moveName) return true;
        }
		
		return false;
	}

	/// <summary>
	/// 当前姿态是否包含指定名称的招式。
	/// </summary>
	/// <param name="moveName">招式名。</param>
	/// <returns>存在返回 true。</returns>
	public bool HasMove(string moveName){
		foreach(MoveInfo move in this.moves)
			if (moveName == move.moveName) return true;
		
		return false;
	}


	/// <summary>
	/// 校验招式是否满足全部执行条件（在招式列表、姿态/基础动作条件、能量、前置招式与取消链）。
	/// </summary>
	/// <param name="move">要校验的招式。</param>
	/// <returns>满足全部条件返回 true。</returns>
    public bool ValidateMoveExecution(MoveInfo move) {
        if (!searchMove(move.moveName, attackMoves)) return false;
		if (!ValidateMoveStances(move.selfConditions, controlsScript, true)) return false;
		if (!ValidateMoveStances(move.opponentConditions, controlsScript.opControlsScript)) return false;
		if (!ValidadeBasicMove(move.selfConditions, controlsScript)) return false;
		if (!ValidadeBasicMove(move.opponentConditions, controlsScript.opControlsScript)) return false;
		if (!hasEnoughGauge(move._gaugeRequired)) return false;
		if (move.previousMoves.Length > 0 && controlsScript.currentMove == null) return false;
		if (move.previousMoves.Length > 0 && !searchMove(controlsScript.currentMove.moveName, move.previousMoves)) return false;

		if (controlsScript.currentMove != null && controlsScript.currentMove.frameLinks.Length == 0) return false;
		if (controlsScript.currentMove != null && !SearchMove(move.moveName, controlsScript.currentMove.frameLinks)) return false;
		return true;
	}

	
	/// <summary>
	/// 校验招式的状态条件（默认不跳过下蹲姿态）。
	/// </summary>
	/// <param name="conditions">玩家条件。</param>
	/// <param name="cScript">目标控制脚本。</param>
	/// <returns>满足返回 true。</returns>
	public bool ValidateMoveStances(PlayerConditions conditions, ControlsScript cScript){
		return ValidateMoveStances(conditions, cScript, false);
	}

	/// <summary>
	/// 校验招式的状态条件：状态匹配、距离范围、跳跃弧线、移动/格挡/眩晕限制。
	/// </summary>
	/// <param name="conditions">玩家条件。</param>
	/// <param name="cScript">目标控制脚本。</param>
	/// <param name="bypassCrouchStance">是否跳过下蹲姿态检查（自身招式使用）。</param>
	/// <returns>满足返回 true。</returns>
	public bool ValidateMoveStances(PlayerConditions conditions, ControlsScript cScript, bool bypassCrouchStance){
		bool stateCheck = conditions.possibleMoveStates.Length > 0? false : true;
		foreach(PossibleMoveStates possibleMoveState in conditions.possibleMoveStates){

			if (possibleMoveState.possibleState != cScript.currentState
			    && (!bypassCrouchStance || (bypassCrouchStance && cScript.currentState != PossibleStates.Stand))) continue;
			
			if (cScript.normalizedDistance < (Fix64)possibleMoveState.proximityRangeBegins/100) continue;
			if (cScript.normalizedDistance > (Fix64)possibleMoveState.proximityRangeEnds/100) continue;

            if (cScript.currentState == PossibleStates.Stand) {
                if (cScript.Physics.isTakingOff) continue;
				if (!possibleMoveState.standBy && cScript.currentSubState == SubStates.Resting) continue;
				if (!possibleMoveState.movingBack && cScript.currentSubState == SubStates.MovingBack) continue;
				if (!possibleMoveState.movingForward && cScript.currentSubState == SubStates.MovingForward) continue;

			} else if (cScript.currentState == PossibleStates.NeutralJump
			          || cScript.currentState == PossibleStates.ForwardJump
			          || cScript.currentState == PossibleStates.BackJump){ 
				
				if (cScript.normalizedJumpArc < (Fix64)possibleMoveState.jumpArcBegins/100) continue;
				if (cScript.normalizedJumpArc > (Fix64)possibleMoveState.jumpArcEnds/100) continue;
			}

            if ((!possibleMoveState.blocking && !UFE.config.blockOptions.allowMoveCancel) 
                && (cScript.currentSubState == SubStates.Blocking || cScript.isBlocking)) continue;

			if ((!possibleMoveState.stunned && possibleMoveState.possibleState != PossibleStates.Down) 
			    && cScript.currentSubState == SubStates.Stunned) continue;

			stateCheck = true;
		}
		return stateCheck;
	}

	/// <summary>
	/// 校验基础动作限制条件：无限制或当前基础动作在允许列表中即为通过。
	/// </summary>
	/// <param name="conditions">玩家条件。</param>
	/// <param name="cScript">目标控制脚本。</param>
	/// <returns>通过返回 true。</returns>
	public bool ValidadeBasicMove(PlayerConditions conditions, ControlsScript cScript){
		if (conditions.basicMoveLimitation.Length == 0) return true;
		if (System.Array.IndexOf(conditions.basicMoveLimitation, cScript.currentBasicMove) != -1) return true;
		return false;
	}
	
	/// <summary>
	/// 测试招式执行（非强制执行版本）。
	/// </summary>
	/// <param name="move">待测招式。</param>
	/// <param name="currentMove">当前招式。</param>
	/// <param name="buttonPress">当前输入按钮。</param>
	/// <param name="inputUp">是否松键。</param>
	/// <param name="fromSequence">是否从按键序列匹配。</param>
	/// <returns>可执行时返回招式实例；否则返回 null。</returns>
	private MoveInfo TestMoveExecution(MoveInfo move, MoveInfo currentMove, ButtonPress[] buttonPress, bool inputUp, bool fromSequence) {
		return TestMoveExecution(move, currentMove, buttonPress, inputUp, fromSequence, false);
	}

	/// <summary>
	/// 测试招式执行：校验能量/前置招式/状态条件/基础动作条件/输入指令匹配，通过则实例化招式。
	/// <para>同时校验空中招式次数限制与取消链/出招缓冲/存储招式。</para>
	/// </summary>
	/// <param name="move">待测招式。</param>
	/// <param name="currentMove">当前招式。</param>
	/// <param name="buttonPress">当前输入按钮。</param>
	/// <param name="inputUp">是否松键。</param>
	/// <param name="fromSequence">是否从按键序列匹配。</param>
	/// <param name="forceExecution">是否强制跳过缓冲检查。</param>
	/// <returns>可执行时返回招式实例；否则返回 null。</returns>
	private MoveInfo TestMoveExecution(MoveInfo move, MoveInfo currentMove, ButtonPress[] buttonPress, bool inputUp, bool fromSequence, bool forceExecution) {
        if (!hasEnoughGauge(move._gaugeRequired)) return null;
		if (move.previousMoves.Length > 0 && currentMove == null) return null;
        if (move.previousMoves.Length > 0 && !searchMove(currentMove.moveName, move.previousMoves)) return null;
        if (controlsScript.isAirRecovering && controlsScript.airRecoveryType == AirRecoveryType.CantMove) return null;

		if (currentMove == null || (currentMove != null && !SearchMove(move.moveName, currentMove.frameLinks, true))){
			if (!ValidateMoveStances(move.selfConditions, controlsScript)) return null;
			if (!ValidateMoveStances(move.opponentConditions, controlsScript.opControlsScript)) return null;
			if (!ValidadeBasicMove(move.selfConditions, controlsScript)) return null;
			if (!ValidadeBasicMove(move.opponentConditions, controlsScript.opControlsScript)) return null;
		}

        if (!CompareSequence(move.defaultInputs, buttonPress, inputUp, fromSequence, true) 
            && !CompareSequence(move.altInputs, buttonPress, inputUp, fromSequence, false)) return null;

        /*
		Array.Sort(buttonPress);
		Array.Sort(move.defaultInputs.buttonExecution);


		if (fromSequence){
			if (move.defaultInputs.buttonSequence.Length == 0) return null;
            if (move.defaultInputs.chargeMove) {
                bool charged = false;
                foreach (ButtonSequenceRecord bsr in lastButtonPresses) {
                    if (bsr.buttonPress == move.defaultInputs.buttonSequence[0]
                        && bsr.chargeTime >= move.defaultInputs._chargeTiming) {
                            charged = true;
                    }
                }

				if (!charged) return null;
			}

            //if (controlsScript.debugInfo.buttonSequence) {
            //    string allbp = "";
            //    foreach (ButtonSequenceRecord bp in lastButtonPresses) allbp += " " + bp.buttonPress.ToString();
            //    Debug.Log(allbp);
            //    string allbp2 = "";
            //    foreach (ButtonPress bp in move.buttonSequence) allbp2 += " " + bp.ToString();
            //    Debug.Log(allbp + "=" + allbp2 + "? " + ArraysEqual<ButtonPress>(buttonPress, move.buttonExecution));
            //}


            List<ButtonPress> buttonPressesList = new List<ButtonPress>();
            foreach (ButtonSequenceRecord bsr in lastButtonPresses) {
                if (bsr.chargeTime == 0 || (move.defaultInputs.allowNegativeEdge && bsr.buttonPress == move.defaultInputs.buttonSequence[0])) {
                    buttonPressesList.Add(bsr.buttonPress);
                }
            }

            if (buttonPressesList.Count >= move.defaultInputs.buttonSequence.Length) {

                ButtonPress[] compareSequence;
                int compareRange = buttonPressesList.Count - move.defaultInputs.buttonSequence.Length;
                if (compareRange < 0) compareRange = 0;

				if (move.defaultInputs.allowInputLeniency){
                    compareRange -= move.defaultInputs.leniencyBuffer;
                    if (compareRange < 0) compareRange = 0;
                    compareSequence = buttonPressesList.GetRange(compareRange, buttonPressesList.Count - compareRange).ToArray();
					compareSequence = ArrayIntersect<ButtonPress>(move.defaultInputs.buttonSequence, compareSequence);

					//string allbp = "";
					//foreach(ButtonPress bp in lastButtonPresses) allbp += " "+ bp.ToString();
					//Debug.Log(move.moveName + ": lastButtonPresses (leniency): "+ allbp);
					//string allbp2 = "";
					//foreach(ButtonPress bp in move.buttonSequence) allbp2 += " "+ bp.ToString();
					//Debug.Log(move.moveName + ": move.buttonSequence (leniency): "+ allbp2);
					//if (compareSequence != null){
					//	allbp3 = "";
					//	foreach(ButtonPress bp in compareSequence) allbp3 += " "+ bp.ToString();
					//	Debug.Log(move.moveName + ": compareSequence (leniency): "+ allbp3);
					//}

				}else{
                    compareSequence = buttonPressesList.GetRange(compareRange, move.defaultInputs.buttonSequence.Length).ToArray();

					//string allbp3 = "";
					//foreach(ButtonPress bp in compareSequence) allbp3 += " "+ bp.ToString();
					//Debug.Log(move.moveName + ": compareSequence: "+ allbp3);
				}
				
				if (!ArraysEqual<ButtonPress>(compareSequence, move.defaultInputs.buttonSequence)) return null;
			}else{
				return null;
			}
			
			//Debug.Log("Sequence for "+ move.moveName +" pass! Testing Execution:"+ ArraysEqual<ButtonPress>(buttonPress, move.buttonExecution));
		}else{
			if (move.defaultInputs.buttonSequence.Length > 0) return null;
		}

		if (!inputUp && !move.defaultInputs.onPressExecution) return null;
		if (inputUp && !move.defaultInputs.onReleaseExecution) return null;
		if (!ArraysEqual<ButtonPress>(buttonPress, move.defaultInputs.buttonExecution)) return null;
        */

		if (controlsScript.storedMove != null && move.moveName == controlsScript.storedMove.moveName)
			return controlsScript.storedMove;

        if (controlsScript.debugInfo.buttonSequence) {
            string allbp4 = "";
            foreach (ButtonPress bp in buttonPress) allbp4 += " " + bp.ToString();
            Debug.Log(move.moveName + ": Button Execution: " + allbp4);
        }

        if (currentMove == null || forceExecution || (searchMoveBuffer(move.moveName, currentMove.frameLinks, currentMove.currentFrame)) ||
		    UFE.config.executionBufferType == ExecutionBufferType.AnyMove){
			MoveInfo newMove = InstantiateMove(move);
			
			if ((controlsScript.currentState == PossibleStates.NeutralJump ||
			    controlsScript.currentState == PossibleStates.ForwardJump ||
			    controlsScript.currentState == PossibleStates.BackJump) &&
			    totalAirMoves >= controlsScript.myInfo.possibleAirMoves) return null;

			return newMove;
		}

		return null;
	}
	

	/// <summary>
	/// 比较输入序列与招式的指令/执行按钮是否匹配（含蓄力技、输入宽容、松键触发与负边沿判断）。
	/// </summary>
	/// <param name="moveInputs">招式输入配置。</param>
	/// <param name="buttonPress">当前输入按钮。</param>
	/// <param name="inputUp">是否松键。</param>
	/// <param name="fromSequence">是否从按键序列匹配。</param>
	/// <param name="allowEmptyExecution">是否允许空执行按钮。</param>
	/// <returns>匹配返回 true。</returns>
    private bool CompareSequence(MoveInputs moveInputs, ButtonPress[] buttonPress, bool inputUp, bool fromSequence, bool allowEmptyExecution)
    {
        if (!allowEmptyExecution && moveInputs.buttonExecution.Length == 0) return false;
        Array.Sort(buttonPress);
		Array.Sort(moveInputs.buttonExecution);

		if (fromSequence){
			if (moveInputs.buttonSequence.Length == 0) return false;
            if (moveInputs.chargeMove) {
                bool charged = false;
                foreach (ButtonSequenceRecord bsr in lastButtonPresses) {
                    if (bsr.buttonPress == moveInputs.buttonSequence[0]
                        && bsr.chargeTime >= moveInputs._chargeTiming) {
                            charged = true;
                    }
                }

				if (!charged) return false;
			}

            if (controlsScript.debugInfo.buttonSequence) {
                string allbp = "";
                foreach (ButtonSequenceRecord bp in lastButtonPresses) allbp += " " + bp.buttonPress.ToString();
                Debug.Log(allbp);
                string allbp2 = "";
                foreach (ButtonPress bp in moveInputs.buttonSequence) allbp2 += " " + bp.ToString();
                Debug.Log(allbp + "=" + allbp2 + "? " + ArraysEqual<ButtonPress>(buttonPress, moveInputs.buttonExecution));
            }

            List<ButtonPress> buttonPressesList = new List<ButtonPress>();
            foreach (ButtonSequenceRecord bsr in lastButtonPresses) {
                if (bsr.chargeTime == 0 || (moveInputs.allowNegativeEdge && bsr.buttonPress == moveInputs.buttonSequence[0])) {
                    buttonPressesList.Add(bsr.buttonPress);
                }
            }

            if (buttonPressesList.Count >= moveInputs.buttonSequence.Length) {

                ButtonPress[] compareSequence;
                int compareRange = buttonPressesList.Count - moveInputs.buttonSequence.Length;
                if (compareRange < 0) compareRange = 0;

				if (moveInputs.allowInputLeniency){
                    compareRange -= moveInputs.leniencyBuffer;
                    if (compareRange < 0) compareRange = 0;
                    compareSequence = buttonPressesList.GetRange(compareRange, buttonPressesList.Count - compareRange).ToArray();
					compareSequence = ArrayIntersect<ButtonPress>(moveInputs.buttonSequence, compareSequence);

                    // If more debug is needed, use these tools
					/*string allbp = "";
					foreach(ButtonPress bp in lastButtonPresses) allbp += " "+ bp.ToString();
					Debug.Log(move.moveName + ": lastButtonPresses (leniency): "+ allbp);

					string allbp2 = "";
					foreach(ButtonPress bp in move.buttonSequence) allbp2 += " "+ bp.ToString();
					Debug.Log(move.moveName + ": move.buttonSequence (leniency): "+ allbp2);

					if (compareSequence != null){
						allbp3 = "";
						foreach(ButtonPress bp in compareSequence) allbp3 += " "+ bp.ToString();
						Debug.Log(move.moveName + ": compareSequence (leniency): "+ allbp3);
					}*/

				}else{
                    compareSequence = buttonPressesList.GetRange(compareRange, moveInputs.buttonSequence.Length).ToArray();
                    
                    // If more debug is needed, use these tools
                    /*string allbp3 = "";
					foreach(ButtonPress bp in compareSequence) allbp3 += " "+ bp.ToString();
					Debug.Log(move.moveName + ": compareSequence: "+ allbp3);*/
                }

                if (!ArraysEqual<ButtonPress>(compareSequence, moveInputs.buttonSequence)) return false;
			}else{
				return false;
			}
            // If more debug is needed, use these tools
            //Debug.Log("Sequence for "+ move.moveName +" pass! Testing Execution:"+ ArraysEqual<ButtonPress>(buttonPress, move.buttonExecution));
        }
        else
        {
			if (moveInputs.buttonSequence.Length > 0) return false;
		}

		if (!inputUp && !moveInputs.onPressExecution) return false;
		if (inputUp && !moveInputs.onReleaseExecution) return false;
		if (!ArraysEqual<ButtonPress>(buttonPress, moveInputs.buttonExecution)) return false;

        return true;
    }

	/// <summary>
	/// 计算两个数组的按序交集（按 a1 顺序在 a2 中依次查找，允许输入宽容匹配）。
	/// </summary>
	/// <typeparam name="T">元素类型。</typeparam>
	/// <param name="a1">基准序列（招式指令序列）。</param>
	/// <param name="a2">待比较序列（玩家按键序列）。</param>
	/// <returns>交集数组；任一为空返回 null。</returns>
	private T[] ArrayIntersect<T>(T[] a1, T[] a2) {
		if (a1 == null || a2 == null) return null;
		
		EqualityComparer<T> comparer = EqualityComparer<T>.Default;
		List<T> intersection = new List<T>();
		int nextStartingPoint = 0;
		for (int i = 0; i < a1.Length; i++){ // button sequence
			bool added = false;
			for (int k = nextStartingPoint; k < a2.Length; k++){ // button presses
				if (comparer.Equals(a1[i], a2[k])) {
					intersection.Add(a2[k]);
					nextStartingPoint = k;
					added = true;
					break;
				}
			}
			if (!added) return null;
		}

		return intersection.ToArray();
	}

	/// <summary>
	/// 判断两个数组是否按序完全相等。
	/// </summary>
	/// <typeparam name="T">元素类型。</typeparam>
	/// <param name="a1">数组1。</param>
	/// <param name="a2">数组2。</param>
	/// <returns>相等返回 true。</returns>
	private bool ArraysEqual<T>(T[] a1, T[] a2) {
    	if (ReferenceEquals(a1,a2)) return true;
  		if (a1 == null || a2 == null) return false;
		if (a1.Length != a2.Length) return false;
	    EqualityComparer<T> comparer = EqualityComparer<T>.Default;
		for (int i = 0; i < a1.Length; i++){
        	if (!comparer.Equals(a1[i], a2[i])) return false;
    	}
    	return true;
	}
}
