using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FPLibrary;
using UFE3D;

/// <summary>
/// 默认角色选择界面（DefaultCharacterSelectionScreen）。
/// <para>用途：完整的选人界面——以网格（characters）展示可选角色，支持大头像（CharacterPortrait）与 3D 模型</para>
/// <para>（CharacterGameObject）两种展示模式；为双方玩家分别处理光标移动/确认/取消（含 CPU 模式下共用一套输入），</para>
/// <para>并在选人后播放角色选择动画、更新 HUD 光标位置。</para>
/// </summary>
public class DefaultCharacterSelectionScreen : CharacterSelectionScreen {
	#region public enum definitions
	/// <summary>角色展示模式。</summary>
	public enum DisplayMode{
		/// <summary>大头像（Image）。</summary>
		CharacterPortrait,
		/// <summary>3D 角色模型。</summary>
		CharacterGameObject,
	}
	#endregion

	#region public instance fields
	/// <summary>移动光标音效。</summary>
	public AudioClip moveCursorSound;
	/// <summary>加载音效。</summary>
	public AudioClip onLoadSound;
	/// <summary>背景音乐。</summary>
	public AudioClip music;
	/// <summary>加载时是否停止之前的音效。</summary>
	public bool stopPreviousSoundEffectsOnLoad = false;
	/// <summary>延迟播放音乐的时间。</summary>
	public float delayBeforePlayingMusic = 0.1f;
	/// <summary>玩家1名称文本。</summary>
	public Text namePlayer1;
	/// <summary>玩家2名称文本。</summary>
	public Text namePlayer2;
	/// <summary>角色展示模式。</summary>
	public DisplayMode displayMode = DisplayMode.CharacterPortrait;
	/// <summary>玩家1大头像。</summary>
	public Image portraitPlayer1;
	/// <summary>玩家2大头像。</summary>
	public Image portraitPlayer2;
	/// <summary>3D 展示模式的背景预制体。</summary>
	public GameObject background3dPrefab;
	/// <summary>玩家1 3D 模型位置。</summary>
	public Vector3 positionPlayer1 = new Vector3(-4,0,0);
	/// <summary>玩家2 3D 模型位置。</summary>
	public Vector3 positionPlayer2 = new Vector3(4,0,0);
	/// <summary>角色网格图片列表（选人按钮）。</summary>
	public Image[] characters;
	/// <summary>玩家1 HUD 光标。</summary>
	public Animator hudPlayer1;
	/// <summary>玩家2 HUD 光标。</summary>
	public Animator hudPlayer2;
	/// <summary>双方共用的 HUD 光标。</summary>
	public Animator hudBothPlayers;
	/// <summary>空位显示的无角色贴图。</summary>
	public Sprite noCharacterSprite;

	/// <summary>玩家1默认悬停角色索引。</summary>
	public int defaultCharacterPlayer1 = 0;
	/// <summary>玩家2默认悬停角色索引。</summary>
	public int defaultCharacterPlayer2 = 999;
	#endregion

	#region protected instance fields
	/// <summary>角色按钮白名单（导航用）。</summary>
	protected List<Selectable> characterButtonsWhiteList = new List<Selectable>();

	/// <summary>3D 展示模式的背景实例。</summary>
	protected GameObject background;
	/// <summary>玩家1 3D 模型实例。</summary>
	protected GameObject gameObjectPlayer1;
	/// <summary>玩家2 3D 模型实例。</summary>
	protected GameObject gameObjectPlayer2;
	#endregion

	#region public override methods
	/// <summary>
	/// 固定帧更新：按双人/单人（CPU）模式为双方分别或共用调用特殊导航系统处理光标移动与选择/取消。
	/// </summary>
	public override void DoFixedUpdate(
		IDictionary<InputReferences, InputEvents> player1PreviousInputs,
		IDictionary<InputReferences, InputEvents> player1CurrentInputs,
		IDictionary<InputReferences, InputEvents> player2PreviousInputs,
		IDictionary<InputReferences, InputEvents> player2CurrentInputs
	){
		base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);

		if (UFE.gameMode != GameMode.StoryMode && !UFE.GetCPU(2)){
			// If both characters will be controlled by human players...
			this.SpecialNavigationSystem(
				player1PreviousInputs
				, 
				player1CurrentInputs
				,
				new UFEScreenExtensions.MoveCursorCallback(
				delegate(
					Fix64 horizontalAxis, 
					Fix64 verticalAxis, 
					bool horizontalAxisDown, 
					bool verticalAxisDown, 
					bool confirmButtonDown, 
					bool cancelButtonDown, 
					AudioClip sound
				){
					this.MoveCursor(
						1,
						horizontalAxis,
						verticalAxis,
						horizontalAxisDown,
						verticalAxisDown,
						confirmButtonDown,
						cancelButtonDown,
						sound
					);
				})
				,
				new UFEScreenExtensions.ActionCallback(delegate(AudioClip sound){
					this.TrySelectCharacter(this.p1HoverIndex, 1);
				})
				,
				new UFEScreenExtensions.ActionCallback(delegate(AudioClip sound){
					this.TryDeselectCharacter(1);
				})
			);

			this.SpecialNavigationSystem( 
				player2PreviousInputs
				, 
				player2CurrentInputs
				,
				new UFEScreenExtensions.MoveCursorCallback(
				delegate(
					Fix64 horizontalAxis, 
					Fix64 verticalAxis, 
					bool horizontalAxisDown, 
					bool verticalAxisDown, 
					bool confirmButtonDown, 
					bool cancelButtonDown, 
					AudioClip sound
				){
					this.MoveCursor(
						2,
						horizontalAxis,
						verticalAxis,
						horizontalAxisDown,
						verticalAxisDown,
						confirmButtonDown,
						cancelButtonDown,
						sound
					);
				})
				,
				new UFEScreenExtensions.ActionCallback(delegate(AudioClip sound){
					this.TrySelectCharacter(this.p2HoverIndex, 2);
				})
				,
				new UFEScreenExtensions.ActionCallback(delegate(AudioClip sound){
					this.TryDeselectCharacter(2);
				})
			);
		}else{
			// If at least one characters will be controlled by the CPU...
			this.SpecialNavigationSystem(
				player1PreviousInputs
				, 
				player1CurrentInputs
				,
				new UFEScreenExtensions.MoveCursorCallback(delegate(
					Fix64 horizontalAxis, 
					Fix64 verticalAxis, 
					bool horizontalAxisDown, 
					bool verticalAxisDown, 
					bool confirmButtonDown, 
					bool cancelButtonDown, 
					AudioClip sound
				){
					this.MoveCursor(
						UFE.config.player1Character == null ? 1 : 2,
						horizontalAxis,
						verticalAxis,
						horizontalAxisDown,
						verticalAxisDown,
						confirmButtonDown,
						cancelButtonDown,
						sound
					);
				})
				,
				new UFEScreenExtensions.ActionCallback(this.TrySelectCharacter),
				new UFEScreenExtensions.ActionCallback(this.TryDeselectCharacter)
			);
		}
	}

	/// <summary>
	/// 设置悬停索引：更新角色名称/头像/3D 模型、HUD 光标位置。
	/// </summary>
	/// <param name="player">玩家编号。</param>
	/// <param name="characterIndex">角色索引。</param>
	public override void SetHoverIndex(int player, int characterIndex){
		if (!this.closing){
			int maxCharacterIndex = this.GetMaxCharacterIndex();
			this.p1HoverIndex = Mathf.Clamp(this.p1HoverIndex, 0, maxCharacterIndex);
			this.p2HoverIndex = Mathf.Clamp(this.p2HoverIndex, 0, maxCharacterIndex);
			base.SetHoverIndex(player, characterIndex);

			if (characterIndex >= 0 && characterIndex <= maxCharacterIndex){
				UFE3D.CharacterInfo character = this.selectableCharacters[characterIndex];

				// First, update the big portrait or the character 3D model (depending on the Display Mode)
				if (player == 1){
					if (this.namePlayer1 != null){
						this.namePlayer1.text = character.characterName;
					}

					if (this.displayMode == DisplayMode.CharacterPortrait){
						if (this.portraitPlayer1 != null){
							this.portraitPlayer1.sprite = Sprite.Create(
								character.profilePictureBig,
								new Rect(0f, 0f, character.profilePictureBig.width, character.profilePictureBig.height),
								new Vector2(0.5f * character.profilePictureBig.width, 0.5f * character.profilePictureBig.height)
							);
						}
					}else{
						UFE3D.CharacterInfo characterInfo = UFE.config.characters[characterIndex];
						if (this.gameObjectPlayer1 != null){
							GameObject.Destroy(this.gameObjectPlayer1);
						}


						AnimationClip clip = 
							characterInfo.selectionAnimation != null ?
							characterInfo.selectionAnimation : 
							characterInfo.moves[0].basicMoves.idle.animMap[0].clip;

                        
                        if (characterInfo.characterPrefabStorage == StorageMode.Legacy) {
                            this.gameObjectPlayer1 = GameObject.Instantiate(characterInfo.characterPrefab);
                        } else {
                            this.gameObjectPlayer1 = GameObject.Instantiate(Resources.Load<GameObject>(characterInfo.prefabResourcePath));
                        }
						//this.gameObjectPlayer1 = GameObject.Instantiate(characterInfo.characterPrefab);
						this.gameObjectPlayer1.transform.position = this.positionPlayer1;
						this.gameObjectPlayer1.transform.SetParent(this.transform, true);

						HitBoxesScript hitBoxes = this.gameObjectPlayer1.GetComponent<HitBoxesScript>();
						if (hitBoxes != null){
							foreach (HitBox hitBox in hitBoxes.hitBoxes){
								if (hitBox != null && hitBox.bodyPart != BodyPart.none && hitBox.position != null){
									hitBox.position.gameObject.SetActive(hitBox.defaultVisibility);
								}
							}
						}

						if (characterInfo.animationType == AnimationType.Legacy){
							Animation animation = this.gameObjectPlayer1.GetComponent<Animation>();
							if (animation == null){
								animation = this.gameObjectPlayer1.AddComponent<Animation>();
							}

							animation.AddClip(clip, "Idle");
							animation.wrapMode = WrapMode.Loop;
							animation.Play("Idle");
						}else {
							Animator animator = this.gameObjectPlayer1.GetComponent<Animator>();
							if (animator == null){
								animator = this.gameObjectPlayer1.AddComponent<Animator>();
							}
							
							AnimatorOverrideController overrideController = new AnimatorOverrideController();
							overrideController.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("MC_Controller");
							overrideController["State2"] = clip;
							
							animator.avatar = characterInfo.avatar;
							animator.applyRootMotion = characterInfo.applyRootMotion;
							animator.runtimeAnimatorController = overrideController;
							animator.Play("State2");
						}
					}
				}else if (player == 2){
					if (this.namePlayer2 != null){
						this.namePlayer2.text = character.characterName;
					}

					if (this.displayMode == DisplayMode.CharacterPortrait){
						if (this.portraitPlayer2 != null){
							this.portraitPlayer2.sprite = Sprite.Create(
								character.profilePictureBig,
								new Rect(0f, 0f, character.profilePictureBig.width, character.profilePictureBig.height),
								new Vector2(0.5f * character.profilePictureBig.width, 0.5f * character.profilePictureBig.height)
							);
						}
					}else{
						UFE3D.CharacterInfo characterInfo = UFE.config.characters[characterIndex];
						if (this.gameObjectPlayer2 != null){
							GameObject.Destroy(this.gameObjectPlayer2);
						}
						
						if (UFE.gameMode != GameMode.StoryMode){
							AnimationClip clip = 
								characterInfo.selectionAnimation != null ?
								characterInfo.selectionAnimation : 
								characterInfo.moves[0].basicMoves.idle.animMap[0].clip;
                            
                            if (characterInfo.characterPrefabStorage == StorageMode.Legacy) {
                                this.gameObjectPlayer2 = GameObject.Instantiate(characterInfo.characterPrefab);
                            } else {
                                this.gameObjectPlayer2 = GameObject.Instantiate(Resources.Load<GameObject>(characterInfo.prefabResourcePath));
                            }
							//this.gameObjectPlayer2 = GameObject.Instantiate(characterInfo.characterPrefab);
							this.gameObjectPlayer2.transform.position = this.positionPlayer2;
							this.gameObjectPlayer2.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
							this.gameObjectPlayer2.transform.SetParent(this.transform, true);

							HitBoxesScript hitBoxes = this.gameObjectPlayer2.GetComponent<HitBoxesScript>();
							if (hitBoxes != null){
								foreach (HitBox hitBox in hitBoxes.hitBoxes){
									if (hitBox != null && hitBox.bodyPart != BodyPart.none && hitBox.position != null){
										hitBox.position.gameObject.SetActive(hitBox.defaultVisibility);
									}
								}
							}
							
							if (characterInfo.animationType == AnimationType.Legacy){
								Animation animation = this.gameObjectPlayer2.GetComponent<Animation>();
								if (animation == null){
									animation = this.gameObjectPlayer2.AddComponent<Animation>();
								}
								
								this.gameObjectPlayer2.transform.localScale = new Vector3(
									-this.gameObjectPlayer2.transform.localScale.x, 
									this.gameObjectPlayer2.transform.localScale.y, 
									this.gameObjectPlayer2.transform.localScale.z
								);
								
								animation.AddClip(clip, "Idle");
								animation.wrapMode = WrapMode.Loop;
								animation.Play("Idle");
							}else{
								Animator animator = this.gameObjectPlayer2.GetComponent<Animator>();
								if (animator == null){
									animator = this.gameObjectPlayer2.AddComponent<Animator>();
								}
								
								// Mecanim, mirror via Animator...
								AnimatorOverrideController overrideController = new AnimatorOverrideController();
                                overrideController.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("MC_Controller");
                                overrideController["State3"] = clip;
								
								animator.avatar = characterInfo.avatar;
								animator.applyRootMotion = characterInfo.applyRootMotion;
								animator.runtimeAnimatorController = overrideController;
                                animator.Play("State3");
							}
						}
					}
				}

				// Deal with alternative colors if both players have selected the same character
				/*if (this.gameObjectPlayer2 != null && this.displayMode == DisplayMode.CharacterGameObject){
					UFE3D.CharacterInfo p2CharacterInfo = UFE.config.characters[this.p2HoverIndex];
					if (p2CharacterInfo.enableAlternativeColor && this.p1HoverIndex == this.p2HoverIndex){
						foreach(Renderer renderer in this.gameObjectPlayer2.GetComponentsInChildren<Renderer>()){
							renderer.material.color = p2CharacterInfo.alternativeColor;
						}
					}else{
						Renderer[] originalRenderers = p2CharacterInfo.characterPrefab.GetComponentsInChildren<Renderer>(true);
						Renderer[] instanceRenderers = this.gameObjectPlayer2.GetComponentsInChildren<Renderer>(true);

						for (int i = 0; i < originalRenderers.Length && i < instanceRenderers.Length; ++i){
							instanceRenderers[i].material.color = originalRenderers[i].sharedMaterial.color;
						}
					}
				}*/

				// Then, update the cursor position
				if (this.hudPlayer1 != null){
					RectTransform rt = this.hudPlayer1.transform as RectTransform;
					if (rt != null){
						rt.anchoredPosition = this.characters[this.p1HoverIndex].rectTransform.anchoredPosition;
					}else{
						this.hudPlayer1.transform.position = this.characters[this.p1HoverIndex].transform.position;
					}
				}

				if (this.hudPlayer2 != null){
					RectTransform rt = this.hudPlayer2.transform as RectTransform;
					if (rt != null){
						rt.anchoredPosition = this.characters[this.p2HoverIndex].rectTransform.anchoredPosition;
					}else{
						this.hudPlayer2.transform.position = this.characters[this.p2HoverIndex].transform.position;
					}
				}

				if (this.hudBothPlayers != null){
					RectTransform rt = this.hudBothPlayers.transform as RectTransform;
					if (rt != null){
						rt.anchoredPosition = this.characters[this.p2HoverIndex].rectTransform.anchoredPosition;
					}else{
						this.hudBothPlayers.transform.position = this.characters[this.p2HoverIndex].transform.position;
					}
				}
			}

			this.UpdateHud();
		}
	}

	/// <summary>
	/// 角色选择确认回调：调用基类逻辑后更新 HUD。
	/// </summary>
	/// <param name="characterIndex">角色索引。</param>
	/// <param name="player">操作玩家。</param>
	public override void OnCharacterSelectionAllowed (int characterIndex, int player){
		base.OnCharacterSelectionAllowed (characterIndex, player);
		this.UpdateHud();
	}

	/// <summary>
	/// 界面隐藏时：销毁 3D 模型/背景并恢复 Canvas 为 Overlay 渲染模式。
	/// </summary>
	public override void OnHide(){
		if (this.gameObjectPlayer1 != null){
			GameObject.Destroy(this.gameObjectPlayer1);
		}
		if (this.gameObjectPlayer2 != null){
			GameObject.Destroy(this.gameObjectPlayer2);
		}
		if (this.background != null){
			GameObject.Destroy(this.background);
		}

		UFE.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		UFE.canvas.worldCamera = null;
		base.OnHide ();
	}

	/// <summary>
	/// 界面显示时：设置摄像机与 3D 背景（按展示模式）、创建角色按钮、播放音乐/音效并初始化默认悬停。
	/// </summary>
	public override void OnShow (){
		// We add these lines before base.OnShow() because they will affect how will the engine display
		// characters selected by default
        Camera.main.transform.position = UFE.config.cameraOptions.initialDistance;
        Camera.main.transform.eulerAngles = UFE.config.cameraOptions.initialRotation;
        Camera.main.fieldOfView = UFE.config.cameraOptions.initialFieldOfView;
		if (this.displayMode == DisplayMode.CharacterGameObject){
			if (background3dPrefab != null){
				this.background = GameObject.Instantiate(background3dPrefab);
			}

			UFE.canvas.planeDistance = 0.1f;
			UFE.canvas.worldCamera = Camera.main;
			UFE.canvas.renderMode = RenderMode.ScreenSpaceCamera;
		}

		base.OnShow();
		this.characterButtonsWhiteList.Clear();

		// Set the portraits of the characters
		if (this.characters != null){
			// First, update the portraits of the characters until we run out of characters or portrait slots....
			for (int i = 0; i < this.selectableCharacters.Length && i < this.characters.Length; ++i){
				Image character = this.characters[i];
				UFE3D.CharacterInfo selectableCharacter = this.selectableCharacters[i];

				if (character != null){
					character.gameObject.SetActive(true);
					character.sprite = Sprite.Create(
						selectableCharacter.profilePictureSmall,
						new Rect(0f, 0f, selectableCharacter.profilePictureSmall.width, selectableCharacter.profilePictureSmall.height),
						new Vector2(0.5f * selectableCharacter.profilePictureSmall.width, 0.5f * selectableCharacter.profilePictureSmall.height)
					);

					Button button = character.GetComponent<Button>();
					if (button == null){
						button = character.gameObject.AddComponent<Button>();
					}
					
					int index = i;
					button.onClick.AddListener(() => {this.TrySelectCharacter(index);});
					button.targetGraphic = character;
					this.characterButtonsWhiteList.Add(button);
				}
			}

			// If there are more slots than characters, fill the remaining slots with the "No Character" sprite...
			// If the "No Character" sprite is undefined, hide the image instead.
			for (int i = this.selectableCharacters.Length; i < this.characters.Length; ++i){
				Image character = this.characters[i];
				if (character != null){
					if (this.noCharacterSprite != null){
						this.characters[i].gameObject.SetActive(true);
						this.characters[i].sprite = this.noCharacterSprite;
					}else{
						this.characters[i].gameObject.SetActive(false);
					}
				}
			}
		}

		if (this.music != null){
			UFE.DelayLocalAction(delegate(){UFE.PlayMusic(this.music);}, this.delayBeforePlayingMusic);
		}

		if (this.stopPreviousSoundEffectsOnLoad){
			UFE.StopSounds();
		}
		
		if (this.onLoadSound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.onLoadSound);}, this.delayBeforePlayingMusic);
		}

		this.SetHoverIndex(1, Mathf.Clamp(this.defaultCharacterPlayer1, 0, this.selectableCharacters.Length - 1));
		if (UFE.gameMode == GameMode.StoryMode){
			if (this.namePlayer2 != null){
				this.namePlayer2.text = "???";
			}

			if (this.portraitPlayer2 != null){
				this.portraitPlayer2.gameObject.SetActive(false);
			}

			this.UpdateHud();
		}else{
			this.SetHoverIndex(2, Mathf.Clamp(this.defaultCharacterPlayer2, 0, this.selectableCharacters.Length - 1));

			if (this.portraitPlayer2 != null){
				this.portraitPlayer2.gameObject.SetActive(true);
			}
		}
	}
	#endregion

	#region protected instance methods
	/// <summary>
	/// 获取最大角色索引（角色列表与按钮网格数量取小）。
	/// </summary>
	/// <returns>最大索引。</returns>
	protected override int GetMaxCharacterIndex(){
		return Mathf.Min(this.selectableCharacters.Length, this.characters.Length) - 1;
	}

	/// <summary>
	/// 更新 HUD 光标状态：根据选择状态与双人是否同框显示各自的隐藏/选中动画参数。
	/// </summary>
	protected virtual void UpdateHud(){
		if (UFE.gameMode == GameMode.StoryMode){
			if (this.hudPlayer1 != null){
				this.hudPlayer1.SetBool("IsHidden", false);
				this.hudPlayer1.SetBool("IsSelected", UFE.config.player1Character != null);
			}
			
			if (this.hudPlayer2 != null){
				this.hudPlayer2.SetBool("IsHidden", true);
				this.hudPlayer2.SetBool("IsSelected", UFE.config.player2Character != null);
			}
			
			if (this.hudBothPlayers != null){
				this.hudBothPlayers.SetBool("IsHidden", true);
				this.hudBothPlayers.SetBool("IsSelected", UFE.config.player1Character != null && UFE.config.player2Character != null);
			}
		}else{
			if (this.hudPlayer1 != null){
				this.hudPlayer1.SetBool("IsHidden", this.p1HoverIndex == this.p2HoverIndex);
				this.hudPlayer1.SetBool("IsSelected", UFE.config.player1Character != null);
			}
			
			if (this.hudPlayer2 != null){
				this.hudPlayer2.SetBool("IsHidden", this.p1HoverIndex == this.p2HoverIndex);
				this.hudPlayer2.SetBool("IsSelected", UFE.config.player2Character != null);
			}

			if (this.hudBothPlayers != null){
				this.hudBothPlayers.SetBool("IsHidden", this.p1HoverIndex != this.p2HoverIndex);

				this.hudBothPlayers.SetBool(
					"IsSelected", 
					UFE.config.player1Character != null && UFE.config.player2Character != null
				);
			}
		}
	}

	/// <summary>
	/// 移动光标到指定角色（索引变化时播放移动音效）。
	/// </summary>
	/// <param name="player">玩家编号。</param>
	/// <param name="characterIndex">角色索引。</param>
	protected virtual void MoveCursor(int player, int characterIndex){
		int previousIndex = this.GetHoverIndex(player);
		this.SetHoverIndex(player, characterIndex);
		int newIndex = this.GetHoverIndex(player);
		if (previousIndex != newIndex && this.moveCursorSound != null) UFE.PlaySound(this.moveCursorSound);
	}
	#endregion

	#region protected instance methods: methods required by the Special Navigation System (GUI)
	/// <summary>
	/// 光标移动处理（特殊导航系统回调）：角色未选中时按方向键在角色网格中查找相邻可选角色并移动光标。
	/// </summary>
	/// <param name="player">玩家编号。</param>
	/// <param name="horizontalAxis">水平轴输入。</param>
	/// <param name="verticalAxis">垂直轴输入。</param>
	/// <param name="horizontalAxisDown">水平键是否按下。</param>
	/// <param name="verticalAxisDown">垂直键是否按下。</param>
	/// <param name="confirmButtonDown">确认键是否按下。</param>
	/// <param name="cancelButtonDown">取消键是否按下。</param>
	/// <param name="sound">光标音效。</param>
	protected virtual void MoveCursor(
		int player,
		Fix64 horizontalAxis, 
		Fix64 verticalAxis, 
		bool horizontalAxisDown, 
		bool verticalAxisDown, 
		bool confirmButtonDown, 
		bool cancelButtonDown, 
		AudioClip sound
	){
		bool characterSelected = true;
		int currentIndex = -1;
		
		if (player == 1){
			currentIndex = this.p1HoverIndex;
			characterSelected = UFE.config.player1Character != null;
		}else if (player == 2){
			currentIndex = this.p2HoverIndex;
			characterSelected = UFE.config.player2Character != null;
		}
		
		if (!characterSelected || currentIndex < 0){
			Vector3 direction = Vector3.zero;

			if (horizontalAxisDown){
				if (horizontalAxis > 0)				direction = Vector3.right;
				else if (horizontalAxis < 0)		direction = Vector3.left;
			}

			if (verticalAxisDown){
				if (verticalAxis > 0)				direction = Vector3.up;
				else if (verticalAxis < 0)			direction = Vector3.down;
			}

			if (direction != Vector3.zero){
				GameObject currentGameObject = this.characters[currentIndex].gameObject;
				GameObject nextGameObject = currentGameObject.FindSelectableGameObject(
					direction, 
					this.wrapInput,
					this.characterButtonsWhiteList
				);

				if (nextGameObject != null && nextGameObject != currentGameObject){
					int index = -1;
					
					for (int i = 0; i < this.characters.Length; ++i){
						if (this.characters[i].gameObject == nextGameObject){
							index = i;
							break;
						}
					}
					
					this.MoveCursor(player, index);
				}
			}
		}
	}
	
	/// <summary>
	/// 取消选择回调（特殊导航系统）。
	/// </summary>
	/// <param name="sound">取消音效。</param>
	protected virtual void TryDeselectCharacter(AudioClip sound){
		this.TryDeselectCharacter();
	}
	
	/// <summary>
	/// 确认选择回调（特殊导航系统）。
	/// </summary>
	/// <param name="sound">选择音效。</param>
	protected virtual void TrySelectCharacter(AudioClip sound){
		this.TrySelectCharacter();
	}
	#endregion
}
