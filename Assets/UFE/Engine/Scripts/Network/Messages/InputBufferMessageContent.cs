using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// 输入缓冲消息内容（InputBufferMessageContent）。
/// <para>用途：作为网络消息的负载——包含下一期望帧号与一串（帧号→输入）记录，用于帧同步的输入交换。</para>
/// </summary>
public class InputBufferMessageContent{
	#region public instance properties
	/// <summary>下一期望收到的帧号。</summary>
	public long NextExpectedFrame{get; private set;}
	/// <summary>输入缓冲记录列表（帧号, FrameInput）。</summary>
	public IList<Tuple<long, FrameInput>> InputBuffer{get; private set;}
	#endregion

	#region public override methods
	/// <summary>
	/// 构造函数：创建输入缓冲消息内容。
	/// </summary>
	/// <param name="nextExpectedFrame">下一期望帧号。</param>
	/// <param name="inputBuffer">输入缓冲记录列表。</param>
	public InputBufferMessageContent(long nextExpectedFrame, IList<Tuple<long, FrameInput>> inputBuffer){
		this.NextExpectedFrame = nextExpectedFrame;
		this.InputBuffer = inputBuffer ?? new List<Tuple<long, FrameInput>>();
	}
	#endregion

	#region public override methods
	/// <summary>
	/// 生成可读的调试字符串。
	/// </summary>
	/// <returns>调试字符串。</returns>
	public override string ToString (){
		StringBuilder sb = new StringBuilder();
		sb.Append("{");
		for (int i = 0; i < this.InputBuffer.Count; ++i){
			if (sb.Length > 0){
				sb.Append(", ");
			}

			sb	.Append("\"").Append(this.InputBuffer[i].Item1).Append("\":\"")
				.Append(this.InputBuffer[i].Item2).Append("\"");
		}
		sb.Append("}");

		return string.Format(
			"[{0} | nextExpectedFrame = {1} | inputBuffer = {2}]",
			this.GetType().ToString(),
			this.NextExpectedFrame,
			sb.ToString()
		);
	}
	#endregion
}
