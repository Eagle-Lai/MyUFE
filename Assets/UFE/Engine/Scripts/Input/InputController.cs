using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

///--------------------------------------------------------------------------------------------------------------------
/// <summary>
/// This class tries to read the player input using cInput:
/// https://www.assetstore.unity3d.com/#/content/3129
/// 
/// If cInput is not available, it will use the Unity Input instead.
/// </summary>
///--------------------------------------------------------------------------------------------------------------------
/// <summary>
/// 输入控制器（InputController）。
/// <para>用途：读取玩家输入，优先使用 cInput 插件，未安装时回退到 Unity Input。</para>
/// <para>通过反射缓存 cInput 方法委托，避免未导入 cInput 时产生编译错误。</para>
/// <para>在 InitializeCInput 中还会根据配置自动注册轴/按键到 cInput。</para>
/// </summary>
public class InputController : AbstractInputController{
	#region public instance properties
	//-----------------------------------------------------------------------------
	// TODO: This value should be read from cInput
	/// <summary>
	/// 空输入名（cInput 中表示未绑定按键的占位名）。
	/// </summary>
	protected string None = "None";
	//-----------------------------------------------------------------------------
	#endregion
	
	#region protected instance properties
	/// <summary>
	/// 轴值读取委托（GetAxis，返回平滑轴值）。
	/// </summary>
	protected Func<string, float>	getAxis			= null;
	/// <summary>
	/// 轴原始值读取委托（GetAxisRaw，返回未平滑的轴值）。
	/// </summary>
	protected Func<string, float>	getAxisRaw		= null;
	/// <summary>
	/// 按钮状态读取委托（GetButton）。
	/// </summary>
	protected Func<string, bool>	getButton		= null;
	/// <summary>
	/// 是否使用 Unity InputManager（true 时支持摇杆轴名叠加读取）。
	/// </summary>
	protected bool					inputManager	= false;
	#endregion
	
	#region public overriden methods 
	/// <summary>
	/// 初始化输入控制器并选择输入类型（cInput 或 Unity Input）。
	/// </summary>
	/// <param name="inputs">输入引用列表。</param>
	public override void Initialize(IEnumerable<InputReferences> inputs){
		base.Initialize(inputs);
		this.SelectInputType();
	}
	
	/// <summary>
	/// 读取指定输入引用的输入：轴输入读取轴向值（支持摇杆轴叠加），按钮输入读取按下状态。
	/// </summary>
	/// <param name="inputReference">输入引用。</param>
	/// <returns>读取到的输入事件。</returns>
	public override InputEvents ReadInput(InputReferences inputReference){
		if (inputReference != null){
			string buttonName = inputReference.inputButtonName;
			string joystickAxisName = inputReference.joystickAxisName;

			if(
				inputReference.inputType == InputType.HorizontalAxis ||
				inputReference.inputType == InputType.VerticalAxis
			){
				float axisRaw = this.getAxisRaw(buttonName);

				if (this.inputManager && !string.IsNullOrEmpty(joystickAxisName)){
					axisRaw += this.getAxisRaw(joystickAxisName);
				}

				// If we try to read the axis value as if it were a button,
				// it will return count as pressed if the value of the axis is not zero
				return new InputEvents(axisRaw);
			}else{
				return new InputEvents(this.getButton(buttonName));
			}
		}else{
			return InputEvents.Default;
		}
	}
	#endregion
	
	#region protected instance methods
	/// <summary>
	/// 选择输入类型：若已安装 cInput 且配置要求使用 cInput，则初始化 cInput；否则使用 Unity Input。
	/// </summary>
	protected virtual void SelectInputType(){
		// Check if we have already selected if we are going to use CInput or the built-in Unity Input
		if (this.getAxis == null){
			// If we haven't made a decision yet, check if CInput is installed
			if (UFE.isCInputInstalled && UFE.config.inputOptions.inputManagerType == InputManagerType.cInput){
				this.InitializeCInput();
			}else{
				this.InitializeInput();
			}
		}
	}
	
	/// <summary>
	/// 初始化使用 Unity 内置 Input 读取输入。
	/// </summary>
	protected virtual void InitializeInput(){
		// Otherwise, use the built-in Unity Input
		if (this.getAxis == null){
			this.getAxis = Input.GetAxis;
		}
		
		if (this.getAxisRaw == null){
			this.getAxisRaw = Input.GetAxisRaw;
		}
		
		if (this.getButton == null){
			this.getButton = Input.GetButton;
		}
		
		this.inputManager = true;
	}
	
	/// <summary>
	/// 初始化使用 cInput 读取输入，并通过反射为每个输入引用注册轴/按键。
	/// <para>所有 cInput 方法均通过反射调用，避免未导入 cInput 时报编译错误。</para>
	/// </summary>
	protected virtual void InitializeCInput(){
		// If cInput is defined, use cInput
		Type inputType = UFE.SearchClass("cInput");
		
		if (inputType != null){
			// Retrieve the required methods using the Reflection API to avoid 
			// compilation errors if cInput hasn't been imported into the project
			// We will cache the method information to call these methods later
			MethodInfo getAxisInfo = inputType.GetMethod(
				"GetAxis",
				BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
				null,
				new Type[]{typeof(string)},
				null
			);
			
			/// <summary>反射缓存：读取平滑轴值。</summary>
			if (getAxisInfo != null){
				this.getAxis = delegate(string axis){
					return (float) getAxisInfo.Invoke(null, new object[]{axis});
				};
			}
			
			MethodInfo getAxisRawInfo = inputType.GetMethod(
				"GetAxisRaw",
				BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
				null,
				new Type[]{typeof(string)},
				null
			);
			
			/// <summary>反射缓存：读取轴原始值。</summary>
			if (getAxisRawInfo != null){
				this.getAxisRaw = delegate(string axis){
					return (float) getAxisRawInfo.Invoke(null, new object[]{axis});
				};
			}
			
			
			MethodInfo getButtonInfo = inputType.GetMethod(
				"GetButton",
				BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
				null,
				new Type[]{typeof(string)},
				null
			);
			
			/// <summary>反射缓存：读取按钮状态。</summary>
			if (getButtonInfo != null){
				this.getButton = delegate(string button){
					return (bool) getButtonInfo.Invoke(null, new object[]{button});
				};
			}
			
			
			MethodInfo setAxisInfo = inputType.GetMethod(
				"SetAxis",
				BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
				null,
				new Type[]{typeof(string), typeof(string), typeof(string)},
				null
			);
			
			/// <summary>反射缓存：注册轴（设置轴名与正负按键）。</summary>
			Action<string, string, string> setAxis = delegate(string axis, string negativeButton, string positiveButton){
				setAxisInfo.Invoke(null, new object[]{axis, negativeButton, positiveButton});
			};
			
			
			MethodInfo setKeyInfo = inputType.GetMethod(
				"SetKey",
				BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
				null,
				new Type[]{typeof(string), typeof(string), typeof(string)},
				null
			);
			
			/// <summary>反射缓存：注册按键（设置键名与主副按键）。</summary>
			Action<string, string, string> setKey = delegate(string key, string primary, string secondary){
				setKeyInfo.Invoke(null, new object[]{key, primary, secondary});
			};
			
			
			MethodInfo isAxisDefinedInfo = inputType.GetMethod(
				"IsAxisDefined",
				BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
				null,
				new Type[]{typeof(string)},
				null
			);
			
			/// <summary>反射缓存：判断轴是否已定义。</summary>
			Func<string, bool> isAxisDefined = delegate(string axis){
				return (bool) isAxisDefinedInfo.Invoke(null, new object[]{axis});
			};
			
			
			MethodInfo isKeyDefinedInfo = inputType.GetMethod(
				"IsKeyDefined",
				BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
				null,
				new Type[]{typeof(string)},
				null
			);
			
			/// <summary>反射缓存：判断按键是否已定义。</summary>
			Func<string, bool> isKeyDefined = delegate(string key){
				return (bool) isKeyDefinedInfo.Invoke(null, new object[]{key});
			};
			
			/// <summary>应用 cInput 的允许重复按键配置。</summary>
			PropertyInfo allowDuplicatesInfo = inputType.GetProperty("allowDuplicates");
			allowDuplicatesInfo.SetValue(
				null, 
				Convert.ChangeType(UFE.config.inputOptions.cInputAllowDuplicates, allowDuplicatesInfo.PropertyType),
				null
			);
			
			/// <summary>应用 cInput 的重力配置。</summary>
			inputType.GetField("gravity").SetValue(null, UFE.config.inputOptions.cInputGravity);
			/// <summary>应用 cInput 的灵敏度配置。</summary>
			inputType.GetField("sensitivity").SetValue(null, UFE.config.inputOptions.cInputSensitivity);
			/// <summary>应用 cInput 的死区配置。</summary>
			inputType.GetField("deadzone").SetValue(null, UFE.config.inputOptions.cInputDeadZone);
			
			
			// Iterate over all the input references...
			foreach (InputReferences input in this.inputReferences){
				// Check the type of input...
				if (input.inputType == InputType.Button){
					// If this input reference represents the vertical axis,
					// check if the reference is defined in cInput...
					if (!isKeyDefined(input.inputButtonName)){
						string defaultKey = input.cInputPositiveDefaultKey;
						string alternativeKey = input.cInputPositiveAlternativeKey;
						
						if (string.IsNullOrEmpty(defaultKey)){
							defaultKey = this.None;
						}

						if (string.IsNullOrEmpty(alternativeKey)){
							alternativeKey = this.None;
						}
						
						// If it wasn't defined, define the input with the default values
						setKey(input.inputButtonName, defaultKey, alternativeKey);
					}
				}else{
					string negativeKeyName = input.cInputNegativeKeyName;
					string positiveKeyName = input.cInputPositiveKeyName;
					string negativeDefaultKey = input.cInputNegativeDefaultKey;
					string positiveDefaultKey = input.cInputPositiveDefaultKey;
					string positiveAlternativeKey = input.cInputPositiveAlternativeKey;
					string negativeAlternativeKey = input.cInputNegativeAlternativeKey;
					
					if (input.inputType == InputType.HorizontalAxis){
						// If this input reference represents the horizontal axis,
						// check if we should use the default values...
						if (string.IsNullOrEmpty(negativeKeyName)){
							negativeKeyName = input.inputButtonName + "_Left";
						}
						
						if (string.IsNullOrEmpty(positiveKeyName)){
							positiveKeyName = input.inputButtonName + "_Right";
						}
						
						if (string.IsNullOrEmpty(negativeDefaultKey)){
							negativeDefaultKey = "LeftArrow";
						}
						
						if (string.IsNullOrEmpty(positiveDefaultKey)){
							positiveDefaultKey = "RightArrow";
						}
					}else{
						// If this input reference represents the vertical axis,
						// check if we should use the default values...
						if (string.IsNullOrEmpty(negativeKeyName)){
							negativeKeyName = input.inputButtonName + "_Down";
						}
						
						if (string.IsNullOrEmpty(positiveKeyName)){
							positiveKeyName = input.inputButtonName + "_Up";
						}
						
						if (string.IsNullOrEmpty(negativeDefaultKey)){
							negativeDefaultKey = "DownArrow";
						}
						
						if (string.IsNullOrEmpty(positiveDefaultKey)){
							positiveDefaultKey = "UpArrow";
						}
					}

					if (string.IsNullOrEmpty(positiveAlternativeKey)){
						positiveAlternativeKey = this.None;
					}

					if (string.IsNullOrEmpty(negativeAlternativeKey)){
						negativeAlternativeKey = this.None;
					}
					
					// Finally, check if the axis is defined in cInput...
					if (!isAxisDefined(input.inputButtonName)){
						if (!isKeyDefined(negativeKeyName)){
							setKey(negativeKeyName, negativeDefaultKey, negativeAlternativeKey);
						}
						if (!isKeyDefined(positiveKeyName)){
							setKey(positiveKeyName, positiveDefaultKey, positiveAlternativeKey);
						}
						setAxis(input.inputButtonName, negativeKeyName, positiveKeyName);
					}
				}
			}
		}
	}
	#endregion
}
