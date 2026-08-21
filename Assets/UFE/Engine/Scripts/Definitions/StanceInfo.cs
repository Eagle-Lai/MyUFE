using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFE3D;

/// <summary>
/// 姿态信息定义（StanceInfo）。
/// <para>用途：描述角色在某个战斗姿态（CombatStances）下的招式集合，包括开场/退场演出动画、基础动作和攻击招式列表。</para>
/// <para>运行时通过 <see cref="ConvertData"/> 转换为 <see cref="MoveSetData"/> 供角色控制器使用。</para>
/// </summary>
namespace UFE3D
{
	/// <summary>
	/// 姿态信息：一个战斗姿态（如普通站立/特殊架势）下所有招式数据的容器。
	/// <para>对应 Unity 资产 .asset，可在 UFE 编辑器中编辑。</para>
	/// </summary>
	[System.Serializable]
	public class StanceInfo : ScriptableObject
	{
		/// <summary>
		/// 该姿态所属的战斗姿态类型（枚举 CombatStances，如姿态1/姿态2...）。
		/// </summary>
		public CombatStances combatStance = CombatStances.Stance1;

		/// <summary>
		/// 进入该姿态时的电影化演出招式（MoveInfo 资产引用）。
		/// </summary>
		public MoveInfo cinematicIntro;

		/// <summary>
		/// 离开该姿态时的电影化退场招式（MoveInfo 资产引用）。
		/// </summary>
		public MoveInfo cinematicOutro;

		/// <summary>
		/// 基础动作集合（站立/行走/跳跃/受击等基础招式，BasicMoves 容器）。
		/// </summary>
		public BasicMoves basicMoves = new BasicMoves();

		/// <summary>
		/// 攻击招式列表（普通技/必杀技等所有可攻击招式，MoveInfo 数组）。
		/// </summary>
		public MoveInfo[] attackMoves = new MoveInfo[0];

		/// <summary>
		/// 将当前姿态信息转换为 MoveSetData 数据对象。
		/// </summary>
		/// <returns>包含本姿态全部招式数据的新 MoveSetData 实例。</returns>
		public MoveSetData ConvertData()
		{
			MoveSetData moveSet = new MoveSetData();
			moveSet.combatStance = this.combatStance;
			moveSet.cinematicIntro = this.cinematicIntro;
			moveSet.cinematicOutro = this.cinematicOutro;
			moveSet.basicMoves = this.basicMoves;
			moveSet.attackMoves = this.attackMoves;

			return moveSet;
		}
	}
}
