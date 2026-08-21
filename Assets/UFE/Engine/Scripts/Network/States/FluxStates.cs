using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using FPLibrary;
using UFENetcode;
using UFE3D;

/// <summary>
/// 帧同步游戏状态（FluxStates）。
/// <para>用途：帧同步系统保存/恢复的完整游戏状态快照——包含网络帧号、全局状态、摄像机状态、双方角色状态</para>
/// <para>（控制/物理/招式/判定盒/动画）等所有需确定性同步的字段，供回滚（Rollback）与反同步恢复使用。</para>
/// </summary>
public struct FluxStates{
	#region public instance properties
	/// <summary>网络帧号。</summary>
	public long networkFrame {get; set;}

	/// <summary>状态追踪字典（RecordVar 字段快照）。</summary>
    public Dictionary<System.Reflection.MemberInfo, System.Object> tracker;

    // Deprecated Below
	/// <summary>全局状态（UFE/GlobalInfo 相关字段）。</summary>
    public GlobalState global;
	/// <summary>战斗 HUD 状态。</summary>
    public GUIState battleGUI;
	/// <summary>摄像机状态。</summary>
    public CameraState camera;
	/// <summary>玩家1角色状态。</summary>
    public CharacterState player1;
	/// <summary>玩家2角色状态。</summary>
    public CharacterState player2;
	#endregion

	#region public instance methods
	/// <summary>
	/// 用简单状态覆盖当前状态的生命/能量/位置（反同步恢复用）。
	/// </summary>
	/// <param name="state">简单状态快照。</param>
	public void Override(FluxSimpleState state){
		this.player1.life							= state.p1.life;
		this.player1.gauge							= state.p1.gauge;
		this.player1.shellTransform.position		= state.p1.position;
		this.player1.shellTransform.fpPosition		= FPVector.ToFPVector(state.p1.position);

		this.player2.life							= state.p2.life;
		this.player2.gauge							= state.p2.gauge;
		this.player2.shellTransform.position		= state.p2.position;
        this.player2.shellTransform.fpPosition      = FPVector.ToFPVector(state.p2.position);

        this.networkFrame = state.frame;
	}
	#endregion

	#region struct definitions
	/// <summary>
	/// 全局状态：UFE 与 GlobalInfo 的全局字段快照。
	/// </summary>
    public struct GlobalState {
        // UFE
		/// <summary>是否自由摄像机。</summary>
        public bool freeCamera;
		/// <summary>是否冻结物理。</summary>
        public bool freezePhysics;
		/// <summary>是否已广播新回合。</summary>
        public bool newRoundCasted;
		/// <summary>是否标准化摄像机。</summary>
        public bool normalizedCam;
		/// <summary>是否暂停计时器。</summary>
        public bool pauseTimer;
		/// <summary>剩余时间。</summary>
        public Fix64 timer;
		/// <summary>延迟动作列表。</summary>
        public List<DelayedAction> delayedActions;
		/// <summary>实例化对象状态列表。</summary>
        public List<InstantiatedGameObjectState> instantiatedObjects;

        // GlobalInfo
		/// <summary>当前回合数。</summary>
        public int currentRound;
		/// <summary>是否锁定输入。</summary>
        public bool lockInputs;
		/// <summary>是否锁定移动。</summary>
        public bool lockMovements;
		/// <summary>时间倍率。</summary>
        public Fix64 timeScale;
    }
    
	/// <summary>
	/// 实例化对象状态：帧同步实例化对象的快照。
	/// </summary>
    public struct InstantiatedGameObjectState {
		/// <summary>游戏对象。</summary>
        public GameObject gameObject;
		/// <summary>MrFusion 组件。</summary>
        public MrFusion mrFusion;
		/// <summary>创建帧。</summary>
        public long creationFrame;
		/// <summary>销毁帧。</summary>
        public long? destructionFrame;
		/// <summary>变换状态。</summary>
        public TransformState transformState;
    }

	/// <summary>
	/// 变换状态：Transform 的定点/浮点位置旋转缩放快照。
	/// </summary>
    public struct TransformState {
		/// <summary>定点位置。</summary>
        public FPVector fpPosition;
		/// <summary>定点旋转。</summary>
        public FPQuaternion fpRotation;
		/// <summary>世界位置。</summary>
        public Vector3 position;
		/// <summary>本地位置。</summary>
        public Vector3 localPosition;
		/// <summary>世界旋转。</summary>
        public Quaternion rotation;
		/// <summary>本地旋转。</summary>
        public Quaternion localRotation;
		/// <summary>本地缩放。</summary>
        public Vector3 localScale;
		/// <summary>是否激活。</summary>
        public bool active;
    }

	/// <summary>
	/// GUI 状态：战斗 HUD 的快照（当前为空，预留）。
	/// </summary>
    public struct GUIState {

    }

	/// <summary>
	/// 摄像机状态：摄像机与淡入淡出相关字段快照。
	/// </summary>
    public struct CameraState {
        // Transform
		/// <summary>本地位置。</summary>
        public Vector3 localPosition;
		/// <summary>本地旋转。</summary>
        public Quaternion localRotation;
		/// <summary>世界位置。</summary>
        public Vector3 position;
		/// <summary>世界旋转。</summary>
        public Quaternion rotation;

		/// <summary>是否存在摄像机脚本。</summary>
		public bool cameraScript;
		/// <summary>电影化冻结。</summary>
        public bool cinematicFreeze;
		/// <summary>当前注视位置。</summary>
        public Vector3 currentLookAtPosition;
		/// <summary>是否启用。</summary>
        public bool enabled;
		/// <summary>视野。</summary>
        public float fieldOfView;
		/// <summary>自由模式速度。</summary>
        public float freeCameraSpeed;
		/// <summary>控制者。</summary>
        public string lastOwner;
		/// <summary>是否停止移动。</summary>
        public bool killCamMove;
		/// <summary>移动速度。</summary>
        public float movementSpeed;
		/// <summary>旋转速度。</summary>
        public float rotationSpeed;
		/// <summary>标准距离。</summary>
        public float standardDistance;
		/// <summary>标准地面高度。</summary>
        public float standardGroundHeight;
		/// <summary>目标位置。</summary>
        public Vector3 targetPosition;
		/// <summary>目标旋转。</summary>
        public Quaternion targetRotation;
		/// <summary>目标视野。</summary>
        public float targetFieldOfView;

		// Camera Fade
		/// <summary>是否存在淡入淡出。</summary>
        public bool cameraFade;
		/// <summary>当前遮罩颜色。</summary>
        public Color currentScreenOverlayColor;
    }

	/// <summary>
	/// 角色状态：一名角色的完整战斗状态快照（控制/物理/招式/判定盒/动画）。
	/// </summary>
    public struct CharacterState {
		// ControlsScript
		/// <summary>是否存在控制脚本。</summary>
		public bool controlsScript;

        // Transforms
		/// <summary>外壳变换状态。</summary>
        public TransformState shellTransform;
		/// <summary>角色模型变换状态。</summary>
        public TransformState characterTransform;

        // Global Properties
		/// <summary>战斗姿态。</summary>
        public CombatStances combatStance;
		/// <summary>生命值。</summary>
        public Fix64 life;
		/// <summary>能量值。</summary>
        public Fix64 gauge;

        // Control
		/// <summary>AFK 计时器。</summary>
        public Fix64 afkTimer;
        public int airJuggleHits;
        public AirRecoveryType airRecoveryType;
        public bool applyRootMotion;
        public bool blockStunned;
        public Fix64 comboDamage;
        public Fix64 comboHitDamage;
        public int comboHits;
        public int consecutiveCrumple;
        public BasicMoveReference currentBasicMove;
        public Fix64 currentDrained;
        public Hit currentHit;
        public string currentHitAnimation;
        public PossibleStates currentState;
        public SubStates currentSubState;
        public MoveInfo DCMove;
        public CombatStances DCStance;
        public bool firstHit;
        public Fix64 gaugeDPS;
        public bool hitDetected;
        public Fix64 hitAnimationSpeed;
        public Fix64 hitStunDeceleration;
        public Fix64 horizontalForce;
        public bool inhibitGainWhileDraining;
        public bool isAirRecovering;
        public bool isBlocking;
        public bool isDead;
        public bool ignoreCollisionMass;
        public bool introPlayed;
        public bool lit;
        public int mirror;
        public Fix64 normalizedDistance;
        public Fix64 normalizedJumpArc;
        public bool outroPlayed;
        public bool potentialBlock;
        public Fix64 potentialParry;
        public bool roundMsgCasted;
        public int roundsWon;
        public bool shakeCamera;
        public bool shakeCharacter;
        public Fix64 shakeDensity;
        public Fix64 shakeCameraDensity;
        public StandUpOptions standUpOverride;
        public Fix64 standardYRotation;
        public Fix64 storedMoveTime;
        public Fix64 stunTime;
        public Fix64 totalDrain;

        // Sub Classes
        public PullInState activePullIn;
        public MoveState currentMove;
        public MoveState storedMove;

        // Core Scripts
        public PhysicsState physics;
        public MoveSetState moveSet;
        public HitBoxesState hitBoxes;
        public AnimatiorState animator;

        // Arrays
        //public List<ProjectileMoveScript> projectiles;
        public Dictionary<ButtonPress, Fix64> inputHeldDown;
		public List<ProjectileMoveScript> projectiles;

        // Nested Structs
        public struct PullInState {
            public PullIn pullIn;
            public FPVector position;
        }

        public struct MoveState {
            public MoveInfo move;
            //public bool cancelable;
            public bool kill;
            public int armorHits;
            public int currentFrame;
            public int overrideStartupFrame;
            public Fix64 animationSpeedTemp;
            public Fix64 currentTick;
            public bool hitConfirmOnBlock;
            public bool hitConfirmOnParry;
            public bool hitConfirmOnStrike;
            public bool hitAnimationOverride;
            public StandUpOptions standUpOptions;
            public CurrentFrameData currentFrameData;
            public bool[] hitStates;
            public bool[] frameLinkStates;
            public bool[] castedBodyPartVisibilityChange;
            public bool[] castedProjectile;
            public bool[] castedAppliedForce;
            public bool[] castedMoveParticleEffect;
            public bool[] castedSlowMoEffect;
            public bool[] castedSoundEffect;
            public bool[] castedInGameAlert;
            public bool[] castedStanceChange;
            public bool[] castedCameraMovement;
            public Fix64[] cameraTime;
            public bool[] cameraOver;
            public bool[] castedOpponentOverride;
        }

        public struct PhysicsState {
            public Fix64 airTime;
            public Fix64 appliedGravity;
            public int currentAirJumps;
            public bool freeze;
            public int groundBounceTimes;
            public Fix64 horizontalForce;
            public bool isGroundBouncing;
            public bool isLanding;
            public bool isTakingOff;
            public bool isWallBouncing;
            public Fix64 moveDirection;
            public bool overrideAirAnimation;
            public BasicMoveInfo overrideStunAnimation;
            public Fix64 verticalForce;
            public Fix64 verticalTotalForce;
            public int wallBounceTimes;

        }

        public struct MoveSetState {
            public int totalAirMoves;
            public bool animationPaused;
            public Fix64 overrideNextBlendingValue;
            public Fix64 lastTimePress;
            public AnimatiorState animator;

            public Dictionary<ButtonPress, Fix64> chargeValues;
            public List<ButtonSequenceRecord> lastButtonPresses;
        }

        public struct HitBoxesState {
            public bool isHit;
            public HitConfirmType hitConfirmType;
            public Fix64 collisionBoxSize;
            public bool currentMirror;
            public bool bakeSpeed;
            public AnimationMap[] animationMaps;

            public HitBoxState[] hitBoxes;
            public HurtBoxState[] activeHurtBoxes;
            public BlockAreaState blockableArea;
        }

        public struct HitBoxState {
            public HitBox hitBox;
            public Rect rendererBounds;
            public int state;
            public bool hide;
            public bool visibility;
        }

        public struct HurtBoxState {
            public HurtBox hurtBox;
            public Rect rendererBounds;
            public bool isBlock;
            public FPVector position;
        }

        public struct BlockAreaState {
            public BlockArea blockArea;
            public FPVector position;
        }

        public struct AnimatiorState {
            public AnimationDataState currentAnimationData;
            public bool currentMirror;

            // Mecanim Control
            public Fix64 currentNormalizedTime;
            public string currentState;
            public Fix64 currentSpeed;
            public RuntimeAnimatorController overrideController;

            // MC3
            public int currentInput;
            public int transitionDuration;
            public int transitionTime;
            public Fix64[] weightList;
            public Fix64[] speedList;
            public Fix64[] timeList;

            // Legacy
            public Fix64 globalSpeed;
            public Vector3 lastPosition;
        }

        public struct AnimationDataState {
            public LegacyAnimationData legacyAnimationData;
            //public MC3AnimationData mecanimAnimationData;
            public MecanimAnimationData mecanimAnimationData;

            // Legacy
            public AnimationState animState;

            // Mecanim
            public Fix64 secondsPlayed;
            public int timesPlayed;
            public Fix64 speed;

            // Both
            public Fix64 normalizedTime;
        }
        #endregion
    }
}
