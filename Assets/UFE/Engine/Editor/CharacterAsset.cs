using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// 角色资产创建工具（CharacterAsset，编辑器专用）。
/// <para>用途：在 Assets/Create/U.F.E. 菜单中提供"创建角色文件"入口，创建新的 CharacterInfo 资产。</para>
/// </summary>
public class CharacterAsset
{
	/// <summary>
	/// 创建新的角色信息资产（菜单入口）。
	/// </summary>
	[MenuItem("Assets/Create/U.F.E./Character File")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<UFE3D.CharacterInfo> ();
    }
}
