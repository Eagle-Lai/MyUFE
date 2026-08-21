using UnityEngine;
using System;
using System.Collections.Generic;
using FPLibrary;

/// <summary>
/// 帧同步扩展（FluxExtensions）。
/// <para>用途：在 FrameInput（紧凑网络输入结构）与 UFE 输入字典（Dictionary&lt;InputReferences, InputEvents&gt;）之间转换，</para>
/// <para>并按网络消息位数（8/16/32 位）裁剪按钮位掩码，实现网络对战的输入序列化/反序列化。</para>
/// </summary>
public static class FluxExtensions{
	/// <summary>
	/// 将 FrameInput 转换为 UFE 输入字典与选中选项（网络→本地）。
	/// </summary>
	/// <param name="inputReferences">输入引用列表。</param>
	/// <param name="frameInput">网络帧输入数据。</param>
	/// <returns>转换后的输入字典与可空选中选项。</returns>
	public static Tuple<Dictionary<InputReferences, InputEvents>, sbyte?> GetInputEvents(
		this IList<InputReferences> inputReferences,
		FrameInput frameInput
	){
		Dictionary<InputReferences, InputEvents> dict = new Dictionary<InputReferences, InputEvents>();

		sbyte? selectedOption = 
			(frameInput.selectedOption == FrameInput.NullSelectedOption ? null : new sbyte?(frameInput.selectedOption));

		NetworkButtonPress buttons = frameInput.buttons;
		if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size8Bits){
			buttons &= (NetworkButtonPress)((sbyte)(-1));
		}else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size16Bits){
			buttons &= (NetworkButtonPress)((short)(-1));
		}

		foreach (InputReferences input in inputReferences){
			if (input.inputType == InputType.HorizontalAxis){
				dict[input] = new InputEvents(frameInput.horizontalAxisRaw);
			}else if (input.inputType == InputType.VerticalAxis){
				dict[input] = new InputEvents(frameInput.verticalAxisRaw);
			}else if(input.inputType == InputType.Button){
				NetworkButtonPress networkButtonPress = input.engineRelatedButton.ToNetworkButtonPress();
				dict[input] = new InputEvents((buttons & networkButtonPress) != NetworkButtonPress.None);
			}
		}

		return new Tuple<Dictionary<InputReferences,InputEvents>, sbyte?>(dict, selectedOption);
	}


	/// <summary>
	/// 将 UFE 输入字典转换为 FrameInput（本地→网络）。
	/// <para>可选强制数字输入（方向取符号）；按消息位数裁剪按钮位掩码。</para>
	/// </summary>
	/// <param name="inputs">输入字典。</param>
	/// <param name="selectedOption">可空的选中选项（菜单选择用）。</param>
	/// <returns>转换后的 FrameInput。</returns>
	public static FrameInput ToFrameInput(this Dictionary<InputReferences, InputEvents> inputs, sbyte? selectedOption){
		Fix64 horizontalAxisRaw = 0;
		Fix64 verticalAxisRaw = 0;
		NetworkButtonPress buttons = NetworkButtonPress.None;

		foreach (KeyValuePair<InputReferences, InputEvents> pair in inputs){
			InputReferences inputReference = pair.Key;
			InputEvents inputEvent = pair.Value;

			if (inputReference.inputType == InputType.HorizontalAxis){
				horizontalAxisRaw = inputEvent.axisRaw;
			}else if (inputReference.inputType == InputType.VerticalAxis){
				verticalAxisRaw = inputEvent.axisRaw;
			}else if (inputReference.inputType == InputType.Button && inputEvent.button){
				NetworkButtonPress buttonPress = inputReference.engineRelatedButton.ToNetworkButtonPress();
				if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size8Bits){
					buttonPress &= (NetworkButtonPress)((sbyte)(-1));
				}else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size16Bits){
					buttonPress &= (NetworkButtonPress)((short)(-1));
				}

				buttons |= buttonPress;

				//buttons |= inputReference.engineRelatedButton.ToNetworkButtonPress();
			}
		}

		if (UFE.config.inputOptions.forceDigitalInput){
			return new FrameInput(
				FPMath.Sign(horizontalAxisRaw),
                FPMath.Sign(verticalAxisRaw),
				buttons,
				selectedOption == null ? FrameInput.NullSelectedOption : selectedOption.Value
			);
		}else{
			return new FrameInput(
				horizontalAxisRaw, 
				verticalAxisRaw,
				buttons,
				selectedOption == null ? FrameInput.NullSelectedOption : selectedOption.Value
			);
		}
	}
}
