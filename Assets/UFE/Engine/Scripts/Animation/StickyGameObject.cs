using UnityEngine;
using System.Collections;

/// <summary>
/// 粘性游戏对象（StickyGameObject）。
/// <para>用途：让子物体（如粒子特效）每帧跟随父级 Transform 的旋转，实现"贴附在角色骨骼上并随角色转向"的效果。</para>
/// </summary>
public class StickyGameObject : MonoBehaviour {

	/// <summary>旋转偏移（预留，当前未使用）。</summary>
    public Quaternion rotationOffSet;
	/// <summary>父级 Transform 引用。</summary>
    private Transform parentTransform;

	/// <summary>
	/// 启动：缓存父级 Transform。
	/// </summary>
    void Start() {
        parentTransform = GetComponentInParent<Transform>();
    }

	/// <summary>
	/// 固定帧更新：将自身旋转同步为父级旋转。
	/// </summary>
	void FixedUpdate()
    {
        if (parentTransform != null) {
            transform.rotation = parentTransform.rotation;
            //transform.localRotation = parentTransform.localRotation;
        }
	
	}
}
