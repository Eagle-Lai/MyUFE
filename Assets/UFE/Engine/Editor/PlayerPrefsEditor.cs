using UnityEngine;
using UnityEditor;

/// <summary>
/// PlayerPrefs 编辑器工具（PlayerPrefsEditor，编辑器专用）。
/// <para>用途：提供"清除全部 PlayerPrefs"菜单命令（调试/重置数据用）。</para>
/// </summary>
public static class PlayerPrefsEditor{
	/// <summary>
	/// 清空全部 PlayerPrefs 数据（菜单入口）。
	/// </summary>
	[MenuItem("Window/U.F.E./Clear PlayerPrefs")]
	public static void Clear(){
		PlayerPrefs.DeleteAll();
	}
}
