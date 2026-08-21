using System.Collections.Generic;

/// <summary>
/// 帧同步玩家输入（FluxPlayerInput）。
/// <para>用途：封装一名玩家在某一帧的输入——包含预测输入（PredictedInput，本地先行）与确认输入（ConfirmedInput，网络确认后），</para>
/// <para>供帧同步的回滚/延迟机制使用。</para>
/// </summary>
public class FluxPlayerInput{
	#region public instance properties
	/// <summary>预测输入（本地预测，未确认）。</summary>
	public FrameInput? PredictedInput;
	/// <summary>确认输入（已确认的预测输入）。</summary>
	public FrameInput? ConfirmedInput;
	#endregion

	#region public instance constructors
	/// <summary>默认构造函数。</summary>
	public FluxPlayerInput() : this(null, null){}

	/// <summary>构造函数（仅预测输入）。</summary>
	/// <param name="predictedInput">预测输入。</param>
	public FluxPlayerInput(FrameInput? predictedInput) : this(predictedInput, null){}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Initializes a new instance of the <see cref="BUM.Runtime.GameEngine.PlayerInput"/> class.
	/// </summary>
	/// <param name="predictedInput">Predicted input.</param>
	/// <param name="confirmedInput">Confirmed input.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>构造函数（完整参数）。</summary>
	/// <param name="predictedInput">预测输入。</param>
	/// <param name="confirmedInput">确认输入。</param>
	public FluxPlayerInput(FrameInput? predictedInput, FrameInput? confirmedInput){
		this.PredictedInput = predictedInput;
		this.ConfirmedInput = confirmedInput;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Initializes a new instance of the <see cref="BUM.InputSystem.PlayerInput"/> class.
	/// </summary>
	/// <param name="source">Source.</param>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>拷贝构造函数。</summary>
	/// <param name="source">源对象。</param>
	public FluxPlayerInput(FluxPlayerInput source) : this(source.PredictedInput, source.ConfirmedInput){}
	#endregion

	#region public instance methods
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Determines whether the predicted and confirmed values are equal.
	/// </summary>
	/// <returns><c>true</c> if the input values are equal; otherwise, <c>false</c>.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>判断预测输入与确认输入是否相等。</summary>
	/// <returns>相等返回 true。</returns>
	public bool ArePredictedAndConfirmedInputsEqual(){
		return
			this.PredictedInput == null && 
			this.ConfirmedInput == null 
			||
			this.PredictedInput != null && 
			this.ConfirmedInput != null &&
			this.PredictedInput.Value.Equals(this.ConfirmedInput.Value);
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Confirms the predicted input.
	/// </summary>
	/// <returns><c>true</c>, if input as confirmed was marked, <c>false</c> otherwise.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>将预测输入标记为确认输入。</summary>
	/// <returns>有预测输入并确认成功返回 true。</returns>
	public bool ConfirmPredictedInput(){
		if (this.PredictedInput != null){
			this.ConfirmedInput = this.PredictedInput;
			return true;
		}
		return false;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This method returns the confirmed input if it's defined; otherwise, it returns the predicted input.
	/// </summary>
	/// <returns>The input.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>返回确认输入（未定义时返回预测输入）。</summary>
	/// <returns>输入值。</returns>
	public FrameInput? GetInput(){
		return this.ConfirmedInput ?? this.PredictedInput;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Determines whether the input has been confirmed by the player.
	/// </summary>
	/// <returns><c>true</c> if the input has been confirmed by the player; otherwise, <c>false</c>.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>判断输入是否已确认。</summary>
	/// <returns>已确认返回 true。</returns>
	public bool IsInputConfirmed(){
		return this.ConfirmedInput != null;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Determines whether the input has been predicted by the system.
	/// </summary>
	/// <returns><c>true</c> if the input has been predicted by the system; otherwise, <c>false</c>.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>判断输入是否为预测输入。</summary>
	/// <returns>有预测输入返回 true。</returns>
	public bool IsInputPredicted(){
		return this.PredictedInput != null;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Determines whether this instance is ready because the player have at least a predicted or a confirmed input.
	/// </summary>
	/// <returns><c>true</c> if this instance is ready; otherwise, <c>false</c>.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>判断输入是否就绪（有预测或确认输入）。</summary>
	/// <returns>就绪返回 true。</returns>
	public bool IsInputReady(){
		return this.GetInput() != null;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Overrides the predicted input with the confirmed input.
	/// </summary>
	/// <returns><c>true</c>, if input as confirmed was marked, <c>false</c> otherwise.</returns>
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>用确认输入覆盖预测输入（回滚校正用）。</summary>
	/// <returns>有确认输入并覆盖成功返回 true。</returns>
	public bool OverridePredictionWithConfirmedInput(){
		if (this.ConfirmedInput != null){
			this.PredictedInput = this.ConfirmedInput;
			return true;
		}
		return false;
	}
	#endregion
}
