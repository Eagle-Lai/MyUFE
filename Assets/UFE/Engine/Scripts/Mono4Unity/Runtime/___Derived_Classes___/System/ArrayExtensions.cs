using System;

/// <summary>
/// 数组扩展（ArrayExtensions）。
/// <para>用途：提供旧版 .NET 缺少的 ConvertAll 数组转换方法。</para>
/// </summary>
namespace System{
	/// <summary>
	/// 数组扩展类。
	/// </summary>
	public class ArrayExtensions{
		/// <summary>
		/// 将数组每个元素按转换器转换为新类型数组。
		/// </summary>
		/// <typeparam name="TInput">输入元素类型。</typeparam>
		/// <typeparam name="TOutput">输出元素类型。</typeparam>
		/// <param name="input">源数组。</param>
		/// <param name="converter">转换器委托。</param>
		/// <returns>转换后的数组。</returns>
		public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] input, Converter<TInput, TOutput> converter){
			if (input == null) {
				throw new ArgumentNullException("input");
			}
			if (converter == null) {
				throw new ArgumentNullException("converter");
			}

			TOutput[] output = new TOutput[input.Length];
			for (int i = 0; i < input.Length; ++i){
				output[i] = converter.Invoke(input[i]);
			}
			return output;
		}
	}
}
