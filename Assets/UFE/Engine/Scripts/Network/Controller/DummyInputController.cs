using System.Collections.Generic;

/// <summary>
/// 虚拟输入控制器（DummyInputController）。
/// <para>用途：不读取真实输入，仅由外部（如帧同步系统）通过 SetInput 注入输入数据的控制器，用于网络对战的远端玩家输入回放。</para>
/// </summary>
public class DummyInputController : UFEController {
	#region public override methods
	/// <summary>固定帧更新（空实现，不读取输入）。</summary>
	public override void DoFixedUpdate(){}
	/// <summary>每帧更新（空实现，不读取输入）。</summary>
	public override void DoUpdate(){}
	/// <summary>读取输入（始终返回默认空输入）。</summary>
	/// <param name="inputReference">输入引用。</param>
	/// <returns>默认空输入事件。</returns>
	public override InputEvents ReadInput (InputReferences inputReference){
		return InputEvents.Default;
	}
	#endregion

	#region public instance methods
	/// <summary>
	/// 批量设置输入数据。
	/// </summary>
	/// <param name="inputs">输入字典（输入引用→输入事件）。</param>
	public virtual void SetInput(IDictionary<InputReferences, InputEvents> inputs){
		foreach (KeyValuePair<InputReferences, InputEvents> pair in inputs){
			this.SetInput(pair.Key, pair.Value);
		}
	}

	/// <summary>
	/// 设置单个输入引用对应的输入数据。
	/// </summary>
	/// <param name="inputReference">输入引用。</param>
	/// <param name="ev">输入事件。</param>
	public virtual void SetInput(InputReferences inputReference, InputEvents ev){
		this.inputs[inputReference] = ev;
	}
	#endregion
}
