using UnityEngine;
using UnityEditor;
using System;
using UFE3D;

/// <summary>
/// AI 资产创建工具（AIAsset，编辑器专用）。
/// <para>用途：在 Assets/Create/U.F.E. 菜单中提供"创建 AI 指令文件"入口，创建新的 AIInfo 资产。</para>
/// </summary>
public class AIAsset
{
	/// <summary>
	/// 创建新的 AIInfo 资产（菜单入口）。
	/// </summary>
	[MenuItem("Assets/Create/U.F.E./A.I. File")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<AIInfo> ();
    }
}
