//
// Lazy.cs
//
// Authors:
//  Rodrigo Kumpera (kumpera@gmail.com)
//
// Copyright (C) 2010 Novell
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

//#if NET_4_0 || MOONLIGHT

using System;

/// <summary>
/// 惰性初始化线程安全模式（LazyThreadSafetyMode）。
/// <para>用途：从 Mono 移植——指定 Lazy&lt;T&gt; 初始化时的线程安全行为。</para>
/// </summary>
namespace System.Threading
{
	/// <summary>
	/// 惰性初始化线程安全模式枚举。
	/// </summary>
	public enum LazyThreadSafetyMode
	{
		/// <summary>不保证线程安全（多线程并发访问时行为未定义）。</summary>
		None,
		/// <summary>仅发布（PublicationOnly）：多个线程可同时执行工厂，以先完成的结果为准，异常被忽略。</summary>
		PublicationOnly,
		/// <summary>执行并发布（ExecutionAndPublication）：使用锁保证工厂只执行一次，异常会传播。</summary>
		ExecutionAndPublication
	}
}

//#endif
