//
// System.ComponentModel.CollectionChangeEventArgsDerived.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
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
/// 集合变更事件参数（CollectionChangeEventArgsDerived）。
/// <para>用途：从 Mono 移植——为 CollectionChanged 事件提供数据（变更动作与涉及的元素）。</para>
/// </summary>
namespace System.ComponentModel
{
	/// <summary>
	/// Provides data for the CollectionChanged event.
	/// </summary>
	/// <summary>集合变更事件参数类。</summary>
	public class CollectionChangeEventArgsDerived : EventArgs
	{
		/// <summary>变更动作。</summary>
		private CollectionChangeActionDerived changeAction;
		/// <summary>涉及的元素。</summary>
		private object theElement;
		
		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="action">变更动作。</param>
		/// <param name="element">涉及的元素。</param>
		public CollectionChangeEventArgsDerived (CollectionChangeActionDerived action,
						  object element) {
			changeAction = action;
			theElement = element;
		}

		/// <summary>
		/// 变更动作。
		/// </summary>
		public virtual CollectionChangeActionDerived Action {
			get {
				return changeAction;
			}
		}

		/// <summary>
		/// 涉及的元素。
		/// </summary>
		public virtual object Element {
			get {
				return theElement;
			}
		}
	}
}
