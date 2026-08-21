using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using UFE3D;

/// <summary>
/// ScriptableObject 资产创建工具（ScriptableObjectUtility，编辑器专用）。
/// <para>用途：创建/更新 UFE 配置类资产（GlobalInfo/CharacterInfo/MoveInfo/StanceInfo/AIInfo），</para>
/// <para>自动生成唯一路径、设置文件名称、选中新资产，并按类型打开对应的编辑器窗口。</para>
/// </summary>
public static class ScriptableObjectUtility
{
	/// <summary>
	/// 创建（或覆盖）指定类型的 UFE 资产。
	/// <para>若传入了 data/oldFile，则在旧文件路径上保存（用于编辑器保存场景）；否则在 Assets 下新建唯一命名资产。</para>
	/// </summary>
	/// <typeparam name="T">资产类型（ScriptableObject 子类）。</typeparam>
	/// <param name="data">可选的资产数据（更新已有资产时传入）。</param>
	/// <param name="oldFile">可选的旧资产引用（决定保存路径与文件名）。</param>
    public static void CreateAsset<T> (T data = null, T oldFile = null) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T> ();
        Object referencePath = Selection.activeObject;
        if (data != null) {
            asset = data;
            if (oldFile != null) referencePath = oldFile;
        }
        
        string path = AssetDatabase.GetAssetPath (referencePath);
        if (path == "") {
            path = "Assets";
        } else if (Path.GetExtension (path) != "") {
            path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (referencePath)), "");
        }

        string fileName;
        if (oldFile != null) {
            fileName = oldFile.name;
		}else if (asset is MoveInfo) {
			fileName = "New Move";
		}else if (asset is UFE3D.CharacterInfo) {
			fileName = "New Character";
		}else if (asset.GetType().ToString().Equals("UFE3D.AIInfo")) {
			fileName = "New AI Instructions";
		}else if (asset is GlobalInfo) {
			fileName = "New UFE Config";
		}else if (asset is StanceInfo) {
            fileName = "New Combat Stance";
		}else{
			fileName = typeof(T).ToString();
		}
        string assetPathAndName = oldFile != null? path + fileName + ".asset" : AssetDatabase.GenerateUniqueAssetPath (path + "/" + fileName + ".asset");
        
        if (!AssetDatabase.Contains(asset)) AssetDatabase.CreateAsset (asset, assetPathAndName);
        
        AssetDatabase.SaveAssets ();
        EditorUtility.FocusProjectWindow ();
        Selection.activeObject = asset;
		
		if (asset is MoveInfo) {
			MoveEditorWindow.Init();
		}else if (asset is GlobalInfo) {
			GlobalEditorWindow.Init();
		}else if (asset.GetType().ToString().Equals("UFE3D.AIInfo")){
			UFE.SearchClass("AIEditorWindow").GetMethod(
				"Init", 
				BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
				null,
				null,
				null
			).Invoke(null, new object[]{});
		}else if (asset is UFE3D.CharacterInfo) {
			CharacterEditorWindow.Init();
		}
		
    }
}
