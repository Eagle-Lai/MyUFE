//
// NotifyCollectionChangedEventHandler.cs
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

/// <summary>
/// 集合变更事件处理（NotifyCollectionChangedEventHandler）。
/// <para>用途：从 Mono 移植——定义集合变更事件委托，并提供 Raise 扩展方法便捷触发各种变更事件。</para>
/// </summary>
namespace System.Collections.Specialized {

	/// <summary>
	/// 集合变更事件委托。
	/// </summary>
	/// <param name="sender">事件发送者。</param>
	/// <param name="e">变更事件参数。</param>
	public delegate void NotifyCollectionChangedEventHandler (object sender, NotifyCollectionChangedEventArgs e);

	/// <summary>
	/// 事件触发扩展类。
	/// </summary>
	static class NotifyCollectionChangedEventHandlerExtensions {

		/// <summary>触发 Reset 事件。</summary>
		/// <param name="handler">事件委托。</param>
		/// <param name="sender">发送者。</param>
		/// <param name="action">变更动作。</param>
		public static void Raise (this NotifyCollectionChangedEventHandler handler, object sender, NotifyCollectionChangedAction action)
		{
			if (handler != null)
				handler (sender, new NotifyCollectionChangedEventArgs (action));
		}

		/// <summary>触发 Add/Remove 事件（单个元素）。</summary>
		/// <param name="handler">事件委托。</param>
		/// <param name="sender">发送者。</param>
		/// <param name="action">变更动作。</param>
		/// <param name="changedItem">变更的元素。</param>
		/// <param name="index">元素索引。</param>
		public static void Raise (this NotifyCollectionChangedEventHandler handler, object sender, NotifyCollectionChangedAction action, object changedItem, int index)
		{
			if (handler != null)
				handler (sender, new NotifyCollectionChangedEventArgs (action, changedItem, index));
		}

		/// <summary>触发 Replace 事件（新/旧元素）。</summary>
		/// <param name="handler">事件委托。</param>
		/// <param name="sender">发送者。</param>
		/// <param name="action">变更动作。</param>
		/// <param name="newItem">新元素。</param>
		/// <param name="oldItem">旧元素。</param>
		/// <param name="index">元素索引。</param>
		public static void Raise (this NotifyCollectionChangedEventHandler handler, object sender, NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
		{
			if (handler != null)
				handler (sender, new NotifyCollectionChangedEventArgs (action, newItem, oldItem, index));
		}

		/// <summary>触发指定事件参数的事件。</summary>
		/// <param name="handler">事件委托。</param>
		/// <param name="sender">发送者。</param>
		/// <param name="e">事件参数。</param>
		public static void Raise (this NotifyCollectionChangedEventHandler handler, object sender, NotifyCollectionChangedEventArgs e)
		{
			if (handler != null)
				handler (sender, e);
		}
	}
}
