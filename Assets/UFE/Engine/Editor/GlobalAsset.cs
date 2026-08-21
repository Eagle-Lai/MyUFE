using UnityEngine;
using UnityEditor;
using System;
using UFE3D;

/// <summary>
/// 全局配置资产创建工具（GlobalAsset，编辑器专用）。
/// <para>用途：在 Assets/Create/U.F.E. 菜单中提供"创建配置文件"入口，创建新的 GlobalInfo 资产。</para>
/// </summary>
public class GlobalAsset
{
	/// <summary>
	/// 创建新的全局配置资产（菜单入口）。
	/// </summary>
	[MenuItem("Assets/Create/U.F.E./Config File")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<GlobalInfo> ();
    }
}
