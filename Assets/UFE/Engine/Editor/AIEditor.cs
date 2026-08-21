using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UFE3D;

/// <summary>
/// AI 信息自定义 Inspector（AIEditor，编辑器专用）。
/// <para>用途：当选中 AIInfo 资产时，在 Inspector 显示"打开 AI 编辑器"按钮，启动 AIEditorWindow。</para>
/// </summary>
[CustomEditor(typeof(AIInfo))]
public class AIEditor : Editor {
	/// <summary>
	/// 绘制自定义 Inspector GUI：显示"打开 AI 编辑器"按钮。
	/// </summary>
	public override void OnInspectorGUI(){
		if (GUILayout.Button("Open A.I. Editor")) 
			AIEditorWindow.Init();
			
	}
}
