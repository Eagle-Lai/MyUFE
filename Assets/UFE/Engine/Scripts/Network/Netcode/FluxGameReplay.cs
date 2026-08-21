/// <summary>
/// 帧同步录像（FluxGameReplay）。
/// <para>用途：保存一场网络对战的初始状态与双方输入缓冲，用于战斗回放（Replay）。</para>
/// </summary>
public class FluxGameReplay{
	#region public instance properties
	/// <summary>录像的初始游戏状态。</summary>
	public FluxStates InitialState {get; set;}
	/// <summary>玩家1的输入缓冲（逐帧输入历史）。</summary>
	public FluxPlayerInputBuffer Player1InputBuffer{get; set;}
	/// <summary>玩家2的输入缓冲（逐帧输入历史）。</summary>
	public FluxPlayerInputBuffer Player2InputBuffer{get; set;}
	#endregion
}
