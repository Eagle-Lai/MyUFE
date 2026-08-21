using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UFE3D;

/// <summary>
/// 全局配置自定义 Inspector（GlobalEditor，编辑器专用）。
/// <para>用途：当选中 GlobalInfo 资产时，在 Inspector 显示"打开全局配置"按钮，启动 GlobalEditorWindow。</para>
/// </summary>
[CustomEditor(typeof(GlobalInfo))]
public class GlobalEditor : Editor {
	/// <summary>
	/// 绘制自定义 Inspector GUI：显示"打开全局配置"按钮。
	/// </summary>
	public override void OnInspectorGUI(){
		if (GUILayout.Button("Open U.F.E Global Config")) 
			GlobalEditorWindow.Init();
		
	}
}
