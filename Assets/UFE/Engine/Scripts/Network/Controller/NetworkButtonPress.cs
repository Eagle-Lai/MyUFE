using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// 网络按钮位掩码（NetworkButtonPress，Flags 枚举）。
/// <para>用途：将按钮输入压缩为按位掩码以便网络传输——8 位覆盖 Button1~4+方向，16 位覆盖 Button1~12，</para>
/// <para>32 位额外覆盖 Start。与引擎 ButtonPress 枚举互相转换。</para>
/// </summary>
[Flags]
public enum NetworkButtonPress{
	/// <summary>无。</summary>
	None		= 0,
	/// <summary>前方向。</summary>
	Forward		= 1<<0,
	/// <summary>后方向。</summary>
	Back		= 1<<1,
	/// <summary>上方向。</summary>
	Up			= 1<<2,
	/// <summary>下方向。</summary>
	Down		= 1<<3,
	/// <summary>按钮1。</summary>
	Button1		= 1<<4,
	/// <summary>按钮2。</summary>
	Button2		= 1<<5,
	/// <summary>按钮3。</summary>
	Button3		= 1<<6,
	/// <summary>按钮4。</summary>
	Button4		= 1<<7,

	// 16bits network packages required
	/// <summary>按钮5。</summary>
	Button5		= 1<<8,
	/// <summary>按钮6。</summary>
	Button6		= 1<<9,
	/// <summary>按钮7。</summary>
	Button7		= 1<<10,
	/// <summary>按钮8。</summary>
	Button8		= 1<<11,
	/// <summary>按钮9。</summary>
	Button9		= 1<<12,
	/// <summary>按钮10。</summary>
	Button10	= 1<<13,
	/// <summary>按钮11。</summary>
	Button11	= 1<<14,
	/// <summary>按钮12。</summary>
	Button12	= 1<<15,

	// 32bits network packages required
	/// <summary>Start 键。</summary>
	Start		= 1<<16,
}

/// <summary>
/// 网络按钮位掩码扩展（NetworkButtonPressExtensions）。
/// <para>提供引擎 ButtonPress 枚举与网络位掩码的双向转换。</para>
/// </summary>
public static class NetworkButtonPressExtensions{
	/// <summary>
	/// 将单个引擎按钮转换为网络位掩码。
	/// </summary>
	/// <param name="button">引擎按钮。</param>
	/// <returns>对应的网络位掩码。</returns>
	public static NetworkButtonPress ToNetworkButtonPress(this ButtonPress button){
		switch(button){
		case ButtonPress.Up:		return NetworkButtonPress.Up;
		case ButtonPress.Down:		return NetworkButtonPress.Down;
		case ButtonPress.Back:		return NetworkButtonPress.Back;
		case ButtonPress.Forward:	return NetworkButtonPress.Forward;
		case ButtonPress.Button1:	return NetworkButtonPress.Button1;
		case ButtonPress.Button2:	return NetworkButtonPress.Button2;
		case ButtonPress.Button3:	return NetworkButtonPress.Button3;
		case ButtonPress.Button4:	return NetworkButtonPress.Button4;
		case ButtonPress.Button5:	return NetworkButtonPress.Button5;
		case ButtonPress.Button6:	return NetworkButtonPress.Button6;
		case ButtonPress.Button7:	return NetworkButtonPress.Button7;
		case ButtonPress.Button8:	return NetworkButtonPress.Button8;
		case ButtonPress.Button9:	return NetworkButtonPress.Button9;
		case ButtonPress.Button10:	return NetworkButtonPress.Button10;
		case ButtonPress.Button11:	return NetworkButtonPress.Button11;
		case ButtonPress.Button12:	return NetworkButtonPress.Button12;
		case ButtonPress.Start:		return NetworkButtonPress.Start;
		default:					return NetworkButtonPress.None;
		}
	}

	/// <summary>
	/// 将一组引擎按钮按位或合并为网络位掩码。
	/// </summary>
	/// <param name="buttons">引擎按钮集合。</param>
	/// <returns>合并后的网络位掩码。</returns>
	public static NetworkButtonPress ToNetworkButtonPress(this IEnumerable<ButtonPress> buttons){
		NetworkButtonPress n = NetworkButtonPress.None;

		if (buttons != null){
			foreach (ButtonPress button in buttons){
				n |= button.ToNetworkButtonPress();
			}
		}

		return n;
	}

	/// <summary>
	/// 将网络位掩码拆解为引擎按钮只读集合。
	/// </summary>
	/// <param name="buttonPresses">网络位掩码。</param>
	/// <returns>引擎按钮列表。</returns>
	public static ReadOnlyCollection<ButtonPress> ToButtonPresses(this NetworkButtonPress buttonPresses){
		List<ButtonPress> list = new List<ButtonPress>();

		if (buttonPresses != NetworkButtonPress.None){
			if ((buttonPresses & NetworkButtonPress.Up) != 0)		list.Add(ButtonPress.Up);
			if ((buttonPresses & NetworkButtonPress.Down) != 0)		list.Add(ButtonPress.Down);
			if ((buttonPresses & NetworkButtonPress.Back) != 0)		list.Add(ButtonPress.Back);
			if ((buttonPresses & NetworkButtonPress.Forward) != 0)	list.Add(ButtonPress.Forward);
			if ((buttonPresses & NetworkButtonPress.Button1) != 0)	list.Add(ButtonPress.Button1);
			if ((buttonPresses & NetworkButtonPress.Button2) != 0)	list.Add(ButtonPress.Button2);
			if ((buttonPresses & NetworkButtonPress.Button3) != 0)	list.Add(ButtonPress.Button3);
			if ((buttonPresses & NetworkButtonPress.Button4) != 0)	list.Add(ButtonPress.Button4);
			if ((buttonPresses & NetworkButtonPress.Button5) != 0)	list.Add(ButtonPress.Button5);
			if ((buttonPresses & NetworkButtonPress.Button6) != 0)	list.Add(ButtonPress.Button6);
			if ((buttonPresses & NetworkButtonPress.Button7) != 0)	list.Add(ButtonPress.Button7);
			if ((buttonPresses & NetworkButtonPress.Button8) != 0)	list.Add(ButtonPress.Button8);
			if ((buttonPresses & NetworkButtonPress.Button9) != 0)	list.Add(ButtonPress.Button9);
			if ((buttonPresses & NetworkButtonPress.Button10) != 0)	list.Add(ButtonPress.Button10);
			if ((buttonPresses & NetworkButtonPress.Button11) != 0)	list.Add(ButtonPress.Button11);
			if ((buttonPresses & NetworkButtonPress.Button12) != 0)	list.Add(ButtonPress.Button12);
			if ((buttonPresses & NetworkButtonPress.Start) != 0)	list.Add(ButtonPress.Start);
		}

		return new ReadOnlyCollection<ButtonPress>(list);
	}
}
