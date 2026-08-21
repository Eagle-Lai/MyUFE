// 
// IConcurrentDeque.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2011 Jérémie "Garuma" Laval
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

//#if NET_4_0 || MOBILE

using System;
using System.Collections.Generic;
using System.Threading;

#if INSIDE_MONO_PARALLEL
namespace Mono.Threading.Tasks
#else
namespace System.Threading.Tasks
#endif
{
#if INSIDE_MONO_PARALLEL
	public
#endif
	/// <summary>
	/// 并发双端队列接口（IConcurrentDeque&lt;T&gt;）。
	/// </summary>
	interface IConcurrentDeque<T>
	{
		/// <summary>底部压入元素（线程独占）。</summary>
		/// <param name="obj">元素。</param>
		void PushBottom (T obj);
		/// <summary>底部弹出元素（线程独占）。</summary>
		/// <param name="obj">输出弹出的元素。</param>
		/// <returns>弹出结果。</returns>
		PopResult PopBottom (out T obj);
		/// <summary>顶部弹出元素（跨线程偷取）。</summary>
		/// <param name="obj">输出弹出的元素。</param>
		/// <returns>弹出结果。</returns>
		PopResult PopTop (out T obj);
		/// <summary>获取可枚举序列。</summary>
		/// <returns>元素枚举器。</returns>
		IEnumerable<T> GetEnumerable ();
	}
}

//#endif