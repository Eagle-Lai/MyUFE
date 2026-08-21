using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UFE3D;

/// <summary>
/// 姿态信息自定义 Inspector（StanceEditor，编辑器专用）。
/// <para>用途：当选中姿态资产时，在 Inspector 显示姿态标签与"打开角色编辑器"按钮。</para>
/// </summary>
[CustomEditor(typeof(StanceInfo))]
public class StanceEditor : Editor {
	/// <summary>
	/// 绘制自定义 Inspector GUI：显示姿态标签与打开角色编辑器的按钮。
	/// </summary>
    public override void OnInspectorGUI()
    {
        GUILayout.Label("Stance File");
        if (GUILayout.Button("Open Character Editor"))
            CharacterEditorWindow.Init();

    }
}
