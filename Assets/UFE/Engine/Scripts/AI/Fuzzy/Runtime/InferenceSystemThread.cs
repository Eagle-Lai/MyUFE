using System;
using System.Collections.Generic;
using System.Threading;
using AI4Unity.Fuzzy;

/// <summary>
/// 模糊推理系统线程包装（InferenceSystemThread）。
/// <para>用途：将 Fuzzy AI 的推理系统（InferenceSystem）的求值过程放到后台线程执行，避免阻塞主线程。</para>
/// <para>支持异步（AsyncCalculateOutputs）与同步（SyncCalculateOutputs）两种求值方式，</para>
/// <para>通过 done 标志与 Output 字典向主线程提供求值结果。</para>
/// </summary>
public class InferenceSystemThread{
	#region public instance properties
	/// <summary>
	/// 默认输出值（未设置输入时的回退值）。
	/// </summary>
	public float DefaultValue{
		get{return this.defaultValue;}
	}

	/// <summary>
	/// 求值是否已完成。
	/// </summary>
	public bool Done{
		get{return this.done;}
	}

	/// <summary>
	/// 求值输出字典（变量名→值）。
	/// </summary>
	public Dictionary<string, float> Output{
		get{return this.output;}
	}
	#endregion

	#region private instance fields
	/// <summary>默认输出值。</summary>
	private float defaultValue;
	/// <summary>求值完成标志（volatile 保证跨线程可见）。</summary>
	private volatile bool done;
	/// <summary>模糊推理引擎引用。</summary>
	private InferenceSystem inferenceEngine;
	/// <summary>求值输出字典。</summary>
	private Dictionary<string, float> output;
	/// <summary>本次请求的输出变量集合。</summary>
	private HashSet<string> requestedOutputs;
	#endregion

	#region public instance constructors
	/// <summary>
	/// 构造函数：绑定推理引擎与默认值。
	/// </summary>
	/// <param name="inferenceEngine">推理引擎。</param>
	/// <param name="defaultValue">默认输出值。</param>
	public InferenceSystemThread(InferenceSystem inferenceEngine, float defaultValue){
		this.requestedOutputs = new HashSet<string>();
		this.inferenceEngine = inferenceEngine;
		this.defaultValue = defaultValue;

		this.output = null;
		this.done = true;
	}
	#endregion

	#region public instance methods
	/// <summary>
	/// 异步求值：在新线程中计算指定输出变量并立即返回线程句柄。
	/// </summary>
	/// <param name="requestedOutputs">需要的输出变量集合。</param>
	/// <returns>执行求值的工作线程。</returns>
	public Thread AsyncCalculateOutputs(HashSet<string> requestedOutputs){
		this.done = false;
		this.output = null;
		this.requestedOutputs = requestedOutputs;
		
		Thread t = new Thread(this.Run);
		t.Start();
		return t;
	}

	/// <summary>
	/// 获取推理引擎的输入语言变量。
	/// </summary>
	/// <param name="variableName">变量名。</param>
	/// <returns>语言变量对象。</returns>
	public AForge.Fuzzy.LinguisticVariable GetInputVariable(string variableName){
		return this.inferenceEngine.GetInputVariable(variableName);
	}

	/// <summary>
	/// 设置单个输入变量值。
	/// </summary>
	/// <param name="variableName">变量名。</param>
	/// <param name="input">输入值。</param>
	public void SetInput(string variableName, float input){
		this.inferenceEngine.SetInput(variableName, input);
	}

	/// <summary>
	/// 批量设置输入变量值。
	/// </summary>
	/// <param name="inputs">输入字典（变量名→值）。</param>
	public void SetInputs(Dictionary<string, float> inputs){
		this.inferenceEngine.SetInputs(inputs);
	}

	/// <summary>
	/// 同步求值：直接在当前线程计算指定输出变量。
	/// </summary>
	/// <param name="requestedOutputs">需要的输出变量集合。</param>
	public void SyncCalculateOutputs(HashSet<string> requestedOutputs){
		this.done = false;
		this.output = null;
		this.requestedOutputs = requestedOutputs;
		this.Run();
	}
	#endregion

	#region protected instance methods
	/// <summary>
	/// 执行求值：调用推理引擎评估输出变量并标记完成（工作线程入口）。
	/// </summary>
	protected void Run(){
		this.output = this.inferenceEngine.Evaluate(this.requestedOutputs, this.defaultValue);
		this.done = true;
	}
	#endregion
}
