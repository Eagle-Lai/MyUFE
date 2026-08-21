//
// System.OperationCanceledException.cs
//
// Authors:
//   Zoltan Varga  <vargaz@freemail.hu>
//   Jérémie Laval <jeremie.laval@gmail.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Threading;

/// <summary>
/// 操作取消异常派生类（OperationCanceledExceptionDerived）。
/// <para>用途：从 Mono 移植的带取消令牌（CancellationToken）的操作取消异常——</para>
/// <para>在标准 OperationCanceledException 基础上增加可空取消令牌支持，供 Task 取消机制使用。</para>
/// </summary>
namespace System{
	/// <summary>
	/// 操作取消异常派生类（含取消令牌）。
	/// </summary>
	[Serializable]
	[ComVisible (true)]
	public class OperationCanceledExceptionDerived : OperationCanceledException{
		#region protected instance field
		/// <summary>关联的取消令牌（可空）。</summary>
		CancellationToken? token;
		#endregion
		
		#region public instance properties
		/// <summary>
		/// 关联的取消令牌（未设置时返回 None）。
		/// </summary>
		public CancellationToken CancellationToken{
			get {
				if (token == null){
					return CancellationToken.None;
				}
				return token.Value;
			}
		}
		#endregion
		
		#region Base Constructors
		/// <summary>默认构造函数。</summary>
		public OperationCanceledExceptionDerived() : base(){}

		/// <summary>构造函数（指定消息）。</summary>
		/// <param name="message">错误消息。</param>
		public OperationCanceledExceptionDerived(string message) : 
		base(message){}
		
		/// <summary>构造函数（指定消息与内部异常）。</summary>
		/// <param name="message">错误消息。</param>
		/// <param name="innerException">内部异常。</param>
		public OperationCanceledExceptionDerived(
			string message, 
			Exception innerException
		) : base (message, innerException){}

		/// <summary>反序列化构造函数。</summary>
		/// <param name="info">序列化信息。</param>
		/// <param name="context">流上下文。</param>
		protected OperationCanceledExceptionDerived(
			SerializationInfo info, 
			StreamingContext context
		) : base (info, context){}
		#endregion
		
		#region New Constructors
		/// <summary>构造函数（指定取消令牌）。</summary>
		/// <param name="token">取消令牌。</param>
		public OperationCanceledExceptionDerived(CancellationToken token) :
		this(){
			this.token = token;
		}

		/// <summary>构造函数（指定消息与取消令牌）。</summary>
		/// <param name="message">错误消息。</param>
		/// <param name="token">取消令牌。</param>
		public OperationCanceledExceptionDerived(
			string message, 
			CancellationToken token
		):this (message){
			this.token = token;
		}

		/// <summary>构造函数（指定消息、内部异常与取消令牌）。</summary>
		/// <param name="message">错误消息。</param>
		/// <param name="innerException">内部异常。</param>
		/// <param name="token">取消令牌。</param>
		public OperationCanceledExceptionDerived(
			string message, 
			Exception innerException, 
			CancellationToken token)
		: base (message, innerException){
			this.token = token;
		}
		#endregion
	}
}
