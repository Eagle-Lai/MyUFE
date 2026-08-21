using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AI 规则生成器（AIRulesGenerator）。
/// <para>用途：为 Fuzzy AI 自动生成模糊推理规则字符串列表——根据配置（移动/跳跃/攻击/格挡的频率与偏好距离），</para>
/// <para>生成符合 AIRule 语法的 IF...THEN 规则（如 距离=近 THEN 前进=可取），供 RuleBasedAI 加载使用。</para>
/// <para>支持调试文本转换（ToDebugInformation）便于阅读规则。</para>
/// </summary>
[Serializable]
public class AIRulesGenerator {
	/// <summary>AI 偏好的交战距离。</summary>
	public CharacterDistance preferableCombatDistance = CharacterDistance.Any;
	/// <summary>在偏好距离处的攻击渴望度。</summary>
	public AIDesirability attacksAtPreferableDistance = AIDesirability.VeryDesirable;
	/// <summary>是否自动移动。</summary>
	public bool autoMove;
	/// <summary>是否在到达位置后待机（静止）。</summary>
	public bool restOnLocation = true;
	/// <summary>移动频率档位。</summary>
	public int moveFrequency = 4;
	/// <summary>是否自动跳跃。</summary>
	public bool autoJump;
	/// <summary>后跳频率。</summary>
	public int jumpBackFrequency = 1;
	/// <summary>垂直跳频率。</summary>
	public int jumpStraightFrequency = 2;
	/// <summary>前跳频率。</summary>
	public int jumpForwardFrequency = 3;
	/// <summary>是否自动格挡。</summary>
	public bool autoBlock;
	/// <summary>格挡规则是否遵循命中类型（上段/下段）。</summary>
	public bool obeyHitType = true;
	/// <summary>站立格挡准确度。</summary>
	public int standBlockAccuracy = 6;
	/// <summary>下蹲格挡准确度。</summary>
	public int crouchBlockAccuracy = 6;
	/// <summary>空中格挡准确度。</summary>
	public int jumpBlockAccuracy = 0;
	/// <summary>是否自动攻击。</summary>
	public bool autoAttack;
	/// <summary>攻击规则是否遵循偏好距离。</summary>
	public bool obeyPreferableDistances = false;
	/// <summary>攻击频率档位。</summary>
	public int attackFrequency = 5;
	/// <summary>调试开关。</summary>
	public bool debugToggle;
	
	/// <summary>
	/// 将浮点数值转换为渴望度枚举（按预定义阈值分级）。
	/// </summary>
	/// <param name="value">渴望度数值。</param>
	/// <returns>渴望度枚举。</returns>
	public AIDesirability GetDesirabilityValue(float value){
		DesirabilityDefinitions desirability = new DesirabilityDefinitions();
		if (value >= desirability.theBestOption) return AIDesirability.TheBestOption;
		if (value >= desirability.veryDesirable) return AIDesirability.VeryDesirable;
		if (value >= desirability.desirable) return AIDesirability.Desirable;
		if (value >= desirability.notBad) return AIDesirability.NotBad;
		if (value >= desirability.undesirable) return AIDesirability.Undesirable;
		if (value >= desirability.veryUndesirable) return AIDesirability.VeryUndesirable;
		
		return AIDesirability.TheWorstOption;
	}
	
	/// <summary>
	/// 将整数值转换为渴望度枚举（6=最佳，5=非常可取 ... 1=非常不可取，0=最差）。
	/// </summary>
	/// <param name="value">渴望度数值（0~6）。</param>
	/// <returns>渴望度枚举。</returns>
	public AIDesirability GetDesirabilityValue(int value){
		if (value >= 6) return AIDesirability.TheBestOption;
		if (value == 5) return AIDesirability.VeryDesirable;
		if (value == 4) return AIDesirability.Desirable;
		if (value == 3) return AIDesirability.NotBad;
		if (value == 2) return AIDesirability.Undesirable;
		if (value == 1) return AIDesirability.VeryUndesirable;
		
		return AIDesirability.TheWorstOption;
	}
	
	/// <summary>
	/// 生成全部模糊规则字符串列表：按移动/跳跃/攻击/格挡配置组装 IF...THEN 规则。
	/// </summary>
	/// <returns>模糊规则字符串列表。</returns>
	public List<string> GenerateRules(){
		List<string> fuzzyRules = new List<string>();
		
		if (this.autoMove){
			if (this.restOnLocation) fuzzyRules = addDistanceReaction(fuzzyRules, this.preferableCombatDistance, AIReaction.Idle, this.moveFrequency);
			fuzzyRules = addSystematicRules(fuzzyRules, this.preferableCombatDistance, AIReaction.MoveForward, 1, this.moveFrequency);
			fuzzyRules = addSystematicRules(fuzzyRules, this.preferableCombatDistance, AIReaction.MoveBackward, -1, this.moveFrequency);
			
			if (this.autoJump){
				fuzzyRules = addDistanceReaction(fuzzyRules, this.preferableCombatDistance, AIReaction.JumpStraight, this.jumpStraightFrequency);
				fuzzyRules = addSystematicRules(fuzzyRules, this.preferableCombatDistance, AIReaction.JumpForward, 1, this.jumpForwardFrequency);
				fuzzyRules = addSystematicRules(fuzzyRules, this.preferableCombatDistance, AIReaction.JumpBackward, -1, this.jumpBackFrequency);
			}
		}
		
		if (this.autoJump && !this.autoMove){
			if (this.jumpStraightFrequency > 0){
				fuzzyRules.Add(
					AIRule.Rule_IF							+ 
					AICondition.Health_Self					+ 
					AIRule.Rule_IS							+ 
					AIRule.Rule_NOT							+ 
					HealthStatus.Dead						+ 
					
					AIRule.Rule_THEN									+ 
					AIReaction.JumpStraight								+ 
					AIRule.Rule_IS										+
					GetDesirabilityValue(this.jumpStraightFrequency)
					);
			}
			
			if (this.jumpBackFrequency > 0){
				fuzzyRules.Add(
					AIRule.Rule_IF							+ 
					AICondition.Health_Self					+ 
					AIRule.Rule_IS							+ 
					AIRule.Rule_NOT							+ 
					HealthStatus.Dead						+ 
					
					AIRule.Rule_THEN									+ 
					AIReaction.JumpBackward								+ 
					AIRule.Rule_IS										+
					GetDesirabilityValue(this.jumpBackFrequency)
					);
			}
			
			if (this.jumpForwardFrequency > 0){
				fuzzyRules.Add(
					AIRule.Rule_IF							+ 
					AICondition.Health_Self					+ 
					AIRule.Rule_IS							+ 
					AIRule.Rule_NOT							+ 
					HealthStatus.Dead						+ 
					
					AIRule.Rule_THEN									+ 
					AIReaction.JumpForward								+ 
					AIRule.Rule_IS										+
					GetDesirabilityValue(this.jumpForwardFrequency)
					);
			}
		}
		
		if (this.autoAttack){
			if (this.obeyPreferableDistances){
				fuzzyRules = addSystematicRules(fuzzyRules, CharacterDistance.VeryClose, AIReaction.PlayMove_PreferableDistance_VeryClose, 1, this.attackFrequency, true);
				fuzzyRules = addSystematicRules(fuzzyRules, CharacterDistance.Close, AIReaction.PlayMove_PreferableDistance_Close, 1, this.attackFrequency, true);
				fuzzyRules = addSystematicRules(fuzzyRules, CharacterDistance.Mid, AIReaction.PlayMove_PreferableDistance_Mid, 1, this.attackFrequency, true);
				fuzzyRules = addSystematicRules(fuzzyRules, CharacterDistance.Far, AIReaction.PlayMove_PreferableDistance_Far, 1, this.attackFrequency, true);
				fuzzyRules = addSystematicRules(fuzzyRules, CharacterDistance.VeryFar, AIReaction.PlayMove_PreferableDistance_VeryFar, 1, this.attackFrequency, true);
				
				/*fuzzyRules = addDistanceReaction(fuzzyRules, CharacterDistance.VeryClose, AIReaction.PlayMove_PreferableDistance_VeryClose, this.attackFrequency);
				fuzzyRules = addDistanceReaction(fuzzyRules, CharacterDistance.Close, AIReaction.PlayMove_PreferableDistance_Close, this.attackFrequency);
				fuzzyRules = addDistanceReaction(fuzzyRules, CharacterDistance.Mid, AIReaction.PlayMove_PreferableDistance_Mid, this.attackFrequency);
				fuzzyRules = addDistanceReaction(fuzzyRules, CharacterDistance.Far, AIReaction.PlayMove_PreferableDistance_Far, this.attackFrequency);
				fuzzyRules = addDistanceReaction(fuzzyRules, CharacterDistance.VeryFar, AIReaction.PlayMove_PreferableDistance_VeryFar, this.attackFrequency);
				fuzzyRules = addDistanceReaction(fuzzyRules, CharacterDistance.Any, AIReaction.PlayMove_PreferableDistance, this.aggressiveness);*/
				
			}else{
				if (this.autoMove){
					//fuzzyRules = addDistanceReaction(fuzzyRules, this.preferableCombatDistance, AIReaction.PlayMove_RandomAttack, this.attackFrequency);
					fuzzyRules = addSystematicRules(fuzzyRules, this.preferableCombatDistance, AIReaction.PlayMove_RandomAttack, 1, this.attackFrequency, true);
					
				}else{
					fuzzyRules.Add(
						AIRule.Rule_IF							+ 
						AICondition.Health_Self					+ 
						AIRule.Rule_IS							+ 
						AIRule.Rule_NOT							+ 
						HealthStatus.Dead						+ 
						
						AIRule.Rule_THEN						+ 
						AIReaction.PlayMove_RandomAttack		+ 
						AIRule.Rule_IS							+
						GetDesirabilityValue(this.attackFrequency)
						);
				}
			}
		}
		
		if (this.autoBlock){
			fuzzyRules = addBlockReaction(fuzzyRules, CurrentFrameData.StartupFrames, CurrentFrameData.ActiveFrames, 
			                              AIReaction.StandBlock, this.standBlockAccuracy, this.obeyHitType);
			fuzzyRules = addBlockReaction(fuzzyRules, CurrentFrameData.StartupFrames, CurrentFrameData.ActiveFrames, 
			                              AIReaction.CrouchBlock, this.crouchBlockAccuracy, this.obeyHitType);

			fuzzyRules = addBlockReaction(fuzzyRules, CurrentFrameData.RecoveryFrames, AIReaction.StandBlock, 0);
			fuzzyRules = addBlockReaction(fuzzyRules, CurrentFrameData.RecoveryFrames, AIReaction.CrouchBlock, 0);
			
			/*fuzzyRules.Add(
				AIRule.Rule_IF								+ 
				AICondition.Attacking_Opponent				+ 
				AIRule.Rule_IS								+ 
				AIBoolean.TRUE								+
				
				AIRule.Rule_THEN							+ 
				AIReaction.StandBlock						+ 
				AIRule.Rule_IS								+
				GetDesirabilityValue(this.standBlockAccuracy)
				);

			fuzzyRules.Add(
				AIRule.Rule_IF								+ 
				AICondition.Attacking_Opponent				+ 
				AIRule.Rule_IS								+ 
				AIBoolean.TRUE								+
				
				AIRule.Rule_THEN							+ 
				AIReaction.CrouchBlock						+ 
				AIRule.Rule_IS								+
				GetDesirabilityValue(this.crouchBlockAccuracy)
				);
			
			fuzzyRules.Add(
				AIRule.Rule_IF								+ 
				AICondition.Attacking_Opponent				+ 
				AIRule.Rule_IS								+ 
				AIBoolean.TRUE								+
				
				AIRule.Rule_THEN							+ 
				AIReaction.JumpBlock						+ 
				AIRule.Rule_IS								+
				GetDesirabilityValue(this.jumpBlockAccuracy)
				);*/
		}
		
		return fuzzyRules;
	}
	
	/// <summary>
	/// 添加系统性距离规则（非抛物线版本）。
	/// </summary>
	/// <param name="fuzzyRules">已有规则列表。</param>
	/// <param name="preferableDistance">偏好距离。</param>
	/// <param name="reaction">AI 反应动作。</param>
	/// <param name="multiplier">距离影响乘数。</param>
	/// <param name="frequencyVariant">频率基础值。</param>
	/// <returns>更新后的规则列表。</returns>
	private List<string> addSystematicRules(List<string> fuzzyRules, CharacterDistance preferableDistance, string reaction, int multiplier, int frequencyVariant){
		return addSystematicRules(fuzzyRules, preferableDistance, reaction, multiplier, frequencyVariant, false);
	}
	
	/// <summary>
	/// 添加系统性距离规则：为各距离档位（非常近/近/中/远/非常远）生成一条规则，
	/// 渴望度按偏好距离呈线性或抛物线分布。
	/// </summary>
	/// <param name="fuzzyRules">已有规则列表。</param>
	/// <param name="preferableDistance">偏好距离。</param>
	/// <param name="reaction">AI 反应动作。</param>
	/// <param name="multiplier">距离影响乘数。</param>
	/// <param name="frequencyVariant">频率基础值。</param>
	/// <param name="parabola">是否抛物线分布（中心频率最高）。</param>
	/// <returns>更新后的规则列表。</returns>
	private List<string> addSystematicRules(List<string> fuzzyRules, CharacterDistance preferableDistance, string reaction, int multiplier, int frequencyVariant, bool parabola){
		if (frequencyVariant == 0) return fuzzyRules;
		// predefined values for preferableDistance == CharacterDistance.VeryClose
		int veryCloseVariant = 0;
		int closeVariant = 0;
		int midVariant = 0;
		int farVariant = 0;
		int veryFarVariant = 0;
		int parabolaVariant = parabola? -1: 1;
		
		if (preferableDistance == CharacterDistance.VeryClose){
			veryCloseVariant	= parabola? frequencyVariant : frequencyVariant - 3;
			closeVariant		= frequencyVariant + (1 * multiplier * parabolaVariant);
			midVariant			= frequencyVariant + (2 * multiplier * parabolaVariant);
			farVariant			= frequencyVariant + (3 * multiplier * parabolaVariant);
			veryFarVariant		= frequencyVariant + (5 * multiplier * parabolaVariant);
			
		}else if (preferableDistance == CharacterDistance.Close){
			veryCloseVariant	= frequencyVariant - (1 * multiplier);
			closeVariant		= parabola? frequencyVariant : frequencyVariant - 3;
			midVariant			= frequencyVariant + (1 * multiplier * parabolaVariant);
			farVariant			= frequencyVariant + (2 * multiplier * parabolaVariant);
			veryFarVariant		= frequencyVariant + (3 * multiplier * parabolaVariant);
			
		}else if (preferableDistance == CharacterDistance.Mid){
			veryCloseVariant	= frequencyVariant - (2 * multiplier);
			closeVariant		= frequencyVariant - (1 * multiplier);
			midVariant			= parabola? frequencyVariant : frequencyVariant - 3;
			farVariant			= frequencyVariant + (1 * multiplier * parabolaVariant);
			veryFarVariant		= frequencyVariant + (2 * multiplier * parabolaVariant);
			
		}else if (preferableDistance == CharacterDistance.Far){
			veryCloseVariant	= frequencyVariant - (3 * multiplier);
			closeVariant		= frequencyVariant - (2 * multiplier);
			midVariant			= frequencyVariant - (1 * multiplier);
			farVariant			= parabola? frequencyVariant : frequencyVariant - 3;
			veryFarVariant		= frequencyVariant + (1 * multiplier * parabolaVariant);
			
		}else if (preferableDistance == CharacterDistance.VeryFar){
			veryCloseVariant	= frequencyVariant - (5 * multiplier);
			closeVariant		= frequencyVariant - (3 * multiplier);
			midVariant			= frequencyVariant - (2 * multiplier);
			farVariant			= frequencyVariant - (1 * multiplier);
			veryFarVariant		= parabola? frequencyVariant : frequencyVariant - 3;
		}
		
		//---------------------------------------------------------------------------------------------------------
		// Add Conditions:
		//---------------------------------------------------------------------------------------------------------
		//if (parabola || preferableDistance != CharacterDistance.VeryClose){
			fuzzyRules.Add(
				AIRule.Rule_IF										+ 
				AICondition.Distance_Self							+ 
				AIRule.Rule_IS										+ 
				CharacterDistance.VeryClose							+
				
				AIRule.Rule_THEN									+ 
				reaction											+ 
				AIRule.Rule_IS										+
				GetDesirabilityValue(veryCloseVariant)
				);
		//}
		
		//if (parabola || preferableDistance != CharacterDistance.Close){
			fuzzyRules.Add(
				AIRule.Rule_IF										+ 
				AICondition.Distance_Self							+ 
				AIRule.Rule_IS										+ 
				CharacterDistance.Close								+
				
				AIRule.Rule_THEN									+ 
				reaction											+ 
				AIRule.Rule_IS										+
				GetDesirabilityValue(closeVariant)
				);
		//}
		
		//if (parabola || preferableDistance != CharacterDistance.Mid){
			fuzzyRules.Add(
				AIRule.Rule_IF										+ 
				AICondition.Distance_Self							+ 
				AIRule.Rule_IS										+ 
				CharacterDistance.Mid								+
				
				AIRule.Rule_THEN									+ 
				reaction											+ 
				AIRule.Rule_IS										+
				GetDesirabilityValue(midVariant)
				);
		//}
		
		//if (parabola || preferableDistance != CharacterDistance.Far){
			fuzzyRules.Add(
				AIRule.Rule_IF										+ 
				AICondition.Distance_Self							+ 
				AIRule.Rule_IS										+ 
				CharacterDistance.Far								+
				
				AIRule.Rule_THEN									+ 
				reaction											+ 
				AIRule.Rule_IS										+
				GetDesirabilityValue(farVariant)
				);
		//}
		
		//if (parabola || preferableDistance != CharacterDistance.VeryFar){
			fuzzyRules.Add(
				AIRule.Rule_IF										+ 
				AICondition.Distance_Self							+ 
				AIRule.Rule_IS										+ 
				CharacterDistance.VeryFar							+
				
				AIRule.Rule_THEN									+ 
				reaction											+ 
				AIRule.Rule_IS										+
				GetDesirabilityValue(veryFarVariant)
				);
		//}
		
		return fuzzyRules;
	}
	
	/// <summary>
	/// 添加格挡反应规则（单帧阶段条件版本）。
	/// </summary>
	/// <param name="fuzzyRules">已有规则列表。</param>
	/// <param name="frameData1">对方招式帧阶段条件。</param>
	/// <param name="reaction">格挡反应动作。</param>
	/// <param name="frequency">格挡渴望度值。</param>
	/// <returns>更新后的规则列表。</returns>
	private List<string> addBlockReaction(List<string> fuzzyRules, CurrentFrameData frameData1, string reaction, int frequency){
		fuzzyRules.Add(
			AIRule.Rule_IF								+ 
			AICondition.Attacking_Opponent				+ 
			AIRule.Rule_IS								+ 
			AIBoolean.TRUE								+
			AIRule.Rule_AND								+
			AICondition.Attacking_FrameData_Opponent	+ 
			AIRule.Rule_IS								+ 
			frameData1									+ 
			
			AIRule.Rule_THEN							+ 
			reaction									+ 
			AIRule.Rule_IS								+
			GetDesirabilityValue(frequency)
			);
		
		return fuzzyRules;
	}
	
	/// <summary>
	/// 添加格挡反应规则（双帧阶段条件版本）：可选按命中类型（上段/下段）过滤格挡姿势。
	/// </summary>
	/// <param name="fuzzyRules">已有规则列表。</param>
	/// <param name="frameData1">对方招式帧阶段条件1。</param>
	/// <param name="frameData2">对方招式帧阶段条件2。</param>
	/// <param name="reaction">格挡反应动作。</param>
	/// <param name="frequency">格挡渴望度值。</param>
	/// <param name="obeyHitType">是否按命中类型过滤。</param>
	/// <returns>更新后的规则列表。</returns>
	private List<string> addBlockReaction(List<string> fuzzyRules, CurrentFrameData frameData1, CurrentFrameData frameData2, string reaction, int frequency, bool obeyHitType){
		string hitTypeString = "";
		if (obeyHitType){
			if (reaction == AIReaction.StandBlock){
				hitTypeString  = AIRule.Rule_AND;
				hitTypeString += AIRule.Rule_Open_Parenthesis;
				hitTypeString += AICondition.Attacking_HitType_Opponent + AIRule.Rule_IS + AIRule.Rule_NOT + HitType.Low;
				hitTypeString += AIRule.Rule_AND + AICondition.Attacking_HitType_Opponent + AIRule.Rule_IS + AIRule.Rule_NOT + HitType.Sweep;
				hitTypeString += AIRule.Rule_Close_Parenthesis;
			}else if (reaction == AIReaction.CrouchBlock){
				hitTypeString  = AIRule.Rule_AND;
				hitTypeString += AIRule.Rule_Open_Parenthesis;
				hitTypeString += AICondition.Attacking_HitType_Opponent + AIRule.Rule_IS + AIRule.Rule_NOT + HitType.Overhead;
				hitTypeString += AIRule.Rule_AND + AICondition.Attacking_HitType_Opponent + AIRule.Rule_IS + AIRule.Rule_NOT + HitType.HighKnockdown;
				hitTypeString += AIRule.Rule_Close_Parenthesis;
			}
		}
		fuzzyRules.Add(
			AIRule.Rule_IF								+ 
			AICondition.Attacking_Opponent				+ 
			AIRule.Rule_IS								+ 
			AIBoolean.TRUE								+
			hitTypeString								+
			AIRule.Rule_AND								+
			AIRule.Rule_Open_Parenthesis				+
			AICondition.Attacking_FrameData_Opponent	+ 
			AIRule.Rule_IS								+ 
			frameData1									+ 
			AIRule.Rule_OR								+
			AICondition.Attacking_FrameData_Opponent	+ 
			AIRule.Rule_IS								+ 
			frameData2									+
			AIRule.Rule_Close_Parenthesis				+
			
			AIRule.Rule_THEN							+ 
			reaction									+ 
			AIRule.Rule_IS								+
			GetDesirabilityValue(frequency)
			);
		
		return fuzzyRules;
	}
	
	/// <summary>
	/// 添加距离反应规则：指定距离下执行指定动作（带渴望度）。
	/// </summary>
	/// <param name="fuzzyRules">已有规则列表。</param>
	/// <param name="distance">距离条件。</param>
	/// <param name="reaction">反应动作。</param>
	/// <param name="frequency">渴望度值。</param>
	/// <returns>更新后的规则列表。</returns>
	private List<string> addDistanceReaction(List<string> fuzzyRules, CharacterDistance distance, string reaction, int frequency){
		fuzzyRules.Add(
			AIRule.Rule_IF							+ 
			AICondition.Distance_Self				+ 
			AIRule.Rule_IS							+ 
			distance								+ 
			
			AIRule.Rule_THEN						+ 
			reaction								+ 
			AIRule.Rule_IS							+
			GetDesirabilityValue(frequency)
			);
		
		return fuzzyRules;
	}
	
	/// <summary>
	/// 生成可读的规则调试信息（将规则符号替换为易读文本）。
	/// </summary>
	/// <returns>调试文本行列表。</returns>
	public List<string> ToDebugInformation(){
		List<string> debugInformation = new List<string>();
		List<string> rules = this.GenerateRules();
		
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
}
