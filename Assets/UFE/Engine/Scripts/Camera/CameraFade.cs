using UnityEngine;
using System;
using UFENetcode;

/// <summary>
/// 摄像机淡入淡出（CameraFade）。
/// <para>用途：实现全屏遮罩的淡入/淡出效果（屏幕切换、回合开始等过渡动画）。</para>
/// <para>单例模式：静态方法 StartAlphaFade 启动淡出，DoFixedUpdate 每帧推进颜色变化，</para>
/// <para>淡出完成后调用 OnFadeFinish 回调并在变为透明时销毁自身。</para>
/// </summary>
public class CameraFade : MonoBehaviour
{   
	/// <summary>单例实例（内部引用）。</summary>
	private static CameraFade mInstance = null;
	
	/// <summary>
	/// 单例实例：不存在时自动查找或创建。
	/// </summary>
	public static CameraFade instance
	{
		get
		{
			if( mInstance == null )
			{
				mInstance = GameObject.FindObjectOfType(typeof(CameraFade)) as CameraFade;
				
				if( mInstance == null )
				{
					mInstance = new GameObject("CameraFade").AddComponent<CameraFade>();
				}
			}
			
			return mInstance;
		}
	}
	
	/// <summary>
	/// 唤醒：注册单例并初始化。
	/// </summary>
	void Awake()
	{
		if( mInstance == null )
		{
			mInstance = this as CameraFade;
			instance.init();
		}
	}
	
	/// <summary>背景平铺样式（GUIStyle）。</summary>
	public GUIStyle m_BackgroundStyle = new GUIStyle();						// Style for background tiling
	/// <summary>1x1 像素淡出纹理。</summary>
	public Texture2D m_FadeTexture;											// 1x1 pixel texture used for fading
	/// <summary>当前屏幕遮罩颜色（默认黑色全透明）。</summary>
	public Color m_CurrentScreenOverlayColor = new Color(0,0,0,0);			// default starting color: black and fully transparrent
	/// <summary>目标屏幕遮罩颜色（默认黑色全透明）。</summary>
	public Color m_TargetScreenOverlayColor = new Color(0,0,0,0);			// default target color: black and fully transparrent
	/// <summary>颜色变化速率（每秒变化量 = 目标-当前 除以 淡出时长）。</summary>
	public Color m_DeltaColor = new Color(0,0,0,0);							// the delta-color is basically the "speed / second" at which the current color should change
	/// <summary>GUI 绘制层级（确保绘制在一切之上）。</summary>
	public int m_FadeGUIDepth = -1000;										// make sure this texture is drawn on top of everything

	/// <summary>淡出完成后的回调。</summary>
	public Action m_OnFadeFinish = null;



	// Initialize the texture, background-style and initial color:
	/// <summary>
	/// 初始化：创建淡出纹理并绑定到背景样式。
	/// </summary>
	public void init()
	{		
		instance.m_FadeTexture = new Texture2D(1, 1);        
		instance.m_BackgroundStyle.normal.background = instance.m_FadeTexture;
	}

	/// <summary>
	/// 触发淡出完成：锁定目标颜色、清除变化速率、执行完成回调，透明时销毁实例。
	/// </summary>
	protected virtual void FireFadeFinished(){
		instance.m_CurrentScreenOverlayColor = instance.m_TargetScreenOverlayColor;
		SetScreenOverlayColor(instance.m_CurrentScreenOverlayColor);
		instance.m_DeltaColor = new Color( 0,0,0,0 );

		if( instance.m_OnFadeFinish != null ) {
			Action onFadeFinish = instance.m_OnFadeFinish;
			instance.m_OnFadeFinish = null;
			onFadeFinish();
		}
		if (instance.m_CurrentScreenOverlayColor == Color.clear) Die();
	}

	// Draw the texture and perform the fade:
	/// <summary>
	/// 绘制全屏遮罩纹理（仅在遮罩 alpha 大于 0 时绘制）。
	/// </summary>
	public void OnGUI() {
		// Only draw the texture when the alpha value is greater than 0:
		if (m_CurrentScreenOverlayColor.a > 0) {			
			GUI.depth = instance.m_FadeGUIDepth;
			GUI.Label(new Rect(-10, -10, Screen.width + 10, Screen.height + 10), instance.m_FadeTexture, instance.m_BackgroundStyle);
		}
	}

	/// <summary>
	/// 固定帧更新：按颜色变化速率推进遮罩颜色，接近目标时锁定并触发完成回调。
	/// </summary>
    public void DoFixedUpdate() {
        // If the current color of the screen is not equal to the desired color: keep fading!
        if (instance.m_CurrentScreenOverlayColor != instance.m_TargetScreenOverlayColor) {
            // If the difference between the current alpha and the desired alpha is smaller than delta-alpha * deltaTime, 
            // then we're pretty much done fading:
            if (
                Mathf.Abs(instance.m_CurrentScreenOverlayColor.a - instance.m_TargetScreenOverlayColor.a) <
                Mathf.Abs(instance.m_DeltaColor.a) * UFE.fixedDeltaTime
            ) {
                SetScreenOverlayColor(instance.m_TargetScreenOverlayColor);
                this.FireFadeFinished();
            } else {
                // Fade!
                SetScreenOverlayColor(instance.m_CurrentScreenOverlayColor + instance.m_DeltaColor * (float)UFE.fixedDeltaTime);

                if (instance.m_CurrentScreenOverlayColor == instance.m_TargetScreenOverlayColor) {
                    this.FireFadeFinished();
                }
            }
        }
	}
	//-----------------------------------------------------------------------------------------------------------------
	
	
	/// <summary>
	/// Sets the color of the screen overlay instantly.  Useful to start a fade.
	/// </summary>
	/// <param name='newScreenOverlayColor'>
	/// New screen overlay color.
	/// </param>
	/// <summary>
	/// 立即设置屏幕遮罩颜色（用于开始淡出）。
	/// </summary>
	/// <param name='newScreenOverlayColor'>新的遮罩颜色。</param>
	private static void SetScreenOverlayColor(Color newScreenOverlayColor)
	{
		instance.m_CurrentScreenOverlayColor = newScreenOverlayColor;
		instance.m_FadeTexture.SetPixel(0, 0, instance.m_CurrentScreenOverlayColor);
		instance.m_FadeTexture.Apply();
	}

	/// <summary>
	/// Starts the fade from color newScreenOverlayColor. If isFadeIn, start fully opaque, else start transparent, after a delay, with Action OnFadeFinish.
	/// </summary>
	/// <param name='newScreenOverlayColor'>
	/// New screen overlay color.
	/// </param>
	/// <param name='fadeDuration'>
	/// Fade duration.
	/// </param>
	/// <param name='fadeDelay'>
	/// Fade delay.
	/// </param>
	/// <param name='OnFadeFinish'>
	/// On fade finish, doWork().
	/// </param>
	/// <summary>
	/// 启动淡入/淡出：isFadeIn=true 从不透明淡到透明（淡入），false 从透明淡到指定颜色（淡出）。
	/// <para>时长 <=0 时立即设置并执行回调；否则按目标-当前 计算变化速率。</para>
	/// </summary>
	/// <param name='newScreenOverlayColor'>目标遮罩颜色。</param>
	/// <param name='isFadeIn'>是否淡入。</param>
	/// <param name='fadeDuration'>淡出时长（秒）。</param>
	/// <param name='fadeDelay'>延迟（秒，当前未使用）。</param>
	/// <param name='OnFadeFinish'>淡出完成后的回调。</param>
	public static void StartAlphaFade(
		Color newScreenOverlayColor, 
		bool isFadeIn, 
		float fadeDuration, 
		float fadeDelay = 0f, 
		Action OnFadeFinish = null
	) {
		if (fadeDuration <= 0.0f)		
		{
			if( isFadeIn ){
				SetScreenOverlayColor(Color.clear);
				instance.Die();
			}else{
				SetScreenOverlayColor(newScreenOverlayColor);
			}

			if (OnFadeFinish != null){
				OnFadeFinish();
			}
		}
		else					
		{
            instance.m_OnFadeFinish = OnFadeFinish;
			if( isFadeIn )
			{
				instance.m_TargetScreenOverlayColor = new Color( newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0 );
				SetScreenOverlayColor( newScreenOverlayColor );
			} else {
				instance.m_TargetScreenOverlayColor = newScreenOverlayColor;
				SetScreenOverlayColor( new Color( newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0 ) );
			}
			instance.m_DeltaColor = (instance.m_TargetScreenOverlayColor - instance.m_CurrentScreenOverlayColor) / fadeDuration;
		}
	}
	
	/// <summary>
	/// 销毁单例实例与自身。
	/// </summary>
	void Die()
	{
		mInstance = null;
		Destroy(gameObject);
	}
	
	/// <summary>
	/// 应用退出时清空单例引用。
	/// </summary>
	void OnApplicationQuit()
	{
		mInstance = null;
	}
}
