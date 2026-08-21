using System;
using System.Collections.Generic;

/// <summary>
/// 比较器生成器（ComparerGenerator）。
/// <para>用途：无需单独编写类即可由 Comparison&lt;T&gt; 委托动态生成 IComparer&lt;T&gt; 匿名比较器。</para>
/// </summary>
namespace DGP.Util.System{
	///////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This class contains a method for generating a simple anonymous 
	/// Comparer without having to create a separate class manually.
	/// <seealso cref="System.IComparer"/>
	/// <seealso cref="System.IComparable"/>
	/// <seealso cref="System.IComparison"/>
	/// </summary>
	///////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// 比较器生成器：按比较委托创建匿名比较器。
	/// </summary>
	public static class ComparerGenerator{
		#region private class implementation
		/// <summary>
		/// 比较器实现：包装 Comparison&lt;T&gt; 委托为 IComparer&lt;T&gt;。
		/// </summary>
		/// <typeparam name="T">元素类型。</typeparam>
		private class ComparerImplementation<T> : IComparer<T>{
			/// <summary>比较委托。</summary>
	        private readonly Comparison<T> _comparison;
			
			/// <summary>
			/// 构造函数。
			/// </summary>
			/// <param name="comparison">比较委托。</param>
	        public ComparerImplementation(Comparison<T> comparison){
				this._comparison = comparison;
			}
			
			/// <summary>
			/// 比较两个元素（委托到 _comparison）。
			/// </summary>
			/// <param name="x">元素 x。</param>
			/// <param name="y">元素 y。</param>
			/// <returns>比较结果（负/零/正）。</returns>
	        public int Compare(T x, T y){
				return this._comparison.Invoke(x, y);
			}
	    }
		#endregion
		
		#region public instance methods
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This method is used for generating a simple anonymous 
		/// Comparer without having to create a separate class manually.
		/// </summary>
		/// <returns>The comparer.</returns>
		/// <param name='comparison'>Comparison.</param>
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 由比较委托生成匿名比较器。
		/// </summary>
		/// <returns>比较器实例。</returns>
		/// <param name='comparison'>比较委托。</param>
		public static IComparer<T> GetComparer<T>(
			Comparison<T> comparison
		){
	        return new ComparerImplementation<T>(comparison);
	    }
		#endregion
	}
}
