using UnityEngine;
using System;

/// <summary>
/// 简单 AI 行为（SimpleAIBehaviour）。
/// <para>用途：定义 SimpleAI 的行为剧本——由若干 SimpleAIStep（按键+帧数）组成的有序步骤序列，</para>
/// <para>以及"首次被命中后自动格挡"的选项。作为 ScriptableObject 资产在编辑器中配置。</para>
/// </summary>
[Serializable]
public class SimpleAIBehaviour : ScriptableObject{
	/// <summary>行为步骤序列（按顺序执行）。</summary>
	public SimpleAIStep[] steps = new SimpleAIStep[0];
	/// <summary>被首次命中后是否自动格挡。</summary>
	public bool blockAfterFirstHit;

	/// <summary>编辑器用：Inspector 显示开关。</summary>
	[HideInInspector]
	public bool showInInspector;

	/// <summary>编辑器用：步骤列表显示开关。</summary>
	[HideInInspector]
	public bool showStepsInInspector;

}

/// <summary>
/// 简单 AI 步骤：一个包含多个按键与持续帧数的输入片段。
/// </summary>
[Serializable]
public class SimpleAIStep{
	/// <summary>该步骤按下的按钮列表。</summary>
	public ButtonPress[] buttons = new ButtonPress[0];
	/// <summary>该步骤持续执行的帧数。</summary>
	public int frames;

	/// <summary>编辑器用：Inspector 显示开关。</summary>
	[HideInInspector]
	public bool showInInspector;
}
