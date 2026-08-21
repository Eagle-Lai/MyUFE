using System;
using System.IO;

/// <summary>
/// 网络消息基类（NetworkMessage&lt;T&gt;）。
/// <para>用途：帧同步网络消息的抽象基类——封装消息类型、玩家索引、当前帧号与数据负载，</para>
/// <para>并提供二进制序列化（Serialize）与反序列化（从字节流构造）能力。</para>
/// </summary>
public abstract class NetworkMessage<T>{
	#region public instance properties
	/// <summary>消息类型。</summary>
	public NetworkMessageType MessageType{get; private set;}
	/// <summary>发送方玩家索引。</summary>
	public int PlayerIndex{get; private set;}
	/// <summary>消息关联的当前帧号。</summary>
	public long CurrentFrame{get; private set;}
	/// <summary>消息数据负载。</summary>
	public T Data{get; private set;}
	#endregion
	
	#region protected instance constructors
	/// <summary>
	/// 构造函数（直接指定各字段）。
	/// </summary>
	/// <param name="messageType">消息类型。</param>
	/// <param name="playerIndex">玩家索引。</param>
	/// <param name="currentFrame">当前帧号。</param>
	/// <param name="data">数据负载。</param>
	protected NetworkMessage(NetworkMessageType messageType, int playerIndex, long currentFrame, T data){
		this.MessageType = messageType;
		this.PlayerIndex = playerIndex;
		this.CurrentFrame = currentFrame;
		this.Data = data;
	}
	
	/// <summary>
	/// 构造函数（从序列化字节流反序列化）。
	/// </summary>
	/// <param name="serializedNetworkMessage">序列化的消息字节。</param>
	protected NetworkMessage(byte[] serializedNetworkMessage){
		using (MemoryStream stream = new MemoryStream(serializedNetworkMessage)){
			using (BinaryReader reader = new BinaryReader(stream)){
				// Read the information from the stream...
				this.MessageType = (NetworkMessageType)reader.ReadByte();
				this.PlayerIndex = reader.ReadInt32();
				this.CurrentFrame = reader.ReadInt64();
				this.Data = this.ReadFromStream(reader);
			}
		}
	}
	#endregion
	
	#region public instance methods
	/// <summary>
	/// 将消息序列化为字节数组。
	/// </summary>
	/// <returns>序列化后的字节数组。</returns>
	public byte[] Serialize(){
		using (MemoryStream stream = new MemoryStream()){
			using (BinaryWriter writer = new BinaryWriter(stream)){
				// Write the information into the stream...
				writer.Write((byte)this.MessageType);
				writer.Write(this.PlayerIndex);
				writer.Write(this.CurrentFrame);
				this.AddToStream(writer, this.Data);
				writer.Flush();
				
				// and return the information stored in the stream as a byte[]
				return stream.ToArray();
			}
		}
	}
	#endregion

	#region public override methods
	/// <summary>
	/// 生成可读的调试字符串。
	/// </summary>
	/// <returns>调试字符串。</returns>
	public override string ToString (){
		return string.Format(
			"[{0} | messageType = {1} | playerIndex = {2} | currentFrame = {3} | data = {4}]",
			this.GetType().ToString(),
			this.MessageType,
			this.PlayerIndex,
			this.CurrentFrame,
			this.Data.ToString()
		);
	}
	#endregion
	
	#region protected instance methods
	/// <summary>将数据写入二进制流（由子类实现具体序列化）。</summary>
	/// <param name="writer">二进制写入器。</param>
	/// <param name="data">数据。</param>
	protected abstract void AddToStream(BinaryWriter writer, T data);
	/// <summary>从二进制流读取数据（由子类实现具体反序列化）。</summary>
	/// <param name="reader">二进制读取器。</param>
	/// <returns>读取到的数据。</returns>
	protected abstract T ReadFromStream(BinaryReader reader);
	#endregion
}
