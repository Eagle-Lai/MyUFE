using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UFE 屏幕基类（UFEScreen）。
/// <para>用途：所有游戏界面（主菜单/选项/战斗 HUD/选人/选场等）的抽象基类，定义界面的生命周期钩子</para>
/// <para>（DoFixedUpdate 输入轮询 / OnShow / OnHide / IsVisible）与公共属性（淡入淡出、焦点物体、输入环绕）。</para>
/// </summary>
public class UFEScreen : MonoBehaviour{

	/// <summary>编辑器用：Canvas 预览开关。</summary>
    public bool canvasPreview = true;
    //public bool enableUFEInput = false;
	/// <summary>界面首次聚焦的可交互 UI 物体。</summary>
	public GameObject firstSelectableGameObject = null;
	/// <summary>界面是否带淡入动画。</summary>
    public bool hasFadeIn = true;
	/// <summary>界面是否带淡出动画。</summary>
    public bool hasFadeOut = true;
	/// <summary>菜单导航输入是否在边界处环绕。</summary>
	public bool wrapInput = true;

	/// <summary>
	/// 固定帧更新：接收双方输入，供界面子类处理（虚方法，默认空实现）。
	/// </summary>
	/// <param name="player1PreviousInputs">玩家1上一帧输入。</param>
	/// <param name="player1CurrentInputs">玩家1当前帧输入。</param>
	/// <param name="player2PreviousInputs">玩家2上一帧输入。</param>
	/// <param name="player2CurrentInputs">玩家2当前帧输入。</param>
	public virtual void DoFixedUpdate(
		IDictionary<InputReferences, InputEvents> player1PreviousInputs,
		IDictionary<InputReferences, InputEvents> player1CurrentInputs,
		IDictionary<InputReferences, InputEvents> player2PreviousInputs,
		IDictionary<InputReferences, InputEvents> player2CurrentInputs
	){}

	/// <summary>
	/// 界面当前是否可见（依据 GameObject 是否激活）。
	/// </summary>
	/// <returns>可见返回 true。</returns>
	public virtual bool IsVisible(){
		return this.gameObject.activeInHierarchy;
	}

	/// <summary>
	/// 界面隐藏钩子（虚方法，默认空实现）。
	/// </summary>
	public virtual void OnHide(){}

	/// <summary>
	/// 界面显示钩子（虚方法，默认空实现）。
	/// </summary>
	public virtual void OnShow(){
        //UFE.PauseGame(!enableUFEInput);
    }

	/// <summary>
	/// 选择菜单选项（虚方法，默认空实现）。
	/// </summary>
	/// <param name="option">选项索引。</param>
	/// <param name="player">操作玩家。</param>
	public virtual void SelectOption(int option, int player){}
}
