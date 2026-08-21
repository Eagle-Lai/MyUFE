using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// GUI 扩展（GUIExtensions）。
/// <para>用途：为 EventTrigger 与 EventSystem 提供便捷的监听器注册与选中方法。</para>
/// </summary>
public static class GUIExtensions
{
	/// <summary>
	/// 为 EventTrigger 添加指定类型的事件监听（已存在该类型则追加监听器）。
	/// </summary>
	/// <param name="eventTrigger">目标 EventTrigger 组件。</param>
	/// <param name="type">事件类型（点击/拖动等）。</param>
	/// <param name="action">事件回调。</param>
	public static void AddListener(this EventTrigger eventTrigger, EventTriggerType type, UnityAction<BaseEventData> action)
	{
		if (eventTrigger.triggers == null)
		{
			eventTrigger.triggers = new List<EventTrigger.Entry>();
		}
		var entry = eventTrigger.triggers.Find(e => e.eventID == type);
		
		if (entry == null)
		{
			entry = new EventTrigger.Entry();
			entry.eventID = type;
			entry.callback = new EventTrigger.TriggerEvent();
			
			eventTrigger.triggers.Add(entry);
		}
		entry.callback.AddListener(action);
		
	}

	/// <summary>
	/// 将指定 MonoBehaviour 物体设为 EventSystem 当前选中物体。
	/// </summary>
	/// <param name="eventSystem">事件系统。</param>
	/// <param name="selected">要选中的物体。</param>
	public static void SetSelected(this EventSystem eventSystem, MonoBehaviour selected)
	{
		var pointer = new BaseEventData(eventSystem);
		eventSystem.SetSelectedGameObject(selected.gameObject, pointer);
	}
	
	/// <summary>
	/// 将指定 GameObject 设为 EventSystem 当前选中物体。
	/// </summary>
	/// <param name="eventSystem">事件系统。</param>
	/// <param name="selected">要选中的物体。</param>
	public static void SetSelected(this EventSystem eventSystem, GameObject selected)
	{
		var pointer = new BaseEventData(eventSystem);
		eventSystem.SetSelectedGameObject(selected, pointer);
	}
}
