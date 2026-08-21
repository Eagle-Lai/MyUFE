//
// TaskCreationOptions.cs
//
// Authors:
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
// Copyright 2011 Xamarin Inc.
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
//
//

//#if NET_4_0 || MOBILE

/// <summary>
/// 任务创建选项（TaskCreationOptions）。
/// <para>用途：从 Mono 移植——控制 Task 创建与调度行为的选项标志。</para>
/// </summary>
namespace System.Threading.Tasks
{
	/// <summary>
	/// 任务创建选项枚举（Flags）。
	/// </summary>
	[FlagsAttribute, SerializableAttribute]
	public enum TaskCreationOptions
	{
		/// <summary>默认选项。</summary>
		None             = 0x0,
		/// <summary>优先公平调度。</summary>
		PreferFairness   = 0x1,
		/// <summary>长时间运行任务（建议独立线程）。</summary>
		LongRunning      = 0x2,
		/// <summary>任务附加到父任务。</summary>
		AttachedToParent = 0x4,
#if NET_4_5
		/// <summary>拒绝子任务附加。</summary>
		DenyChildAttach  = 0x8,
		/// <summary>隐藏调度器。</summary>
		HideScheduler    = 0x10
#endif
	}
}
//#endif
