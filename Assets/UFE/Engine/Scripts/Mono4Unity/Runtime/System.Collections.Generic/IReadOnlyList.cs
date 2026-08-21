//
// IReadOnlyList.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
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

//#if NET_4_5

/// <summary>
/// 只读列表接口（IReadOnlyList&lt;T&gt;）。
/// <para>用途：从 Mono 移植——提供按索引只读访问的列表接口。</para>
/// </summary>
namespace System.Collections.Generic
{
	/// <summary>
	/// 只读列表接口：可按索引读取元素。
	/// </summary>
	public interface IReadOnlyList</*out*/ T> : IReadOnlyCollection<T>
	{
		/// <summary>按索引读取元素。</summary>
		/// <param name="index">索引。</param>
		/// <returns>对应元素。</returns>
		T this [int index] { get; }
	}
}

//#endif