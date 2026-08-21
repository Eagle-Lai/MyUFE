//
// System.ComponentModel.ListChangedTypeDerived.cs
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

/// <summary>
/// 列表变更类型（ListChangedTypeDerived）。
/// <para>用途：从 Mono 移植——描述列表内容如何被改变（重置/添加/删除/移动/修改等）。</para>
/// </summary>
namespace System.ComponentModel {
	/// <summary>
	/// 列表变更类型枚举。
	/// </summary>
	public enum ListChangedTypeDerived {
		/// <summary>列表整体重置。</summary>
		Reset = 0,
		/// <summary>添加元素。</summary>
		ItemAdded = 1,
		/// <summary>删除元素。</summary>
		ItemDeleted = 2,
		/// <summary>移动元素。</summary>
		ItemMoved = 3,
		/// <summary>修改元素。</summary>
		ItemChanged = 4,
		/// <summary>添加属性描述符。</summary>
		PropertyDescriptorAdded = 5,
		/// <summary>删除属性描述符。</summary>
		PropertyDescriptorDeleted = 6,
		/// <summary>修改属性描述符。</summary>
		PropertyDescriptorChanged = 7
	}
}
