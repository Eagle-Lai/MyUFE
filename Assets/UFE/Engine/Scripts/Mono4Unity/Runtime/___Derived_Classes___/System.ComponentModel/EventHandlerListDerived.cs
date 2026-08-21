//
// System.ComponentModel.EventHandlerListDerived.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 事件处理器列表（EventHandlerListDerived）。
/// <para>用途：从 Mono 移植的事件委托容器——以链表按键存储事件委托，</para>
/// <para>支持按键添加/移除/合并事件处理器，并提供 Dispose 清理。</para>
/// </summary>
namespace System.ComponentModel {

	/// <summary>
	/// 事件委托链表项。
	/// </summary>
	internal class ListEntryDerived {
		/// <summary>事件键。</summary>
		public object key;
		/// <summary>事件委托。</summary>
		public Delegate value;
		/// <summary>下一链表项。</summary>
		public ListEntryDerived next;
	}

	// <summary>
	//   List of Event delegates.
	// </summary>
	//
	// <remarks>
	//   Longer description
	// </remarks>
	/// <summary>
	/// 事件处理器列表：按键存储事件委托的容器。
	/// </summary>
	public sealed class EventHandlerListDerived : IDisposable
	{
		/// <summary>链表头。</summary>
		ListEntryDerived entries;

		/// <summary>null 键对应的事件委托。</summary>
		Delegate null_entry;

		/// <summary>
		/// 默认构造函数。
		/// </summary>
		public EventHandlerListDerived ()
		{
		}

		/// <summary>
		/// 按键索引器：获取/设置指定键的事件委托。
		/// </summary>
		/// <param name="key">事件键。</param>
		/// <returns>事件委托。</returns>
		public Delegate this [object key] {
			get {
				if (key == null)
					return null_entry;
				ListEntryDerived entry = FindEntry (key);
				if (entry != null)
					return entry.value;
				else
					return null;
			}

			set {
				AddHandler (key, value);
			}
		}

		/// <summary>
		/// 添加事件处理器（与已存在的合并）。
		/// </summary>
		/// <param name="key">事件键。</param>
		/// <param name="value">事件委托。</param>
		public void AddHandler (object key, Delegate value)
		{
			if (key == null) {
				null_entry = Delegate.Combine (null_entry, value);
				return;
			}

			ListEntryDerived entry = FindEntry (key);
			if (entry == null) {
				entry = new ListEntryDerived ();
				entry.key = key;
				entry.value = null;
				entry.next = entries;
				entries = entry;
			}

			entry.value = Delegate.Combine (entry.value, value);
		}

		/// <summary>
		/// 合并另一个事件处理器列表中的所有处理器。
		/// </summary>
		/// <param name="listToAddFrom">源事件处理器列表。</param>
		public void AddHandlers (EventHandlerListDerived listToAddFrom)
		{
			if (listToAddFrom == null)
				return;
			
			ListEntryDerived entry = listToAddFrom.entries;
			while (entry != null) {
				AddHandler (entry.key, entry.value);
				entry = entry.next;
			}
		}

		/// <summary>
		/// 移除事件处理器。
		/// </summary>
		/// <param name="key">事件键。</param>
		/// <param name="value">事件委托。</param>
		public void RemoveHandler (object key, Delegate value)
		{
			if (key == null) {
				null_entry = Delegate.Remove (null_entry, value);
				return;
			}

			ListEntryDerived entry = FindEntry (key);
			if (entry == null)
				return;

			entry.value = Delegate.Remove (entry.value, value);
		}

		/// <summary>
		/// 清理列表（释放所有委托引用）。
		/// </summary>
		public void Dispose ()
		{
			entries = null;
		}
		
		/// <summary>
		/// 按键查找链表项。
		/// </summary>
		/// <param name="key">事件键。</param>
		/// <returns>链表项；未找到返回 null。</returns>
		private ListEntryDerived FindEntry (object key)
		{
			ListEntryDerived entry = entries;
			while (entry != null) {
				if (entry.key == key)
					return entry;
				entry = entry.next;
			}

			return null;
		}
	}
}
