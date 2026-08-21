//
// System.ComponentModel.CollectionChangeActionDerived.cs
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
/// 集合变更动作（CollectionChangeActionDerived）。
/// <para>用途：从 Mono 移植——描述集合如何被改变（添加/移除/刷新）。</para>
/// </summary>
namespace System.ComponentModel
{
	/// <summary>
	/// Specifies how the collection is changed.
	/// </summary>
	/// <summary>集合变更动作枚举。</summary>
	public enum CollectionChangeActionDerived {
		/// <summary>添加元素。</summary>
		Add = 1,
		/// <summary>移除元素。</summary>
		Remove = 2,
		/// <summary>刷新集合。</summary>
		Refresh = 3
	}
}
