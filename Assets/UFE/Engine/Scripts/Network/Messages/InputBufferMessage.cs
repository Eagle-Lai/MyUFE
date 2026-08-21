using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// 输入缓冲消息（InputBufferMessage）。
/// <para>用途：携带输入缓冲内容（InputBufferMessageContent）的网络消息，用于帧同步中客户端之间交换逐帧输入，</para>
/// <para>按消息位数（8/16/32 位）与是否强制数字输入选择对应的序列化格式。</para>
/// </summary>
public class InputBufferMessage : NetworkMessage<InputBufferMessageContent>{
	#region public override methods
	/// <summary>
	/// 构造函数（直接指定字段）。
	/// </summary>
	/// <param name="playerIndex">玩家索引。</param>
	/// <param name="currentFrame">当前帧号。</param>
	/// <param name="data">输入缓冲内容。</param>
	public InputBufferMessage(int playerIndex, long currentFrame, InputBufferMessageContent data) : 
	base(NetworkMessageType.InputBuffer, playerIndex, currentFrame, data){}
	
	/// <summary>
	/// 构造函数（从序列化字节流反序列化，校验消息类型）。
	/// </summary>
	/// <param name="serializedNetworkMessage">序列化的消息字节。</param>
	public InputBufferMessage(byte[] serializedNetworkMessage) : base(serializedNetworkMessage){
		if (this.MessageType != NetworkMessageType.InputBuffer){
			throw new System.FormatException(string.Format(
				"The message type was {0}, but it should have been {1}.",
				this.MessageType,
				NetworkMessageType.InputBuffer
			));
		}
	}
	#endregion

	#region protected override methods
	/// <summary>
	/// 将输入缓冲内容写入二进制流（按数字/模拟输入与消息位数选择格式）。
	/// </summary>
	/// <param name="writer">二进制写入器。</param>
	/// <param name="data">输入缓冲内容。</param>
	protected override void AddToStream(BinaryWriter writer, InputBufferMessageContent data){
		writer.Write(data.NextExpectedFrame);
		writer.Write (data.InputBuffer != null ? data.InputBuffer.Count : 0);

		if (UFE.config.inputOptions.forceDigitalInput) {
			for (int i = 0; i < data.InputBuffer.Count; ++i){
				writer.Write(data.InputBuffer[i].Item1);

				NetworkButtonPress button = data.InputBuffer[i].Item2.buttons;
				if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size8Bits){
					button &= (NetworkButtonPress)((sbyte)(-1));
					writer.Write((byte)button);
				}else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size16Bits){
					button &= (NetworkButtonPress)((short)(-1));
					writer.Write((ushort)button);
				}else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size32Bits){
					writer.Write((uint)button);
				}

				writer.Write(data.InputBuffer[i].Item2.selectedOption);
			}
		}else{
			for (int i = 0; i < data.InputBuffer.Count; ++i){
				writer.Write(data.InputBuffer[i].Item1);
				writer.Write((float)data.InputBuffer[i].Item2.horizontalAxisRaw);
				writer.Write((float)data.InputBuffer[i].Item2.verticalAxisRaw);

				NetworkButtonPress button = data.InputBuffer[i].Item2.buttons;
				if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size8Bits){
					button &= (NetworkButtonPress)((sbyte)(-1));
					writer.Write((byte)button);
				}else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size16Bits){
					button &= (NetworkButtonPress)((short)(-1));
					writer.Write((ushort)button);
				}else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size32Bits){
					writer.Write((uint)button);
				}

				writer.Write(data.InputBuffer[i].Item2.selectedOption);
			}
		}
	}

	/// <summary>
	/// 从二进制流读取输入缓冲内容。
	/// </summary>
	/// <param name="reader">二进制读取器。</param>
	/// <returns>输入缓冲内容。</returns>
	protected override InputBufferMessageContent ReadFromStream(BinaryReader reader){
		long nextExpectedFrame = reader.ReadInt64();
		Tuple<long, FrameInput>[] buffer = new Tuple<long, FrameInput>[reader.ReadInt32()];

		if (UFE.config.inputOptions.forceDigitalInput){
			if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size8Bits){
				for (int i = 0; i < buffer.Length; ++i){
					buffer[i] = new Tuple<long, FrameInput>(
						reader.ReadInt64()
						,
						new FrameInput(
							(NetworkButtonPress)reader.ReadByte(),
							reader.ReadSByte()
						)
					);
				}
			}else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size16Bits){
				for (int i = 0; i < buffer.Length; ++i){
					buffer[i] = new Tuple<long, FrameInput>(
						reader.ReadInt64()
						,
						new FrameInput(
							(NetworkButtonPress)reader.ReadUInt16(),
							reader.ReadSByte()
						)
					);
				}
			}else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size32Bits){
				for (int i = 0; i < buffer.Length; ++i){
					buffer[i] = new Tuple<long, FrameInput>(
						reader.ReadInt64()
						,
						new FrameInput(
							(NetworkButtonPress)reader.ReadUInt32(),
							reader.ReadSByte()
						)
					);
				}
			}
		}else{
			if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size8Bits){
				for (int i = 0; i < buffer.Length; ++i){
					buffer[i] = new Tuple<long, FrameInput>(
						reader.ReadInt64()
						,
						new FrameInput(
							reader.ReadSingle(),
							reader.ReadSingle(),
							(NetworkButtonPress)reader.ReadByte(),
							reader.ReadSByte()
						)
					);
				}
			}else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size16Bits){
				for (int i = 0; i < buffer.Length; ++i){
					buffer[i] = new Tuple<long, FrameInput>(
						reader.ReadInt64()
						,
						new FrameInput(
							reader.ReadSingle(),
							reader.ReadSingle(),
							(NetworkButtonPress)reader.ReadUInt16(),
							reader.ReadSByte()
						)
					);
				}
			}else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size32Bits){
				for (int i = 0; i < buffer.Length; ++i){
					buffer[i] = new Tuple<long, FrameInput>(
						reader.ReadInt64()
						,
						new FrameInput(
							reader.ReadSingle(),
							reader.ReadSingle(),
							(NetworkButtonPress)reader.ReadUInt32(),
							reader.ReadSByte()
						)
					);
				}
			}
		}
		
		return new InputBufferMessageContent(nextExpectedFrame, buffer);
	}
	#endregion
}
