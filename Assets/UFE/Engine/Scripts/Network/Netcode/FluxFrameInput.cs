using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 帧输入（FluxFrameInput）。
/// <para>用途：封装一帧内双方（玩家1/玩家2）的上一帧与当前帧输入（FrameInput），供帧同步（FluxCapacitor）驱动战斗。</para>
/// </summary>
public struct FluxFrameInput{
	#region public instance fields
	/// <summary>玩家1上一帧输入。</summary>
	public FrameInput Player1PreviousInput;
	/// <summary>玩家1当前帧输入。</summary>
	public FrameInput Player1CurrentInput;
	/// <summary>玩家2上一帧输入。</summary>
	public FrameInput Player2PreviousInput;
	/// <summary>玩家2当前帧输入。</summary>
	public FrameInput Player2CurrentInput;
	#endregion

	#region public instance constructors
	/// <summary>
	/// 构造函数：创建一帧双方输入数据。
	/// </summary>
	/// <param name="player1PreviousInput">玩家1上一帧输入。</param>
	/// <param name="player1CurrentInput">玩家1当前帧输入。</param>
	/// <param name="player2PreviousInput">玩家2上一帧输入。</param>
	/// <param name="player2CurrentInput">玩家2当前帧输入。</param>
	public FluxFrameInput(
		FrameInput player1PreviousInput, 
		FrameInput player1CurrentInput, 
		FrameInput player2PreviousInput, 
		FrameInput player2CurrentInput
	){
		this.Player1PreviousInput = player1PreviousInput;
		this.Player1CurrentInput = player1CurrentInput;
		this.Player2PreviousInput = player2PreviousInput;
		this.Player2CurrentInput = player2CurrentInput;
	}
	#endregion
}
