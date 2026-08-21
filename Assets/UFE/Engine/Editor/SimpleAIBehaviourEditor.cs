using UnityEngine;
using UnityEditor;

/// <summary>
/// 简单 AI 行为自定义 Inspector（SimpleAIBehaviourEditor，编辑器专用）。
/// <para>用途：当选中 SimpleAIBehaviour 资产时，在 Inspector 显示"打开简单 AI 编辑器"按钮。</para>
/// </summary>
[CustomEditor(typeof(SimpleAIBehaviour))]
public class SimpleAIBehaviourEditor : Editor{
	/// <summary>
	/// 绘制自定义 Inspector GUI：显示"打开简单 AI 编辑器"按钮。
	/// </summary>
	public override void OnInspectorGUI(){
		if (GUILayout.Button("Open Simple AI Editor")){
			SimpleAIBehaviourEditorWindow.Init();
		}
	}
}
