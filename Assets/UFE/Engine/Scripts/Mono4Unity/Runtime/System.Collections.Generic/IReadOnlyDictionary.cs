//
// IReadOnlyDictionary.cs
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
/// 只读字典接口（IReadOnlyDictionary&lt;TKey, TValue&gt;）。
/// <para>用途：从 Mono 移植——提供键值对的只读访问（按键索引、键/值集合、ContainsKey、TryGetValue）。</para>
/// </summary>
namespace System.Collections.Generic
{
	/// <summary>
	/// 只读字典接口：可按键读取值与遍历键/值集合。
	/// </summary>
	public interface IReadOnlyDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
	{
		/// <summary>按键索引取值。</summary>
		/// <param name="key">键。</param>
		/// <returns>对应的值。</returns>
		TValue this [TKey key] { get; }
		/// <summary>键集合。</summary>
		IEnumerable<TKey> Keys { get; }
		/// <summary>值集合。</summary>
		IEnumerable<TValue> Values { get; }
		
		/// <summary>判断是否包含指定键。</summary>
		/// <param name="key">键。</param>
		/// <returns>包含返回 true。</returns>
		bool ContainsKey (TKey key);
		/// <summary>尝试按键获取值。</summary>
		/// <param name="key">键。</param>
		/// <param name="value">输出值。</param>
		/// <returns>找到返回 true。</returns>
		bool TryGetValue (TKey key, out TValue value);
	}
}

//#endif