using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 帧同步玩家（FluxPlayer）。
/// <para>用途：表示网络对战中的一名玩家（本地/远端），持有其输入缓冲并绑定到对应的控制器与控制脚本。</para>
/// </summary>
public class FluxPlayer{
	#region public instance properties
	/// <summary>该玩家的输入缓冲。</summary>
	public FluxPlayerInputBuffer inputBuffer{
		get{
			return this._inputBuffer;
		}
	}

	/// <summary>该玩家的输入控制器（UFEController）。</summary>
	public UFEController inputController{
		get{
			return UFE.GetController(this.player);
		}
	}

	/// <summary>是否为本地玩家。</summary>
	public bool isLocalPlayer{
		get{
//			int localPlayer = UFE.GetLocalPlayer();
//			return localPlayer <= 0 && localPlayer == this.player;

			return !this.isRemotePlayer;
		}
	}

	/// <summary>是否为远端玩家。</summary>
	public bool isRemotePlayer{
		get{
			return this.player == UFE.GetRemotePlayer();
		}
	}

	/// <summary>该玩家的角色控制脚本。</summary>
	public ControlsScript controlsScript{
		get{
			return UFE.GetControlsScript(this.player);
		}
	}

	/// <summary>该玩家的编号（1 或 2）。</summary>
	public int player{
		get{
			return this._player;
		}
	}
	#endregion

	#region private instance properties
	/// <summary>输入缓冲内部引用。</summary>
	public FluxPlayerInputBuffer _inputBuffer = new FluxPlayerInputBuffer();
	/// <summary>玩家编号内部引用。</summary>
	public int _player;
	#endregion

	#region public instance constructors
	/// <summary>构造函数（默认帧号0，无缓冲上限）。</summary>
	/// <param name="player">玩家编号。</param>
	public FluxPlayer(int player) : this(player, 0){}

	/// <summary>构造函数（指定当前帧号）。</summary>
	/// <param name="player">玩家编号。</param>
	/// <param name="currentFrame">当前帧号。</param>
	public FluxPlayer(int player, int currentFrame) : this(player, currentFrame, -1){}

	/// <summary>构造函数（完整参数）。</summary>
	/// <param name="player">玩家编号。</param>
	/// <param name="currentFrame">当前帧号。</param>
	/// <param name="maxBufferSize">最大缓冲大小（-1 表示无限制）。</param>
	public FluxPlayer(int player, int currentFrame, int maxBufferSize){
		this._player = player;
		this.Initialize(currentFrame, maxBufferSize);
	}
	#endregion

	#region public instance methods
	/// <summary>初始化（帧号0）。</summary>
	public virtual void Initialize(){
		this.Initialize(0);
	}

	/// <summary>初始化（指定帧号）。</summary>
	/// <param name="currentFrame">当前帧号。</param>
	public virtual void Initialize(long currentFrame){
		this.Initialize(currentFrame, -1);
	}

	/// <summary>初始化（指定帧号与缓冲上限）。</summary>
	/// <param name="currentFrame">当前帧号。</param>
	/// <param name="maxBufferSize">最大缓冲大小。</param>
	public virtual void Initialize(long currentFrame, int maxBufferSize){
		this._inputBuffer.Initialize(currentFrame, maxBufferSize);
	}

	/// <summary>移除输入缓冲中的下一个输入。</summary>
	/// <returns>移除成功返回 true。</returns>
	public bool RemoveNextInput(){
		return this._inputBuffer.RemoveNextInput();
	}
	#endregion
}
