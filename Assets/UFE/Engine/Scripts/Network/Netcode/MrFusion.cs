using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFENetcode;

/// <summary>
/// MrFusion（帧同步状态记录器）。
/// <para>用途：为挂在同一物体上的所有 UFE 接口/行为组件提供逐帧状态保存与加载（快照），</para>
/// <para>供帧同步回滚（Rollback）在反同步时恢复任意帧的状态。</para>
/// </summary>
public class MrFusion : MonoBehaviour {
	/// <summary>调试开关（加载空接口时输出警告）。</summary>
    public bool debugger = false;

	/// <summary>可追踪接口的内部结构（接口引用 + 状态追踪字典）。</summary>
    private struct TrackableInterface
    {
		/// <summary>UFE 接口引用。</summary>
        public UFEInterface ufeInterface;
		/// <summary>该接口的字段状态追踪字典（MemberInfo→值）。</summary>
        public Dictionary<System.Reflection.MemberInfo, System.Object> tracker;
    }

	/// <summary>按帧号保存的接口状态历史。</summary>
    private Dictionary<long, TrackableInterface[]> gameHistory = new Dictionary<long, TrackableInterface[]>();
	/// <summary>所有 UFE 接口组件。</summary>
    private UFEInterface[] ufeInterfaces;
	/// <summary>所有 UFE 行为组件。</summary>
    private UFEBehaviour[] ufeBehaviours;


	/// <summary>
	/// 启动：收集子物体上的全部 UFE 接口与行为组件。
	/// </summary>
    void Start () {
        ufeInterfaces = GetComponentsInChildren<UFEInterface>();
        ufeBehaviours = GetComponentsInChildren<UFEBehaviour>();
    }

	/// <summary>
	/// 调用全部 UFE 行为的固定帧更新（帧同步推进）。
	/// </summary>
    public void UpdateBehaviours(){
        if (ufeBehaviours == null) return;
        foreach (UFEBehaviour ufeBehaviour in ufeBehaviours) {
            ufeBehaviour.UFEFixedUpdate();
        }
    }

	/// <summary>
	/// 保存指定帧的全部接口状态（快照）。
	/// </summary>
	/// <param name="frame">帧号。</param>
    public void SaveState(long frame)
    {
        List<TrackableInterface> newTrackableList = new List<TrackableInterface>();
        foreach(UFEInterface ufeInterface in ufeInterfaces)
        {
            TrackableInterface newTrackableInterface;
            newTrackableInterface.ufeInterface = ufeInterface;
            newTrackableInterface.tracker = RecordVar.SaveStateTrackers(ufeInterface, new Dictionary<System.Reflection.MemberInfo, object>());
            newTrackableList.Add(newTrackableInterface);
        }

        if (gameHistory.ContainsKey(frame)) {
            gameHistory[frame] = newTrackableList.ToArray();
        } else {
            gameHistory.Add(frame, newTrackableList.ToArray());
        }
    }

	/// <summary>
	/// 加载指定帧的接口状态（恢复快照）。
	/// </summary>
	/// <param name="frame">帧号。</param>
    public void LoadState(long frame)
    {
        if (gameHistory.ContainsKey(frame)) {
            TrackableInterface[] loadedInterfaces = gameHistory[frame];
            foreach (TrackableInterface trackableInterface in loadedInterfaces)
            {
                UFEInterface reflectionTarget = trackableInterface.ufeInterface;
                reflectionTarget = RecordVar.LoadStateTrackers(trackableInterface.ufeInterface, trackableInterface.tracker);
                if (reflectionTarget == null && debugger) Debug.LogWarning("Empty interface found at '"+ trackableInterface.ToString() + "'");
            }
        } else {
            Debug.LogError("Frame data not found (" + frame + ")");
        }
    }

}
