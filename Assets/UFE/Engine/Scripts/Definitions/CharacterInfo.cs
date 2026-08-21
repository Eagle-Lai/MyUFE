using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using FPLibrary;
using UFE3D;

/// <summary>
/// 角色物理数据（PhysicsData）。
/// <para>用途：定义角色的移动、跳跃、摩擦、重量等物理属性，供 ControlsScript 在战斗物理模拟中使用。</para>
/// <para>每个 float 字段通常对应一个 Fix64 定点数字段（_前缀），后者用于保证网络对战的确定性。</para>
/// </summary>
[System.Serializable]
public class PhysicsData {
	/// <summary>
	/// 角色向前移动的速度（float 值，仅编辑器显示用）。
	/// </summary>
	public float moveForwardSpeed = 4f; // How fast this character can move forward
	/// <summary>
	/// 角色向前移动的速度（定点数，运行时实际使用）。
	/// </summary>
	public Fix64 _moveForwardSpeed = 4;
	/// <summary>
	/// 角色向后移动的速度（float 值）。
	/// </summary>
	public float moveBackSpeed = 3.5f; // How fast this character can move backwards
	/// <summary>
	/// 角色向后移动的速度（定点数，运行时实际使用）。
	/// </summary>
	public Fix64 _moveBackSpeed = 3.5; // How fast this character can move backwards
	/// <summary>
	/// 是否使用高移动摩擦：松开水平方向输入时角色立即停止。
	/// </summary>
	public bool highMovingFriction = true; // When releasing the horizontal controls character will stop imediatelly
	/// <summary>
	/// 摩擦系数：当 highMovingFriction 为 false 时使用，也用于角色被推动时。
	/// </summary>
	public float friction = 30f; // Friction used in case of highMovingFriction false. Also used when player is pushed
	/// <summary>
	/// 摩擦系数（定点数，运行时实际使用）。
	/// </summary>
	public Fix64 _friction = 30; // Friction used in case of highMovingFriction false. Also used when player is pushed

	/// <summary>
	/// 角色是否可以下蹲。
	/// </summary>
    public bool canCrouch = true;
	/// <summary>
	/// 进入下蹲状态所需的延迟帧数。
	/// </summary>
    public int crouchDelay = 2;
	/// <summary>
	/// 从下蹲恢复到站立状态所需的延迟帧数。
	/// </summary>
    public int standingDelay = 2;

	/// <summary>
	/// 角色是否可以跳跃。
	/// </summary>
	public bool canJump = true;
	/// <summary>
	/// 是否使用按键压力感应跳跃（按住越久跳得越高）。
	/// </summary>
    public bool pressureSensitiveJump = false; // How high this character will jumps
	/// <summary>
	/// 是否覆盖下蹲行为（用于某些特殊姿态）。
	/// </summary>
    public bool overrideCrouch = false;
	/// <summary>
	/// 跳跃初速度（float 值），决定跳跃高度。
	/// </summary>
	public float jumpForce = 40f; // How high this character will jumps
	/// <summary>
	/// 跳跃初速度（定点数，运行时实际使用）。
	/// </summary>
	public Fix64 _jumpForce = 40; // How high this character will jumps
	/// <summary>
	/// 最小跳跃初速度（轻按跳跃键时）。
	/// </summary>
	public float minJumpForce = 30f;
	/// <summary>
	/// 最小跳跃初速度（定点数，运行时实际使用）。
	/// </summary>
	public Fix64 _minJumpForce = 30;
	/// <summary>
	/// 最小跳跃力生效所需的按键持续帧数（压力感应跳跃用）。
	/// </summary>
    public int minJumpDelay = 4;
	/// <summary>
	/// 跳跃时水平移动的距离（float 值）。
	/// </summary>
	public float jumpDistance = 8f; // How far this character will move horizontally while jumping
	/// <summary>
	/// 跳跃时水平移动的距离（定点数，运行时实际使用）。
	/// </summary>
	public Fix64 _jumpDistance = 8; // How far this character will move horizontally while jumping
	/// <summary>
	/// 是否累加受力：角色被连续打击（juggle）时，新的力是累加到现有力上还是替换现有力。
	/// </summary>
	public bool cumulativeForce = true; // If this character is being juggled, should new forces add to or replace existing force?
	/// <summary>
	/// 二段跳/三段跳次数：允许角色在空中跳几次（1 表示只能跳一次）。
	/// </summary>
	public int multiJumps = 1; // Can this character double or triple jump? Set how many times the character can jump here
	/// <summary>
	/// 角色重量（float 值），影响受击击退距离。
	/// </summary>
	public float weight = 175;
	/// <summary>
	/// 角色重量（定点数，运行时实际使用）。
	/// </summary>
	public Fix64 _weight = 175;
	/// <summary>
	/// 跳跃启动延迟帧数。
	/// </summary>
	public int jumpDelay = 8;
	/// <summary>
	/// 落地后的硬直帧数。
	/// </summary>
	public int landingDelay = 7;
	/// <summary>
	/// 地面碰撞质量（float 值），影响与地面交互的物理效果。
	/// </summary>
	public float groundCollisionMass = 2;
	/// <summary>
	/// 地面碰撞质量（定点数，运行时实际使用）。
	/// </summary>
	public Fix64 _groundCollisionMass = 2;
}

/// <summary>
/// 头部注视（HeadLook）配置。
/// <para>用途：配置角色的头部朝向跟随目标注视（如看向对手），用于增强角色表现力。</para>
/// </summary>
[System.Serializable]
public class HeadLook {
	/// <summary>
	/// 是否启用头部注视功能。
	/// </summary>
	public bool enabled = false;
	/// <summary>
	/// 可弯曲的骨骼段列表（BendingSegment 数组）。
	/// </summary>
	public BendingSegment[] segments = new BendingSegment[0];
	/// <summary>
	/// 不受头部注视影响（不弯曲）的关节列表。
	/// </summary>
	public NonAffectedJoints[] nonAffectedJoints = new NonAffectedJoints[0];
	/// <summary>
	/// 注视目标身体部位（如头部、胸部等）。
	/// </summary>
	public BodyPart target = BodyPart.head;
	/// <summary>
	/// 注视效果强度（0~1，1 表示完全跟随目标）。
	/// </summary>
	public float effect = 1;
	/// <summary>
	/// 是否覆盖动画中的头部旋转（由本系统控制）。
	/// </summary>
	public bool overrideAnimation = true;
	/// <summary>
	/// 角色受击时是否禁用头部注视。
	/// </summary>
	public bool disableOnHit = true;
}

/// <summary>
/// 自定义按键（CustomControls）配置。
/// <para>用途：允许为角色覆盖默认的输入按键映射（行走/下蹲/跳跃/攻击按钮等），实现自定义操作方案。</para>
/// </summary>
[System.Serializable]
public class CustomControls {
	/// <summary>
	/// 是否启用自定义按键覆盖。
	/// </summary>
    public bool enabled = false;
	/// <summary>
	/// 是否覆盖（override）默认输入映射。
	/// </summary>
    public bool overrideInputs = false;
	/// <summary>
	/// 向前行走对应的引擎按键（ButtonPress 枚举）。
	/// </summary>
    public ButtonPress walkForward = ButtonPress.Forward;
	/// <summary>
	/// 向后行走对应的引擎按键。
	/// </summary>
    public ButtonPress walkBack = ButtonPress.Back;
	/// <summary>
	/// 下蹲对应的引擎按键。
	/// </summary>
    public ButtonPress crouch = ButtonPress.Down;
	/// <summary>
	/// 跳跃对应的引擎按键。
	/// </summary>
    public ButtonPress jump = ButtonPress.Up;
	/// <summary>
	/// 按钮1（轻拳）对应的引擎按键。
	/// </summary>
    public ButtonPress button1 = ButtonPress.Button1;
	/// <summary>
	/// 按钮2（中拳）对应的引擎按键。
	/// </summary>
    public ButtonPress button2 = ButtonPress.Button2;
	/// <summary>
	/// 按钮3（重拳）对应的引擎按键。
	/// </summary>
    public ButtonPress button3 = ButtonPress.Button3;
	/// <summary>
	/// 按钮4（轻脚）对应的引擎按键。
	/// </summary>
    public ButtonPress button4 = ButtonPress.Button4;
	/// <summary>
	/// 按钮5（中脚）对应的引擎按键。
	/// </summary>
    public ButtonPress button5 = ButtonPress.Button5;
	/// <summary>
	/// 按钮6（重脚）对应的引擎按键。
	/// </summary>
    public ButtonPress button6 = ButtonPress.Button6;
	/// <summary>
	/// 按钮7（自定义）对应的引擎按键。
	/// </summary>
    public ButtonPress button7 = ButtonPress.Button7;
	/// <summary>
	/// 按钮8（自定义）对应的引擎按键。
	/// </summary>
    public ButtonPress button8 = ButtonPress.Button8;
	/// <summary>
	/// 按钮9（自定义）对应的引擎按键。
	/// </summary>
    public ButtonPress button9 = ButtonPress.Button9;
	/// <summary>
	/// 按钮10（自定义）对应的引擎按键。
	/// </summary>
    public ButtonPress button10 = ButtonPress.Button10;
	/// <summary>
	/// 按钮11（自定义）对应的引擎按键。
	/// </summary>
    public ButtonPress button11 = ButtonPress.Button11;
	/// <summary>
	/// 按钮12（自定义）对应的引擎按键。
	/// </summary>
    public ButtonPress button12 = ButtonPress.Button12;
	/// <summary>
	/// 是否覆盖 Control Freak（移动端虚拟摇杆）输入。
	/// </summary>
    public bool overrideControlFreak = false;
	/// <summary>
	/// 移动端虚拟摇杆桥接组件引用（InputTouchControllerBridge）。
	/// </summary>
    public InputTouchControllerBridge controlFreak2Prefab = null;
}

/// <summary>
/// 招式集合数据（MoveSetData）。
/// <para>用途：描述一个战斗姿态下完整的基础动作和攻击招式数据，是 StanceInfo 的可编辑/克隆版本。</para>
/// <para>实现了 ICloneable，用于运行时深度克隆招式数据。</para>
/// </summary>
[System.Serializable]
public class MoveSetData: ICloneable {
	/// <summary>
	/// 本招式集合对应的战斗姿态类型。
	/// </summary>
	public CombatStances combatStance = CombatStances.Stance1; // This move set combat stance
	/// <summary>
	/// 进入该姿态时的电影化演出招式。
	/// </summary>
	public MoveInfo cinematicIntro;
	/// <summary>
	/// 离开该姿态时的电影化退场招式。
	/// </summary>
	public MoveInfo cinematicOutro;

	/// <summary>
	/// 基础动作集合（BasicMoves）。
	/// </summary>
	public BasicMoves basicMoves = new BasicMoves(); // List of basic moves
	/// <summary>
	/// 攻击招式列表（MoveInfo 数组）。
	/// </summary>
	public MoveInfo[] attackMoves = new MoveInfo[0]; // List of attack moves
	
	/// <summary>
	/// 编辑器用：基础动作折叠面板开关（Inspector 隐藏）。
	/// </summary>
	[HideInInspector] public bool enabledBasicMovesToggle;
	/// <summary>
	/// 编辑器用：基础动作展开/折叠开关（Inspector 隐藏）。
	/// </summary>
	[HideInInspector] public bool basicMovesToggle;
	/// <summary>
	/// 编辑器用：攻击招式展开/折叠开关（Inspector 隐藏）。
	/// </summary>
	[HideInInspector] public bool attackMovesToggle;


	/// <summary>
	/// 将当前 MoveSetData 转换为 StanceInfo 数据对象。
	/// </summary>
	/// <returns>包含本数据全部字段的新 StanceInfo 实例。</returns>
    public StanceInfo ConvertData() {
        StanceInfo stanceData = new StanceInfo();
        stanceData.combatStance = this.combatStance;
        stanceData.cinematicIntro = this.cinematicIntro;
        stanceData.cinematicOutro = this.cinematicOutro;
        stanceData.basicMoves = this.basicMoves;
        stanceData.attackMoves = this.attackMoves;

        return stanceData;
    }

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 替代服装（AltCostume）配置。
/// <para>用途：描述角色的备用外观/服装，支持在不同服装间切换（换色蒙版等）。</para>
/// </summary>
[System.Serializable]
public class AltCostume {
	/// <summary>
	/// 替代服装的名称。
	/// </summary>
    public string name;
	/// <summary>
	/// 角色预制体的存储模式（资源加载方式）。
	/// </summary>
    public StorageMode characterPrefabStorage = StorageMode.Legacy;
	/// <summary>
	/// 替代服装对应的预制体引用。
	/// </summary>
    public GameObject prefab;
	/// <summary>
	/// 替代服装预制体的资源路径（替代加载方式）。
	/// </summary>
    public string prefabResourcePath;
	/// <summary>
	/// 是否启用颜色蒙版（用于换色）。
	/// </summary>
    public bool enableColorMask;
	/// <summary>
	/// 颜色蒙版使用的颜色。
	/// </summary>
    public Color colorMask;
}

/// <summary>
/// 角色信息定义（CharacterInfo）。
/// <para>用途：定义角色的全部基础属性（名字、血量、能量、移动、跳跃、动画类型、姿态、招式、AI 指令等）。</para>
/// <para>对应 Unity 资产 .asset（角色配置文件），可在 UFE 的 Character Editor 中编辑。</para>
/// <para>运行时字段（playerNum/isAlt/selectedCostume/loadedMoves/currentCombatStance/当前血量和能量）用于战斗状态跟踪。</para>
/// </summary>
namespace UFE3D
{
	/// <summary>
	/// 角色信息：一个可出战角色的完整配置数据（ScriptableObject 资产）。
	/// </summary>
    [System.Serializable]
    public class CharacterInfo : ScriptableObject
    {
		/// <summary>
		/// 配置版本号（用于兼容性升级判断）。
		/// </summary>
        public float version;
		/// <summary>
		/// 小尺寸头像贴图（用于选人界面等小图）。
		/// </summary>
        public Texture2D profilePictureSmall;
		/// <summary>
		/// 大尺寸立绘贴图（用于选人界面大图展示）。
		/// </summary>
        public Texture2D profilePictureBig;
		/// <summary>
		/// 角色显示名称。
		/// </summary>
        public string characterName;
		/// <summary>
		/// 角色性别（Gender 枚举）。
		/// </summary>
        public Gender gender;
		/// <summary>
		/// 角色描述文本。
		/// </summary>
        public string characterDescription;
		/// <summary>
		/// 选人界面中角色被选中时播放的动画片段。
		/// </summary>
        public AnimationClip selectionAnimation;
		/// <summary>
		/// 选人界面中角色被选中时播放的音效。
		/// </summary>
        public AudioClip selectionSound;
		/// <summary>
		/// 角色死亡时播放的音效。
		/// </summary>
        public AudioClip deathSound;
		/// <summary>
		/// 角色身高（用于判定框缩放等）。
		/// </summary>
        public float height;
		/// <summary>
		/// 角色年龄。
		/// </summary>
        public int age;
		/// <summary>
		/// 角色血型（剧情/展示用）。
		/// </summary>
        public string bloodType;
		/// <summary>
		/// 生命值上限（默认 1000）。
		/// </summary>
        public int lifePoints = 1000;
		/// <summary>
		/// 能量槽（超必杀槽）上限。
		/// </summary>
        public int maxGaugePoints;
		/// <summary>
		/// 角色预制体的存储模式（资源加载方式）。
		/// </summary>
        public StorageMode characterPrefabStorage = StorageMode.Legacy;
		/// <summary>
		/// 角色预制体引用（必须挂有 hitBoxScript 组件）。
		/// </summary>
        public GameObject characterPrefab; // The prefab representing the character (must have hitBoxScript attached to it)
		/// <summary>
		/// 角色预制体的资源路径（Resource 加载替代方案）。
		/// </summary>
        public string prefabResourcePath; // Resource Path alternative loading
		/// <summary>
		/// 替代服装列表。
		/// </summary>
        public AltCostume[] alternativeCostumes = new AltCostume[0];
		/// <summary>
		/// 角色初始出生位置（定点向量）。
		/// </summary>
        public FPVector initialPosition;
		/// <summary>
		/// 角色初始出生旋转（定点四元数）。
		/// </summary>
        public FPQuaternion initialRotation;

		/// <summary>
		/// 角色物理属性数据（移动/跳跃/摩擦等）。
		/// </summary>
        public PhysicsData physics;
		/// <summary>
		/// 头部注视配置。
		/// </summary>
        public HeadLook headLook;
		/// <summary>
		/// 自定义按键配置。
		/// </summary>
        public CustomControls customControls;

		/// <summary>
		/// 必杀技输入宽容时间（float 值）：玩家在指令序列中每个键按下的最大间隔。
		/// </summary>
        public float executionTiming = .3f; // How fast the player needs to press each key during the execution of a special move
		/// <summary>
		/// 必杀技输入宽容时间（定点数，运行时实际使用）。
		/// </summary>
        public Fix64 _executionTiming = .3; // How fast the player needs to press each key during the execution of a special move
		/// <summary>
		/// 空中可执行的招式次数（允许在空中的连段/追加攻击次数）。
		/// </summary>
        public int possibleAirMoves = 1; // How many moves this character can perform while in the air
		/// <summary>
		/// 基础动作间切换的融合时间（float 值）。
		/// </summary>
        public float blendingTime = .1f; // The speed of transiction between basic moves
		/// <summary>
		/// 基础动作间切换的融合时间（定点数，运行时实际使用）。
		/// </summary>
        public Fix64 _blendingTime = .1; // The speed of transiction between basic moves

		/// <summary>
		/// 动画系统类型（Mecanim / Legacy）。
		/// </summary>
        public AnimationType animationType;
		/// <summary>
		/// Mecanim 人形 Avatar（动画重定向用）。
		/// </summary>
        public Avatar avatar; // Mecanim variable
		/// <summary>
		/// Mecanim 是否启用根骨骼运动（Root Motion）。
		/// </summary>
        public bool applyRootMotion; // Mecanim variable
		/// <summary>
		/// 动画流程类型（AnimationFlow 枚举）。
		/// </summary>
        public AnimationFlow animationFlow;
		/// <summary>
		/// 是否使用动画映射表（animMap，逐帧判定框映射）。
		/// </summary>
        public bool useAnimationMaps;

		/// <summary>
		/// 各姿态的资源路径列表（姿态场景/模型资源）。
		/// </summary>
        public string[] stanceResourcePath = new string[0];
		/// <summary>
		/// 各姿态的招式集合数据列表（MoveSetData 数组）。
		/// </summary>
        public MoveSetData[] moves = new MoveSetData[0];
		/// <summary>
		/// AI 指令集列表（Fuzzy AI 指令资产）。
		/// </summary>
        public AIInstructionsSet[] aiInstructionsSet = new AIInstructionsSet[0];

		/// <summary>
		/// 玩家编号（1 或 2），运行时设置。
		/// </summary>
        public int playerNum { get; set; }
		/// <summary>
		/// 是否为替代服装状态，运行时设置。
		/// </summary>
        public bool isAlt { get; set; }
		/// <summary>
		/// 当前选中的服装序号，运行时设置。
		/// </summary>
        public int selectedCostume { get; set; }
		/// <summary>
		/// 加载后的招式集合数据（运行时克隆数据），运行时设置。
		/// </summary>
        public MoveSetData[] loadedMoves { get; set; }

		#region trackable definitions
		/// <summary>
		/// 当前战斗姿态类型（运行时跟踪）。
		/// </summary>
        public CombatStances currentCombatStance { get; set; }
		/// <summary>
		/// 当前生命值（定点数，运行时跟踪）。
		/// </summary>
        public Fix64 currentLifePoints { get; set; }
		/// <summary>
		/// 当前能量槽值（定点数，运行时跟踪）。
		/// </summary>
        public Fix64 currentGaugePoints { get; set; }
		#endregion
    }
}
