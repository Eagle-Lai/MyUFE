using System;
using System.IO;
using FPLibrary;

/// <summary>
/// 帧输入（FrameInput）。
/// <para>用途：网络对战中一帧输入的紧凑数据结构——包含水平/垂直轴向原始值与按钮位掩码（NetworkButtonPress），</para>
/// <para>以及菜单选中选项（selectedOption）。实现 IEquatable 用于输入比较（回滚校正）。</para>
/// <para>构造函数自动保持轴向值与方向按钮位的一致性。</para>
/// </summary>
[Serializable]
public struct FrameInput : IEquatable<FrameInput>{
	#region public class properties
	/// <summary>无选中选项的标记值（sbyte 最小值）。</summary>
	public readonly static sbyte NullSelectedOption = sbyte.MinValue;
	#endregion

	#region public instance properties
	/// <summary>水平轴向原始值。</summary>
	public Fix64 horizontalAxisRaw;
	/// <summary>垂直轴向原始值。</summary>
	public Fix64 verticalAxisRaw;
	/// <summary>按钮位掩码。</summary>
	public NetworkButtonPress buttons;
	/// <summary>菜单选中选项。</summary>
	public sbyte selectedOption;
	#endregion

	#region public instance constructors
	/// <summary>构造函数（仅选中选项，无按钮）。</summary>
	/// <param name="selectedOption">选中选项。</param>
	public FrameInput(sbyte selectedOption) : this(NetworkButtonPress.None, selectedOption){}

	/// <summary>构造函数（从按钮位掩码推导方向轴向）。</summary>
	/// <param name="buttons">按钮位掩码。</param>
	/// <param name="selectedOption">选中选项。</param>
	public FrameInput(NetworkButtonPress buttons, sbyte selectedOption) : this(
		((buttons & NetworkButtonPress.Forward) != 0 ? 1 : 0) - ((buttons & NetworkButtonPress.Back) != 0 ? 1 : 0),
		((buttons & NetworkButtonPress.Up) != 0 ? 1 : 0) - ((buttons & NetworkButtonPress.Down) != 0 ? 1 : 0),
		buttons,
		selectedOption
	){}

	/// <summary>构造函数（完整参数，自动校正方向按钮位）。</summary>
	/// <param name="horizontalAxisRaw">水平轴向值。</param>
	/// <param name="verticalAxisRaw">垂直轴向值。</param>
	/// <param name="buttons">按钮位掩码。</param>
	/// <param name="selectedOption">选中选项。</param>
	public FrameInput(
        Fix64 horizontalAxisRaw,
        Fix64 verticalAxisRaw, 
		NetworkButtonPress buttons,
		sbyte selectedOption
	){
		// Make sure the buttons match the axis values
		if (horizontalAxisRaw == 0f){
			buttons = buttons & ~NetworkButtonPress.Back;
			buttons = buttons & ~NetworkButtonPress.Forward;
		}else if (horizontalAxisRaw > 0f){
			buttons = buttons & ~NetworkButtonPress.Back;
			buttons = buttons |  NetworkButtonPress.Forward;
		}else{
			buttons = buttons |  NetworkButtonPress.Back;
			buttons = buttons & ~NetworkButtonPress.Forward;
		}

		if (verticalAxisRaw == 0f){
			buttons = buttons & ~NetworkButtonPress.Down;
			buttons = buttons & ~NetworkButtonPress.Up;
		}else if (verticalAxisRaw > 0f){
			buttons = buttons & ~NetworkButtonPress.Down;
			buttons = buttons |  NetworkButtonPress.Up;
		}else{
			buttons = buttons |  NetworkButtonPress.Down;
			buttons = buttons & ~NetworkButtonPress.Up;
		}

		// Assign the values
		this.horizontalAxisRaw = horizontalAxisRaw;
		this.verticalAxisRaw = verticalAxisRaw;
		this.buttons = buttons;
		this.selectedOption = selectedOption;
	}

	/// <summary>拷贝构造函数。</summary>
	/// <param name="other">源帧输入。</param>
	public FrameInput(FrameInput other) : this(
		other.horizontalAxisRaw,
		other.verticalAxisRaw,
		other.buttons,
		other.selectedOption
	){}
	#endregion

	#region public instance methods
//	public byte[] Serialize(){
//		return FrameInput.Serialize(this);
//	}
	#endregion

	#region public override methods
	/// <summary>
	/// 生成可读的调试字符串。
	/// </summary>
	/// <returns>调试字符串。</returns>
	public override string ToString (){
		return string.Format(
			"[FrameInput | horizontalAxisRaw = {0} | verticalAxisRaw = {1} | buttons = {2} | selected option = {3}]",
			this.horizontalAxisRaw,
			this.verticalAxisRaw,
			this.buttons,
			this.selectedOption
		);
	}
	#endregion

	#region IEquatable<FrameInput> interface implementation
	/// <summary>
	/// 判断对象是否与当前帧输入相等。
	/// </summary>
	/// <param name="obj">比较对象。</param>
	/// <returns>相等返回 true。</returns>
	public override bool Equals (object obj){
		if (obj is FrameInput){
			return this.Equals((FrameInput)obj);
		}
		return false;
	}

	/// <summary>
	/// 判断两个帧输入是否相等（轴向/按钮/选中选项全部相等）。
	/// </summary>
	/// <param name="other">另一个帧输入。</param>
	/// <returns>相等返回 true。</returns>
	public bool Equals(FrameInput other){
		return 
			this.horizontalAxisRaw == other.horizontalAxisRaw &&
			this.verticalAxisRaw == other.verticalAxisRaw &&
			this.buttons == other.buttons &&
			this.selectedOption == other.selectedOption;
	}

	/// <summary>
	/// 生成哈希码。
	/// </summary>
	/// <returns>哈希码。</returns>
	public override int GetHashCode (){
		unchecked{
			return 
				(int)(this.buttons) +
//				11 * this.horizontalAxis +
//				47 * this.horizontalAxisRaw +
//				101 * this.verticalAxis + 
//				449 * this.verticalAxisRaw +
				1553 * this.selectedOption;
		}
	}

	/// <summary>相等运算符。</summary>
	/// <param name="f1">帧输入1。</param>
	/// <param name="f2">帧输入2。</param>
	/// <returns>相等返回 true。</returns>
	public static bool operator == (FrameInput f1, FrameInput f2){
		return f1.Equals(f2);
	}

	/// <summary>不等运算符。</summary>
	/// <param name="f1">帧输入1。</param>
	/// <param name="f2">帧输入2。</param>
	/// <returns>不相等返回 true。</returns>
	public static bool operator != (FrameInput f1, FrameInput f2){
		return !(f1 == f2);
	}
	#endregion
}
