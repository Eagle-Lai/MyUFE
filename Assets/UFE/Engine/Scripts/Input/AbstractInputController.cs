using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// 抽象输入控制器（AbstractInputController）。
/// <para>用途：定义读取玩家输入的抽象基类。维护输入引用列表（方向轴/垂直轴/按钮），</para>
/// <para>并在 DoUpdate 中逐项读取输入打包为 InputEvents。</para>
/// <para>子类（InputController/RewiredInputController/InputTouchController 等）实现具体输入读取逻辑。</para>
/// </summary>
public abstract class AbstractInputController : MonoBehaviour{
	#region public instance properties
	/// <summary>
	/// 当前控制器管理的全部输入引用（只读集合）。
	/// </summary>
	public ReadOnlyCollection<InputReferences> inputReferences {get; protected set;}
	/// <summary>
	/// 水平轴输入引用。
	/// </summary>
	public InputReferences horizontalAxis {get; protected set;}
	/// <summary>
	/// 垂直轴输入引用。
	/// </summary>
	public InputReferences verticalAxis {get; protected set;}
	/// <summary>
	/// 按钮输入引用集合（只读）。
	/// </summary>
	public ReadOnlyCollection<InputReferences> buttons {get; protected set;}
	/// <summary>
	/// 各输入引用对应的当前输入事件字典（按输入引用索引）。
	/// </summary>
	public virtual Dictionary<InputReferences, InputEvents> inputs{get; protected set;}
	/// <summary>
	/// 玩家编号（1 或 2）。
	/// </summary>
	public virtual int player{get; set;}
	#endregion

	#region public instance methods
	/// <summary>
	/// 获取指定输入引用对应的当前输入事件。
	/// </summary>
	/// <param name="inputReference">目标输入引用。</param>
	/// <returns>该输入引用的当前 InputEvents；未找到时返回 null。</returns>
	public InputEvents GetInput(InputReferences inputReference){
		InputEvents currentEvent = null;
		if (inputReference != null && this.inputs.TryGetValue(inputReference, out currentEvent)){
			return currentEvent;
		}
		return null;
	}

	/// <summary>
	/// 根据引擎按键（ButtonPress）查找对应的输入引用。
	/// </summary>
	/// <param name="button">目标引擎按键。</param>
	/// <returns>映射到该按键的输入引用；未找到时返回 null。</returns>
	public InputReferences GetInputReference(ButtonPress button){
		foreach (InputReferences inputReference in this.inputReferences){
			if (inputReference != null && inputReference.engineRelatedButton == button){
				return inputReference;
			}
		}
		return null;
	}

	/// <summary>
	/// 初始化输入控制器：为每个输入引用创建默认输入事件，并分类轴与按钮。
	/// </summary>
	/// <param name="inputReferences">要管理的输入引用列表。</param>
	public virtual void Initialize(IEnumerable<InputReferences> inputReferences){
		List<InputReferences> buttonList = new List<InputReferences>();
		List<InputReferences> inputReferenceList = new List<InputReferences>();

		this.inputs = new Dictionary<InputReferences, InputEvents>();
		if (inputReferences != null){
			foreach (InputReferences inputReference in inputReferences){
				if (inputReference != null){
					this.inputs[inputReference] = InputEvents.Default;

					inputReferenceList.Add(inputReference);
					if (inputReference.inputType == InputType.HorizontalAxis){
						this.horizontalAxis = inputReference;
					}else if (inputReference.inputType == InputType.VerticalAxis){
						this.verticalAxis = inputReference;
					}else{
						buttonList.Add(inputReference);
					}
				}
			}
		}
		
		this.inputReferences = new ReadOnlyCollection<InputReferences>(inputReferenceList);
		this.buttons = new ReadOnlyCollection<InputReferences>(buttonList);
	}
	#endregion

	#region abstract methods definition
	/// <summary>
	/// 抽象方法：读取指定输入引用的当前输入事件（由子类实现具体输入源读取）。
	/// </summary>
	/// <param name="inputReference">要读取的输入引用。</param>
	/// <returns>读取到的输入事件。</returns>
	public abstract InputEvents ReadInput(InputReferences inputReference);
	#endregion

	#region MonoBehaviour methods
	/// <summary>
	/// 每帧更新：读取所有输入引用的当前输入状态。
	/// </summary>
	public virtual void DoUpdate(){
		if (this.inputReferences != null){
			//---------------------------------------------------------------------------------------------------------
			// Read the player input.
			//---------------------------------------------------------------------------------------------------------
			foreach (InputReferences inputReference in this.inputReferences){
				this.inputs[inputReference] = this.ReadInput(inputReference);
			}
		}
	}

	/// <summary>
	/// 固定帧率更新（虚方法，默认空实现）。
	/// </summary>
	public virtual void DoFixedUpdate(){}
	#endregion
}
