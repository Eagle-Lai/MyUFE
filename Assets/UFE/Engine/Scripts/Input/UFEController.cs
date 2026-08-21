using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UFE 输入控制器（UFEController）。
/// <para>用途：同时管理一个"人类控制器"（humanController）与一个"AI 控制器"（cpuController），</para>
/// <para>根据 isCPU 标志决定使用哪个控制器的输入，同时始终监听人类控制器的 Start 键（暂停用）。</para>
/// </summary>
public class UFEController : AbstractInputController {
	#region public instance fields
	/// <summary>
	/// 当前角色是否由 CPU（AI）控制。
	/// </summary>
	public bool isCPU = false;

	/// <summary>
	/// CPU 控制器引用（设置时自动用当前输入引用初始化）。
	/// </summary>
	public AbstractInputController cpuController{
		get{
			return this._cpuController;
		}
		set{
			this._cpuController = value;
			
			if (this._cpuController != null && this.inputReferences != null) {
				this._cpuController.Initialize (this.inputReferences);
			}
		}
	}

	/// <summary>
	/// 人类控制器引用（设置时自动用当前输入引用初始化）。
	/// </summary>
	public AbstractInputController humanController{
		get{
				return this._humanController;
		}
		set{
			this._humanController = value;

			if (this._humanController != null && this.inputReferences != null) {
				this._humanController.Initialize (this.inputReferences);
			}
		}
	}

	/// <summary>
	/// 玩家编号（同时同步给人类控制器与 CPU 控制器）。
	/// </summary>
	public override int player{
		get{return base.player;}
		set{
			base.player = value;

			if (this._humanController != null){
				this.humanController.player = value;
			}

			if (this.cpuController){
				this.cpuController.player = value;
			}
		}
	}
	#endregion

	#region protected instance fields
	/// <summary>
	/// 人类控制器内部引用。
	/// </summary>
	protected AbstractInputController _humanController;
	/// <summary>
	/// CPU 控制器内部引用。
	/// </summary>
	protected AbstractInputController _cpuController;
	#endregion

	#region override methods
	/// <summary>
	/// 初始化：同时初始化人类控制器与 CPU 控制器。
	/// </summary>
	/// <param name="inputs">要管理的输入引用列表。</param>
	public override void Initialize (IEnumerable<InputReferences> inputs){
		base.Initialize (inputs);
		if (this.cpuController != null) {
			this.cpuController.Initialize (inputs);
		}
		if (this.humanController != null) {
			this.humanController.Initialize(inputs);
		}
	}
	
	/// <summary>
	/// 直接读取输入（本类不直接读取，返回默认空输入）。
	/// </summary>
	/// <param name="inputReference">输入引用。</param>
	/// <returns>默认空输入事件。</returns>
	public override InputEvents ReadInput (InputReferences inputReference){
		return InputEvents.Default;
	}

	/// <summary>
	/// 固定帧率更新：根据是否 CPU 控制分发输入来源。
	/// <para>CPU 控制器仅在角色被 CPU 控制且游戏运行时更新；人类控制器始终更新（监听 Start 键）；</para>
	/// <para>最后按 UseHumanController 结果将输入写入统一字典。</para>
	/// </summary>
	public override void DoFixedUpdate (){
		if (this.inputReferences != null){
			//---------------------------------------------------------------------------------------------------------
			// The CPU Controller is only updated when the character is controlled by the CPU...
			//---------------------------------------------------------------------------------------------------------
			if (this.cpuController != null && this.isCPU && UFE.gameRunning && !UFE.isPaused()){
				this.cpuController.DoFixedUpdate();
			}

			//---------------------------------------------------------------------------------------------------------
			// But the player controller is always updated because we want to know if the player pressed the "Start"
			// button even if the character is being controlled by the CPU
			//---------------------------------------------------------------------------------------------------------
			if (this.humanController != null){
				this.humanController.DoFixedUpdate();
			}

			//---------------------------------------------------------------------------------------------------------
			// After that, we update every input refefrence stored in this class.
			//---------------------------------------------------------------------------------------------------------
			foreach (InputReferences inputReference in this.inputReferences){
				if (this.UseHumanController(inputReference)){
					this.inputs[inputReference] = this.humanController.inputs[inputReference];

				}else if (this.cpuController != null){
					this.inputs[inputReference] = this.cpuController.inputs[inputReference];

				}else{
					this.inputs[inputReference] = InputEvents.Default;
				}
			}
		}
	}

	/// <summary>
	/// 每帧更新：仅更新 CPU 控制器（当为 CPU 控制时）与人类控制器。
	/// </summary>
	public override void DoUpdate (){
		//---------------------------------------------------------------------------------------------------------
		// The CPU Controller is only updated when the character is controlled by the CPU...
		//---------------------------------------------------------------------------------------------------------
		if (this.cpuController != null && this.isCPU && UFE.gameRunning && !UFE.isPaused()){
			this.cpuController.DoUpdate();
		}
		
		//---------------------------------------------------------------------------------------------------------
		// But the player controller is always updated because we want to know if the player pressed the "Start"
		// button even if the character is being controlled by the CPU
		//---------------------------------------------------------------------------------------------------------
		if (this.humanController != null){
			this.humanController.DoUpdate();
		}
	}
	#endregion

	#region protected instance methods
	/// <summary>
	/// 判断指定输入引用是否应使用人类控制器输入。
	/// <para>规则：人类控制器存在，且（非 CPU 控制 或 游戏暂停 或 该输入是 Start 键）。</para>
	/// </summary>
	/// <param name="inputReference">输入引用。</param>
	/// <returns>true 表示使用人类控制器输入。</returns>
	protected bool UseHumanController(InputReferences inputReference){
		return
			this.humanController != null &&
			(
				!(this.isCPU && UFE.gameRunning)
				||
				UFE.isPaused()
				||
				// Even if the character is being controlled by the CPU,
				// we want to listen "Start" button events from the player controller
				inputReference.inputType == InputType.Button && inputReference.engineRelatedButton == ButtonPress.Start 
			);
	}
	#endregion
}
