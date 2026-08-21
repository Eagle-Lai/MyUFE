using UnityEngine;
using UnityEditor;

/// <summary>
/// UFE 输入设置工具（UFEInputSetup，编辑器专用）。
/// <para>用途：提供菜单命令用 UFE 预设的 InputManager.asset 覆盖项目的输入管理器配置，</para>
/// <para>执行前自动备份原文件，完成后提示重启 Unity。</para>
/// </summary>
public class UFEInputSetup : EditorWindow {
	// creates a menu entry that replaces a file.
	/// <summary>
	/// 覆盖项目 InputManager.asset 文件（菜单入口）。
	/// </summary>
	[MenuItem("Window/U.F.E./Project Settings/Override Input Manager")]
	static void ReplaceInputManagerAssetFile() {
		string path = Application.dataPath;
		path = path.Remove(path.Length - 6);

		string destPath = path + "ProjectSettings/";
		string sourcePath = path = "Assets/UFE/Engine/ProjectSettings/";
		bool exit = false;

		if (UnityEditor.EditorUtility.DisplayDialog("Replace InputManager.asset file", "Replace InputManager.asset file with one designed to work with UFE?"
													+ " (A backup will be made.)"
													+ "\n\nMake sure the InputManager settings are NOT open in the inspector or this won't work!", "OK", "Cancel")) {
			if (!System.IO.File.Exists(destPath + "InputManager.backup")) {
				// backup the old intputmanager.asset file
				FileUtil.CopyFileOrDirectory(destPath + "InputManager.asset", destPath + "InputManager.backup");
			}

			// copy the new inputmanager.asset file over the old version
			FileUtil.ReplaceFile(sourcePath + "UFEInput.asset", destPath + "InputManager.asset");
			exit = true;
		}

		if (exit && UnityEditor.EditorUtility.DisplayDialog("Restart Unity", "Inputmanager.asset successfully installed.\nUnity will have to restart now.\nClose the Editor?", "Yes", "No, I'll do it myself")) {
			EditorApplication.Exit(0);
		}
	}
}
