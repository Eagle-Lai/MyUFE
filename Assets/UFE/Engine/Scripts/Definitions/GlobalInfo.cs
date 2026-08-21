using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FPLibrary;
using UFE3D;

/// <summary>
/// 全局信息定义（GlobalInfo）。
/// <para>用途：本文件是 UFE 引擎的"配置字典"，定义了游戏全局配置所需的全部枚举与配置类，</para>
/// <para>包括动画类型、比赛类型、防守方式、身体部位、AI 引擎、回合规则、网络选项、故事模式等。</para>
/// <para>核心类为 UFE3D.GlobalInfo（ScriptableObject 资产），运行时由 UFE.cs 从 Config.asset 加载。</para>
/// <para>辅助类 CloneObject 提供通用对象克隆工具（反射/序列化两种方式）。</para>
/// </summary>

/// <summary>
/// 动画流程类型：决定招式动画由谁驱动。
/// </summary>
public enum AnimationFlow{
	/// <summary>UFE 引擎自身驱动动画流程。</summary>
	UFEEngine,
	/// <summary>Unity 引擎驱动动画流程。</summary>
	UnityEngine
}

/// <summary>
/// 动画系统类型：角色使用的动画系统。
/// </summary>
public enum AnimationType {
	/// <summary>旧版 Legacy 动画系统（Animation 组件播放）。</summary>
    Legacy,
	/// <summary>新版 Mecanim 动画系统（Animator 状态机）。</summary>
    Mecanim
}

/// <summary>
/// 比赛类型：游戏的玩法模式。
/// </summary>
public enum MatchType {
	/// <summary>常规对战。</summary>
    Regular,
	/// <summary>训练模式。</summary>
    Training,
	/// <summary>挑战模式。</summary>
    Challenge
}

/// <summary>
/// 挑战模式自动序列：完成挑战后的自动行为。
/// </summary>
public enum ChallengeAutoSequence {
	/// <summary>自动进入下一挑战。</summary>
    MoveToNext,
	/// <summary>结束挑战。</summary>
    End
}

/// <summary>
/// 动作类型：挑战模式中要求玩家执行的动作类别。
/// </summary>
public enum ActionType {
	/// <summary>按指定按钮。</summary>
    ButtonPress,
	/// <summary>使出指定必杀技。</summary>
    SpecialMove,
	/// <summary>使出指定基础动作。</summary>
    BasicMove
}

/// <summary>
/// 防守方式（BlockType）：角色如何进行格挡。
/// </summary>
public enum BlockType{
	/// <summary>无法格挡。</summary>
	None,
	/// <summary>按住后方向即格挡（默认）。</summary>
	HoldBack,
	/// <summary>自动格挡。</summary>
	AutoBlock,
	/// <summary>按住按钮1格挡。</summary>
	HoldButton1,
	/// <summary>按住按钮2格挡。</summary>
	HoldButton2,
	/// <summary>按住按钮3格挡。</summary>
	HoldButton3,
	/// <summary>按住按钮4格挡。</summary>
	HoldButton4,
	/// <summary>按住按钮5格挡。</summary>
	HoldButton5,
	/// <summary>按住按钮6格挡。</summary>
	HoldButton6,
	/// <summary>按住按钮7格挡。</summary>
	HoldButton7,
	/// <summary>按住按钮8格挡。</summary>
	HoldButton8,
	/// <summary>按住按钮9格挡。</summary>
	HoldButton9,
	/// <summary>按住按钮10格挡。</summary>
	HoldButton10,
	/// <summary>按住按钮11格挡。</summary>
	HoldButton11,
	/// <summary>按住按钮12格挡。</summary>
	HoldButton12
}

/// <summary>
/// 身体部位（BodyPart）：用于 HitBox 判定框绑定与受击部位识别。
/// </summary>
public enum BodyPart {
	/// <summary>无。</summary>
	none,
	/// <summary>头部。</summary>
	head,
	/// <summary>上躯干。</summary>
	upperTorso,
	/// <summary>下躯干。</summary>
	lowerTorso,
	/// <summary>左上臂。</summary>
	leftUpperArm,
	/// <summary>右上臂。</summary>
	rightUpperArm,
	/// <summary>左前臂。</summary>
	leftForearm,
	/// <summary>右前臂。</summary>
	rightForearm,
	/// <summary>左手。</summary>
	leftHand,
	/// <summary>右手。</summary>
	rightHand,
	/// <summary>左大腿。</summary>
	leftThigh,
	/// <summary>右大腿。</summary>
	rightThigh,
	/// <summary>左小腿。</summary>
	leftCalf,
	/// <summary>右小腿。</summary>
	rightCalf,
	/// <summary>左脚。</summary>
	leftFoot,
	/// <summary>右脚。</summary>
	rightFoot,
	/// <summary>根节点。</summary>
	root,
	/// <summary>自定义部位1。</summary>
	custom1,
	/// <summary>自定义部位2。</summary>
	custom2,
	/// <summary>自定义部位3。</summary>
	custom3,
	/// <summary>自定义部位4。</summary>
	custom4,
	/// <summary>自定义部位5。</summary>
	custom5,
	/// <summary>自定义部位6。</summary>
	custom6,
	/// <summary>自定义部位7。</summary>
	custom7,
	/// <summary>自定义部位8。</summary>
	custom8,
	/// <summary>自定义部位9。</summary>
	custom9
}

/// <summary>
/// 网络服务类型：联机功能使用的服务提供商。
/// </summary>
public enum NetworkService {
	/// <summary>Unity 自建服务。</summary>
	Unity,
	/// <summary>Photon 服务。</summary>
	Photon,
	/// <summary>禁用网络。</summary>
    Disabled
}

/// <summary>
/// Photon 托管服务类型：Photon 的部署方式。
/// </summary>
public enum PhotonHostingService{
	/// <summary>自建 Photon 服务器。</summary>
	PhotonServer,
	/// <summary>Photon 云服务。</summary>
	PhotonCloud,
	//PlayFab,
}

/// <summary>
/// 网络消息大小：网络消息中的数值位数。
/// </summary>
public enum NetworkMessageSize{
	/// <summary>8 位。</summary>
	Size8Bits,
	/// <summary>16 位。</summary>
	Size16Bits,
	/// <summary>32 位。</summary>
	Size32Bits,
}

/// <summary>
/// 网络回滚平衡策略：回滚（Rollback）的激进程度。
/// </summary>
public enum NetworkRollbackBalancing{
	/// <summary>禁用回滚。</summary>
	Disabled,
	/// <summary>保守回滚。</summary>
	Conservative,
	/// <summary>激进回滚。</summary>
	Aggressive,
}

/// <summary>
/// 网络帧延迟类型：网络对战时的输入延迟策略。
/// </summary>
public enum NetworkFrameDelay {
	/// <summary>禁用帧延迟。</summary>
    Disabled,
	/// <summary>固定帧延迟。</summary>
    Fixed,
	/// <summary>自动调整帧延迟。</summary>
    Auto
}

/// <summary>
/// 网络输入消息发送频率。
/// </summary>
public enum NetworkInputMessageFrequency{
	/// <summary>每帧发送。</summary>
	EveryFrame,
	/// <summary>每隔一帧发送。</summary>
	EveryOtherFrame,
	/// <summary>每隔几帧发送。</summary>
	EveryFewFrames,
}

/// <summary>
/// 网络同步消息发送频率。
/// </summary>
public enum NetworkSynchronizationMessageFrequency{
	/// <summary>禁用同步消息。</summary>
	Disabled,
	/// <summary>每帧发送。</summary>
	EveryFrame,
	/// <summary>每秒发送。</summary>
	EverySecond,
}

/// <summary>
/// 碰撞类型：角色身上碰撞体的用途分类。
/// </summary>
public enum CollisionType {
	/// <summary>身体碰撞体（物理阻挡）。</summary>
    bodyCollider,
	/// <summary>攻击/受击判定碰撞体。</summary>
    hitCollider,
	/// <summary>无碰撞体。</summary>
    noCollider,
	/// <summary>投技碰撞体。</summary>
    throwCollider
}

/// <summary>
/// 连击显示模式：连击数字（combo）的显示时机。
/// </summary>
public enum ComboDisplayMode{
	/// <summary>连击执行过程中显示。</summary>
	ShowDuringComboExecution,
	/// <summary>连击执行结束后显示。</summary>
	ShowAfterComboExecution,
}

/// <summary>
/// 游戏模式：全局模式状态。
/// </summary>
public enum GameMode{
	/// <summary>无模式。</summary>
	None,
	/// <summary>故事模式。</summary>
	StoryMode,
	/// <summary>对战模式。</summary>
	VersusMode,
	/// <summary>训练模式。</summary>
	TrainingRoom,
	/// <summary>网络对战。</summary>
	NetworkGame,
	/// <summary>挑战模式。</summary>
    ChallengeMode
}

/// <summary>
/// 性别：角色的性别（用于展示）。
/// </summary>
public enum Gender {
	/// <summary>未知。</summary>
	Unknown,
	/// <summary>男。</summary>
	Male,
	/// <summary>女。</summary>
	Female
}

/// <summary>
/// 受击盒类型：判定框的高低类别。
/// </summary>
public enum HitBoxType {
	/// <summary>高位受击盒。</summary>
	high,
	/// <summary>低位受击盒。</summary>
	low
}

/// <summary>
/// 判定盒形状。
/// </summary>
public enum HitBoxShape {
	/// <summary>圆形。</summary>
	circle,
	/// <summary>矩形。</summary>
	rectangle
}

/// <summary>
/// 出招缓冲类型：缓冲输入时允许匹配的范围。
/// </summary>
public enum ExecutionBufferType{
	/// <summary>仅允许招式链接（派生技/连段）。</summary>
	OnlyMoveLinks,
	/// <summary>允许任意招式。</summary>
	AnyMove,
	/// <summary>无缓冲。</summary>
	NoBuffer
}

/// <summary>
/// 弹反（Parry）类型：触发弹反所需的输入。
/// </summary>
public enum ParryType{
	/// <summary>无弹反。</summary>
	None,
	/// <summary>快速按后方向。</summary>
	TapBack,
	/// <summary>快速按前方向。</summary>
	TapForward,
	/// <summary>快速按按钮1。</summary>
	TapButton1,
	/// <summary>快速按按钮2。</summary>
	TapButton2,
	/// <summary>快速按按钮3。</summary>
	TapButton3,
	/// <summary>快速按按钮4。</summary>
	TapButton4,
	/// <summary>快速按按钮5。</summary>
	TapButton5,
	/// <summary>快速按按钮6。</summary>
	TapButton6,
	/// <summary>快速按按钮7。</summary>
	TapButton7,
	/// <summary>快速按按钮8。</summary>
	TapButton8,
	/// <summary>快速按按钮9。</summary>
	TapButton9,
	/// <summary>快速按按钮10。</summary>
	TapButton10,
	/// <summary>快速按按钮11。</summary>
	TapButton11,
	/// <summary>快速按按钮12。</summary>
	TapButton12
}

/// <summary>
/// 弹反硬直类型：弹反成功后对方硬直的计算方式。
/// </summary>
public enum ParryStunType{
	/// <summary>固定硬直帧。</summary>
	Fixed,
	/// <summary>按格挡硬直的百分比。</summary>
	BlockStunPercentage
}

/// <summary>
/// 玩家：对战双方的编号。
/// </summary>
public enum Player { 
	/// <summary>玩家1。</summary>
	Player1,
	/// <summary>玩家2。</summary>
	Player2
}

/// <summary>
/// 朝向：角色所处的方向侧。
/// </summary>
public enum Side {
	/// <summary>左侧。</summary>
	Left,
	/// <summary>右侧。</summary>
	Right
}

/// <summary>
/// 尺寸档位：用于连击衰减、弹跳力度等强度分级。
/// </summary>
public enum Sizes{
	/// <summary>无。</summary>
	None,
	/// <summary>小。</summary>
	Small,
	/// <summary>中。</summary>
	Medium,
	/// <summary>大。</summary>
	High
}

/// <summary>
/// 空中连击衰减类型：空中连击（juggle）衰减的计数依据。
/// </summary>
public enum AirJuggleDeteriorationType{
	/// <summary>按总连击数衰减。</summary>
	ComboHits,
	/// <summary>按空中命中次数衰减。</summary>
	AirHits
}

/// <summary>
/// 空中受击恢复类型：角色被击飞空中的恢复方式。
/// </summary>
public enum AirRecoveryType {
	/// <summary>恢复后允许出招。</summary>
    AllowMoves,
	/// <summary>恢复后不能移动。</summary>
    CantMove,
	/// <summary>不恢复（持续被击飞状态）。</summary>
    DontRecover
}

/// <summary>
/// 输入管理器类型：读取玩家输入使用的输入系统。
/// </summary>
public enum InputManagerType{
	/// <summary>Unity 内置输入管理器。</summary>
	UnityInputManager,
	/// <summary>cInput 插件。</summary>
	cInput,
	/// <summary>Control Freak 插件（移动端虚拟摇杆）。</summary>
	ControlFreak,
	/// <summary>Rewired 插件。</summary>
    Rewired
}

/// <summary>
/// AI 引擎类型：电脑对手使用的 AI 决策引擎。
/// </summary>
public enum AIEngine{
	/// <summary>随机 AI（基于概率）。</summary>
	RandomAI,
	/// <summary>模糊逻辑 AI（评分制）。</summary>
	FuzzyAI
}

/// <summary>
/// AI 行为风格：电脑对手的进攻/防守倾向。
/// </summary>
public enum AIBehavior{
	/// <summary>任意风格。</summary>
	Any,
	/// <summary>非常防守。</summary>
	VeryDefensive,
	/// <summary>防守。</summary>
	Defensive,
	/// <summary>均衡。</summary>
	Balanced,
	/// <summary>进攻。</summary>
	Aggressive,
	/// <summary>非常进攻。</summary>
	VeryAggressive
}

/// <summary>
/// AI 难度级别：电脑对手的强弱档位。
/// </summary>
public enum AIDifficultyLevel{
	/// <summary>非常简单。</summary>
	VeryEasy,
	/// <summary>简单。</summary>
	Easy,
	/// <summary>普通。</summary>
	Normal,
	/// <summary>困难。</summary>
	Hard,
	/// <summary>非常困难。</summary>
	VeryHard,
	/// <summary>不可能战胜。</summary>
	Impossible
}

/// <summary>
/// 训练模式血条模式：训练模式中生命/能量的行为。
/// </summary>
public enum LifeBarTrainingMode{
	/// <summary>自动回满。</summary>
	Refill,
	/// <summary>无限（不掉血）。</summary>
	Infinite,
	/// <summary>正常。</summary>
	Normal
}

/// <summary>
/// 大厅比赛创建系统：联机房间的创建方式。
/// </summary>
public enum LobbyMatchCreationSystem{
	/// <summary>手动创建房间。</summary>
	ManualRoomCreation,
	/// <summary>匹配系统自动配对。</summary>
	MatchMaking
}

/// <summary>
/// 匹配过滤类型：用户数据匹配时的比较方式。
/// </summary>
public enum MatchMakingFilterType {
	/// <summary>数值在范围内。</summary>
    Range,
	/// <summary>相等。</summary>
    Equal,
	/// <summary>不同。</summary>
    Different,
	/// <summary>高于。</summary>
    HigherThen,
	/// <summary>低于。</summary>
    LowerThen
}

/// <summary>
/// 服务器变量更新类型：用户数据变量的更新方式。
/// </summary>
public enum ServerVariableUpdateType {
	/// <summary>ELO 评级系统更新。</summary>
    ELO,
	/// <summary>直接设为指定值。</summary>
    Set,
	/// <summary>在原值上累加。</summary>
    Increment
}

/// <summary>
/// 服务器变量类型：用户数据变量的数据类型。
/// </summary>
public enum ServerVariableType {
	/// <summary>浮点数。</summary>
    Float,
	/// <summary>整数。</summary>
    Integer,
	/// <summary>字符串。</summary>
    String,
	/// <summary>布尔值。</summary>
    Boolean
}

/// <summary>
/// 资源存储模式：预制体等资产的加载方式。
/// </summary>
public enum StorageMode {
	/// <summary>Legacy 方式（直接引用预制体）。</summary>
    Legacy,
	/// <summary>Resources 文件夹方式（按路径加载）。</summary>
    ResourcesFolder,
}

/// <summary>
/// UFE 布尔值：自定义三态环境下的布尔表示。
/// </summary>
public enum UFEBoolean {
	/// <summary>真。</summary>
    TRUE,
	/// <summary>假。</summary>
    FALSE
}

/// <summary>
/// 命中特效生成点：打击特效的生成位置。
/// </summary>
public enum HitEffectSpawnPoint {
	/// <summary>受击盒位置。</summary>
    StrikingHurtBox,
	/// <summary>攻击盒位置。</summary>
    StrokeHitBox,
	/// <summary>两者中间。</summary>
    InBetween
}

/// <summary>
/// 运动传感器类型：移动设备上的姿态传感器。
/// </summary>
public enum MotionSensor {
	/// <summary>无。</summary>
    None,
	/// <summary>加速度计。</summary>
    Accelerometer,
	/// <summary>陀螺仪。</summary>
    Gyroscope
}

/// <summary>
/// AI 指令集：将一个 AI 信息资产（ScriptableObject）与其行为风格绑定。
/// </summary>
[System.Serializable]
public class AIInstructionsSet: ICloneable {
	/// <summary>AI 信息资产（如 Fuzzy AI 的 AIInfo 或 Simple AI 的 SimpleAIBehaviour）。</summary>
	public ScriptableObject aiInfo;
	/// <summary>AI 行为风格（进攻/防守倾向）。</summary>
	public AIBehavior behavior;
	
	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// AI 难度设置：某一难度档位的全部 AI 行为参数。
/// <para>每个 override 字段用于决定是否覆盖对应参数的全局默认值。</para>
/// </summary>
[System.Serializable]
public class AIDifficultySettings: ICloneable {
	/// <summary>该设置对应的难度级别。</summary>
	public AIDifficultyLevel difficultyLevel;
	/// <summary>是否覆盖"决策间隔"参数。</summary>
	public bool overrideTimeBetweenDecisions;
	/// <summary>决策间隔（秒）：AI 每次做出决策的间隔时间。</summary>
	public float timeBetweenDecisions = 0;
	/// <summary>是否覆盖"动作间隔"参数。</summary>
	public bool overrideTimeBetweenActions;
	/// <summary>动作间隔（秒）：AI 执行连续动作的间隔时间。</summary>
	public float timeBetweenActions = 0.05f;
	/// <summary>是否覆盖"侵略性"参数。</summary>
	public bool overrideAggressiveness;
	/// <summary>侵略性（0~1）：AI 主动进攻的倾向。</summary>
	public float aggressiveness = 0.5f;
	/// <summary>是否覆盖"规则遵循度"参数。</summary>
	public bool overrideRuleCompliance;
	/// <summary>规则遵循度（0~1）：AI 按既定规则行动的比例。</summary>
	public float ruleCompliance = .9f;
	/// <summary>是否覆盖"连招效率"参数。</summary>
	public bool overrideComboEfficiency;
	/// <summary>连招效率（0~1）：AI 成功执行连招的比率。</summary>
	public float comboEfficiency = 1f;
	/// <summary>开局时的行为风格。</summary>
	public AIBehavior startupBehavior;

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 调试选项：控制游戏调试功能与网络调试信息的显示。
/// </summary>
[System.Serializable]
public class DebugOptions {
	/// <summary>启动游戏后立即开始战斗（跳过主菜单）。</summary>
    public bool startGameImmediately;
	/// <summary>跳过加载界面。</summary>
    public bool skipLoadingScreen;
	/// <summary>立即开始的比赛类型。</summary>
    public MatchType matchType;
	/// <summary>启用调试模式（屏幕上显示调试信息）。</summary>
    public bool debugMode;
	/// <summary>模拟网络对战（本地帧同步测试）。</summary>
    public bool emulateNetwork;
	/// <summary>训练模式调试器（显示训练专用调试信息）。</summary>
    public bool trainingModeDebugger;
	/// <summary>显示预加载对象列表。</summary>
    public bool preloadedObjects;

	/// <summary>启用网络调试信息面板。</summary>
    public bool networkToggle;
	/// <summary>显示连接日志。</summary>
    public bool connectionLog = true;
	/// <summary>显示 Ping 值。</summary>
    public bool ping = true;
	/// <summary>显示帧延迟。</summary>
    public bool frameDelay = true;
	/// <summary>显示当前本地帧号。</summary>
    public bool currentLocalFrame = true;
	/// <summary>显示当前网络帧号。</summary>
    public bool currentNetworkFrame = true;
	/// <summary>记录反同步错误日志。</summary>
    public bool desyncErrorLog = false;
	/// <summary>运行状态追踪器测试。</summary>
    public bool stateTrackerTest = false;

	/// <summary>玩家1的角色调试信息配置。</summary>
    public CharacterDebugInfo p1DebugInfo;
	/// <summary>玩家2的角色调试信息配置。</summary>
    public CharacterDebugInfo p2DebugInfo;
}

/// <summary>
/// 训练模式选项：训练模式下的生命/能量行为与初始值设置。
/// </summary>
[System.Serializable]
public class TrainingModeOptions {
	/// <summary>显示输入信息。</summary>
    public bool inputInfo;
	/// <summary>冻结时间（时间停止）。</summary>
    public bool freezeTime;
	/// <summary>玩家1初始生命百分比。</summary>
    public float p1StartingLife = 100f;
	/// <summary>玩家2初始生命百分比。</summary>
    public float p2StartingLife = 100f;
	/// <summary>玩家1初始能量百分比。</summary>
    public float p1StartingGauge = 0f;
	/// <summary>玩家2初始能量百分比。</summary>
    public float p2StartingGauge = 0f;
	/// <summary>玩家1生命条模式（回满/无限/正常）。</summary>
    public LifeBarTrainingMode p1Life;
	/// <summary>玩家1能量条模式。</summary>
    public LifeBarTrainingMode p1Gauge;
	/// <summary>玩家2生命条模式。</summary>
    public LifeBarTrainingMode p2Life;
	/// <summary>玩家2能量条模式。</summary>
    public LifeBarTrainingMode p2Gauge;
	/// <summary>回满时间（秒）：生命/能量自动回满所需时间。</summary>
    public float refillTime = 3f;
}

/// <summary>
/// 挑战模式选项：单个挑战关卡（任务）的完整配置。
/// <para>挑战模式要求玩家按指定序列完成动作（如连招挑战）。</para>
/// </summary>
[System.Serializable]
public class ChallengeModeOptions : ICloneable {
	/// <summary>挑战名称。</summary>
    public string challengeName = "";
	/// <summary>挑战描述文本。</summary>
    public string description = "";
	/// <summary>挑战使用的玩家角色。</summary>
    public UFE3D.CharacterInfo character;
	/// <summary>挑战使用的对手角色。</summary>
    public UFE3D.CharacterInfo opCharacter;
	/// <summary>简单 AI 行为引用（对手为 AI 时使用）。</summary>
	public SimpleAIBehaviour ai;
	/// <summary>是否为连招挑战。</summary>
    public bool isCombo;
	/// <summary>对手是否为 AI 控制。</summary>
    public bool aiOpponent;
	/// <summary>挑战数据是否重置。</summary>
    public bool resetData;
	/// <summary>挑战重复次数。</summary>
    public int repeats = 1;
	/// <summary>挑战完成后的自动序列。</summary>
    public ChallengeAutoSequence challengeSequence;
	/// <summary>编辑器用：动作列表面板开关。</summary>
    public bool actionListToggle;
	/// <summary>挑战要求的动作序列。</summary>
    public ActionSequence[] actionSequence = new ActionSequence[0];

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
        return CloneObject.Clone(this);
    }
}

/// <summary>
/// 动作序列：挑战模式中单个要求动作的定义。
/// </summary>
[System.Serializable]
public class ActionSequence {
	/// <summary>动作类型（按钮/必杀技/基础动作）。</summary>
    public ActionType actionType;
	/// <summary>必杀技引用（actionType 为 SpecialMove 时使用）。</summary>
    public MoveInfo specialMove;
	/// <summary>基础动作引用（actionType 为 BasicMove 时使用）。</summary>
    public BasicMoveReference basicMove;
	/// <summary>按钮（actionType 为 ButtonPress 时使用）。</summary>
    public ButtonPress button;
	/// <summary>是否只允许该按钮（禁止其他按钮干扰判定）。</summary>
    public bool onlyAllowThisButton;
}

/// <summary>
/// 角色调试信息：调试模式下显示的角色数据项开关。
/// </summary>
[System.Serializable]
public class CharacterDebugInfo {
	/// <summary>是否启用角色调试信息。</summary>
    public bool toggle;
	/// <summary>显示当前招式名。</summary>
    public bool currentMove = true;
	/// <summary>显示位置坐标。</summary>
    public bool position = true;
	/// <summary>显示生命值。</summary>
    public bool lifePoints = true;
	/// <summary>显示当前主状态。</summary>
    public bool currentState;
	/// <summary>显示当前子状态。</summary>
    public bool currentSubState;
	/// <summary>显示眩晕时间。</summary>
    public bool stunTime = true;
	/// <summary>显示连击数。</summary>
    public bool comboHits = true;
	/// <summary>显示连击伤害。</summary>
    public bool comboDamage = true;
	/// <summary>显示输入。</summary>
    public bool inputs = true;
	/// <summary>显示按钮序列。</summary>
    public bool buttonSequence;
	/// <summary>显示 AI 权重列表。</summary>
    public bool aiWeightList;

	/// <summary>
	/// 默认构造函数。
	/// </summary>
	public CharacterDebugInfo(){}

	/// <summary>
	/// 拷贝构造函数：从另一个调试信息配置复制数据。
	/// </summary>
	/// <param name="other">要复制的源配置。</param>
	public CharacterDebugInfo(CharacterDebugInfo other){
		this.toggle = other.toggle;
		this.currentMove = other.currentMove;
		this.position = other.position;
		this.lifePoints = other.lifePoints;
		this.currentState = other.currentState;
		this.currentSubState = other.currentSubState;
		this.stunTime = other.stunTime;
		this.comboHits = other.comboHits;
		this.comboDamage = other.comboDamage;
		this.inputs = other.inputs;
		this.buttonSequence = other.buttonSequence;
		this.aiWeightList = other.aiWeightList;
	}
}

/// <summary>
/// AI 选项：全局 AI 引擎与难度参数配置。
/// </summary>
[System.Serializable]
public class AIOptions {
	/// <summary>使用的 AI 引擎类型（随机 AI / 模糊 AI）。</summary>
	public AIEngine engine;
	
	// Random AI Engine
	/// <summary>敌人倒地时是否主动攻击。</summary>
	public bool attackWhenEnemyIsDown = false;
	/// <summary>敌人倒地时是否移动接近。</summary>
	public bool moveWhenEnemyIsDown = false;
	/// <summary>随机 AI 的输入频率（每秒输入次数）。</summary>
	public float inputFrequency = .3f;
	/// <summary>编辑器用：距离行为面板开关。</summary>
	public bool behaviourToggle;
	/// <summary>随机 AI 的距离行为列表（按与对手距离切换行为概率）。</summary>
	public AIDistanceBehaviour[] distanceBehaviour = new AIDistanceBehaviour[0];
	
	
	// Fuzzy AI Engine
	/// <summary>模糊 AI 是否支持多核并行计算。</summary>
	public bool multiCoreSupport = true;
	/// <summary>AI 行为是否跨战斗持续保持。</summary>
	public bool persistentBehavior = false;
	/// <summary>编辑器用：难度设置面板开关。</summary>
	public bool difficultyToggle;
	/// <summary>各难度档位的 AI 参数列表。</summary>
	public AIDifficultySettings[] difficultySettings = new AIDifficultySettings[0];

	/// <summary>当前选中的难度级别（Inspector 隐藏，运行时由配置决定）。</summary>
	[HideInInspector] public AIDifficultyLevel selectedDifficultyLevel = AIDifficultyLevel.Normal;
	/// <summary>当前选中的难度参数对象（Inspector 隐藏）。</summary>
	[HideInInspector] public AIDifficultySettings selectedDifficulty;
}

/// <summary>
/// 摄像机选项：对战摄像机的初始位置、旋转、视野及跟随行为配置。
/// </summary>
[System.Serializable]
public class CameraOptions {
	/// <summary>摄像机初始距离（相对目标的偏移）。</summary>
	public Vector3 initialDistance;
	/// <summary>摄像机初始旋转。</summary>
	public Vector3 initialRotation;
	/// <summary>摄像机初始视野（FOV）。</summary>
	public float initialFieldOfView;
	/// <summary>是否跟随跳跃中的角色。</summary>
	public bool followJumpingCharacter;
	/// <summary>摄像机移动速度。</summary>
	public float movementSpeed = 15;
	/// <summary>最小缩放（镜头拉近限制）。</summary>
	public float minZoom = 38;
	/// <summary>最大缩放（镜头拉远限制）。</summary>
	public float maxZoom = 54;
	/// <summary>摄像机最大跟随距离（float 值）。</summary>
	public float maxDistance = 22;
	/// <summary>摄像机最大跟随距离（定点数，运行时实际使用）。</summary>
	public Fix64 _maxDistance = 22;
	/// <summary>是否启用注视（LookAt）目标。</summary>
	public bool enableLookAt;
	/// <summary>摄像机旋转速度。</summary>
	public float rotationSpeed = 20;
	/// <summary>摄像机高度偏移。</summary>
	public float heightOffSet = 4;
	/// <summary>旋转偏移量。</summary>
    public Vector3 rotationOffSet = new Vector3(0, 4, 0);
	/// <summary>运动传感器类型（移动端）。</summary>
    public MotionSensor motionSensor;
	/// <summary>运动传感器灵敏度。</summary>
    public float motionSensibility = 1;
}

/// <summary>
/// 角色旋转选项：控制角色自动转向（朝向对手）的行为。
/// </summary>
[System.Serializable]
public class CharacterRotationOptions {
	/// <summary>是否自动镜像（面向对手）。</summary>
	public bool autoMirror = true;
	/// <summary>跳跃中是否允许旋转朝向。</summary>
	public bool rotateWhileJumping = false;
	/// <summary>是否仅在移动时旋转。</summary>
	public bool rotateOnMoveOnly = false;
	/// <summary>眩晕时是否固定朝向。</summary>
	public bool fixRotationWhenStunned = false;
	/// <summary>格挡时是否固定朝向。</summary>
	public bool fixRotationWhenBlocking = true;
	/// <summary>受击时是否固定朝向。</summary>
	public bool fixRotationOnHit = true;
	/// <summary>旋转速度。</summary>
	public float rotationSpeed = 10;
	/// <summary>旋转速度（定点数，运行时实际使用）。</summary>
    public Fix64 _rotationSpeed;
	/// <summary>镜像切换的融合时间（float 值）。</summary>
    public float mirrorBlending = .1f;
	/// <summary>镜像切换的融合时间（定点数，运行时实际使用）。</summary>
    public Fix64 _mirrorBlending;
}

/// <summary>
/// 连击选项：连击伤害/硬直/空中连击的衰减规则与最小阈值。
/// </summary>
[System.Serializable]
public class ComboOptions {
	/// <summary>连击显示模式。</summary>
	public ComboDisplayMode comboDisplayMode;
	/// <summary>受击硬直衰减档位。</summary>
	public Sizes hitStunDeterioration;
	/// <summary>伤害衰减档位。</summary>
	public Sizes damageDeterioration;
	/// <summary>空中连击衰减档位。</summary>
	public Sizes airJuggleDeterioration;
	/// <summary>最小受击硬直（帧）。</summary>
	public float minHitStun = 1;
	/// <summary>最小受击硬直（定点数版本）。</summary>
    public int _minHitStun;
	/// <summary>最小伤害值（float 值）。</summary>
    public float minDamage = 5;
	/// <summary>最小伤害值（定点数，运行时实际使用）。</summary>
    public Fix64 _minDamage;
	/// <summary>最小击退力（float 值）。</summary>
    public float minPushForce = 5;
	/// <summary>最小击退力（定点数，运行时实际使用）。</summary>
    public Fix64 _minPushForce;
	/// <summary>最大连续破防（crumple）次数。</summary>
    public int maxConsecutiveCrumple = 1;
	/// <summary>空中连击衰减的计数类型。</summary>
	public AirJuggleDeteriorationType airJuggleDeteriorationType;
	/// <summary>是否永不空中受身恢复。</summary>
    public bool neverAirRecover = false;
	/// <summary>空中受身恢复类型。</summary>
    public AirRecoveryType airRecoveryType = AirRecoveryType.CantMove;
	/// <summary>命中时是否重置下落力。</summary>
	public bool resetFallingForceOnHit = true;
	/// <summary>最大连击数。</summary>
	public int maxCombo = 99;
	/// <summary>击退最小力（float 值）。</summary>
	public float knockBackMinForce = 0;
	/// <summary>击退最小力（定点数，运行时实际使用）。</summary>
    public Fix64 _knockBackMinForce;
	/// <summary>是否永不角落推挤（pushback）。</summary>
    public bool neverCornerPush;
	/// <summary>是否固定空中连击重量。</summary>
	public bool fixJuggleWeight = true;
	/// <summary>空中连击固定重量值（float 值）。</summary>
	public float juggleWeight = 200;
	/// <summary>空中连击固定重量值（定点数，运行时实际使用）。</summary>
    public Fix64 _juggleWeight;

}

/// <summary>
/// 弹跳选项：角色被击倒后反弹（bounce）的配置。
/// </summary>
[System.Serializable]
public class BounceOptions {
	/// <summary>弹跳力度档位。</summary>
	public Sizes bounceForce;
	/// <summary>弹跳特效预制体。</summary>
	public GameObject bouncePrefab;
	/// <summary>弹跳特效销毁时间。</summary>
	public float bounceKillTime = 2;
	/// <summary>最小弹跳力（float 值）。</summary>
    public float minimumBounceForce = 30;
	/// <summary>最小弹跳力（定点数，运行时实际使用）。</summary>
    public Fix64 _minimumBounceForce;
	/// <summary>最大弹跳次数（float 值）。</summary>
    public float maximumBounces = 2;
	/// <summary>最大弹跳次数（定点数，运行时实际使用）。</summary>
    public Fix64 _maximumBounces;
	/// <summary>是否粘性反弹（不脱离反弹面）。</summary>
    public bool sticky = false;
	/// <summary>弹跳时是否启用判定盒。</summary>
    public bool bounceHitBoxes = true;
	/// <summary>弹跳时是否震动摄像机。</summary>
	public bool shakeCamOnBounce = true;
	/// <summary>弹跳震屏密度（float 值）。</summary>
	public float shakeDensity = .6f;
	/// <summary>弹跳震屏密度（定点数，运行时实际使用）。</summary>
    public Fix64 _shakeDensity;
	/// <summary>弹跳音效。</summary>
    public AudioClip bounceSound;

}

/// <summary>
/// 格挡选项：格挡与弹反（Parry）的完整配置。
/// </summary>
[System.Serializable]
public class BlockOptions {
	/// <summary>格挡方式类型。</summary>
	public BlockType blockType;
	/// <summary>是否允许空中格挡。</summary>
    public bool allowAirBlock;
	/// <summary>是否忽略受击推挤力下的格挡。</summary>
    public bool ignoreAppliedForceBlock;
	/// <summary>格挡后是否允许取消招式。</summary>
    public bool allowMoveCancel;

	/// <summary>格挡特效预制体。</summary>
	public GameObject blockPrefab;
	/// <summary>格挡特效销毁时间。</summary>
	public float blockKillTime;
	/// <summary>格挡音效。</summary>
	public AudioClip blockSound;
	/// <summary>是否覆盖默认的格挡命中特效。</summary>
	public bool overrideBlockHitEffects;
	/// <summary>自定义的格挡命中特效配置。</summary>
	public HitTypeOptions blockHitEffects;



	/// <summary>弹反触发方式。</summary>
	public ParryType parryType;
	/// <summary>弹反判定时间窗口（float 值）。</summary>
	public float parryTiming;
	/// <summary>弹反判定时间窗口（定点数，运行时实际使用）。</summary>
	public Fix64 _parryTiming;
	/// <summary>弹反硬直类型。</summary>
	public ParryStunType parryStunType;
	/// <summary>弹反硬直帧数。</summary>
	public int parryStunFrames = 10;
	
	/// <summary>弹反特效预制体。</summary>
	public GameObject parryPrefab;
	/// <summary>弹反特效销毁时间。</summary>
	public float parryKillTime;
	/// <summary>弹反音效。</summary>
	public AudioClip parrySound;
	/// <summary>是否覆盖默认的弹反命中特效。</summary>
	public bool overrideParryHitEffects;
	/// <summary>自定义的弹反命中特效配置。</summary>
	public HitTypeOptions parryHitEffects;


	/// <summary>弹反时角色颜色高亮。</summary>
	public Color parryColor;
	/// <summary>是否允许空中弹反。</summary>
	public bool allowAirParry;
	/// <summary>弹反成功时是否高亮显示。</summary>
	public bool highlightWhenParry;
	/// <summary>弹反成功后是否重置按钮序列。</summary>
	public bool resetButtonSequence;
	/// <summary>简易弹反（降低判定难度）。</summary>
	public bool easyParry;
	/// <summary>是否忽略受击推挤力下的弹反。</summary>
	public bool ignoreAppliedForceParry;
	/// <summary>格挡推挤力档位（TODO）。</summary>
	public Sizes blockPushForce; // TODO
	/// <summary>推进格挡按钮列表（TODO）。</summary>
	public ButtonPress[] pushBlockButtons; // TODO
}

/// <summary>
/// Canvas 缩放器信息：UGUI Canvas Scaler 的序列化配置参数。
/// </summary>
[System.Serializable]
public class CanvasScalerInformation{
	/// <summary>默认精灵 DPI。</summary>
	public float defaultSpriteDPI = 96f;
	/// <summary>回退屏幕 DPI。</summary>
	public float fallbackScreenDPI = 96f;
	/// <summary>宽高匹配比例（0=宽，1=高）。</summary>
	public float matchWidthOrHeight = 0f;
	/// <summary>物理单位类型。</summary>
	public CanvasScaler.Unit physicalUnit = CanvasScaler.Unit.Points;
	/// <summary>参考每单位像素数。</summary>
	public float referencePixelsPerUnit = 100f;
	/// <summary>参考分辨率。</summary>
	public Vector2 referenceResolution = new Vector2(1920f, 1080f);
	/// <summary>缩放因子。</summary>
	public float scaleFactor = 1f;
	/// <summary>屏幕匹配模式。</summary>
	public CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
	/// <summary>缩放模式。</summary>
	public CanvasScaler.ScaleMode scaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

	//---------------------------------------------------------------------------------------------------------
	// We use comment the next line because we use a "Screen Space - Overlay" canvas
	// and the "dynamicPixelsPerUnit" property is only used in "World Space" Canvas.
	//---------------------------------------------------------------------------------------------------------
	//public float dynamicPixelsPerUnit = 100f;
}

/// <summary>
/// 游戏 GUI 配置：UFE 全部界面（屏幕）预制体的引用集合。
/// <para>每个字段对应一个界面预制体，由 UFE.cs 动态加载显示。</para>
/// </summary>
[System.Serializable]
public class GameGUI{
	/// <summary>是否有能量槽（Gauge）。</summary>
	public bool hasGauge = true;
	/// <summary>屏幕切换淡出/淡入时长（定点数）。</summary>
    public Fix64 screenFadeDuration = .5;
	/// <summary>整场游戏切换淡出时长（定点数）。</summary>
    public Fix64 gameFadeDuration = .5;
	/// <summary>回合切换淡出时长（定点数）。</summary>
    public Fix64 roundFadeDuration = .5;
	/// <summary>屏幕切换淡出颜色。</summary>
    public Color screenFadeColor = Color.black;
	/// <summary>游戏切换淡出颜色。</summary>
    public Color gameFadeColor = Color.black;
	/// <summary>回合切换淡出颜色。</summary>
    public Color roundFadeColor = Color.black;
	/// <summary>是否使用 Canvas Scaler 自适应。</summary>
	public bool useCanvasScaler = false;
	/// <summary>Canvas Scaler 参数配置。</summary>
	public CanvasScalerInformation canvasScaler = new CanvasScalerInformation();

	/// <summary>片头界面预制体。</summary>
	public IntroScreen introScreen;
	/// <summary>主菜单界面预制体。</summary>
	public MainMenuScreen mainMenuScreen;
	/// <summary>选项界面预制体。</summary>
	public OptionsScreen optionsScreen;
	/// <summary>制作人员界面预制体。</summary>
	public CreditsScreen creditsScreen;
	/// <summary>暂停界面预制体。</summary>
	public PauseScreen pauseScreen;

	/// <summary>角色选择界面预制体。</summary>
	public CharacterSelectionScreen characterSelectionScreen;
	/// <summary>场地选择界面预制体。</summary>
	public StageSelectionScreen stageSelectionScreen;
	/// <summary>战斗加载界面预制体。</summary>
	public LoadingBattleScreen loadingBattleScreen;
	/// <summary>战斗 HUD（血条/能量条等）预制体。</summary>
	public BattleGUI battleGUI;

	/// <summary>故事模式继续界面预制体。</summary>
	public StoryModeContinueScreen storyModeContinueScreen;
	/// <summary>故事模式游戏结束界面预制体。</summary>
	public StoryModeScreen storyModeGameOverScreen;
	/// <summary>故事模式通关祝贺界面预制体。</summary>
	public StoryModeScreen storyModeCongratulationsScreen;

	/// <summary>对战模式选择界面预制体。</summary>
	public VersusModeScreen versusModeScreen;
	/// <summary>对战结算界面预制体。</summary>
	public VersusModeAfterBattleScreen versusModeAfterBattleScreen;
	
	/// <summary>连接丢失界面预制体。</summary>
	public ConnectionLostScreen connectionLostScreen;
	/// <summary>主机建房界面预制体。</summary>
	public HostGameScreen hostGameScreen;
	/// <summary>加入游戏界面预制体。</summary>
	public JoinGameScreen joinGameScreen;
	/// <summary>网络游戏界面预制体。</summary>
	public NetworkGameScreen networkGameScreen;
	/// <summary>蓝牙对战界面预制体。</summary>
	public BluetoothGameScreen bluetoothGameScreen;
	/// <summary>随机匹配界面预制体。</summary>
	public RandomMatchScreen randomMatchScreen;
	/// <summary>搜索比赛界面预制体。</summary>
	public SearchMatchScreen searchMatchScreen;
}

/// <summary>
/// 击倒选项：不同击倒类型（空中/高/高低/扫腿/破防/墙弹）的倒地行为配置集合。
/// </summary>
[System.Serializable]
public class KnockDownOptions {
	/// <summary>空中击倒配置。</summary>
	public SubKnockdownOptions air;
	/// <summary>高位击倒配置。</summary>
	public SubKnockdownOptions high;
	/// <summary>高低位击倒配置。</summary>
	public SubKnockdownOptions highLow;
	/// <summary>扫腿击倒配置。</summary>
	public SubKnockdownOptions sweep;
	/// <summary>破防（crumple）击倒配置。</summary>
    public SubKnockdownOptions crumple;
	/// <summary>墙壁反弹击倒配置。</summary>
    public SubKnockdownOptions wallbounce;
}

/// <summary>
/// 子击倒选项：单一击倒类型的具体倒地行为参数。
/// </summary>
[System.Serializable]
public class SubKnockdownOptions {
	/// <summary>倒地时间（float 值，秒）。</summary>
	public float knockedOutTime = 2;
	/// <summary>倒地时间（定点数，运行时实际使用）。</summary>
	public Fix64 _knockedOutTime = 2;
	/// <summary>起身时间（float 值，秒）。</summary>
	public float standUpTime = .6f;
	/// <summary>起身时间（定点数，运行时实际使用）。</summary>
	public Fix64 _standUpTime = .6;
	/// <summary>隐藏受击盒的帧数。</summary>
	public int hideHitBoxesOnFrame = 10;
	/// <summary>是否隐藏受击盒。</summary>
    public bool hideHitBoxes;
	/// <summary>编辑器用：面板折叠开关。</summary>
	public bool editorToggle;
	/// <summary>是否支持快速起身（Quick Stand）。</summary>
	public bool hasQuickStand;
	/// <summary>预设的击退力（Vector2，float 版本）。</summary>
	public Vector2 predefinedPushForce;
	/// <summary>预设的击退力（定点向量，运行时实际使用）。</summary>
	public FPVector _predefinedPushForce;
	/// <summary>快速起身按钮列表（TODO）。</summary>
    public ButtonPress[] quickStandButtons = new ButtonPress[0]; // TODO
	/// <summary>最小快速起身时间（TODO）。</summary>
    public Fix64 minQuickStandTime; // TODO
	/// <summary>是否支持延迟起身（Delayed Stand）。</summary>
	public bool hasDelayedStand;
	/// <summary>延迟起身按钮列表（TODO）。</summary>
	public ButtonPress[] delayedStandButtons = new ButtonPress[0]; // TODO
	/// <summary>最大延迟起身时间（TODO）。</summary>
    public Fix64 maxDelayedStandTime; // TODO
}

/// <summary>
/// 命中类型选项：单种命中效果（粒子/音效/震屏/停顿）的配置。
/// </summary>
[System.Serializable]
public class HitTypeOptions {
	/// <summary>命中粒子特效预制体。</summary>
	public GameObject hitParticle;
	/// <summary>特效销毁时间。</summary>
	public float killTime;
	/// <summary>命中音效。</summary>
    public AudioClip hitSound;
	/// <summary>特效生成点。</summary>
    public HitEffectSpawnPoint spawnPoint = HitEffectSpawnPoint.StrokeHitBox;
	/// <summary>冻结时间（float 值，命中瞬间时停）。</summary>
	public float freezingTime;
	/// <summary>冻结时间（定点数，运行时实际使用）。</summary>
	public Fix64 _freezingTime;
	/// <summary>动画播放速度（float 值）。</summary>
	public float animationSpeed = .1f;
	/// <summary>动画播放速度（定点数，运行时实际使用）。</summary>
	public Fix64 _animationSpeed = .1;
	/// <summary>是否自动触发打击停顿（Hit Stop）。</summary>
	public bool autoHitStop = true;
	/// <summary>打击停顿时间（float 值，秒）。</summary>
	public float hitStop = .1f;
	/// <summary>打击停顿时间（定点数，运行时实际使用）。</summary>
	public Fix64 _hitStop = .1;
	/// <summary>在 2P 侧是否镜像特效。</summary>
    public bool mirrorOn2PSide = false;
	/// <summary>命中时是否震动角色。</summary>
    public bool shakeCharacterOnHit = true;
	/// <summary>命中时是否震动摄像机。</summary>
	public bool shakeCameraOnHit = true;
	/// <summary>角色震屏密度（float 值）。</summary>
	public float shakeDensity = .8f;
	/// <summary>角色震屏密度（定点数，运行时实际使用）。</summary>
	public Fix64 _shakeDensity = .8;
	/// <summary>摄像机震屏密度（定点数，运行时实际使用）。</summary>
	public Fix64 _shakeCameraDensity = .8;
	/// <summary>编辑器用：面板折叠开关。</summary>
	public bool editorToggle;
}

/// <summary>
/// 命中选项：不同强度命中（轻/中/重/破防/自定义）的效果配置集合。
/// </summary>
[System.Serializable]
public class HitOptions {
	/// <summary>命中时是否重置动画。</summary>
	public bool resetAnimationOnHit = true;
	/// <summary>轻击效果配置。</summary>
	public HitTypeOptions weakHit;
	/// <summary>中击效果配置。</summary>
	public HitTypeOptions mediumHit;
	/// <summary>重击效果配置。</summary>
	public HitTypeOptions heavyHit;
	/// <summary>破防效果配置。</summary>
	public HitTypeOptions crumpleHit;
	/// <summary>自定义效果1。</summary>
	public HitTypeOptions customHit1;
	/// <summary>自定义效果2。</summary>
	public HitTypeOptions customHit2;
	/// <summary>自定义效果3。</summary>
	public HitTypeOptions customHit3;
}

/// <summary>
/// 输入选项：全局输入管理器选择与按钮确认/取消键配置。
/// </summary>
[System.Serializable]
public class InputOptions {
	/// <summary>输入管理器类型。</summary>
	public InputManagerType inputManagerType;
	/// <summary>cInput 是否允许重复按键。</summary>
	public bool cInputAllowDuplicates = false;
	/// <summary>cInput 重力。</summary>
	public float cInputGravity = 3;
	/// <summary>cInput 灵敏度。</summary>
	public float cInputSensitivity = 3;
	/// <summary>cInput 死区。</summary>
	public float cInputDeadZone = 0.001f;
	/// <summary>cInput 皮肤。</summary>
	public GUISkin cInputSkin = null;
	/// <summary>Control Freak 摇杆预制体。</summary>
    public GameObject controlFreakPrefab = null;
	/// <summary>Control Freak 2 桥接组件。</summary>
    public InputTouchControllerBridge controlFreak2Prefab = null;
	/// <summary>Control Freak 摇杆死区。</summary>
    public float controlFreakDeadZone = 0.5f;
	/// <summary>是否强制数字输入（摇杆输入转为数字方向）。</summary>
    public bool forceDigitalInput = true;
	/// <summary>菜单确认按钮。</summary>
	public ButtonPress confirmButton;
	/// <summary>菜单取消按钮。</summary>
	public ButtonPress cancelButton;
}

/// <summary>
/// 场地选项：单个对战场地的配置。
/// </summary>
[System.Serializable]
public class StageOptions: ICloneable {
	/// <summary>场地名称。</summary>
    public string stageName;
	/// <summary>场地资源路径（Resources 加载方式）。</summary>
    public string stageResourcePath;
	/// <summary>场地音乐资源路径。</summary>
    public string musicResourcePath;
	/// <summary>场地截图（选场界面显示）。</summary>
	public Texture2D screenshot;
	/// <summary>场地预制体。</summary>
	public GameObject prefab;
	/// <summary>场地音乐。</summary>
	public AudioClip music;
	/// <summary>地面摩擦（float 值）。</summary>
    public float groundFriction = 100;
	/// <summary>地面摩擦（定点数，运行时实际使用）。</summary>
    public Fix64 _groundFriction = 100;
	/// <summary>左边界（float 值）。</summary>
	public float leftBoundary = -38;
	/// <summary>左边界（定点数，运行时实际使用）。</summary>
	public Fix64 _leftBoundary = -38;
	/// <summary>右边界（float 值）。</summary>
	public float rightBoundary = 38;
	/// <summary>右边界（定点数，运行时实际使用）。</summary>
	public Fix64 _rightBoundary = 38;
	/// <summary>地面高度（定点数，运行时实际使用）。</summary>
    public Fix64 _groundHeight = 0;

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 语言选项：游戏内显示文本的本地化字符串集合。
/// </summary>
[System.Serializable]
public class LanguageOptions: ICloneable {
	/// <summary>语言名称。</summary>
	public string languageName = "English";
	/// <summary>开始。</summary>
	public string start = "Start";
	/// <summary>选项。</summary>
	public string options = "Options";
	/// <summary>制作人员。</summary>
	public string credits = "Credits";
	/// <summary>选择你的角色。</summary>
	public string selectYourCharacter = "Select Your Character";
	/// <summary>选择你的场地。</summary>
	public string selectYourStage = "Select Your Stage";
	/// <summary>第 %round% 回合。</summary>
	public string round = "Round %round%";
	/// <summary>最终回合。</summary>
	public string finalRound = "Final Round";
	/// <summary>开始战斗！</summary>
	public string fight = "Fight!";
	/// <summary>先手命中！</summary>
	public string firstHit = "First Hit!";
	/// <summary>%number% 连击！</summary>
	public string combo = "%number% hit combo!";
	/// <summary>弹反！</summary>
	public string parry = "Parry!";
	/// <summary>反击！</summary>
	public string counterHit = "Counter!";
	/// <summary>%character% 获胜！</summary>
	public string victory = "%character% wins!";
	/// <summary>挑战开始！</summary>
	public string challengeBegins = "Start!";
	/// <summary>挑战成功！</summary>
	public string challengeEnds = "Success!";
	/// <summary>时间到。</summary>
	public string timeOver = "Time Over";
	/// <summary>完美！</summary>
	public string perfect = "Perfect!";
	/// <summary>再战。</summary>
	public string rematch = "Rematch";
	/// <summary>退出。</summary>
	public string quit = "Quit";
	/// <summary>K.O.。</summary>
	public string ko = "K.O.";
	/// <summary>平局。</summary>
	public string draw = "Draw";
	/// <summary>是否为默认选中的语言。</summary>
	public bool defaultSelection;

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 播报员选项：回合/战斗事件的语音播报音效配置。
/// </summary>
[System.Serializable]
public class AnnouncerOptions {
	/// <summary>播报员名称（Inspector 隐藏）。</summary>
	[HideInInspector]public string announcerName = string.Empty;
	/// <summary>第1回合播报音效。</summary>
	public AudioClip round1;
	/// <summary>第2回合播报音效。</summary>
	public AudioClip round2;
	/// <summary>第3回合播报音效。</summary>
	public AudioClip round3;
	/// <summary>其他回合播报音效。</summary>
	public AudioClip otherRounds;
	/// <summary>最终回合播报音效。</summary>
	public AudioClip finalRound;
	/// <summary>开战播报音效。</summary>
	public AudioClip fight;
	/// <summary>玩家1获胜播报音效。</summary>
	public AudioClip player1Wins;
	/// <summary>玩家2获胜播报音效。</summary>
	public AudioClip player2Wins;
	/// <summary>完美胜利播报音效。</summary>
	public AudioClip perfect;
	/// <summary>先手命中播报音效。</summary>
	public AudioClip firstHit;
	/// <summary>反击播报音效。</summary>
	public AudioClip counterHit;
	/// <summary>弹反播报音效。</summary>
	public AudioClip parry;
	/// <summary>时间到播报音效。</summary>
	public AudioClip timeOver;
	/// <summary>K.O. 播报音效。</summary>
	public AudioClip ko;
	//public bool combosToggle;
	/// <summary>连击播报音效列表。</summary>
	public ComboAnnouncer[] combos;
}

/// <summary>
/// 连击播报：特定连击数触发的语音播报。
/// </summary>
[System.Serializable]
public class ComboAnnouncer: ICloneable {
	/// <summary>播报音效。</summary>
	public AudioClip audio;
	/// <summary>触发该播报的连击数。</summary>
	public int hits;

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 反击选项：Counter Hit（打断对方招式）的增强效果配置。
/// </summary>
[System.Serializable]
public class CounterHitOptions {
	/// <summary>对方前摇阶段被打断是否触发反击判定。</summary>
	public bool startUpFrames = true;
	/// <summary>对方判定阶段被打断是否触发反击。</summary>
	public bool activeFrames = false;
	/// <summary>对方后摇阶段被打断是否触发反击。</summary>
	public bool recoveryFrames = false;
	/// <summary>反击伤害加成百分比（float 值）。</summary>
	public float damageIncrease = 10;
	/// <summary>反击伤害加成百分比（定点数，运行时实际使用）。</summary>
	public Fix64 _damageIncrease = 10;
	/// <summary>反击受击硬直加成百分比（float 值）。</summary>
	public float hitStunIncrease = 50;
	/// <summary>反击受击硬直加成百分比（定点数，运行时实际使用）。</summary>
	public Fix64 _hitStunIncrease = 50;
	/// <summary>反击触发音效。</summary>
	public AudioClip sound;
}

/// <summary>
/// GUI 条选项：血条/能量条等 UI 条的贴图与颜色配置。
/// </summary>
[System.Serializable]
public class GUIBarOptions {
	/// <summary>编辑器用：面板折叠开关。</summary>
	public bool editorToggle;
	/// <summary>编辑器用：预览开关。</summary>
	public bool previewToggle;
	/// <summary>是否水平翻转。</summary>
	public bool flip;
	/// <summary>背景贴图。</summary>
	public Texture2D backgroundImage;
	/// <summary>背景颜色。</summary>
	public Color backgroundColor;
	/// <summary>填充贴图。</summary>
	public Texture2D fillImage;
	/// <summary>填充颜色。</summary>
	public Color fillColor;
	/// <summary>背景矩形区域。</summary>
	public Rect backgroundRect;
	/// <summary>填充矩形区域。</summary>
	public Rect fillRect;
	/// <summary>编辑器用：背景预览对象。</summary>
	public GameObject bgPreview;
	/// <summary>编辑器用：填充预览对象。</summary>
	public GameObject fillPreview;
}

/// <summary>
/// 输入引用：单个输入项（按钮/轴向）到引擎动作的映射定义。
/// <para>将 Unity InputManager 的轴/按钮名映射到 UFE 引擎的 ButtonPress 枚举。</para>
/// </summary>
[System.Serializable]
public class InputReferences: ICloneable {
	// Common Parameters
	/// <summary>输入类型（水平轴/垂直轴/按钮）。</summary>
	public InputType inputType;
	/// <summary>Unity InputManager 中的按钮/轴名称。</summary>
	public string inputButtonName;
	/// <summary>映射到的引擎动作（ButtonPress 枚举）。</summary>
	public ButtonPress engineRelatedButton;
	
	// Input Manager parameters
	/// <summary>摇杆轴名称（Unity InputManager）。</summary>
	public string joystickAxisName;
	
	// cInput parameters
	/// <summary>cInput 正向按键名。</summary>
	public string cInputPositiveKeyName;
	/// <summary>cInput 正向默认按键。</summary>
	public string cInputPositiveDefaultKey;
	/// <summary>cInput 正向备用按键。</summary>
	public string cInputPositiveAlternativeKey;
	
	/// <summary>cInput 负向按键名。</summary>
	public string cInputNegativeKeyName;
	/// <summary>cInput 负向默认按键。</summary>
	public string cInputNegativeDefaultKey;
	/// <summary>cInput 负向备用按键。</summary>
	public string cInputNegativeAlternativeKey;
	
	// Input Viewer
	/// <summary>输入查看器图标1。</summary>
	public Texture2D inputViewerIcon1;
	/// <summary>输入查看器图标2。</summary>
	public Texture2D inputViewerIcon2;
	/// <summary>激活状态图标。</summary>
	public Texture2D activeIcon;
	
	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 回合选项：回合数、计时、出生位置、回合过渡等战斗规则配置。
/// </summary>
[System.Serializable]
public class RoundOptions {
	/// <summary>总回合数（默认 3）。</summary>
	public int totalRounds = 3;
	/// <summary>是否启用计时器。</summary>
	public bool hasTimer = true;
	/// <summary>回合倒计时秒数（float 值）。</summary>
	public float timer = 99;
	/// <summary>回合倒计时秒数（定点数，运行时实际使用）。</summary>
	public Fix64 _timer = 99;
	/// <summary>计时器速度（float 值）。</summary>
	public float timerSpeed = 100;
	/// <summary>计时器速度（定点数，运行时实际使用）。</summary>
	public Fix64 _timerSpeed = 100;
	/// <summary>玩家1出生 X 坐标（float 值）。</summary>
	public float p1XPosition = -5;
	/// <summary>玩家1出生 X 坐标（定点数，运行时实际使用）。</summary>
	public Fix64 _p1XPosition = -5;
	/// <summary>玩家2出生 X 坐标（float 值）。</summary>
	public float p2XPosition = 5;
	/// <summary>玩家2出生 X 坐标（定点数，运行时实际使用）。</summary>
	public Fix64 _p2XPosition = 5;
	/// <summary>整场游戏结束延迟（float 值，秒）。</summary>
	public float endGameDelay = 4;
	/// <summary>整场游戏结束延迟（定点数，运行时实际使用）。</summary>
	public Fix64 _endGameDelay = 4;
	/// <summary>新回合开始延迟（float 值，秒）。</summary>
	public float newRoundDelay = 1;
	/// <summary>新回合开始延迟（定点数，运行时实际使用）。</summary>
	public Fix64 _newRoundDelay = 1;
	/// <summary>慢动作计时（float 值，K.O. 时）。</summary>
    public float slowMoTimer = 3;
	/// <summary>慢动作计时（定点数，运行时实际使用）。</summary>
    public Fix64 _slowMoTimer = 3;
	/// <summary>慢动作速度倍率（float 值）。</summary>
    public float slowMoSpeed = .2f;
	/// <summary>慢动作速度倍率（定点数，运行时实际使用）。</summary>
    public Fix64 _slowMoSpeed = .2;
	/// <summary>胜利音乐。</summary>
	public AudioClip victoryMusic;
	/// <summary>新回合是否重置生命值。</summary>
	public bool resetLifePoints = true;
	/// <summary>新回合是否重置位置。</summary>
	public bool resetPositions = true;
	/// <summary>回合开始是否允许移动。</summary>
    public bool allowMovementStart = true;
	/// <summary>回合结束是否允许移动。</summary>
    public bool allowMovementEnd = true;
	/// <summary>是否禁止能量槽累积。</summary>
    public bool inhibitGaugeGain = true;
	/// <summary>K.O. 时身体是否旋转倒地。</summary>
    public bool rotateBodyKO = true;
	/// <summary>K.O. 时是否慢动作。</summary>
	public bool slowMotionKO = true;
	/// <summary>K.O. 时摄像机是否变焦。</summary>
	public bool cameraZoomKO = true;
	/// <summary>退场后是否冻结摄像机。</summary>
	public bool freezeCamAfterOutro = true;
}

/// <summary>
/// 角色故事集合：以角色索引为键的故事模式配置字典。
/// </summary>
[System.Serializable]
public class CharacterStories : DGP.Util.Collections.SerializableDictionary<int, CharacterStory>{}

/// <summary>
/// 故事模式选项：故事模式的全局配置。
/// </summary>
[System.Serializable]
public class StoryMode{
	/// <summary>是否所有角色共用同一个故事。</summary>
	public bool useSameStoryForAllCharacters;
	/// <summary>角色是否可以在故事模式中与自己对战。</summary>
	public bool canCharactersFightAgainstThemselves;
	/// <summary>默认故事配置（未单独配置角色时使用）。</summary>
	public CharacterStory defaultStory;

	/// <summary>故事模式中可选的角色索引列表。</summary>
	public List<int> selectableCharactersInStoryMode = new List<int>();
	/// <summary>对战模式中可选的角色索引列表。</summary>
	public List<int> selectableCharactersInVersusMode = new List<int>();
	/// <summary>各角色专属的故事配置字典。</summary>
	public CharacterStories characterStories = new CharacterStories();
}

/// <summary>
/// 故事模式战斗：一场故事战斗的配置（战前/战后对话与对手/场地）。
/// </summary>
[System.Serializable]
public class StoryModeBattle{
	/// <summary>战前对话界面。</summary>
	public StoryModeScreen conversationBeforeBattle;
	/// <summary>战后对话界面。</summary>
	public StoryModeScreen conversationAfterBattle;
	/// <summary>对手角色索引。</summary>
	public int opponentCharacterIndex;
	/// <summary>可能的场地索引列表。</summary>
	public List<int> possibleStagesIndexes = new List<int>();
}

/// <summary>
/// 故事模式运行时信息：跟踪当前故事进度（组/战斗索引、已击败对手）。
/// </summary>
public class StoryModeInfo{
	// The information about the character story
	/// <summary>当前角色的故事配置。</summary>
	public CharacterStory characterStory = null;

	// Whether the character can fight against himself in Story Mode
	/// <summary>角色是否可与自己战斗。</summary>
	public bool canFightAgainstHimself = false;

	// The index of the current "group"
	/// <summary>当前"组"的索引。</summary>
	public int currentGroup = 0;

	// The index of the current "battle" in the current "group"
	/// <summary>当前组内"战斗"的索引。</summary>
	public int currentBattle = 0;

	// The information about the current battle
	/// <summary>当前战斗的配置信息。</summary>
	public StoryModeBattle currentBattleInformation = null;

	// The indexes of the characters that have been defeated in the current "group".
	// It's used only if the player must fight the opponents in a group in random order.
	/// <summary>当前组内已击败的对手索引集合（随机顺序战斗时使用）。</summary>
	public HashSet<int> defeatedOpponents = new HashSet<int>();
}

/// <summary>
/// 战斗组：故事模式中一组连续战斗的配置。
/// </summary>
[System.Serializable]
public class FightsGroup{
	/// <summary>最大战斗场数（仅在随机挑选若干对手模式中使用）。</summary>
	public int maxFights = 4; // maxFights is only used when mode == FightsGroupMode.FightAgainstSeveralRandomOpponents
	/// <summary>组内战斗的进行模式。</summary>
	public FightsGroupMode mode = FightsGroupMode.FightAgainstAllOpponentsInTheGroupInRandomOrder;
	/// <summary>组名称。</summary>
	public string name = string.Empty;
	/// <summary>组内对手战斗列表。</summary>
	public StoryModeBattle[] opponents = new StoryModeBattle[0];
	/// <summary>编辑器用：是否在编辑器中显示对手列表。</summary>
	public bool showOpponentsInEditor;
}

/// <summary>
/// 战斗组模式：组内对手战斗的进行方式。
/// </summary>
public enum FightsGroupMode{
	/// <summary>随机挑选组内若干对手战斗。</summary>
	FightAgainstSeveralOpponentsInTheGroupInRandomOrder,
	/// <summary>与组内全部对手按随机顺序战斗。</summary>
	FightAgainstAllOpponentsInTheGroupInRandomOrder,
	/// <summary>与组内全部对手按定义顺序战斗。</summary>
	FightAgainstAllOpponentsInTheGroupInTheDefinedOrder
}

/// <summary>
/// 角色故事：一个角色的开场/结尾演出与战斗组列表。
/// </summary>
[System.Serializable]
public class CharacterStory{
	/// <summary>开场演出界面。</summary>
	public StoryModeScreen opening;
	/// <summary>结尾演出界面。</summary>
	public StoryModeScreen ending;
	/// <summary>战斗组列表。</summary>
	public FightsGroup[] fightsGroups = new FightsGroup[0];
	/// <summary>编辑器用：是否在编辑器中显示故事。</summary>
	public bool showStoryInEditor;
}

/// <summary>
/// AI 距离行为：随机 AI 在特定距离范围内的各动作概率。
/// </summary>
[System.Serializable]
public class AIDistanceBehaviour: ICloneable {
	/// <summary>角色距离类别。</summary>
	public CharacterDistance characterDistance;
	/// <summary>该距离范围起始值。</summary>
	public int proximityRangeBegins = 0;
	/// <summary>该距离范围结束值。</summary>
	public int proximityRangeEnds = 100;

	/// <summary>前进概率。</summary>
	public float movingForwardProbability = .5f;
	/// <summary>后退概率。</summary>
	public float movingBackProbability = .5f;
	/// <summary>跳跃概率。</summary>
	public float jumpingProbability = .5f;
	/// <summary>下蹲概率。</summary>
	public float crouchProbability = .5f;
	/// <summary>攻击概率。</summary>
	public float attackProbability = .5f;
	
	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 网络用户数据：联机大厅中玩家的自定义数据变量定义。
/// </summary>
[System.Serializable]
public class NetworkUserData: ICloneable {
	/// <summary>变量名称。</summary>
    public string variableName;
	/// <summary>变量数据类型。</summary>
    public ServerVariableType variableType;
	/// <summary>浮点数值。</summary>
    public float floatValue;
	/// <summary>整数值。</summary>
    public int intValue;
	/// <summary>字符串值。</summary>
    public string stringValue;
	/// <summary>布尔值。</summary>
    public bool boolValue;
	/// <summary>变量更新类型。</summary>
    public ServerVariableUpdateType variableUpdateType;
	/// <summary>匹配过滤类型。</summary>
    public MatchMakingFilterType matchMakingFilterType;

	/// <summary>UFE 布尔值表示。</summary>
    public UFEBoolean ufeBoolean;

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
        return CloneObject.Clone(this);
    }
}

/// <summary>
/// 大厅选项：联机大厅（房间列表）的配置。
/// </summary>
[System.Serializable]
public class LobbyOptions: ICloneable {
	/// <summary>大厅名称。</summary>
    public string lobbyName;
	/// <summary>房间创建方式。</summary>
    public LobbyMatchCreationSystem matchMakingType;
	/// <summary>该大厅对应的游戏模式。</summary>
    public GameMode gameMode;
	/// <summary>是否允许私密房间。</summary>
    public bool allowPrivateRooms;
	/// <summary>编辑器用：匹配面板开关。</summary>
    public bool matchMakingToggle;
	/// <summary>编辑器用：胜利者面板开关。</summary>
    public bool winnerToggle;
	/// <summary>编辑器用：失败者面板开关。</summary>
    public bool loserToggle;
	/// <summary>匹配时使用的用户数据变量列表。</summary>
    public NetworkUserData[] matchMakingUserData = new NetworkUserData[0];
	/// <summary>胜利者更新/匹配的用户数据变量列表。</summary>
    public NetworkUserData[] winnerUserData = new NetworkUserData[0];
	/// <summary>失败者更新/匹配的用户数据变量列表。</summary>
    public NetworkUserData[] loserUserData = new NetworkUserData[0];

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
        return CloneObject.Clone(this);
    }
}

/// <summary>
/// 网络选项：联机对战与帧同步（Netcode）的完整配置。
/// </summary>
[System.Serializable]
public class NetworkOptions {
    //general options
	/// <summary>是否强制动画控制（网络回放时）。</summary>
    public bool forceAnimationControl;
	/// <summary>是否禁用根骨骼运动。</summary>
    public bool disableRootMotion;
	/// <summary>是否禁用动画融合。</summary>
    public bool disableBlending;
	/// <summary>是否禁用旋转融合。</summary>
    public bool disableRotationBlend;
	/// <summary>离线模式是否也应用帧延迟（用于测试）。</summary>
    public bool applyFrameDelayOffline;
	/// <summary>发生反同步时是否断开连接。</summary>
    public bool disconnectOnDesynchronization;
	/// <summary>是否启用反同步恢复。</summary>
    public bool desynchronizationRecovery;
	/// <summary>允许的反同步次数。</summary>
	public int allowedDesynchronizations;
	/// <summary>服务器端口。</summary>
	public int port = 1337;
	/// <summary>反同步浮点阈值。</summary>
	public float floatDesynchronizationThreshold = 0.5f;

    //online service
	/// <summary>网络服务类型。</summary>
    public NetworkService networkService;
	/// <summary>认证密钥。</summary>
    public string authKey;
	/// <summary>大厅列表。</summary>
    public LobbyOptions[] lobbies = new LobbyOptions[] { new LobbyOptions() };

	//Photon options
	/// <summary>Photon 托管方式。</summary>
	public PhotonHostingService photonHostingService = PhotonHostingService.PhotonServer;
	/// <summary>PlayFab 标题 ID。</summary>
	public string playFabTitleId; 
	/// <summary>Photon 应用 ID。</summary>
	public string photonApplicationId;

	// LAN Game Discovery
	/// <summary>LAN 游戏发现端口。</summary>
	public int lanDiscoveryPort = 1338;
	/// <summary>LAN 广播间隔（秒）。</summary>
	public float lanDiscoveryBroadcastInterval = 1f;
	/// <summary>LAN 搜索间隔（秒）。</summary>
	public float lanDiscoverySearchInterval = 5f;
	/// <summary>LAN 搜索超时（秒）。</summary>
	public float lanDiscoverySearchTimeout = 120f;

    //netcode
	/// <summary>是否允许回滚（Rollback）。</summary>
	public bool allowRollBacks = false;
	/// <summary>回滚平衡策略。</summary>
	public NetworkRollbackBalancing rollbackBalancing = NetworkRollbackBalancing.Conservative;
	/// <summary>帧延迟类型。</summary>
    public NetworkFrameDelay frameDelayType = NetworkFrameDelay.Auto;
	/// <summary>最小帧延迟。</summary>
	public int minFrameDelay = 4;
	/// <summary>最大帧延迟。</summary>
    public int maxFrameDelay = 30;
	/// <summary>默认帧延迟。</summary>
    public int defaultFrameDelay = 6;
	/// <summary>最大缓冲大小。</summary>
    public int maxBufferSize = 30;
	/// <summary>最大快进帧数。</summary>
	public int maxFastForwards = 10;
	/// <summary>生成缓冲。</summary>
	public int spawnBuffer = 30;
	/// <summary>是否使用 UFE 状态追踪器。</summary>
	public bool ufeTrackers = false;
	/// <summary>网络消息大小。</summary>
	public NetworkMessageSize networkMessageSize = NetworkMessageSize.Size32Bits;
	/// <summary>是否仅发送输入变化。</summary>
	public bool onlySendInputChanges = true;
	/// <summary>输入消息发送频率。</summary>
	public NetworkInputMessageFrequency inputMessageFrequency = NetworkInputMessageFrequency.EveryFrame;
	/// <summary>同步消息发送频率。</summary>
	public NetworkSynchronizationMessageFrequency synchronizationMessageFrequency = NetworkSynchronizationMessageFrequency.EverySecond;
}

namespace UFE3D
{
	/// <summary>
	/// 全局信息（GlobalInfo）：UFE 引擎的全局配置根类（ScriptableObject 资产）。
	/// <para>用途：聚合游戏名、角色、场地、GUI、回合规则、AI、网络、调试等全部全局配置，</para>
	/// <para>由 UFE.cs 在 Awake 时从 Config.asset 加载并驱动整个游戏。</para>
	/// </summary>
    [System.Serializable]
    public class GlobalInfo : ScriptableObject
    {

        #region public instance fields
		/// <summary>配置版本号。</summary>
        public float version;
		/// <summary>当前选中的语言选项。</summary>
        public LanguageOptions selectedLanguage;
		/// <summary>玩家1当前角色。</summary>
        public CharacterInfo player1Character;
		/// <summary>玩家2当前角色。</summary>
        public CharacterInfo player2Character;
		/// <summary>玩家1角色缓存（存储引用）。</summary>
        public CharacterInfo p1CharStorage;
		/// <summary>玩家2角色缓存（存储引用）。</summary>
        public CharacterInfo p2CharStorage;
		/// <summary>当前选中的场地。</summary>
        public StageOptions selectedStage;
		/// <summary>场地预制体存储模式。</summary>
        public StorageMode stagePrefabStorage = StorageMode.Legacy;
		/// <summary>场地音乐存储模式。</summary>
        public StorageMode stageMusicStorage = StorageMode.Legacy;
		/// <summary>玩家1是否由 CPU 控制。</summary>
        public bool p1CPUControl;
		/// <summary>玩家2是否由 CPU 控制。</summary>
        public bool p2CPUControl;
		/// <summary>游戏名称。</summary>
        public string gameName;


        //-----------------------------------------------------------------------------------------------------------------
		/// <summary>全部游戏界面（屏幕）预制体引用。</summary>
        public GameGUI gameGUI;
		/// <summary>故事模式配置。</summary>
        public StoryMode storyMode;
        //-----------------------------------------------------------------------------------------------------------------


        //public int fps = 60;
		/// <summary>目标帧率（代理到 UFE.fps）。</summary>
        public int fps { get { return UFE.fps; } set { UFE.fps = value; } }
		/// <summary>游戏全局速度倍率（float 值）。</summary>
        public float gameSpeed = 1;
		/// <summary>游戏全局速度倍率（定点数，运行时实际使用）。</summary>
        public Fix64 _gameSpeed = 1;
		/// <summary>出招缓冲时间（帧）。</summary>
        public int executionBufferTime = 10;
		/// <summary>出招缓冲类型。</summary>
        public ExecutionBufferType executionBufferType;
		/// <summary>双键连按（Plink）延迟（帧）。</summary>
        public int plinkingDelay = 1;

		/// <summary>预加载时间（float 值，秒）。</summary>
        public float preloadingTime = 1f;
		/// <summary>预加载时间（定点数，运行时实际使用）。</summary>
        public Fix64 _preloadingTime = 1;
		/// <summary>是否预加载玩家1角色。</summary>
        public bool preloadCharacter1 = true;
		/// <summary>是否预加载玩家2角色。</summary>
        public bool preloadCharacter2 = true;
		/// <summary>是否预加载场地。</summary>
        public bool preloadStage = true;
		/// <summary>是否预加载命中特效。</summary>
        public bool preloadHitEffects = true;
		/// <summary>是否预热所有着色器。</summary>
        public bool warmAllShaders = true;

		/// <summary>重力加速度（float 值）。</summary>
        public float gravity = .37f;
		/// <summary>重力加速度（定点数，运行时实际使用）。</summary>
        public Fix64 _gravity = .37;
		/// <summary>是否检测 3D 命中（Z 轴判定）。</summary>
        public bool detect3D_Hits;
		/// <summary>游戏是否在后台运行。</summary>
        public bool runInBackground;
		/// <summary>支持的语言列表。</summary>
        public LanguageOptions[] languages = new LanguageOptions[] { new LanguageOptions() };
		/// <summary>摄像机选项。</summary>
        public CameraOptions cameraOptions;
		/// <summary>角色旋转选项。</summary>
        public CharacterRotationOptions characterRotationOptions;
		/// <summary>回合选项。</summary>
        public RoundOptions roundOptions;
		/// <summary>地面弹跳选项。</summary>
        public BounceOptions groundBounceOptions;
		/// <summary>墙壁弹跳选项。</summary>
        public BounceOptions wallBounceOptions;
		/// <summary>反击选项。</summary>
        public CounterHitOptions counterHitOptions;
		/// <summary>连击选项。</summary>
        public ComboOptions comboOptions;
		/// <summary>格挡选项。</summary>
        public BlockOptions blockOptions;
		/// <summary>击倒选项。</summary>
        public KnockDownOptions knockDownOptions;
		/// <summary>命中效果选项。</summary>
        public HitOptions hitOptions;

		/// <summary>玩家1输入映射（Unity InputManager 到 UFE 按键）。</summary>
        public InputReferences[] player1_Inputs = new InputReferences[0]; // Reference to Unity's InputManager to UFE's keys
		/// <summary>玩家2输入映射（Unity InputManager 到 UFE 按键）。</summary>
        public InputReferences[] player2_Inputs = new InputReferences[0]; // Reference to Unity's InputManager to UFE's keys
		/// <summary>全局输入选项。</summary>
        public InputOptions inputOptions = new InputOptions();

		/// <summary>场地列表。</summary>
        public StageOptions[] stages = new StageOptions[0];
		/// <summary>角色列表。</summary>
        public CharacterInfo[] characters = new CharacterInfo[0];
		/// <summary>调试选项。</summary>
        public DebugOptions debugOptions = new DebugOptions();
		/// <summary>训练模式选项。</summary>
        public TrainingModeOptions trainingModeOptions = new TrainingModeOptions();
		/// <summary>挑战模式选项列表。</summary>
        public ChallengeModeOptions[] challengeModeOptions = new ChallengeModeOptions[0];
		/// <summary>AI 选项。</summary>
        public AIOptions aiOptions = new AIOptions();
		/// <summary>网络选项。</summary>
        public NetworkOptions networkOptions = new NetworkOptions();

		/// <summary>是否启用音乐。</summary>
        public bool music = true;
		/// <summary>音乐音量。</summary>
        public float musicVolume = 1f;
		/// <summary>是否启用音效。</summary>
        public bool soundfx = true;
		/// <summary>音效音量。</summary>
        public float soundfxVolume = 1f;
        #endregion


        #region trackable definitions
		/// <summary>当前回合数（运行时跟踪）。</summary>
        public int currentRound { get; set; }
		/// <summary>是否锁定玩家输入（运行时跟踪）。</summary>
        public bool lockInputs { get; set; }
		/// <summary>是否锁定角色移动（运行时跟踪）。</summary>
        public bool lockMovements { get; set; }
		/// <summary>当前选中的挑战索引（运行时跟踪）。</summary>
        public int selectedChallenge { get; set; }
        #endregion

        #region public instance methods
		/// <summary>
		/// 校验故事模式配置中的角色/场地索引是否有效。
		/// <para>清理无效索引，并为缺失故事的角色创建默认故事。</para>
		/// </summary>
        public virtual void ValidateStoryModeInformation()
        {
            // First, check that every character index in Story Mode is valid
            for (int i = this.storyMode.selectableCharactersInStoryMode.Count - 1; i >= 0; --i)
            {
                int character = this.storyMode.selectableCharactersInStoryMode[i];

                if (character < 0 || character >= this.characters.Length)
                {
                    this.storyMode.characterStories.Remove(character);
                    this.storyMode.selectableCharactersInStoryMode.RemoveAt(i);
                }
                else if (!this.storyMode.characterStories.ContainsKey(character))
                {
                    this.storyMode.characterStories[character] = new CharacterStory();
                }
            }

            // Then check that every character index in Versus Mode is valid
            for (int i = this.storyMode.selectableCharactersInVersusMode.Count - 1; i >= 0; --i)
            {
                int character = this.storyMode.selectableCharactersInVersusMode[i];
                if (character < 0 || character >= this.characters.Length)
                {
                    this.storyMode.selectableCharactersInVersusMode.RemoveAt(i);
                }
            }

            // Finally, check that every character and stage index are valid in the Character Stories
            this.ValidateCharacterStory(this.storyMode.defaultStory);
            foreach (CharacterStory story in this.storyMode.characterStories.Values)
            {
                this.ValidateCharacterStory(story);
            }
        }
        #endregion

        #region protected instance methods
		/// <summary>
		/// 校验单个角色故事中的对手与场地索引是否有效。
		/// </summary>
		/// <param name="story">要校验的角色故事配置。</param>
        protected virtual void ValidateCharacterStory(CharacterStory story)
        {
            if (story != null && story.fightsGroups != null)
            {
                foreach (FightsGroup group in story.fightsGroups)
                {
                    List<StoryModeBattle> battles = new List<StoryModeBattle>(group.opponents);

                    for (int i = battles.Count - 1; i >= 0; --i)
                    {
                        StoryModeBattle battle = battles[i];

                        if (battle.opponentCharacterIndex < 0 || battle.opponentCharacterIndex >= this.characters.Length)
                        {
                            battles.RemoveAt(i);
                        }
                        else
                        {
                            for (int j = battle.possibleStagesIndexes.Count - 1; j >= 0; --j)
                            {
                                int stageIndex = battle.possibleStagesIndexes[j];

                                if (stageIndex < 0 || stageIndex >= this.stages.Length)
                                {
                                    battle.possibleStagesIndexes.RemoveAt(j);
                                }
                            }

                            if (battle.possibleStagesIndexes.Count == 0 && this.stages.Length > 0)
                            {
                                battle.possibleStagesIndexes.Add(i % this.stages.Length);
                            }
                        }
                    }

                    group.opponents = battles.ToArray();
                }
            }
        }
        #endregion
    }
}

/// <summary>
/// 对象克隆工具（CloneObject）。
/// <para>用途：提供通用的对象深拷贝工具，支持反射克隆（ReflectionClone）与二进制序列化克隆（SerializedClone）两种方式。</para>
/// <para>UFE 各类配置对象（实现 ICloneable）通过该工具实现运行时克隆。</para>
/// </summary>
[System.Serializable]
public static class CloneObject{
	/// <summary>克隆结果缓存对象。</summary>
	public static object objCopy;
	
	/// <summary>
	/// 克隆目标对象（使用反射克隆）。
	/// </summary>
	/// <param name="target">要克隆的目标对象。</param>
	/// <returns>克隆出的新对象。</returns>
	public static object Clone(object target){
		return ReflectionClone(target);
	}
	
	/// <summary>
	/// 克隆目标对象，可选择序列化方式。
	/// </summary>
	/// <param name="target">要克隆的目标对象。</param>
	/// <param name="serialized">true 使用二进制序列化克隆；false 使用反射克隆。</param>
	/// <returns>克隆出的新对象。</returns>
	public static object Clone(object target, bool serialized){
		if (serialized) return SerializedClone(target);
		return ReflectionClone(target);
	}

	/// <summary>
	/// 使用二进制序列化克隆对象（要求类型可序列化）。
	/// </summary>
	/// <param name="target">要克隆的目标对象。</param>
	/// <returns>克隆出的新对象；目标为 null 时返回 null。</returns>
    public static object SerializedClone(object target){
		if (target == null) return null;

		using (Stream objectStream = new MemoryStream()) {
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(objectStream, target);
			objectStream.Seek(0, SeekOrigin.Begin);
			return (object)formatter.Deserialize(objectStream);
		}
	}

	/// <summary>
	/// 递归克隆对象数组（简单元素浅拷贝，复杂对象深度克隆）。
	/// </summary>
	/// <param name="target">要克隆的对象数组。</param>
	/// <returns>克隆出的新数组。</returns>
	public static object[] ReflectionCloneArray(object[] target){
		object[] arrayObj = (object[]) Array.CreateInstance(target.GetType().GetElementType(), target.Length);

		for (int i = 0; i < target.Length; i ++){
			
			if (target[i] == null 
			    || target[i].GetType().IsEnum
			    || target[i].GetType().IsValueType
			    || target[i].GetType().IsGenericType
			    || target[i].GetType().Equals(typeof(String)) 
			    || target[i].GetType().IsSubclassOf(typeof(ScriptableObject))){
				
				// If its a simple element, use shallow copy
				arrayObj[i] = target[i];
			}else{
				// If its a complex interface, go deeper into recursion
				arrayObj[i] = ReflectionClone(target[i]);
			}
		}

		return arrayObj;
	}

	/// <summary>
	/// 通过反射递归克隆对象（逐字段拷贝，简单字段浅拷贝，复杂对象深度克隆）。
	/// </summary>
	/// <param name="target">要克隆的目标对象。</param>
	/// <returns>克隆出的新对象。</returns>
	public static object ReflectionClone(object target){
		Type typeSource = target.GetType();

        // If its an array, identify and recurse each element
        if (typeSource.IsArray) return (object) ReflectionCloneArray((object[])target);

        object newObj = Activator.CreateInstance(typeSource);
		FieldInfo[] fields = typeSource.GetFields();

		foreach (FieldInfo field in fields){
			object fieldValue = field.GetValue(target);

			if (fieldValue == null 
			    || field.FieldType.IsEnum
			    || field.FieldType.IsValueType
			    || field.FieldType.Equals(typeof(String)) 
			    || field.FieldType.GetInterface("ICloneable", true ) == null
			    || field.FieldType.GetInterface("ScriptableObject", true ) != null
			    || field.FieldType.IsSubclassOf(typeof(ScriptableObject))){
				// If its a simple element, use shallow copy
				field.SetValue(newObj, fieldValue);
			}else{
				// If its a complex interface, go deeper into recursion
				field.SetValue(newObj, ReflectionClone(fieldValue));

			}
		}
		return newObj;
	}

	/// <summary>
	/// 克隆字典（浅拷贝：键值直接引用原对象）。
	/// </summary>
	/// <typeparam name="TKey">键类型。</typeparam>
	/// <typeparam name="TValue">值类型。</typeparam>
	/// <param name="original">原始字典。</param>
	/// <returns>克隆出的新字典；原始为 null 时返回 null。</returns>
    public static Dictionary<TKey, TValue> CloneDictionary<TKey, TValue>(Dictionary<TKey, TValue> original)
    {
        if (original == null) return null;
        Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer);
        foreach (KeyValuePair<TKey, TValue> entry in original)
        {
            ret.Add(entry.Key, (TValue)entry.Value);
        }
        return ret;
    }

	/// <summary>
	/// 克隆列表（浅拷贝）。
	/// </summary>
	/// <typeparam name="T">元素类型。</typeparam>
	/// <param name="original">原始列表。</param>
	/// <returns>克隆出的新列表。</returns>
    public static List<T> CloneList<T>(List<T> original)
    {
        List<T> ret = new List<T>(original.Count);
        foreach (T entry in original)
        {
            ret.Add(entry);
        }
        return ret;
    }


	/// <summary>
	/// 克隆非泛型 IList（浅拷贝）。
	/// </summary>
	/// <param name="original">原始列表。</param>
	/// <param name="T">目标列表类型。</param>
	/// <returns>克隆出的新列表实例。</returns>
    public static IList CloneList(IList original, Type T)
    {
        IList ret = Activator.CreateInstance(T) as IList;
        foreach (var entry in original)
        {
            ret.Add(entry);
        }
        return ret;
    }
}