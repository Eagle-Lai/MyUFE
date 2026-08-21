using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UFE3D;

/// <summary>
/// 招式信息自定义 Inspector（MoveEditor，编辑器专用）。
/// <para>用途：当选中招式资产（支持多选）时，在 Inspector 显示"打开招式编辑器"按钮，启动 MoveEditorWindow。</para>
/// </summary>
[CustomEditor(typeof(MoveInfo))]
[CanEditMultipleObjects]
public class MoveEditor : Editor {
	/// <summary>
	/// 绘制自定义 Inspector GUI：显示"打开招式编辑器"按钮。
	/// </summary>
	public override void OnInspectorGUI(){
		if (GUILayout.Button("Open Move Editor")) 
			MoveEditorWindow.Init();
		
	}
}
