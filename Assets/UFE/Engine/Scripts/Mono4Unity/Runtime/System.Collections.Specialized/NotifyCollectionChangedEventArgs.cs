//
// NotifyCollectionChangedEventArgs.cs
//
// Contact:
//   Moonlight List (moonlight-list@lists.ximian.com)
//
// Copyright 2008 Novell, Inc.
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
using System.Collections.Generic;

/// <summary>
/// 集合变更事件参数（NotifyCollectionChangedEventArgs）。
/// <para>用途：从 Mono 移植——携带集合变更的详细信息（动作、新增/移除元素与索引），</para>
/// <para>供 CollectionChanged 事件处理器使用。</para>
/// </summary>
namespace System.Collections.Specialized {

	/// <summary>
	/// 集合变更事件参数类（sealed）。
	/// </summary>
	public sealed class NotifyCollectionChangedEventArgs : EventArgs {
		/// <summary>新增元素列表。</summary>
		List<object> new_items, old_items;

		/// <summary>
		/// 构造函数（Reset 动作）。
		/// </summary>
		/// <param name="action">变更动作（必须为 Reset）。</param>
		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action)
		{
			if (action != NotifyCollectionChangedAction.Reset)
				throw new NotSupportedException ();

			Action = action;
			NewStartingIndex = -1;
			OldStartingIndex = -1;
		}
		
		/// <summary>
		/// 构造函数（Add/Remove 动作，单个元素）。
		/// </summary>
		/// <param name="action">变更动作。</param>
		/// <param name="changedItem">变更的元素。</param>
		/// <param name="index">元素索引。</param>
		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object changedItem, int index)
		{
			switch (action) {
			case NotifyCollectionChangedAction.Add:
				new_items = new List<object>();
				new_items.Add (changedItem);
				NewStartingIndex = index;
				OldStartingIndex = -1;
				break;
			case NotifyCollectionChangedAction.Remove:
				old_items = new List<object>();
				old_items.Add (changedItem);
				OldStartingIndex = index;
				NewStartingIndex = -1;
				break;
			default:
				throw new NotSupportedException ();
			}

			Action = action;
		}

		/// <summary>
		/// 构造函数（Replace 动作，新/旧元素）。
		/// </summary>
		/// <param name="action">变更动作（必须为 Replace）。</param>
		/// <param name="newItem">新元素。</param>
		/// <param name="oldItem">旧元素。</param>
		/// <param name="index">元素索引。</param>
		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
		{
			if (action != NotifyCollectionChangedAction.Replace)
				throw new NotSupportedException ();

			Action = action;

			new_items = new List<object>();
			new_items.Add (newItem);

			old_items = new List<object>();
			old_items.Add (oldItem);

			NewStartingIndex = index;
			OldStartingIndex = -1;
		}

		/// <summary>变更动作。</summary>
		public NotifyCollectionChangedAction Action { get; private set; }

		/// <summary>新增元素列表。</summary>
		public IList NewItems {
			get { return new_items; }
		}

		/// <summary>移除的元素列表。</summary>
		public IList OldItems {
			get { return old_items; }
		}

		/// <summary>新增元素的起始索引。</summary>
		public int NewStartingIndex { get; private set; }
		/// <summary>移除元素的起始索引。</summary>
		public int OldStartingIndex { get; private set; }
	}
}
