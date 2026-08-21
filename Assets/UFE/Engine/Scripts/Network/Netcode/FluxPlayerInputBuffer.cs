using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// 帧同步玩家输入缓冲（FluxPlayerInputBuffer）。
/// <para>用途：保存一名玩家在最近若干帧的输入（FluxPlayerInput，含预测/确认输入）的环形缓冲，</para>
/// <para>提供按帧号存取输入、确认预测输入、覆盖预测输入、检查回滚需求帧等方法，是帧延迟/回滚算法的核心数据结构。</para>
/// </summary>
public class FluxPlayerInputBuffer{
	#region public instance properties
	/// <summary>缓冲中的输入帧数。</summary>
	public long Count{
		get{
			return (long)this._buffer.Count;
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// The frame of the input in the first position of the buffer.
	/// </summary>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>缓冲中第一帧输入的帧号。</summary>
	public long FirstFrame{get; set;}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// The frame of the input in the last position of the buffer.
	/// </summary>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>缓冲中最后一帧输入的帧号。</summary>
	public long LastFrame{
		get{
			return this.FirstFrame + this.Count	- 1;
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// The max size of the buffer. A value equals to or lesser than zero means that there is no limit.
	/// </summary>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>缓冲最大容量（≤0 表示无限制）。</summary>
	public int MaxBufferSize{get; private set;}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the <see cref="FluxPlayerInputBuffer"/> at the specified index.
	/// </summary>
	/// <param name="index">Index.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>按索引访问玩家输入。</summary>
	/// <param name="index">索引。</param>
	/// <returns>该索引处的玩家输入。</returns>
	public FluxPlayerInput this[int index]{
		get{
			return this._buffer[index];
		}
	}
	#endregion

	#region private instance fields
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// The buffer with the player inputs during a few frames.
	/// </summary>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>逐帧玩家输入列表（内部缓冲）。</summary>
	private List<FluxPlayerInput> _buffer = new List<FluxPlayerInput>();
	#endregion

	#region public instance methods
	/// <summary>
	/// 判断指定帧的预测输入与确认输入是否相等。
	/// </summary>
	/// <param name="frame">帧号。</param>
	/// <returns>相等返回 true。</returns>
	public bool ArePredictedAndConfirmedInputsEqual(long frame){
		return this._buffer[this.GetIndex(frame)].ArePredictedAndConfirmedInputsEqual();
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the input buffer with all inputs that has been confirmed by the player.
	/// </summary>
	/// <returns>The input buffer.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>获取从首帧开始的所有已确认输入。</summary>
	/// <returns>确认输入列表（帧号, 输入）。</returns>
	public ReadOnlyCollection<Tuple<long, FrameInput>> GetConfirmedInputBuffer(){
		return this.GetConfirmedInputBuffer(0L);
	}

	/// <summary>
	/// 获取从指定帧开始的所有已确认输入。
	/// </summary>
	/// <param name="firstFrame">起始帧号。</param>
	/// <returns>确认输入列表（帧号, 输入）。</returns>
	public ReadOnlyCollection<Tuple<long, FrameInput>> GetConfirmedInputBuffer(long firstFrame){
		List<Tuple<long, FrameInput>> buffer = new List<Tuple<long, FrameInput>>();

		for (int i = 0; i < this._buffer.Count; ++i){
			long currentFrame = this.GetFrame(i);

			if (currentFrame >= firstFrame){
				FrameInput? input = this._buffer[i].ConfirmedInput;

				if (input != null){
					buffer.Add(new Tuple<long, FrameInput>(currentFrame, input.Value));
				}else{
					break;
				}
			}
		}

		return new ReadOnlyCollection<Tuple<long, FrameInput>>(buffer);
	}

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
	public long GetFrame(int index){
		return (long)(index) + this.FirstFrame;
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
	public int GetIndex(long frame){
		return (int)(frame - this.FirstFrame);
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the input buffer with all inputs that has been predicted by the system or confirmed by the player.
	/// </summary>
	/// <returns>The input buffer.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>获取所有可用输入（预测或确认）。</summary>
	/// <returns>输入列表。</returns>
	public ReadOnlyCollection<FrameInput> GetInputBuffer(){
		List<FrameInput> buffer = new List<FrameInput>();

		for (int i = 0; i < this._buffer.Count; ++i){
			FrameInput? input = this._buffer[i].GetInput();

			if (input != null){
				buffer.Add(input.Value);
			}else{
				break;
			}
		}

		return new ReadOnlyCollection<FrameInput>(buffer);
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the input buffer with all inputs that has been predicted by the system.
	/// </summary>
	/// <returns>The input buffer.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>获取所有预测输入。</summary>
	/// <returns>预测输入列表。</returns>
	public ReadOnlyCollection<FrameInput> GetPredictedInputBuffer(){
		List<FrameInput> buffer = new List<FrameInput>();

		for (int i = 0; i < this._buffer.Count; ++i){
			FrameInput? input = this._buffer[i].PredictedInput;

			if (input != null){
				buffer.Add(input.Value);
			}else{
				break;
			}
		}

		return new ReadOnlyCollection<FrameInput>(buffer);
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Get the first frame where a rollback is required because the predicted input didn't match 
	/// the confirmed input or a negative value if all predicted inputs match the confirmed inputs.
	/// </summary>
	/// <returns>The first frame where the predicted input didn't match the confirmed input.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>获取第一个需要回滚的帧号（预测输入与确认输入不匹配）；全部匹配返回 -1。</summary>
	/// <returns>首个不匹配帧号。</returns>
	public long GetFirstFrameWhereRollbackIsRequired(){
		for (int i = 0; i < this._buffer.Count; ++i){
			FluxPlayerInput input = this._buffer[i];

			if (input != null && input.ConfirmedInput != null && !input.ArePredictedAndConfirmedInputsEqual()){
				return this.GetFrame(i);
			}
		}
		return -1;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the frame of the last input confirmed by the user or a negative value if no input has been confirmed.
	/// </summary>
	/// <returns>The last confirmed frame.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>获取最后一个已确认输入的帧号。</summary>
	/// <returns>最后确认帧号。</returns>
	public long GetLastFrameWithConfirmedInput(){
//		for (int i = this._buffer.Count - 1; i >= 0; --i){
//			if (this._buffer[i] != null && this._buffer[i].IsInputConfirmed()){
//				return this.GetFrame(i);
//			}
//		}
//		return -1L;


		// Return the first frame without a confirmed input
		for (int i = 0; i < this._buffer.Count; ++i){
			if (this._buffer[i] == null || !this._buffer[i].IsInputConfirmed()){
				return this.GetFrame(i) - 1L;
			}
		}

		return this.LastFrame;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the frame of the last input predicted by the system or a negative value if no input has been predicted.
	/// </summary>
	/// <returns>The last confirmed frame.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>获取最后一个预测输入的帧号。</summary>
	/// <returns>最后预测帧号。</returns>
	public long GetLastFrameWithPredictedInput(){
//		for (int i = this._buffer.Count - 1; i >= 0; --i){
//			if (this._buffer[i] != null && this._buffer[i].IsInputPredicted()){
//				return this.GetFrame(i);
//			}
//		}
//		return -1;


		// Return the first frame without a predicted input
		for (int i = 0; i < this._buffer.Count; ++i){
			if (this._buffer[i] == null || !this._buffer[i].IsInputPredicted()){
				return this.GetFrame(i) - 1L;
			}
		}

		return this.LastFrame;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Gets the frame of the last input confirmed by the user or a negative value if no input has been confirmed.
	/// </summary>
	/// <returns>The last confirmed frame.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>获取最后一个就绪输入（预测或确认）的帧号。</summary>
	/// <returns>最后就绪帧号。</returns>
	public long GetLastFrameWithReadyInput(){
//		for (int i = this._buffer.Count - 1; i >= 0; --i){
//			if (this._buffer[i] != null && this._buffer[i].IsInputReady()){
//				return this.GetFrame(i);
//			}
//		}
//		return -1;

		// Return the first frame without a predicted input
		for (int i = 0; i < this._buffer.Count; ++i){
			if (this._buffer[i] == null || !this._buffer[i].IsInputReady()){
				return this.GetFrame(i) - 1L;
			}
		}

		return this.LastFrame;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Initializes a new instance of the <see cref="BUM.Runtime.GameEngine.PlayerInputBuffer"/> class.
	/// </summary>
	/// <param name="maxBufferSize">Max buffer size.</param>
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
		this.FirstFrame = firstFrame;
		this.MaxBufferSize = maxBufferSize > 0 ? maxBufferSize : -1;
		this._buffer.Clear();
	}

	/// <summary>判断缓冲是否为空。</summary>
	/// <returns>为空返回 true。</returns>
	public bool IsEmpty(){
		return this._buffer.Count == 0;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Determines whether the input buffer is full.
	/// </summary>
	/// <returns><c>true</c> if the input buffer is full; otherwise, <c>false</c>.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>判断缓冲是否已满。</summary>
	/// <returns>已满返回 true。</returns>
	public bool IsFull(){
		return this.MaxBufferSize > 0 && this._buffer.Count == this.MaxBufferSize;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Remove the inputs associated to all existing frames until reaching the specified frame.
	/// </summary>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>移除直至指定帧为止的输入（前进缓冲）。</summary>
	/// <param name="frame">目标帧号。</param>
	/// <returns>目标帧未过期返回 true。</returns>
	public bool RemoveInputsUntilFrame(long frame){
		// Check if we have already passed the specified frame...
		if (this.FirstFrame > frame){
			return false;
		}

		// If we haven't reached the specified frame yet, remove the first frame of the buffer 
		// and add a new frame to the end of the buffer until we reach the specified frame...
		while (this.FirstFrame < frame){
			this.RemoveNextInput();
		}

		return true;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Remove the next input of the buffer.
	/// </summary>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>移除缓冲中的下一个（最早）输入。</summary>
	/// <returns>移除成功返回 true。</returns>
	public bool RemoveNextInput(){
		if (this._buffer.Count > 0){
			this._buffer.RemoveAt(0);
			++this.FirstFrame;

			return true;
		}
		return false;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Determines whether this instance is confirmed.
	/// </summary>
	/// <returns><c>true</c> if this instance is confirmed; otherwise, <c>false</c>.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>尝试检查指定帧输入是否已确认。</summary>
	/// <param name="frame">帧号。</param>
	/// <param name="isConfirmed">输出是否已确认。</param>
	/// <returns>帧存在返回 true。</returns>
	public bool TryCheckIfInputIsConfirmed(long frame, out bool isConfirmed){
		int index = this.GetIndex(frame);

		if (index >= 0 && index < this._buffer.Count){
			isConfirmed = this._buffer[this.GetIndex(frame)].IsInputConfirmed();
			return true;
		}

		isConfirmed = false;
		return false;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Determines whether this instance is predicted.
	/// </summary>
	/// <returns><c>true</c> if this instance is predicted; otherwise, <c>false</c>.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>尝试检查指定帧输入是否为预测输入。</summary>
	/// <param name="frame">帧号。</param>
	/// <param name="isPredicted">输出是否预测输入。</param>
	/// <returns>帧存在返回 true。</returns>
	public bool TryCheckIfInputIsPredicted(long frame, out bool isPredicted){
		int index = this.GetIndex(frame);

		if (index >= 0 && index < this._buffer.Count){
			isPredicted = this._buffer[this.GetIndex(frame)].IsInputPredicted();
			return true;
		}

		isPredicted = false;
		return false;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Determines whether this instance is ready because all players have at least a predicted or confirmed input.
	/// </summary>
	/// <returns><c>true</c> if this instance is ready; otherwise, <c>false</c>.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>尝试检查指定帧输入是否就绪（预测或确认）。</summary>
	/// <param name="frame">帧号。</param>
	/// <param name="isReady">输出是否就绪。</param>
	/// <returns>帧存在返回 true。</returns>
	public bool TryCheckIfInputIsReady(long frame, out bool isReady){
		int index = this.GetIndex(frame);

        if (index >= 0 && index < this._buffer.Count){
			isReady = this._buffer[this.GetIndex(frame)].IsInputReady();
			return true;
		}

		isReady = false;
		return false;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Try to confirm the predicted input.
	/// </summary>
	/// <returns>Whether the input could be marked as confirmed successfully.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>尝试将指定帧的预测输入标记为确认输入。</summary>
	/// <param name="frame">帧号。</param>
	/// <returns>成功返回 true。</returns>
	public bool TryConfirmPredictedInput(long frame){
		int index = this.GetIndex(frame);

		if (index >= 0 && index < this._buffer.Count){
			this._buffer[index].ConfirmPredictedInput();
			return true;
		}

		return false;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Try to get the input associated to the specified player in the specified frame, 
	/// regardless of if the input has been predicted by the system or confirmed by the user.
	/// </summary>
	/// <returns>Whether the input could be retrieved successfully.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>尝试获取指定帧的输入（预测或确认）。</summary>
	/// <param name="frame">帧号。</param>
	/// <param name="input">输出输入。</param>
	/// <returns>成功返回 true。</returns>
	public bool TryGetInput(long frame, out FrameInput? input){
		int index = this.GetIndex(frame);

		if (index >= 0 && index < this._buffer.Count){
			input = this._buffer[index].GetInput();
			return true;
		}

		input = null;
		return false;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Try to override the predicted input with the confirmed input.
	/// </summary>
	/// <returns>Whether the input could be marked as confirmed successfully.</returns>
	/// <param name="frame">Frame.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>尝试用确认输入覆盖指定帧的预测输入（回滚校正）。</summary>
	/// <param name="frame">帧号。</param>
	/// <returns>成功返回 true。</returns>
	public bool TryOverridePredictionWithConfirmedInput(long frame){
		int index = this.GetIndex(frame);

		if (index >= 0 && index < this._buffer.Count){
			this._buffer[index].OverridePredictionWithConfirmedInput();
			return true;
		}

		return false;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Try to set the input that has been confirmed by the player.
	/// </summary>
	/// <returns>Whether the input could be set successfully.</returns>
	/// <param name="frame">Frame.</param>
	/// <param name="playerInput">Player Input.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>尝试设置指定帧的确认输入（不覆盖预测）。</summary>
	/// <param name="frame">帧号。</param>
	/// <param name="playerInput">玩家输入。</param>
	/// <returns>成功返回 true。</returns>
	public bool TrySetConfirmedInput(long frame, FrameInput playerInput){
		return this.TrySetConfirmedInput(frame, playerInput, false);
	}

	/// <summary>
	/// 尝试设置指定帧的确认输入（可选覆盖预测输入）。
	/// </summary>
	/// <param name="frame">帧号。</param>
	/// <param name="playerInput">玩家输入。</param>
	/// <param name="overridePrediction">是否同时覆盖预测输入。</param>
	/// <returns>成功返回 true。</returns>
	public bool TrySetConfirmedInput(long frame, FrameInput playerInput, bool overridePrediction){
		int index = this.GetIndex(frame);

		// If the index is greater than or equal to zero...
		if (index >= 0){
			// Check if we need to make room in the buffer for the new input...
			while (index >= this._buffer.Count && !this.IsFull()){
				this._buffer.Add(new FluxPlayerInput());
			}

			if (index < this._buffer.Count){
				// And add the new confirmed input to the buffer
				this._buffer[index].ConfirmedInput = playerInput;

				if (overridePrediction){
					this._buffer[index].PredictedInput = playerInput;
				}

				return true;
			}
		}
		return false;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Try to set the input that has been predicted by the system.
	/// </summary>
	/// <returns>Whether the input could be set successfully.</returns>
	/// <param name="playerInput">Player Input.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>尝试设置指定帧的预测输入。</summary>
	/// <param name="frame">帧号。</param>
	/// <param name="playerInput">玩家输入。</param>
	/// <returns>成功返回 true。</returns>
	public bool TrySetPredictedInput(long frame, FrameInput playerInput){
		int index = this.GetIndex(frame);

		// If the index is greater than or equal to zero...
		if (index >= 0){
			// Check if we need to make room in the buffer for the new input...
			while (index >= this._buffer.Count && !this.IsFull()){
				this._buffer.Add(new FluxPlayerInput());
			}

			if (index < this._buffer.Count){
				// And add the new predicted input to the buffer
				this._buffer[index].PredictedInput = playerInput;
				return true;
			}
		}
		return false;
	}
	#endregion
}
