using UnityEngine;
using UFENetcode;

/// <summary>
/// 实例化游戏对象（InstantiatedGameObject）。
/// <para>用途：在帧同步（Netcode）系统中跟踪一个运行时实例化的游戏对象，记录其创建帧、销毁帧及关联的帧同步组件（MrFusion）。</para>
/// <para>用于保证网络对战中实例化/销毁操作在各客户端帧对齐。</para>
/// </summary>
public class InstantiatedGameObject {
	/// <summary>
	/// 被实例化的游戏对象引用。
	/// </summary>
	public GameObject gameObject;
	/// <summary>
	/// 关联的帧同步组件引用（MrFusion，用于网络状态同步）。
	/// </summary>
	public MrFusion mrFusion;
	/// <summary>
	/// 对象创建的帧号（帧同步时间轴）。
	/// </summary>
	public long creationFrame;
	/// <summary>
	/// 对象销毁的帧号（可空，null 表示尚未销毁）。
	/// </summary>
	public long? destructionFrame;

	/// <summary>
	/// 是否标记为需要销毁（延迟销毁标记）。
	/// </summary>
    public bool destroyMe;

	/// <summary>
	/// 构造函数：创建实例化对象记录。
	/// </summary>
	/// <param name="gameObject">被实例化的游戏对象。</param>
	/// <param name="mrFusion">关联的帧同步组件。</param>
	/// <param name="creationFrame">创建帧号。</param>
	/// <param name="destructionFrame">销毁帧号（可空）。</param>
	public InstantiatedGameObject(
		GameObject gameObject = null,
        MrFusion mrFusion = null,
        long creationFrame = 0, 
		long? destructionFrame = null
	){
        this.gameObject = gameObject;
        this.mrFusion = mrFusion;
        this.creationFrame = creationFrame;
		this.destructionFrame = destructionFrame != null ? new long?(destructionFrame.Value) : null;
	}

	/// <summary>
	/// 拷贝构造函数：从另一个实例化对象记录复制数据。
	/// </summary>
	/// <param name="other">要复制的源记录。</param>
	public InstantiatedGameObject(InstantiatedGameObject other) : this(
		other.gameObject,
		other.mrFusion,
        other.creationFrame,
		other.destructionFrame
	){}
    
	/// <summary>
	/// 判断该对象是否已被销毁。
	/// </summary>
	/// <returns>true 表示对象已被销毁（自身为 null 或 destroyMe 标记为 true）。</returns>
    public bool IsDestroyed()
    {
        if (this == null) return true;
        return destroyMe;
    }
}
