using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FPLibrary;
using UFE3D;

/// <summary>
/// 招式信息定义（MoveInfo）。
/// <para>用途：本文件是 UFE 出招/战斗系统的核心数据定义，包含全部与"招式（Move）"相关的枚举、数据结构与配置类。</para>
/// <para>核心类 UFE3D.MoveInfo（ScriptableObject）描述一个完整招式（帧数据/输入指令/命中判定/特效/飞行道具/无敌/霸体等）。</para>
/// <para>ButtonPress 枚举是输入到引擎动作的映射基础（数值与 InputReferences.engineRelatedButton 对应）。</para>
/// </summary>

/// <summary>
/// 按钮输入（ButtonPress）：引擎可识别的全部输入动作。
/// <para>0~3 为方向，4~15 为按钮1~12，16~19 为斜方向，20 为 Start。</para>
/// </summary>
public enum ButtonPress {
	/// <summary>前方向。</summary>
	Forward,
	/// <summary>后方向。</summary>
	Back,
	/// <summary>上方向。</summary>
	Up,
	/// <summary>下方向。</summary>
    Down,
	/// <summary>按钮1（通常轻拳）。</summary>
	Button1,
	/// <summary>按钮2（通常中拳）。</summary>
	Button2,
	/// <summary>按钮3（通常重拳）。</summary>
	Button3,
	/// <summary>按钮4（通常轻脚）。</summary>
	Button4,
	/// <summary>按钮5（通常中脚）。</summary>
	Button5,
	/// <summary>按钮6（通常重脚）。</summary>
	Button6,
	/// <summary>按钮7（自定义）。</summary>
	Button7,
	/// <summary>按钮8（自定义）。</summary>
	Button8,
	/// <summary>按钮9（自定义）。</summary>
	Button9,
	/// <summary>按钮10（自定义）。</summary>
	Button10,
	/// <summary>按钮11（自定义）。</summary>
	Button11,
	/// <summary>按钮12（自定义）。</summary>
    Button12,
	/// <summary>下+后 斜方向。</summary>
    DownBack,
	/// <summary>下+前 斜方向。</summary>
    DownForward,
	/// <summary>上+前 斜方向。</summary>
    UpForward,
	/// <summary>上+后 斜方向。</summary>
    UpBack,
	/// <summary>Start 键。</summary>
	Start
}

/// <summary>
/// 基础动作引用（BasicMoveReference）：角色基础动作的标识枚举。
/// <para>包括待机/移动/跳跃/受击/倒地/起身/格挡/弹反等全部基础状态动作。</para>
/// </summary>
public enum BasicMoveReference {
	/// <summary>待机。</summary>
	Idle,
	/// <summary>前进。</summary>
	MoveForward,
	/// <summary>后退。</summary>
    MoveBack,
	/// <summary>下蹲。</summary>
    Crouching,
	/// <summary>起跳。</summary>
	TakeOff,
	/// <summary>垂直跳跃。</summary>
	JumpStraight,
	/// <summary>向后跳跃。</summary>
	JumpBack,
	/// <summary>向前跳跃。</summary>
	JumpForward,
	/// <summary>垂直下落。</summary>
	FallStraight,
	/// <summary>向后下落。</summary>
	FallBack,
	/// <summary>向前下落。</summary>
	FallForward,
	/// <summary>落地。</summary>
    Landing,
	/// <summary>站立格挡姿态。</summary>
    BlockingHighPose,
	/// <summary>站立格挡受击。</summary>
    BlockingHighHit,
	/// <summary>低位格挡受击。</summary>
    BlockingLowHit,
	/// <summary>下蹲格挡姿态。</summary>
	BlockingCrouchingPose,
	/// <summary>下蹲格挡受击。</summary>
	BlockingCrouchingHit,
	/// <summary>空中格挡姿态。</summary>
	BlockingAirPose,
	/// <summary>空中格挡受击。</summary>
    BlockingAirHit,
	/// <summary>高位弹反。</summary>
    ParryHigh,
	/// <summary>低位弹反。</summary>
    ParryLow,
	/// <summary>下蹲弹反。</summary>
	ParryCrouching,
	/// <summary>空中弹反。</summary>
    ParryAir,
	/// <summary>站立高位受击。</summary>
    HitStandingHigh,
	/// <summary>站立低位受击。</summary>
    HitStandingLow,
	/// <summary>站立下蹲受击。</summary>
    HitStandingCrouching,
	/// <summary>空中连击受击。</summary>
    HitAirJuggle,
	/// <summary>击退受击。</summary>
    HitKnockBack,
	/// <summary>站立高位击倒受击。</summary>
    HitStandingHighKnockdown,
	/// <summary>站立中位击倒受击。</summary>
    HitStandingMidKnockdown,
	/// <summary>扫腿击倒受击。</summary>
    HitSweep,
	/// <summary>破防（crumple）受击。</summary>
    HitCrumple,
	/// <summary>地面弹跳。</summary>
	StageGroundBounce,
	/// <summary>站立墙壁弹跳。</summary>
    StageStandingWallBounce,
	/// <summary>站立墙壁弹跳击倒。</summary>
    StageStandingWallBounceKnockdown,
	/// <summary>空中墙壁弹跳。</summary>
    StageAirWallBounce,
	/// <summary>默认倒地。</summary>
    FallDownDefault,
	/// <summary>空中连击后倒地。</summary>
    FallDownFromAirJuggle,
	/// <summary>地面弹跳后倒地。</summary>
    FallDownFromGroundBounce,
	/// <summary>空中受身恢复。</summary>
    AirRecovery,
	/// <summary>默认起身。</summary>
	StandUpDefault,
	/// <summary>空中连击后起身。</summary>
    StandUpFromAirJuggle,
	/// <summary>击退后起身。</summary>
    StandUpFromKnockBack,
	/// <summary>站立高位受击后起身。</summary>
    StandUpFromStandingHighHit,
	/// <summary>站立中位受击后起身。</summary>
    StandUpFromStandingMidHit,
	/// <summary>扫腿后起身。</summary>
    StandUpFromSweep,
	/// <summary>破防后起身。</summary>
    StandUpFromCrumple,
	/// <summary>站立墙壁弹跳后起身。</summary>
    StandUpFromStandingWallBounce,
	/// <summary>空中墙壁弹跳后起身。</summary>
    StandUpFromAirWallBounce,
	/// <summary>地面弹跳后起身。</summary>
    StandUpFromGroundBounce
}

/// <summary>
/// 动画片段编号：基础动作可配置的动画片段数量。
/// </summary>
public enum ClipNum {
	/// <summary>片段1。</summary>
	Clip1,
	/// <summary>片段2。</summary>
	Clip2,
	/// <summary>片段3。</summary>
	Clip3,
	/// <summary>片段4。</summary>
	Clip4,
	/// <summary>片段5。</summary>
	Clip5,
	/// <summary>片段6。</summary>
	Clip6
}

/// <summary>
/// 起身选项：倒地后起身使用的动画片段类型。
/// </summary>
public enum StandUpOptions {
	/// <summary>无。</summary>
	None,
	/// <summary>默认起身片段。</summary>
	DefaultClip,
	/// <summary>高位击倒起身片段。</summary>
	HighKnockdownClip,
	/// <summary>低位击倒起身片段。</summary>
	LowKnockdownClip,
	/// <summary>扫腿击倒起身片段。</summary>
	SweepClip,
	/// <summary>空中连击后起身片段。</summary>
    AirJuggleClip,
	/// <summary>击退后起身片段。</summary>
    KnockBackClip,
	/// <summary>破防后起身片段。</summary>
    CrumpleClip,
	/// <summary>站立墙壁弹跳后起身片段。</summary>
    StandingWallBounceClip,
	/// <summary>空中墙壁弹跳后起身片段。</summary>
    AirWallBounceClip,
	/// <summary>地面弹跳后起身片段。</summary>
    GroundBounceClip
}

/// <summary>
/// 输入类型：输入项的类别。
/// </summary>
public enum InputType {
	/// <summary>水平轴。</summary>
	HorizontalAxis,
	/// <summary>垂直轴。</summary>
	VerticalAxis,
	/// <summary>按钮。</summary>
	Button
}

/// <summary>
/// 可能状态（PossibleStates）：角色的主状态枚举。
/// </summary>
public enum PossibleStates {
	/// <summary>站立。</summary>
	Stand,
	/// <summary>下蹲。</summary>
	Crouch,
	/// <summary>垂直跳跃。</summary>
	NeutralJump,
	/// <summary>向前跳跃。</summary>
	ForwardJump,
	/// <summary>向后跳跃。</summary>
	BackJump,
	/// <summary>倒地/躺下。</summary>
	Down
}

/// <summary>
/// 子状态（SubStates）：主状态下的细分状态。
/// </summary>
public enum SubStates {
	/// <summary>静止。</summary>
	Resting,
	/// <summary>向前移动。</summary>
	MovingForward,
	/// <summary>向后移动。</summary>
	MovingBack,
	/// <summary>格挡中。</summary>
	Blocking,
	/// <summary>眩晕中。</summary>
	Stunned
}

/// <summary>
/// 战斗姿态：角色可切换的战斗架势（最多10种）。
/// </summary>
public enum CombatStances {
	/// <summary>姿态1。</summary>
	Stance1,
	/// <summary>姿态2。</summary>
	Stance2,
	/// <summary>姿态3。</summary>
	Stance3,
	/// <summary>姿态4。</summary>
	Stance4,
	/// <summary>姿态5。</summary>
	Stance5,
	/// <summary>姿态6。</summary>
	Stance6,
	/// <summary>姿态7。</summary>
	Stance7,
	/// <summary>姿态8。</summary>
	Stance8,
	/// <summary>姿态9。</summary>
	Stance9,
	/// <summary>姿态10。</summary>
	Stance10
}

/// <summary>
/// 伤害类型：伤害值的计算方式。
/// </summary>
public enum DamageType {
	/// <summary>百分比（基于生命上限）。</summary>
	Percentage,
	/// <summary>固定点数。</summary>
	Points
}

/// <summary>
/// 攻击类型：招式的攻击性质分类。
/// </summary>
public enum AttackType {
	/// <summary>中性（无分类）。</summary>
	Neutral,
	/// <summary>普通攻击。</summary>
	NormalAttack,
	/// <summary>向前击飞（Launcher）。</summary>
	ForwardLauncher,
	/// <summary>向后击飞。</summary>
	BackLauncher,
	/// <summary>俯冲攻击。</summary>
	Dive,
	/// <summary>对空攻击。</summary>
	AntiAir,
	/// <summary>飞行道具。</summary>
	Projectile
}

/// <summary>
/// 能量槽消耗：招式使用所需的能量槽量。
/// </summary>
public enum GaugeUsage {
	/// <summary>任意（不限）。</summary>
	Any,
	/// <summary>不消耗能量。</summary>
	None,
	/// <summary>四分之一。</summary>
	Quarter,
	/// <summary>一半。</summary>
	Half,
	/// <summary>四分之三。</summary>
	ThreeQuarters,
	/// <summary>全部。</summary>
	All
}

/// <summary>
/// 飞行道具类型：气功弹的形态类别。
/// </summary>
public enum ProjectileType {
	/// <summary>发射物。</summary>
	Shot,
	/// <summary>光束。</summary>
	Beam
}

/// <summary>
/// 命中确认类型：命中的判定类别。
/// </summary>
public enum HitConfirmType {
	/// <summary>普通命中。</summary>
	Hit,
	/// <summary>投技。</summary>
	Throw
}

/// <summary>
/// 命中类型：招式的攻击判定性质。
/// </summary>
public enum HitType {
	/// <summary>中段。</summary>
	Mid,
	/// <summary>下段。</summary>
	Low,
	/// <summary>上段（需站立防御）。</summary>
	Overhead,
	/// <summary>击飞。</summary>
	Launcher,
	/// <summary>高位击倒。</summary>
	HighKnockdown,
	/// <summary>中位击倒。</summary>
	MidKnockdown,
	/// <summary>击退。</summary>
	KnockBack,
	/// <summary>扫腿击倒。</summary>
	Sweep
}

/// <summary>
/// 命中强度：招式命中的威力等级（决定命中效果档位）。
/// </summary>
public enum HitStrengh {
	/// <summary>轻。</summary>
	Weak,
	/// <summary>中。</summary>
	Medium,
	/// <summary>重。</summary>
	Heavy,
	/// <summary>破防。</summary>
	Crumple,
	/// <summary>自定义1。</summary>
	Custom1,
	/// <summary>自定义2。</summary>
	Custom2,
	/// <summary>自定义3。</summary>
	Custom3
}

/// <summary>
/// 受击硬直类型：命中后对方硬直的计算方式。
/// </summary>
public enum HitStunType {
	/// <summary>帧优势（按差值计算）。</summary>
	FrameAdvantage,
	/// <summary>固定帧数。</summary>
	Frames,
	/// <summary>秒数。</summary>
	Seconds
}

/// <summary>
/// 链接类型：连段取消（FrameLink）的触发条件。
/// </summary>
public enum LinkType {
	/// <summary>命中确认后取消。</summary>
	HitConfirm,
	/// <summary>反击技（Counter Move）。</summary>
	CounterMove,
	/// <summary>无条件取消。</summary>
	NoConditions
}

/// <summary>
/// 反击技类型：Counter Move 的判定方式。
/// </summary>
public enum CounterMoveType {
	/// <summary>按招式过滤条件。</summary>
	MoveFilter,
	/// <summary>指定特定招式。</summary>
	SpecificMove
}

/// <summary>
/// 电影化演出类型：招式动画演出的实现方式。
/// </summary>
public enum CinematicType {
	/// <summary>摄像机编辑器。</summary>
	CameraEditor,
	/// <summary>动画文件。</summary>
	AnimationFile,
	/// <summary>预制体。</summary>
	Prefab
}

/// <summary>
/// 角色距离：与对手的距离类别（AI 决策用）。
/// </summary>
public enum CharacterDistance {
	/// <summary>任意。</summary>
	Any,
	/// <summary>非常近。</summary>
	VeryClose,
	/// <summary>近。</summary>
	Close,
	/// <summary>中。</summary>
	Mid,
	/// <summary>远。</summary>
	Far,
	/// <summary>非常远。</summary>
	VeryFar,
	/// <summary>其他。</summary>
	Other
}

/// <summary>
/// 帧速度档位：招式出招/收招速度的分类（AI 决策用）。
/// </summary>
public enum FrameSpeed {
	/// <summary>任意。</summary>
	Any,
	/// <summary>非常慢。</summary>
	VerySlow,
	/// <summary>慢。</summary>
	Slow,
	/// <summary>正常。</summary>
	Normal,
	/// <summary>快。</summary>
	Fast,
	/// <summary>非常快。</summary>
	VeryFast
}

/// <summary>
/// 跳跃弧线阶段：跳跃过程中的阶段分类（AI 决策用）。
/// </summary>
public enum JumpArc {
	/// <summary>任意。</summary>
	Any,
	/// <summary>起跳。</summary>
	TakeOff,
	/// <summary>跳跃中。</summary>
	Jumping,
	/// <summary>顶点。</summary>
	Top,
	/// <summary>下落。</summary>
	Falling,
	/// <summary>落地。</summary>
	Landing,
	/// <summary>其他。</summary>
	Other
}

/// <summary>
/// 当前帧数据阶段：招式执行过程中所处的帧阶段。
/// </summary>
public enum CurrentFrameData {
	/// <summary>任意。</summary>
	Any,
	/// <summary>前摇帧。</summary>
	StartupFrames,
	/// <summary>判定生效帧。</summary>
	ActiveFrames,
	/// <summary>后摇帧。</summary>
	RecoveryFrames
}


/// <summary>
/// 粒子效果信息：招式中的粒子特效配置。
/// </summary>
[System.Serializable]
public class ParticleInfo : ICloneable
{
	/// <summary>编辑器用：面板折叠开关。</summary>
    public bool editorToggle;
	/// <summary>粒子特效预制体。</summary>
    public GameObject prefab;
	/// <summary>特效持续时间（秒）。</summary>
    public float duration = 1;
	/// <summary>特效是否附着在角色身体上。</summary>
    public bool stick = false;
	/// <summary>招式移动结束后是否销毁特效。</summary>
    public bool destroyOnMoveOver = false;
	/// <summary>是否跟随角色旋转。</summary>
    public bool followRotation = false;
	/// <summary>是否锁定本地位置。</summary>
    public bool lockLocalPosition = false;
	/// <summary>在 2P 侧是否镜像。</summary>
    public bool mirrorOn2PSide = false;
	/// <summary>初始旋转。</summary>
    public Vector3 initialRotation;
	/// <summary>位置偏移。</summary>
    public Vector3 positionOffSet;
	/// <summary>特效绑定的身体部位。</summary>
    public BodyPart bodyPart;

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}

/// <summary>
/// 身体部位可见性变化：招式特定帧改变身体部位（判定盒）的可见性。
/// </summary>
[System.Serializable]
public class BodyPartVisibilityChange: ICloneable {
	/// <summary>触发帧。</summary>
	public int castingFrame;
	/// <summary>目标身体部位。</summary>
	public BodyPart bodyPart;
	/// <summary>是否可见。</summary>
	public bool visible;
	/// <summary>是否作用于左侧身体。</summary>
	public bool left;
	/// <summary>是否作用于右侧身体。</summary>
	public bool right;

	/// <summary>是否已触发（运行时跟踪）。</summary>
    public bool casted{get; set;}
	
	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 飞行道具（Projectile）：招式发射的气功弹等飞行物配置。
/// </summary>
[System.Serializable]
public class Projectile: ICloneable {
	/// <summary>发射帧。</summary>
	public int castingFrame = 1;
	/// <summary>飞行道具预制体。</summary>
	public GameObject projectilePrefab;
	/// <summary>命中特效预制体。</summary>
	public GameObject impactPrefab;

	/// <summary>发射点绑定的身体部位。</summary>
	public BodyPart bodyPart;
	/// <summary>发射位置偏移（float 值）。</summary>
	public Vector3 castingOffSet;
	/// <summary>发射位置偏移（定点向量，运行时实际使用）。</summary>
	public FPVector _castingOffSet;
	/// <summary>飞行速度。</summary>
	public int speed = 20;
	/// <summary>发射方向角度。</summary>
	public int directionAngle;
	/// <summary>飞行道具持续时间（float 值，秒）。</summary>
    public float duration = 5f;
	/// <summary>飞行道具持续时间（定点数，运行时实际使用）。</summary>
	public Fix64 _duration = 5;
	/// <summary>命中特效持续时间。</summary>
	public float impactDuration = 1;
	/// <summary>是否固定 Z 轴。</summary>
	public bool fixedZAxis;
	/// <summary>飞行道具之间是否碰撞。</summary>
	public bool projectileCollision;
	/// <summary>是否不可格挡。</summary>
    public bool unblockable;
	/// <summary>2P 侧是否镜像。</summary>
    public bool mirrorOn2PSide;

	/// <summary>攻击判定盒。</summary>
	public HitBox hitBox;
	/// <summary>受击判定盒。</summary>
	public HurtBox hurtBox;
	/// <summary>可格挡区域。</summary>
	public BlockArea blockableArea;

	/*public HitBoxShape shape;
	public Rect rect = new Rect(0, 0, 4, 4);
	public bool followXBounds;
	public bool followYBounds;
	public float hitRadius;
	public Vector2 hitOffSet;*/
	
	/// <summary>多段命中之间的间隔档位。</summary>
	public Sizes spaceBetweenHits;
	/// <summary>总命中段数。</summary>
	public int totalHits = 1;
	/// <summary>是否重置之前的受击硬直。</summary>
	public bool resetPreviousHitStun = true;
	/// <summary>命中时的受击硬直（帧）。</summary>
	public int hitStunOnHit;
	/// <summary>格挡时的受击硬直（帧）。</summary>
    public int hitStunOnBlock;
	
	/// <summary>是否覆盖默认命中特效。</summary>
	public bool overrideHitEffects;
	/// <summary>是否破霸体（armor breaker）。</summary>
    public bool armorBreaker;
	/// <summary>自定义命中特效配置。</summary>
	public HitTypeOptions hitEffects;

	/// <summary>是否命中站立/地面目标。</summary>
	public bool groundHit;
	/// <summary>是否命中空中目标。</summary>
	public bool airHit;
	/// <summary>是否命中倒地目标。</summary>
	public bool downHit;

	/// <summary>伤害类型。</summary>
	public DamageType damageType;
	/// <summary>命中伤害（float 值）。</summary>
	public float damageOnHit;
	/// <summary>命中伤害（定点数，运行时实际使用）。</summary>
	public Fix64 _damageOnHit;
	/// <summary>格挡伤害（float 值）。</summary>
	public float damageOnBlock;
	/// <summary>格挡伤害（定点数，运行时实际使用）。</summary>
	public Fix64 _damageOnBlock;
	/// <summary>是否应用伤害衰减（连击衰减）。</summary>
	public bool damageScaling;


	/// <summary>是否遵循方向性命中。</summary>
    public bool obeyDirectionalHit = true;
	/// <summary>命中时是否触发命中特效。</summary>
    public bool hitEffectsOnHit = true;
	/// <summary>命中推挤力（Vector2，float 版本）。</summary>
	public Vector2 pushForce;
	/// <summary>是否重置之前水平推挤力。</summary>
	public bool resetPreviousHorizontalPush;
	/// <summary>是否重置之前垂直推挤力。</summary>
	public bool resetPreviousVerticalPush;
	/// <summary>是否应用不同的空中力。</summary>
    public bool applyDifferentAirForce;
	/// <summary>是否应用不同的格挡力。</summary>
    public bool applyDifferentBlockForce;
	/// <summary>推挤力（定点向量，运行时实际使用）。</summary>
    public FPVector _pushForce;
	/// <summary>空中推挤力（定点向量，运行时实际使用）。</summary>
    public FPVector _pushForceAir;
	/// <summary>格挡推挤力（定点向量，运行时实际使用）。</summary>
    public FPVector _pushForceBlock;
	/// <summary>命中强度。</summary>
    public HitStrengh hitStrength;
	/// <summary>命中类型。</summary>
	public HitType hitType;

	/// <summary>命中后链接的招式（取消链）。</summary>
    public MoveInfo moveLinkOnStrike;
	/// <summary>格挡后链接的招式。</summary>
    public MoveInfo moveLinkOnBlock;
	/// <summary>弹反后链接的招式。</summary>
    public MoveInfo moveLinkOnParry;
	/// <summary>是否强制落地。</summary>
    public bool forceGrounded;
    
	/// <summary>编辑器用：招式链接面板开关。</summary>
	[HideInInspector] public bool moveLinksToggle;
	/// <summary>编辑器用：伤害选项面板开关。</summary>
	[HideInInspector] public bool damageOptionsToggle;
	/// <summary>编辑器用：硬直选项面板开关。</summary>
	[HideInInspector] public bool hitStunOptionsToggle;
	/// <summary>编辑器用：预览开关。</summary>
	[HideInInspector] public bool preview;

    #region trackable definitions
	/// <summary>是否已发射（运行时跟踪）。</summary>
    public bool casted{get; set;}
	/// <summary>命中时的能量回复（运行时跟踪）。</summary>
    public Fix64 gaugeGainOnHit{get; set;}
	/// <summary>格挡时的能量回复（运行时跟踪）。</summary>
    public Fix64 gaugeGainOnBlock { get; set; }
	/// <summary>对方命中时的能量回复（运行时跟踪）。</summary>
    public Fix64 opGaugeGainOnHit { get; set; }
	/// <summary>对方格挡时的能量回复（运行时跟踪）。</summary>
    public Fix64 opGaugeGainOnBlock { get; set; }
	/// <summary>对方弹反时的能量回复（运行时跟踪）。</summary>
    public Fix64 opGaugeGainOnParry { get; set; }
	/// <summary>飞行道具当前位置（运行时跟踪）。</summary>
    public Transform position{get; set;}
    #endregion

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 无敌身体部位：招式特定帧内指定身体部位无敌（不可被命中）。
/// </summary>
[System.Serializable]
public class InvincibleBodyParts: ICloneable {
	/// <summary>无敌的身体部位列表。</summary>
	public BodyPart[] bodyParts = new BodyPart[0];
	/// <summary>是否完全无敌（所有部位）。</summary>
	public bool completelyInvincible = true;
	/// <summary>是否忽略身体碰撞体。</summary>
	public bool ignoreBodyColliders = false;
	/// <summary>无敌生效起始帧。</summary>
	public int activeFramesBegin;
	/// <summary>无敌生效结束帧。</summary>
	public int activeFramesEnds;

	/// <summary>关联的判定盒（Inspector 隐藏，运行时使用）。</summary>
	[HideInInspector]	public HitBox[] hitBoxes;
	
	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}


/// <summary>
/// 施加力（AppliedForce）：招式特定帧对角色施加的推力。
/// </summary>
[System.Serializable]
public class AppliedForce: ICloneable {
	/// <summary>触发帧。</summary>
	public int castingFrame;
	/// <summary>是否重置之前的垂直方向力。</summary>
	public bool resetPreviousVertical;
	/// <summary>是否重置之前的水平方向力。</summary>
	public bool resetPreviousHorizontal;
	/// <summary>施加的力（Vector2，float 版本）。</summary>
	public Vector2 force;
	/// <summary>施加的力（定点向量，运行时实际使用）。</summary>
	public FPVector _force;

    #region trackable definitions
	/// <summary>是否已施加（运行时跟踪）。</summary>
    public bool casted{get; set;}
    #endregion

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 命中判定（Hit）：招式的一段攻击判定（伤害/硬直/击退/弹跳等）。
/// </summary>
[System.Serializable]
public class Hit: ICloneable {
	/// <summary>判定生效起始帧。</summary>
	public int activeFramesBegin;
	/// <summary>判定生效结束帧。</summary>
	public int activeFramesEnds;
	/// <summary>命中确认类型（普通命中/投技）。</summary>
	public HitConfirmType hitConfirmType;
	/// <summary>投技命中后的投掷招式（hitConfirmType 为 Throw 时使用）。</summary>
	public MoveInfo throwMove;
	/// <summary>拆投招式（tech move）。</summary>
	public MoveInfo techMove;
	/// <summary>是否可拆投。</summary>
	public bool techable = true;
	/// <summary>是否重置受击动画。</summary>
    public bool resetHitAnimations = true;
	/// <summary>是否强制目标保持站立。</summary>
    public bool forceStand = false;
	/// <summary>是否破霸体。</summary>
	public bool armorBreaker;
	/// <summary>是否为持续命中（多段接触判定）。</summary>
    public bool continuousHit;
	/// <summary>是否不可格挡。</summary>
    public bool unblockable;
	/// <summary>多段命中之间的间隔档位。</summary>
	public Sizes spaceBetweenHits;
	/// <summary>是否命中站立目标。</summary>
	public bool groundHit = true;
	/// <summary>是否命中间下蹲目标。</summary>
	public bool crouchingHit = true;
	/// <summary>是否命中空中目标。</summary>
	public bool airHit = true;
	/// <summary>是否命中眩晕目标。</summary>
    public bool stunHit = true;
	/// <summary>命中目标的额外条件。</summary>
    public PlayerConditions opponentConditions = new PlayerConditions();

	/// <summary>是否命中倒地目标。</summary>
	public bool downHit;
	/// <summary>是否重置之前受击硬直。</summary>
	public bool resetPreviousHitStun;
	/// <summary>是否重置破防计数。</summary>
    public bool resetCrumples;
	/// <summary>是否使用自定义硬直值。</summary>
    public bool customStunValues;

	/// <summary>是否覆盖默认命中特效。</summary>
	public bool overrideHitEffects;
	/// <summary>自定义命中特效。</summary>
	public HitTypeOptions hitEffects;
	/// <summary>是否覆盖格挡时的命中特效。</summary>
    public bool overrideHitEffectsBlock;
	/// <summary>自定义格挡命中特效。</summary>
    public HitTypeOptions hitEffectsBlock;
	/// <summary>是否覆盖特效生成点。</summary>
    public bool overrideEffectSpawnPoint;
	/// <summary>特效生成点。</summary>
    public HitEffectSpawnPoint spawnPoint = HitEffectSpawnPoint.StrokeHitBox;
	/// <summary>是否覆盖受击动画融合参数。</summary>
    public bool overrideHitAnimationBlend;
	/// <summary>新的受击动画融合进时长（float 值）。</summary>
    public float newHitBlendingIn;
	/// <summary>新的受击动画融合进时长（定点数，运行时实际使用）。</summary>
    public Fix64 _newHitBlendingIn;
	/// <summary>是否覆盖空中连击重量。</summary>
    public bool overrideJuggleWeight;
	/// <summary>新的空中连击重量（float 值）。</summary>
    public float newJuggleWeight;
	/// <summary>新的空中连击重量（定点数，运行时实际使用）。</summary>
    public Fix64 _newJuggleWeight;
	/// <summary>是否覆盖空中受身恢复类型。</summary>
    public bool overrideAirRecoveryType;
	/// <summary>新的空中受身恢复类型。</summary>
    public AirRecoveryType newAirRecoveryType = AirRecoveryType.AllowMoves;
	/// <summary>是否立即空中受身恢复。</summary>
    public bool instantAirRecovery;
	/// <summary>是否覆盖受击动画。</summary>
    public bool overrideHitAnimation;
    //public bool overrideHitAcceleration = true; // deprecated
	/// <summary>新的受击动画。</summary>
    public BasicMoveReference newHitAnimation = BasicMoveReference.HitKnockBack;

	/// <summary>命中强度。</summary>
	public HitStrengh hitStrength;
	/// <summary>受击硬直类型。</summary>
	public HitStunType hitStunType = HitStunType.Frames;
	/// <summary>命中时的受击硬直（float 值）。</summary>
	public float hitStunOnHit;
	/// <summary>命中时的受击硬直（定点数，运行时实际使用）。</summary>
	public Fix64 _hitStunOnHit;
	/// <summary>格挡时的受击硬直（float 值）。</summary>
	public float hitStunOnBlock;
	/// <summary>格挡时的受击硬直（定点数，运行时实际使用）。</summary>
	public Fix64 _hitStunOnBlock;
	/// <summary>命中后的帧优势。</summary>
	public int frameAdvantageOnHit;
	/// <summary>格挡后的帧优势。</summary>
	public int frameAdvantageOnBlock;
	/// <summary>是否应用伤害衰减。</summary>
	public bool damageScaling;
	/// <summary>伤害类型。</summary>
	public DamageType damageType;
	/// <summary>命中伤害（float 值）。</summary>
	public float damageOnHit;
	/// <summary>命中伤害（定点数，运行时实际使用）。</summary>
	public Fix64 _damageOnHit;
	/// <summary>格挡伤害（float 值）。</summary>
	public float damageOnBlock;
	/// <summary>格挡伤害（定点数，运行时实际使用）。</summary>
	public Fix64 _damageOnBlock;
	/// <summary>是否不会致死（保留至少1点生命）。</summary>
	public bool doesntKill;
	/// <summary>命中类型。</summary>
    public HitType hitType;

	/// <summary>是否重置之前水平推挤力。</summary>
	public bool resetPreviousHorizontalPush;
	/// <summary>是否重置之前垂直推挤力。</summary>
	public bool resetPreviousVerticalPush;
	/// <summary>是否应用不同的空中推挤力。</summary>
    public bool applyDifferentAirForce;
	/// <summary>是否应用不同的格挡推挤力。</summary>
    public bool applyDifferentBlockForce;
	/// <summary>推挤力（Vector2，float 版本）。</summary>
	public Vector2 pushForce;
	/// <summary>推挤力（定点向量，运行时实际使用）。</summary>
	public FPVector _pushForce;
	/// <summary>空中推挤力（Vector2，float 版本）。</summary>
	public Vector2 pushForceAir;
	/// <summary>空中推挤力（定点向量，运行时实际使用）。</summary>
	public FPVector _pushForceAir;
	/// <summary>格挡推挤力（定点向量，运行时实际使用）。</summary>
	public FPVector _pushForceBlock;
	/// <summary>是否重置之前水平力。</summary>
	public bool resetPreviousHorizontal;
	/// <summary>是否重置之前垂直力。</summary>
	public bool resetPreviousVertical;
	/// <summary>施加力（Vector2，float 版本）。</summary>
	public Vector2 appliedForce;
	/// <summary>施加力（定点向量，运行时实际使用）。</summary>
	public FPVector _appliedForce;

	/// <summary>是否允许角落推挤（pushback）。</summary>
    public bool cornerPush = true;

	/// <summary>是否触发地面弹跳。</summary>
    public bool groundBounce = true;
	/// <summary>是否覆盖地面弹跳力。</summary>
    public bool overrideForcesOnGroundBounce = false;
	/// <summary>地面弹跳是否重置水平推挤力。</summary>
    public bool resetGroundBounceHorizontalPush;
	/// <summary>地面弹跳是否重置垂直推挤力。</summary>
    public bool resetGroundBounceVerticalPush;
	/// <summary>地面弹跳推挤力（Vector2，float 版本）。</summary>
    public Vector2 groundBouncePushForce;
	/// <summary>地面弹跳推挤力（定点向量，运行时实际使用）。</summary>
    public FPVector _groundBouncePushForce;

	/// <summary>是否触发墙壁弹跳。</summary>
    public bool wallBounce = false;
	/// <summary>墙壁弹跳时是否击倒。</summary>
    public bool knockOutOnWallBounce = false;
	/// <summary>是否覆盖墙壁弹跳力。</summary>
    public bool overrideForcesOnWallBounce = false;
	/// <summary>墙壁弹跳是否重置水平推挤力。</summary>
    public bool resetWallBounceHorizontalPush;
	/// <summary>墙壁弹跳是否重置垂直推挤力。</summary>
    public bool resetWallBounceVerticalPush;
	/// <summary>墙壁弹跳推挤力（Vector2，float 版本）。</summary>
    public Vector2 wallBouncePushForce;
	/// <summary>墙壁弹跳推挤力（定点向量，运行时实际使用）。</summary>
    public FPVector _wallBouncePushForce;
	/// <summary>是否在摄像机边缘弹跳。</summary>
    public bool bounceOnCameraEdge = false;
	/// <summary>是否覆盖摄像机速度。</summary>
    public bool overrideCameraSpeed = false;
	/// <summary>新的摄像机移动速度（float 值）。</summary>
    public float newMovementSpeed;
	/// <summary>新的摄像机移动速度（定点数，运行时实际使用）。</summary>
    public Fix64 _newMovementSpeed;
	/// <summary>新的摄像机旋转速度（float 值）。</summary>
    public float newRotationSpeed;
	/// <summary>新的摄像机旋转速度（定点数，运行时实际使用）。</summary>
    public Fix64 _newRotationSpeed;
	/// <summary>摄像机速度持续时间（float 值）。</summary>
    public float cameraSpeedDuration;
	/// <summary>摄像机速度持续时间（定点数，运行时实际使用）。</summary>
    public Fix64 _cameraSpeedDuration;

	/// <summary>将敌人拉向自身（pull-in）配置。</summary>
	public PullIn pullEnemyIn;
	/// <summary>将自身拉向敌人配置。</summary>
	public PullIn pullSelfIn;

	/// <summary>编辑器用：伤害选项面板开关。</summary>
	[HideInInspector]	public bool damageOptionsToggle;
	/// <summary>编辑器用：硬直选项面板开关。</summary>
	[HideInInspector]	public bool hitStunOptionsToggle;
	/// <summary>编辑器用：力选项面板开关。</summary>
	[HideInInspector]	public bool forceOptionsToggle;
	/// <summary>编辑器用：对手力面板开关。</summary>
	[HideInInspector]	public bool opponentForceToggle;
	/// <summary>编辑器用：自身力面板开关。</summary>
	[HideInInspector]	public bool selfForceToggle;
	/// <summary>编辑器用：场地反应面板开关。</summary>
	[HideInInspector]	public bool stageReactionsToggle;
	/// <summary>编辑器用：覆盖事件面板开关。</summary>
	[HideInInspector]	public bool overrideEventsToggle;
	/// <summary>编辑器用：命中条件面板开关。</summary>
	[HideInInspector]	public bool hitConditionsToggle;
	/// <summary>编辑器用：拉近面板开关。</summary>
	[HideInInspector]	public bool pullInToggle;
	/// <summary>编辑器用：受击盒面板开关。</summary>
	[HideInInspector]	public bool hurtBoxesToggle;
	/// <summary>编辑器用：墙壁弹跳面板开关。</summary>
	[HideInInspector]	public bool wallBounceToggle;
	/// <summary>编辑器用：地面弹跳面板开关。</summary>
	[HideInInspector]	public bool groundBounceToggle;
    
    #region trackable definitions
	/// <summary>受击盒列表（运行时跟踪）。</summary>
    public HurtBox[] hurtBoxes = new HurtBox[0];
	/// <summary>是否已禁用（运行时跟踪）。</summary>
    public bool disabled{get; set;}
    #endregion

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 帧链接（FrameLink）：连段取消链配置。
/// <para>定义在特定命中条件下，本招式可取消接入哪些后续招式。</para>
/// </summary>
[System.Serializable]
public class FrameLink: ICloneable {
	/// <summary>链接类型（命中确认/反击技/无条件）。</summary>
	public LinkType linkType = LinkType.NoConditions;
	/// <summary>是否允许缓冲输入。</summary>
	public bool allowBuffer = true;
	/// <summary>命中时是否可取消。</summary>
	public bool onStrike = true;
	/// <summary>格挡时是否可取消。</summary>
	public bool onBlock = true;
	/// <summary>弹反时是否可取消。</summary>
	public bool onParry = true;
	/// <summary>可取消的起始帧。</summary>
	public int activeFramesBegins;
	/// <summary>可取消的结束帧。</summary>
	public int activeFramesEnds;
	/// <summary>反击技判定类型。</summary>
	public CounterMoveType counterMoveType;
	/// <summary>反击技过滤的招式（linkType 为 CounterMove 时使用）。</summary>
	public MoveInfo counterMoveFilter;
	/// <summary>取消时是否禁用打击停顿（hit impact）。</summary>
	public bool disableHitImpact = true;
	/// <summary>是否任意命中强度都可取消。</summary>
	public bool anyHitStrength = true;
	/// <summary>要求的命中强度。</summary>
	public HitStrengh hitStrength;
	/// <summary>是否任意攻击盒都可取消。</summary>
	public bool anyStrokeHitBox = true;
	/// <summary>要求的攻击盒类型。</summary>
	public HitBoxType hitBoxType;
	/// <summary>是否任意命中类型都可取消。</summary>
	public bool anyHitType = true;
	/// <summary>要求的命中类型。</summary>
	public HitType hitType;
	/// <summary>是否忽略输入要求。</summary>
	public bool ignoreInputs;
	/// <summary>是否忽略玩家条件。</summary>
	public bool ignorePlayerConditions;
	/// <summary>下一招式开始的前摇帧。</summary>
	public int nextMoveStartupFrame = 1;
	/// <summary>可取消接入的招式列表。</summary>
	public MoveInfo[] linkableMoves = new MoveInfo[0];


    #region trackable definitions
	/// <summary>是否当前可取消（运行时跟踪）。</summary>
    public bool cancelable { get; set; }
    //public bool counterCancelable { get; set; }
    #endregion


	/// <summary>编辑器用：可链接招式面板开关。</summary>
    [HideInInspector]	public bool linkableMovesToggle;
	/// <summary>编辑器用：命中确认面板开关。</summary>
	[HideInInspector]	public bool hitConfirmToggle;
	/// <summary>编辑器用：反击技面板开关。</summary>
	[HideInInspector]	public bool counterMoveToggle;
	
	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 招式粒子效果：在指定帧播放粒子特效。
/// </summary>
[System.Serializable]
public class MoveParticleEffect: ICloneable {
	/// <summary>触发帧。</summary>
	public int castingFrame;
	/// <summary>粒子效果配置。</summary>
	public ParticleInfo particleEffect;

    #region trackable definitions
	/// <summary>是否已触发（运行时跟踪）。</summary>
    public bool casted{get; set;}
    #endregion

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 慢动作效果：在指定帧触发慢动作（时间减速）。
/// </summary>
[System.Serializable]
public class SlowMoEffect: ICloneable {
	/// <summary>触发帧。</summary>
	public int castingFrame;
	/// <summary>慢动作持续时间（float 值，秒）。</summary>
	public float duration;
	/// <summary>慢动作持续时间（定点数，运行时实际使用）。</summary>
	public Fix64 _duration;
	/// <summary>慢动作速度百分比（float 值）。</summary>
	public float percentage;
	/// <summary>慢动作速度百分比（定点数，运行时实际使用）。</summary>
	public Fix64 _percentage;

    #region trackable definitions
	/// <summary>是否已触发（运行时跟踪）。</summary>
    public bool casted{get; set;}
    #endregion

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 音效效果：在指定帧播放音效。
/// </summary>
[System.Serializable]
public class SoundEffect: ICloneable {
	/// <summary>触发帧。</summary>
	public int castingFrame;
	/// <summary>音效列表（随机播放其一）。</summary>
	public AudioClip[] sounds = new AudioClip[0];
	
	/// <summary>编辑器用：音效面板开关。</summary>
	[HideInInspector]	public bool soundEffectsToggle;

    #region trackable definitions
	/// <summary>是否已触发（运行时跟踪）。</summary>
    public bool casted{get; set;}
    #endregion

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 游戏内提示：在指定帧显示文字提示（如必杀技名称）。
/// </summary>
[System.Serializable]
public class InGameAlert: ICloneable {
	/// <summary>触发帧。</summary>
	public int castingFrame;
	/// <summary>提示文本。</summary>
	public string alert;

    #region trackable definitions
	/// <summary>是否已触发（运行时跟踪）。</summary>
    public bool casted{get; set;}
    #endregion

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 姿态切换：在指定帧切换角色的战斗姿态。
/// </summary>
[System.Serializable]
public class StanceChange: ICloneable {
	/// <summary>触发帧。</summary>
	public int castingFrame;
	/// <summary>切换到的新姿态。</summary>
	public CombatStances newStance;

    #region trackable definitions
	/// <summary>是否已触发（运行时跟踪）。</summary>
    public bool casted{get; set;}
    #endregion

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 霸体选项（Armor）：指定帧内可吸收特定次数/伤害的攻击而不被打断。
/// </summary>
[System.Serializable]
public class ArmorOptions {
	/// <summary>霸体生效起始帧。</summary>
	public int activeFramesBegin;
	/// <summary>霸体生效结束帧。</summary>
	public int activeFramesEnds;
	
	/// <summary>是否覆盖霸体受击特效。</summary>
	public bool overrideHitEffects;
	/// <summary>霸体受击特效配置。</summary>
	public HitTypeOptions hitEffects;

	/// <summary>可吸收的命中次数。</summary>
    public int hitAbsorption;
	/// <summary>可吸收的伤害量。</summary>
	public int damageAbsorption;
	/// <summary>不受霸体保护（仍会被打断）的身体部位。</summary>
	public BodyPart[] nonAffectedBodyParts = new BodyPart[0];

    #region trackable definitions
	/// <summary>已承受的命中次数（运行时跟踪）。</summary>
    public int hitsTaken { get; set; }
    #endregion
}

/// <summary>
/// 摄像机移动（电影化演出）：指定帧触发摄像机移动/变焦演出。
/// </summary>
[System.Serializable]
public class CameraMovement: ICloneable {
	/// <summary>演出实现类型。</summary>
	public CinematicType cinematicType = CinematicType.CameraEditor;
	/// <summary>演出动画片段。</summary>
	public AnimationClip animationClip;
	/// <summary>演出预制体。</summary>
	public GameObject prefab;
	/// <summary>动画播放速度。</summary>
	public float camAnimationSpeed = 1;
	/// <summary>融合速度。</summary>
	public float blendSpeed = 100;
	/// <summary>预制体位置。</summary>
	public Vector3 gameObjectPosition;
	/// <summary>摄像机位置。</summary>
	public Vector3 position;
	/// <summary>摄像机旋转。</summary>
	public Vector3 rotation;
	/// <summary>触发帧。</summary>
	public int castingFrame;
	/// <summary>演出持续时间（float 值，秒）。</summary>
	public float duration;
	/// <summary>演出持续时间（定点数，运行时实际使用）。</summary>
	public Fix64 _duration;
	/// <summary>视野（FOV）。</summary>
	public float fieldOfView;
	/// <summary>摄像机移动速度。</summary>
	public float camSpeed = 2;
	/// <summary>演出期间是否冻结物理。</summary>
	public bool freezePhysics;
	/// <summary>本角色动画速度（float 值）。</summary>
	public float myAnimationSpeed = 100;
	/// <summary>本角色动画速度（定点数，运行时实际使用）。</summary>
	public Fix64 _myAnimationSpeed = 100;
	/// <summary>对手动画速度（float 值）。</summary>
	public float opAnimationSpeed = 100;
	/// <summary>对手动画速度（定点数，运行时实际使用）。</summary>
	public Fix64 _opAnimationSpeed = 100;
	/// <summary>编辑器用：预览开关。</summary>
	public bool previewToggle;

    #region trackable definitions
	/// <summary>是否已触发（运行时跟踪）。</summary>
    public bool casted{get; set;}
	/// <summary>演出是否结束（运行时跟踪）。</summary>
    public bool over{get; set;}
	/// <summary>演出已进行时间（运行时跟踪）。</summary>
    public FPLibrary.Fix64 time {get; set;}
    #endregion

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 对手覆盖（OpponentOverride）：指定帧对对手施加的控制（位移/硬直/动画/招式）。
/// </summary>
[System.Serializable]
public class OpponentOverride: ICloneable {
	/// <summary>对手位置（float 版本）。</summary>
	public Vector3 position;
	/// <summary>对手位置（定点向量，运行时实际使用）。</summary>
	public FPVector _position;
	/// <summary>触发帧。</summary>
	public int castingFrame;
	/// <summary>位移融合速度。</summary>
	public int blendSpeed = 80;
	/// <summary>是否使对手眩晕。</summary>
	public bool stun;
	/// <summary>眩晕时间（float 值）。</summary>
	public float stunTime;
	/// <summary>眩晕时间（定点数，运行时实际使用）。</summary>
	public Fix64 _stunTime;
	/// <summary>是否覆盖对手受击动画。</summary>
	public bool overrideHitAnimations;
	/// <summary>是否重置对手已施加的力。</summary>
	public bool resetAppliedForces;
	
	// End Options
	/// <summary>演出结束后的起身选项。</summary>
	public StandUpOptions standUpOptions;

	// Options
	/// <summary>是否角色特定（按角色配置不同演出）。</summary>
	public bool characterSpecific;

	// Move
	/// <summary>演出期间让对手播放的招式。</summary>
	public MoveInfo move;
	// Character Specific Moves
	/// <summary>按角色特定的招式列表。</summary>
	public CharacterSpecificMoves[] characterSpecificMoves = new CharacterSpecificMoves[0];
	
	/// <summary>编辑器用：动画预览开关。</summary>
	[HideInInspector]	public bool animationPreview = false;
	/// <summary>编辑器用：招式面板开关。</summary>
	[HideInInspector]	public bool movesToggle = false;

    #region trackable definitions
	/// <summary>是否已触发（运行时跟踪）。</summary>
    public bool casted{get; set;}
    #endregion

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 角色特定招式：为特定角色覆盖的演出招式。
/// </summary>
[System.Serializable]
public class CharacterSpecificMoves {
	/// <summary>招式引用。</summary>
	public MoveInfo move;
	/// <summary>角色名称。</summary>
	public string characterName;
}

/// <summary>
/// 可能招式状态：招式的状态/距离等前置条件。
/// </summary>
[System.Serializable]
public class PossibleMoveStates: ICloneable {
	/// <summary>可能的主状态。</summary>
	public PossibleStates possibleState;
	/// <summary>跳跃弧线阶段。</summary>
	public JumpArc jumpArc;
	/// <summary>跳跃弧线阶段起始范围。</summary>
	public int jumpArcBegins = 0;
	/// <summary>跳跃弧线阶段结束范围。</summary>
	public int jumpArcEnds = 100;
	
	/// <summary>对手距离类别。</summary>
	public CharacterDistance opponentDistance;
	/// <summary>距离范围起始值。</summary>
	public int proximityRangeBegins = 0;
	/// <summary>距离范围结束值。</summary>
	public int proximityRangeEnds = 100;

	/// <summary>是否允许向前移动。</summary>
	public bool movingForward = true;
	/// <summary>是否允许向后移动。</summary>
	public bool movingBack = true;

	/// <summary>是否要求待机状态。</summary>
	public bool standBy = true;
	/// <summary>是否要求格挡状态。</summary>
	public bool blocking;
	/// <summary>是否要求眩晕状态。</summary>
	public bool stunned;
	
	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 玩家条件：招式的释放/命中前置条件集合。
/// </summary>
[System.Serializable]
public class PlayerConditions {
	/// <summary>基础动作限制列表。</summary>
	public BasicMoveReference[] basicMoveLimitation = new BasicMoveReference[0];
	/// <summary>可能的招式状态列表。</summary>
	public PossibleMoveStates[] possibleMoveStates = new PossibleMoveStates[0];

	/// <summary>编辑器用：基础动作面板开关。</summary>
	[HideInInspector]	public bool basicMovesToggle = false;
	/// <summary>编辑器用：状态面板开关。</summary>
	[HideInInspector]	public bool statesToggle = false;
}

/// <summary>
/// 出招输入定义（MoveInputs）：招式的指令序列与执行按钮配置。
/// <para>包括蓄力技、输入宽容度、松键触发等高级输入特性。</para>
/// </summary>
[System.Serializable]
public class MoveInputs {
	/// <summary>是否为蓄力技（按住方向一定时间后放开触发）。</summary>
    public bool chargeMove;
	/// <summary>蓄力所需时间（定点数，秒）。</summary>
    public Fix64 _chargeTiming = .7;
	/// <summary>是否允许输入宽容（多余输入不打断匹配）。</summary>
    public bool allowInputLeniency;
	/// <summary>是否允许松键触发（Negative Edge）。</summary>
    public bool allowNegativeEdge = true;
	/// <summary>输入宽容缓冲帧数。</summary>
    public int leniencyBuffer = 3;
	/// <summary>是否松开按钮时执行。</summary>
    public bool onReleaseExecution;
	/// <summary>是否要求按钮按下（buttonExecution 必须包含按钮）。</summary>
    public bool requireButtonPress = true;
	/// <summary>是否按下按钮时执行。</summary>
    public bool onPressExecution = true;
	/// <summary>指令序列（摇杆方向等）。</summary>
    public ButtonPress[] buttonSequence = new ButtonPress[0];
	/// <summary>执行按钮（触发招式的按钮）。</summary>
    public ButtonPress[] buttonExecution = new ButtonPress[0];

	/// <summary>编辑器用：面板折叠开关。</summary>
    [HideInInspector] public bool editorToggle = false;
	/// <summary>编辑器用：指令序列面板开关。</summary>
    [HideInInspector] public bool buttonSequenceToggle = false;
	/// <summary>编辑器用：执行按钮面板开关。</summary>
    [HideInInspector] public bool buttonExecutionToggle = false;
}

/// <summary>
/// 招式分类（MoveClassification）：招式的 AI/系统分类信息。
/// </summary>
[System.Serializable]
public class MoveClassification {
	/// <summary>攻击类型。</summary>
    public AttackType attackType;
	/// <summary>命中类型。</summary>
    public HitType hitType;
	/// <summary>出招速度档位。</summary>
    public FrameSpeed startupSpeed;
	/// <summary>收招速度档位。</summary>
    public FrameSpeed recoverySpeed;
	/// <summary>命中确认类型。</summary>
    public HitConfirmType hitConfirmType;
	/// <summary>最佳使用距离。</summary>
    public CharacterDistance preferableDistance;
	/// <summary>能量消耗档位。</summary>
    public GaugeUsage gaugeUsage;
	/// <summary>是否任意攻击类型都匹配。</summary>
    public bool anyAttackType = true;
	/// <summary>是否任意命中类型都匹配。</summary>
    public bool anyHitType = true;
	/// <summary>是否任意命中确认类型都匹配。</summary>
    public bool anyHitConfirmType = true;
}

/// <summary>
/// 动画速度关键帧：指定帧调整动画播放速度。
/// </summary>
[System.Serializable]
public class AnimSpeedKeyFrame: ICloneable {
	/// <summary>触发帧。</summary>
    public int castingFrame = 0;
	/// <summary>动画速度倍率（float 值）。</summary>
    public float speed = 1;
	/// <summary>动画速度倍率（定点数，运行时实际使用）。</summary>
    public Fix64 _speed = 1;
    
	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
        return CloneObject.Clone(this);
    }
}

/// <summary>
/// 序列化动画映射：一个动画片段对应的逐帧判定盒映射数据。
/// </summary>
[System.Serializable]
public class SerializedAnimationMap {
	/// <summary>逐帧映射列表。</summary>
    public AnimationMap[] animationMaps = new AnimationMap[0];
	/// <summary>对应动画片段。</summary>
    public AnimationClip clip;
	/// <summary>片段时长（定点数）。</summary>
    public Fix64 length;
	/// <summary>是否烘焙动画速度。</summary>
    public bool bakeSpeed = false;
}

/// <summary>
/// 动画映射：某一帧的判定盒位置与位移数据。
/// </summary>
[System.Serializable]
public class AnimationMap {
	/// <summary>帧号。</summary>
    public int frame;
	/// <summary>该帧各身体部位的判定盒映射。</summary>
    public HitBoxMap[] hitBoxMaps = new HitBoxMap[0];
	/// <summary>该帧的位移增量。</summary>
    public FPVector deltaDisplacement;
}

/// <summary>
/// 判定盒映射：单个身体部位在某帧的映射位置。
/// </summary>
[System.Serializable]
public class HitBoxMap {
	/// <summary>身体部位。</summary>
    public BodyPart bodyPart;
	/// <summary>映射位置（定点向量）。</summary>
    public FPVector mappedPosition;
}

/// <summary>
/// 基础动作信息（BasicMoveInfo）：一个基础动作（待机/行走/跳跃等）的完整配置。
/// </summary>
[System.Serializable]
public class BasicMoveInfo : ICloneable {
	/// <summary>动画片段1。</summary>
    public AnimationClip clip1;
	/// <summary>动画片段2。</summary>
    public AnimationClip clip2;
	/// <summary>动画片段3。</summary>
    public AnimationClip clip3;
	/// <summary>动画片段4。</summary>
    public AnimationClip clip4;
	/// <summary>动画片段5。</summary>
    public AnimationClip clip5;
	/// <summary>动画片段6。</summary>
    public AnimationClip clip6;
	/// <summary>逐帧动画映射（6 个片段各一份）。</summary>
    public SerializedAnimationMap[] animMap = new SerializedAnimationMap[6];
	/// <summary>动画播放速度（float 值）。</summary>
    public float animationSpeed = 1;
	/// <summary>动画播放速度（定点数，运行时实际使用）。</summary>
    public Fix64 _animationSpeed = 1;
	/// <summary>动画播放模式（循环/单次等）。</summary>
    public WrapMode wrapMode;

	/// <summary>是否自动计算动画速度。</summary>
    public bool autoSpeed = true;
	/// <summary>待机动画切换间隔（float 值）。</summary>
    public float restingClipInterval = 6;
	/// <summary>待机动画切换间隔（定点数，运行时实际使用）。</summary>
    public Fix64 _restingClipInterval = 6;
	/// <summary>是否覆盖融合进时长。</summary>
    public bool overrideBlendingIn = false;
	/// <summary>是否覆盖融合出时长。</summary>
    public bool overrideBlendingOut = false;
	/// <summary>融合进时长（float 值）。</summary>
    public float blendingIn = 0;
	/// <summary>融合进时长（定点数，运行时实际使用）。</summary>
    public Fix64 _blendingIn = 0;
	/// <summary>融合出时长（float 值）。</summary>
    public float blendingOut = 0;
	/// <summary>融合出时长（定点数，运行时实际使用）。</summary>
    public Fix64 _blendingOut = 0;
	/// <summary>该基础动作期间是否无敌。</summary>
    public bool invincible;
	/// <summary>是否禁用头部注视。</summary>
    public bool disableHeadLook;
	/// <summary>是否应用根骨骼运动。</summary>
    public bool applyRootMotion;
	/// <summary>是否为倒地动作片段。</summary>
    public bool downClip;
	/// <summary>音效列表。</summary>
    public AudioClip[] soundEffects = new AudioClip[0];
	/// <summary>是否持续循环音效。</summary>
    public bool continuousSound;
	/// <summary>粒子效果配置。</summary>
    public ParticleInfo particleEffect = new ParticleInfo();
	/// <summary>基础动作引用（标识）。</summary>
    public BasicMoveReference reference;
    

	/// <summary>动作名称（Inspector 隐藏）。</summary>
    [HideInInspector] public string name;
	/// <summary>编辑器用：面板折叠开关。</summary>
    [HideInInspector] public bool editorToggle;
	/// <summary>编辑器用：音效面板开关。</summary>
    [HideInInspector] public bool soundEffectsToggle;

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}

namespace UFE3D
{
	/// <summary>
	/// 招式信息（MoveInfo）：一个完整招式（普通技/必杀技/投技等）的配置（ScriptableObject 资产）。
	/// <para>包含动画与帧数据（前摇/判定/后摇）、输入指令、前置/派生技、连段取消链、命中判定、</para>
	/// <para>特效/音效/慢动作/姿态切换/演出、无敌/霸体、飞行道具、能量消耗等全部招式行为。</para>
	/// <para>对应 Unity 资产 .asset（如 Stand_DF1.asset 波动拳），可在 UFE 的 Move Editor 中编辑。</para>
	/// </summary>
    [System.Serializable]
    public class MoveInfo : ScriptableObject
    {
		/// <summary>配置版本号。</summary>
        public float version;
		/// <summary>招式动画片段。</summary>
        public AnimationClip animationClip;
		/// <summary>逐帧动画映射（判定盒位置映射）。</summary>
        public SerializedAnimationMap animMap = new SerializedAnimationMap();
		/// <summary>动画播放速度（float 值）。</summary>
        public float animationSpeed = 1;
		/// <summary>动画播放速度（定点数，运行时实际使用）。</summary>
        public Fix64 _animationSpeed = 1;
		/// <summary>动画播放模式。</summary>
        public WrapMode wrapMode;

		/// <summary>招式预制体（额外物体）。</summary>
        public GameObject characterPrefab;
		/// <summary>招式名称。</summary>
        public string moveName;
		/// <summary>招式描述。</summary>
        public string description;
		/// <summary>动画帧率。</summary>
        public int fps = 60;
		/// <summary>是否忽略重力。</summary>
        public bool ignoreGravity;
		/// <summary>是否忽略摩擦。</summary>
        public bool ignoreFriction;
		/// <summary>落地时是否取消招式。</summary>
        public bool cancelMoveWheLanding;
		/// <summary>是否强制向左镜像。</summary>
        public bool forceMirrorLeft;
		/// <summary>是否强制向右镜像。</summary>
        public bool forceMirrorRight;
		/// <summary>是否反转向左旋转。</summary>
        public bool invertRotationLeft;
		/// <summary>是否反转向右旋转。</summary>
        public bool invertRotationRight;
		/// <summary>是否自动修正朝向。</summary>
        public bool autoCorrectRotation;
		/// <summary>朝向修正的时间窗口帧数。</summary>
        public int frameWindowRotation;

		/// <summary>编辑器用：能量面板开关。</summary>
        public bool gaugeToggle;
		/// <summary>是否开始消耗能量。</summary>
        public bool startDrainingGauge;
		/// <summary>消耗期间是否禁止能量回复。</summary>
        public bool inhibitGainWhileDraining;
		/// <summary>是否停止消耗能量。</summary>
        public bool stopDrainingGauge;
		/// <summary>每秒能量消耗速率（float 值）。</summary>
        public float gaugeDPS;
		/// <summary>每秒能量消耗速率（定点数，运行时实际使用）。</summary>
        public Fix64 _gaugeDPS;
		/// <summary>总消耗量（float 值）。</summary>
        public float totalDrain;
		/// <summary>总消耗量（定点数，运行时实际使用）。</summary>
        public Fix64 _totalDrain;
		/// <summary>所需能量值（float 值）。</summary>
        public float gaugeRequired;
		/// <summary>所需能量值（定点数，运行时实际使用）。</summary>
        public Fix64 _gaugeRequired;
		/// <summary>能量使用量（float 值）。</summary>
        public float gaugeUsage;
		/// <summary>能量使用量（定点数，运行时实际使用）。</summary>
        public Fix64 _gaugeUsage;
		/// <summary>落空时能量回复（float 值）。</summary>
        public float gaugeGainOnMiss;
		/// <summary>落空时能量回复（定点数，运行时实际使用）。</summary>
        public Fix64 _gaugeGainOnMiss;
		/// <summary>命中时能量回复（float 值）。</summary>
        public float gaugeGainOnHit;
		/// <summary>命中时能量回复（定点数，运行时实际使用）。</summary>
        public Fix64 _gaugeGainOnHit;
		/// <summary>格挡时能量回复（float 值）。</summary>
        public float gaugeGainOnBlock;
		/// <summary>格挡时能量回复（定点数，运行时实际使用）。</summary>
        public Fix64 _gaugeGainOnBlock;
		/// <summary>对方格挡时对方能量回复（float 值）。</summary>
        public float opGaugeGainOnBlock;
		/// <summary>对方格挡时对方能量回复（定点数，运行时实际使用）。</summary>
        public Fix64 _opGaugeGainOnBlock;
		/// <summary>对方弹反时对方能量回复（float 值）。</summary>
        public float opGaugeGainOnParry;
		/// <summary>对方弹反时对方能量回复（定点数，运行时实际使用）。</summary>
        public Fix64 _opGaugeGainOnParry;
		/// <summary>对方命中时对方能量回复（float 值）。</summary>
        public float opGaugeGainOnHit;
		/// <summary>对方命中时对方能量回复（定点数，运行时实际使用）。</summary>
        public Fix64 _opGaugeGainOnHit;
		/// <summary>双取消（Double Cancel）招式。</summary>
        public MoveInfo DCMove;
		/// <summary>双取消后的姿态。</summary>
        public CombatStances DCStance;

		/// <summary>是否禁用头部注视。</summary>
        public bool disableHeadLook = true;

		/// <summary>编辑器用：动画速度关键帧面板开关。</summary>
        public bool speedKeyFrameToggle = false;
		/// <summary>是否固定动画速度。</summary>
        public bool fixedSpeed = true;
		/// <summary>动画速度关键帧列表。</summary>
        public AnimSpeedKeyFrame[] animSpeedKeyFrame = new AnimSpeedKeyFrame[0];
		/// <summary>招式总帧数。</summary>
        public int totalFrames = 15;

		/// <summary>前摇帧数。</summary>
        public int startUpFrames = 0;
		/// <summary>判定生效帧数。</summary>
        public int activeFrames = 1;
		/// <summary>后摇帧数。</summary>
        public int recoveryFrames = 2;
		/// <summary>是否应用根骨骼运动。</summary>
        public bool applyRootMotion = false;
		/// <summary>是否强制落地。</summary>
        public bool forceGrounded = false;
		/// <summary>根骨骼运动节点。</summary>
        public BodyPart rootMotionNode = BodyPart.none;
		/// <summary>是否覆盖融合进时长。</summary>
        public bool overrideBlendingIn = true;
		/// <summary>是否覆盖融合出时长。</summary>
        public bool overrideBlendingOut = false;
		/// <summary>融合进时长（float 值）。</summary>
        public float blendingIn = 0;
		/// <summary>融合进时长（定点数，运行时实际使用）。</summary>
        public Fix64 _blendingIn = 0;
		/// <summary>融合出时长（float 值）。</summary>
        public float blendingOut = 0;
		/// <summary>融合出时长（定点数，运行时实际使用）。</summary>
        public Fix64 _blendingOut = 0;


		/// <summary>是否为蓄力技。</summary>
        public bool chargeMove;
		/// <summary>蓄力时间（float 值，秒）。</summary>
        public float chargeTiming = .7f;
		/// <summary>蓄力时间（定点数，运行时实际使用）。</summary>
        public Fix64 _chargeTiming = .7;
		/// <summary>是否允许输入宽容。</summary>
        public bool allowInputLeniency;
		/// <summary>是否允许松键触发。</summary>
        public bool allowNegativeEdge = true;
		/// <summary>输入宽容缓冲帧数。</summary>
        public int leniencyBuffer = 3;
		/// <summary>是否松开按钮时执行。</summary>
        public bool onReleaseExecution;
		/// <summary>是否要求按钮按下。</summary>
        public bool requireButtonPress = true;
		/// <summary>是否按下按钮时执行。</summary>
        public bool onPressExecution = true;
		/// <summary>指令序列。</summary>
        public ButtonPress[] buttonSequence = new ButtonPress[0];
		/// <summary>执行按钮。</summary>
        public ButtonPress[] buttonExecution = new ButtonPress[0];

		/// <summary>默认输入配置。</summary>
        public MoveInputs defaultInputs = new MoveInputs();
		/// <summary>备用输入配置（简化指令）。</summary>
        public MoveInputs altInputs = new MoveInputs();


		/// <summary>前置招式（派生技前置）。</summary>
        public MoveInfo[] previousMoves = new MoveInfo[0];
		/// <summary>对手释放条件。</summary>
        public PlayerConditions opponentConditions = new PlayerConditions();
		/// <summary>自身释放条件。</summary>
        public PlayerConditions selfConditions = new PlayerConditions();
		/// <summary>招式分类。</summary>
        public MoveClassification moveClassification;

		/// <summary>模拟输入（AI/训练用）。</summary>
        public ButtonPress[][] simulatedInputs;


        #region trackable definitions
		/// <summary>连段取消链列表。</summary>
        public FrameLink[] frameLinks = new FrameLink[0];
		/// <summary>粒子特效列表。</summary>
        public MoveParticleEffect[] particleEffects = new MoveParticleEffect[0];
		/// <summary>施加力列表。</summary>
        public AppliedForce[] appliedForces = new AppliedForce[0];
		/// <summary>慢动作效果列表。</summary>
        public SlowMoEffect[] slowMoEffects = new SlowMoEffect[0];
		/// <summary>身体部位可见性变化列表。</summary>
        public BodyPartVisibilityChange[] bodyPartVisibilityChanges = new BodyPartVisibilityChange[0];
		/// <summary>对手覆盖（演出控制）列表。</summary>
        public OpponentOverride[] opponentOverride = new OpponentOverride[0];
		/// <summary>音效列表。</summary>
        public SoundEffect[] soundEffects = new SoundEffect[0];
		/// <summary>游戏内提示列表。</summary>
        public InGameAlert[] inGameAlert = new InGameAlert[0];
		/// <summary>姿态切换列表。</summary>
        public StanceChange[] stanceChanges = new StanceChange[0];
		/// <summary>摄像机移动（演出）列表。</summary>
        public CameraMovement[] cameraMovements = new CameraMovement[0];
		/// <summary>命中判定列表（一段招式可含多段判定）。</summary>
        public Hit[] hits = new Hit[0];
		/// <summary>可格挡区域。</summary>
        public BlockArea blockableArea;
		/// <summary>无敌身体部位列表。</summary>
        public InvincibleBodyParts[] invincibleBodyParts = new InvincibleBodyParts[0];
		/// <summary>霸体选项。</summary>
        public ArmorOptions armorOptions;
		/// <summary>飞行道具列表。</summary>
        public Projectile[] projectiles = new Projectile[0];

		/// <summary>是否当前可取消（运行时跟踪）。</summary>
        public bool cancelable { get; set; }
		/// <summary>招式是否已结束（运行时跟踪）。</summary>
        public bool kill { get; set; }
		/// <summary>当前执行帧（运行时跟踪）。</summary>
        public int currentFrame { get; set; }
		/// <summary>覆盖的前摇帧数（运行时跟踪）。</summary>
        public int overrideStartupFrame { get; set; }
		/// <summary>临时动画速度（运行时跟踪）。</summary>
        public Fix64 animationSpeedTemp { get; set; }
		/// <summary>当前累计帧数（运行时跟踪）。</summary>
        public Fix64 currentTick { get; set; }
		/// <summary>格挡命中确认状态（运行时跟踪）。</summary>
        public bool hitConfirmOnBlock { get; set; }
		/// <summary>弹反命中确认状态（运行时跟踪）。</summary>
        public bool hitConfirmOnParry { get; set; }
		/// <summary>命中确认状态（运行时跟踪）。</summary>
        public bool hitConfirmOnStrike { get; set; }
		/// <summary>受击动画覆盖状态（运行时跟踪）。</summary>
        public bool hitAnimationOverride { get; set; }
		/// <summary>起身选项（运行时跟踪）。</summary>
        public StandUpOptions standUpOptions { get; set; }
		/// <summary>当前帧阶段（前摇/判定/后摇，运行时跟踪）。</summary>
        public CurrentFrameData currentFrameData { get; set; }
        #endregion

		/// <summary>
		/// 判断本招式是否为投技。
		/// </summary>
		/// <param name="techable">是否检查可拆投的投技（true 查可拆投，false 查不可拆投）。</param>
		/// <returns>存在符合条件（Throw 类型且拆投状态匹配）的命中判定时返回 true。</returns>
        public bool IsThrow(bool techable)
        {
            foreach (Hit hit in this.hits)
            {
                //if (this.currentFrame >= hit.activeFramesBegin && this.currentFrame < hit.activeFramesEnds) {
                if (hit.hitConfirmType == HitConfirmType.Throw && hit.techable == techable) return true;
                //}
            }
            return false;
        }

		/// <summary>
		/// 获取本投技的拆投招式。
		/// </summary>
		/// <returns>返回第一个可拆投的投技命中对应的拆投招式；无则返回 null。</returns>
        public MoveInfo GetTechMove()
        {
            foreach (Hit hit in this.hits)
            {
                //if (this.currentFrame >= hit.activeFramesBegin && this.currentFrame < hit.activeFramesEnds) {
                if (hit.hitConfirmType == HitConfirmType.Throw && hit.techable) return hit.techMove;
                //}
            }
            return null;
        }
    }
}