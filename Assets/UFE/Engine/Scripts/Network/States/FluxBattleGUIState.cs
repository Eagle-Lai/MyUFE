using UnityEngine.UI;
using System.Collections.Generic;


/// <summary>
/// 战斗 HUD 帧同步状态（FluxBattleGUIState）。
/// <para>用途：保存战斗 HUD 在帧同步中需要追踪的状态（如训练模式的输入引用历史），用于状态快照/回滚。</para>
/// </summary>
public class FluxBattleGUIState{
	#region public instance properties
	//public List<List<Image>> player1ButtonPresses{get; set;}
	//public List<List<Image>> player2ButtonPresses{get; set;}

	/// <summary>玩家1训练模式输入引用历史。</summary>
	public List<InputReferences[]> player1InputReferences{get;set;}
	/// <summary>玩家2训练模式输入引用历史。</summary>
	public List<InputReferences[]> player2InputReferences{get;set;}
	#endregion
}
