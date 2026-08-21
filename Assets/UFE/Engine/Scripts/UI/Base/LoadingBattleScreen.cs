using UnityEngine;
using System.Collections;

/// <summary>
/// 战斗加载界面（LoadingBattleScreen）。
/// <para>用途：进入战斗前的加载/过渡界面基类，提供开始战斗的方法。</para>
/// </summary>
public class LoadingBattleScreen : UFEScreen {
	#region public instance methods
	/// <summary>
	/// 开始战斗（使用配置的游戏淡出时长）。
	/// </summary>
	public virtual void StartBattle(){
		UFE.StartGame((float)UFE.config.gameGUI.gameFadeDuration);
	}
	#endregion
}
