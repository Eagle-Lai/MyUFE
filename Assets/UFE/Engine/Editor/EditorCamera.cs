using UnityEngine; 
using UnityEditor; 
using System.Collections; 
using System.Reflection;  

/// <summary>
/// 编辑器摄像机工具（EditorCamera，编辑器专用）。
/// <para>用途：通过反射读取/设置 SceneView（场景视图）摄像机的属性（位置/旋转/大小/正交模式），</para>
/// <para>供角色编辑器等预览场景调整视角使用。</para>
/// </summary>
public static class EditorCamera { 
	/// <summary>设置场景摄像机位置。</summary>
	/// <param name="newPosition">新位置。</param>
	/// <param name="sceneView">目标场景视图（默认激活视图）。</param>
	public static void SetPosition( Vector3 newPosition , SceneView sceneView = null ) { 
		SetEditorCameraValue<Vector3>( "m_Position", newPosition, sceneView );
	}  
	/// <summary>设置场景摄像机旋转。</summary>
	/// <param name="newRotation">新旋转。</param>
	/// <param name="sceneView">目标场景视图。</param>
	public static void SetRotation( Quaternion newRotation , SceneView sceneView = null ) { 
		SetEditorCameraValue<Quaternion>( "m_Rotation", newRotation, sceneView );
	}
	/// <summary>设置场景摄像机大小（缩放）。</summary>
	/// <param name="newSize">新大小。</param>
	/// <param name="sceneView">目标场景视图。</param>
	public static void SetSize( float newSize , SceneView sceneView = null ) { 
		SetEditorCameraValue<float>( "m_Size", newSize, sceneView );
	}
	/// <summary>设置场景摄像机是否正交模式。</summary>
	/// <param name="newOrthographic">是否正交。</param>
	/// <param name="sceneView">目标场景视图。</param>
	public static void SetOrthographic( bool newOrthographic , SceneView sceneView = null ) { 
		if (sceneView == null) sceneView = GetActiveSceneView();
		sceneView.orthographic = newOrthographic;
		//SetEditorCameraValue<float>( "m_Orthographic" , ( newOrthographic == true ) ? 1f : 0f , sceneView ); 
	}  
	/// <summary>获取场景摄像机大小。</summary>
	/// <param name="sceneView">目标场景视图。</param>
	/// <returns>大小值。</returns>
	public static float GetSize( SceneView sceneView = null ) { 
		return GetEditorCameraValue<float>( "m_Size", sceneView );
	}  
	/// <summary>获取场景摄像机位置。</summary>
	/// <param name="sceneView">目标场景视图。</param>
	/// <returns>位置。</returns>
	public static Vector3 GetPosition( SceneView sceneView = null ) { 
		return GetEditorCameraValue<Vector3>( "m_Position", sceneView );
	}  
	/// <summary>获取场景摄像机旋转。</summary>
	/// <param name="sceneView">目标场景视图。</param>
	/// <returns>旋转。</returns>
	public static Quaternion GetRotation( SceneView sceneView = null ) { 
		return GetEditorCameraValue<Quaternion>( "m_Rotation", sceneView );
	}  
	/// <summary>获取场景摄像机是否正交模式。</summary>
	/// <param name="sceneView">目标场景视图。</param>
	/// <returns>是否正交。</returns>
	public static bool GetOrthographic( SceneView sceneView = null ) { 
		if (sceneView == null) sceneView = GetActiveSceneView();
		return sceneView.orthographic;
		//return GetEditorCameraValue<float>( "m_Orthographic", sceneView ) == 1f; 
	}  
	/// <summary>获取当前激活的场景视图（无聚焦时取第一个场景视图）。</summary>
	/// <returns>场景视图。</returns>
	public static SceneView GetActiveSceneView() { 
		if( EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == typeof( SceneView ) ) { 
			return (SceneView)EditorWindow.focusedWindow; }  ArrayList temp = SceneView.sceneViews;  
		return (SceneView)temp[ 0 ]; 
	}  
	/// <summary>通过反射读取场景摄像机私有字段（AnimBool 包装的 m_Value）。</summary>
	/// <typeparam name="T">字段类型。</typeparam>
	/// <param name="fieldName">私有字段名。</param>
	/// <param name="sceneView">目标场景视图。</param>
	/// <returns>字段值。</returns>
	static T GetEditorCameraValue<T>( string fieldName , SceneView sceneView = null ) { 
		FieldInfo field = typeof( SceneView ).GetField( fieldName , BindingFlags.Instance | BindingFlags.NonPublic );  
		object animBool = field.GetValue( ( sceneView != null ) ? sceneView : GetActiveSceneView() );  
		FieldInfo field2 = animBool.GetType().GetField( "m_Value" , BindingFlags.Instance | BindingFlags.NonPublic );  
		return (T)field2.GetValue( animBool );
	}  
	/// <summary>通过反射写入场景摄像机私有字段（调用 BeginAnimating 平滑过渡）。</summary>
	/// <typeparam name="T">字段类型。</typeparam>
	/// <param name="fieldName">私有字段名。</param>
	/// <param name="newValue">新值。</param>
	/// <param name="sceneView">目标场景视图。</param>
	static void SetEditorCameraValue<T>( string fieldName , T newValue , SceneView sceneView = null ) { 
		FieldInfo field = typeof( SceneView ).GetField( fieldName , BindingFlags.Instance | BindingFlags.NonPublic );  
		object animBool = field.GetValue( ( sceneView != null ) ? sceneView : GetActiveSceneView() );  
		FieldInfo field2 = animBool.GetType().GetField( "m_Value" , BindingFlags.Instance | BindingFlags.NonPublic );  
		T currentValue = (T)field2.GetValue( animBool );  
		object[] param = new object[ 2 ]; param[ 0 ] = newValue; param[ 1 ] = currentValue;  
		animBool.GetType() .GetMethod( "BeginAnimating" , BindingFlags.Instance | BindingFlags.NonPublic ) .Invoke( animBool, param );
	} 
}
