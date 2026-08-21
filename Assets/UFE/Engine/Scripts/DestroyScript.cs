using UnityEngine;
using System.Collections;

/// <summary>
/// 自动销毁脚本（DestroyScript）。
/// <para>用途：挂载到帧同步实例化对象上，启动时通过 UFE.DestroyGameObject 在指定帧数后由帧同步系统统一销毁该对象。</para>
/// </summary>
public class DestroyScript : MonoBehaviour {
	/// <summary>销毁延迟帧数（从创建起算）。</summary>
	public int destroyTime = 30; // frames
	
	/// <summary>
	/// 启动：向帧同步系统注册延迟销毁。
	/// </summary>
	void Start () {
        UFE.DestroyGameObject(gameObject, destroyTime);
	}
}
