using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AForge.Fuzzy;
using AI4Unity.Fuzzy;
using UFE3D;

/// <summary>
/// AI 信息定义（AIInfo）。
/// <para>用途：本文件定义 Fuzzy AI（模糊逻辑 AI）的全部数据结构与配置类——</para>
/// <para>包括伤害/距离/渴望度/生命/速度的模糊隶属度阈值（AIDefinitions 系列）、AI 高级参数（AIAdvancedOptions）、</para>
/// <para>规则条件（AICondition）、事件（AIEvent）、反应（AIReaction）、规则（AIRule），</para>
/// <para>以及核心类 UFE3D.AIInfo（ScriptableObject）——负责将规则转换为可求值的模糊推理系统（InferenceSystem）。</para>
/// <para>Fuzzy AI 通过将战斗状态模糊化为语言变量，依据规则求值得出各动作的"渴望度"，从而选择最优动作。</para>
/// </summary>

/// <summary>
/// AI 定义集合：各模糊变量的隶属度阈值配置。
/// </summary>
[Serializable]
public class AIDefinitions{
	/// <summary>伤害阈值定义。</summary>
	public DamageDefinitions damage;
	/// <summary>距离阈值定义。</summary>
	public DistanceDefinitions distance;
	/// <summary>渴望度阈值定义。</summary>
	public DesirabilityDefinitions desirability;
	/// <summary>生命状态阈值定义。</summary>
	public HealthDefinitions health;
	/// <summary>速度阈值定义。</summary>
	public SpeedDefinitions speed;
}

/// <summary>
/// 伤害阈值定义：伤害等级的模糊隶属度区间。
/// </summary>
[Serializable]
public class DamageDefinitions{
	/// <summary>非常弱的伤害阈值。</summary>
	public float veryWeak = 0.05f;
	/// <summary>弱的伤害阈值。</summary>
	public float weak = 0.10f;
	/// <summary>中的伤害阈值。</summary>
	public float medium = 0.15f;
	/// <summary>强的伤害阈值。</summary>
	public float strong = 0.20f;
	/// <summary>非常强的伤害阈值。</summary>
	public float veryStrong = 0.25f;
}

/// <summary>
/// 距离阈值定义：距离档位的模糊隶属度区间。
/// </summary>
[Serializable]
public class DistanceDefinitions{
	/// <summary>非常近的距离阈值。</summary>
	public float veryClose = 0.05f;
	/// <summary>近的距离阈值。</summary>
	public float close = 0.25f;
	/// <summary>中的距离阈值。</summary>
	public float mid = 0.5f;
	/// <summary>远的距离阈值。</summary>
	public float far = 0.75f;
	/// <summary>非常远的距离阈值。</summary>
	public float veryFar = 0.95f;
}

/// <summary>
/// 渴望度阈值定义：动作渴望度档位的模糊隶属度区间。
/// </summary>
[Serializable]
public class DesirabilityDefinitions{
	/// <summary>最差选项阈值。</summary>
	public float theWorstOption = 0.00f;
	/// <summary>非常不可取阈值。</summary>
	public float veryUndesirable = 0.15f;
	/// <summary>不可取阈值。</summary>
	public float undesirable = 0.30f;
	/// <summary>尚可阈值。</summary>
	public float notBad = 0.45f;
	/// <summary>可取阈值。</summary>
	public float desirable = 0.60f;
	/// <summary>非常可取阈值。</summary>
	public float veryDesirable = 0.80f;
	/// <summary>最佳选项阈值。</summary>
	public float theBestOption = 1.00f;
}

/// <summary>
/// 生命状态阈值定义：生命值档位的模糊隶属度区间。
/// </summary>
[Serializable]
public class HealthDefinitions{
	/// <summary>健康阈值。</summary>
	public float healthy = 1.0f;
	/// <summary>轻伤阈值。</summary>
	public float scratched = 0.9f;
	/// <summary>轻度受伤阈值。</summary>
	public float lightlyWounded = 0.8f;
	/// <summary>中度受伤阈值。</summary>
	public float moderatelyWounded = 0.6f;
	/// <summary>严重受伤阈值。</summary>
	public float seriouslyWounded = 0.4f;
	/// <summary>危急受伤阈值。</summary>
	public float criticallyWounded = 0.2f;
	/// <summary>濒死阈值。</summary>
	public float almostDead = 0.1f;
	/// <summary>死亡阈值。</summary>
	public float dead = 0.0f;
}

/// <summary>
/// AI 高级选项：Fuzzy AI 的决策/动作时机与行为倾向参数。
/// </summary>
[Serializable]
public class AIAdvancedOptions{
	/// <summary>决策间隔（秒）：AI 每次做决策的间隔。</summary>
	public float timeBetweenDecisions = 0;
	/// <summary>动作间隔（秒）：AI 执行连续动作的间隔。</summary>
	public float timeBetweenActions = 0.05f;
	/// <summary>侵略性（0~1）：主动进攻倾向。</summary>
	public float aggressiveness = 0.5f;
	/// <summary>规则遵循度（0~1）：0=加权随机选择，1=始终使用最优动作。</summary>
	public float ruleCompliance = .9f; // 0 = Weighted Random Selection / 1 = Use the Best Available Move
	/// <summary>连招效率（0~1）：成功执行连招的比率。</summary>
	public float comboEfficiency = 1f;
	/// <summary>移动持续时间（秒）：单个移动指令的持续时间。</summary>
    public float movementDuration = .1f;
	/// <summary>按钮序列输入间隔。</summary>
    public int buttonSequenceInterval = 1;
	/// <summary>攻击渴望度的计算方式（平均/受限和/最大/最小）。</summary>
	public AIAttackDesirabilityCalculation attackDesirabilityCalculation = AIAttackDesirabilityCalculation.Max;
	/// <summary>默认渴望度（无规则匹配时的回退值）。</summary>
	public AIDesirability defaultDesirability = AIDesirability.TheWorstOption;
	/// <summary>是否随机出招（无视渴望度）。</summary>
	public bool playRandomMoves;
	/// <summary>反应参数（何时攻击/格挡/输入）。</summary>
	public AIReactionParameters reactionParameters = new AIReactionParameters();
}

/// <summary>
/// AI 反应参数：控制 AI 在不同战况下的行为开关。
/// </summary>
[Serializable]
public class AIReactionParameters{
	/// <summary>敌人倒地时是否攻击。</summary>
	public bool attackWhenEnemyIsDown = false;
	/// <summary>敌人格挡时是否攻击。</summary>
	public bool attackWhenEnemyIsBlocking = true;
	/// <summary>敌人眩晕时是否停止格挡。</summary>
	public bool stopBlockingWhenEnemyIsStunned = true;
	
	/// <summary>自身倒地时是否输入。</summary>
	public bool inputWhenDown = false;
	/// <summary>自身格挡时是否输入。</summary>
	public bool inputWhenBlocking = true;
	/// <summary>自身眩晕时是否输入。</summary>
	public bool inputWhenStunned = true;

	/// <summary>是否启用攻击类型过滤。</summary>
	public bool enableAttackTypeFilter = true;
	/// <summary>是否启用能量槽过滤。</summary>
	public bool enableGaugeFilter = true;
	/// <summary>是否启用距离过滤。</summary>
	public bool enableDistanceFilter = true;
	/// <summary>是否启用伤害过滤。</summary>
	public bool enableDamageFilter = true;
	/// <summary>是否启用命中确认类型过滤。</summary>
	public bool enableHitConfirmTypeFilter = true;
	/// <summary>是否启用攻击速度过滤。</summary>
	public bool enableAttackSpeedFilter = false;
	/// <summary>是否启用命中类型过滤。</summary>
	public bool enableHitTypeFilter = true;
}

/// <summary>
/// 速度阈值定义：移动速度档位的模糊隶属度区间。
/// </summary>
[Serializable]
public class SpeedDefinitions{
	/// <summary>非常慢速度阈值。</summary>
	public float verySlow = 0.5f;
	/// <summary>慢速度阈值。</summary>
	public float slow = 1.0f;
	/// <summary>正常速度阈值。</summary>
	public float normal = 3.0f;
	/// <summary>快速度阈值。</summary>
	public float fast = 5.0f;
	/// <summary>非常快速度阈值。</summary>
	public float veryFast = 7.0f;
}


/// <summary>
/// 生命状态：角色的生命值档位。
/// </summary>
public enum HealthStatus {
	/// <summary>健康。</summary>
	Healthy,
	/// <summary>轻伤。</summary>
	Scratched,
	/// <summary>轻度受伤。</summary>
	LightlyWounded,
	/// <summary>中度受伤。</summary>
	ModeratelyWounded,
	/// <summary>严重受伤。</summary>
	SeriouslyWounded,
	/// <summary>危急受伤。</summary>
	CriticallyWounded,
	/// <summary>濒死。</summary>
	AlmostDead,
	/// <summary>死亡。</summary>
	Dead
}

/// <summary>
/// 目标角色：条件/反应作用的对象。
/// </summary>
public enum TargetCharacter {
	/// <summary>自身。</summary>
	Self,
	/// <summary>对手。</summary>
	Opponent
}

/// <summary>
/// 攻击渴望度计算方式：多规则求值结果的合成方式。
/// </summary>
public enum AIAttackDesirabilityCalculation{
	/// <summary>取平均值。</summary>
	Average,
	/// <summary>受限求和。</summary>
	ClampedSum,
	/// <summary>取最大值。</summary>
	Max,
	/// <summary>取最小值。</summary>
	Min
}

/// <summary>
/// AI 条件类型：规则条件可检查的战斗状态类别。
/// </summary>
public enum AIConditionType {
	/// <summary>待机。</summary>
	Idle,
	/// <summary>水平移动。</summary>
	HorizontalMovement,
	/// <summary>垂直移动。</summary>
	VerticalMovement,
	/// <summary>生命状态。</summary>
	HealthStatus,
	/// <summary>能量槽状态。</summary>
	GaugeStatus,
	/// <summary>距离。</summary>
	Distance,
	/// <summary>正在攻击。</summary>
	Attacking,
	/// <summary>正在格挡。</summary>
	Blocking,
	/// <summary>眩晕。</summary>
	Stunned,
	/// <summary>倒地。</summary>
	Down,
	//ProjectileDistance, // In front of or behind the character?
	//ProjectileSpeed,
}

/// <summary>
/// AI 格挡姿势：格挡状态类别。
/// </summary>
public enum AIBlocking {
	/// <summary>空中格挡。</summary>
	Air,
	/// <summary>站立格挡。</summary>
	High,
	/// <summary>下蹲格挡。</summary>
	Low
}

/// <summary>
/// AI 渴望度：动作的期望程度档位。
/// </summary>
public enum AIDesirability{
	/// <summary>最差选项。</summary>
	TheWorstOption,
	/// <summary>非常不可取。</summary>
	VeryUndesirable,
	/// <summary>不可取。</summary>
	Undesirable,
	/// <summary>尚可。</summary>
	NotBad,
	/// <summary>可取。</summary>
	Desirable,
	/// <summary>非常可取。</summary>
	VeryDesirable,
	/// <summary>最佳选项。</summary>
	TheBestOption
}

/// <summary>
/// AI 反应类型：规则满足后 AI 采取的动作类别。
/// </summary>
public enum AIReactionType {
	/// <summary>待机。</summary>
	Idle,
	/// <summary>前进。</summary>
	MoveForward,
	/// <summary>后退。</summary>
	MoveBack,
	/// <summary>下蹲。</summary>
	Crouch,
	/// <summary>垂直跳。</summary>
	JumpStraight,
	/// <summary>前跳。</summary>
	JumpForward,
	/// <summary>后跳。</summary>
	JumpBack,
	/// <summary>下蹲格挡。</summary>
	CrouchBlock,
	/// <summary>站立格挡。</summary>
	StandBlock,
	/// <summary>空中格挡。</summary>
	JumpBlock,
	/// <summary>出招。</summary>
	PlayMove,
	/// <summary>切换行为风格。</summary>
	ChangeBehavior
}

/// <summary>
/// AI 布尔值：规则中的布尔常量。
/// </summary>
public enum AIBoolean{
	/// <summary>真。</summary>
	TRUE,
	/// <summary>假。</summary>
	FALSE
}

/// <summary>
/// AI 伤害档位：攻击伤害的分类。
/// </summary>
public enum AIDamage{
	/// <summary>任意。</summary>
	Any,
	/// <summary>非常弱。</summary>
	VeryWeak,
	/// <summary>弱。</summary>
	Weak,
	/// <summary>中。</summary>
	Medium,
	/// <summary>强。</summary>
	Strong,
	/// <summary>非常强。</summary>
	VeryStrong
}

/// <summary>
/// AI 水平移动状态：角色的水平移动类别。
/// </summary>
public enum AIHorizontalMovement {
	/// <summary>前进。</summary>
	MovingForward,
	/// <summary>静止。</summary>
	Still,
	/// <summary>后退。</summary>
	MovingBack
}

/// <summary>
/// AI 移动速度档位。
/// </summary>
public enum AIMovementSpeed {
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
/// AI 垂直移动状态：角色的垂直移动类别。
/// </summary>
public enum AIVerticalMovement {
	//Down,
	/// <summary>下蹲。</summary>
	Crouching,
	/// <summary>站立。</summary>
	Standing,
	/// <summary>跳跃。</summary>
	Jumping
}

/// <summary>
/// AI 条件（AICondition）：一条模糊规则中的单个条件。
/// <para>通过 conditionType 指定检查的状态类别（攻击/生命/距离/格挡等），配合目标角色与布尔取反构成条件表达式。</para>
/// <para>静态常量定义各条件的字符串标识（带数字前缀以加速字符串比较），供规则生成使用。</para>
/// </summary>
[Serializable]
public class AICondition:ICloneable {
	//-----------------------------------------------------------------------------------------------------------------
	// Public class properties
	//-----------------------------------------------------------------------------------------------------------------
	// We use a numeric prefix for each condition to make the string comparisons faster
	/// <summary>自身正在攻击条件标识。</summary>
	public static readonly string Attacking_Self = "000_" + AIConditionType.Attacking + "_" +TargetCharacter.Self;
	public static readonly string Attacking_Opponent = "001_" + AIConditionType.Attacking + TargetCharacter.Opponent;

	public static readonly string Attacking_AttackType_Self = "002_" + AIConditionType.Attacking + "_" + typeof(AttackType) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_AttackType_Opponent = "003_" + AIConditionType.Attacking + "_" + typeof(AttackType) + "_" + TargetCharacter.Opponent;

	public static readonly string Attacking_Damage_Self = "004_" + AIConditionType.Attacking + "_" + typeof(AIDamage) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_Damage_Opponent = "005_" + AIConditionType.Attacking + "_" + typeof(AIDamage) + "_" + TargetCharacter.Opponent;

	public static readonly string Attacking_GaugeUsage_Self = "006_" + AIConditionType.Attacking + "_" + typeof(GaugeUsage) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_GaugeUsage_Opponent = "007_" + AIConditionType.Attacking + "_" + typeof(GaugeUsage) + "_" + TargetCharacter.Opponent;

	public static readonly string Attacking_HitType_Self = "008_" + AIConditionType.Attacking + "_" + typeof(HitType) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_HitType_Opponent = "009_" + AIConditionType.Attacking + "_" + typeof(HitType) + "_" + TargetCharacter.Opponent;

	public static readonly string Attacking_StartupSpeed_Self = "010_" + AIConditionType.Attacking + "_StartupSpeed_" + TargetCharacter.Self;
	public static readonly string Attacking_StartupSpeed_Opponent = "011_" + AIConditionType.Attacking + "_StartupSpeed_" + TargetCharacter.Opponent;
	
	public static readonly string Attacking_RecoverySpeed_Self = "012_" + AIConditionType.Attacking + "_RecoverySpeed_" + TargetCharacter.Self;
	public static readonly string Attacking_RecoverySpeed_Opponent = "013_" + AIConditionType.Attacking + "_RecoverySpeed_" + TargetCharacter.Opponent;

	public static readonly string Attacking_HitConfirmType_Self = "014_" + AIConditionType.Attacking + "_" + typeof(HitConfirmType) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_HitConfirmType_Opponent = "015_" + AIConditionType.Attacking + "_" + typeof(HitConfirmType) + "_" + TargetCharacter.Opponent;

	public static readonly string Attacking_FrameData_Self = "016_" + AIConditionType.Attacking + "_" + typeof(CurrentFrameData) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_FrameData_Opponent = "017_" + AIConditionType.Attacking + "_" + typeof(CurrentFrameData) + "_" + TargetCharacter.Opponent;

	public static readonly string Attacking_PreferableDistance_Self = "018_" + AIConditionType.Attacking + "_" + typeof(CharacterDistance) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_PreferableDistance_Opponent = "019_" + AIConditionType.Attacking + "_" + typeof(CharacterDistance) + "_" + TargetCharacter.Opponent;

	public static readonly string Blocking_Self = "020_" + AIConditionType.Blocking + "_" + TargetCharacter.Self;
	public static readonly string Blocking_Opponent = "021_" + AIConditionType.Blocking + "_" + TargetCharacter.Opponent;

	public static readonly string Distance_Self = "022_" + AIConditionType.Distance + "_" + TargetCharacter.Self;
	public static readonly string Distance_Opponent = "023_" + AIConditionType.Distance + "_" + TargetCharacter.Opponent;

	public static readonly string Down_Self = "024_" + AIConditionType.Down + "_" + TargetCharacter.Self;
	public static readonly string Down_Opponent = "025_" + AIConditionType.Down + "_" + TargetCharacter.Opponent;

	public static readonly string Gauge_Self = "026_" + AIConditionType.GaugeStatus + "_" + TargetCharacter.Self;
	public static readonly string Gauge_Opponent = "027_" + AIConditionType.GaugeStatus + "_" + TargetCharacter.Opponent;

	public static readonly string Health_Self = "028_" + AIConditionType.HealthStatus + "_" + TargetCharacter.Self;
	public static readonly string Health_Opponent = "029_" + AIConditionType.HealthStatus + "_" + TargetCharacter.Opponent;

	public static readonly string HorizontalMovement_Self = "030_" + AIConditionType.HorizontalMovement + "_" + TargetCharacter.Self;
	public static readonly string HorizontalMovement_Opponent = "031_" + AIConditionType.HorizontalMovement + "_" + TargetCharacter.Opponent;

	public static readonly string HorizontalMovementSpeed_Self = "032_" + AIConditionType.HorizontalMovement + typeof(AIMovementSpeed) + "_" + TargetCharacter.Self;
	public static readonly string HorizontalMovementSpeed_Opponent = "033_" + AIConditionType.HorizontalMovement + typeof(AIMovementSpeed) + "_" + TargetCharacter.Opponent;

	public static readonly string JumpArc_Self = "034_" + AIConditionType.VerticalMovement + typeof(JumpArc) + "_" + TargetCharacter.Self;
	public static readonly string JumpArc_Opponent = "035_" + AIConditionType.VerticalMovement + typeof(JumpArc) + "_" + TargetCharacter.Opponent;

	public static readonly string Stunned_Self = "036_" + AIConditionType.Stunned + "_" + TargetCharacter.Self;
	public static readonly string Stunned_Opponent = "037_" + AIConditionType.Stunned + "_" + TargetCharacter.Opponent;

	public static readonly string VerticalMovement_Self = "038_" + AIConditionType.VerticalMovement + "_" + TargetCharacter.Self;
	public static readonly string VerticalMovement_Opponent = "039_" + AIConditionType.VerticalMovement + "_" + TargetCharacter.Opponent;

	// Public instance properties
	/// <summary>该条件是否启用。</summary>
	public bool enabled = true;
	/// <summary>布尔取反（FALSE 表示条件取反）。</summary>
	public AIBoolean boolean = AIBoolean.TRUE;

	/// <summary>目标角色（自身/对手）。</summary>
	public TargetCharacter targetCharacter = TargetCharacter.Self;
	/// <summary>条件类型。</summary>
	public AIConditionType conditionType = AIConditionType.Idle;
	/// <summary>水平移动条件。</summary>
	public AIHorizontalMovement horizontalMovement = AIHorizontalMovement.Still;
	/// <summary>垂直移动条件。</summary>
	public AIVerticalMovement verticalMovement = AIVerticalMovement.Standing;
	/// <summary>移动速度条件。</summary>
	public AIMovementSpeed movementSpeed = AIMovementSpeed.Any;
	/// <summary>生命状态条件。</summary>
	public HealthStatus healthStatus = HealthStatus.Healthy;
	/// <summary>能量槽条件。</summary>
	public GaugeUsage gaugeStatus = GaugeUsage.Any;
	/// <summary>距离条件。</summary>
	public CharacterDistance playerDistance = CharacterDistance.Mid;
	/// <summary>跳跃弧线阶段条件。</summary>
	public JumpArc jumping = global::JumpArc.Any;
	/// <summary>格挡姿势条件。</summary>
	public AIBlocking blocking = AIBlocking.High;
	/// <summary>招式分类条件（攻击类型/速度/能量消耗等）。</summary>
	public MoveClassification moveClassification;
	/// <summary>招式帧阶段条件（前摇/判定/后摇）。</summary>
	public CurrentFrameData moveFrameData = CurrentFrameData.Any;
	/// <summary>攻击伤害条件。</summary>
	public AIDamage moveDamage = AIDamage.Any;

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现，使用序列化克隆）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this, true);
	}
}

/// <summary>
/// AI 事件（AIEvent）：一条规则中的条件组（多个条件 AND 组合），满足时触发反应。
/// </summary>
[Serializable]
public class AIEvent: System.ICloneable {
	/// <summary>该事件是否启用。</summary>
	public bool enabled = true;
	/// <summary>事件布尔取反。</summary>
	public AIBoolean boolean = AIBoolean.TRUE;
	/// <summary>事件包含的条件列表（AND 组合）。</summary>
	public AICondition[] conditions = new AICondition[0];
	
	/// <summary>编辑器用：条件面板开关。</summary>
	[HideInInspector] public bool conditionsToggle;

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this, true);
	}
}

/// <summary>
/// AI 反应（AIReaction）：规则条件满足后 AI 采取的动作及其渴望度。
/// <para>支持待机/移动/跳跃/格挡/切换行为/出招（按攻击类型/伤害/命中类型/速度/能量消耗/距离筛选）等反应。</para>
/// <para>静态常量定义各反应的字符串标识，供规则生成使用。</para>
/// </summary>
[Serializable]
public class AIReaction: System.ICloneable {
	//-----------------------------------------------------------------------------------------------------------------
	// Public class properties
	//-----------------------------------------------------------------------------------------------------------------
	// We use a numeric prefix for each condition to make the string comparisons faster
	/// <summary>下蹲反应标识。</summary>
	public static readonly string Crouch = "000_" + AIReactionType.Crouch;
	public static readonly string CrouchBlock = "001_" + AIReactionType.CrouchBlock;
	public static readonly string Idle = "002_" + AIReactionType.Idle;
	public static readonly string JumpBackward = "003_" + AIReactionType.JumpBack;
	public static readonly string JumpBlock = "004_" + AIReactionType.JumpBlock;
	public static readonly string JumpForward = "005_" + AIReactionType.JumpForward;
	public static readonly string JumpStraight = "006_" + AIReactionType.JumpStraight;
	public static readonly string MoveForward = "007_" + AIReactionType.MoveForward;
	public static readonly string MoveBackward = "008_" + AIReactionType.MoveBack;
	public static readonly string StandBlock = "009_" + AIReactionType.StandBlock;

	public static readonly string PlayMove_AttackType_AntiAir = "010_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.AntiAir;
	public static readonly string PlayMove_AttackType_BackLauncher = "011_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.BackLauncher;
	public static readonly string PlayMove_AttackType_Dive = "012_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.Dive;
	public static readonly string PlayMove_AttackType_ForwardLauncher = "013_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.ForwardLauncher;
	public static readonly string PlayMove_AttackType_Neutral = "014_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.Neutral;
	public static readonly string PlayMove_AttackType_NormalAttack = "015_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.NormalAttack;
	public static readonly string PlayMove_AttackType_Projectile = "016_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.Projectile;

	public static readonly string PlayMove_Damage_VeryWeak = "020_" + AIReactionType.PlayMove + "_" + typeof(AIDamage) + "_" + AIDamage.VeryWeak;
	public static readonly string PlayMove_Damage_Weak = "021_" + AIReactionType.PlayMove + "_" + typeof(AIDamage) + "_" + AIDamage.Weak;
	public static readonly string PlayMove_Damage_Medium = "022_" + AIReactionType.PlayMove + "_" + typeof(AIDamage) + "_" + AIDamage.Medium;
	public static readonly string PlayMove_Damage_Strong = "023_" + AIReactionType.PlayMove + "_" + typeof(AIDamage) + "_" + AIDamage.Strong;
	public static readonly string PlayMove_Damage_VeryStrong = "024_" + AIReactionType.PlayMove + "_" + typeof(AIDamage) + "_" + AIDamage.VeryStrong;

	public static readonly string PlayMove_HitType_HighKnockdown = "030_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.HighKnockdown;
	public static readonly string PlayMove_HitType_HighLow = "031_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.Mid;
	public static readonly string PlayMove_HitType_KnockBack = "032_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.KnockBack;
	public static readonly string PlayMove_HitType_Launcher = "033_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.Launcher;
	public static readonly string PlayMove_HitType_Low = "034_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.Low;
	public static readonly string PlayMove_HitType_MidKnockdown = "035_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.MidKnockdown;
	public static readonly string PlayMove_HitType_Overhead = "036_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.Overhead;
	public static readonly string PlayMove_HitType_Sweep = "037_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.Sweep;

	public static readonly string PlayMove_StartupSpeed_VeryFast = "040_" + AIReactionType.PlayMove + "_StartupSpeed_" + FrameSpeed.VeryFast;
	public static readonly string PlayMove_StartupSpeed_Fast = "041_" + AIReactionType.PlayMove + "_StartupSpeed_" + FrameSpeed.Fast;
	public static readonly string PlayMove_StartupSpeed_Normal = "042_" + AIReactionType.PlayMove + "_StartupSpeed_" + FrameSpeed.Normal;
	public static readonly string PlayMove_StartupSpeed_Slow = "043_" + AIReactionType.PlayMove + "_StartupSpeed_" + FrameSpeed.Slow;
	public static readonly string PlayMove_StartupSpeed_VerySlow = "044_" + AIReactionType.PlayMove + "_StartupSpeed_" + FrameSpeed.VerySlow;

	public static readonly string PlayMove_RecoverySpeed_VeryFast = "050_" + AIReactionType.PlayMove + "_RecoverySpeed_" + FrameSpeed.VeryFast;
	public static readonly string PlayMove_RecoverySpeed_Fast = "051_" + AIReactionType.PlayMove + "_RecoverySpeed_" + FrameSpeed.Fast;
	public static readonly string PlayMove_RecoverySpeed_Normal = "052_" + AIReactionType.PlayMove + "_RecoverySpeed_" + FrameSpeed.Normal;
	public static readonly string PlayMove_RecoverySpeed_Slow = "053_" + AIReactionType.PlayMove + "_RecoverySpeed_" + FrameSpeed.Slow;
	public static readonly string PlayMove_RecoverySpeed_VerySlow = "054_" + AIReactionType.PlayMove + "_RecoverySpeed_" + FrameSpeed.VerySlow;

	public static readonly string PlayMove_HitConfirmType_Hit = "060_" + AIReactionType.PlayMove + "_" + typeof(HitConfirmType) + "_" + HitConfirmType.Hit;
	public static readonly string PlayMove_HitConfirmType_Throw = "061_" + AIReactionType.PlayMove + "_" + typeof(HitConfirmType) + "_" + HitConfirmType.Throw;

	public static readonly string PlayMove_GaugeUsage_All = "070_" + AIReactionType.PlayMove + "_" + typeof(GaugeUsage) + "_" + GaugeUsage.All;
	public static readonly string PlayMove_GaugeUsage_Half = "071_" + AIReactionType.PlayMove + "_" + typeof(GaugeUsage) + "_" + GaugeUsage.Half;
	public static readonly string PlayMove_GaugeUsage_None = "072_" + AIReactionType.PlayMove + "_" + typeof(GaugeUsage) + "_" + GaugeUsage.None;
	public static readonly string PlayMove_GaugeUsage_Quarter = "073_" + AIReactionType.PlayMove + "_" + typeof(GaugeUsage) + "_" + GaugeUsage.Quarter;
	public static readonly string PlayMove_GaugeUsage_ThreeQuarters = "074_" + AIReactionType.PlayMove + "_" + typeof(GaugeUsage) + "_" + GaugeUsage.ThreeQuarters;

	public static readonly string PlayMove_PreferableDistance_VeryClose = "080_" + AIReactionType.PlayMove + "_" + typeof(CharacterDistance) + "_" + CharacterDistance.VeryClose;
	public static readonly string PlayMove_PreferableDistance_Close = "081_" + AIReactionType.PlayMove + "_" + typeof(CharacterDistance) + "_" + CharacterDistance.Close;
	public static readonly string PlayMove_PreferableDistance_Mid = "082_" + AIReactionType.PlayMove + "_" + typeof(CharacterDistance) + "_" + CharacterDistance.Mid;
	public static readonly string PlayMove_PreferableDistance_Far = "083_" + AIReactionType.PlayMove + "_" + typeof(CharacterDistance) + "_" + CharacterDistance.Far;
	public static readonly string PlayMove_PreferableDistance_VeryFar = "084_" + AIReactionType.PlayMove + "_" + typeof(CharacterDistance) + "_" + CharacterDistance.VeryFar;
	public static readonly string PlayMove_RandomAttack = "090_" + AIReactionType.PlayMove + "_Random";

	public static readonly string ChangeBehaviour_Aggressive = "A00_" + AIReactionType.ChangeBehavior + "_" + AIBehavior.Aggressive;
	public static readonly string ChangeBehaviour_Any = "A01_" + AIReactionType.ChangeBehavior + "_" + AIBehavior.Any;
	public static readonly string ChangeBehaviour_Balanced = "A02_" + AIReactionType.ChangeBehavior + "_" + AIBehavior.Balanced;
	public static readonly string ChangeBehaviour_Defensive = "A03_" + AIReactionType.ChangeBehavior + "_" + AIBehavior.Defensive;
	public static readonly string ChangeBehaviour_VeryAggressive = "A04_" + AIReactionType.ChangeBehavior + "_" + AIBehavior.VeryAggressive;
	public static readonly string ChangeBehaviour_VeryDefensive = "A105_" + AIReactionType.ChangeBehavior + "_" + AIBehavior.VeryDefensive;

	// Public instance properties
	/// <summary>反应类型。</summary>
	public AIReactionType reactionType;
	/// <summary>招式分类（选择攻击时使用）。</summary>
	public MoveClassification moveClassification;			// When Attack is chosen
	/// <summary>伤害档位（选择攻击时使用）。</summary>
	public AIDamage moveDamage = AIDamage.Any;				// When Attack is chosen
	/// <summary>特定招式（选择指定招式时使用）。</summary>
	public MoveInfo specificMove;							// When Play Specific Move is chosen
	/// <summary>按键（按按钮反应时使用）。</summary>
	public ButtonPress buttonPress = ButtonPress.Button1;	// Press Button
	/// <summary>目标行为风格（切换行为时使用）。</summary>
	public AIBehavior behavior;								// Change Behavior
	/// <summary>渴望度评分。</summary>
	public AIDesirability desirability = AIDesirability.NotBad;	// Desirability score

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this, true);
	}
}

/// <summary>
/// AI 规则（AIRule）：一条完整的模糊规则（事件 + 反应）。
/// <para>提供将规则转换为模糊推理系统可解析字符串的方法（ToRules/ToDebugInformation），</para>
/// <para>内部通过 ConditionToString 生成 IF 条件部分、ReactionToStrings 生成 THEN 反应部分。</para>
/// </summary>
[Serializable]
public class AIRule: System.ICloneable {
	public static readonly string Rule_AND = " AND ";
	public static readonly string Rule_Close_Parenthesis = ") ";
	public static readonly string Rule_IF = "IF ";
	public static readonly string Rule_IS = " IS ";
	public static readonly string Rule_NOT = " NOT ";
	public static readonly string Rule_Open_Parenthesis = " (";
	public static readonly string Rule_OR = " OR ";
	public static readonly string Rule_THEN = " THEN ";
	
	public static readonly string Debug_AND = " AND ";
	public static readonly string Debug_Close_Parenthesis = ") ";
	public static readonly string Debug_IF = "IF\t\t";
	public static readonly string Debug_IS = " IS ";
	public static readonly string Debug_NOT = " NOT ";
	public static readonly string Debug_Open_Parenthesis = " (";
	public static readonly string Debug_OR = "\nOR\t\t";
	public static readonly string Debug_THEN = "\nTHEN\t";
	
	// Public instance properties
	/// <summary>规则名称。</summary>
	public string ruleName;								// The name of the rule
	/// <summary>事件列表（满足任一事件即触发反应）。</summary>
	public AIEvent[] events = new AIEvent[0];			// Events
	/// <summary>反应列表（事件满足时触发的动作）。</summary>
	public AIReaction[] reactions = new AIReaction[0];	// Reactions triggered when one of the events is true

	// Protected instance properties
	/// <summary>编辑器用：调试开关。</summary>
	[HideInInspector] public bool debugToggle;
	/// <summary>编辑器用：事件面板开关。</summary>
	[HideInInspector] public bool eventsToggle;
	/// <summary>编辑器用：反应面板开关。</summary>
	[HideInInspector] public bool reactionsToggle;

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this, true);
	}

	// Public instance methods
	/// <summary>
	/// 将本规则转换为模糊推理系统可解析的规则字符串列表（每个反应生成一条 IF...THEN 规则）。
	/// </summary>
	/// <returns>规则字符串列表。</returns>
	public List<string> ToRules(){
		List<string> rules = new List<string>();
		List<string> reactions = this.ReactionToStrings();

		if (reactions != null && reactions.Count > 0){
			string condition = this.ConditionToString();

			if (!string.IsNullOrEmpty(condition)){
				foreach (string reaction in reactions){
					rules.Add(condition + reaction);
				}
			}
		}

		return rules;
	}

	/// <summary>
	/// 生成可读的规则调试信息（将规则符号替换为易读文本）。
	/// </summary>
	/// <returns>调试文本行列表。</returns>
	public List<string> ToDebugInformation(){
		List<string> debugInformation = new List<string>();
		List<string> rules = this.ToRules();

		if (rules != null && rules.Count > 0){
			foreach (string rule in rules){
				if (!string.IsNullOrEmpty(rule)){
					debugInformation.Add(
						rule.Replace(AIRule.Rule_AND, AIRule.Debug_AND)
							.Replace(AIRule.Rule_Close_Parenthesis, AIRule.Debug_Close_Parenthesis)
							.Replace(AIRule.Rule_IF, AIRule.Debug_IF)
							.Replace(AIRule.Rule_IS, AIRule.Debug_IS)
							.Replace(AIRule.Rule_NOT, AIRule.Debug_NOT)
							.Replace(AIRule.Rule_Open_Parenthesis, AIRule.Debug_Open_Parenthesis)
							.Replace(AIRule.Rule_OR, AIRule.Debug_OR)
							.Replace(AIRule.Rule_THEN, AIRule.Debug_THEN)
					);
				}
			}
		}

		return debugInformation;
	}

	// Protected instance methods
	/// <summary>
	/// 将规则事件转换为 IF 条件部分字符串（支持多个事件 OR、条件 AND、括号与取反）。
	/// </summary>
	/// <returns>条件字符串；无有效条件返回空字符串。</returns>
	protected string ConditionToString(){
		if (this.events != null && this.events.Length > 0){
			StringBuilder sb = new StringBuilder();

			foreach (AIEvent e in this.events){
				if (e != null && e.conditions != null && e.conditions.Length > 0 && e.enabled){
					StringBuilder sb2 = new StringBuilder();
					foreach (AICondition condition in e.conditions){
						if (condition != null && condition.enabled){
							TargetCharacter target = condition.targetCharacter;
							StringBuilder sb3 = new StringBuilder();

							if (condition.conditionType == AIConditionType.Distance){
								if (condition.playerDistance != CharacterDistance.Any && condition.playerDistance != CharacterDistance.Other){
									sb3	.Append(
											target == TargetCharacter.Self 
											? AICondition.Distance_Self 
											: AICondition.Distance_Opponent
										)
										.Append(AIRule.Rule_IS);

									if (condition.boolean == AIBoolean.FALSE){
										sb3.Append(AIRule.Rule_NOT);
									}
									sb3.Append(condition.playerDistance.ToString());
								}

							}else if (condition.conditionType == AIConditionType.Attacking){
								// Define the attack information
								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT).Append(AIRule.Rule_Open_Parenthesis);
								}

								sb3.Append(
									target == TargetCharacter.Self 
									? AICondition.Attacking_Self 
									: AICondition.Attacking_Opponent
								).Append (AIRule.Rule_IS).Append(AIBoolean.TRUE);

								if (condition.moveFrameData != CurrentFrameData.Any){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_FrameData_Self 
											: AICondition.Attacking_FrameData_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveFrameData.ToString());
								}
								if (!condition.moveClassification.anyAttackType){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_AttackType_Self 
											: AICondition.Attacking_AttackType_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveClassification.attackType.ToString());
								}
								if (!condition.moveClassification.anyHitConfirmType){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_HitConfirmType_Self 
											: AICondition.Attacking_HitConfirmType_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveClassification.hitConfirmType.ToString());
								}
								if (condition.moveClassification.startupSpeed != FrameSpeed.Any){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_StartupSpeed_Self
											: AICondition.Attacking_StartupSpeed_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveClassification.startupSpeed.ToString());
								}
								if (condition.moveClassification.recoverySpeed != FrameSpeed.Any){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_RecoverySpeed_Self
											: AICondition.Attacking_RecoverySpeed_Opponent
											)
											.Append(AIRule.Rule_IS)
											.Append(condition.moveClassification.recoverySpeed.ToString());
								}
								if (!condition.moveClassification.anyHitType){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_HitType_Self
											: AICondition.Attacking_HitType_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveClassification.hitType.ToString());
								}
								if (condition.moveDamage != AIDamage.Any){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_Damage_Self
											: AICondition.Attacking_Damage_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveDamage.ToString());
								}
								if (condition.moveClassification.gaugeUsage != GaugeUsage.Any){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_GaugeUsage_Self
											: AICondition.Attacking_GaugeUsage_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveClassification.gaugeUsage.ToString());
								}
								if (condition.moveClassification.preferableDistance != CharacterDistance.Any && condition.moveClassification.preferableDistance != CharacterDistance.Other){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_PreferableDistance_Self
											: AICondition.Attacking_PreferableDistance_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveClassification.preferableDistance.ToString());
								}

								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_Close_Parenthesis);
								}


							}else if (condition.conditionType == AIConditionType.Blocking){
								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.Blocking_Self
										: AICondition.Blocking_Opponent
									)
									.Append(AIRule.Rule_IS);

								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT);
								}
								sb3.Append(condition.blocking.ToString());

							}else if (condition.conditionType == AIConditionType.Down){
								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.Down_Self
										: AICondition.Down_Opponent
									)
									.Append(AIRule.Rule_IS)
									.Append(condition.boolean);

							}else if (condition.conditionType == AIConditionType.GaugeStatus){
								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.Gauge_Self
										: AICondition.Gauge_Opponent
									)
									.Append(AIRule.Rule_IS);

								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT);
								}
								sb3.Append(condition.gaugeStatus.ToString());

							}else if (condition.conditionType == AIConditionType.HealthStatus){
								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.Health_Self
										: AICondition.Health_Opponent
									)
									.Append(AIRule.Rule_IS);

								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT);
								}
								sb3.Append(condition.healthStatus.ToString());

							}else if (condition.conditionType == AIConditionType.Idle){
								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT).Append(AIRule.Rule_Open_Parenthesis);
								}

								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.VerticalMovement_Self
										: AICondition.VerticalMovement_Opponent
									)
									.Append(AIRule.Rule_IS)
									.Append(AIVerticalMovement.Standing.ToString())
									.Append(AIRule.Rule_AND)
									.Append(
										target == TargetCharacter.Self 
										? AICondition.HorizontalMovement_Self
										: AICondition.HorizontalMovement_Opponent
									)
									.Append(AIRule.Rule_IS)
									.Append(AIHorizontalMovement.Still.ToString());
									

								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_Close_Parenthesis);
								}

							}else if (condition.conditionType == AIConditionType.HorizontalMovement){
								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT).Append(AIRule.Rule_Open_Parenthesis);
								}

								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.HorizontalMovement_Self
										: AICondition.HorizontalMovement_Opponent
									)
									.Append(AIRule.Rule_IS)
									.Append(condition.horizontalMovement.ToString());

								if (condition.horizontalMovement != AIHorizontalMovement.Still && condition.movementSpeed != AIMovementSpeed.Any){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.HorizontalMovementSpeed_Self
											: AICondition.HorizontalMovementSpeed_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.movementSpeed.ToString());
								}

								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_Close_Parenthesis);
								}

							}else if (condition.conditionType == AIConditionType.VerticalMovement){
								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT).Append(AIRule.Rule_Open_Parenthesis);
								}
								
								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.VerticalMovement_Self
										: AICondition.VerticalMovement_Opponent
									)
									.Append(AIRule.Rule_IS)
									.Append(condition.verticalMovement.ToString());

								if (
									condition.verticalMovement == AIVerticalMovement.Jumping && 
									condition.jumping != JumpArc.Any &&
								 	condition.jumping != JumpArc.Other
								 ){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.JumpArc_Self
											: AICondition.JumpArc_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.jumping.ToString());
								}
								
								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_Close_Parenthesis);
								}

							}else if (condition.conditionType == AIConditionType.Stunned){
								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.Stunned_Self
										: AICondition.Stunned_Opponent
									)
									.Append(AIRule.Rule_IS)
									.Append(condition.boolean);
							}

							if (sb3.Length > 0){
								if (sb2.Length == 0){
									sb2.Append(AIRule.Rule_Open_Parenthesis);
								}else{
									sb2.Append(AIRule.Rule_AND);
								}

								sb2.Append(sb3.ToString());
							}
						}
					}

					if (sb2.Length > 0){
						if (sb.Length <= 0){
							sb.Append(AIRule.Rule_IF);
						}else{
							sb.Append(AIRule.Rule_OR);
						}
						
						if (e.boolean == AIBoolean.FALSE){
							sb.Append(AIRule.Rule_NOT);
						}

						sb.Append(sb2.ToString()).Append(AIRule.Rule_Close_Parenthesis);
					}
				}
			}

			if (sb.Length > 0){
				sb.Append(AIRule.Rule_THEN);
				return sb.ToString();
			}
		}
		return string.Empty;
	}

	/// <summary>
	/// 将规则反应转换为 THEN 反应部分字符串列表（每个反应生成一条，含渴望度）。
	/// </summary>
	/// <returns>反应字符串列表。</returns>
	protected List<string> ReactionToStrings(){
		List<string> reactions = new List<string>();

		// Iterate over all the reactions associated to this rule...
		if (this.reactions != null){
			foreach (AIReaction reaction in this.reactions){
				if (reaction != null){
					// Create the desirability string..
					string desirability = AIRule.Rule_IS + reaction.desirability;

					// Find out the type of reaction...
					if (reaction.reactionType == AIReactionType.Crouch){
						reactions.Add(AIReaction.Crouch + desirability);
					}else if (reaction.reactionType == AIReactionType.CrouchBlock){
						reactions.Add(AIReaction.CrouchBlock + desirability);
					}else if (reaction.reactionType == AIReactionType.ChangeBehavior){
						if (reaction.behavior == AIBehavior.Aggressive){
							reactions.Add(AIReaction.ChangeBehaviour_Aggressive + desirability);
						}else if (reaction.behavior == AIBehavior.Any){
							reactions.Add(AIReaction.ChangeBehaviour_Any + desirability);
						}else if (reaction.behavior == AIBehavior.Balanced){
							reactions.Add(AIReaction.ChangeBehaviour_Balanced + desirability);
						}else if (reaction.behavior == AIBehavior.Defensive){
							reactions.Add(AIReaction.ChangeBehaviour_Defensive + desirability);
						}else if (reaction.behavior == AIBehavior.VeryAggressive){
							reactions.Add(AIReaction.ChangeBehaviour_VeryAggressive + desirability);
						}else if (reaction.behavior == AIBehavior.VeryDefensive){
							reactions.Add(AIReaction.ChangeBehaviour_VeryDefensive + desirability);
						}
					}else if (reaction.reactionType == AIReactionType.Idle){
						reactions.Add(AIReaction.Idle + desirability);
					}else if (reaction.reactionType == AIReactionType.JumpBack){
						reactions.Add(AIReaction.JumpBackward + desirability);
					}else if (reaction.reactionType == AIReactionType.JumpForward){
						reactions.Add(AIReaction.JumpForward + desirability);
					}else if (reaction.reactionType == AIReactionType.JumpStraight){
						reactions.Add(AIReaction.JumpStraight + desirability);
					}else if (reaction.reactionType == AIReactionType.MoveBack){
						reactions.Add(AIReaction.MoveBackward + desirability);
					}else if (reaction.reactionType == AIReactionType.MoveForward){
						reactions.Add(AIReaction.MoveForward + desirability);
					}else if (reaction.reactionType == AIReactionType.PlayMove){
						// If it's an attack, define the type of attack...
						List<string> attackInformation = new List<string>();

						if (!reaction.moveClassification.anyAttackType){
							if (reaction.moveClassification.attackType == AttackType.AntiAir){
								attackInformation.Add(AIReaction.PlayMove_AttackType_AntiAir + desirability);
							}else if (reaction.moveClassification.attackType == AttackType.BackLauncher){
								attackInformation.Add(AIReaction.PlayMove_AttackType_BackLauncher + desirability);
							}else if (reaction.moveClassification.attackType == AttackType.Dive){
								attackInformation.Add(AIReaction.PlayMove_AttackType_Dive + desirability);
							}else if (reaction.moveClassification.attackType == AttackType.ForwardLauncher){
								attackInformation.Add(AIReaction.PlayMove_AttackType_ForwardLauncher + desirability);
							}else if (reaction.moveClassification.attackType == AttackType.Neutral){
								attackInformation.Add(AIReaction.PlayMove_AttackType_Neutral + desirability);
							}else if (reaction.moveClassification.attackType == AttackType.NormalAttack){
								attackInformation.Add(AIReaction.PlayMove_AttackType_NormalAttack + desirability);
							}else if (reaction.moveClassification.attackType == AttackType.Projectile){
								attackInformation.Add(AIReaction.PlayMove_AttackType_Projectile + desirability);
							}
						}
						if (!reaction.moveClassification.anyHitConfirmType){
							if (reaction.moveClassification.hitConfirmType == HitConfirmType.Hit){
								attackInformation.Add(AIReaction.PlayMove_HitConfirmType_Hit + desirability);
							}else if (reaction.moveClassification.hitConfirmType == HitConfirmType.Throw){
								attackInformation.Add(AIReaction.PlayMove_HitConfirmType_Throw + desirability);
							}
						}

						if (reaction.moveClassification.startupSpeed != FrameSpeed.Any){
							if (reaction.moveClassification.startupSpeed == FrameSpeed.VeryFast){
								attackInformation.Add(AIReaction.PlayMove_StartupSpeed_VeryFast + desirability);
							}else if (reaction.moveClassification.startupSpeed == FrameSpeed.Fast){
								attackInformation.Add(AIReaction.PlayMove_StartupSpeed_Fast + desirability);
							}else if (reaction.moveClassification.startupSpeed == FrameSpeed.Normal){
								attackInformation.Add(AIReaction.PlayMove_StartupSpeed_Normal + desirability);
							}else if (reaction.moveClassification.startupSpeed == FrameSpeed.Slow){
								attackInformation.Add(AIReaction.PlayMove_StartupSpeed_Slow + desirability);
							}else if (reaction.moveClassification.startupSpeed == FrameSpeed.Slow){
								attackInformation.Add(AIReaction.PlayMove_StartupSpeed_VerySlow + desirability);
							}
						}

						if (reaction.moveClassification.recoverySpeed != FrameSpeed.Any){
							if (reaction.moveClassification.recoverySpeed == FrameSpeed.VeryFast){
								attackInformation.Add(AIReaction.PlayMove_RecoverySpeed_VeryFast + desirability);
							}else if (reaction.moveClassification.recoverySpeed == FrameSpeed.Fast){
								attackInformation.Add(AIReaction.PlayMove_RecoverySpeed_Fast + desirability);
							}else if (reaction.moveClassification.recoverySpeed == FrameSpeed.Normal){
								attackInformation.Add(AIReaction.PlayMove_RecoverySpeed_Normal + desirability);
							}else if (reaction.moveClassification.recoverySpeed == FrameSpeed.Slow){
								attackInformation.Add(AIReaction.PlayMove_RecoverySpeed_Slow + desirability);
							}else if (reaction.moveClassification.recoverySpeed == FrameSpeed.Slow){
								attackInformation.Add(AIReaction.PlayMove_RecoverySpeed_VerySlow + desirability);
							}
						}

						if (!reaction.moveClassification.anyHitType){
							if (reaction.moveClassification.hitType == HitType.HighKnockdown){
								attackInformation.Add(AIReaction.PlayMove_HitType_HighKnockdown + desirability);
							}else if (reaction.moveClassification.hitType == HitType.Mid){
								attackInformation.Add(AIReaction.PlayMove_HitType_HighLow + desirability);
							}else if (reaction.moveClassification.hitType == HitType.KnockBack){
								attackInformation.Add(AIReaction.PlayMove_HitType_KnockBack + desirability);
							}else if (reaction.moveClassification.hitType == HitType.Launcher){
								attackInformation.Add(AIReaction.PlayMove_HitType_Launcher + desirability);
							}else if (reaction.moveClassification.hitType == HitType.Low){
								attackInformation.Add(AIReaction.PlayMove_HitType_Low + desirability);
							}else if (reaction.moveClassification.hitType == HitType.MidKnockdown){
								attackInformation.Add(AIReaction.PlayMove_HitType_MidKnockdown + desirability);
							}else if (reaction.moveClassification.hitType == HitType.Overhead){
								attackInformation.Add(AIReaction.PlayMove_HitType_Overhead + desirability);
							}else if (reaction.moveClassification.hitType == HitType.Sweep){
								attackInformation.Add(AIReaction.PlayMove_HitType_Sweep + desirability);
							}

						}
						if (reaction.moveDamage != AIDamage.Any){
							if (reaction.moveDamage == AIDamage.VeryWeak){
								attackInformation.Add(AIReaction.PlayMove_Damage_VeryWeak + desirability);
							}else if (reaction.moveDamage == AIDamage.Weak){
								attackInformation.Add(AIReaction.PlayMove_Damage_Weak + desirability);
							}else if (reaction.moveDamage == AIDamage.Medium){
								attackInformation.Add(AIReaction.PlayMove_Damage_Medium + desirability);
							}else if (reaction.moveDamage == AIDamage.Strong){
								attackInformation.Add(AIReaction.PlayMove_Damage_Strong + desirability);
							}else if (reaction.moveDamage == AIDamage.VeryStrong){
								attackInformation.Add(AIReaction.PlayMove_Damage_VeryStrong + desirability);
							}

						}
						if (reaction.moveClassification.gaugeUsage != GaugeUsage.Any){
							if (reaction.moveClassification.gaugeUsage == GaugeUsage.None){
								attackInformation.Add(AIReaction.PlayMove_GaugeUsage_None + desirability);
							}else if (reaction.moveClassification.gaugeUsage == GaugeUsage.Quarter){
								attackInformation.Add(AIReaction.PlayMove_GaugeUsage_Quarter + desirability);
							}else if (reaction.moveClassification.gaugeUsage == GaugeUsage.Half){
								attackInformation.Add(AIReaction.PlayMove_GaugeUsage_Half + desirability);
							}else if (reaction.moveClassification.gaugeUsage == GaugeUsage.ThreeQuarters){
								attackInformation.Add(AIReaction.PlayMove_GaugeUsage_ThreeQuarters + desirability);
							}else if (reaction.moveClassification.gaugeUsage == GaugeUsage.All){
								attackInformation.Add(AIReaction.PlayMove_GaugeUsage_All + desirability);
							}

						}
						if (reaction.moveClassification.preferableDistance != CharacterDistance.Any && reaction.moveClassification.preferableDistance != CharacterDistance.Other){
							if (reaction.moveClassification.preferableDistance == CharacterDistance.VeryClose){
								attackInformation.Add(AIReaction.PlayMove_PreferableDistance_VeryClose + desirability);
							}else if (reaction.moveClassification.preferableDistance == CharacterDistance.Close){
								attackInformation.Add(AIReaction.PlayMove_PreferableDistance_Close + desirability);
							}else if (reaction.moveClassification.preferableDistance == CharacterDistance.Mid){
								attackInformation.Add(AIReaction.PlayMove_PreferableDistance_Mid + desirability);
							}else if (reaction.moveClassification.preferableDistance == CharacterDistance.Far){
								attackInformation.Add(AIReaction.PlayMove_PreferableDistance_Far + desirability);
							}else if (reaction.moveClassification.preferableDistance == CharacterDistance.VeryFar){
								attackInformation.Add(AIReaction.PlayMove_PreferableDistance_VeryFar + desirability);
							}
						}

						// If we don't have any information about the attack, choose a random attack...
						if (attackInformation.Count > 0){
							reactions.AddRange(attackInformation);
						}else{
							reactions.Add(AIReaction.PlayMove_RandomAttack + desirability);
						}

					//}else if (reaction.reactionType == AIReactionType.PlaySpecificMove){
					//}else if (reaction.reactionType == AIReactionType.PressButton){
					}else if (reaction.reactionType == AIReactionType.StandBlock){
						reactions.Add(AIReaction.StandBlock + desirability);
					}
				}
			}
		}

		return reactions;
	}
}

namespace UFE3D
{
	/// <summary>
	/// AI 信息（AIInfo）：Fuzzy AI 的完整配置资产（ScriptableObject）。
	/// <para>用途：保存规则集（aiRules）、规则生成器（rulesGenerator）、模糊阈值定义（aiDefinitions）与高级参数，</para>
	/// <para>并提供 GenerateInferenceSystem 方法将规则集转换为可求值的模糊推理系统（InferenceSystem），</para>
	/// <para>供 RuleBasedAI 在运行时决策。</para>
	/// </summary>
    [Serializable]
    public class AIInfo : ScriptableObject
    {
        // public instance properties
		/// <summary>AI 指令集名称。</summary>
        public string instructionsName;
		/// <summary>是否输出调试信息。</summary>
        public bool debugMode;
		/// <summary>是否调试反应权重。</summary>
        public bool debug_ReactionWeight;
		/// <summary>高级选项（决策/动作时机与行为倾向）。</summary>
        public AIAdvancedOptions advancedOptions;
		/// <summary>规则生成器（自动生成规则）。</summary>
        public AIRulesGenerator rulesGenerator;
		/// <summary>用户规则列表。</summary>
        public AIRule[] aiRules = new AIRule[0];
		/// <summary>模糊阈值定义。</summary>
        public AIDefinitions aiDefinitions;

        //-----------------------------------------------------------------------------------------------------------------
        // PUBLIC METHODS
        //-----------------------------------------------------------------------------------------------------------------
		/// <summary>
		/// 获取渴望度枚举对应的数值评分。
		/// </summary>
		/// <param name="desirability">渴望度枚举。</param>
		/// <returns>数值评分；未识别返回 0。</returns>
        public float GetDesirabilityScore(AIDesirability desirability)
        {
            switch (desirability)
            {
                case AIDesirability.Desirable: return this.aiDefinitions.desirability.desirable;
                case AIDesirability.NotBad: return this.aiDefinitions.desirability.notBad;
                case AIDesirability.Undesirable: return this.aiDefinitions.desirability.undesirable;
                case AIDesirability.TheBestOption: return this.aiDefinitions.desirability.theBestOption;
                case AIDesirability.TheWorstOption: return this.aiDefinitions.desirability.theWorstOption;
                case AIDesirability.VeryDesirable: return this.aiDefinitions.desirability.veryDesirable;
                case AIDesirability.VeryUndesirable: return this.aiDefinitions.desirability.veryUndesirable;
                default: return 0f;
            }
        }

		/// <summary>
		/// 生成模糊推理系统：定义全部输入/输出语言变量，加载自动规则与用户规则到推理引擎。
		/// </summary>
		/// <returns>配置完成的推理系统。</returns>
        public AI4Unity.Fuzzy.InferenceSystem GenerateInferenceSystem()
        {
            AI4Unity.Fuzzy.InferenceSystem inferenceSystem = new AI4Unity.Fuzzy.InferenceSystem(DefuzzificationMethod.Average);

            // INPUT VARIABLES
            inferenceSystem.AddInputVariable(this.DefineBooleanVariable(AICondition.Attacking_Self));
            inferenceSystem.AddInputVariable(this.DefineAttackTypeVariable(AICondition.Attacking_AttackType_Self));
            inferenceSystem.AddInputVariable(this.DefineDamageVariable(AICondition.Attacking_Damage_Self, 0f, 1f));
            inferenceSystem.AddInputVariable(this.DefineGaugeVariable(AICondition.Attacking_GaugeUsage_Self));
            inferenceSystem.AddInputVariable(this.DefineHitConfirmTypeVariable(AICondition.Attacking_HitConfirmType_Self));
            inferenceSystem.AddInputVariable(this.DefineFrameSpeedVariable(AICondition.Attacking_StartupSpeed_Self));
            inferenceSystem.AddInputVariable(this.DefineFrameSpeedVariable(AICondition.Attacking_RecoverySpeed_Self));
            inferenceSystem.AddInputVariable(this.DefineHitTypeVariable(AICondition.Attacking_HitType_Self));
            inferenceSystem.AddInputVariable(this.DefineFrameDataVariable(AICondition.Attacking_FrameData_Self));
            inferenceSystem.AddInputVariable(this.DefineDistanceVariable(AICondition.Attacking_PreferableDistance_Self));
            inferenceSystem.AddInputVariable(this.DefineBlockingVariable(AICondition.Blocking_Self));
            inferenceSystem.AddInputVariable(this.DefineDistanceVariable(AICondition.Distance_Self, 0f, 1f));
            inferenceSystem.AddInputVariable(this.DefineBooleanVariable(AICondition.Down_Self));
            inferenceSystem.AddInputVariable(this.DefineGaugeVariable(AICondition.Gauge_Self));
            inferenceSystem.AddInputVariable(this.DefineHealthVariable(AICondition.Health_Self, 0f, 1f));
            inferenceSystem.AddInputVariable(this.DefineHorizontalMovementVariable(AICondition.HorizontalMovement_Self));
            inferenceSystem.AddInputVariable(this.DefineMovementSpeedVariable(AICondition.HorizontalMovementSpeed_Self, 0f, 100f));
            inferenceSystem.AddInputVariable(this.DefineJumpArcVariable(AICondition.JumpArc_Self));
            inferenceSystem.AddInputVariable(this.DefineBooleanVariable(AICondition.Stunned_Self));
            inferenceSystem.AddInputVariable(this.DefineVerticalMovementVariable(AICondition.VerticalMovement_Self));


            inferenceSystem.AddInputVariable(this.DefineBooleanVariable(AICondition.Attacking_Opponent));
            inferenceSystem.AddInputVariable(this.DefineAttackTypeVariable(AICondition.Attacking_AttackType_Opponent));
            inferenceSystem.AddInputVariable(this.DefineDamageVariable(AICondition.Attacking_Damage_Opponent, 0f, 1));
            inferenceSystem.AddInputVariable(this.DefineGaugeVariable(AICondition.Attacking_GaugeUsage_Opponent));
            inferenceSystem.AddInputVariable(this.DefineHitConfirmTypeVariable(AICondition.Attacking_HitConfirmType_Opponent));
            inferenceSystem.AddInputVariable(this.DefineFrameSpeedVariable(AICondition.Attacking_StartupSpeed_Opponent));
            inferenceSystem.AddInputVariable(this.DefineFrameSpeedVariable(AICondition.Attacking_RecoverySpeed_Opponent));
            inferenceSystem.AddInputVariable(this.DefineHitTypeVariable(AICondition.Attacking_HitType_Opponent));
            inferenceSystem.AddInputVariable(this.DefineFrameDataVariable(AICondition.Attacking_FrameData_Opponent));
            inferenceSystem.AddInputVariable(this.DefineDistanceVariable(AICondition.Attacking_PreferableDistance_Opponent));
            inferenceSystem.AddInputVariable(this.DefineBlockingVariable(AICondition.Blocking_Opponent));
            inferenceSystem.AddInputVariable(this.DefineDistanceVariable(AICondition.Distance_Opponent, 0f, 1f));
            inferenceSystem.AddInputVariable(this.DefineBooleanVariable(AICondition.Down_Opponent));
            inferenceSystem.AddInputVariable(this.DefineGaugeVariable(AICondition.Gauge_Opponent));
            inferenceSystem.AddInputVariable(this.DefineHealthVariable(AICondition.Health_Opponent, 0f, 1f));
            inferenceSystem.AddInputVariable(this.DefineHorizontalMovementVariable(AICondition.HorizontalMovement_Opponent));
            inferenceSystem.AddInputVariable(this.DefineMovementSpeedVariable(AICondition.HorizontalMovementSpeed_Opponent, 0f, 100f));
            inferenceSystem.AddInputVariable(this.DefineJumpArcVariable(AICondition.JumpArc_Opponent));
            inferenceSystem.AddInputVariable(this.DefineBooleanVariable(AICondition.Stunned_Opponent));
            inferenceSystem.AddInputVariable(this.DefineVerticalMovementVariable(AICondition.VerticalMovement_Opponent));

            // OUTPUT VARIABLES
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.Crouch));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.CrouchBlock));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.Idle));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.JumpBlock));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.JumpBackward));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.JumpForward));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.JumpStraight));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.MoveForward));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.MoveBackward));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.StandBlock));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.ChangeBehaviour_Aggressive));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.ChangeBehaviour_Any));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.ChangeBehaviour_Balanced));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.ChangeBehaviour_Defensive));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.ChangeBehaviour_VeryAggressive));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.ChangeBehaviour_VeryDefensive));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_RandomAttack));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_AntiAir));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_BackLauncher));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_Dive));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_ForwardLauncher));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_Neutral));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_NormalAttack));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_Projectile));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_Damage_Medium));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_Damage_Strong));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_Damage_VeryStrong));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_Damage_VeryWeak));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_Damage_Weak));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_GaugeUsage_All));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_GaugeUsage_Half));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_GaugeUsage_None));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_GaugeUsage_Quarter));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_GaugeUsage_ThreeQuarters));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitConfirmType_Hit));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitConfirmType_Throw));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_StartupSpeed_VeryFast));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_StartupSpeed_Fast));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_StartupSpeed_Normal));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_StartupSpeed_Slow));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_StartupSpeed_VerySlow));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_RecoverySpeed_VeryFast));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_RecoverySpeed_Fast));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_RecoverySpeed_Normal));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_RecoverySpeed_Slow));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_RecoverySpeed_VerySlow));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_HighKnockdown));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_HighLow));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_KnockBack));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_Launcher));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_Low));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_MidKnockdown));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_Overhead));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_Sweep));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_PreferableDistance_Close));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_PreferableDistance_Far));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_PreferableDistance_Mid));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_PreferableDistance_VeryClose));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_PreferableDistance_VeryFar));

            string generatedRulePrefix = "Generated Rule: ";
            string userRulePrefix = "User Rule: ";
            int suffix = 1;

            // Add the fuzzy rules generated automatically
            foreach (string fuzzyRule in this.rulesGenerator.GenerateRules())
            {
                if (!string.IsNullOrEmpty(fuzzyRule))
                {
                    inferenceSystem.NewRule(generatedRulePrefix + suffix, fuzzyRule);
                    ++suffix;
                }
            }

            // Generate the Inference System with all the Rules defined by the user
            foreach (AIRule rule in this.aiRules)
            {
                if (rule != null && !string.IsNullOrEmpty(rule.ruleName))
                {
                    List<string> fuzzyRules = rule.ToRules();

                    if (fuzzyRules != null)
                    {
                        if (fuzzyRules.Count == 1)
                        {
                            string fuzzyRule = fuzzyRules[0];
                            if (!string.IsNullOrEmpty(fuzzyRule))
                            {
                                inferenceSystem.NewRule(userRulePrefix + rule.ruleName, fuzzyRule);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < fuzzyRules.Count; ++i)
                            {
                                string fuzzyRule = fuzzyRules[i];
                                if (!string.IsNullOrEmpty(fuzzyRule))
                                {
                                    inferenceSystem.NewRule(userRulePrefix + rule.ruleName + "_" + (i + 1), fuzzyRule);
                                }
                            }
                        }
                    }
                }
            }

            // Finally, return the generated Inference System
            return inferenceSystem;
        }

        //-----------------------------------------------------------------------------------------------------------------
        // PROTECTED METHODS
        //-----------------------------------------------------------------------------------------------------------------
		/// <summary>
		/// 定义布尔型语言变量（TRUE/FALSE 单点函数）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineBooleanVariable(string name, float start = -1f, float end = 1f)
        {
            LinguisticVariable varAttacking = new LinguisticVariable(name, start, end);
            varAttacking.AddLabel(new FuzzySet(AIBoolean.FALSE.ToString(), new SingletonFunction((float)((int)AIBoolean.FALSE))));
            varAttacking.AddLabel(new FuzzySet(AIBoolean.TRUE.ToString(), new SingletonFunction((float)((int)AIBoolean.TRUE))));

            return varAttacking;
        }

		/// <summary>
		/// 定义攻击类型语言变量（各 AttackType 单点函数）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineAttackTypeVariable(string name, float start = -1f, float end = 6f)
        {
            LinguisticVariable varAttackType = new LinguisticVariable(name, start, end);
            varAttackType.AddLabel(new FuzzySet(AttackType.AntiAir.ToString(), new SingletonFunction((float)((int)AttackType.AntiAir))));
            varAttackType.AddLabel(new FuzzySet(AttackType.BackLauncher.ToString(), new SingletonFunction((float)((int)AttackType.BackLauncher))));
            varAttackType.AddLabel(new FuzzySet(AttackType.Dive.ToString(), new SingletonFunction((float)((int)AttackType.Dive))));
            varAttackType.AddLabel(new FuzzySet(AttackType.ForwardLauncher.ToString(), new SingletonFunction((float)((int)AttackType.ForwardLauncher))));
            varAttackType.AddLabel(new FuzzySet(AttackType.Neutral.ToString(), new SingletonFunction((float)((int)AttackType.Neutral))));
            varAttackType.AddLabel(new FuzzySet(AttackType.NormalAttack.ToString(), new SingletonFunction((float)((int)AttackType.NormalAttack))));
            varAttackType.AddLabel(new FuzzySet(AttackType.Projectile.ToString(), new SingletonFunction((float)((int)AttackType.Projectile))));

            return varAttackType;
        }

		/// <summary>
		/// 定义能量槽语言变量（None~All 梯形隶属函数）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineGaugeVariable(string name, float start = -1f, float end = 4f)
        {
            LinguisticVariable varGaugeUsage = new LinguisticVariable(name, start, end);
            varGaugeUsage.AddLabel(new FuzzySet(GaugeUsage.None.ToString(), new TrapezoidalFunction(start, 0.00f, 0.24f, 0.26f)));
            varGaugeUsage.AddLabel(new FuzzySet(GaugeUsage.Quarter.ToString(), new TrapezoidalFunction(0.25f, 0.25f, 0.49f, 0.51f)));
            varGaugeUsage.AddLabel(new FuzzySet(GaugeUsage.Half.ToString(), new TrapezoidalFunction(0.50f, 0.50f, 0.74f, 0.76f)));
            varGaugeUsage.AddLabel(new FuzzySet(GaugeUsage.ThreeQuarters.ToString(), new TrapezoidalFunction(0.75f, 0.75f, 0.99f, 1.01f)));
            varGaugeUsage.AddLabel(new FuzzySet(GaugeUsage.All.ToString(), new TrapezoidalFunction(1.00f, 1.00f, end)));

            return varGaugeUsage;
        }

		/// <summary>
		/// 定义命中确认类型语言变量（Hit/Throw 单点函数）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineHitConfirmTypeVariable(string name, float start = -1f, float end = 1f)
        {
            LinguisticVariable varHitConfirmType = new LinguisticVariable(name, start, end);
            varHitConfirmType.AddLabel(new FuzzySet(HitConfirmType.Hit.ToString(), new SingletonFunction((float)((int)HitConfirmType.Hit))));
            varHitConfirmType.AddLabel(new FuzzySet(HitConfirmType.Throw.ToString(), new SingletonFunction((float)((int)HitConfirmType.Throw))));

            return varHitConfirmType;
        }

		/// <summary>
		/// 定义帧速度语言变量（VerySlow~VeryFast 单点函数）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineFrameSpeedVariable(string name, float start = -1f, float end = 4f)
        {
            LinguisticVariable varFrameSpeed = new LinguisticVariable(name, start, end);
            varFrameSpeed.AddLabel(new FuzzySet(FrameSpeed.VerySlow.ToString(), new SingletonFunction(0f)));
            varFrameSpeed.AddLabel(new FuzzySet(FrameSpeed.Slow.ToString(), new SingletonFunction(1f)));
            varFrameSpeed.AddLabel(new FuzzySet(FrameSpeed.Normal.ToString(), new SingletonFunction(2f)));
            varFrameSpeed.AddLabel(new FuzzySet(FrameSpeed.Fast.ToString(), new SingletonFunction(3f)));
            varFrameSpeed.AddLabel(new FuzzySet(FrameSpeed.VeryFast.ToString(), new SingletonFunction(4f)));

            return varFrameSpeed;
        }

		/// <summary>
		/// 定义命中类型语言变量（各 HitType 单点函数）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineHitTypeVariable(string name, float start = -1f, float end = 7f)
        {
            LinguisticVariable varHitType = new LinguisticVariable(name, start, end);
            varHitType.AddLabel(new FuzzySet(HitType.HighKnockdown.ToString(), new SingletonFunction((float)((int)HitType.HighKnockdown))));
            varHitType.AddLabel(new FuzzySet(HitType.Mid.ToString(), new SingletonFunction((float)((int)HitType.Mid))));
            varHitType.AddLabel(new FuzzySet(HitType.KnockBack.ToString(), new SingletonFunction((float)((int)HitType.KnockBack))));
            varHitType.AddLabel(new FuzzySet(HitType.Launcher.ToString(), new SingletonFunction((float)((int)HitType.Launcher))));
            varHitType.AddLabel(new FuzzySet(HitType.Low.ToString(), new SingletonFunction((float)((int)HitType.Low))));
            varHitType.AddLabel(new FuzzySet(HitType.MidKnockdown.ToString(), new SingletonFunction((float)((int)HitType.MidKnockdown))));
            varHitType.AddLabel(new FuzzySet(HitType.Overhead.ToString(), new SingletonFunction((float)((int)HitType.Overhead))));
            varHitType.AddLabel(new FuzzySet(HitType.Sweep.ToString(), new SingletonFunction((float)((int)HitType.Sweep))));

            return varHitType;
        }

		/// <summary>
		/// 定义帧阶段语言变量（前摇/判定/后摇 单点函数）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineFrameDataVariable(string name, float start = -1f, float end = 3f)
        {
            LinguisticVariable varFrameData = new LinguisticVariable(name, start, end);
            varFrameData.AddLabel(new FuzzySet(CurrentFrameData.ActiveFrames.ToString(), new SingletonFunction((float)((int)CurrentFrameData.ActiveFrames))));
            varFrameData.AddLabel(new FuzzySet(CurrentFrameData.RecoveryFrames.ToString(), new SingletonFunction((float)((int)CurrentFrameData.RecoveryFrames))));
            varFrameData.AddLabel(new FuzzySet(CurrentFrameData.StartupFrames.ToString(), new SingletonFunction((float)((int)CurrentFrameData.StartupFrames))));

            return varFrameData;
        }

		/// <summary>
		/// 定义格挡姿势语言变量（空中/站立/下蹲 单点函数）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineBlockingVariable(string name, float start = -1f, float end = 2f)
        {
            LinguisticVariable varBlocking = new LinguisticVariable(name, start, end);
            varBlocking.AddLabel(new FuzzySet(AIBlocking.Air.ToString(), new SingletonFunction((float)((int)AIBlocking.Air))));
            varBlocking.AddLabel(new FuzzySet(AIBlocking.High.ToString(), new SingletonFunction((float)((int)AIBlocking.High))));
            varBlocking.AddLabel(new FuzzySet(AIBlocking.Low.ToString(), new SingletonFunction((float)((int)AIBlocking.Low))));

            return varBlocking;
        }

		/// <summary>
		/// 定义水平移动语言变量（前进/静止/后退 单点函数）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineHorizontalMovementVariable(string name, float start = -1f, float end = 2f)
        {
            LinguisticVariable varHorizontalMovement = new LinguisticVariable(name, start, end);
            varHorizontalMovement.AddLabel(new FuzzySet(AIHorizontalMovement.MovingBack.ToString(), new SingletonFunction((float)((int)AIHorizontalMovement.MovingBack))));
            varHorizontalMovement.AddLabel(new FuzzySet(AIHorizontalMovement.MovingForward.ToString(), new SingletonFunction((float)((int)AIHorizontalMovement.MovingForward))));
            varHorizontalMovement.AddLabel(new FuzzySet(AIHorizontalMovement.Still.ToString(), new SingletonFunction((float)((int)AIHorizontalMovement.Still))));

            return varHorizontalMovement;
        }

		/// <summary>
		/// 定义跳跃弧线语言变量（起跳/跳跃/顶点/下落/落地 梯形隶属函数）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineJumpArcVariable(string name, float start = 0f, float end = 1f)
        {
            LinguisticVariable varJumpArc = new LinguisticVariable(name, start - 1f, end + 1f);
            varJumpArc.AddLabel(new FuzzySet(
                JumpArc.TakeOff.ToString(),
                new TrapezoidalFunction(start - 1f, start, 0.3f, 0.4f)
            ));
            varJumpArc.AddLabel(new FuzzySet(
                JumpArc.Jumping.ToString(),
                new TrapezoidalFunction(0.3f, 0.4f, 0.55f, 0.65f)
            ));
            varJumpArc.AddLabel(new FuzzySet(
                JumpArc.Top.ToString(),
                new TrapezoidalFunction(0.55f, 0.65f, 0.75f)
            ));
            varJumpArc.AddLabel(new FuzzySet(
                JumpArc.Falling.ToString(),
                new TrapezoidalFunction(0.65f, 0.75f, 0.85f, 0.95f)
            ));
            varJumpArc.AddLabel(new FuzzySet(
                JumpArc.Landing.ToString(),
                new TrapezoidalFunction(0.85f, 0.95f, end, end + 1f)
            ));

            return varJumpArc;
        }

		/// <summary>
		/// 定义垂直移动语言变量（下蹲/站立/跳跃 单点函数）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineVerticalMovementVariable(string name, float start = 0f, float end = 2f)
        {
            LinguisticVariable varVerticalMovement = new LinguisticVariable(name, start, end);
            varVerticalMovement.AddLabel(new FuzzySet(AIVerticalMovement.Crouching.ToString(), new SingletonFunction((float)((int)AIVerticalMovement.Crouching))));
            varVerticalMovement.AddLabel(new FuzzySet(AIVerticalMovement.Jumping.ToString(), new SingletonFunction((float)((int)AIVerticalMovement.Jumping))));
            varVerticalMovement.AddLabel(new FuzzySet(AIVerticalMovement.Standing.ToString(), new SingletonFunction((float)((int)AIVerticalMovement.Standing))));

            return varVerticalMovement;
        }

		/// <summary>
		/// 定义伤害语言变量（VeryWeak~VeryStrong 梯形隶属函数，按配置阈值）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineDamageVariable(string name, float start = 0f, float end = 1f)
        {
            LinguisticVariable varDamage = new LinguisticVariable(name, start - 1f, end + 1f);
            varDamage.AddLabel(new FuzzySet(
                AIDamage.VeryWeak.ToString(),
                new TrapezoidalFunction(start - 1f, start, this.aiDefinitions.damage.veryWeak, this.aiDefinitions.damage.weak)
            ));
            varDamage.AddLabel(new FuzzySet(
                AIDamage.Weak.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.damage.veryWeak, this.aiDefinitions.damage.weak, this.aiDefinitions.damage.medium)
            ));
            varDamage.AddLabel(new FuzzySet(
                AIDamage.Medium.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.damage.weak, this.aiDefinitions.damage.medium, this.aiDefinitions.damage.strong)
            ));
            varDamage.AddLabel(new FuzzySet(
                AIDamage.Strong.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.damage.medium, this.aiDefinitions.damage.strong, this.aiDefinitions.damage.veryStrong)
            ));
            varDamage.AddLabel(new FuzzySet(
                AIDamage.VeryStrong.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.damage.strong, this.aiDefinitions.damage.veryStrong, end, end + 1f)
            ));

            return varDamage;
        }

		/// <summary>
		/// 定义距离语言变量（VeryClose~VeryFar 梯形隶属函数，按配置阈值）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineDistanceVariable(string name, float start = 0f, float end = 1f)
        {
            LinguisticVariable varDistance = new LinguisticVariable(name, start - 1f, end + 1f);
            varDistance.AddLabel(new FuzzySet(
                CharacterDistance.VeryClose.ToString(),
                new TrapezoidalFunction(start - 1f, start, this.aiDefinitions.distance.veryClose, this.aiDefinitions.distance.close)
            ));
            varDistance.AddLabel(new FuzzySet(
                CharacterDistance.Close.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.distance.veryClose, this.aiDefinitions.distance.close, this.aiDefinitions.distance.mid)
            ));
            varDistance.AddLabel(new FuzzySet(
                CharacterDistance.Mid.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.distance.close, this.aiDefinitions.distance.mid, this.aiDefinitions.distance.far)
            ));
            varDistance.AddLabel(new FuzzySet(
                CharacterDistance.Far.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.distance.mid, this.aiDefinitions.distance.far, this.aiDefinitions.distance.veryFar)
            ));
            varDistance.AddLabel(new FuzzySet(
                CharacterDistance.VeryFar.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.distance.far, this.aiDefinitions.distance.veryFar, end, end + 1f)
            ));

            return varDistance;
        }

		/// <summary>
		/// 定义生命状态语言变量（Dead~Healthy 梯形隶属函数，按配置阈值）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineHealthVariable(string name, float start = 0f, float end = 1f)
        {
            LinguisticVariable varHealth = new LinguisticVariable(name, start - 1f, end + 1f);
            if (this.aiDefinitions.health.healthy <= start)
            {
                varHealth.AddLabel(new FuzzySet(HealthStatus.Dead.ToString(), new SingletonFunction(start)));
            }
            else
            {
                varHealth.AddLabel(new FuzzySet(
                    HealthStatus.Dead.ToString(),
                    new TrapezoidalFunction(start - 1f, start, this.aiDefinitions.health.dead, this.aiDefinitions.health.almostDead)
                ));
            }
            varHealth.AddLabel(new FuzzySet(
                HealthStatus.AlmostDead.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.health.dead, this.aiDefinitions.health.almostDead, this.aiDefinitions.health.criticallyWounded)
            ));
            varHealth.AddLabel(new FuzzySet(
                HealthStatus.CriticallyWounded.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.health.almostDead, this.aiDefinitions.health.criticallyWounded, this.aiDefinitions.health.seriouslyWounded)
            ));
            varHealth.AddLabel(new FuzzySet(
                HealthStatus.SeriouslyWounded.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.health.criticallyWounded, this.aiDefinitions.health.seriouslyWounded, this.aiDefinitions.health.moderatelyWounded)
            ));
            varHealth.AddLabel(new FuzzySet(
                HealthStatus.ModeratelyWounded.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.health.seriouslyWounded, this.aiDefinitions.health.moderatelyWounded, this.aiDefinitions.health.lightlyWounded)
            ));
            varHealth.AddLabel(new FuzzySet(
                HealthStatus.LightlyWounded.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.health.moderatelyWounded, this.aiDefinitions.health.lightlyWounded, this.aiDefinitions.health.scratched)
            ));
            varHealth.AddLabel(new FuzzySet(
                HealthStatus.Scratched.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.health.lightlyWounded, this.aiDefinitions.health.scratched, this.aiDefinitions.health.healthy)
            ));
            if (this.aiDefinitions.health.healthy >= end)
            {
                varHealth.AddLabel(new FuzzySet(HealthStatus.Healthy.ToString(), new SingletonFunction(end)));
            }
            else
            {
                varHealth.AddLabel(new FuzzySet(
                    HealthStatus.Healthy.ToString(),
                    new TrapezoidalFunction(this.aiDefinitions.health.scratched, this.aiDefinitions.health.healthy, end, end + 1f)
                ));
            }

            return varHealth;
        }

		/// <summary>
		/// 定义输出语言变量（渴望度 TheWorstOption~TheBestOption 梯形隶属函数）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineOutputVariable(string name)
        {
            float start = 0f;
            float end = 1f;

            LinguisticVariable varOutput = new LinguisticVariable(name, start - 1f, end + 1f);
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.TheWorstOption.ToString(),
                new TrapezoidalFunction(start - 1f, start, this.aiDefinitions.desirability.theWorstOption, this.aiDefinitions.desirability.veryUndesirable)
            ));
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.VeryUndesirable.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.desirability.theWorstOption, this.aiDefinitions.desirability.veryUndesirable, this.aiDefinitions.desirability.undesirable)
            ));
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.Undesirable.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.desirability.veryUndesirable, this.aiDefinitions.desirability.undesirable, this.aiDefinitions.desirability.notBad)
            ));
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.NotBad.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.desirability.undesirable, this.aiDefinitions.desirability.notBad, this.aiDefinitions.desirability.desirable)
            ));
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.Desirable.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.desirability.notBad, this.aiDefinitions.desirability.desirable, this.aiDefinitions.desirability.veryDesirable)
            ));
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.VeryDesirable.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.desirability.desirable, this.aiDefinitions.desirability.veryDesirable, this.aiDefinitions.desirability.theBestOption)
            ));
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.TheBestOption.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.desirability.veryDesirable, this.aiDefinitions.desirability.theBestOption, end, end + 1f)
            ));

            return varOutput;
        }

		/// <summary>
		/// 定义移动速度语言变量（VerySlow~VeryFast 梯形隶属函数，按配置阈值）。
		/// </summary>
		/// <param name="name">变量名。</param>
		/// <param name="start">变量范围起始。</param>
		/// <param name="end">变量范围结束。</param>
		/// <returns>语言变量对象。</returns>
        protected LinguisticVariable DefineMovementSpeedVariable(string name, float start = 0f, float end = 1000f)
        {
            LinguisticVariable varMovementSpeed = new LinguisticVariable(name, start - 1f, end + 1f);
            varMovementSpeed.AddLabel(new FuzzySet(
                AIMovementSpeed.VerySlow.ToString(),
                new TrapezoidalFunction(start - 1f, start, this.aiDefinitions.speed.verySlow, this.aiDefinitions.speed.slow)
            ));
            varMovementSpeed.AddLabel(new FuzzySet(
                AIMovementSpeed.Slow.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.speed.verySlow, this.aiDefinitions.speed.slow, this.aiDefinitions.speed.normal)
            ));
            varMovementSpeed.AddLabel(new FuzzySet(
                AIMovementSpeed.Normal.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.speed.slow, this.aiDefinitions.speed.normal, this.aiDefinitions.speed.fast)
            ));
            varMovementSpeed.AddLabel(new FuzzySet(
                AIMovementSpeed.Fast.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.speed.normal, this.aiDefinitions.speed.fast, this.aiDefinitions.speed.veryFast)
            ));
            varMovementSpeed.AddLabel(new FuzzySet(
                AIMovementSpeed.VeryFast.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.speed.fast, this.aiDefinitions.speed.veryFast, end, end + 1f)
            ));

            return varMovementSpeed;
        }
    }
}