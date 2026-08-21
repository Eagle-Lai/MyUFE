//
// MonoTODOAttribute.cs
//
// Authors:
//   Ravi Pratap (ravi@ximian.com)
//   Eyal Alaluf <eyala@mainsoft.com> 
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2006 Mainsoft, Inc (http://www.mainsoft.com)
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
/// Mono TODO 标注特性（MonoTODOAttribute）。
/// <para>用途：移植自 Mono 的工具标注特性——标记尚未实现/待办/不支持/扩展的代码位置，便于开发者识别。</para>
/// <para>包含 MonoTODO/MonoDocumentationNote/MonoExtension/MonoInternalNote/MonoLimitation/MonoNotSupported 六种标注。</para>
/// </summary>
namespace System {
	
	/// <summary>
	/// 通用 TODO 标注：带可选注释文本的标注特性基类。
	/// </summary>
	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	internal class MonoTODOAttribute : Attribute {

		/// <summary>标注注释文本。</summary>
		string comment;

		/// <summary>
		/// 默认构造函数。
		/// </summary>
		public MonoTODOAttribute ()
		{
		}

		/// <summary>
		/// 带注释的构造函数。
		/// </summary>
		/// <param name="comment">注释文本。</param>
		public MonoTODOAttribute (string comment)
		{
			this.comment = comment;
		}

		/// <summary>
		/// 注释文本属性。
		/// </summary>
		public virtual string Comment {
			get { return comment; }
		}
	}

	/// <summary>
	/// 文档说明标注：标注缺少文档说明的成员。
	/// </summary>
	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	internal class MonoDocumentationNoteAttribute : MonoTODOAttribute {

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="comment">注释文本。</param>
		public MonoDocumentationNoteAttribute (string comment)
			: base (comment)
		{
		}

		/// <summary>
		/// 注释文本属性。
		/// </summary>
		public override string Comment {
			get { return base.Comment; }
		}
	}

	/// <summary>
	/// 扩展标注：标注 Mono 的扩展成员。
	/// </summary>
	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	internal class MonoExtensionAttribute : MonoTODOAttribute {

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="comment">注释文本。</param>
		public MonoExtensionAttribute (string comment)
			: base (comment)
		{
		}

		/// <summary>
		/// 注释文本属性。
		/// </summary>
		public override string Comment {
			get { return base.Comment; }
		}
	}

	/// <summary>
	/// 内部注释标注：标注需要内部说明的实现细节。
	/// </summary>
	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	internal class MonoInternalNoteAttribute : MonoTODOAttribute {

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="comment">注释文本。</param>
		public MonoInternalNoteAttribute (string comment)
			: base (comment)
		{
		}

		/// <summary>
		/// 注释文本属性。
		/// </summary>
		public override string Comment {
			get { return base.Comment; }
		}
	}

	/// <summary>
	/// 限制标注：标注存在实现限制的成员。
	/// </summary>
	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	internal class MonoLimitationAttribute : MonoTODOAttribute {

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="comment">注释文本。</param>
		public MonoLimitationAttribute (string comment)
			: base (comment)
		{
		}

		/// <summary>
		/// 注释文本属性。
		/// </summary>
		public override string Comment {
			get { return base.Comment; }
		}
	}

	/// <summary>
	/// 不支持标注：标注尚未支持的功能。
	/// </summary>
	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	internal class MonoNotSupportedAttribute : MonoTODOAttribute {

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="comment">注释文本。</param>
		public MonoNotSupportedAttribute (string comment)
			: base (comment)
		{
		}

		/// <summary>
		/// 注释文本属性。
		/// </summary>
		public override string Comment {
			get { return base.Comment; }
		}
	}
}
