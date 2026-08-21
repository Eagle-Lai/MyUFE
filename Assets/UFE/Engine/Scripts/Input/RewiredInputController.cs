using System;
using System.Collections.Generic;
using UnityEngine;

///--------------------------------------------------------------------------------------------------------------------
/// <summary>
/// This class reads the player input using Rewired:
/// https://www.assetstore.unity3d.com/en/#!/content/21676
/// 
/// If Rewired is not available, it will use Unity Input instead.
/// </summary>
///--------------------------------------------------------------------------------------------------------------------
/// <summary>
/// Rewired 输入控制器（RewiredInputController）。
/// <para>用途：通过 Rewired 插件（或回退到 Unity Input）读取玩家输入。</para>
/// <para>通过静态 IInputSource 抽象输入源解耦 Rewired/Unity，并在战斗 GUI 显隐时控制触屏控件显示。</para>
/// </summary>
public sealed class RewiredInputController : AbstractInputController{
    
    #region Static Members
	/// <summary>
	/// 静态输入源实例。
	/// </summary>
    private static IInputSource _inputSource;
	/// <summary>
	/// 静态触屏输入 UI 实例。
	/// </summary>
    private static ITouchInputUI _touchInputUI;
	/// <summary>
	/// 静态输入配置实例。
	/// </summary>
    private static IInputConfiguration _inputConfiguration;

	/// <summary>
	/// 输入源（默认使用 Unity Input 实现，可替换为 Rewired 实现）。
	/// </summary>
    public static IInputSource inputSource {
        get {
            return _inputSource ?? (_inputSource = new UnityInputSource());
        }
        set {
            _inputSource = value;
        }
    }

	/// <summary>
	/// 触屏输入 UI 接口实例。
	/// </summary>
    public static ITouchInputUI touchInputUI {
        get {
            return _touchInputUI;
        }
        set {
            _touchInputUI = value;
        }
    }

	/// <summary>
	/// 输入配置接口实例。
	/// </summary>
    public static IInputConfiguration inputConfiguration {
        get {
            return _inputConfiguration;
        }
        set {
            _inputConfiguration = value;
        }
    }
    
    #endregion

    #region Instance Members
	/// <summary>
	/// Rewired 玩家 ID。
	/// </summary>
    public int rewiredPlayerId;
    
	/// <summary>
	/// 上一帧是否处于战斗 GUI 状态（用于检测变化）。
	/// </summary>
    private bool prevBattleGUI;
	/// <summary>
	/// 上一帧游戏是否暂停（用于检测变化）。
	/// </summary>
    private bool prevGamePaused;

    #region Overriden Methods 

	/// <summary>
	/// 初始化：调用基类初始化。
	/// </summary>
	/// <param name="inputs">输入引用列表。</param>
    public override void Initialize(IEnumerable<InputReferences> inputs){
		base.Initialize (inputs);
	}

	/// <summary>
	/// 每帧更新：调用基类读取输入，并根据战斗 GUI/暂停状态控制触屏控件显隐。
	/// </summary>
    public override void DoUpdate() {
        base.DoUpdate();

        bool battleGUI = (UFE.battleGUI != null);
        bool gamePaused = UFE.isPaused();

        if(touchInputUI != null) {
            if(battleGUI != this.prevBattleGUI) {
                touchInputUI.showTouchControls = battleGUI && !gamePaused;
            } else if(gamePaused != this.prevGamePaused) {
                if(battleGUI) {
                    touchInputUI.showTouchControls = !gamePaused;
                }
            }
        }

        this.prevBattleGUI = battleGUI;
        this.prevGamePaused = gamePaused;
    }

	/// <summary>
	/// 读取指定输入引用的输入：轴输入读取轴向原始值，按钮输入读取按下状态。
	/// </summary>
	/// <param name="inputReference">输入引用。</param>
	/// <returns>读取到的输入事件。</returns>
    public override InputEvents ReadInput(InputReferences inputReference){
		if (inputReference != null){
			string buttonName = inputReference.inputButtonName;
			string axisName = inputReference.joystickAxisName;

			if(
				inputReference.inputType == InputType.HorizontalAxis ||
				inputReference.inputType == InputType.VerticalAxis
			){
				return new InputEvents(
                    inputSource.GetAxisRaw(rewiredPlayerId, axisName)
                );
			}else{
				return new InputEvents(
                    inputSource.GetButton(rewiredPlayerId, buttonName)
                );
			}
		}else{
			return InputEvents.Default;
		}
	}

    #endregion

    #endregion

    #region Classes / Interfaces
	/// <summary>
	/// 输入源接口：抽象按钮/轴向读取（可接入 Rewired 或 Unity Input）。
	/// </summary>
    public interface IInputSource {
		/// <summary>读取指定玩家、指定名称按钮的按下状态。</summary>
        bool GetButton(int playerId, string name);
		/// <summary>读取指定玩家、指定名称轴的当前值。</summary>
        float GetAxis(int playerId, string name);
		/// <summary>读取指定玩家、指定名称轴的原始值。</summary>
        float GetAxisRaw(int playerId, string name);
    }

	/// <summary>
	/// 触屏输入 UI 接口：控制移动端虚拟控件的显示。
	/// </summary>
    public interface ITouchInputUI {
		/// <summary>是否显示触屏控件。</summary>
        bool showTouchControls { get; set; }
    }

	/// <summary>
	/// 输入配置接口：控制输入配置 UI 的显示。
	/// </summary>
    public interface IInputConfiguration {
		/// <summary>是否显示输入配置 UI。</summary>
        bool showInputConfigurationUI { get; set; }
		/// <summary>显示输入配置界面，关闭时回调。</summary>
		/// <param name="closedCallback">界面关闭后的回调。</param>
        void ShowInputConfigurationUI(Action closedCallback);
    }

	/// <summary>
	/// Unity Input 输入源实现（默认实现）。
	/// </summary>
    private class UnityInputSource : IInputSource {
		/// <summary>读取轴值。</summary>
        public float GetAxis(int playerId, string name) {
            return Input.GetAxis(name);
        }

		/// <summary>读取轴原始值。</summary>
        public float GetAxisRaw(int playerId, string name) {
            return Input.GetAxisRaw(name);
        }

		/// <summary>读取按钮状态。</summary>
        public bool GetButton(int playerId, string name) {
            return Input.GetButton(name);
        }
    }

    #endregion
}
