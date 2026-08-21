using UnityEngine;
using UnityEditor;
using System;
using UFE3D;

/// <summary>
/// 招式资产创建工具（MoveAsset，编辑器专用）。
/// <para>用途：在 Assets/Create/U.F.E. 菜单中提供"创建招式文件"入口，创建新的 MoveInfo 资产。</para>
/// </summary>
public class MoveAsset
{
	/// <summary>
	/// 创建新的招式信息资产（菜单入口）。
	/// </summary>
    [MenuItem("Assets/Create/U.F.E./Move File")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<MoveInfo> ();
    }
}
