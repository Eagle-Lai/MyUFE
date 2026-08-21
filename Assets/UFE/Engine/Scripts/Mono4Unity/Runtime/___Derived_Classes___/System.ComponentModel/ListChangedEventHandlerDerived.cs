//
// System.ComponentModel.ListChangedEventHandlerDerived.cs
//
// Author: Duncan Mak (duncan@ximian.com)
// 
// (C) Ximian, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

/// <summary>
/// 列表变更事件委托（ListChangedEventHandlerDerived）。
/// <para>用途：从 Mono 移植——处理列表（IBindingList）内容变更事件的委托签名。</para>
/// </summary>
namespace System.ComponentModel {
	/// <summary>
	/// 列表变更事件委托。
	/// </summary>
	/// <param name="sender">事件发送者。</param>
	/// <param name="e">列表变更事件参数。</param>
	public delegate void ListChangedEventHandlerDerived(object sender, 
						     ListChangedEventArgsDerived e);
}
