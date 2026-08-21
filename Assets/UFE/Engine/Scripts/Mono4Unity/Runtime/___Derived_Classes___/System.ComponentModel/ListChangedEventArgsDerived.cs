//
// System.ComponentModel.ListChangedEventArgsDerived.cs
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

using System.ComponentModel;

/// <summary>
/// 列表变更事件参数（ListChangedEventArgsDerived）。
/// <para>用途：从 Mono 移植——为 ListChanged 事件提供数据（变更类型、新旧索引、相关属性描述符）。</para>
/// </summary>
namespace System.ComponentModel {
	/// <summary>
	/// 列表变更事件参数类。
	/// </summary>
	public class ListChangedEventArgsDerived : EventArgs
	{
		/// <summary>
		/// 相关属性描述符。
		/// </summary>
		public PropertyDescriptor PropertyDescriptor {
			get { return propDesc; }
		}

		/// <summary>变更类型。</summary>
		ListChangedTypeDerived changedType;
		/// <summary>旧索引。</summary>
		int oldIndex;
		/// <summary>新索引。</summary>
		int newIndex;
		/// <summary>相关属性描述符。</summary>
		PropertyDescriptor propDesc;

		/// <summary>
		/// 构造函数（新索引，旧索引默认为 -1）。
		/// </summary>
		/// <param name="listChangedType">变更类型。</param>
		/// <param name="newIndex">新索引。</param>
		public ListChangedEventArgsDerived (ListChangedTypeDerived listChangedType, int newIndex)
		: this (listChangedType, newIndex, -1)
		{
		}

		/// <summary>
		/// 构造函数（仅属性描述符）。
		/// </summary>
		/// <param name="listChangedType">变更类型。</param>
		/// <param name="propDesc">属性描述符。</param>
		public ListChangedEventArgsDerived (ListChangedTypeDerived listChangedType, PropertyDescriptor propDesc)
		{
			this.changedType = listChangedType;
			this.propDesc = propDesc;
		}

		/// <summary>
		/// 构造函数（新索引与属性描述符，旧索引=新索引）。
		/// </summary>
		/// <param name="listChangedType">变更类型。</param>
		/// <param name="newIndex">新索引。</param>
		/// <param name="propDesc">属性描述符。</param>
		public ListChangedEventArgsDerived (ListChangedTypeDerived listChangedType, int newIndex, PropertyDescriptor propDesc)
		{
			this.changedType = listChangedType;
			this.newIndex = newIndex;
			this.oldIndex = newIndex;
			this.propDesc = propDesc;
		}

		/// <summary>
		/// 构造函数（新/旧索引）。
		/// </summary>
		/// <param name="listChangedType">变更类型。</param>
		/// <param name="newIndex">新索引。</param>
		/// <param name="oldIndex">旧索引。</param>
		public ListChangedEventArgsDerived (ListChangedTypeDerived listChangedType, int newIndex, int oldIndex)
		{
			this.changedType = listChangedType;
			this.newIndex = newIndex;
			this.oldIndex = oldIndex;
		}

		/// <summary>
		/// 变更类型。
		/// </summary>
		public ListChangedTypeDerived ListChangedTypeDerived {
			get { return changedType; }
		}
	
		/// <summary>
		/// 旧索引。
		/// </summary>
		public int OldIndex {
			get { return oldIndex; }
		}
	
		/// <summary>
		/// 新索引。
		/// </summary>
		public int NewIndex {
			get { return newIndex; }
		}
	}
}
