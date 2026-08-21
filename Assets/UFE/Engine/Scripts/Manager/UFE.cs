#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#define UNITY_PRE_5_0
#endif

#if UNITY_PRE_5_0 || UNITY_5_0
#define UNITY_PRE_5_1
#endif

#if UNITY_PRE_5_1 || UNITY_5_1
#define UNITY_PRE_5_2
#endif

#if UNITY_PRE_5_2 || UNITY_5_2
#define UNITY_PRE_5_3
#endif

#if UNITY_PRE_5_3 || UNITY_5_3
#define UNITY_PRE_5_4
#endif


using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using FPLibrary;
using UFENetcode;
using UFE3D;

/// <summary>
/// UFE 引擎核心管理类（UFE）。
/// <para>用途：作为整个格斗游戏的中枢——持有全局配置（config）、管理输入控制器/摄像机/帧同步（FluxCapacitor）、</para>
/// <para>驱动全部 UI 屏幕切换、触发游戏事件（OnHit/OnMove/OnRoundEnds 等）、提供音频/AI/网络/角色解锁等静态 API。</para>
/// <para>该单例（MonoBehaviour）在游戏启动时被实例化，所有静态 API 均可从任何脚本调用。</para>
/// </summary>
[RequireComponent(typeof(EventSystem))]
public class UFE : MonoBehaviour, UFEInterface
{
	#region public instance enum
	/// <summary>
	/// 多人游戏模式：联网对战的方式。
	/// </summary>
	public enum MultiplayerMode
	{
		/// <summary>局域网对战。</summary>
		Lan,
		/// <summary>在线对战。</summary>
		Online,
		/// <summary>蓝牙对战。</summary>
		Bluetooth,
	}
	#endregion

	#region public instance properties
	/// <summary>
	/// UFE 全局配置资产引用（在 Inspector 中指定，Awake 时赋给静态 config）。
	/// </summary>
	public GlobalInfo UFE_Config;
	#endregion

	#region public event definitions
	/// <summary>生命值变化事件委托。</summary>
	public delegate void MeterHandler(float newFloat, UFE3D.CharacterInfo player);
	/// <summary>生命值变化事件（参数：新生命值，角色）。</summary>
	public static event MeterHandler OnLifePointsChange;

	/// <summary>回合开始事件委托。</summary>
	public delegate void IntHandler(int newInt);
	/// <summary>回合开始事件（参数：回合编号）。</summary>
	public static event IntHandler OnRoundBegins;

	/// <summary>文字提示事件委托。</summary>
	public delegate void StringHandler(string newString, UFE3D.CharacterInfo player);
	/// <summary>新提示文字事件（参数：提示内容，角色）。</summary>
	public static event StringHandler OnNewAlert;

	/// <summary>命中事件委托（打击判定盒/招式/角色）。</summary>
	public delegate void HitHandler(HitBox strokeHitBox, MoveInfo move, UFE3D.CharacterInfo player);
	/// <summary>命中事件。</summary>
	public static event HitHandler OnHit;
	/// <summary>格挡事件。</summary>
	public static event HitHandler OnBlock;
	/// <summary>弹反事件。</summary>
	public static event HitHandler OnParry;

	/// <summary>出招事件委托。</summary>
	public delegate void MoveHandler(MoveInfo move, UFE3D.CharacterInfo player);
	/// <summary>角色成功使出招式事件。</summary>
	public static event MoveHandler OnMove;

	/// <summary>按键事件委托。</summary>
	public delegate void ButtonHandler(ButtonPress button, UFE3D.CharacterInfo player);
	/// <summary>角色按下按钮事件。</summary>
	public static event ButtonHandler OnButton;

	/// <summary>基础动作事件委托。</summary>
	public delegate void BasicMoveHandler(BasicMoveReference basicMove, UFE3D.CharacterInfo player);
	/// <summary>角色执行基础动作事件。</summary>
	public static event BasicMoveHandler OnBasicMove;

	/// <summary>身体部位可见性变化事件委托。</summary>
	public delegate void BodyVisibilityHandler(MoveInfo move, UFE3D.CharacterInfo player, BodyPartVisibilityChange bodyPartVisibilityChange, HitBox hitBox);
	/// <summary>身体部位可见性变化事件。</summary>
	public static event BodyVisibilityHandler OnBodyVisibilityChange;

	/// <summary>粒子特效事件委托。</summary>
	public delegate void ParticleEffectsHandler(MoveInfo move, UFE3D.CharacterInfo player, MoveParticleEffect particleEffects);
	/// <summary>粒子特效触发事件。</summary>
	public static event ParticleEffectsHandler OnParticleEffects;

	/// <summary>换边（Side Switch）事件委托。</summary>
	public delegate void SideSwitchHandler(int side, UFE3D.CharacterInfo player);
	/// <summary>角色交换朝向事件。</summary>
	public static event SideSwitchHandler OnSideSwitch;

	/// <summary>游戏开始事件委托。</summary>
	public delegate void GameBeginHandler(UFE3D.CharacterInfo player1, UFE3D.CharacterInfo player2, StageOptions stage);
	/// <summary>整场游戏开始事件。</summary>
	public static event GameBeginHandler OnGameBegin;

	/// <summary>游戏/回合结束事件委托。</summary>
	public delegate void GameEndsHandler(UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser);
	/// <summary>整场游戏结束事件。</summary>
	public static event GameEndsHandler OnGameEnds;
	/// <summary>单回合结束事件。</summary>
	public static event GameEndsHandler OnRoundEnds;

	/// <summary>游戏暂停事件委托。</summary>
	public delegate void GamePausedHandler(bool isPaused);
	/// <summary>游戏暂停/恢复事件。</summary>
	public static event GamePausedHandler OnGamePaused;

	/// <summary>界面切换事件委托。</summary>
	public delegate void ScreenChangedHandler(UFEScreen previousScreen, UFEScreen newScreen);
	/// <summary>界面切换事件。</summary>
	public static event ScreenChangedHandler OnScreenChanged;

	/// <summary>故事模式事件委托。</summary>
	public delegate void StoryModeHandler(UFE3D.CharacterInfo character);
	/// <summary>故事模式开始事件。</summary>
	public static event StoryModeHandler OnStoryModeStarted;
	/// <summary>故事模式完成事件。</summary>
	public static event StoryModeHandler OnStoryModeCompleted;

	/// <summary>计时器事件委托。</summary>
	public delegate void TimerHandler(Fix64 time);
	/// <summary>计时器更新事件。</summary>
	public static event TimerHandler OnTimer;

	/// <summary>时间到事件委托。</summary>
	public delegate void TimeOverHandler();
	/// <summary>回合时间结束事件。</summary>
	public static event TimeOverHandler OnTimeOver;

	/// <summary>输入事件委托。</summary>
	public delegate void InputHandler(InputReferences[] inputReferences, int player);
	/// <summary>输入更新事件。</summary>
	public static event InputHandler OnInput;
	#endregion

	#region network definitions
	//-----------------------------------------------------------------------------------------------------------------
	// Network
	//-----------------------------------------------------------------------------------------------------------------
	/// <summary>
	/// 当前可用的多人 API（根据多人模式返回蓝牙/局域网/在线 API）。
	/// </summary>
	public static MultiplayerAPI multiplayerAPI
	{
		get
		{
			if (UFE.multiplayerMode == MultiplayerMode.Bluetooth)
			{
				return UFE.bluetoothMultiplayerAPI;
			}
			else if (UFE.multiplayerMode == MultiplayerMode.Lan)
			{
				return UFE.lanMultiplayerAPI;
			}
			else
			{
				return UFE.onlineMultiplayerAPI;
			}
		}
	}

	/// <summary>
	/// 多人模式：设置时自动启用/禁用对应的 MultiplayerAPI 组件。
	/// </summary>
	public static MultiplayerMode multiplayerMode
	{
		get
		{
			return UFE._multiplayerMode;
		}
		set
		{
			UFE._multiplayerMode = value;

			if (value == MultiplayerMode.Bluetooth)
			{
				UFE.bluetoothMultiplayerAPI.enabled = true;
				UFE.lanMultiplayerAPI.enabled = false;
				UFE.onlineMultiplayerAPI.enabled = false;
			}
			else if (value == MultiplayerMode.Lan)
			{
				UFE.bluetoothMultiplayerAPI.enabled = false;
				UFE.lanMultiplayerAPI.enabled = true;
				UFE.onlineMultiplayerAPI.enabled = false;
			}
			else
			{
				UFE.bluetoothMultiplayerAPI.enabled = false;
				UFE.lanMultiplayerAPI.enabled = false;
				UFE.onlineMultiplayerAPI.enabled = true;
			}
		}
	}

	/// <summary>蓝牙多人 API 实例。</summary>
	private static MultiplayerAPI bluetoothMultiplayerAPI;
	/// <summary>局域网多人 API 实例。</summary>
	private static MultiplayerAPI lanMultiplayerAPI;
	/// <summary>在线多人 API 实例。</summary>
	private static MultiplayerAPI onlineMultiplayerAPI;

	/// <summary>当前多人模式的内部存储值（默认局域网）。</summary>
	private static MultiplayerMode _multiplayerMode = MultiplayerMode.Lan;
	#endregion

	#region gui definitions
	//-----------------------------------------------------------------------------------------------------------------
	// GUI
	//-----------------------------------------------------------------------------------------------------------------
	/// <summary>全局 UI Canvas（ScreenSpaceOverlay）。</summary>
	public static Canvas canvas { get; protected set; }
	/// <summary>全局 CanvasGroup（用于整屏淡入淡出）。</summary>
	public static CanvasGroup canvasGroup { get; protected set; }
	/// <summary>全局 EventSystem（UI 事件系统）。</summary>
	public static EventSystem eventSystem { get; protected set; }
	/// <summary>全局 GraphicRaycaster（UI 射线检测）。</summary>
	public static GraphicRaycaster graphicRaycaster { get; protected set; }
	/// <summary>全局 StandaloneInputModule（UI 输入模块）。</summary>
	public static StandaloneInputModule standaloneInputModule { get; protected set; }
	//-----------------------------------------------------------------------------------------------------------------
	/// <summary>PlayerPrefs 键：音乐是否启用。</summary>
	protected static readonly string MusicEnabledKey = "MusicEnabled";
	/// <summary>PlayerPrefs 键：音乐音量。</summary>
	protected static readonly string MusicVolumeKey = "MusicVolume";
	/// <summary>PlayerPrefs 键：音效是否启用。</summary>
	protected static readonly string SoundsEnabledKey = "SoundsEnabled";
	/// <summary>PlayerPrefs 键：音效音量。</summary>
	protected static readonly string SoundsVolumeKey = "SoundsVolume";
	/// <summary>PlayerPrefs 键：AI 难度级别。</summary>
	protected static readonly string DifficultyLevelKey = "DifficultyLevel";
	/// <summary>PlayerPrefs 键：调试模式。</summary>
	protected static readonly string DebugModeKey = "DebugMode";
	//-----------------------------------------------------------------------------------------------------------------

	/// <summary>当前战斗摄像机脚本。</summary>
	public static CameraScript cameraScript { get; set; }
	/// <summary>帧同步（Rollback Netcode）管理器。</summary>
	public static FluxCapacitor fluxCapacitor;
	/// <summary>当前游戏模式。</summary>
	public static GameMode gameMode = GameMode.None;
	/// <summary>全局配置（GlobalInfo 资产）。</summary>
	public static GlobalInfo config;
	/// <summary>UFE 单例实例。</summary>
	public static UFE UFEInstance;
	/// <summary>是否输出调试信息。</summary>
	public static bool debug = true;
	/// <summary>调试文本1（屏幕左上）。</summary>
	public static Text debugger1;
	/// <summary>调试文本2（屏幕右上）。</summary>
	public static Text debugger2;
	#endregion

	#region addons definitions
	/// <summary>是否安装 AI 插件（Fuzzy AI）。</summary>
	public static bool isAiAddonInstalled { get; set; }
	/// <summary>是否安装 cInput 插件。</summary>
	public static bool isCInputInstalled { get; set; }
	/// <summary>是否安装 Control Freak 插件。</summary>
	public static bool isControlFreakInstalled { get; set; }
	/// <summary>是否安装 Control Freak 1.x。</summary>
	public static bool isControlFreak1Installed { get; set; }
	/// <summary>是否安装 Control Freak 2。</summary>
	public static bool isControlFreak2Installed { get; set; }
	/// <summary>是否安装 Rewired 插件。</summary>
	public static bool isRewiredInstalled { get; set; }
	/// <summary>是否安装网络插件。</summary>
	public static bool isNetworkAddonInstalled { get; set; }
	/// <summary>是否安装 Photon 插件。</summary>
	public static bool isPhotonInstalled { get; set; }
	/// <summary>是否安装蓝牙插件。</summary>
	public static bool isBluetoothAddonInstalled { get; set; }
	/// <summary>Control Freak 虚拟摇杆预制体实例。</summary>
	public static GameObject controlFreakPrefab;
	/// <summary>Control Freak 2 桥接器实例。</summary>
	public static InputTouchControllerBridge touchControllerBridge;
	#endregion

	#region screen definitions
	/// <summary>当前显示的 UI 屏幕。</summary>
	public static UFEScreen currentScreen { get; protected set; }
	/// <summary>战斗 HUD 屏幕。</summary>
	public static UFEScreen battleGUI { get; protected set; }
	/// <summary>游戏引擎根对象（战斗场景容器）。</summary>
	public static GameObject gameEngine { get; protected set; }
	#endregion

	#region trackable definitions
	/// <summary>是否自由摄像机（调试/演出用）。</summary>
	public static bool freeCamera;
	/// <summary>是否冻结物理模拟。</summary>
	public static bool freezePhysics;
	/// <summary>是否已广播新回合开始。</summary>
	public static bool newRoundCasted;
	/// <summary>是否使用标准化摄像机。</summary>
	public static bool normalizedCam = true;
	/// <summary>是否暂停回合计时器。</summary>
	public static bool pauseTimer;
	/// <summary>当前回合剩余时间。</summary>
	public static Fix64 timer;
	/// <summary>游戏全局时间倍率（暂停时为 0）。</summary>
	public static Fix64 timeScale;
	/// <summary>玩家1的角色控制脚本。</summary>
	public static ControlsScript p1ControlsScript;
	/// <summary>玩家2的角色控制脚本。</summary>
	public static ControlsScript p2ControlsScript;
	/// <summary>本地延迟动作列表（仅本地执行）。</summary>
	public static List<DelayedAction> delayedLocalActions = new List<DelayedAction>();
	/// <summary>同步延迟动作列表（所有客户端同步执行）。</summary>
	public static List<DelayedAction> delayedSynchronizedActions = new List<DelayedAction>();
	/// <summary>帧同步实例化对象列表（用于回滚重建）。</summary>
	public static List<InstantiatedGameObject> instantiatedObjects = new List<InstantiatedGameObject>();
	#endregion

	#region story mode definitions
	//-----------------------------------------------------------------------------------------------------------------
	// Required for the Story Mode: if the player lost its previous battle, 
	// he needs to fight the same opponent again, not the next opponent.
	//-----------------------------------------------------------------------------------------------------------------
	/// <summary>故事模式运行时进度信息。</summary>
	private static StoryModeInfo storyMode = new StoryModeInfo();
	/// <summary>故事模式中已解锁的角色名称列表。</summary>
	private static List<string> unlockedCharactersInStoryMode = new List<string>();
	/// <summary>对战模式中已解锁的角色名称列表。</summary>
	private static List<string> unlockedCharactersInVersusMode = new List<string>();
	/// <summary>玩家1是否赢得上一场战斗（决定故事进度）。</summary>
	private static bool player1WonLastBattle;
	/// <summary>上一个使用的场地索引。</summary>
	private static int lastStageIndex;
	#endregion

	#region public definitions
	/// <summary>每帧时长（固定步长 × 时间倍率）。</summary>
	public static Fix64 fixedDeltaTime { get { return _fixedDeltaTime * timeScale; } set { _fixedDeltaTime = value; } }
	/// <summary>回合计时器整数秒。</summary>
	public static int intTimer;
	/// <summary>目标帧率。</summary>
	public static int fps = 60;
	/// <summary>当前帧号（帧同步时间轴）。</summary>
	public static long currentFrame { get; set; }
	/// <summary>游戏是否正在运行（战斗中）。</summary>
	public static bool gameRunning { get; protected set; }

	/// <summary>本地玩家控制器（网络对战用）。</summary>
	public static UFEController localPlayerController;
	/// <summary>远端玩家控制器（网络对战用）。</summary>
	public static UFEController remotePlayerController;
	#endregion

	#region private definitions
	/// <summary>固定步长的内部存储值。</summary>
	private static Fix64 _fixedDeltaTime;
	/// <summary>背景音乐音频源。</summary>
	private static AudioSource musicAudioSource;
	/// <summary>音效音频源。</summary>
	private static AudioSource soundsAudioSource;

	/// <summary>玩家1控制器（人类+AI）。</summary>
	private static UFEController p1Controller;
	/// <summary>玩家2控制器（人类+AI）。</summary>
	private static UFEController p2Controller;

	/// <summary>玩家1随机 AI。</summary>
	private static RandomAI p1RandomAI;
	/// <summary>玩家2随机 AI。</summary>
	private static RandomAI p2RandomAI;
	/// <summary>玩家1模糊 AI。</summary>
	private static AbstractInputController p1FuzzyAI;
	/// <summary>玩家2模糊 AI。</summary>
	private static AbstractInputController p2FuzzyAI;
	/// <summary>玩家1简单 AI。</summary>
	private static SimpleAI p1SimpleAI;
	/// <summary>玩家2简单 AI。</summary>
	private static SimpleAI p2SimpleAI;

	/// <summary>是否正在关闭游戏（退出阶段标志）。</summary>
	private static bool closing = false;
	/// <summary>是否正在断开网络连接。</summary>
	private static bool disconnecting = false;
	/// <summary>预加载内存缓存（避免重复实例化同一对象）。</summary>
	private static List<object> memoryDump = new List<object>();
	#endregion


	#region public class methods: Delay the execution of a method maintaining synchronization between clients
	/// <summary>
	/// 延迟指定秒数后执行本地动作（仅本地客户端执行）。
	/// </summary>
	/// <param name="action">要延迟执行的动作。</param>
	/// <param name="seconds">延迟秒数。</param>
	public static void DelayLocalAction(Action action, Fix64 seconds)
	{
		if (UFE.fixedDeltaTime > 0)
		{
			UFE.DelayLocalAction(action, (int)FPMath.Floor(seconds * config.fps));
		}
		else
		{
			UFE.DelayLocalAction(action, 1);
		}
	}

	/// <summary>
	/// 延迟指定帧数后执行本地动作。
	/// </summary>
	/// <param name="action">要延迟执行的动作。</param>
	/// <param name="steps">延迟执行的帧数。</param>
	public static void DelayLocalAction(Action action, int steps)
	{
		UFE.DelayLocalAction(new DelayedAction(action, steps));
	}

	/// <summary>
	/// 将已打包的延迟动作加入本地延迟队列。
	/// </summary>
	/// <param name="delayedAction">延迟动作对象。</param>
	public static void DelayLocalAction(DelayedAction delayedAction)
	{
		UFE.delayedLocalActions.Add(delayedAction);
	}

	/// <summary>
	/// 延迟指定秒数后执行同步动作（所有客户端同步执行）。
	/// </summary>
	/// <param name="action">要延迟执行的动作。</param>
	/// <param name="seconds">延迟秒数。</param>
	public static void DelaySynchronizedAction(Action action, Fix64 seconds)
	{
		if (UFE.fixedDeltaTime > 0)
		{
			UFE.DelaySynchronizedAction(action, (int)FPMath.Floor(seconds * config.fps));
		}
		else
		{
			UFE.DelaySynchronizedAction(action, 1);
		}
	}

	/// <summary>
	/// 延迟指定帧数后执行同步动作（所有客户端同步执行）。
	/// </summary>
	/// <param name="action">要延迟执行的动作。</param>
	/// <param name="steps">延迟执行的帧数。</param>
	public static void DelaySynchronizedAction(Action action, int steps)
	{
		UFE.DelaySynchronizedAction(new DelayedAction(action, steps));
	}

	/// <summary>
	/// 将已打包的延迟动作加入同步延迟队列。
	/// </summary>
	/// <param name="delayedAction">延迟动作对象。</param>
	public static void DelaySynchronizedAction(DelayedAction delayedAction)
	{
		UFE.delayedSynchronizedActions.Add(delayedAction);
	}


	/// <summary>
	/// 在同步延迟队列中查找指定动作。
	/// </summary>
	/// <param name="action">要查找的动作。</param>
	/// <returns>存在返回 true，否则返回 false。</returns>
	public static bool FindDelaySynchronizedAction(Action action)
	{
		foreach (DelayedAction delayedAction in UFE.delayedSynchronizedActions)
		{
			if (action == delayedAction.action) return true;
		}
		return false;
	}

	/// <summary>
	/// 查找并更新同步延迟动作的剩余帧数。
	/// </summary>
	/// <param name="action">要更新的动作。</param>
	/// <param name="seconds">新的延迟秒数。</param>
	/// <returns>找到并更新返回 true，否则返回 false。</returns>
	public static bool FindAndUpdateDelaySynchronizedAction(Action action, Fix64 seconds)
	{
		foreach (DelayedAction delayedAction in UFE.delayedSynchronizedActions)
		{
			if (action == delayedAction.action)
			{
				delayedAction.steps = (int)FPMath.Floor(seconds * config.fps);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// 查找并从同步延迟队列中移除指定动作。
	/// </summary>
	/// <param name="action">要移除的动作。</param>
	public static void FindAndRemoveDelaySynchronizedAction(Action action)
	{
		foreach (DelayedAction delayedAction in UFE.delayedSynchronizedActions)
		{
			if (action == delayedAction.action)
			{
				UFE.delayedSynchronizedActions.Remove(delayedAction);
				return;
			}
		}
	}

	/// <summary>
	/// 查找并从本地延迟队列中移除指定动作。
	/// </summary>
	/// <param name="action">要移除的动作。</param>
	public static void FindAndRemoveDelayLocalAction(Action action)
	{
		foreach (DelayedAction delayedAction in UFE.delayedLocalActions)
		{
			if (action == delayedAction.action)
			{
				UFE.delayedLocalActions.Remove(delayedAction);
				return;
			}
		}
	}

	/// <summary>
	/// 在帧同步系统中实例化游戏对象并注册到实例列表（保证回滚可重建）。
	/// </summary>
	/// <param name="gameObject">要实例化的预制体。</param>
	/// <param name="position">世界位置。</param>
	/// <param name="rotation">世界旋转。</param>
	/// <param name="destroyTimer">可选的销毁帧号（当前帧 + 延迟帧）。</param>
	/// <returns>实例化出的游戏对象；预制体为 null 时返回 null。</returns>
	public static GameObject SpawnGameObject(GameObject gameObject, Vector3 position, Quaternion rotation, long? destroyTimer = null)
	{
		if (gameObject == null) return null;

		GameObject goInstance = UnityEngine.Object.Instantiate(gameObject, position, rotation);
		goInstance.transform.SetParent(UFE.gameEngine.transform);
		MrFusion mrFusion = (MrFusion)goInstance.GetComponent(typeof(MrFusion));
		if (mrFusion == null) mrFusion = goInstance.AddComponent<MrFusion>();

		UFE.instantiatedObjects.Add(new InstantiatedGameObject(goInstance, mrFusion, UFE.currentFrame, UFE.currentFrame + destroyTimer));

		return goInstance;
	}

	/// <summary>
	/// 销毁帧同步实例化对象（设置其销毁帧号，由帧同步系统统一执行销毁）。
	/// </summary>
	/// <param name="gameObject">要销毁的游戏对象。</param>
	/// <param name="destroyTimer">可选的销毁帧号；为 null 时立即在当前帧销毁。</param>
	public static void DestroyGameObject(GameObject gameObject, long? destroyTimer = null)
	{
		for (int i = 0; i < UFE.instantiatedObjects.Count; ++i)
		{
			if (UFE.instantiatedObjects[i].gameObject == gameObject)
			{
				UFE.instantiatedObjects[i].destructionFrame = destroyTimer == null ? UFE.currentFrame : destroyTimer;
				break;
			}
		}
	}

	#endregion

	#region public class methods: Audio related methods
	/// <summary>
	/// 获取是否启用音乐。
	/// </summary>
	/// <returns>true 表示音乐启用。</returns>
	public static bool GetMusic()
	{
		return config.music;
	}

	/// <summary>
	/// 获取当前播放的音乐片段。
	/// </summary>
	/// <returns>当前音乐 AudioClip。</returns>
	public static AudioClip GetMusicClip()
	{
		return UFE.musicAudioSource.clip;
	}

	/// <summary>
	/// 获取是否启用音效。
	/// </summary>
	/// <returns>true 表示音效启用。</returns>
	public static bool GetSoundFX()
	{
		return config.soundfx;
	}

	/// <summary>
	/// 获取音乐音量（配置不存在时返回 1）。
	/// </summary>
	/// <returns>音乐音量值。</returns>
	public static float GetMusicVolume()
	{
		if (UFE.config != null) return config.musicVolume;
		return 1f;
	}

	/// <summary>
	/// 获取音效音量（配置不存在时返回 1）。
	/// </summary>
	/// <returns>音效音量值。</returns>
	public static float GetSoundFXVolume()
	{
		if (UFE.config != null) return UFE.config.soundfxVolume;
		return 1f;
	}

	/// <summary>
	/// 初始化音频系统：在主摄像机上创建音乐与音效两个 AudioSource。
	/// </summary>
	public static void InitializeAudioSystem()
	{
		Camera cam = Camera.main;

		// Create the AudioSources required for the music and sound effects
		UFE.musicAudioSource = cam.GetComponent<AudioSource>();
		if (UFE.musicAudioSource == null)
		{
			UFE.musicAudioSource = cam.gameObject.AddComponent<AudioSource>();
		}

		UFE.musicAudioSource.loop = true;
		UFE.musicAudioSource.playOnAwake = false;
		UFE.musicAudioSource.volume = config.musicVolume;


		UFE.soundsAudioSource = cam.gameObject.AddComponent<AudioSource>();
		UFE.soundsAudioSource.loop = false;
		UFE.soundsAudioSource.playOnAwake = false;
		UFE.soundsAudioSource.volume = 1f;
	}

	/// <summary>
	/// 音乐当前是否正在播放。
	/// </summary>
	/// <returns>正在播放返回 true，否则返回 false。</returns>
	public static bool IsPlayingMusic()
	{
		if (UFE.musicAudioSource.clip != null) return UFE.musicAudioSource.isPlaying;
		return false;
	}

	/// <summary>
	/// 音乐是否循环播放。
	/// </summary>
	/// <returns>循环返回 true，否则返回 false。</returns>
	public static bool IsMusicLooped()
	{
		return UFE.musicAudioSource.loop;
	}

	/// <summary>
	/// 音效是否正在播放（当前固定返回 false，未实现）。
	/// </summary>
	/// <returns>始终返回 false。</returns>
	public static bool IsPlayingSoundFX()
	{
		return false;
	}

	/// <summary>
	/// 设置音乐是否循环播放。
	/// </summary>
	/// <param name="loop">是否循环。</param>
	public static void LoopMusic(bool loop)
	{
		UFE.musicAudioSource.loop = loop;
	}

	/// <summary>
	/// 播放当前设置的音乐（若已启用且未在播放且有片段）。
	/// </summary>
	public static void PlayMusic()
	{
		if (config.music && !UFE.IsPlayingMusic() && UFE.musicAudioSource.clip != null)
		{
			UFE.musicAudioSource.Play();
		}
	}

	/// <summary>
	/// 切换并播放指定音乐。
	/// </summary>
	/// <param name="music">要播放的音乐片段。</param>
	public static void PlayMusic(AudioClip music)
	{
		if (music != null)
		{
			AudioClip oldMusic = UFE.GetMusicClip();

			if (music != oldMusic)
			{
				UFE.musicAudioSource.clip = music;
			}

			if (config.music && (music != oldMusic || !UFE.IsPlayingMusic()))
			{
				UFE.musicAudioSource.Play();
			}
		}
	}

	/// <summary>
	/// 从音效列表中随机播放一个音效。
	/// </summary>
	/// <param name="sounds">音效列表。</param>
	public static void PlaySound(IList<AudioClip> sounds)
	{
		if (sounds.Count > 0)
		{
			UFE.PlaySound(sounds[UnityEngine.Random.Range(0, sounds.Count)]);
		}
	}

	/// <summary>
	/// 按默认音量播放音效。
	/// </summary>
	/// <param name="soundFX">要播放的音效。</param>
	public static void PlaySound(AudioClip soundFX)
	{
		UFE.PlaySound(soundFX, UFE.GetSoundFXVolume());
	}

	/// <summary>
	/// 按指定音量播放音效（一次性播放）。
	/// </summary>
	/// <param name="soundFX">要播放的音效。</param>
	/// <param name="volume">播放音量。</param>
	public static void PlaySound(AudioClip soundFX, float volume)
	{
		if (config.soundfx && soundFX != null && UFE.soundsAudioSource != null)
		{
			UFE.soundsAudioSource.PlayOneShot(soundFX, volume);
		}
	}

	/// <summary>
	/// 启用/禁用音乐并持久化到 PlayerPrefs。
	/// </summary>
	/// <param name="on">是否启用。</param>
	public static void SetMusic(bool on)
	{
		bool isPlayingMusic = UFE.IsPlayingMusic();
		UFE.config.music = on;

		if (on && !isPlayingMusic) UFE.PlayMusic();
		else if (!on && isPlayingMusic) UFE.StopMusic();

		PlayerPrefs.SetInt(UFE.MusicEnabledKey, on ? 1 : 0);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// 启用/禁用音效并持久化到 PlayerPrefs。
	/// </summary>
	/// <param name="on">是否启用。</param>
	public static void SetSoundFX(bool on)
	{
		UFE.config.soundfx = on;
		PlayerPrefs.SetInt(UFE.SoundsEnabledKey, on ? 1 : 0);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// 设置音乐音量并持久化到 PlayerPrefs。
	/// </summary>
	/// <param name="volume">音量值（0~1）。</param>
	public static void SetMusicVolume(float volume)
	{
		if (UFE.config != null) UFE.config.musicVolume = volume;
		if (UFE.musicAudioSource != null) UFE.musicAudioSource.volume = volume;

		PlayerPrefs.SetFloat(UFE.MusicVolumeKey, volume);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// 设置音效音量并持久化到 PlayerPrefs。
	/// </summary>
	/// <param name="volume">音量值（0~1）。</param>
	public static void SetSoundFXVolume(float volume)
	{
		if (UFE.config != null) UFE.config.soundfxVolume = volume;
		PlayerPrefs.SetFloat(UFE.SoundsVolumeKey, volume);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// 停止播放音乐。
	/// </summary>
	public static void StopMusic()
	{
		if (UFE.musicAudioSource.clip != null) UFE.musicAudioSource.Stop();
	}

	/// <summary>
	/// 停止播放全部音效。
	/// </summary>
	public static void StopSounds()
	{
		UFE.soundsAudioSource.Stop();
	}
	#endregion

	#region public class methods: AI related methods
	/// <summary>
	/// 设置 AI 引擎类型（随机 AI / 模糊 AI）。
	/// </summary>
	/// <param name="engine">AI 引擎类型。</param>
	public static void SetAIEngine(AIEngine engine)
	{
		UFE.config.aiOptions.engine = engine;
	}

	/// <summary>
	/// 获取当前 AI 引擎类型。
	/// </summary>
	/// <returns>AI 引擎类型。</returns>
	public static AIEngine GetAIEngine()
	{
		return UFE.config.aiOptions.engine;
	}

	/// <summary>
	/// 获取指定序号的挑战配置。
	/// </summary>
	/// <param name="challengeNum">挑战索引。</param>
	/// <returns>挑战模式配置对象。</returns>
	public static ChallengeModeOptions GetChallenge(int challengeNum)
	{
		return UFE.config.challengeModeOptions[challengeNum];
	}

	/// <summary>
	/// 设置调试模式开关（同时控制两个调试文本的显示）。
	/// </summary>
	/// <param name="flag">是否启用调试模式。</param>
	public static void SetDebugMode(bool flag)
	{
		UFE.config.debugOptions.debugMode = flag;
		if (debugger1 != null) debugger1.enabled = flag;
		if (debugger2 != null) debugger2.enabled = flag;
	}

	/// <summary>
	/// 按难度级别枚举设置 AI 难度。
	/// </summary>
	/// <param name="difficulty">难度级别。</param>
	public static void SetAIDifficulty(AIDifficultyLevel difficulty)
	{
		foreach (AIDifficultySettings difficultySettings in UFE.config.aiOptions.difficultySettings)
		{
			if (difficultySettings.difficultyLevel == difficulty)
			{
				UFE.SetAIDifficulty(difficultySettings);
				break;
			}
		}
	}

	/// <summary>
	/// 设置 AI 难度参数对象并持久化到 PlayerPrefs。
	/// </summary>
	/// <param name="difficulty">难度参数对象。</param>
	public static void SetAIDifficulty(AIDifficultySettings difficulty)
	{
		UFE.config.aiOptions.selectedDifficulty = difficulty;
		UFE.config.aiOptions.selectedDifficultyLevel = difficulty.difficultyLevel;

		for (int i = 0; i < UFE.config.aiOptions.difficultySettings.Length; ++i)
		{
			if (difficulty == UFE.config.aiOptions.difficultySettings[i])
			{
				PlayerPrefs.SetInt(UFE.DifficultyLevelKey, i);
				PlayerPrefs.Save();
				break;
			}
		}
	}

	/// <summary>
	/// 为指定玩家设置简单 AI（SimpleAI）行为。
	/// </summary>
	/// <param name="player">玩家编号（1 或 2）。</param>
	/// <param name="behaviour">简单 AI 行为资产。</param>
	public static void SetSimpleAI(int player, SimpleAIBehaviour behaviour)
	{
		if (player == 1)
		{
			UFE.p1SimpleAI.behaviour = behaviour;
			UFE.p1Controller.cpuController = UFE.p1SimpleAI;
		}
		else if (player == 2)
		{
			UFE.p2SimpleAI.behaviour = behaviour;
			UFE.p2Controller.cpuController = UFE.p2SimpleAI;
		}
	}

	/// <summary>
	/// 为指定玩家设置随机 AI。
	/// </summary>
	/// <param name="player">玩家编号（1 或 2）。</param>
	public static void SetRandomAI(int player)
	{
		if (player == 1)
		{
			UFE.p1Controller.cpuController = UFE.p1RandomAI;
		}
		else if (player == 2)
		{
			UFE.p2Controller.cpuController = UFE.p2RandomAI;
		}
	}

	/// <summary>
	/// 为指定玩家设置模糊 AI（使用当前选中的难度）。
	/// </summary>
	/// <param name="player">玩家编号（1 或 2）。</param>
	/// <param name="character">角色信息（从中读取 AI 指令集）。</param>
	public static void SetFuzzyAI(int player, UFE3D.CharacterInfo character)
	{
		UFE.SetFuzzyAI(player, character, UFE.config.aiOptions.selectedDifficulty);
	}

	/// <summary>
	/// 为指定玩家设置模糊 AI，并按难度起始行为选择 AI 指令集。
	/// </summary>
	/// <param name="player">玩家编号（1 或 2）。</param>
	/// <param name="character">角色信息（从中读取 AI 指令集）。</param>
	/// <param name="difficulty">AI 难度设置（startupBehavior 决定选哪个指令集）。</param>
	public static void SetFuzzyAI(int player, UFE3D.CharacterInfo character, AIDifficultySettings difficulty)
	{
		if (UFE.isAiAddonInstalled)
		{
			if (player == 1)
			{
				UFE.p1Controller.cpuController = UFE.p1FuzzyAI;
			}
			else if (player == 2)
			{
				UFE.p2Controller.cpuController = UFE.p2FuzzyAI;
			}

			UFEController controller = UFE.GetController(player);
			if (controller != null && controller.isCPU)
			{
				AbstractInputController cpu = controller.cpuController;

				if (cpu != null)
				{
					MethodInfo method = cpu.GetType().GetMethod(
						"SetAIInformation",
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy,
						null,
						new Type[] { typeof(ScriptableObject) },
						null
					);

					if (method != null)
					{
						if (character != null && character.aiInstructionsSet != null && character.aiInstructionsSet.Length > 0)
						{
							if (difficulty.startupBehavior == AIBehavior.Any)
							{
								method.Invoke(cpu, new object[] { character.aiInstructionsSet[0].aiInfo });
							}
							else
							{
								ScriptableObject selectedAIInfo = character.aiInstructionsSet[0].aiInfo;
								foreach (AIInstructionsSet instructionSet in character.aiInstructionsSet)
								{
									if (instructionSet.behavior == difficulty.startupBehavior)
									{
										selectedAIInfo = instructionSet.aiInfo;
										break;
									}
								}
								method.Invoke(cpu, new object[] { selectedAIInfo });
							}
						}
						else
						{
							method.Invoke(cpu, new object[] { null });
						}
					}
				}
			}
		}
	}
	#endregion

	#region public class methods: Story Mode related methods
	/// <summary>
	/// 获取指定角色的故事配置；若未启用"所有角色共用故事"，则查找该角色的专属故事，否则返回默认故事。
	/// </summary>
	/// <param name="character">目标角色。</param>
	/// <returns>角色对应的 CharacterStory 配置。</returns>
	public static CharacterStory GetCharacterStory(UFE3D.CharacterInfo character)
	{
		if (!UFE.config.storyMode.useSameStoryForAllCharacters)
		{
			StoryMode storyMode = UFE.config.storyMode;

			for (int i = 0; i < UFE.config.characters.Length; ++i)
			{
				if (UFE.config.characters[i] == character && storyMode.selectableCharactersInStoryMode.Contains(i))
				{
					CharacterStory characterStory = null;

					if (storyMode.characterStories.TryGetValue(i, out characterStory) && characterStory != null)
					{
						return characterStory;
					}
				}
			}
		}

		return UFE.config.storyMode.defaultStory;
	}


	/// <summary>
	/// 获取当前 AI 难度设置。
	/// </summary>
	/// <returns>当前选中的 AIDifficultySettings。</returns>
	public static AIDifficultySettings GetAIDifficulty()
	{
		return UFE.config.aiOptions.selectedDifficulty;
	}
	#endregion

	#region public class methods: GUI Related methods
	/// <summary>
	/// 获取战斗 HUD 预制体。
	/// </summary>
	/// <returns>BattleGUI 预制体引用。</returns>
	public static BattleGUI GetBattleGUI()
	{
		return UFE.config.gameGUI.battleGUI;
	}

	/// <summary>
	/// 获取蓝牙对战界面预制体。
	/// </summary>
	/// <returns>BluetoothGameScreen 预制体引用。</returns>
	public static BluetoothGameScreen GetBluetoothGameScreen()
	{
		return UFE.config.gameGUI.bluetoothGameScreen;
	}

	/// <summary>
	/// 获取角色选择界面预制体。
	/// </summary>
	/// <returns>CharacterSelectionScreen 预制体引用。</returns>
	public static CharacterSelectionScreen GetCharacterSelectionScreen()
	{
		return UFE.config.gameGUI.characterSelectionScreen;
	}

	/// <summary>
	/// 获取连接丢失界面预制体。
	/// </summary>
	/// <returns>ConnectionLostScreen 预制体引用。</returns>
	public static ConnectionLostScreen GetConnectionLostScreen()
	{
		return UFE.config.gameGUI.connectionLostScreen;
	}

	/// <summary>
	/// 获取制作人员界面预制体。
	/// </summary>
	/// <returns>CreditsScreen 预制体引用。</returns>
	public static CreditsScreen GetCreditsScreen()
	{
		return UFE.config.gameGUI.creditsScreen;
	}

	/// <summary>
	/// 获取建房界面预制体。
	/// </summary>
	/// <returns>HostGameScreen 预制体引用。</returns>
	public static HostGameScreen GetHostGameScreen()
	{
		return UFE.config.gameGUI.hostGameScreen;
	}

	/// <summary>
	/// 获取片头界面预制体。
	/// </summary>
	/// <returns>IntroScreen 预制体引用。</returns>
	public static IntroScreen GetIntroScreen()
	{
		return UFE.config.gameGUI.introScreen;
	}

	/// <summary>
	/// 获取加入游戏界面预制体。
	/// </summary>
	/// <returns>JoinGameScreen 预制体引用。</returns>
	public static JoinGameScreen GetJoinGameScreen()
	{
		return UFE.config.gameGUI.joinGameScreen;
	}

	/// <summary>
	/// 获取战斗加载界面预制体。
	/// </summary>
	/// <returns>LoadingBattleScreen 预制体引用。</returns>
	public static LoadingBattleScreen GetLoadingBattleScreen()
	{
		return UFE.config.gameGUI.loadingBattleScreen;
	}

	/// <summary>
	/// 获取主菜单界面预制体。
	/// </summary>
	/// <returns>MainMenuScreen 预制体引用。</returns>
	public static MainMenuScreen GetMainMenuScreen()
	{
		return UFE.config.gameGUI.mainMenuScreen;
	}

	/// <summary>
	/// 获取网络游戏界面预制体。
	/// </summary>
	/// <returns>NetworkGameScreen 预制体引用。</returns>
	public static NetworkGameScreen GetNetworkGameScreen()
	{
		return UFE.config.gameGUI.networkGameScreen;
	}

	/// <summary>
	/// 获取选项界面预制体。
	/// </summary>
	/// <returns>OptionsScreen 预制体引用。</returns>
	public static OptionsScreen GetOptionsScreen()
	{
		return UFE.config.gameGUI.optionsScreen;
	}

	/// <summary>
	/// 获取场地选择界面预制体。
	/// </summary>
	/// <returns>StageSelectionScreen 预制体引用。</returns>
	public static StageSelectionScreen GetStageSelectionScreen()
	{
		return UFE.config.gameGUI.stageSelectionScreen;
	}

	/// <summary>
	/// 获取故事模式通关祝贺界面预制体。
	/// </summary>
	/// <returns>StoryModeScreen 预制体引用。</returns>
	public static StoryModeScreen GetStoryModeCongratulationsScreen()
	{
		return UFE.config.gameGUI.storyModeCongratulationsScreen;
	}

	/// <summary>
	/// 获取故事模式继续界面预制体。
	/// </summary>
	/// <returns>StoryModeContinueScreen 预制体引用。</returns>
	public static StoryModeContinueScreen GetStoryModeContinueScreen()
	{
		return UFE.config.gameGUI.storyModeContinueScreen;
	}

	/// <summary>
	/// 获取故事模式游戏结束界面预制体。
	/// </summary>
	/// <returns>StoryModeScreen 预制体引用。</returns>
	public static StoryModeScreen GetStoryModeGameOverScreen()
	{
		return UFE.config.gameGUI.storyModeGameOverScreen;
	}

	/// <summary>
	/// 获取对战结算界面预制体。
	/// </summary>
	/// <returns>VersusModeAfterBattleScreen 预制体引用。</returns>
	public static VersusModeAfterBattleScreen GetVersusModeAfterBattleScreen()
	{
		return UFE.config.gameGUI.versusModeAfterBattleScreen;
	}

	/// <summary>
	/// 获取对战模式界面预制体。
	/// </summary>
	/// <returns>VersusModeScreen 预制体引用。</returns>
	public static VersusModeScreen GetVersusModeScreen()
	{
		return UFE.config.gameGUI.versusModeScreen;
	}

	/// <summary>
	/// 隐藏并销毁指定 UI 屏幕；若非战斗状态下且存在游戏引擎，同时结束游戏。
	/// </summary>
	/// <param name="screen">要隐藏的屏幕。</param>
	public static void HideScreen(UFEScreen screen)
	{
		if (screen != null)
		{
			screen.OnHide();
			GameObject.Destroy(screen.gameObject);
			if (!gameRunning && gameEngine != null) UFE.EndGame();
		}
	}

	/// <summary>
	/// 实例化并显示指定 UI 屏幕，触发屏幕切换事件；支持为故事模式屏幕设置下一步动作回调。
	/// </summary>
	/// <param name="screen">要显示的屏幕预制体。</param>
	/// <param name="nextScreenAction">下一步动作回调（故事模式屏幕使用）。</param>
	public static void ShowScreen(UFEScreen screen, Action nextScreenAction = null)
	{
		if (screen != null)
		{
			if (UFE.OnScreenChanged != null)
			{
				UFE.OnScreenChanged(UFE.currentScreen, screen);
			}

			UFE.currentScreen = (UFEScreen)GameObject.Instantiate(screen);
			UFE.currentScreen.transform.SetParent(UFE.canvas != null ? UFE.canvas.transform : null, false);

			StoryModeScreen storyModeScreen = UFE.currentScreen as StoryModeScreen;
			if (storyModeScreen != null)
			{
				storyModeScreen.nextScreenAction = nextScreenAction;
			}

			UFE.currentScreen.OnShow();
		}
	}

	/// <summary>
	/// 退出游戏（编辑器下停止播放模式，发布版本退出应用）。
	/// </summary>
	public static void Quit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	/// <summary>
	/// 进入蓝牙对战界面（使用默认淡出时长）。
	/// </summary>
	public static void StartBluetoothGameScreen()
	{
		UFE.StartBluetoothGameScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入蓝牙对战界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartBluetoothGameScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartBluetoothGameScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartBluetoothGameScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入角色选择界面（使用默认淡出时长）。
	/// </summary>
	public static void StartCharacterSelectionScreen()
	{
		UFE.StartCharacterSelectionScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入角色选择界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartCharacterSelectionScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartCharacterSelectionScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartCharacterSelectionScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 开始 CPU vs CPU 对战（使用默认淡出时长）。
	/// </summary>
	public static void StartCpuVersusCpu()
	{
		UFE.StartCpuVersusCpu((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 开始 CPU vs CPU 对战：设置为对战模式，双方均 CPU 控制，进入角色选择。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartCpuVersusCpu(float fadeTime)
	{
		UFE.gameMode = GameMode.VersusMode;
		UFE.SetCPU(1, true);
		UFE.SetCPU(2, true);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	/// <summary>
	/// 若当前不在主菜单则进入连接丢失界面（使用默认淡出时长）。
	/// </summary>
	public static void StartConnectionLostScreenIfMainMenuNotLoaded()
	{
		UFE.StartConnectionLostScreenIfMainMenuNotLoaded((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 若当前不在主菜单则进入连接丢失界面。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartConnectionLostScreenIfMainMenuNotLoaded(float fadeTime)
	{
		if ((UFE.currentScreen as MainMenuScreen) == null)
		{
			UFE.StartConnectionLostScreen(fadeTime);
		}
	}

	/// <summary>
	/// 进入连接丢失界面（使用默认淡出时长）。
	/// </summary>
	public static void StartConnectionLostScreen()
	{
		UFE.StartConnectionLostScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入连接丢失界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartConnectionLostScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartConnectionLostScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartConnectionLostScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入制作人员界面（使用默认淡出时长）。
	/// </summary>
	public static void StartCreditsScreen()
	{
		UFE.StartCreditsScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入制作人员界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartCreditsScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartCreditsScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartCreditsScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 开始游戏战斗（使用默认淡出时长）。
	/// </summary>
	public static void StartGame()
	{
		UFE.StartGame((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 开始游戏战斗（带淡出动画后初始化战斗）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartGame(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.gameFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartGame(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartGame(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入建房界面（使用默认淡出时长）。
	/// </summary>
	public static void StartHostGameScreen()
	{
		UFE.StartHostGameScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入建房界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartHostGameScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartHostGameScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartHostGameScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入片头界面（使用默认淡出时长）。
	/// </summary>
	public static void StartIntroScreen()
	{
		UFE.StartIntroScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入片头界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartIntroScreen(float fadeTime)
	{
		if (UFE.currentScreen != null && UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartIntroScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartIntroScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入加入游戏界面（使用默认淡出时长）。
	/// </summary>
	public static void StartJoinGameScreen()
	{
		UFE.StartJoinGameScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入加入游戏界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartJoinGameScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartJoinGameScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartJoinGameScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入战斗加载界面（使用默认淡出时长）。
	/// </summary>
	public static void StartLoadingBattleScreen()
	{
		UFE.StartLoadingBattleScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入战斗加载界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartLoadingBattleScreen(float fadeTime)
	{
		if (UFE.currentScreen != null && UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartLoadingBattleScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartLoadingBattleScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入主菜单界面（使用默认淡出时长）。
	/// </summary>
	public static void StartMainMenuScreen()
	{
		UFE.StartMainMenuScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入主菜单界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartMainMenuScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartMainMenuScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartMainMenuScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入搜索匹配界面（使用默认淡出时长）。
	/// </summary>
	public static void StartSearchMatchScreen()
	{
		UFE.StartSearchMatchScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入搜索匹配界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartSearchMatchScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartSearchMatchScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartSearchMatchScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入网络游戏界面（使用默认淡出时长）。
	/// </summary>
	public static void StartNetworkGameScreen()
	{
		UFE.StartNetworkGameScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入网络游戏界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartNetworkGameScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartNetworkGameScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartNetworkGameScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入选项界面（使用默认淡出时长）。
	/// </summary>
	public static void StartOptionsScreen()
	{
		UFE.StartOptionsScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入选项界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartOptionsScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartOptionsScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartOptionsScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 开始玩家 vs 玩家对战（使用默认淡出时长）。
	/// </summary>
	public static void StartPlayerVersusPlayer()
	{
		UFE.StartPlayerVersusPlayer((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 开始玩家 vs 玩家对战：设置为对战模式，双方均人类控制，进入角色选择。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartPlayerVersusPlayer(float fadeTime)
	{
		UFE.gameMode = GameMode.VersusMode;
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, false);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	/// <summary>
	/// 开始玩家 vs CPU 对战（使用默认淡出时长）。
	/// </summary>
	public static void StartPlayerVersusCpu()
	{
		UFE.StartPlayerVersusCpu((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 开始玩家 vs CPU 对战：玩家1人类控制、玩家2 CPU 控制，进入角色选择。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartPlayerVersusCpu(float fadeTime)
	{
		UFE.gameMode = GameMode.VersusMode;
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, true);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	/// <summary>
	/// 初始化网络对战：设置本地/远端控制器、初始化帧同步，然后进入加载界面或角色选择。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	/// <param name="localPlayer">本地玩家编号（1 或 2）。</param>
	/// <param name="startImmediately">true 直接进入战斗加载界面，false 先进入角色选择。</param>
	public static void StartNetworkGame(float fadeTime, int localPlayer, bool startImmediately)
	{
		UFE.disconnecting = false;
		Application.runInBackground = true;

		UFE.localPlayerController.Initialize(UFE.p1Controller.inputReferences);
		UFE.localPlayerController.humanController = UFE.p1Controller.humanController;
		UFE.localPlayerController.cpuController = UFE.p1Controller.cpuController;
		UFE.remotePlayerController.Initialize(UFE.p2Controller.inputReferences);

		if (localPlayer == 1)
		{
			UFE.localPlayerController.player = 1;
			UFE.remotePlayerController.player = 2;
		}
		else
		{
			UFE.localPlayerController.player = 2;
			UFE.remotePlayerController.player = 1;
		}

		UFE.fluxCapacitor.Initialize();
		UFE.gameMode = GameMode.NetworkGame;
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, false);

		if (startImmediately)
		{
			UFE.StartLoadingBattleScreen(fadeTime);
			//UFE.StartGame();
		}
		else
		{
			UFE.StartCharacterSelectionScreen(fadeTime);
		}
	}

	/// <summary>
	/// 进入场地选择界面（使用默认淡出时长）。
	/// </summary>
	public static void StartStageSelectionScreen()
	{
		UFE.StartStageSelectionScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入场地选择界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartStageSelectionScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStageSelectionScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStageSelectionScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 开始故事模式（使用默认淡出时长）。
	/// </summary>
	public static void StartStoryMode()
	{
		UFE.StartStoryMode((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 开始故事模式：重置故事进度、玩家1为人类/玩家2为 CPU，进入角色选择。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartStoryMode(float fadeTime)
	{
		//-------------------------------------------------------------------------------------------------------------
		// Required for loading the first combat correctly.
		UFE.player1WonLastBattle = true;
		//-------------------------------------------------------------------------------------------------------------
		UFE.gameMode = GameMode.StoryMode;
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, true);
		UFE.storyMode.characterStory = null;
		UFE.storyMode.canFightAgainstHimself = UFE.config.storyMode.canCharactersFightAgainstThemselves;
		UFE.storyMode.currentGroup = -1;
		UFE.storyMode.currentBattle = -1;
		UFE.storyMode.currentBattleInformation = null;
		UFE.storyMode.defeatedOpponents.Clear();
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	/// <summary>
	/// 进入故事模式战斗（使用默认淡出时长）。
	/// </summary>
	public static void StartStoryModeBattle()
	{
		UFE.StartStoryModeBattle((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入故事模式战斗（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartStoryModeBattle(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeBattle(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeBattle(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入故事模式通关祝贺界面（使用默认淡出时长）。
	/// </summary>
	public static void StartStoryModeCongratulationsScreen()
	{
		UFE.StartStoryModeCongratulationsScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入故事模式通关祝贺界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartStoryModeCongratulationsScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeCongratulationsScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeCongratulationsScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入故事模式继续界面（使用默认淡出时长）。
	/// </summary>
	public static void StartStoryModeContinueScreen()
	{
		UFE.StartStoryModeContinueScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入故事模式继续界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartStoryModeContinueScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeContinueScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeContinueScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入故事模式战后对话界面（使用默认淡出时长）。
	/// </summary>
	/// <param name="conversationScreen">对话界面预制体。</param>
	public static void StartStoryModeConversationAfterBattleScreen(UFEScreen conversationScreen)
	{
		UFE.StartStoryModeConversationAfterBattleScreen(conversationScreen, (float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入故事模式战后对话界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="conversationScreen">对话界面预制体。</param>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartStoryModeConversationAfterBattleScreen(UFEScreen conversationScreen, float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeConversationAfterBattleScreen(conversationScreen, fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeConversationAfterBattleScreen(conversationScreen, fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入故事模式战前对话界面（使用默认淡出时长）。
	/// </summary>
	/// <param name="conversationScreen">对话界面预制体。</param>
	public static void StartStoryModeConversationBeforeBattleScreen(UFEScreen conversationScreen)
	{
		UFE.StartStoryModeConversationBeforeBattleScreen(conversationScreen, (float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入故事模式战前对话界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="conversationScreen">对话界面预制体。</param>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartStoryModeConversationBeforeBattleScreen(UFEScreen conversationScreen, float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeConversationBeforeBattleScreen(conversationScreen, fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeConversationBeforeBattleScreen(conversationScreen, fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入故事模式结尾演出界面（使用默认淡出时长）。
	/// </summary>
	public static void StartStoryModeEndingScreen()
	{
		UFE.StartStoryModeEndingScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入故事模式结尾演出界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartStoryModeEndingScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeEndingScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeEndingScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入故事模式游戏结束界面（使用默认淡出时长）。
	/// </summary>
	public static void StartStoryModeGameOverScreen()
	{
		UFE.StartStoryModeGameOverScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入故事模式游戏结束界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartStoryModeGameOverScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeGameOverScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeGameOverScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入故事模式开场演出界面（使用默认淡出时长）。
	/// </summary>
	public static void StartStoryModeOpeningScreen()
	{
		UFE.StartStoryModeOpeningScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入故事模式开场演出界面（先获取玩家1的故事配置，带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartStoryModeOpeningScreen(float fadeTime)
	{
		// First, retrieve the character story, so we can find the opening associated to this player
		UFE.storyMode.characterStory = UFE.GetCharacterStory(UFE.GetPlayer1());

		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeOpeningScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeOpeningScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 开始训练模式（使用默认淡出时长）。
	/// </summary>
	public static void StartTrainingMode()
	{
		UFE.StartTrainingMode((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 开始训练模式：设置为训练模式、双方人类控制，进入角色选择。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartTrainingMode(float fadeTime)
	{
		UFE.gameMode = GameMode.TrainingRoom;
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, false);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	/// <summary>
	/// 进入对战结算界面（无淡出动画）。
	/// </summary>
	public static void StartVersusModeAfterBattleScreen()
	{
		UFE.StartVersusModeAfterBattleScreen(0f);
	}

	/// <summary>
	/// 进入对战结算界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartVersusModeAfterBattleScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartVersusModeAfterBattleScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartVersusModeAfterBattleScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 进入对战模式选择界面（使用默认淡出时长）。
	/// </summary>
	public static void StartVersusModeScreen()
	{
		UFE.StartVersusModeScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 进入对战模式选择界面（带淡出动画后切换）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void StartVersusModeScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartVersusModeScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartVersusModeScreen(fadeTime / 2f);
		}
	}

	/// <summary>
	/// 标记故事模式战斗胜利（使用默认淡出时长）：记录已击败对手并进入战后对话。
	/// </summary>
	public static void WonStoryModeBattle()
	{
		UFE.WonStoryModeBattle((float)UFE.config.gameGUI.screenFadeDuration);
	}

	/// <summary>
	/// 标记故事模式战斗胜利：记录已击败对手并进入战后对话。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void WonStoryModeBattle(float fadeTime)
	{
		UFE.storyMode.defeatedOpponents.Add(UFE.storyMode.currentBattleInformation.opponentCharacterIndex);
		UFE.StartStoryModeConversationAfterBattleScreen(UFE.storyMode.currentBattleInformation.conversationAfterBattle, fadeTime);
	}
	#endregion

	#region public class methods: Language
	/// <summary>
	/// 设置默认语言：选中语言列表中标记为 defaultSelection 的语言。
	/// </summary>
	public static void SetLanguage()
	{
		foreach (LanguageOptions languageOption in config.languages)
		{
			if (languageOption.defaultSelection)
			{
				config.selectedLanguage = languageOption;
				return;
			}
		}
	}

	/// <summary>
	/// 按语言名称设置当前语言。
	/// </summary>
	/// <param name="language">目标语言名称。</param>
	public static void SetLanguage(string language)
	{
		foreach (LanguageOptions languageOption in config.languages)
		{
			if (language == languageOption.languageName)
			{
				config.selectedLanguage = languageOption;
				return;
			}
		}
	}
	#endregion

	#region public class methods: Input Related methods
	/// <summary>
	/// 获取指定玩家是否由 CPU 控制。
	/// </summary>
	/// <param name="player">玩家编号（1 或 2）。</param>
	/// <returns>true 表示 CPU 控制。</returns>
	public static bool GetCPU(int player)
	{
		UFEController controller = UFE.GetController(player);
		if (controller != null)
		{
			return controller.isCPU;
		}
		return false;
	}

	/// <summary>
	/// 在指定输入引用列表中查找映射到指定引擎按键的输入名。
	/// </summary>
	/// <param name="button">引擎按键（ButtonPress）。</param>
	/// <param name="inputReferences">输入引用列表。</param>
	/// <returns>匹配的输入名（inputButtonName）；未找到返回 null。</returns>
	public static string GetInputReference(ButtonPress button, InputReferences[] inputReferences)
	{
		foreach (InputReferences inputReference in inputReferences)
		{
			if (inputReference.engineRelatedButton == button) return inputReference.inputButtonName;
		}
		return null;
	}

	/// <summary>
	/// 在指定输入引用列表中查找指定输入类型的输入名。
	/// </summary>
	/// <param name="inputType">输入类型（水平轴/垂直轴/按钮）。</param>
	/// <param name="inputReferences">输入引用列表。</param>
	/// <returns>匹配的输入名（inputButtonName）；未找到返回 null。</returns>
	public static string GetInputReference(InputType inputType, InputReferences[] inputReferences)
	{
		foreach (InputReferences inputReference in inputReferences)
		{
			if (inputReference.inputType == inputType) return inputReference.inputButtonName;
		}
		return null;
	}

	/// <summary>
	/// 获取玩家1的控制器（网络对战时按服务器身份映射到本地/远端控制器）。
	/// </summary>
	/// <returns>玩家1的 UFEController。</returns>
	public static UFEController GetPlayer1Controller()
	{
		if (UFE.isNetworkAddonInstalled && UFE.isConnected)
		{
			if (UFE.multiplayerAPI.IsServer())
			{
				return UFE.localPlayerController;
			}
			else
			{
				return UFE.remotePlayerController;
			}
		}
		return UFE.p1Controller;
	}

	/// <summary>
	/// 获取玩家2的控制器（网络对战时按服务器身份映射到远端/本地控制器）。
	/// </summary>
	/// <returns>玩家2的 UFEController。</returns>
	public static UFEController GetPlayer2Controller()
	{
		if (UFE.isNetworkAddonInstalled && UFE.isConnected)
		{
			if (UFE.multiplayerAPI.IsServer())
			{
				return UFE.remotePlayerController;
			}
			else
			{
				return UFE.localPlayerController;
			}
		}
		return UFE.p2Controller;
	}

	/// <summary>
	/// 获取指定玩家的控制器。
	/// </summary>
	/// <param name="player">玩家编号（1 或 2）。</param>
	/// <returns>对应的 UFEController；无效编号返回 null。</returns>
	public static UFEController GetController(int player)
	{
		if (player == 1) return UFE.GetPlayer1Controller();
		else if (player == 2) return UFE.GetPlayer2Controller();
		else return null;
	}

	/// <summary>
	/// 获取本地玩家编号。
	/// </summary>
	/// <returns>1 或 2；无法确定返回 -1。</returns>
	public static int GetLocalPlayer()
	{
		if (UFE.localPlayerController == UFE.GetPlayer1Controller()) return 1;
		else if (UFE.localPlayerController == UFE.GetPlayer2Controller()) return 2;
		else return -1;
	}

	/// <summary>
	/// 获取远端玩家编号。
	/// </summary>
	/// <returns>1 或 2；无法确定返回 -1。</returns>
	public static int GetRemotePlayer()
	{
		if (UFE.remotePlayerController == UFE.GetPlayer1Controller()) return 1;
		else if (UFE.remotePlayerController == UFE.GetPlayer2Controller()) return 2;
		else return -1;
	}

	/// <summary>
	/// 为指定玩家的 AI 控制器设置 AI 信息（通过反射调用 SetAIInformation）。
	/// </summary>
	/// <param name="player">玩家编号（1 或 2）。</param>
	/// <param name="character">角色信息（读取 AI 指令集）。</param>
	public static void SetAI(int player, UFE3D.CharacterInfo character)
	{
		if (UFE.isAiAddonInstalled)
		{
			UFEController controller = UFE.GetController(player);

			if (controller != null && controller.isCPU)
			{
				AbstractInputController cpu = controller.cpuController;

				if (cpu != null)
				{
					MethodInfo method = cpu.GetType().GetMethod(
						"SetAIInformation",
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy,
						null,
						new Type[] { typeof(ScriptableObject) },
					null
					);

					if (method != null)
					{
						if (character != null && character.aiInstructionsSet != null && character.aiInstructionsSet.Length > 0)
						{
							method.Invoke(cpu, new object[] { character.aiInstructionsSet[0].aiInfo });
						}
						else
						{
							method.Invoke(cpu, new object[] { null });
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// 设置指定玩家是否由 CPU 控制。
	/// </summary>
	/// <param name="player">玩家编号（1 或 2）。</param>
	/// <param name="cpuToggle">是否 CPU 控制。</param>
	public static void SetCPU(int player, bool cpuToggle)
	{
		UFEController controller = UFE.GetController(player);
		if (controller != null)
		{
			controller.isCPU = cpuToggle;
		}
	}
	#endregion

	#region public class methods: methods related to the character selection
	/// <summary>
	/// 获取指定玩家的角色信息。
	/// </summary>
	/// <param name="player">玩家编号（1 或 2）。</param>
	/// <returns>角色信息；无效编号返回 null。</returns>
	public static UFE3D.CharacterInfo GetPlayer(int player)
	{
		if (player == 1)
		{
			return UFE.GetPlayer1();
		}
		else if (player == 2)
		{
			return UFE.GetPlayer2();
		}
		return null;
	}

	/// <summary>
	/// 获取玩家1当前角色。
	/// </summary>
	/// <returns>玩家1角色信息。</returns>
	public static UFE3D.CharacterInfo GetPlayer1()
	{
		return config.player1Character;
	}

	/// <summary>
	/// 获取玩家2当前角色。
	/// </summary>
	/// <returns>玩家2角色信息。</returns>
	public static UFE3D.CharacterInfo GetPlayer2()
	{
		return config.player2Character;
	}

	/// <summary>
	/// 获取故事模式可选角色列表（配置可选或已解锁）。
	/// </summary>
	/// <returns>可选角色数组。</returns>
	public static UFE3D.CharacterInfo[] GetStoryModeSelectableCharacters()
	{
		List<UFE3D.CharacterInfo> characters = new List<UFE3D.CharacterInfo>();

		for (int i = 0; i < UFE.config.characters.Length; ++i)
		{
			if (
				UFE.config.characters[i] != null
				&&
				(
					UFE.config.storyMode.selectableCharactersInStoryMode.Contains(i) ||
					UFE.unlockedCharactersInStoryMode.Contains(UFE.config.characters[i].characterName)
				)
			)
			{
				characters.Add(UFE.config.characters[i]);
			}
		}

		return characters.ToArray();
	}

	/// <summary>
	/// 获取训练模式可选角色列表（故事或对战模式可选/解锁的角色）。
	/// </summary>
	/// <returns>可选角色数组。</returns>
	public static UFE3D.CharacterInfo[] GetTrainingRoomSelectableCharacters()
	{
		List<UFE3D.CharacterInfo> characters = new List<UFE3D.CharacterInfo>();

		for (int i = 0; i < UFE.config.characters.Length; ++i)
		{
			// If the character is selectable on Story Mode or Versus Mode,
			// then the character should be selectable on Training Room...
			if (
				UFE.config.characters[i] != null
				&&
				(
					UFE.config.storyMode.selectableCharactersInStoryMode.Contains(i) ||
					UFE.config.storyMode.selectableCharactersInVersusMode.Contains(i) ||
					UFE.unlockedCharactersInStoryMode.Contains(UFE.config.characters[i].characterName) ||
					UFE.unlockedCharactersInVersusMode.Contains(UFE.config.characters[i].characterName)
				)
			)
			{
				characters.Add(UFE.config.characters[i]);
			}
		}

		return characters.ToArray();
	}

	/// <summary>
	/// 获取对战模式可选角色列表（配置可选或已解锁）。
	/// </summary>
	/// <returns>可选角色数组。</returns>
	public static UFE3D.CharacterInfo[] GetVersusModeSelectableCharacters()
	{
		List<UFE3D.CharacterInfo> characters = new List<UFE3D.CharacterInfo>();

		for (int i = 0; i < UFE.config.characters.Length; ++i)
		{
			if (
				UFE.config.characters[i] != null &&
				(
					UFE.config.storyMode.selectableCharactersInVersusMode.Contains(i) ||
					UFE.unlockedCharactersInVersusMode.Contains(UFE.config.characters[i].characterName)
				)
			)
			{
				characters.Add(UFE.config.characters[i]);
			}
		}

		return characters.ToArray();
	}

	/// <summary>
	/// 按玩家编号设置角色。
	/// </summary>
	/// <param name="player">玩家编号（1 或 2）。</param>
	/// <param name="info">要设置的角色信息。</param>
	public static void SetPlayer(int player, UFE3D.CharacterInfo info)
	{
		if (player == 1)
		{
			config.player1Character = info;
		}
		else if (player == 2)
		{
			config.player2Character = info;
		}
	}

	/// <summary>
	/// 设置玩家1角色。
	/// </summary>
	/// <param name="player1">角色信息。</param>
	public static void SetPlayer1(UFE3D.CharacterInfo player1)
	{
		config.player1Character = player1;
	}

	/// <summary>
	/// 设置玩家2角色。
	/// </summary>
	/// <param name="player2">角色信息。</param>
	public static void SetPlayer2(UFE3D.CharacterInfo player2)
	{
		config.player2Character = player2;
	}

	/// <summary>
	/// 从 PlayerPrefs 加载已解锁角色列表（故事模式 UCSM / 对战模式 UCVM）。
	/// </summary>
	public static void LoadUnlockedCharacters()
	{
		UFE.unlockedCharactersInStoryMode.Clear();
		string value = PlayerPrefs.GetString("UCSM", null);

		if (!string.IsNullOrEmpty(value))
		{
			string[] characters = value.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string character in characters)
			{
				unlockedCharactersInStoryMode.Add(character);
			}
		}


		UFE.unlockedCharactersInVersusMode.Clear();
		value = PlayerPrefs.GetString("UCVM", null);

		if (!string.IsNullOrEmpty(value))
		{
			string[] characters = value.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string character in characters)
			{
				unlockedCharactersInVersusMode.Add(character);
			}
		}
	}

	/// <summary>
	/// 将已解锁角色列表保存到 PlayerPrefs。
	/// </summary>
	public static void SaveUnlockedCharacters()
	{
		StringBuilder sb = new StringBuilder();
		foreach (string characterName in UFE.unlockedCharactersInStoryMode)
		{
			if (!string.IsNullOrEmpty(characterName))
			{
				if (sb.Length > 0)
				{
					sb.AppendLine();
				}
				sb.Append(characterName);
			}
		}
		PlayerPrefs.SetString("UCSM", sb.ToString());

		sb = new StringBuilder();
		foreach (string characterName in UFE.unlockedCharactersInVersusMode)
		{
			if (!string.IsNullOrEmpty(characterName))
			{
				if (sb.Length > 0)
				{
					sb.AppendLine();
				}
				sb.Append(characterName);
			}
		}
		PlayerPrefs.SetString("UCVM", sb.ToString());
		PlayerPrefs.Save();
	}

	/// <summary>
	/// 从故事模式已解锁列表中移除指定角色。
	/// </summary>
	/// <param name="character">角色信息。</param>
	public static void RemoveUnlockedCharacterInStoryMode(UFE3D.CharacterInfo character)
	{
		if (character != null && !string.IsNullOrEmpty(character.characterName))
		{
			UFE.unlockedCharactersInStoryMode.Remove(character.characterName);
		}

		UFE.SaveUnlockedCharacters();
	}

	/// <summary>
	/// 从对战模式已解锁列表中移除指定角色。
	/// </summary>
	/// <param name="character">角色信息。</param>
	public static void RemoveUnlockedCharacterInVersusMode(UFE3D.CharacterInfo character)
	{
		if (character != null && !string.IsNullOrEmpty(character.characterName))
		{
			UFE.unlockedCharactersInVersusMode.Remove(character.characterName);
		}

		UFE.SaveUnlockedCharacters();
	}

	/// <summary>
	/// 清空故事模式全部已解锁角色。
	/// </summary>
	public static void RemoveUnlockedCharactersInStoryMode()
	{
		UFE.unlockedCharactersInStoryMode.Clear();
		UFE.SaveUnlockedCharacters();
	}

	/// <summary>
	/// 清空对战模式全部已解锁角色。
	/// </summary>
	public static void RemoveUnlockedCharactersInVersusMode()
	{
		UFE.unlockedCharactersInVersusMode.Clear();
		UFE.SaveUnlockedCharacters();
	}

	/// <summary>
	/// 解锁故事模式角色。
	/// </summary>
	/// <param name="character">要解锁的角色。</param>
	public static void UnlockCharacterInStoryMode(UFE3D.CharacterInfo character)
	{
		if (
			character != null &&
			!string.IsNullOrEmpty(character.characterName) &&
			!UFE.unlockedCharactersInStoryMode.Contains(character.characterName)
		)
		{
			UFE.unlockedCharactersInStoryMode.Add(character.characterName);
		}

		UFE.SaveUnlockedCharacters();
	}

	/// <summary>
	/// 解锁对战模式角色。
	/// </summary>
	/// <param name="character">要解锁的角色。</param>
	public static void UnlockCharacterInVersusMode(UFE3D.CharacterInfo character)
	{
		if (
			character != null &&
			!string.IsNullOrEmpty(character.characterName) &&
			!UFE.unlockedCharactersInVersusMode.Contains(character.characterName)
		)
		{
			UFE.unlockedCharactersInVersusMode.Add(character.characterName);
		}

		UFE.SaveUnlockedCharacters();
	}
	#endregion

	#region public class methods: methods related to the stage selection
	/// <summary>
	/// 设置当前选中的场地。
	/// </summary>
	/// <param name="stage">场地选项。</param>
	public static void SetStage(StageOptions stage)
	{
		config.selectedStage = stage;
	}

	/// <summary>
	/// 按场地名称设置当前选中的场地。
	/// </summary>
	/// <param name="stageName">场地名称。</param>
	public static void SetStage(string stageName)
	{
		foreach (StageOptions stage in config.stages)
		{
			if (stageName == stage.stageName)
			{
				UFE.SetStage(stage);
				return;
			}
		}
	}

	/// <summary>
	/// 获取当前选中的场地。
	/// </summary>
	/// <returns>场地选项。</returns>
	public static StageOptions GetStage()
	{
		return config.selectedStage;
	}
	#endregion


	#region public class methods: methods related to the behaviour of the characters during the battle
	/// <summary>
	/// 获取指定玩家的角色控制脚本。
	/// </summary>
	/// <param name="player">玩家编号（1 或 2）。</param>
	/// <returns>对应的 ControlsScript；无效编号返回 null。</returns>
	public static ControlsScript GetControlsScript(int player)
	{
		if (player == 1)
		{
			return UFE.GetPlayer1ControlsScript();
		}
		else if (player == 2)
		{
			return UFE.GetPlayer2ControlsScript();
		}
		return null;
	}

	/// <summary>
	/// 获取玩家1的角色控制脚本。
	/// </summary>
	/// <returns>玩家1的 ControlsScript。</returns>
	public static ControlsScript GetPlayer1ControlsScript()
	{
		return p1ControlsScript;
	}

	/// <summary>
	/// 获取玩家2的角色控制脚本。
	/// </summary>
	/// <returns>玩家2的 ControlsScript。</returns>
	public static ControlsScript GetPlayer2ControlsScript()
	{
		return p2ControlsScript;
	}
	#endregion

	#region public class methods: methods that are used for raising events
	/// <summary>
	/// 触发生命值变化事件。
	/// </summary>
	/// <param name="newValue">新的生命值。</param>
	/// <param name="player">所属角色。</param>
	public static void SetLifePoints(Fix64 newValue, UFE3D.CharacterInfo player)
	{
		if (UFE.OnLifePointsChange != null) UFE.OnLifePointsChange((float)newValue, player);
	}

	/// <summary>
	/// 触发游戏内文字提示事件。
	/// </summary>
	/// <param name="alertMessage">提示文本。</param>
	/// <param name="player">所属角色（可为 null）。</param>
	public static void FireAlert(string alertMessage, UFE3D.CharacterInfo player)
	{
		if (UFE.OnNewAlert != null) UFE.OnNewAlert(alertMessage, player);
	}

	/// <summary>
	/// 触发命中事件。
	/// </summary>
	/// <param name="strokeHitBox">打击判定盒。</param>
	/// <param name="move">使用的招式。</param>
	/// <param name="player">攻击方角色。</param>
	public static void FireHit(HitBox strokeHitBox, MoveInfo move, UFE3D.CharacterInfo player)
	{
		if (UFE.OnHit != null) UFE.OnHit(strokeHitBox, move, player);
	}

	/// <summary>
	/// 触发格挡事件。
	/// </summary>
	/// <param name="strokeHitBox">打击判定盒。</param>
	/// <param name="move">使用的招式。</param>
	/// <param name="player">攻击方角色。</param>
	public static void FireBlock(HitBox strokeHitBox, MoveInfo move, UFE3D.CharacterInfo player)
	{
		if (UFE.OnBlock != null) UFE.OnBlock(strokeHitBox, move, player);
	}

	/// <summary>
	/// 触发弹反事件。
	/// </summary>
	/// <param name="strokeHitBox">打击判定盒。</param>
	/// <param name="move">使用的招式。</param>
	/// <param name="player">攻击方角色。</param>
	public static void FireParry(HitBox strokeHitBox, MoveInfo move, UFE3D.CharacterInfo player)
	{
		if (UFE.OnParry != null) UFE.OnParry(strokeHitBox, move, player);
	}

	/// <summary>
	/// 触发出招事件。
	/// </summary>
	/// <param name="move">使出的招式。</param>
	/// <param name="player">使出角色。</param>
	public static void FireMove(MoveInfo move, UFE3D.CharacterInfo player)
	{
		if (UFE.OnMove != null) UFE.OnMove(move, player);
	}

	/// <summary>
	/// 触发按键事件。
	/// </summary>
	/// <param name="button">按下的按钮。</param>
	/// <param name="player">按下角色。</param>
	public static void FireButton(ButtonPress button, UFE3D.CharacterInfo player)
	{
		if (UFE.OnButton != null) UFE.OnButton(button, player);
	}

	/// <summary>
	/// 触发基础动作事件。
	/// </summary>
	/// <param name="basicMove">执行的基础动作。</param>
	/// <param name="player">执行角色。</param>
	public static void FireBasicMove(BasicMoveReference basicMove, UFE3D.CharacterInfo player)
	{
		if (UFE.OnBasicMove != null) UFE.OnBasicMove(basicMove, player);
	}

	/// <summary>
	/// 触发身体部位可见性变化事件。
	/// </summary>
	/// <param name="move">当前招式。</param>
	/// <param name="player">所属角色。</param>
	/// <param name="bodyPartVisibilityChange">可见性变化数据。</param>
	/// <param name="hitBox">关联的判定盒。</param>
	public static void FireBodyVisibilityChange(MoveInfo move, UFE3D.CharacterInfo player, BodyPartVisibilityChange bodyPartVisibilityChange, HitBox hitBox)
	{
		if (UFE.OnBodyVisibilityChange != null) UFE.OnBodyVisibilityChange(move, player, bodyPartVisibilityChange, hitBox);
	}

	/// <summary>
	/// 触发粒子特效事件。
	/// </summary>
	/// <param name="move">当前招式。</param>
	/// <param name="player">所属角色。</param>
	/// <param name="particleEffects">粒子效果数据。</param>
	public static void FireParticleEffects(MoveInfo move, UFE3D.CharacterInfo player, MoveParticleEffect particleEffects)
	{
		if (UFE.OnParticleEffects != null) UFE.OnParticleEffects(move, player, particleEffects);
	}

	/// <summary>
	/// 触发换边事件。
	/// </summary>
	/// <param name="side">新朝向侧。</param>
	/// <param name="player">换边角色。</param>
	public static void FireSideSwitch(int side, UFE3D.CharacterInfo player)
	{
		if (UFE.OnSideSwitch != null) UFE.OnSideSwitch(side, player);
	}

	/// <summary>
	/// 触发整场游戏开始事件（同时标记游戏运行中）。
	/// </summary>
	public static void FireGameBegins()
	{
		if (UFE.OnGameBegin != null)
		{
			gameRunning = true;
			UFE.OnGameBegin(config.player1Character, config.player2Character, config.selectedStage);
		}
	}

	/// <summary>
	/// 触发整场游戏结束事件（重置时间倍率/运行标志/回合广播/故事进度）。
	/// </summary>
	/// <param name="winner">获胜角色。</param>
	/// <param name="loser">失败角色。</param>
	public static void FireGameEnds(UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser)
	{
		// I've commented the next line because it worked with the old GUI, but not with the new one.
		//UFE.EndGame();

		UFE.timeScale = UFE.config._gameSpeed;
		UFE.gameRunning = false;
		UFE.newRoundCasted = false;
		UFE.player1WonLastBattle = (winner == UFE.GetPlayer1());

		/*if (UFE.fluxGameManager != null){
			UFE.fluxGameManager.Initialize();
		}*/

		if (UFE.OnGameEnds != null)
		{
			UFE.OnGameEnds(winner, loser);
		}
	}

	/// <summary>
	/// 触发回合开始事件。
	/// </summary>
	/// <param name="currentRound">回合编号。</param>
	public static void FireRoundBegins(int currentRound)
	{
		if (UFE.OnRoundBegins != null) UFE.OnRoundBegins(currentRound);
	}

	/// <summary>
	/// 触发回合结束事件。
	/// </summary>
	/// <param name="winner">获胜角色。</param>
	/// <param name="loser">失败角色。</param>
	public static void FireRoundEnds(UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser)
	{
		if (UFE.OnRoundEnds != null) UFE.OnRoundEnds(winner, loser);
	}

	/// <summary>
	/// 触发计时器更新事件。
	/// </summary>
	/// <param name="timer">当前剩余时间。</param>
	public static void FireTimer(float timer)
	{
		if (UFE.OnTimer != null) UFE.OnTimer(timer);
	}

	/// <summary>
	/// 触发时间到事件。
	/// </summary>
	public static void FireTimeOver()
	{
		if (UFE.OnTimeOver != null) UFE.OnTimeOver();
	}
	#endregion


	#region public class methods: UFE CORE methods
	/// <summary>
	/// 暂停/恢复游戏（暂停时时间倍率置 0，恢复时还原为配置速度）。
	/// </summary>
	/// <param name="pause">true 暂停，false 恢复。</param>
	public static void PauseGame(bool pause)
	{
		if (pause && UFE.timeScale == 0) return;

		if (pause)
		{
			UFE.timeScale = 0;
		}
		else
		{
			UFE.timeScale = UFE.config._gameSpeed;
		}

		if (UFE.OnGamePaused != null)
		{
			UFE.OnGamePaused(pause);
		}
	}

	/// <summary>
	/// 判断指定类名是否在已加载程序集中存在（用于检测插件安装）。
	/// </summary>
	/// <param name="theClass">类全名。</param>
	/// <returns>存在返回 true。</returns>
	public static bool IsInstalled(string theClass)
	{
		return UFE.SearchClass(theClass) != null;
	}

	/// <summary>
	/// 游戏当前是否暂停。
	/// </summary>
	/// <returns>时间倍率小于等于 0 表示暂停。</returns>
	public static bool isPaused()
	{
		return UFE.timeScale <= 0;
	}

	/// <summary>
	/// 获取当前回合剩余时间。
	/// </summary>
	/// <returns>剩余时间（定点数）。</returns>
	public static Fix64 GetTimer()
	{
		return timer;
	}

	/// <summary>
	/// 重置回合计时器为配置的回合时间。
	/// </summary>
	public static void ResetTimer()
	{
		timer = config.roundOptions._timer;
		intTimer = (int)FPMath.Round(config.roundOptions._timer);
		if (UFE.OnTimer != null) OnTimer((float)timer);
	}

	/// <summary>
	/// 在所有已加载程序集中搜索指定类（通过反射）。
	/// </summary>
	/// <param name="theClass">类全名。</param>
	/// <returns>找到的 Type；未找到返回 null。</returns>
	public static Type SearchClass(string theClass)
	{
		Type type = null;

		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			type = assembly.GetType(theClass);
			if (type != null) { break; }
		}

		return type;
	}

	/// <summary>
	/// 设置回合计时器为指定时间。
	/// </summary>
	/// <param name="time">新剩余时间。</param>
	public static void SetTimer(Fix64 time)
	{
		timer = time;
		intTimer = (int)FPMath.Round(time);
		if (UFE.OnTimer != null) OnTimer(timer);
	}

	/// <summary>
	/// 恢复回合计时器运行。
	/// </summary>
	public static void PlayTimer()
	{
		pauseTimer = false;
	}

	/// <summary>
	/// 暂停回合计时器。
	/// </summary>
	public static void PauseTimer()
	{
		pauseTimer = true;
	}

	/// <summary>
	/// 回合计时器是否处于暂停状态。
	/// </summary>
	/// <returns>暂停返回 true。</returns>
	public static bool IsTimerPaused()
	{
		return pauseTimer;
	}

	/// <summary>
	/// 结束整场游戏：销毁战斗 HUD 与游戏引擎对象。
	/// </summary>
	public static void EndGame()
	{
		/*UFE.timeScale = UFE.ToDouble(UFE.config.gameSpeed);
		UFE.gameRunning = false;
		UFE.newRoundCasted = false;*/

		if (UFE.battleGUI != null)
		{
			UFE.battleGUI.OnHide();
			GameObject.Destroy(UFE.battleGUI.gameObject);
			UFE.battleGUI = null;
		}

		if (gameEngine != null)
		{
			GameObject.Destroy(gameEngine);
			gameEngine = null;
		}
	}

	/// <summary>
	/// 重置"新回合已广播"标记。
	/// </summary>
	public static void ResetRoundCast()
	{
		newRoundCasted = false;
	}

	/// <summary>
	/// 广播新回合开始：双方开场动画均播放完毕后触发回合开始事件，并延迟 2 帧调用 StartFight。
	/// </summary>
	public static void CastNewRound()
	{
		if (newRoundCasted) return;
		if (p1ControlsScript.introPlayed && p2ControlsScript.introPlayed)
		{
			UFE.FireRoundBegins(config.currentRound);
			UFE.DelaySynchronizedAction(StartFight, (Fix64)2);
			newRoundCasted = true;
		}
	}

	/// <summary>
	/// 开始战斗：显示"开战"提示、解锁输入与移动、恢复计时器。
	/// </summary>
	public static void StartFight()
	{
		if (UFE.gameMode != GameMode.ChallengeMode)
			UFE.FireAlert(UFE.config.selectedLanguage.fight, null);
		UFE.config.lockInputs = false;
		UFE.config.lockMovements = false;
		UFE.PlayTimer();
	}

	/// <summary>
	/// 触发输入更新事件。
	/// </summary>
	/// <param name="inputReferences">当前输入引用列表。</param>
	/// <param name="player">玩家编号。</param>
	public static void CastInput(InputReferences[] inputReferences, int player)
	{
		if (UFE.OnInput != null) OnInput(inputReferences, player);
	}
	#endregion

	#region public class methods: Network Related methods
	/// <summary>
	/// 创建蓝牙对战（主机）：切换蓝牙模式、注册网络事件并创建比赛。
	/// </summary>
	public static void HostBluetoothGame()
	{
		if (UFE.isNetworkAddonInstalled)
		{
			UFE.multiplayerMode = MultiplayerMode.Bluetooth;
			UFE.AddNetworkEventListeners();
			UFE.multiplayerAPI.CreateMatch(new MultiplayerAPI.MatchCreationRequest(UFE.config.networkOptions.port, null, 1, false, null));
		}
	}

	/// <summary>
	/// 创建局域网对战（主机）：切换局域网模式、注册网络事件并创建比赛。
	/// </summary>
	public static void HostGame()
	{
		if (UFE.isNetworkAddonInstalled)
		{
			UFE.multiplayerMode = MultiplayerMode.Lan;

			UFE.AddNetworkEventListeners();
			UFE.multiplayerAPI.CreateMatch(new MultiplayerAPI.MatchCreationRequest(UFE.config.networkOptions.port, null, 1, false, null));
		}
	}

	/// <summary>
	/// 搜索蓝牙对战（客户端）：切换蓝牙模式并开始搜索比赛。
	/// </summary>
	public static void JoinBluetoothGame()
	{
		if (UFE.isNetworkAddonInstalled)
		{
			UFE.multiplayerMode = MultiplayerMode.Bluetooth;

			UFE.multiplayerAPI.OnMatchesDiscovered += UFE.OnMatchesDiscovered;
			UFE.multiplayerAPI.OnMatchDiscoveryError += UFE.OnMatchDiscoveryError;
			UFE.multiplayerAPI.StartSearchingMatches();
		}
	}

	/// <summary>
	/// 比赛发现回调：自动加入第一个发现的比赛；无比赛则显示连接丢失界面。
	/// </summary>
	/// <param name="matches">发现到的比赛列表。</param>
	protected static void OnMatchesDiscovered(ReadOnlyCollection<MultiplayerAPI.MatchInformation> matches)
	{
		UFE.multiplayerAPI.OnMatchesDiscovered -= UFE.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError -= UFE.OnMatchDiscoveryError;
		UFE.AddNetworkEventListeners();

		if (matches != null && matches.Count > 0)
		{
			// TODO: let the player choose the desired game
			UFE.multiplayerAPI.JoinMatch(matches[0]);
		}
		else
		{
			UFE.StartConnectionLostScreen();
		}
	}

	/// <summary>
	/// 比赛发现错误回调：显示连接丢失界面。
	/// </summary>
	protected static void OnMatchDiscoveryError()
	{
		UFE.multiplayerAPI.OnMatchesDiscovered -= UFE.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError -= UFE.OnMatchDiscoveryError;
		UFE.StartConnectionLostScreen();
	}

	/// <summary>
	/// 加入指定局域网比赛（客户端）。
	/// </summary>
	/// <param name="match">目标比赛信息。</param>
	public static void JoinGame(MultiplayerAPI.MatchInformation match)
	{
		if (UFE.isNetworkAddonInstalled)
		{
			UFE.multiplayerMode = MultiplayerMode.Lan;

			UFE.AddNetworkEventListeners();
			UFE.multiplayerAPI.JoinMatch(match);
		}
	}

	/// <summary>
	/// 断开网络连接：客户端调用断开比赛，服务器销毁比赛。
	/// </summary>
	public static void DisconnectFromGame()
	{
		if (UFE.isNetworkAddonInstalled)
		{
			NetworkState state = UFE.multiplayerAPI.GetConnectionState();
			if (state == NetworkState.Client)
			{
				UFE.multiplayerAPI.DisconnectFromMatch();
			}
			else if (state == NetworkState.Server)
			{
				UFE.multiplayerAPI.DestroyMatch();
			}
		}
	}
	#endregion


	#region protected instance methods: MonoBehaviour methods
	/// <summary>
	/// UFE 初始化（Awake）：加载配置、检测插件、创建 UI Canvas、初始化输入/AI/音频、初始化网络与帧同步，
	/// 并按调试配置进入战斗或片头界面。
	/// </summary>
	protected void Awake()
	{
		UFE.config = UFE_Config;
		UFE.UFEInstance = this;
		UFE.fixedDeltaTime = 1 / (Fix64)UFE.config.fps;

		FPRandom.Init();

		// Check which characters have been unlocked
		UFE.LoadUnlockedCharacters();

		// Check the installed Addons and supported 3rd party products
		UFE.isCInputInstalled = UFE.IsInstalled("cInput");
#if UFE_LITE
        UFE.isAiAddonInstalled = false;
#else
		UFE.isAiAddonInstalled = UFE.IsInstalled("RuleBasedAI");
#endif

#if UFE_LITE || UFE_BASIC
		UFE.isNetworkAddonInstalled = false;
		UFE.isPhotonInstalled = false;
        UFE.isBluetoothAddonInstalled = false;
#else
		UFE.isNetworkAddonInstalled = UFE.IsInstalled("UnetHighLevelMultiplayerAPI") && UFE.config.networkOptions.networkService != NetworkService.Disabled;
		UFE.isPhotonInstalled = UFE.IsInstalled("PhotonMultiplayerAPI") && UFE.config.networkOptions.networkService != NetworkService.Disabled;
		UFE.isBluetoothAddonInstalled = UFE.IsInstalled("BluetoothMultiplayerAPI") && UFE.config.networkOptions.networkService != NetworkService.Disabled;
#endif

		UFE.isControlFreak1Installed = UFE.IsInstalled("TouchController");              // [DGT]
		UFE.isControlFreak2Installed = UFE.IsInstalled("ControlFreak2.UFEBridge");
		UFE.isControlFreakInstalled = UFE.isControlFreak1Installed || UFE.isControlFreak2Installed;
		UFE.isRewiredInstalled = UFE.IsInstalled("Rewired.Integration.UniversalFightingEngine.RewiredUFEInputManager");

		// Check if we should run the application in background
		Application.runInBackground = UFE.config.runInBackground;

		// Check if cInput is installed and initialize the cInput GUI
		if (UFE.isCInputInstalled)
		{
			Type t = UFE.SearchClass("cGUI");
			if (t != null) t.GetField("cSkin").SetValue(null, UFE.config.inputOptions.cInputSkin);
		}

		//-------------------------------------------------------------------------------------------------------------
		// Initialize the GUI
		//-------------------------------------------------------------------------------------------------------------
		GameObject goGroup = new GameObject("CanvasGroup");
		UFE.canvasGroup = goGroup.AddComponent<CanvasGroup>();


		GameObject go = new GameObject("Canvas");
		go.transform.SetParent(goGroup.transform);

		UFE.canvas = go.AddComponent<Canvas>();
		UFE.canvas.renderMode = RenderMode.ScreenSpaceOverlay;

		if (EventSystem.current != null)
		{
			// Use the current event system if one exists
			UFE.eventSystem = EventSystem.current;
		}
		else
		{
			UFE.eventSystem = go.AddComponent<EventSystem>();
		}
		//UFE.eventSystem = GameObject.FindObjectOfType<EventSystem>();
		//if (UFE.eventSystem == null) UFE.eventSystem = go.AddComponent<EventSystem>();

		UFE.graphicRaycaster = go.AddComponent<GraphicRaycaster>();

		UFE.standaloneInputModule = go.AddComponent<StandaloneInputModule>();
		UFE.standaloneInputModule.verticalAxis = "Mouse Wheel";
		UFE.standaloneInputModule.horizontalAxis = "Mouse Wheel";
		UFE.standaloneInputModule.forceModuleActive = true;

		if (UFE.config.gameGUI.useCanvasScaler)
		{
			CanvasScaler cs = go.AddComponent<CanvasScaler>();
			cs.defaultSpriteDPI = UFE.config.gameGUI.canvasScaler.defaultSpriteDPI;
			cs.fallbackScreenDPI = UFE.config.gameGUI.canvasScaler.fallbackScreenDPI;
			cs.matchWidthOrHeight = UFE.config.gameGUI.canvasScaler.matchWidthOrHeight;
			cs.physicalUnit = UFE.config.gameGUI.canvasScaler.physicalUnit;
			cs.referencePixelsPerUnit = UFE.config.gameGUI.canvasScaler.referencePixelsPerUnit;
			cs.referenceResolution = UFE.config.gameGUI.canvasScaler.referenceResolution;
			cs.scaleFactor = UFE.config.gameGUI.canvasScaler.scaleFactor;
			cs.screenMatchMode = UFE.config.gameGUI.canvasScaler.screenMatchMode;
			cs.uiScaleMode = UFE.config.gameGUI.canvasScaler.scaleMode;

			//Line commented because we use "Screen Space - Overlay" canvas and the "dynaicPixelsPerUnit" property is only used in "World Space" Canvas.
			//cs.dynamicPixelsPerUnit = UFE.config.gameGUI.canvasScaler.dynamicPixelsPerUnit; 
		}

		// Check if "Control Freak Virtual Controller" is installed and instantiate the prefab
		if (UFE.isControlFreakInstalled && UFE.config.inputOptions.inputManagerType == InputManagerType.ControlFreak)
		{
			if (UFE.isControlFreak2Installed && (UFE.config.inputOptions.controlFreak2Prefab != null))
			{
				// Try to instantiate Control Freak 2 rig prefab...
				UFE.controlFreakPrefab = (GameObject)Instantiate(UFE.config.inputOptions.controlFreak2Prefab.gameObject);
				UFE.touchControllerBridge = (UFE.controlFreakPrefab != null) ? UFE.controlFreakPrefab.GetComponent<InputTouchControllerBridge>() : null;
				UFE.touchControllerBridge.Init();

			}
			else if (UFE.isControlFreak1Installed && (UFE.config.inputOptions.controlFreakPrefab != null))
			{
				// ...or try to instantiate Control Freak 1.x controller prefab...
				UFE.controlFreakPrefab = (GameObject)Instantiate(UFE.config.inputOptions.controlFreakPrefab);
			}
		}

		// Check if the "network addon" is installed
		string uuid = (UFE.config.gameName ?? "UFE") /*+ "_" + Application.version*/;
		if (UFE.isNetworkAddonInstalled)
		{
			GameObject networkManager = new GameObject("Network Manager");
			networkManager.transform.SetParent(this.gameObject.transform);

			UFE.lanMultiplayerAPI = networkManager.AddComponent(UFE.SearchClass("UnetLanMultiplayerAPI")) as MultiplayerAPI;
			UFE.lanMultiplayerAPI.Initialize(uuid);

			if (UFE.config.networkOptions.networkService == NetworkService.Unity)
			{
				UFE.onlineMultiplayerAPI = networkManager.AddComponent(UFE.SearchClass("UnetOnlineMultiplayerAPI")) as MultiplayerAPI;
			}
			else if (UFE.config.networkOptions.networkService == NetworkService.Photon && UFE.isPhotonInstalled)
			{
				UFE.onlineMultiplayerAPI = networkManager.AddComponent(UFE.SearchClass("PhotonMultiplayerAPI")) as MultiplayerAPI;
			}
			else if (UFE.config.networkOptions.networkService == NetworkService.Photon && !UFE.isPhotonInstalled)
			{
				Debug.LogError("You need 'Photon Unity Networking' installed in order to use Photon as a Network Service.");
			}
			UFE.onlineMultiplayerAPI.Initialize(uuid);

			if (Application.platform == RuntimePlatform.Android && UFE.isBluetoothAddonInstalled)
			{
				UFE.bluetoothMultiplayerAPI = networkManager.AddComponent(UFE.SearchClass("BluetoothMultiplayerAPI")) as MultiplayerAPI;
			}
			else
			{
				UFE.bluetoothMultiplayerAPI = networkManager.AddComponent<NullMultiplayerAPI>();
			}
			UFE.bluetoothMultiplayerAPI.Initialize(uuid);


			UFE.multiplayerAPI.SendRate = 1 / (float)UFE.config.fps;

			UFE.localPlayerController = gameObject.AddComponent<UFEController>();
			UFE.remotePlayerController = gameObject.AddComponent<DummyInputController>();

			UFE.localPlayerController.isCPU = false;
			UFE.remotePlayerController.isCPU = false;

			// TODO deprecated
			//NetworkView network = this.gameObject.AddComponent<NetworkView>();
			//network.stateSynchronization = NetworkStateSynchronization.Off;
			//network.observed = UFE.remotePlayerController;
		}
		else
		{
			UFE.lanMultiplayerAPI = this.gameObject.AddComponent<NullMultiplayerAPI>();
			UFE.lanMultiplayerAPI.Initialize(uuid);

			UFE.onlineMultiplayerAPI = this.gameObject.AddComponent<NullMultiplayerAPI>();
			UFE.onlineMultiplayerAPI.Initialize(uuid);

			UFE.bluetoothMultiplayerAPI = this.gameObject.AddComponent<NullMultiplayerAPI>();
			UFE.bluetoothMultiplayerAPI.Initialize(uuid);
		}

		UFE.fluxCapacitor = new FluxCapacitor(UFE.currentFrame, UFE.config.networkOptions.maxBufferSize);
		UFE._multiplayerMode = MultiplayerMode.Lan;


		// Initialize the input systems
		p1Controller = gameObject.AddComponent<UFEController>();
		if (UFE.config.inputOptions.inputManagerType == InputManagerType.ControlFreak)
		{
			p1Controller.humanController = gameObject.AddComponent<InputTouchController>();
		}
		else if (UFE.config.inputOptions.inputManagerType == InputManagerType.Rewired)
		{
			p1Controller.humanController = gameObject.AddComponent<RewiredInputController>();
			(p1Controller.humanController as RewiredInputController).rewiredPlayerId = 0;
		}
		else
		{
			p1Controller.humanController = gameObject.AddComponent<InputController>();
		}

		// Initialize AI
		p1SimpleAI = gameObject.AddComponent<SimpleAI>();
		p1SimpleAI.player = 1;

		p1RandomAI = gameObject.AddComponent<RandomAI>();
		p1RandomAI.player = 1;

		p1FuzzyAI = null;
		if (UFE.isAiAddonInstalled && UFE.config.aiOptions.engine == AIEngine.FuzzyAI)
		{
			p1FuzzyAI = gameObject.AddComponent(UFE.SearchClass("RuleBasedAI")) as AbstractInputController;
			p1FuzzyAI.player = 1;
			p1Controller.cpuController = p1FuzzyAI;
		}
		else
		{
			p1Controller.cpuController = p1RandomAI;
		}

		p1Controller.isCPU = config.p1CPUControl;
		p1Controller.player = 1;

		p2Controller = gameObject.AddComponent<UFEController>();
		p2Controller.humanController = gameObject.AddComponent<InputController>();

		p2SimpleAI = gameObject.AddComponent<SimpleAI>();
		p2SimpleAI.player = 2;

		p2RandomAI = gameObject.AddComponent<RandomAI>();
		p2RandomAI.player = 2;

		p2FuzzyAI = null;
		if (UFE.isAiAddonInstalled && UFE.config.aiOptions.engine == AIEngine.FuzzyAI)
		{
			p2FuzzyAI = gameObject.AddComponent(UFE.SearchClass("RuleBasedAI")) as AbstractInputController;
			p2FuzzyAI.player = 2;
			p2Controller.cpuController = p2FuzzyAI;
		}
		else
		{
			p2Controller.cpuController = p2RandomAI;
		}

		p2Controller.isCPU = config.p2CPUControl;
		p2Controller.player = 2;


		p1Controller.Initialize(config.player1_Inputs);
		p2Controller.Initialize(config.player2_Inputs);

		if (config.fps > 0)
		{
			UFE.timeScale = UFE.config._gameSpeed;
			Application.targetFrameRate = config.fps;
		}

		SetLanguage();
		UFE.InitializeAudioSystem();
		UFE.SetAIDifficulty(UFE.config.aiOptions.selectedDifficultyLevel);
		UFE.SetDebugMode(config.debugOptions.debugMode);

		// Load the player settings from disk
		UFE.SetMusic(PlayerPrefs.GetInt(UFE.MusicEnabledKey, 1) > 0);
		UFE.SetMusicVolume(PlayerPrefs.GetFloat(UFE.MusicVolumeKey, 1f));
		UFE.SetSoundFX(PlayerPrefs.GetInt(UFE.SoundsEnabledKey, 1) > 0);
		UFE.SetSoundFXVolume(PlayerPrefs.GetFloat(UFE.SoundsVolumeKey, 1f));

		// Load the intro screen or the combat, depending on the UFE Config settings
		if (UFE.config.debugOptions.startGameImmediately)
		{
			if (UFE.config.debugOptions.matchType == MatchType.Training)
			{
				UFE.gameMode = GameMode.TrainingRoom;
			}
			else if (UFE.config.debugOptions.matchType == MatchType.Challenge)
			{
				UFE.gameMode = GameMode.ChallengeMode;
			}
			else
			{
				UFE.gameMode = GameMode.VersusMode;
			}
			UFE.config.player1Character = config.p1CharStorage;
			UFE.config.player2Character = config.p2CharStorage;
			UFE.SetCPU(1, config.p1CPUControl);
			UFE.SetCPU(2, config.p2CPUControl);

			if (UFE.config.debugOptions.skipLoadingScreen)
			{
				UFE._StartGame((float)UFE.config.gameGUI.gameFadeDuration);
			}
			else
			{
				UFE._StartLoadingBattleScreen((float)UFE.config.gameGUI.screenFadeDuration);
			}
		}
		else
		{
			UFE.StartIntroScreen(0f);
		}
	}

	/// <summary>
	/// 每帧更新：驱动双方控制器读取输入；编辑器下支持 F2/F3 保存/加载帧同步状态（状态追踪器测试）。
	/// </summary>
	protected void Update()
	{
		UFE.GetPlayer1Controller().DoUpdate();
		UFE.GetPlayer2Controller().DoUpdate();

#if UNITY_EDITOR
		// Save and Load State
		if (UFE.fluxCapacitor != null && UFE.config.debugOptions.stateTrackerTest)
		{
			if (Input.GetKeyDown(KeyCode.F2))
			{ // Save State
				Debug.Log("Save (" + UFE.currentFrame + ")");
				UFE.fluxCapacitor.savedState = FluxStateTracker.SaveGameState(UFE.currentFrame);

				//dictionaryList.Add(RecordVar.SaveStateTrackers(this, new Dictionary<MemberInfo, object>()));
				//testRecording = !testRecording;
			}
			if (UFE.fluxCapacitor.savedState != null && Input.GetKeyDown(KeyCode.F3))
			{ // Load State
				Debug.Log("Load (" + UFE.fluxCapacitor.savedState.Value.networkFrame + ")");
				FluxStateTracker.LoadGameState(UFE.fluxCapacitor.savedState.Value);
				UFE.fluxCapacitor.PlayerManager.Initialize(UFE.fluxCapacitor.savedState.Value.networkFrame);

				//UFE ufeInstance = this;
				//ufeInstance = RecordVar.LoadStateTrackers(ufeInstance, dictionaryList[dictionaryList.Count - 1]) as UFE;
				//p1ControlsScript.MoveSet.MecanimControl.Refresh();
				//p2ControlsScript.MoveSet.MecanimControl.Refresh();
			}
		}
#endif
	}

#if UNITY_EDITOR
	/// <summary>
	/// 编辑器 GUI：状态追踪器测试模式下显示"保存/加载状态"按钮。
	/// </summary>
	private void OnGUI()
	{
		if (UFE.config.debugOptions.stateTrackerTest && UFE.gameRunning)
		{
			if (GUI.Button(new Rect(10, 10, 160, 40), "Save State"))
			{
				Debug.Log("Save (" + UFE.currentFrame + ")");
				UFE.fluxCapacitor.savedState = FluxStateTracker.SaveGameState(UFE.currentFrame);

				//Debug.Log(UFE.GetPlayer1ControlsScript().MoveSet.GetCurrentClipFrame());
			}

			if (GUI.Button(new Rect(10, 60, 160, 40), "Load State"))
			{
				Debug.Log("Load (" + UFE.fluxCapacitor.savedState.Value.networkFrame + ")");
				FluxStateTracker.LoadGameState(UFE.fluxCapacitor.savedState.Value);
				UFE.fluxCapacitor.PlayerManager.Initialize(UFE.fluxCapacitor.savedState.Value.networkFrame);

				//Debug.Log(UFE.GetPlayer1ControlsScript().MoveSet.GetCurrentClipFrame());
			}
		}
	}
#endif

	//public List<Dictionary<System.Reflection.MemberInfo, System.Object>> dictionaryList = new List<Dictionary<System.Reflection.MemberInfo, System.Object>>();
	//public bool testRecording = false;

	/// <summary>
	/// 固定帧率更新：驱动帧同步（FluxCapacitor）推进。
	/// </summary>
	protected void FixedUpdate()
	{
		if (UFE.fluxCapacitor != null)
		{
			UFE.fluxCapacitor.DoFixedUpdate();

			/*if (testRecording)
            {
                dictionaryList.Add(RecordVar.SaveStateTrackers(this, new Dictionary<MemberInfo, object>()));
                if (dictionaryList.Count > 30) dictionaryList.RemoveAt(0);
            }*/
		}
	}

	/// <summary>
	/// 应用退出回调：标记关闭状态并确保断开网络连接。
	/// </summary>
	protected void OnApplicationQuit()
	{
		UFE.closing = true;
		UFE.EnsureNetworkDisconnection();
	}
	#endregion

	#region protected instance methods: Network Events
	/// <summary>
	/// 是否已连接网络（API 已连接且连接数大于 0）。
	/// </summary>
	public static bool isConnected
	{
		get
		{
			return UFE.multiplayerAPI.IsConnected() && UFE.multiplayerAPI.Connections > 0;
		}
	}

	/// <summary>
	/// 确保断开网络连接：客户端断开比赛、服务器销毁比赛（已在断开中则跳过）。
	/// </summary>
	public static void EnsureNetworkDisconnection()
	{
		if (!UFE.disconnecting)
		{
			NetworkState state = UFE.multiplayerAPI.GetConnectionState();

			if (state == NetworkState.Client)
			{
				UFE.RemoveNetworkEventListeners();
				UFE.multiplayerAPI.DisconnectFromMatch();
			}
			else if (state == NetworkState.Server)
			{
				UFE.RemoveNetworkEventListeners();
				UFE.multiplayerAPI.DestroyMatch();
			}
		}
	}

	/// <summary>
	/// 注册全部网络事件监听器（先取消后注册，避免重复）。
	/// </summary>
	protected static void AddNetworkEventListeners()
	{
		UFE.multiplayerAPI.OnDisconnection -= UFE.OnDisconnectedFromServer;
		UFE.multiplayerAPI.OnJoined -= UFE.OnJoined;
		UFE.multiplayerAPI.OnJoinError -= UFE.OnJoinError;
		UFE.multiplayerAPI.OnPlayerConnectedToMatch -= UFE.OnPlayerConnectedToMatch;
		UFE.multiplayerAPI.OnPlayerDisconnectedFromMatch -= UFE.OnPlayerDisconnectedFromMatch;
		UFE.multiplayerAPI.OnMatchesDiscovered -= UFE.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError -= UFE.OnMatchDiscoveryError;
		UFE.multiplayerAPI.OnMatchCreated -= UFE.OnMatchCreated;
		UFE.multiplayerAPI.OnMatchDestroyed -= UFE.OnMatchDestroyed;

		UFE.multiplayerAPI.OnDisconnection += UFE.OnDisconnectedFromServer;
		UFE.multiplayerAPI.OnJoined += UFE.OnJoined;
		UFE.multiplayerAPI.OnJoinError += UFE.OnJoinError;
		UFE.multiplayerAPI.OnPlayerConnectedToMatch += UFE.OnPlayerConnectedToMatch;
		UFE.multiplayerAPI.OnPlayerDisconnectedFromMatch += UFE.OnPlayerDisconnectedFromMatch;
		UFE.multiplayerAPI.OnMatchesDiscovered += UFE.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError += UFE.OnMatchDiscoveryError;
		UFE.multiplayerAPI.OnMatchCreated += UFE.OnMatchCreated;
		UFE.multiplayerAPI.OnMatchDestroyed += UFE.OnMatchDestroyed;
	}

	/// <summary>
	/// 取消全部网络事件监听器。
	/// </summary>
	protected static void RemoveNetworkEventListeners()
	{
		UFE.multiplayerAPI.OnDisconnection -= UFE.OnDisconnectedFromServer;
		UFE.multiplayerAPI.OnJoined -= UFE.OnJoined;
		UFE.multiplayerAPI.OnJoinError -= UFE.OnJoinError;
		UFE.multiplayerAPI.OnPlayerConnectedToMatch -= UFE.OnPlayerConnectedToMatch;
		UFE.multiplayerAPI.OnPlayerDisconnectedFromMatch -= UFE.OnPlayerDisconnectedFromMatch;
		UFE.multiplayerAPI.OnMatchesDiscovered -= UFE.OnMatchesDiscovered;
		UFE.multiplayerAPI.OnMatchDiscoveryError -= UFE.OnMatchDiscoveryError;
		UFE.multiplayerAPI.OnMatchCreated -= UFE.OnMatchCreated;
		UFE.multiplayerAPI.OnMatchDestroyed -= UFE.OnMatchDestroyed;
	}

	/// <summary>
	/// 加入服务器成功回调：启动网络对战（本地玩家为玩家2）。
	/// </summary>
	/// <param name="match">加入的比赛信息。</param>
	protected static void OnJoined(MultiplayerAPI.JoinedMatchInformation match)
	{
		if (UFE.config.debugOptions.connectionLog) Debug.Log("Connected to server");
		UFE.StartNetworkGame(0.5f, 2, false);
	}

	/// <summary>
	/// 与服务器断开回调：恢复单机控制并显示连接丢失界面。
	/// </summary>
	protected static void OnDisconnectedFromServer()
	{
		if (UFE.config.debugOptions.connectionLog) Debug.Log("Disconnected from server");
		UFE.fluxCapacitor.Initialize(); // Return to single player controls

		if (!UFE.closing)
		{
			UFE.disconnecting = true;
			Application.runInBackground = UFE.config.runInBackground;

			if (UFE.config.lockInputs && UFE.currentScreen == null)
			{
				UFE.DelayLocalAction(UFE.StartConnectionLostScreenIfMainMenuNotLoaded, 1f);
			}
			else
			{
				UFE.StartConnectionLostScreen();
			}
		}
	}

	/// <summary>
	/// 加入服务器失败回调：显示连接丢失界面。
	/// </summary>
	protected static void OnJoinError()
	{
		if (UFE.config.debugOptions.connectionLog) Debug.Log("Could not connect to server");
		Application.runInBackground = UFE.config.runInBackground;
		UFE.StartConnectionLostScreen();
	}

	/// <summary>
	/// 比赛创建成功回调（当前空实现）。
	/// </summary>
	/// <param name="match">创建的比赛信息。</param>
	protected static void OnMatchCreated(MultiplayerAPI.CreatedMatchInformation match) { }

	/// <summary>
	/// 比赛销毁回调（当前空实现）。
	/// </summary>
	protected static void OnMatchDestroyed() { }

	/// <summary>
	/// 加入比赛响应回调（当前空实现）。
	/// </summary>
	/// <param name="response">加入响应。</param>
	protected static void OnMatchJoined(JoinMatchResponse response) { }

	/// <summary>
	/// 比赛被丢弃回调（当前空实现）。
	/// </summary>
	protected static void OnMatchDropped() { }

	/// <summary>
	/// 玩家连接比赛回调：启动网络对战（本地玩家为玩家1）。
	/// </summary>
	/// <param name="player">连接进来的玩家信息。</param>
	protected static void OnPlayerConnectedToMatch(MultiplayerAPI.PlayerInformation player)
	{
		if (UFE.config.debugOptions.connectionLog)
		{
			if (player.networkIdentity != null)
			{
				Debug.Log("Connection: " + player.networkIdentity.connectionToClient);
			}
			else
			{
				Debug.Log("Player connected: " + player.photonPlayer);
			}
		}

		UFE.StartNetworkGame(0.5f, 1, false);
	}

	/// <summary>
	/// 玩家离开比赛回调：恢复单机控制并显示连接丢失界面。
	/// </summary>
	/// <param name="player">离开的玩家信息。</param>
	protected static void OnPlayerDisconnectedFromMatch(MultiplayerAPI.PlayerInformation player)
	{
		if (UFE.config.debugOptions.connectionLog) Debug.Log("Clean up after player " + player);
		UFE.fluxCapacitor.Initialize(); // Return to single player controls

		if (!UFE.closing)
		{
			UFE.disconnecting = true;
			Application.runInBackground = UFE.config.runInBackground;

			if (UFE.config.lockInputs && UFE.currentScreen == null)
			{
				UFE.DelayLocalAction(UFE.StartConnectionLostScreenIfMainMenuNotLoaded, 1f);
			}
			else
			{
				UFE.StartConnectionLostScreen();
			}
		}
	}

	/// <summary>
	/// 服务器初始化完成回调：允许后台运行并重置断开标志。
	/// </summary>
	protected static void OnServerInitialized()
	{
		if (UFE.config.debugOptions.connectionLog) Debug.Log("Server initialized and ready");
		Application.runInBackground = true;
		UFE.disconnecting = false;
	}
	#endregion

	#region private class methods: GUI Related methods
	/// <summary>
	/// 在 Canvas 下创建一个调试文本（黑体加粗、可溢出显示）。
	/// </summary>
	/// <param name="dName">调试对象名称。</param>
	/// <param name="dText">初始文本内容。</param>
	/// <param name="position">屏幕锚定位置。</param>
	/// <param name="alignment">文本对齐方式。</param>
	/// <returns>创建的 UGUI Text 组件。</returns>
	public static Text DebuggerText(string dName, string dText, Vector2 position, TextAnchor alignment)
	{
		GameObject debugger = new GameObject(dName);
		debugger.transform.SetParent(UFE.canvas.transform);

		RectTransform trans = debugger.AddComponent<RectTransform>();
		trans.anchoredPosition = position;

		Text debuggerText = debugger.AddComponent<Text>();
		debuggerText.text = dText;
		debuggerText.alignment = alignment;
		debuggerText.color = Color.black;
		debuggerText.fontStyle = FontStyle.Bold;

		Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
		debuggerText.font = ArialFont;
		debuggerText.fontSize = 24;
		debuggerText.verticalOverflow = VerticalWrapMode.Overflow;
		debuggerText.horizontalOverflow = HorizontalWrapMode.Overflow;
		debuggerText.material = ArialFont.material;

		//Outline debuggerTextOutline = debugger.AddComponent<Outline>();
		//debuggerTextOutline.effectColor = Color.white;

		return debuggerText;
	}

	/// <summary>
	/// 进入网络游戏界面（按多人模式选择蓝牙或网络界面）。
	/// </summary>
	public static void GoToNetworkGameScreen()
	{
		if (UFE.multiplayerMode == MultiplayerMode.Bluetooth)
		{
			UFE.StartBluetoothGameScreen();
		}
		else
		{
			UFE.StartNetworkGameScreen();
		}
	}

	/// <summary>
	/// 进入网络游戏界面（按多人模式选择蓝牙或网络界面，带淡出动画）。
	/// </summary>
	/// <param name="fadeTime">淡出时长。</param>
	public static void GoToNetworkGameScreen(float fadeTime)
	{
		if (UFE.multiplayerMode == MultiplayerMode.Bluetooth)
		{
			UFE.StartBluetoothGameScreen(fadeTime);
		}
		else
		{
			UFE.StartNetworkGameScreen(fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到蓝牙对战界面（显示预制体并淡入）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartBluetoothGameScreen(float fadeTime)
	{
		UFE.EnsureNetworkDisconnection();

		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.bluetoothGameScreen == null)
		{
			Debug.LogError("Bluetooth Game Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		else if (UFE.isNetworkAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.bluetoothGameScreen);
			if (!UFE.config.gameGUI.bluetoothGameScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
		}
	}

	/// <summary>
	/// 实际切换到角色选择界面（显示预制体并淡入）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartCharacterSelectionScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.characterSelectionScreen == null)
		{
			Debug.LogError("Character Selection Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.characterSelectionScreen);
			if (!UFE.config.gameGUI.characterSelectionScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到片头界面（无片头时直接进入主菜单）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartIntroScreen(float fadeTime)
	{
		UFE.EnsureNetworkDisconnection();

		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.introScreen == null)
		{
			//Debug.Log("Intro Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.introScreen);
			if (!UFE.config.gameGUI.introScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到主菜单界面（显示预制体并淡入）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartMainMenuScreen(float fadeTime)
	{
		UFE.EnsureNetworkDisconnection();

		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.mainMenuScreen == null)
		{
			Debug.LogError("Main Menu Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.mainMenuScreen);
			if (!UFE.config.gameGUI.mainMenuScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到场地选择界面（显示预制体并淡入）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartStageSelectionScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.stageSelectionScreen == null)
		{
			Debug.LogError("Stage Selection Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.stageSelectionScreen);
			if (!UFE.config.gameGUI.stageSelectionScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到制作人员界面（显示预制体并淡入）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartCreditsScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.creditsScreen == null)
		{
			Debug.Log("Credits screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.creditsScreen);
			if (!UFE.config.gameGUI.creditsScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到连接丢失界面（未安装网络插件或无界面时回退主菜单）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartConnectionLostScreen(float fadeTime)
	{
		UFE.EnsureNetworkDisconnection();

		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.connectionLostScreen == null)
		{
			Debug.LogError("Connection Lost Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.isNetworkAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.connectionLostScreen);
			if (!UFE.config.gameGUI.connectionLostScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	/// <summary>
	/// 实际开始游戏：显示战斗 HUD、创建游戏引擎/摄像机/场地、初始化双方角色与 AI、重置回合数据。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartGame(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.battleGUI == null)
		{
			Debug.LogError("Battle GUI not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE.battleGUI = new GameObject("BattleGUI").AddComponent<UFEScreen>();
		}
		else
		{
			UFE.battleGUI = (UFEScreen)GameObject.Instantiate(UFE.config.gameGUI.battleGUI);
		}
		if (!UFE.battleGUI.hasFadeIn) fadeTime = 0;
		CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);

		UFE.battleGUI.transform.SetParent(UFE.canvas != null ? UFE.canvas.transform : null, false);
		UFE.battleGUI.OnShow();
		UFE.canvasGroup.alpha = 0;

		UFE.gameEngine = new GameObject("Game");
		UFE.cameraScript = UFE.gameEngine.AddComponent<CameraScript>();

		if (UFE.config.player1Character == null)
		{
			Debug.LogError("No character selected for player 1.");
			return;
		}
		if (UFE.config.player2Character == null)
		{
			Debug.LogError("No character selected for player 2.");
			return;
		}
		if (UFE.config.selectedStage == null)
		{
			Debug.LogError("No stage selected.");
			return;
		}

		if (UFE.config.aiOptions.engine == AIEngine.FuzzyAI)
		{
			UFE.SetFuzzyAI(1, UFE.config.player1Character);
			UFE.SetFuzzyAI(2, UFE.config.player2Character);
		}
		else
		{
			UFE.SetRandomAI(1);
			UFE.SetRandomAI(2);
		}

		UFE.config.player1Character.currentLifePoints = (Fix64)UFE.config.player1Character.lifePoints;
		UFE.config.player2Character.currentLifePoints = (Fix64)UFE.config.player2Character.lifePoints;
		UFE.config.player1Character.currentGaugePoints = 0;
		UFE.config.player2Character.currentGaugePoints = 0;

		GameObject stageInstance = null;
		if (UFE.config.stagePrefabStorage == StorageMode.Legacy)
		{
			if (UFE.config.selectedStage.prefab != null)
			{
				stageInstance = (GameObject)Instantiate(config.selectedStage.prefab);
				stageInstance.transform.parent = gameEngine.transform;
			}
			else
			{
				Debug.LogError("Stage prefab not found! Make sure you have set the prefab correctly in the Global Editor.");
			}
		}
		else
		{
			GameObject prefab = Resources.Load<GameObject>(config.selectedStage.stageResourcePath);

			if (prefab != null)
			{
				stageInstance = (GameObject)GameObject.Instantiate(prefab);
				stageInstance.transform.parent = gameEngine.transform;
			}
			else
			{
				Debug.LogError("Stage prefab not found! Make sure the prefab is correctly located under the Resources folder and the path is written correctly.");
			}
		}


		UFE.config.currentRound = 1;
		UFE.config.lockInputs = true;
		UFE.SetTimer(config.roundOptions._timer);
		UFE.PauseTimer();

		// Initialize Player 1 Character
		GameObject p1 = new GameObject("Player1");
		p1.transform.parent = gameEngine.transform;
		UFE.p1ControlsScript = p1.AddComponent<ControlsScript>();
		UFE.p1ControlsScript.Physics = p1.AddComponent<PhysicsScript>();
		UFE.p1ControlsScript.myInfo = (UFE3D.CharacterInfo)Instantiate(UFE.config.player1Character);

		UFE.config.player1Character = UFE.p1ControlsScript.myInfo;
		UFE.p1ControlsScript.myInfo.playerNum = 1;
		if (UFE.isControlFreak2Installed && UFE.p1ControlsScript.myInfo.customControls.overrideControlFreak && UFE.p1ControlsScript.myInfo.customControls.controlFreak2Prefab != null)
		{
			UFE.controlFreakPrefab = (GameObject)Instantiate(UFE.p1ControlsScript.myInfo.customControls.controlFreak2Prefab.gameObject);
			UFE.touchControllerBridge = (UFE.controlFreakPrefab != null) ? UFE.controlFreakPrefab.GetComponent<InputTouchControllerBridge>() : null;
			UFE.touchControllerBridge.Init();
		}

		// Initialize Player 2 Character
		GameObject p2 = new GameObject("Player2");
		p2.transform.parent = gameEngine.transform;
		UFE.p2ControlsScript = p2.AddComponent<ControlsScript>();
		UFE.p2ControlsScript.Physics = p2.AddComponent<PhysicsScript>();
		UFE.p2ControlsScript.myInfo = (UFE3D.CharacterInfo)Instantiate(UFE.config.player2Character);
		UFE.config.player2Character = UFE.p2ControlsScript.myInfo;
		UFE.p2ControlsScript.myInfo.playerNum = 2;


		// If the same character is selected, try loading the alt costume
		if (UFE.config.player1Character.name == UFE.config.player2Character.name)
		{
			if (UFE.config.player2Character.alternativeCostumes.Length > 0)
			{
				UFE.config.player2Character.isAlt = true;
				UFE.config.player2Character.selectedCostume = 0;

				if (UFE.config.player2Character.alternativeCostumes[0].characterPrefabStorage == StorageMode.Legacy)
				{
					UFE.p2ControlsScript.myInfo.characterPrefab = UFE.config.player2Character.alternativeCostumes[0].prefab;
				}
				else
				{
					UFE.p2ControlsScript.myInfo.characterPrefab = Resources.Load<GameObject>(UFE.config.player2Character.alternativeCostumes[0].prefabResourcePath);
				}
			}
		}

		// Initialize Debuggers
		UFE.debugger1 = UFE.DebuggerText("Debugger1", "", new Vector2(-Screen.width + 50, Screen.height - 180), TextAnchor.UpperLeft);
		UFE.debugger2 = UFE.DebuggerText("Debugger2", "", new Vector2(Screen.width - 50, Screen.height - 180), TextAnchor.UpperRight);
		UFE.p1ControlsScript.debugger = UFE.debugger1;
		UFE.p2ControlsScript.debugger = UFE.debugger2;
		UFE.debugger1.enabled = UFE.debugger2.enabled = config.debugOptions.debugMode;


		//UFE.fluxGameManager.Initialize(UFE.currentFrame);
		UFE.fluxCapacitor.savedState = null;
		UFE.PauseGame(false);
	}

	//Preloader
	/// <summary>
	/// 预加载战斗资源（使用默认预热时长）。
	/// </summary>
	public static void PreloadBattle()
	{
		PreloadBattle((float)UFE.config._preloadingTime);
	}

	/// <summary>
	/// 预加载战斗资源：按配置预热命中特效、场地、角色预制体并预热全部着色器。
	/// </summary>
	/// <param name="warmTimer">预热时长（秒）。</param>
	public static void PreloadBattle(float warmTimer)
	{
		if (UFE.config.preloadHitEffects)
		{
			SearchAndCastGameObject(UFE.config.hitOptions, warmTimer);
			SearchAndCastGameObject(UFE.config.groundBounceOptions, warmTimer);
			SearchAndCastGameObject(UFE.config.wallBounceOptions, warmTimer);
			if (UFE.config.debugOptions.preloadedObjects) Debug.Log("Hit Effects Loaded");
		}
		if (UFE.config.preloadStage)
		{
			SearchAndCastGameObject(UFE.config.selectedStage, warmTimer);
			if (UFE.config.debugOptions.preloadedObjects) Debug.Log("Stage Loaded");
		}
		if (UFE.config.preloadCharacter1)
		{
			SearchAndCastGameObject(UFE.config.player1Character, warmTimer);
			if (UFE.config.debugOptions.preloadedObjects) Debug.Log("Character 1 Loaded");
		}
		if (UFE.config.preloadCharacter2)
		{
			SearchAndCastGameObject(UFE.config.player2Character, warmTimer);
			if (UFE.config.debugOptions.preloadedObjects) Debug.Log("Character 2 Loaded");
		}
		if (UFE.config.warmAllShaders) Shader.WarmupAllShaders();

		memoryDump.Clear();
	}

	/// <summary>
	/// 递归搜索对象中的 GameObject 字段并实例化预热（释放显存/着色器编译），数组字段递归处理。
	/// </summary>
	/// <param name="target">要搜索的对象。</param>
	/// <param name="warmTimer">预热时长（秒）。</param>
	public static void SearchAndCastGameObject(object target, float warmTimer)
	{
		if (target != null)
		{
			Type typeSource = target.GetType();
			FieldInfo[] fields = typeSource.GetFields();

			foreach (FieldInfo field in fields)
			{
				object fieldValue = field.GetValue(target);
				if (fieldValue == null || fieldValue.Equals(null)) continue;
				if (memoryDump.Contains(fieldValue)) continue;
				memoryDump.Add(fieldValue);

				if (field.FieldType.Equals(typeof(GameObject)))
				{
					if (UFE.config.debugOptions.preloadedObjects) Debug.Log(fieldValue + " preloaded");
					GameObject tempGO = (GameObject)Instantiate((GameObject)fieldValue);
					tempGO.transform.position = new Vector2(-999, -999);

					//Light lightComponent = tempGO.GetComponent<Light>();
					//if (lightComponent != null) lightComponent.enabled = false;

					Destroy(tempGO, warmTimer);

				}
				else if (field.FieldType.IsArray && !field.FieldType.GetElementType().IsEnum)
				{
					object[] fieldValueArray = (object[])fieldValue;
					foreach (object obj in fieldValueArray)
					{
						SearchAndCastGameObject(obj, warmTimer);
					}
				}
			}
		}
	}

	/// <summary>
	/// 实际切换到建房界面（显示预制体并淡入；无网络插件或界面时回退主菜单）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartHostGameScreen(float fadeTime)
	{
		UFE.EnsureNetworkDisconnection();

		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.hostGameScreen == null)
		{
			Debug.LogError("Host Game Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.isNetworkAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.hostGameScreen);
			if (!UFE.config.gameGUI.hostGameScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到加入游戏界面（显示预制体并淡入；无网络插件或界面时回退主菜单）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartJoinGameScreen(float fadeTime)
	{
		UFE.EnsureNetworkDisconnection();

		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.joinGameScreen == null)
		{
			Debug.LogError("Join To Game Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.isNetworkAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.joinGameScreen);
			if (!UFE.config.gameGUI.joinGameScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到战斗加载界面（锁定输入；无加载界面时直接开始游戏）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartLoadingBattleScreen(float fadeTime)
	{
		UFE.config.lockInputs = true;

		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.loadingBattleScreen == null)
		{
			Debug.Log("Loading Battle Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartGame((float)UFE.config.gameGUI.gameFadeDuration);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.loadingBattleScreen);
			if (!UFE.config.gameGUI.loadingBattleScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到随机匹配界面（显示预制体并淡入；无网络插件或界面时回退主菜单）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartSearchMatchScreen(float fadeTime)
	{
		//UFE.EnsureNetworkDisconnection();

		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.searchMatchScreen == null)
		{
			Debug.LogError("Random Match Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.isNetworkAddonInstalled)
		{
			//UFE.AddNetworkEventListeners();
			UFE.ShowScreen(UFE.config.gameGUI.searchMatchScreen);
			if (!UFE.config.gameGUI.searchMatchScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到网络游戏界面（显示预制体并淡入；无网络插件或界面时回退主菜单）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartNetworkGameScreen(float fadeTime)
	{
		UFE.EnsureNetworkDisconnection();

		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.networkGameScreen == null)
		{
			Debug.LogError("Network Game Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.isNetworkAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.networkGameScreen);
			if (!UFE.config.gameGUI.networkGameScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到选项界面（显示预制体并淡入）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartOptionsScreen(float fadeTime)
	{

		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.optionsScreen == null)
		{
			Debug.LogError("Options Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.optionsScreen);
			if (!UFE.config.gameGUI.optionsScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际进入故事模式战斗：根据玩家1上一场胜负推进故事进度（胜利则下一场战斗，失败则重复当前战斗），
	/// 加载对手与场地；无可用战斗时显示通关祝贺界面。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	public static void _StartStoryModeBattle(float fadeTime)
	{
		// If the player 1 won the last battle, load the information of the next battle. 
		// Otherwise, repeat the last battle...
		UFE3D.CharacterInfo character = UFE.GetPlayer(1);

		if (UFE.player1WonLastBattle)
		{
			// If the player 1 won the last battle...
			if (UFE.storyMode.currentGroup < 0)
			{
				// If we haven't fought any battle, raise the "Story Mode Started" event...
				if (UFE.OnStoryModeStarted != null)
				{
					UFE.OnStoryModeStarted(character);
				}

				// And start with the first battle of the first group
				UFE.storyMode.currentGroup = 0;
				UFE.storyMode.currentBattle = 0;
			}
			else if (UFE.storyMode.currentGroup >= 0 && UFE.storyMode.currentGroup < UFE.storyMode.characterStory.fightsGroups.Length)
			{
				// Otherwise, check if there are more remaining battles in the current group
				FightsGroup currentGroup = UFE.storyMode.characterStory.fightsGroups[UFE.storyMode.currentGroup];
				int numberOfFights = currentGroup.maxFights;

				if (currentGroup.mode != FightsGroupMode.FightAgainstSeveralOpponentsInTheGroupInRandomOrder)
				{
					numberOfFights = currentGroup.opponents.Length;
				}

				if (UFE.storyMode.currentBattle < numberOfFights - 1)
				{
					// If there are more battles in the current group, go to the next battle...
					++UFE.storyMode.currentBattle;
				}
				else
				{
					// Otherwise, go to the next group of battles...
					++UFE.storyMode.currentGroup;
					UFE.storyMode.currentBattle = 0;
					UFE.storyMode.defeatedOpponents.Clear();
				}
			}

			// If the player hasn't finished the game...
			UFE.storyMode.currentBattleInformation = null;
			while (
				UFE.storyMode.currentBattleInformation == null &&
				UFE.storyMode.currentGroup >= 0 &&
				UFE.storyMode.currentGroup < UFE.storyMode.characterStory.fightsGroups.Length
			)
			{
				// Try to retrieve the information of the next battle
				FightsGroup currentGroup = UFE.storyMode.characterStory.fightsGroups[UFE.storyMode.currentGroup];
				UFE.storyMode.currentBattleInformation = null;

				if (currentGroup.mode == FightsGroupMode.FightAgainstAllOpponentsInTheGroupInTheDefinedOrder)
				{
					StoryModeBattle b = currentGroup.opponents[UFE.storyMode.currentBattle];
					UFE3D.CharacterInfo opponent = UFE.config.characters[b.opponentCharacterIndex];

					if (UFE.storyMode.canFightAgainstHimself || !character.characterName.Equals(opponent.characterName))
					{
						UFE.storyMode.currentBattleInformation = b;
					}
					else
					{
						// Otherwise, check if there are more remaining battles in the current group
						int numberOfFights = currentGroup.maxFights;

						if (currentGroup.mode != FightsGroupMode.FightAgainstSeveralOpponentsInTheGroupInRandomOrder)
						{
							numberOfFights = currentGroup.opponents.Length;
						}

						if (UFE.storyMode.currentBattle < numberOfFights - 1)
						{
							// If there are more battles in the current group, go to the next battle...
							++UFE.storyMode.currentBattle;
						}
						else
						{
							// Otherwise, go to the next group of battles...
							++UFE.storyMode.currentGroup;
							UFE.storyMode.currentBattle = 0;
							UFE.storyMode.defeatedOpponents.Clear();
						}
					}
				}
				else
				{
					List<StoryModeBattle> possibleBattles = new List<StoryModeBattle>();

					foreach (StoryModeBattle b in currentGroup.opponents)
					{
						if (!UFE.storyMode.defeatedOpponents.Contains(b.opponentCharacterIndex))
						{
							UFE3D.CharacterInfo opponent = UFE.config.characters[b.opponentCharacterIndex];

							if (UFE.storyMode.canFightAgainstHimself || !character.characterName.Equals(opponent.characterName))
							{
								possibleBattles.Add(b);
							}
						}
					}

					if (possibleBattles.Count > 0)
					{
						int index = UnityEngine.Random.Range(0, possibleBattles.Count);
						UFE.storyMode.currentBattleInformation = possibleBattles[index];
					}
					else
					{
						// If we can't find a valid battle in this group, try moving to the next group
						++UFE.storyMode.currentGroup;
					}
				}
			}
		}

		if (UFE.storyMode.currentBattleInformation != null)
		{
			// If we could retrieve the battle information, load the opponent and the stage
			int characterIndex = UFE.storyMode.currentBattleInformation.opponentCharacterIndex;
			UFE.SetPlayer2(UFE.config.characters[characterIndex]);

			if (UFE.player1WonLastBattle)
			{
				UFE.lastStageIndex = UnityEngine.Random.Range(0, UFE.storyMode.currentBattleInformation.possibleStagesIndexes.Count);
			}

			UFE.SetStage(UFE.config.stages[UFE.storyMode.currentBattleInformation.possibleStagesIndexes[UFE.lastStageIndex]]);

			// Finally, check if we should display any "Conversation Screen" before the battle
			UFE._StartStoryModeConversationBeforeBattleScreen(UFE.storyMode.currentBattleInformation.conversationBeforeBattle, fadeTime);
		}
		else
		{
			// Otherwise, show the "Congratulations" Screen
			if (UFE.OnStoryModeCompleted != null)
			{
				UFE.OnStoryModeCompleted(character);
			}

			UFE._StartStoryModeCongratulationsScreen(fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到故事模式通关祝贺界面（无界面时进入结尾演出）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartStoryModeCongratulationsScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.storyModeCongratulationsScreen == null)
		{
			Debug.Log("Congratulations Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartStoryModeEndingScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.storyModeCongratulationsScreen, delegate () { UFE.StartStoryModeEndingScreen(fadeTime); });
			if (!UFE.config.gameGUI.storyModeCongratulationsScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到故事模式继续界面（无界面时回退主菜单）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartStoryModeContinueScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.storyModeContinueScreen == null)
		{
			Debug.Log("Continue Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.storyModeContinueScreen);
			if (!UFE.config.gameGUI.storyModeContinueScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到故事模式战后对话界面（无对话界面时直接进入下一场故事战斗）。
	/// </summary>
	/// <param name="conversationScreen">对话界面预制体。</param>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartStoryModeConversationAfterBattleScreen(UFEScreen conversationScreen, float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (conversationScreen != null)
		{
			UFE.ShowScreen(conversationScreen, delegate () { UFE.StartStoryModeBattle(fadeTime); });
			if (!conversationScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			UFE._StartStoryModeBattle(fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到故事模式战前对话界面（无对话界面时直接进入战斗加载）。
	/// </summary>
	/// <param name="conversationScreen">对话界面预制体。</param>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartStoryModeConversationBeforeBattleScreen(UFEScreen conversationScreen, float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (conversationScreen != null)
		{
			UFE.ShowScreen(conversationScreen, delegate () { UFE.StartLoadingBattleScreen(fadeTime); });
			if (!conversationScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			UFE._StartLoadingBattleScreen(fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到故事模式结尾演出界面（无结尾演出时进入制作人员界面）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartStoryModeEndingScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.storyMode.characterStory.ending == null)
		{
			Debug.Log("Ending Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartCreditsScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.storyMode.characterStory.ending, delegate () { UFE.StartCreditsScreen(fadeTime); });
			if (!UFE.storyMode.characterStory.ending.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到故事模式游戏结束界面（无界面时回退主菜单）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartStoryModeGameOverScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.storyModeGameOverScreen == null)
		{
			Debug.Log("Game Over Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.storyModeGameOverScreen, delegate () { UFE.StartMainMenuScreen(fadeTime); });
			if (!UFE.config.gameGUI.storyModeGameOverScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到故事模式开场演出界面（无开场演出时直接进入第一场战斗）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartStoryModeOpeningScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.storyMode.characterStory.opening == null)
		{
			Debug.Log("Opening Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartStoryModeBattle(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.storyMode.characterStory.opening, delegate () { UFE.StartStoryModeBattle(fadeTime); });
			if (!UFE.storyMode.characterStory.opening.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到对战模式选择界面（无界面时直接开始玩家对战）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartVersusModeScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.versusModeScreen == null)
		{
			Debug.Log("Versus Mode Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE.StartPlayerVersusPlayer(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.versusModeScreen);
			if (!UFE.config.gameGUI.versusModeScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	/// <summary>
	/// 实际切换到对战结算界面（无界面时回退主菜单）。
	/// </summary>
	/// <param name="fadeTime">淡入时长。</param>
	private static void _StartVersusModeAfterBattleScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.versusModeAfterBattleScreen == null)
		{
			Debug.Log("Versus Mode \"After Battle\" Screen not found! Make sure you have set the prefab correctly in the Global Editor");

			UFE._StartMainMenuScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.versusModeAfterBattleScreen);
			if (!UFE.config.gameGUI.versusModeAfterBattleScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}
	#endregion
}