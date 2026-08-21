using UnityEngine;
using System;
using System.IO;

/// <summary>
/// 简单游戏状态（FluxSimpleState）。
/// <para>用途：用于网络同步/反同步检测的轻量游戏状态快照——仅含双方生命/能量/位置与帧号，</para>
/// <para>支持二进制序列化/反序列化，可按配置决定是否包含完整位置数据（desynchronizationRecovery）。</para>
/// </summary>
[Serializable]
public struct FluxSimpleState : IEquatable<FluxSimpleState>{
	#region public class definitions
	/// <summary>
	/// 玩家信息：一名玩家的生命/能量/位置快照。
	/// </summary>
	public struct PlayerInformation : IEquatable<PlayerInformation>{
		/// <summary>生命值。</summary>
		public float life;
		/// <summary>能量值。</summary>
		public float gauge;
		/// <summary>位置。</summary>
		public Vector3 position;

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="life">生命值。</param>
		/// <param name="gauge">能量值。</param>
		/// <param name="position">位置。</param>
		public PlayerInformation(float life, float gauge, Vector3 position){
			this.life = life;
			this.gauge = gauge;
			this.position = position;
		}

		/// <summary>
		/// 生成哈希码。
		/// </summary>
		/// <returns>哈希码。</returns>
		public override int GetHashCode (){
			unchecked{
				return 11 * this.life.GetHashCode() + 13 * this.gauge.GetHashCode() + 17 * this.position.GetHashCode();
			}
		}

		/// <summary>
		/// 判断对象是否相等。
		/// </summary>
		/// <param name="obj">比较对象。</param>
		/// <returns>相等返回 true。</returns>
		public override bool Equals (object obj){
			if (obj is PlayerInformation){
				return this.Equals((PlayerInformation)obj);
			}
			return false;
		}

		/// <summary>
		/// 判断两个玩家信息是否相等。
		/// </summary>
		/// <param name="other">另一个玩家信息。</param>
		/// <returns>相等返回 true。</returns>
		public bool Equals(PlayerInformation other){
			return this.life == other.life && this.gauge == other.gauge && Vector3.Equals(this.position, other.position);
		}

		/// <summary>
		/// 生成可读的调试字符串。
		/// </summary>
		/// <returns>调试字符串。</returns>
		public override string ToString (){
			return string.Format(
				"[PlayerInformation | position = ({0}, {1}, {2}) | life = {3} | gauge = {4}]", 
				this.position.x,
				this.position.y,
				this.position.z,
				this.life, 
				this.gauge
			);
		}
	}
	#endregion

	#region public instance properties
	/// <summary>玩家1状态。</summary>
	public PlayerInformation p1;
	/// <summary>玩家2状态。</summary>
	public PlayerInformation p2;

	/// <summary>快照帧号。</summary>
	public long frame;

	#endregion

	#region public instance constructors
	/// <summary>
	/// 构造函数。
	/// </summary>
	/// <param name="p1">玩家1状态。</param>
	/// <param name="p2">玩家2状态。</param>
	/// <param name="frame">帧号。</param>
	public FluxSimpleState(PlayerInformation p1, PlayerInformation p2, long frame){
		this.p1 = p1;
		this.p2 = p2;
		this.frame = frame;
	}

	/// <summary>
	/// 构造函数（从完整游戏状态 FluxStates 提取）。
	/// </summary>
	/// <param name="state">完整游戏状态。</param>
	public FluxSimpleState(FluxStates state){
		this.p1 = new PlayerInformation((float)state.player1.life, (float)state.player1.gauge,state.player1.shellTransform.position);
		this.p2 = new PlayerInformation((float)state.player2.life, (float)state.player2.gauge,state.player2.shellTransform.position);
		this.frame = state.networkFrame;
	}
	#endregion

	#region IEquatable<FluxSimpleState> implementation
	/// <summary>
	/// 判断对象是否相等。
	/// </summary>
	/// <param name="obj">比较对象。</param>
	/// <returns>相等返回 true。</returns>
	public override bool Equals(object obj){
		return (obj is FluxSimpleState) && this.Equals((FluxSimpleState)obj);
	}

	/// <summary>
	/// 生成哈希码。
	/// </summary>
	/// <returns>哈希码。</returns>
	public override int GetHashCode (){
		unchecked{
			return 
				11 * this.p1.GetHashCode() + 
				13 * this.p2.GetHashCode() + 
				17 * this.frame.GetHashCode();
		}
	}

	/// <summary>
	/// 判断两个状态是否相等。
	/// </summary>
	/// <param name="other">另一个状态。</param>
	/// <returns>相等返回 true。</returns>
	public bool Equals(FluxSimpleState other){
		return 
			this.p1.Equals(other.p1) && 
			this.p2.Equals(other.p2) &&
			this.frame.Equals(other.frame);
	}
	#endregion

	#region public instance methods
	/// <summary>
	/// 序列化当前状态。
	/// </summary>
	/// <returns>序列化字节。</returns>
	public byte[] Serialize(){
		return FluxSimpleState.Serialize(this);
	}
	#endregion

	#region public override methods
	/// <summary>
	/// 生成可读的调试字符串。
	/// </summary>
	/// <returns>调试字符串。</returns>
	public override string ToString (){
		return string.Format(
			"[FluxSimpleState | p1 = {0} | p2 = {1} | frame = {2}]", 
			this.p1.ToString(), 
			this.p2.ToString(), 
			this.frame
		);
	}
	#endregion

	#region public class methods
	/// <summary>
	/// 将状态写入二进制流（反同步恢复开启时包含完整位置，否则仅生命与帧号）。
	/// </summary>
	/// <param name="writer">二进制写入器。</param>
	/// <param name="gameState">游戏状态。</param>
	public static void AddToStream(BinaryWriter writer, FluxSimpleState gameState){
		if (UFE.config.networkOptions.desynchronizationRecovery){
			writer.Write(gameState.p1.life);
			writer.Write(gameState.p1.gauge);
			writer.Write(gameState.p1.position.x);
			writer.Write(gameState.p1.position.y);
			writer.Write(gameState.p1.position.z);

			writer.Write(gameState.p2.life);
			writer.Write(gameState.p2.gauge);
			writer.Write(gameState.p2.position.x);
			writer.Write(gameState.p2.position.y);
			writer.Write(gameState.p2.position.z);

			writer.Write(gameState.frame);
		}else{
			writer.Write(gameState.p1.life);
			writer.Write(gameState.p2.life);
			writer.Write(gameState.frame);
		}
	}


	/// <summary>
	/// 从字节数组反序列化状态。
	/// </summary>
	/// <param name="bytes">序列化字节。</param>
	/// <returns>游戏状态。</returns>
	public static FluxSimpleState Deserialize(byte[] bytes){
		using (MemoryStream stream = new MemoryStream(bytes)){
			using (BinaryReader reader = new BinaryReader(stream)){
				return FluxSimpleState.ReadFromStream(reader);
			}
		}
	}

	/// <summary>
	/// 从二进制流读取状态。
	/// </summary>
	/// <param name="reader">二进制读取器。</param>
	/// <returns>游戏状态。</returns>
	public static FluxSimpleState ReadFromStream(BinaryReader reader){
		if (UFE.config.networkOptions.desynchronizationRecovery){
			return new FluxSimpleState(
				new PlayerInformation(
					reader.ReadSingle(),
					reader.ReadSingle(),
					new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
				)
				,
				new PlayerInformation(
					reader.ReadSingle(),
					reader.ReadSingle(),
					new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
				)
				,
				reader.ReadInt64()
			);
		}else{
			float p1Life = reader.ReadSingle();
			float p2Life = reader.ReadSingle();
			long frame = reader.ReadInt64();

            return new FluxSimpleState(
				new PlayerInformation(p1Life, 0f, Vector3.zero),
				new PlayerInformation(p2Life, 0f, Vector3.zero),
				frame
			);
		}
	}

	/// <summary>
	/// 序列化游戏状态为字节数组。
	/// </summary>
	/// <param name="gameState">游戏状态。</param>
	/// <returns>序列化字节。</returns>
	public static byte[] Serialize(FluxSimpleState gameState){
		using (MemoryStream stream = new MemoryStream()){
			using (BinaryWriter writer = new BinaryWriter(stream)){
				FluxSimpleState.AddToStream(writer, gameState);
				writer.Flush();
				return stream.ToArray();
			}
		}
	}
	#endregion
}
