using FPLibrary;

/// <summary>
/// 输入事件（InputEvents）。
/// <para>用途：表示玩家一帧的输入数据，包含轴向输入值（axisRaw）和按钮按下状态（button）。</para>
/// <para>由 InputController 读取原始输入后打包生成，交由 ControlsScript 处理。</para>
/// </summary>
public class InputEvents{
	#region public class properties
	/// <summary>
	/// 获取默认的空输入事件实例（单例，供静态引用使用）。
	/// </summary>
	public static InputEvents Default{
		get{
			return InputEvents._Default;
		}
	}
	#endregion

	#region private class properties
	/// <summary>
	/// 默认空输入事件的静态实例。
	/// </summary>
	private static InputEvents _Default = new InputEvents();
	#endregion

	#region public instance properties
	/// <summary>
	/// 轴向原始输入值（定点数）：方向摇杆/按键轴的连续值。
	/// </summary>
	public Fix64 axisRaw {get; protected set;}
	/// <summary>
	/// 按钮按下状态：本帧该按钮是否被按下。
	/// </summary>
	public bool button {get; protected set;}
	#endregion

	#region public constructors
	/// <summary>
	/// 默认构造函数：创建空输入事件（轴向 0、按钮未按下）。
	/// </summary>
	public InputEvents() : this(0f, false){}
	/// <summary>
	/// 按钮构造函数：创建只有按钮状态的输入事件。
	/// </summary>
	/// <param name="button">按钮按下状态。</param>
	public InputEvents(bool button) : this(0f, button){}
	/// <summary>
	/// 轴向构造函数：创建只有轴向值的输入事件，按钮状态由轴向值是否为 0 推导。
	/// </summary>
	/// <param name="axisRaw">轴向原始输入值。</param>
	public InputEvents(Fix64 axisRaw) : this(axisRaw, axisRaw != 0f){}
	/// <summary>
	/// 拷贝构造函数：从另一个输入事件复制数据。
	/// </summary>
	/// <param name="other">要复制的源输入事件。</param>
	public InputEvents(InputEvents other) : this(other.axisRaw, other.button){}
	#endregion

	#region protected constructors
	/// <summary>
	/// 受保护的完整构造函数：同时设置轴向值与按钮状态（供派生类调用）。
	/// </summary>
	/// <param name="axisRaw">轴向原始输入值。</param>
	/// <param name="button">按钮按下状态。</param>
	protected InputEvents(Fix64 axisRaw, bool button){
		this.axisRaw = axisRaw;
		this.button = button;
	}
	#endregion
}
