using System;
using System.Reflection;
using UnityEngine;

///--------------------------------------------------------------------------------------------------------------------
/// <summary>
/// This class tries to read the player input using Control-Freak's TouchController:
/// https://www.assetstore.unity3d.com/#/content/11562
/// 
/// If Control-Freak's TouchController is not available, it will use cInput or the Unity Input instead.
/// </summary>
///--------------------------------------------------------------------------------------------------------------------
/// <summary>
/// 触屏输入控制器（InputTouchController）。
/// <para>用途：读取移动端虚拟摇杆（Control Freak）的输入；依次尝试 Control Freak 2 桥接器、Control Freak 1.x，</para>
/// <para>均不可用时回退到 cInput / Unity Input。为轴输入应用死区过滤。</para>
/// </summary>
public class InputTouchController : InputController{
	#region public instance properties
	/// <summary>
	/// 摇杆死区：轴值绝对值低于该值视为 0。
	/// </summary>
	public float deadZone = 0f;
	#endregion

	#region public instance properties
	/// <summary>
	/// 是否使用 Control Freak 触屏输入。
	/// </summary>
	protected bool useControlFreak = false;
	/// <summary>
	/// Control Freak 2 桥接器引用（[DGT]）。
	/// </summary>
	protected InputTouchControllerBridge touchControllerBridge = null;		// [DGT]
	#endregion

	#region public overriden methods
	/// <summary>
	/// 读取输入：先调用基类读取，若为 Control Freak 轴输入且绝对值小于死区则返回 0。
	/// </summary>
	/// <param name="inputReference">输入引用。</param>
	/// <returns>读取到的输入事件。</returns>
	public override InputEvents ReadInput (InputReferences inputReference){
		InputEvents ev = base.ReadInput(inputReference);

		if (this.useControlFreak && inputReference.inputType != InputType.Button && Mathf.Abs((float)ev.axisRaw) < this.deadZone){
			return new InputEvents(0f);
		}

		return ev;
	}
	#endregion

	#region protected overriden methods
	/// <summary>
	/// 选择输入类型：优先查找 Control Freak 2 桥接器，其次 Control Freak 1.x，最后回退基类（cInput/Unity Input）。
	/// </summary>
	protected override void SelectInputType (){

		// [DGT]
		// First, look for Control Freak 2 rig with UFE Bridge component...

		InputTouchControllerBridge bridge =
			GameObject.FindObjectOfType<InputTouchControllerBridge>();

		if (bridge != null)
			{
			this.InitializeTouchControllerBridge(bridge);
			return;
			}

		// Then, look for Control Freak 1.x controller...

		else
			{
		Type type = UFE.SearchClass("TouchController");
		UnityEngine.Object touchController = null;

			if ((type != null) && ((touchController = GameObject.FindObjectOfType(type)) != null))
				{
				this.InitializeControlFreakTouchController(touchController);
				return;
				}
			}

		// If nothing found, use standard Input...

		base.SelectInputType();
		
	}
	#endregion

	#region protected instance methods

	// [DGT]
	// Init Control Freak 2 bridge.

	/// <summary>
	/// 初始化 Control Freak 2 桥接器：绑定读取委托并隐藏战斗触屏控件。
	/// </summary>
	/// <param name="bridge">Control Freak 2 桥接器组件。</param>
	protected void InitializeTouchControllerBridge(InputTouchControllerBridge bridge)
		{
		this.touchControllerBridge = bridge;
		this.touchControllerBridge.Init();

		this.touchControllerBridge.ShowBattleControls(false, false);			//  Start with battle controls hidden.

		this.getAxis		= this.touchControllerBridge.GetAxis;
		this.getAxisRaw	= this.touchControllerBridge.GetAxisRaw;
		this.getButton		= this.touchControllerBridge.GetButton;
		}


	/// <summary>
	/// 初始化 Control Freak 1.x 触屏控制器：通过反射绑定读取委托。
	/// </summary>
	/// <param name="touchController">Control Freak 的 TouchController 对象。</param>
	protected virtual void InitializeControlFreakTouchController(UnityEngine.Object touchController){
		if (touchController != null){
			Type inputType = touchController.GetType();

			if (inputType != null){
				this.deadZone = UFE.config.inputOptions.controlFreakDeadZone;
				this.useControlFreak = true;

				// Retrieve the required methods using the Reflection API to avoid 
				// compilation errors if Control-Freak's TouchController hasn't been 
				// imported into the project. We will cache the method information 
				// to call these methods later
				MethodInfo getAxisInfo = inputType.GetMethod(
					"GetAxis",
					BindingFlags.Instance | BindingFlags.Public,
					null,
					new Type[]{typeof(string)},
					null
				);
				
				/// <summary>反射缓存：读取平滑轴值。</summary>
				if (getAxisInfo != null){
					this.getAxis = delegate(string axis){
						return (float) getAxisInfo.Invoke(touchController, new object[]{axis});
					};
				}

				MethodInfo getAxisRawInfo = inputType.GetMethod(
					"GetAxisRaw",
					BindingFlags.Instance | BindingFlags.Public,
					null,
					new Type[]{typeof(string)},
					null
				);
				
				/// <summary>反射缓存：读取轴原始值。</summary>
				if (getAxisRawInfo != null){
					this.getAxisRaw = delegate(string axis){
						return (float) getAxisRawInfo.Invoke(touchController, new object[]{axis});
					};
				}


				MethodInfo getButtonInfo = inputType.GetMethod(
					"GetButton",
					BindingFlags.Instance | BindingFlags.Public,
					null,
					new Type[]{typeof(string)},
					null
				);
				
				/// <summary>反射缓存：读取按钮状态。</summary>
				if (getButtonInfo != null){
					this.getButton = delegate(string button){
						return (bool) getButtonInfo.Invoke(touchController, new object[]{button});
					};
				}
			}
		}
	}
	#endregion
}
