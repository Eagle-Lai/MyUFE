using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 随机 AI（RandomAI）。
/// <para>用途：基于概率决策的简单 AI——按输入频率周期性地根据与对手的距离行为配置（AIDistanceBehaviour），</para>
/// <para>随机生成移动/跳跃/下蹲/攻击输入。距离行为配置定义在各距离范围内的动作概率。</para>
/// </summary>
public class RandomAI : AbstractInputController {
	#region protected instance fields
	/// <summary>上次决策时间（用于控制输入频率）。</summary>
	protected float timeLastDecision = float.NegativeInfinity;
	#endregion

	#region public override methods
	/// <summary>
	/// 初始化：重置决策计时并调用基类初始化。
	/// </summary>
	/// <param name="inputs">输入引用列表。</param>
	public override void Initialize (IEnumerable<InputReferences> inputs){
		this.timeLastDecision = float.NegativeInfinity;
		base.Initialize (inputs);
	}

	/// <summary>
	/// 每帧更新：按输入频率周期性地生成随机输入；未到决策间隔时不输入。
	/// </summary>
	public override void DoUpdate (){
		if (this.inputReferences != null){
			//---------------------------------------------------------------------------------------------------------
			// Check the time that has passed since the last update.
			//---------------------------------------------------------------------------------------------------------
			float currentTime = Time.realtimeSinceStartup;

			if (this.timeLastDecision < 0f){
				this.timeLastDecision = currentTime;
			}

			//---------------------------------------------------------------------------------------------------------
			// If the time since the last update is greater than the input frequency, read the AI input.
			// Otherwise, don't press any input.
			//---------------------------------------------------------------------------------------------------------
			this.inputs.Clear();
			if (currentTime - this.timeLastDecision >= UFE.config.aiOptions.inputFrequency){
				this.timeLastDecision = currentTime;

				foreach (InputReferences input in this.inputReferences) {
					this.inputs[input] = this.ReadInput(input);
				}
			}else{
				foreach (InputReferences input in this.inputReferences) {
					this.inputs[input] = InputEvents.Default;
				}
			}
		}
	}

	/// <summary>
	/// 读取输入：根据与对手的距离查找对应的距离行为配置，按概率生成方向/跳跃/下蹲/攻击输入。
	/// </summary>
	/// <param name="inputReference">输入引用。</param>
	/// <returns>生成的输入事件。</returns>
	public override InputEvents ReadInput (InputReferences inputReference){
		ControlsScript self = UFE.GetControlsScript(this.player);
		if (self != null){
			ControlsScript opponent = self.opControlsScript;
			
			if (opponent != null){
				bool isOpponentDown = opponent.currentState == PossibleStates.Down;
				float dx = opponent.transform.position.x - self.transform.position.x;
				int distance = Mathf.RoundToInt(100f * Mathf.Clamp01((float)self.normalizedDistance));

				float maxDistance = float.NegativeInfinity;
				AIDistanceBehaviour behaviour = null;

				// Try to find the correct "Distance Behaviour"
				// If there are several overlapping "Distance Behaviour", we choose the first in the list.
				foreach(AIDistanceBehaviour thisBehaviour in UFE.config.aiOptions.distanceBehaviour){
					if (thisBehaviour != null){
						if (distance >= thisBehaviour.proximityRangeBegins && distance <= thisBehaviour.proximityRangeEnds){
							behaviour = thisBehaviour;
							break;
						}

						if (thisBehaviour.proximityRangeEnds > maxDistance){
							maxDistance = thisBehaviour.proximityRangeEnds;
						}
					}
				}

				// If we don't find the correct "Distance Behaviour", make our best effort...
				if (behaviour == null){
					foreach(AIDistanceBehaviour thisBehaviour in UFE.config.aiOptions.distanceBehaviour){
						if (thisBehaviour != null && thisBehaviour.proximityRangeEnds == maxDistance){
							behaviour = thisBehaviour;
						}
					}
				}

				if (behaviour == null){
					return InputEvents.Default;
				}else if (inputReference.inputType == InputType.HorizontalAxis) {
					float axis = 0f;
					if (UFE.config.aiOptions.moveWhenEnemyIsDown || !isOpponentDown){
						axis =
							Mathf.Sign(dx)
							*
							(
								(Random.Range (0f, 1f) < behaviour.movingForwardProbability ? 1f : 0f) -
								(Random.Range (0f, 1f) < behaviour.movingBackProbability ? 1f : 0f)
							);
					}
					
					return new InputEvents (axis);
				} else if (inputReference.inputType == InputType.VerticalAxis) {
					float axis = 0f;
					if (UFE.config.aiOptions.moveWhenEnemyIsDown || !isOpponentDown){
						axis = 
							(Random.Range (0f, 1f) < behaviour.jumpingProbability ? 1f : 0f) -
							(Random.Range (0f, 1f) < behaviour.movingBackProbability ? 1f : 0f);
					}
					
					return new InputEvents (axis);
				}else{
					if (!UFE.config.aiOptions.attackWhenEnemyIsDown && isOpponentDown){
						return InputEvents.Default;
					} else if (inputReference.engineRelatedButton == ButtonPress.Button1) {
						return new InputEvents (Random.Range (0f, 1f) < behaviour.attackProbability);
					} else if (inputReference.engineRelatedButton == ButtonPress.Button2) {
						return new InputEvents (Random.Range (0f, 1f) < behaviour.attackProbability);
					} else if (inputReference.engineRelatedButton == ButtonPress.Button3) {
						return new InputEvents (Random.Range (0f, 1f) < behaviour.attackProbability);
					} else if (inputReference.engineRelatedButton == ButtonPress.Button4) {
						return new InputEvents (Random.Range (0f, 1f) < behaviour.attackProbability);
					} else if (inputReference.engineRelatedButton == ButtonPress.Button5) {
						return new InputEvents (Random.Range (0f, 1f) < behaviour.attackProbability);
					} else if (inputReference.engineRelatedButton == ButtonPress.Button6) {
						return new InputEvents (Random.Range (0f, 1f) < behaviour.attackProbability);
					} else if (inputReference.engineRelatedButton == ButtonPress.Button7) {
						return new InputEvents (Random.Range (0f, 1f) < behaviour.attackProbability);
					} else if (inputReference.engineRelatedButton == ButtonPress.Button8) {
						return new InputEvents (Random.Range (0f, 1f) < behaviour.attackProbability);
					} else if (inputReference.engineRelatedButton == ButtonPress.Button9) {
						return new InputEvents (Random.Range (0f, 1f) < behaviour.attackProbability);
					} else if (inputReference.engineRelatedButton == ButtonPress.Button10) {
						return new InputEvents (Random.Range (0f, 1f) < behaviour.attackProbability);
					} else if (inputReference.engineRelatedButton == ButtonPress.Button11) {
						return new InputEvents (Random.Range (0f, 1f) < behaviour.attackProbability);
					} else if (inputReference.engineRelatedButton == ButtonPress.Button12) {
						return new InputEvents (Random.Range (0f, 1f) < behaviour.attackProbability);
					}else{
						return InputEvents.Default;
					}
				}
			}
		}
		return InputEvents.Default;
	}
	#endregion
}
