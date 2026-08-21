using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 简单 AI（SimpleAI）。
/// <para>用途：实现可编程的"剧本式" AI——按 SimpleAIBehaviour 中定义的按键序列（SimpleAIStep）逐帧模拟输入，</para>
/// <para>在角色处于待机/下蹲状态且无招式执行时，按步骤生成未来若干帧的输入缓冲。</para>
/// <para>用于演示/教学或挑战模式的固定连段对手。</para>
/// </summary>
public class SimpleAI : AbstractInputController{
	#region public instance fields
	/// <summary>AI 行为资产（步骤序列）。</summary>
	public SimpleAIBehaviour behaviour;
	#endregion

	#region protected instance fields: Cached information to improve performance
	/// <summary>空按钮序列（缓存）。</summary>
	protected ButtonPress[] noButtonsPressed = new ButtonPress[0];
	/// <summary>输入缓冲列表（buffer[0]=上一帧、buffer[1]=当前帧、后续=未来帧）。</summary>
	protected List<Dictionary<InputReferences, InputEvents>> inputBuffer;
	#endregion

	#region overriden methods
	/// <summary>
	/// 初始化：创建输入缓冲（至少 2 帧），为每个输入引用建立默认输入。
	/// </summary>
	/// <param name="inputs">输入引用列表。</param>
	public override void Initialize (IEnumerable<InputReferences> inputs){
//		UFE.OnRoundBegins -= OnRoundBegins;
//		UFE.OnRoundBegins += OnRoundBegins;

		//-------------------------------------------------
		// We need at least a buffer of 2 positions:
		// + buffer[0] -------> previous Input
		// + buffer[1] -------> current Input
		// + buffer[i > 1] ---> future Inputs 
		//-------------------------------------------------
		int bufferSize = 2;

		this.inputBuffer = new List<Dictionary<InputReferences, InputEvents>>();
		for (int i = 0; i < bufferSize; ++i){
			this.inputBuffer.Add(new Dictionary<InputReferences, InputEvents>());
		}

		if (inputs != null){
			foreach (InputReferences input in inputs){
				if (input != null){
					for (int i = 0; i < bufferSize; ++i){
						this.inputBuffer[i][input] = InputEvents.Default;
					}
				}
			}
		}

		base.Initialize (inputs);
	}

	/// <summary>
	/// 固定帧更新：维护输入缓冲，并在角色空闲时按行为步骤序列生成未来输入帧。
	/// <para>方向按键根据与对手的相对位置自动转向（前进=朝对手）。</para>
	/// </summary>
	public override void DoFixedUpdate(){
		//this.ShowDebugInformation();


		ControlsScript self = UFE.GetControlsScript(this.player);
		if (this.inputReferences != null && this.inputBuffer != null && self != null){
			ControlsScript opponent = self.opControlsScript;
			if (opponent != null){
				//-------------------------------------------------------------------------------------------------
				// Check the information stored in the input buffer...
				//-------------------------------------------------------------------------------------------------
				if (this.inputBuffer.Count == 0){
					//---------------------------------------------------------------------------------------------
					// If the we don't have the input of the previous frame, use the default input...
					//---------------------------------------------------------------------------------------------
					Dictionary<InputReferences, InputEvents> frame = new Dictionary<InputReferences, InputEvents>();
					foreach (InputReferences input in this.inputReferences){
						frame[input] = InputEvents.Default;
					}
					this.inputBuffer.Add(frame);
				}else if (this.inputBuffer.Count >= 2){
					this.inputBuffer.RemoveAt(0);
				}

				//-----------------------------------------------------------------------------------------------------
				// If we haven't decided the input for the current frame yet...
				//-----------------------------------------------------------------------------------------------------
				if (this.inputBuffer.Count < 2){
					//-------------------------------------------------------------------------------------------------
					// And simulate the input required for executing the next movement
					//-------------------------------------------------------------------------------------------------
					if(
						this.behaviour != null 
						&& 
						this.behaviour.steps.Length > 0 
						&&
						self.currentMove == null 
						&&
						(
							self.currentBasicMove == BasicMoveReference.Idle || 
							self.currentBasicMove == BasicMoveReference.Crouching
						)
					){
						float sign = Mathf.Sign(opponent.transform.position.x - self.transform.position.x);

						foreach (SimpleAIStep step in this.behaviour.steps){
							Dictionary<InputReferences,InputEvents> frame = new Dictionary<InputReferences,InputEvents>();
							foreach (InputReferences input in this.inputReferences){
								frame[input] = InputEvents.Default;
							}

							foreach (InputReferences input in this.inputReferences){
								if (input.inputType == InputType.HorizontalAxis){
									foreach (ButtonPress buttonPress in step.buttons){
										if (buttonPress == ButtonPress.Back){
											frame[input] = new InputEvents(-1f * sign);
										}else if (buttonPress == ButtonPress.Forward){
											frame[input] = new InputEvents(1f * sign);
										}
									}
								}else if (input.inputType == InputType.VerticalAxis){
									foreach (ButtonPress buttonPress in step.buttons){
										if (buttonPress == ButtonPress.Up){
											frame[input] = new InputEvents(1f);
										}else if (buttonPress == ButtonPress.Down){
											frame[input] = new InputEvents(-1f);
										}
									}
								}else{
									foreach (ButtonPress buttonPress in step.buttons){
										if (input.engineRelatedButton == buttonPress){
											frame[input] = new InputEvents(true);
										}
									}
								}
							}

							for (int i = 0; i < step.frames; ++i){
								this.inputBuffer.Add(frame);
							}
						}
					}else{
						Dictionary<InputReferences, InputEvents> frame = new Dictionary<InputReferences, InputEvents>();
						foreach (InputReferences input in this.inputReferences){
							frame[input] = InputEvents.Default;
						}
						this.inputBuffer.Add(frame);
					}
				}
			}
		}
	}

	/// <summary>
	/// 每帧更新（空实现，输入由 DoFixedUpdate 缓冲驱动）。
	/// </summary>
	public override void DoUpdate(){}

	/// <summary>
	/// 读取输入：缓冲足够时返回缓冲中的输入，否则返回默认输入。
	/// </summary>
	/// <param name="inputReference">输入引用。</param>
	/// <returns>输入事件。</returns>
	public override InputEvents ReadInput (InputReferences inputReference){
		if(
			this.behaviour != null && 
			this.inputReferences != null && 
			this.inputBuffer != null && 
			this.inputBuffer.Count >= 2
		){
			return this.inputs[inputReference];
		}
		return InputEvents.Default;
	}
	#endregion

	#region protected instance methods

	#endregion
}
