using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 帧同步游戏历史（FluxGameHistory）。
/// <para>用途：保存逐帧的游戏状态（FluxStates）与该帧输入的循环缓冲，支持按帧号存取状态/输入、前进到指定帧，</para>
/// <para>以及缓冲大小限制（MaxBufferSize，超出后丢弃最旧帧）。是回滚（Rollback）系统的状态存储基础。</para>
/// </summary>
public class FluxGameHistory{
	#region public instance properties
	/// <summary>当前历史帧数。</summary>
	public long Count{
		get{
			return this._history.Count;
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// The frame of the first position of the buffer.
	/// </summary>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>缓冲中第一帧的帧号。</summary>
	public long FirstStoredFrame{get; private set;}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the last stored frame.
	/// </summary>
	/// <value>The last stored frame.</value>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>缓冲中最后一帧的帧号。</summary>
	public long LastStoredFrame{
		get{
			return this.FirstStoredFrame + this._history.Count - 1;
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// If this property contains a positive value, it will be the max size of the history. 
	/// If it contains a number lesser than or equal to zero, it means the history buffer doesn't have any limit.
	/// </summary>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>缓冲最大容量（≤0 表示无限制）。</summary>
	public long MaxBufferSize{get; private set;}
	#endregion

	#region protected instance 
	/// <summary>历史记录列表（帧状态, 帧输入）。</summary>
	protected List<KeyValuePair<FluxStates, FluxFrameInput>> _history = new List<KeyValuePair<FluxStates, FluxFrameInput>>();
	#endregion

	#region public instance methods
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Initializes a new instance of the <see cref="BUM.Runtime.GameEngine.GameHistory"/> class.
	/// </summary>
	/// <param name="firstFrame">First frame.</param>
	/// <param name="bufferSize">Max buffer size.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>初始化（首帧0）。</summary>
	public virtual void Initialize(){
		this.Initialize(0);
	}

	/// <summary>初始化（指定首帧）。</summary>
	/// <param name="firstFrame">首帧号。</param>
	public virtual void Initialize(long firstFrame){
		this.Initialize(firstFrame, -1);
	}

	/// <summary>初始化（指定首帧与缓冲上限）。</summary>
	/// <param name="firstFrame">首帧号。</param>
	/// <param name="maxBufferSize">最大缓冲大小。</param>
	public virtual void Initialize(long firstFrame, int maxBufferSize){
		this.FirstStoredFrame = firstFrame;
		this.MaxBufferSize = maxBufferSize;

		// Reserve space in the history buffer 
		this._history.Clear();
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Determines whether the input buffer is empty.
	/// </summary>
	/// <returns><c>true</c> if the input buffer is empty; otherwise, <c>false</c>.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>判断历史缓冲是否为空。</summary>
	/// <returns>为空返回 true。</returns>
	public bool IsBufferEmpty(){
		return this._history.Count == 0;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Determines whether the input buffer is full.
	/// </summary>
	/// <returns><c>true</c> if the input buffer is full; otherwise, <c>false</c>.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>判断历史缓冲是否已满。</summary>
	/// <returns>已满返回 true。</returns>
	public bool IsBufferFull(){
		return this.MaxBufferSize > 0 && this._history.Count >= this.MaxBufferSize;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the state of the game at the specified frame.
	/// </summary>
	/// <returns>The state.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>尝试获取指定帧的游戏状态。</summary>
	/// <param name="frame">帧号。</param>
	/// <param name="state">输出状态。</param>
	/// <returns>找到返回 true。</returns>
	public bool TryGetState(long frame, out FluxStates state){
		KeyValuePair<FluxStates, FluxFrameInput> pair;
		bool result = this.TryGetStateAndInput(frame, out pair);
		state = pair.Key;
		return result;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the state at the specified frame.
	/// </summary>
	/// <returns>The state.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>获取指定帧的游戏状态（不存在时越界异常）。</summary>
	/// <param name="frame">帧号。</param>
	/// <returns>游戏状态。</returns>
    public FluxStates GetState(long frame) {
		return this.GetStateAndInput(frame).Key;
    }

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the state of the game at the specified frame.
	/// </summary>
	/// <returns>The state.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>尝试获取指定帧的状态与输入。</summary>
	/// <param name="frame">帧号。</param>
	/// <param name="stateAndInput">输出（状态, 输入）对。</param>
	/// <returns>找到返回 true。</returns>
	public bool TryGetStateAndInput(long frame, out KeyValuePair<FluxStates, FluxFrameInput> stateAndInput){
		int index = this.GetIndex(frame);

		if (this.IsValidIndex(index)){
			stateAndInput = this._history[index];
			return true;
		}

		stateAndInput = new KeyValuePair<FluxStates, FluxFrameInput>(new FluxStates(), new FluxFrameInput());
		return false;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the state at the specified frame.
	/// </summary>
	/// <returns>The state.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>获取指定帧的状态与输入（按帧号线性查找）。</summary>
	/// <param name="frame">帧号。</param>
	/// <returns>（状态, 输入）对。</returns>
	public KeyValuePair<FluxStates, FluxFrameInput> GetStateAndInput(long frame) {
		return this._history[this.GetFrameIndex(frame)];
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the player inputs at the specified frame.
	/// </summary>
	/// <returns>The player inputs.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>尝试获取指定帧的双方输入。</summary>
	/// <param name="frame">帧号。</param>
	/// <param name="input">输出输入。</param>
	/// <returns>找到返回 true。</returns>
	public bool TryGetInput(long frame, out FluxFrameInput input){
		KeyValuePair<FluxStates, FluxFrameInput> pair;
		bool result = this.TryGetStateAndInput(frame, out pair);
		input = pair.Value;
		return result;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the player inputs at the specified frame.
	/// </summary>
	/// <returns>The player inputs.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>获取指定帧的双方输入。</summary>
	/// <param name="frame">帧号。</param>
	/// <returns>双方输入。</returns>
	public FluxFrameInput GetInput(long frame) {
		return this.GetStateAndInput(frame).Value;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Go to the specified frame, removing the existing frames from the buffer if necessary.
	/// </summary>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>前进到指定帧（移除最早帧直至缓冲首帧等于目标帧）。</summary>
	/// <param name="frame">目标帧。</param>
	/// <returns>目标帧未过期返回 true。</returns>
	public bool TryGoToFrame(long frame){
		// Check if we have already passed the specified frame...
		if (this.FirstStoredFrame > frame){
			return false;
		}

		// If we haven't reached the specified frame yet, remove the first frame of the buffer 
		// and add a new frame to the end of the buffer until we reach the specified frame...
		while (this.FirstStoredFrame < frame){
			this.RemoveNextFrame();
		}

		return true;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Sets the state of the game at the specified frame.
	/// </summary>
	/// <returns>Whether the state could be set successfully.</returns>
	/// <param name="frame">Frame.</param>
	/// <param name="state">The state.</param>
	/// <param name="input">The input that was applied to the specified state to get the next state.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>设置（添加/覆盖）一帧的状态与输入。</summary>
	/// <param name="state">游戏状态。</param>
	/// <param name="input">该帧输入。</param>
	/// <returns>设置成功返回 true。</returns>
	public bool TrySetState(FluxStates state, FluxFrameInput input){
		return this.TrySetState(new KeyValuePair<FluxStates, FluxFrameInput>(state, input));
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Sets the state of the game at the specified frame.
	/// </summary>
	/// <returns><c>true</c>, if set state was tryed, <c>false</c> otherwise.</returns>
	/// <param name="state">The game state (including the player inputs).</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>设置（添加/覆盖）一帧的状态与输入（完整参数）。</summary>
	/// <param name="state">游戏状态与输入的键值对。</param>
	/// <returns>设置成功返回 true。</returns>
	public bool TrySetState(KeyValuePair<FluxStates, FluxFrameInput> state){
		int index = this.GetIndex(state.Key.networkFrame);

		if (index == this._history.Count && !this.IsBufferFull()){
			this._history.Add(state);
			return true;
		}else if (this.IsValidIndex(index)){
			this._history[index] = state;
			return true;
		}

		return false;
	}
	#endregion

	#region protected instance methods
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the frame associated to the specified index... assuming that the index has a valid value.
	/// </summary>
	/// <returns>The frame.</returns>
	/// <param name="index">Index.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>将索引转换为帧号（假设索引有效）。</summary>
	/// <param name="index">索引。</param>
	/// <returns>帧号。</returns>
	protected long GetFrame(int index){
		return (long)(index) + this.FirstStoredFrame;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the index where the information of the specified frame will be stored in the input buffer...
	/// assuming that the returned index has a valid value.
	/// </summary>
	/// <returns>The index.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>将帧号转换为缓冲索引（假设索引有效）。</summary>
	/// <param name="frame">帧号。</param>
	/// <returns>索引。</returns>
	protected int GetIndex(long frame){
		return (int)(frame - this.FirstStoredFrame);
	}

	/// <summary>按帧号线性查找缓冲索引。</summary>
	/// <param name="frame">帧号。</param>
	/// <returns>索引；未找到返回 -1。</returns>
    protected int GetFrameIndex(long frame) {
        int i = 0;
		foreach (KeyValuePair<FluxStates, FluxFrameInput> pair in this._history) {
			if (frame == pair.Key.networkFrame) return i;
            i++;
        }
        return - 1;
    }

	/// <summary>判断索引是否有效。</summary>
	/// <param name="index">索引。</param>
	/// <returns>有效返回 true。</returns>
	protected bool IsValidIndex(int index){
		return index >= 0 && index < this._history.Count;
	}

	/// <summary>判断指定帧号是否存在于缓冲中。</summary>
	/// <param name="frame">帧号。</param>
	/// <returns>存在返回 true。</returns>
    protected bool IsValidFrame(int frame) {
		foreach (KeyValuePair<FluxStates, FluxFrameInput> pair in this._history) {
			if (frame == pair.Key.networkFrame) return true;
        }
        return false;
    }

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Move the buffer to the next frame.
	/// </summary>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>移除缓冲中的最早一帧并递增首帧号。</summary>
	/// <returns>移除成功返回 true。</returns>
	public bool RemoveNextFrame(){
		if (this._history.Count > 0){
			this._history.RemoveAt(0);
			++this.FirstStoredFrame;

			return true;
		}
		return false;
	}
	#endregion
}
