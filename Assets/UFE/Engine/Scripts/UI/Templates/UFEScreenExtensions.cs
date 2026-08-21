using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FPLibrary;

/// <summary>
/// UI 屏幕扩展（UFEScreenExtensions）。
/// <para>用途：为 UFEScreen 提供通用的菜单导航系统扩展方法——方向键移动光标（DefaultMoveCursorAction，支持 Slider 调节）、</para>
/// <para>确认/取消动作（Button 点击/Toggle 切换）、查找并高亮下一个可交互物体、默认导航系统（DefaultNavigationSystem）与</para>
/// <para>特殊导航系统（SpecialNavigationSystem，用于选人/选场等复杂界面）。</para>
/// </summary>
public static class UFEScreenExtensions{
	#region public class definitions
	/// <summary>
	/// 动作回调（ActionCallback）：绑定一个可执行动作与对应的触发音效（用于导航系统的确认/取消动作）。
	/// </summary>
	public class ActionCallback{
		/// <summary>动作委托（接收一个音效参数）。</summary>
		public delegate void ActionDelegate(AudioClip sound);
		/// <summary>要执行的动作。</summary>
		public ActionDelegate Action{get; set;}
		/// <summary>动作触发时播放的音效。</summary>
		public AudioClip Sound{get; set;}

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="action">要执行的动作。</param>
		/// <param name="sound">触发音效。</param>
		public ActionCallback(ActionDelegate action = null, AudioClip sound = null){
			this.Action = action;
			this.Sound = sound;
		}
	}

	/// <summary>
	/// 移动光标回调（MoveCursorCallback）：绑定一个光标移动处理器与对应音效（用于特殊导航系统的光标移动）。
	/// </summary>
	public class MoveCursorCallback{
		/// <summary>光标移动委托（接收轴/按键状态与音效）。</summary>
		public delegate void ActionDelegate(
			Fix64 horizontalAxis,
			Fix64 verticalAxis,
			bool horizontalAxisDown,
			bool verticalAxisDown,
			bool confirmButtonDown,
			bool cancelButtonDown,
			AudioClip sound
		);

		/// <summary>要执行的光标移动动作。</summary>
		public ActionDelegate Action{get; set;}
		/// <summary>光标移动时播放的音效。</summary>
		public AudioClip Sound{get; set;}
		
		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="action">光标移动处理器。</param>
		/// <param name="sound">移动音效。</param>
		public MoveCursorCallback(ActionDelegate action = null, AudioClip sound = null){
			this.Action = action;
			this.Sound = sound;
		}
	}
	#endregion

	#region public class properties
	/// <summary>滑杆归一化调节速度（方向键改变 Slider 值的步长）。</summary>
	public static float NormalizedSliderSpeed = 0.1f;
	#endregion

	#region public class methods
	/// <summary>
	/// 默认导航系统（双人版）：为玩家1与玩家2分别处理输入（任一玩家有操作即返回 true）。
	/// </summary>
	/// <param name="screen">当前屏幕。</param>
	/// <param name="player1PreviousInputs">玩家1上一帧输入。</param>
	/// <param name="player1CurrentInputs">玩家1当前帧输入。</param>
	/// <param name="player2PreviousInputs">玩家2上一帧输入。</param>
	/// <param name="player2CurrentInputs">玩家2当前帧输入。</param>
	/// <param name="moveCursorSound">移动光标音效。</param>
	/// <param name="confirmSound">确认音效。</param>
	/// <param name="cancelSound">取消音效。</param>
	/// <param name="cancelAction">取消动作。</param>
	/// <returns>任一玩家有操作返回 true。</returns>
	public static bool DefaultNavigationSystem(
		this UFEScreen screen, 
		IDictionary<InputReferences, InputEvents> player1PreviousInputs,
		IDictionary<InputReferences, InputEvents> player1CurrentInputs,
		IDictionary<InputReferences, InputEvents> player2PreviousInputs,
		IDictionary<InputReferences, InputEvents> player2CurrentInputs,
		AudioClip moveCursorSound = null,
		AudioClip confirmSound = null,
		AudioClip cancelSound = null,
		Action cancelAction = null
	){
		return
			screen.DefaultNavigationSystem(
				player1PreviousInputs,
				player1CurrentInputs, 
				moveCursorSound, 
				confirmSound, 
				cancelSound, 
				cancelAction
			) 
			||
			screen.DefaultNavigationSystem(
				player2PreviousInputs,
				player2CurrentInputs, 
				moveCursorSound, 
				confirmSound, 
				cancelSound, 
				cancelAction
			);
	}

	/// <summary>
	/// 默认导航系统（单人版）：处理输入框（InputField）特例或调用特殊导航系统处理普通菜单导航。
	/// </summary>
	/// <param name="screen">当前屏幕。</param>
	/// <param name="previousInputs">上一帧输入。</param>
	/// <param name="currentInputs">当前帧输入。</param>
	/// <param name="moveCursorSound">移动光标音效。</param>
	/// <param name="confirmSound">确认音效。</param>
	/// <param name="cancelSound">取消音效。</param>
	/// <param name="cancelAction">取消动作。</param>
	/// <returns>有操作返回 true。</returns>
	public static bool DefaultNavigationSystem(
		this UFEScreen screen, 
		IDictionary<InputReferences, InputEvents> previousInputs,
		IDictionary<InputReferences, InputEvents> currentInputs,
		AudioClip moveCursorSound = null,
		AudioClip confirmSound = null,
		AudioClip cancelSound = null,
		Action cancelAction = null
	){
		if (UFE.eventSystem != null && UFE.eventSystem.isActiveAndEnabled){
			//---------------------------------------------------------------------------------------------------------
			// First, check if the current Selectable Object is an Input Field, because it's a special case...
			//---------------------------------------------------------------------------------------------------------
			GameObject currentGameObject = UFE.eventSystem.currentSelectedGameObject;
			InputField inputField = currentGameObject != null ? currentGameObject.GetComponent<InputField>() : null;

			if (inputField != null){
				//-----------------------------------------------------------------------------------------------------
				// If it's an Input Field, check if the user wants to write a text
				// or if he wants to move the caret or exit from the Input Field...
				//-----------------------------------------------------------------------------------------------------
				Vector3 direction = 
					(Input.GetKeyDown(KeyCode.UpArrow) ? Vector3.up : Vector3.zero) + 
					(Input.GetKeyDown(KeyCode.DownArrow) ? Vector3.down : Vector3.zero);

				if(
					direction != Vector3.zero ||
					Input.GetKeyDown(KeyCode.Tab) || 
					Input.GetKeyDown(KeyCode.Return) || 
					Input.GetKeyDown(KeyCode.KeypadEnter)
				){
					Selectable previousSelectable = inputField;
					Selectable nextSelectable = null;

					if (direction != Vector3.zero){
						nextSelectable = currentGameObject.FindSelectable(direction, false);
					}

					if (nextSelectable == null || previousSelectable == nextSelectable){
						nextSelectable = currentGameObject.FindSelectable(Vector3.right, false);

						if (nextSelectable == null || previousSelectable == nextSelectable){
							nextSelectable = currentGameObject.FindSelectable(Vector3.down, false);

							if (nextSelectable == null || previousSelectable == nextSelectable){
								nextSelectable = currentGameObject.FindSelectable(Vector3.left, false);

								if (nextSelectable == null || previousSelectable == nextSelectable){
									nextSelectable = currentGameObject.FindSelectable(Vector3.up, false);
								}
							}
						}
					}

					screen.HighlightOption(nextSelectable);
				}else{
					inputField.OnUpdateSelected(new AxisEventData(UFE.eventSystem));
				}
				return true;
			}else{
				//-----------------------------------------------------------------------------------------------------
				// Otherwise, invoke the "Special Navigation System" with the default functions
				//-----------------------------------------------------------------------------------------------------
				return screen.SpecialNavigationSystem(
					previousInputs,
					currentInputs,
					new MoveCursorCallback(screen.DefaultMoveCursorAction, moveCursorSound),
					new ActionCallback(UFE.eventSystem.currentSelectedGameObject.DefaultConfirmAction, confirmSound),
					new ActionCallback(cancelAction.DefaultCancelAction, cancelSound)
				);
			}
		}
		return false;
	}

	/// <summary>
	/// 查找屏幕中第一个可交互且属于该屏幕的 Selectable（优先返回 firstSelectableGameObject；否则按左上到右下的规则选择）。
	/// </summary>
	/// <param name="screen">当前屏幕。</param>
	/// <returns>找到的 Selectable；无则返回 null。</returns>
	public static Selectable FindFirstSelectable(this UFEScreen screen){
		List<Selectable> selectables = Selectable.allSelectables;
		Transform firstSelectableTransform = null;
		Selectable firstSelectable = null;
		
		for (int i = 0; i < selectables.Count; ++i){
			Selectable currentSelectable = selectables[i];
			
			if(
				currentSelectable != null && 
				currentSelectable.gameObject.activeInHierarchy && 
				currentSelectable.IsInteractable() &&
				screen.HasSelectable(currentSelectable)
			){
				Transform currentTransform = currentSelectable.transform;
				
				if (screen.firstSelectableGameObject != null){
					if (currentSelectable.gameObject == screen.firstSelectableGameObject){
						return currentSelectable;
					}
				}else if(
					firstSelectable == null ||
					firstSelectableTransform == null ||
					currentTransform.position.y > firstSelectableTransform.position.y ||
					(
					currentTransform.position.y == firstSelectableTransform.position.y &&
					currentTransform.position.x < firstSelectableTransform.position.x
					)
					){
					firstSelectable = currentSelectable;
					firstSelectableTransform = currentTransform;
				}
			}
		}
		
		return firstSelectable;
	}
	
	/// <summary>
	/// 查找屏幕中第一个可交互 Selectable 的 GameObject。
	/// </summary>
	/// <param name="screen">当前屏幕。</param>
	/// <returns>第一个可交互物体的 GameObject。</returns>
	public static GameObject FindFirstSelectableGameObject(this UFEScreen screen){
		Selectable selectable = screen.FindFirstSelectable();
		return selectable != null ? selectable.gameObject : null;
	}

	/// <summary>
	/// 按方向查找当前选中物体在屏幕中的下一个 Selectable。
	/// </summary>
	/// <param name="screen">当前屏幕。</param>
	/// <param name="direction">查找方向。</param>
	/// <returns>找到的 Selectable；无当前选中物则返回第一个可交互物体。</returns>
	public static Selectable FindSelectable(this UFEScreen screen, Vector3 direction){
		GameObject currentGameObject = UFE.eventSystem.currentSelectedGameObject;
		if (currentGameObject == null){
			return screen.FindFirstSelectable();
		}else{
			return currentGameObject.FindSelectable(direction, screen.wrapInput) ?? screen.FindFirstSelectable();
		}
	}
	
	/// <summary>
	/// 按方向从指定 GameObject 查找下一个 Selectable（支持环绕、白名单过滤与 Automatic/Explicit/Horizontal/Vertical/None 五种导航模式）。
	/// </summary>
	/// <param name="currentGameObject">当前物体。</param>
	/// <param name="direction">查找方向。</param>
	/// <param name="wrapInput">是否环绕（在反方向找不到时循环到另一侧）。</param>
	/// <param name="whiteList">可选的白名单（仅在这些可交互物体中查找）。</param>
	/// <returns>找到的 Selectable；未找到返回 null。</returns>
	public static Selectable FindSelectable(
		this GameObject currentGameObject, 
		Vector3 direction, 
		bool wrapInput,
		IList<Selectable> whiteList = null
	){
		if (currentGameObject == null || !currentGameObject.activeInHierarchy){
			// If no GameObject is selected, search the first Selectable GameObject in the screen
			return null;
		}else{
			// If a GameObject is selected, check if it has a Selectable component and if it's interactable...
			Selectable currentSelectableObject = currentGameObject.GetComponent<Selectable>();
			if (currentSelectableObject == null || !currentSelectableObject.IsInteractable()){
				// If the selected GameObject isn't Selectable and Interactable, 
				// search the first Selectable GameObject in the screen
				return null;
			}else{
				// Otherwise, check which Navigation Mode is defined for the current Selectable Object
				// and try to find the next Selectable Object in the specified direction...
				if (currentSelectableObject.navigation.mode == Navigation.Mode.Automatic){
					//-------------------------------------------------------------------------------------------------
					// "AUTOMATIC" Navigation Mode
					//-------------------------------------------------------------------------------------------------
					Selectable nextSelectableObject = UFEScreenExtensions.FindSelectable(currentSelectableObject, direction, whiteList);
					if (nextSelectableObject != null){
						return nextSelectableObject;
					}else if (wrapInput){
						// If we couldn't find any selectable GameObject but we want to wrap 
						// the input in the current screen, we search the first selectable 
						// GameObject in the opposite part of the screen.
						Vector3 oppositeDirection = -direction;
						nextSelectableObject = currentSelectableObject;
						Selectable temp = UFEScreenExtensions.FindSelectable(nextSelectableObject, oppositeDirection, whiteList);

						while (temp != null){
							nextSelectableObject = temp;
							temp = UFEScreenExtensions.FindSelectable(temp, oppositeDirection, whiteList);
						}
						
						return nextSelectableObject ?? currentSelectableObject;
					}else{
						// If we couldn't find any selectable GameObject and we don't want to wrap the input 
						// in the current screen, then we return the current selectable object (if any).
						return currentSelectableObject;
					}
				}else if (currentSelectableObject.navigation.mode == Navigation.Mode.Explicit){
					//-------------------------------------------------------------------------------------------------
					// "EXPLICIT" Navigation Mode
					//-------------------------------------------------------------------------------------------------
					if (direction.x == 0f){
						if (direction.y > 0f){
							return currentSelectableObject.navigation.selectOnUp;
						}else if (direction.y < 0f){
							return currentSelectableObject.navigation.selectOnDown;
						}else{
							return currentSelectableObject;
						}
					}else if (direction.x < 0f){
						return currentSelectableObject.navigation.selectOnLeft;
					}else if (direction.x > 0f){
						return currentSelectableObject.navigation.selectOnRight;
					}else{
						return currentSelectableObject;
					}
				}else if (currentSelectableObject.navigation.mode == Navigation.Mode.Horizontal){
					//-------------------------------------------------------------------------------------------------
					// "HORIZONTAL" Navigation Mode
					//-------------------------------------------------------------------------------------------------
					Vector3 currentSelectablePosition = currentSelectableObject.transform.position;
					List<Selectable> selectables = Selectable.allSelectables;
					Selectable first = null;
					Selectable last = null;
					Selectable previous = null;
					Selectable next = null;

					for (int i = 0; i < selectables.Count; ++i){
						Selectable current = selectables[i];

						if (current!=null && (whiteList==null || whiteList.Count==0 || whiteList.Contains(current))){
							Transform currentTransform = current.transform;

							if(
								first == null
								|| 
								currentTransform.position.x < first.transform.position.x
								||
								currentTransform.position.x == first.transform.position.x &&
								currentTransform.position.y > first.transform.position.y
							){
								first = current;
							}

							if(
								last == null
								|| 
								currentTransform.position.x > last.transform.position.x
								||
								currentTransform.position.x == last.transform.position.x &&
								currentTransform.position.y < last.transform.position.y
							){
								last = current;
							}

							if(
								(
									previous == null 
									||
									currentTransform.position.x > previous.transform.position.x 
									||
									currentTransform.position.x == previous.transform.position.x &&
									currentTransform.position.y < previous.transform.position.y
								)
								&&
								(
									currentTransform.position.x < currentSelectablePosition.x
									||
									currentTransform.position.x == currentSelectablePosition.x &&
									currentTransform.position.y > currentSelectablePosition.y
								)
							){
								previous = current;
							}

							if(
								(
									next == null 
									||
									currentTransform.position.x < next.transform.position.x 
									||
									currentTransform.position.x == next.transform.position.x &&
									currentTransform.position.y > next.transform.position.y
								)
								&&
								(
									currentTransform.position.x > currentSelectablePosition.x
									||
									currentTransform.position.x == currentSelectablePosition.x &&
									currentTransform.position.y < currentSelectablePosition.y
								)
							){
								next = current;
							}
						}
					}

					if (direction.x < 0f){
						return previous ?? (wrapInput ? last : currentSelectableObject);
					}else if (direction.x > 0f){
						return next ?? (wrapInput ? first : currentSelectableObject);
					}else{
						return currentSelectableObject;
					}
				}else if (currentSelectableObject.navigation.mode == Navigation.Mode.Vertical){
					//-------------------------------------------------------------------------------------------------
					// "VERTICAL" Navigation Mode
					//-------------------------------------------------------------------------------------------------
					Vector3 currentSelectablePosition = currentSelectableObject.transform.position;
					List<Selectable> selectables = Selectable.allSelectables;
					Selectable first = null;
					Selectable last = null;
					Selectable previous = null;
					Selectable next = null;
					
					for (int i = 0; i < selectables.Count; ++i){
						Selectable current = selectables[i];
						
						if (current!=null && (whiteList==null || whiteList.Count==0 || whiteList.Contains(current))){
							Transform currentTransform = current.transform;
							
							if(
								first == null
								|| 
								currentTransform.position.y > first.transform.position.y
								||
								currentTransform.position.y == first.transform.position.y &&
								currentTransform.position.x < first.transform.position.x
							){
								first = current;
							}
							
							if(
								last == null
								||
								currentTransform.position.y < last.transform.position.y
								||
								currentTransform.position.y == last.transform.position.y &&
								currentTransform.position.x > last.transform.position.x
							){
								last = current;
							}
							
							if(
								(
									previous == null 
									||
									currentTransform.position.y < previous.transform.position.y
									||
									currentTransform.position.y == previous.transform.position.y &&
									currentTransform.position.x > previous.transform.position.x
								)
								&&
								(
									currentTransform.position.y > currentSelectablePosition.y
									||
									currentTransform.position.y == currentSelectablePosition.y &&
									currentTransform.position.x < currentSelectablePosition.x
								)
							){
								previous = current;
							}
							
							if(
								(
									next == null 
									|| 
									currentTransform.position.y > next.transform.position.y
									||
									currentTransform.position.y == next.transform.position.y &&
									currentTransform.position.x < next.transform.position.x
								)
								&&
								(
									currentTransform.position.y < currentSelectablePosition.y
									||
									currentTransform.position.y == currentSelectablePosition.y &&
									currentTransform.position.x > currentSelectablePosition.x
								)
							){
								next = current;
							}
						}
					}

					if (direction.y < 0f){
						return next ?? (wrapInput ? first : currentSelectableObject);
					}else if (direction.y > 0f){
						return previous ?? (wrapInput ? last : currentSelectableObject);
					}else{
						return currentSelectableObject;
					}
				}else{
					//-------------------------------------------------------------------------------------------------
					// "NONE" Navigation Mode
					//-------------------------------------------------------------------------------------------------
					return currentSelectableObject;
				}
			}
		}
	}
	
	/// <summary>
	/// 查找屏幕中按方向的下一个 Selectable 的 GameObject。
	/// </summary>
	/// <param name="screen">当前屏幕。</param>
	/// <param name="direction">查找方向。</param>
	/// <returns>找到的 GameObject。</returns>
	public static GameObject FindSelectableGameObject(this UFEScreen screen, Vector3 direction){
		Selectable selectable = screen.FindSelectable(direction);
		return selectable != null ? selectable.gameObject : null;
	}
	
	/// <summary>
	/// 按方向从指定 GameObject 查找下一个 Selectable 的 GameObject。
	/// </summary>
	/// <param name="currentGameObject">当前物体。</param>
	/// <param name="direction">查找方向。</param>
	/// <param name="wrapInput">是否环绕。</param>
	/// <param name="whiteList">可选白名单。</param>
	/// <returns>找到的 GameObject。</returns>
	public static GameObject FindSelectableGameObject(
		this GameObject currentGameObject, 
		Vector3 direction, 
		bool wrapInput,
		IList<Selectable> whiteList = null
	){
		Selectable selectable = currentGameObject.FindSelectable(direction, wrapInput, whiteList);
		return selectable != null ? selectable.gameObject : null;
	}

	/// <summary>
	/// 高亮指定选项（按 Selectable）。
	/// </summary>
	/// <param name="screen">当前屏幕。</param>
	/// <param name="option">要高亮的选项。</param>
	/// <param name="pointer">可选指针事件。</param>
	public static void HighlightOption(this UFEScreen screen, Selectable option, BaseEventData pointer = null){
		screen.HighlightOption(option != null ? option.gameObject : null, pointer);
	}

	/// <summary>
	/// 高亮指定选项（按 GameObject）：设置 EventSystem 选中物体；输入框自动激活。
	/// </summary>
	/// <param name="screen">当前屏幕。</param>
	/// <param name="option">要高亮的 GameObject。</param>
	/// <param name="pointer">可选指针事件。</param>
	public static void HighlightOption(this UFEScreen screen, GameObject option, BaseEventData pointer = null){
		UFE.eventSystem.SetSelectedGameObject(option, pointer);

		InputField nextInputField = option != null ? option.GetComponent<InputField>() : null;
		if (nextInputField != null){
			nextInputField.OnPointerClick(new PointerEventData(UFE.eventSystem));
			nextInputField.selectionAnchorPosition = 0;
			nextInputField.selectionFocusPosition = 0;
			nextInputField.ActivateInputField();
			nextInputField.Select();
		}
	}

	/// <summary>
	/// 按方向移动光标到下一个 Selectable（若变化则播放移动音效并高亮）。
	/// </summary>
	/// <param name="screen">当前屏幕。</param>
	/// <param name="direction">移动方向。</param>
	/// <param name="moveCursorSound">移动音效。</param>
	public static void MoveCursor(this UFEScreen screen, Vector3 direction, AudioClip moveCursorSound = null){
		GameObject currentGameObject = UFE.eventSystem.currentSelectedGameObject;
		GameObject nextGameObject = screen.FindSelectableGameObject(direction);
		
		if (nextGameObject == null){
			nextGameObject = currentGameObject;
		}
		
		if (currentGameObject != nextGameObject){
			if (moveCursorSound != null){
				UFE.PlaySound(moveCursorSound);
			}

			screen.HighlightOption(nextGameObject);
		}
	}

	/// <summary>
	/// 特殊导航系统（双人版）：为玩家1与玩家2分别处理输入（任一玩家有操作返回 true）。
	/// </summary>
	/// <param name="screen">当前屏幕。</param>
	/// <param name="player1PreviousInputs">玩家1上一帧输入。</param>
	/// <param name="player1CurrentInputs">玩家1当前帧输入。</param>
	/// <param name="player2PreviousInputs">玩家2上一帧输入。</param>
	/// <param name="player2CurrentInputs">玩家2当前帧输入。</param>
	/// <param name="moveCursorCallback">光标移动回调。</param>
	/// <param name="confirmCallback">确认回调。</param>
	/// <param name="cancelCallback">取消回调。</param>
	/// <returns>任一玩家有操作返回 true。</returns>
	public static bool SpecialNavigationSystem(
		this UFEScreen screen, 
		IDictionary<InputReferences, InputEvents> player1PreviousInputs,
		IDictionary<InputReferences, InputEvents> player1CurrentInputs,
		IDictionary<InputReferences, InputEvents> player2PreviousInputs,
		IDictionary<InputReferences, InputEvents> player2CurrentInputs,
		MoveCursorCallback moveCursorCallback = null,
		ActionCallback confirmCallback = null,
		ActionCallback cancelCallback = null
	){
		return
			screen.SpecialNavigationSystem(
				player1PreviousInputs,
				player1CurrentInputs, 
				moveCursorCallback, 
				confirmCallback, 
				cancelCallback
			) 
			||
			screen.SpecialNavigationSystem(
				player2PreviousInputs,
				player2PreviousInputs, 
				moveCursorCallback, 
				confirmCallback, 
				cancelCallback
			);
	}

	/// <summary>
	/// 特殊导航系统（单人版）：读取水平/垂直轴与确认/取消按钮的边缘触发，依次调用光标移动、确认、取消回调。
	/// </summary>
	/// <param name="screen">当前屏幕。</param>
	/// <param name="previousInputs">上一帧输入。</param>
	/// <param name="currentInputs">当前帧输入。</param>
	/// <param name="moveCursorCallback">光标移动回调。</param>
	/// <param name="confirmCallback">确认回调。</param>
	/// <param name="cancelCallback">取消回调。</param>
	/// <returns>有操作返回 true。</returns>
	public static bool SpecialNavigationSystem(
		this UFEScreen screen, 
		IDictionary<InputReferences, InputEvents> previousInputs,
		IDictionary<InputReferences, InputEvents> currentInputs,
		MoveCursorCallback moveCursorCallback,
		ActionCallback confirmCallback,
		ActionCallback cancelCallback
	){
		Fix64 currentHorizontalAxis = 0f;
		Fix64 currentVerticalAxis = 0f;

		bool currentHorizontalButton = false;
		bool currentVerticalButton = false;
		bool currentConfirmButton = false;
		bool currentCancelButton = false;

		foreach (KeyValuePair<InputReferences, InputEvents> pair in currentInputs){
			if (pair.Key.inputType == InputType.HorizontalAxis){
				currentHorizontalAxis = pair.Value.axisRaw;
				currentHorizontalButton = pair.Value.button;
			}else if (pair.Key.inputType == InputType.VerticalAxis){
				currentVerticalAxis = pair.Value.axisRaw;
				currentVerticalButton = pair.Value.button;
			}else{
				if (pair.Key.engineRelatedButton == UFE.config.inputOptions.confirmButton){
					currentConfirmButton = pair.Value.button;
				}
				if (pair.Key.engineRelatedButton == UFE.config.inputOptions.cancelButton){
					currentCancelButton = pair.Value.button;
				}
			}
		}


		bool previousHorizontalButton = false;
		bool previousVerticalButton = false;
		bool previousConfirmButton = false;
		bool previousCancelButton = false;

		foreach (KeyValuePair<InputReferences, InputEvents> pair in previousInputs){
			if (pair.Key.inputType == InputType.HorizontalAxis){
				previousHorizontalButton = pair.Value.button;
			}else if (pair.Key.inputType == InputType.VerticalAxis){
				previousVerticalButton = pair.Value.button;
			}else{
				if (pair.Key.engineRelatedButton == UFE.config.inputOptions.confirmButton){
					previousConfirmButton = pair.Value.button;
				}
				if (pair.Key.engineRelatedButton == UFE.config.inputOptions.cancelButton){
					previousCancelButton = pair.Value.button;
				}
			}
		}

		bool horizontalAxisDown = currentHorizontalButton && !previousHorizontalButton;
		bool verticalAxisDown = currentVerticalButton && !previousVerticalButton;
		bool confirmButtonDown = currentConfirmButton && !previousConfirmButton;
		bool cancelButtonDown = currentCancelButton && !previousCancelButton;

//		UnityEngine.Debug.Log(
//			UFE.currentFrame + " | " + 
//			previousHorizontalButton + " > " + currentHorizontalButton + " | " +
//			previousVerticalButton + " > " + currentVerticalButton
//		);

		if (moveCursorCallback != null && moveCursorCallback.Action != null){
			moveCursorCallback.Action(
				currentHorizontalAxis,
				currentVerticalAxis,
				horizontalAxisDown,
				verticalAxisDown,
				confirmButtonDown,
				cancelButtonDown,
				moveCursorCallback.Sound
			);
		}

		if (confirmButtonDown){
			if (confirmCallback != null && confirmCallback.Action != null){
				confirmCallback.Action(confirmCallback.Sound);
			}
			return true;
		}else if (cancelButtonDown){
			if (cancelCallback != null && cancelCallback.Action != null){
				cancelCallback.Action(cancelCallback.Sound);
			}
			return true;
		}
		return false;
	}

	#endregion

	#region private static methods
	/// <summary>
	/// 默认光标移动动作：无选中物时选择首个物体；当前为 Slider 时按方向键调节滑杆值，否则移动光标。
	/// </summary>
	/// <param name="screen">当前屏幕。</param>
	/// <param name="horizontalAxis">水平轴输入。</param>
	/// <param name="verticalAxis">垂直轴输入。</param>
	/// <param name="horizontalAxisDown">水平键是否按下。</param>
	/// <param name="verticalAxisDown">垂直键是否按下。</param>
	/// <param name="confirmButtonDown">确认键是否按下。</param>
	/// <param name="cancelButtonDown">取消键是否按下。</param>
	/// <param name="sound">移动音效。</param>
	private static void DefaultMoveCursorAction(
		this UFEScreen screen,
		Fix64 horizontalAxis,
		Fix64 verticalAxis,
		bool horizontalAxisDown,
		bool verticalAxisDown,
		bool confirmButtonDown,
		bool cancelButtonDown,
		AudioClip sound
	){
		bool axisDown = horizontalAxisDown || verticalAxisDown;

		//---------------------------------------------------------------------------------------------------------
		// Retrieve the current selected GameObject.
		// If no GameObject is selected and the player press any button, select the first GameObject at the screen.
		//---------------------------------------------------------------------------------------------------------
		GameObject currentGameObject = UFE.eventSystem.currentSelectedGameObject;
		if (currentGameObject == null && axisDown || confirmButtonDown || cancelButtonDown){
			currentGameObject = screen.FindFirstSelectableGameObject();
		}

		//---------------------------------------------------------------------------------------------------------
		// Check if the current Selectable Object is a Slider
		//---------------------------------------------------------------------------------------------------------
		Slider slider = currentGameObject != null ? currentGameObject.GetComponent<Slider>() : null;

		//-----------------------------------------------------------------------------------------------------
		// If the current Selectable Object is a Slider, check if the user has pressed a button
		// in the same direction (horizontal / vertical) than the slider, change the slider value.
		//
		// If the current Selectable Object is not an Slider or if the user hasn't pressed a button
		// in the same direction (horizontal / vertical) than the slider, move the cursor
		//-----------------------------------------------------------------------------------------------------
		if (slider != null){
			if (horizontalAxisDown && slider.direction == Slider.Direction.LeftToRight){
				if (slider.wholeNumbers){
					slider.value += FPMath.Sign(horizontalAxis);
				}else{
					slider.normalizedValue += FPMath.Sign(horizontalAxis)*UFEScreenExtensions.NormalizedSliderSpeed;
				}
			}else if (horizontalAxisDown && slider.direction == Slider.Direction.RightToLeft){
				if (slider.wholeNumbers){
					slider.value -= FPMath.Sign(horizontalAxis);
				}else{
					slider.normalizedValue -= FPMath.Sign(horizontalAxis)*UFEScreenExtensions.NormalizedSliderSpeed;
				}
			}else if (verticalAxisDown && slider.direction == Slider.Direction.BottomToTop){
				if (slider.wholeNumbers){
					slider.value += FPMath.Sign(verticalAxis);
				}else{
					slider.normalizedValue += FPMath.Sign(verticalAxis) * UFEScreenExtensions.NormalizedSliderSpeed;
				}
			}else if (verticalAxisDown && slider.direction==Slider.Direction.TopToBottom){
				if (slider.wholeNumbers){
					slider.value -= FPMath.Sign(verticalAxis);
				}else{
					slider.normalizedValue -= FPMath.Sign(verticalAxis) * UFEScreenExtensions.NormalizedSliderSpeed;
				}
			}else if (axisDown){
				screen.MoveCursor(new Vector3((float)horizontalAxis, (float)verticalAxis), sound);
			}
		}else if (axisDown){
			screen.MoveCursor(new Vector3((float)horizontalAxis, (float)verticalAxis), sound);
		}
	}

	/// <summary>
	/// 默认取消动作：播放取消音效并执行取消回调。
	/// </summary>
	/// <param name="action">取消动作。</param>
	/// <param name="sound">取消音效。</param>
	private static void DefaultCancelAction(this Action action, AudioClip sound){
		if (sound != null){
			UFE.PlaySound(sound);
		}
		
		if (action != null){
			action();
		}
	}

	/// <summary>
	/// 默认确认动作：若选中物是 Button 则触发其点击事件；若是 Toggle 则切换其状态。
	/// </summary>
	/// <param name="gameObject">选中的 GameObject。</param>
	/// <param name="sound">确认音效。</param>
	private static void DefaultConfirmAction(this GameObject gameObject, AudioClip sound){
		// Check if the GameObject is defined...
		if (gameObject != null){
			// Check if it's a button...
			Button currentButton = gameObject.GetComponent<Button>();
			if (currentButton != null){
				// In that case, raise the "On Click" event
				if (sound != null){
					UFE.PlaySound(sound);
				}

				if (currentButton.onClick != null){
					currentButton.onClick.Invoke();
				}
			}else{
				// Otherwise, check if it's a toggle...
				Toggle currentToggle = gameObject.GetComponent<Toggle>();
				if (currentToggle != null){
					// In that case, change the state of the toggle...
					if (sound != null){
						UFE.PlaySound(sound);
					}

					currentToggle.isOn = !currentToggle.isOn;
				}
			}
		}
	}

	/// <summary>
	/// 判断指定 Selectable 是否属于当前屏幕（沿父级链查找 UFEScreen 组件）。
	/// </summary>
	/// <param name="screen">目标屏幕。</param>
	/// <param name="selectable">要判断的 Selectable。</param>
	/// <returns>属于该屏幕返回 true。</returns>
	private static bool HasSelectable(this UFEScreen screen, Selectable selectable){
		if (selectable != null){
			Transform t = selectable.transform;
			UFEScreen s;

			while (t != null){
				s = t.GetComponent<UFEScreen>();

				if (s == screen){
					return true;
				}

				t = t.parent;
			}
		}
		return false;
	}

	/// <summary>
	/// 在指定白名单中查找方向上的下一个 Selectable（基于射线/投影夹角的最优选择，白名单为空时用 Unity 原生 FindSelectable）。
	/// </summary>
	/// <param name="s">当前 Selectable。</param>
	/// <param name="dir">方向。</param>
	/// <param name="whiteList">白名单。</param>
	/// <returns>下一个 Selectable；无则返回 null。</returns>
	private static Selectable FindSelectable(Selectable s, Vector3 dir, IList<Selectable> whiteList){
		if (whiteList == null || whiteList.Count == 0){
			return s.FindSelectable(dir);
		}else{
			dir = dir.normalized;
			Vector3 vector = Quaternion.Inverse (s.transform.rotation) * dir;
			Vector3 vector2 = s.transform.TransformPoint (UFEScreenExtensions.GetPointOnRectEdge(s.transform as RectTransform, vector));
			float num = float.NegativeInfinity;
			Selectable result = null;

			for (int i = 0; i < Selectable.allSelectables.Count; i++) {
				Selectable selectable = Selectable.allSelectables[i];
				if (selectable != s && selectable != null && whiteList.Contains(selectable)) {
					if (selectable.IsInteractable () && selectable.navigation.mode != Navigation.Mode.None) {
						RectTransform rectTransform = selectable.transform as RectTransform;
						Vector3 vector3 = (!(rectTransform != null)) ? Vector3.zero : new Vector3(rectTransform.rect.center.x, rectTransform.rect.center.y, 0f);
						Vector3 vector4 = selectable.transform.TransformPoint (vector3) - vector2;
						float num2 = Vector3.Dot (dir, vector4);
						if (num2 > 0) {
							float num3 = num2 / vector4.sqrMagnitude;
							if (num3 > num) {
								num = num3;
								result = selectable;
							}
						}
					}
				}
			}
			return result;
		}
	}

	/// <summary>
	/// 计算矩形边缘上沿指定方向的点（用于白名单方向查找的起点）。
	/// </summary>
	/// <param name="rect">矩形变换。</param>
	/// <param name="dir">方向。</param>
	/// <returns>矩形边缘上的点。</returns>
	private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
	{
		if (rect == null) {
			return Vector3.zero;
		}
		if (dir != Vector2.zero) {
			dir /= Mathf.Max (Mathf.Abs (dir.x), Mathf.Abs (dir.y));
		}
		dir = rect.rect.center + Vector2.Scale (rect.rect.size, dir * 0.5f);
		return dir;
	}
	#endregion
}
