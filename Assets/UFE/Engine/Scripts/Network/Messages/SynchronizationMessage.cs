using System;
using System.IO;
using System.Text;

/// <summary>
/// 同步消息（SynchronizationMessage）。
/// <para>用途：携带游戏状态快照（FluxSimpleState）的网络消息，用于帧同步的定期状态同步与反同步恢复。</para>
/// </summary>
public class SynchronizationMessage : NetworkMessage<FluxSimpleState>{
	#region public override methods
	/// <summary>
	/// 构造函数（直接指定字段）。
	/// </summary>
	/// <param name="playerIndex">玩家索引。</param>
	/// <param name="currentFrame">当前帧号。</param>
	/// <param name="data">游戏状态快照。</param>
	public SynchronizationMessage(int playerIndex, long currentFrame, FluxSimpleState data) : 
	base(NetworkMessageType.Syncronization, playerIndex, currentFrame, data){}
	
	/// <summary>
	/// 构造函数（从序列化字节流反序列化，校验消息类型）。
	/// </summary>
	/// <param name="serializedNetworkMessage">序列化的消息字节。</param>
	public SynchronizationMessage(byte[] serializedNetworkMessage) : base(serializedNetworkMessage){
		if (this.MessageType != NetworkMessageType.Syncronization){
			throw new System.FormatException(string.Format(
				"The message type was {0}, but it should have been {1}.",
				this.MessageType,
				NetworkMessageType.Syncronization
			));
		}
	}
	#endregion

	#region protected override methods
	/// <summary>将状态快照写入二进制流。</summary>
	/// <param name="writer">二进制写入器。</param>
	/// <param name="gameState">状态快照。</param>
	protected override void AddToStream(BinaryWriter writer, FluxSimpleState gameState){
		FluxSimpleState.AddToStream(writer, gameState);
	}

	/// <summary>从二进制流读取状态快照。</summary>
	/// <param name="reader">二进制读取器。</param>
	/// <returns>状态快照。</returns>
	protected override FluxSimpleState ReadFromStream(BinaryReader reader){
		return FluxSimpleState.ReadFromStream(reader);
	}
	#endregion

	#region public override methods
	/// <summary>
	/// 生成可读的调试字符串（含双方状态）。
	/// </summary>
	/// <returns>调试字符串。</returns>
	public override string ToString (){
		StringBuilder sb = new StringBuilder();

		sb	.Append("{")
			.Append("\"p1\"=\"")
			.Append(this.Data.p1)
			.Append("\", \"p2\"=\"")
			.Append(this.Data.p2)
			.Append("\"}");

		return string.Format(
			"[{0} | messageType = {1} | playerIndex = {2} | currentFrame = {3} | data = {4}]",
			this.GetType().ToString(),
			this.MessageType,
			this.PlayerIndex,
			this.CurrentFrame,
			sb.ToString()
		);
	}
	#endregion
}
