/// <summary>
/// Func 委托扩展定义。
/// <para>用途：为旧版 .NET（缺少多参数 Func 委托）提供 5~9 个参数的 Func 委托定义。</para>
/// </summary>
namespace System{
	/// <summary>5 参数有返回值委托。</summary>
	public delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
	/// <summary>6 参数有返回值委托。</summary>
	public delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);
	/// <summary>7 参数有返回值委托。</summary>
	public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7);
	/// <summary>8 参数有返回值委托。</summary>
	public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8);
	/// <summary>9 参数有返回值委托。</summary>
	public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9);
}
