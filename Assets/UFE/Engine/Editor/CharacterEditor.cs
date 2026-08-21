using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 角色信息自定义 Inspector（CharacterEditor，编辑器专用）。
/// <para>用途：当选中角色资产时，在 Inspector 显示"打开角色编辑器"按钮，启动 CharacterEditorWindow。</para>
/// </summary>
[CustomEditor(typeof(UFE3D.CharacterInfo))]
public class CharacterEditor : Editor {
	/// <summary>
	/// 绘制自定义 Inspector GUI：显示"打开角色编辑器"按钮。
	/// </summary>
	public override void OnInspectorGUI(){
		if (GUILayout.Button("Open Character Editor")) 
			CharacterEditorWindow.Init();
			
	}
}
