using System;
using System.IO;

/// <summary>
/// 网络消息类型（NetworkMessageType）：帧同步网络消息的类别。
/// </summary>
public enum NetworkMessageType : byte{
	/// <summary>输入缓冲消息。</summary>
	InputBuffer = 0,
	/// <summary>请求随机种子同步。</summary>
	RandomSeedSynchronization,
	/// <summary>随机种子已同步。</summary>
	RandomSeedSynchronized,
	/// <summary>同步消息（状态快照）。</summary>
	Syncronization,
}
