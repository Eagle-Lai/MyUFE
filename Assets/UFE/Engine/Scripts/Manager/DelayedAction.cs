using System;

/// <summary>
/// 延迟动作（DelayedAction）。
/// <para>用途：将一个待执行的动作（Action 委托）与其延迟执行的帧数（steps）绑定。</para>
/// <para>由 UFE 的延迟执行系统（如 FluxCapacitor 的帧同步延迟队列）在指定帧数后执行。</para>
/// </summary>
public class DelayedAction {
	/// <summary>
	/// 延迟到期后要执行的动作委托。
	/// </summary>
	public Action action;
	/// <summary>
	/// 执行前需要等待的帧数。
	/// </summary>
    public int steps;

	/// <summary>
	/// 构造函数：创建延迟动作。
	/// </summary>
	/// <param name="action">要延迟执行的动作委托。</param>
	/// <param name="steps">延迟执行的帧数。</param>
	public DelayedAction(Action action, int steps){
		this.action = action;
		this.steps = steps;
	}
}
