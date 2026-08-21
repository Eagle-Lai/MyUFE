using System.Collections.Generic;

/// <summary>
/// 枚举器扩展（IEnumeratorExtensions）。
/// <para>用途：为 IEnumerable 提供 Zip 方法——并行枚举两个序列并按组合函数生成新序列，</para>
/// <para>当任一序列耗尽时结束。</para>
/// </summary>
namespace System.Collections.Generic{
	/// <summary>
	/// 枚举器扩展静态类。
	/// </summary>
	public static class IEnumeratorExtensions{
		/// <summary>
		/// 将两个可枚举序列按元素逐一组合（Zip）。
		/// </summary>
		/// <typeparam name="A">序列 A 元素类型。</typeparam>
		/// <typeparam name="B">序列 B 元素类型。</typeparam>
		/// <typeparam name="T">组合结果类型。</typeparam>
		/// <param name="seqA">序列 A。</param>
		/// <param name="seqB">序列 B。</param>
		/// <param name="func">组合函数。</param>
		/// <returns>组合后的可枚举序列。</returns>
		public static IEnumerable<T> Zip<A, B, T>(this IEnumerable<A> seqA, IEnumerable<B> seqB, Func<A, B, T> func){
			if (seqA == null) throw new ArgumentNullException("seqA");
			if (seqB == null) throw new ArgumentNullException("seqB");
			
			return Zip35Deferred(seqA, seqB, func);
		}

		/// <summary>
		/// Zip 延迟执行实现（使用迭代器块惰性求值）。
		/// </summary>
		/// <typeparam name="A">序列 A 元素类型。</typeparam>
		/// <typeparam name="B">序列 B 元素类型。</typeparam>
		/// <typeparam name="T">组合结果类型。</typeparam>
		/// <param name="seqA">序列 A。</param>
		/// <param name="seqB">序列 B。</param>
		/// <param name="func">组合函数。</param>
		/// <returns>组合后的可枚举序列。</returns>
		private static IEnumerable<T> Zip35Deferred<A, B, T>(
			this IEnumerable<A> seqA, 
			IEnumerable<B> seqB, 
			Func<A, B, T> func
		){
			using (var iteratorA = seqA.GetEnumerator()){
				using (var iteratorB = seqB.GetEnumerator()){
					while (iteratorA.MoveNext() && iteratorB.MoveNext()){
						yield return func(iteratorA.Current, iteratorB.Current);
					}
				}
			}
		}
	}
}
