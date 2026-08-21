using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UFE3D;

/// <summary>
/// 默认战斗 HUD（DefaultBattleGUI）。
/// <para>用途：战斗中血条/能量条/计时/回合标记/文字提示/播报员/暂停菜单/训练输入显示等全部 HUD 的默认实现，</para>
/// <para>覆写 BattleGUI 的各个事件回调以更新 UI，并驱动生命值动画（缓慢减少/增加）与屏幕提示动画。</para>
/// </summary>
public class DefaultBattleGUI : BattleGUI{
	#region public class definitions
	/// <summary>
	/// 玩家 GUI 数据：一名玩家在 HUD 上的全部显示元素（名称/头像/血条/能量条/回合标记/提示）。
	/// </summary>
	[Serializable]
	public class PlayerGUI{
		/// <summary>角色名称文本。</summary>
		public Text name;
		/// <summary>角色头像。</summary>
		public Image portrait;
		/// <summary>生命条。</summary>
		public Image lifeBar;
		/// <summary>能量条。</summary>
		public Image gaugeMeter;
		/// <summary>已获胜回合标记图片数组。</summary>
		public Image[] wonRoundsImages;
		/// <summary>玩家专属文字提示。</summary>
		public AlertGUI alert = new AlertGUI();
	}

	/// <summary>
	/// 提示 GUI 数据：一条屏幕文字提示的显示/动画配置。
	/// </summary>
	[Serializable]
	public class AlertGUI{
		/// <summary>提示文本。</summary>
		public Text text;
		/// <summary>提示初始位置（出现位置）。</summary>
		public Vector3 initialPosition;
		/// <summary>提示最终位置（移向位置）。</summary>
		public Vector3 finalPosition;
		/// <summary>提示移动速度。</summary>
		public float movementSpeed = 15f;
	}

	/// <summary>
	/// 回合标记 GUI 配置：胜/负/未决回合的贴图与显示模式。
	/// </summary>
	[Serializable]
	public class WonRoundsGUI{
		/// <summary>未决回合贴图。</summary>
		public Sprite NotFinishedRounds;
		/// <summary>获胜回合贴图。</summary>
		public Sprite WonRounds;
		/// <summary>失败回合贴图。</summary>
		public Sprite LostRounds;
		/// <summary>显示模式（只显示胜场/显示全部回合）。</summary>
		public DefaultBattleGUI.VisibleImages VisibleImages = DefaultBattleGUI.VisibleImages.WonRounds;

		/// <summary>
		/// 计算需要显示的回合标记图片数量。
		/// </summary>
		/// <returns>图片数量。</returns>
		public int GetNumberOfRoundsImages(){
			// To calculate the target number of images, check if the "Lost Rounds" Sprite is defined or not
			if (this.VisibleImages == VisibleImages.AllRounds){
				return UFE.config.roundOptions.totalRounds;
			}
			return (UFE.config.roundOptions.totalRounds + 1) / 2;
		}
	}

	/// <summary>回合标记显示模式。</summary>
	public enum VisibleImages{
		/// <summary>只显示胜场。</summary>
		WonRounds,
		/// <summary>显示全部回合（胜+负）。</summary>
		AllRounds,
	}
	#endregion

	#region public instance properties
	/// <summary>是否静音播报员语音。</summary>
	public bool muteAnnouncer = false;
	/// <summary>播报员选项（回合/命中/连击等语音）。</summary>
    public AnnouncerOptions announcer;
	/// <summary>回合标记配置。</summary>
	public WonRoundsGUI wonRounds = new WonRoundsGUI();
	/// <summary>玩家1 HUD 数据。</summary>
	public PlayerGUI player1GUI = new PlayerGUI();
	/// <summary>玩家2 HUD 数据。</summary>
	public PlayerGUI player2GUI = new PlayerGUI();
	/// <summary>中央主提示（回合/开战/K.O. 等）。</summary>
	public AlertGUI mainAlert = new AlertGUI();
	/// <summary>信息文本（回合开始等）。</summary>
	public Text info;
	/// <summary>回合计时器文本。</summary>
	public Text timer;
	/// <summary>生命值减少时的动画速度。</summary>
	public float lifeDownSpeed = 500f;
	/// <summary>生命值增加时的动画速度。</summary>
	public float lifeUpSpeed = 900f;
	/// <summary>暂停菜单预制体。</summary>
    public UFEScreen pauseScreen;
	/// <summary>网络对战中本地玩家指示箭头贴图。</summary>
    public Sprite networkPlayerPointer;
	/// <summary>本地玩家指示箭头的显示时长。</summary>
    public float pointerTimer = 4f;
	#endregion

	#region protected instance properties
	/// <summary>玩家1 训练模式按键图标列表（按帧）。</summary>
	public List<List<Image>> player1ButtonPresses = new List<List<Image>>(12);
	/// <summary>玩家1 训练模式输入引用历史。</summary>
	public List<InputReferences[]> player1InputReferences = new List<InputReferences[]>(12);
	/// <summary>玩家2 训练模式按键图标列表（按帧）。</summary>
	public List<List<Image>> player2ButtonPresses = new List<List<Image>>(12);
	/// <summary>玩家2 训练模式输入引用历史。</summary>
	public List<InputReferences[]> player2InputReferences = new List<InputReferences[]>(12);

	/// <summary>是否显示训练模式输入信息。</summary>
	protected bool showInputs = true;
	/// <summary>HUD 是否正在隐藏（暂停恢复时避免恢复音乐）。</summary>
	protected bool hiding = false;

	/// <summary>玩家1 提示显示剩余时间。</summary>
	protected float player1AlertTimer = 0f;
	/// <summary>玩家2 提示显示剩余时间。</summary>
	protected float player2AlertTimer = 0f;
	/// <summary>中央提示显示剩余时间。</summary>
	protected float mainAlertTimer = 0f;
	/// <summary>当前暂停菜单实例。</summary>
	protected UFEScreen pause = null;
	#endregion

	#region public instance methods
	/// <summary>
	/// 添加输入信息（转发到 OnInput，供训练模式显示按键）。
	/// </summary>
	/// <param name="inputReferences">输入引用列表。</param>
	/// <param name="player">玩家编号。</param>
	public void AddInput (InputReferences[] inputReferences, int player){
		this.OnInput(inputReferences, player);
	}
	#endregion

	#region public override methods
	/// <summary>
	/// 固定帧更新：驱动提示文字动画与生命值动画、检测 Start 键暂停、更新血条/能量条、
	/// 显示网络本地玩家指示箭头并转发输入到暂停菜单。
	/// </summary>
	public override void DoFixedUpdate(
		IDictionary<InputReferences, InputEvents> player1PreviousInputs,
		IDictionary<InputReferences, InputEvents> player1CurrentInputs,
		IDictionary<InputReferences, InputEvents> player2PreviousInputs,
		IDictionary<InputReferences, InputEvents> player2CurrentInputs
	){
		base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);

		if (this.isRunning){
			float deltaTime = (float)UFE.fixedDeltaTime;

			// Animate the alert messages if they exist
			if (this.player1GUI != null && this.player1GUI.alert != null && this.player1GUI.alert.text != null){
				this.player1GUI.alert.text.rectTransform.anchoredPosition = Vector3.Lerp(
					this.player1GUI.alert.text.rectTransform.anchoredPosition, 
					this.player1GUI.alert.finalPosition, 
					this.player1GUI.alert.movementSpeed * deltaTime
				);

				if (this.player1AlertTimer > 0f){
					this.player1AlertTimer -= deltaTime;
				}else if (!string.IsNullOrEmpty(this.player1GUI.alert.text.text)){
					this.player1GUI.alert.text.text = string.Empty;
				}
			}

			if (this.player2GUI != null && this.player2GUI.alert != null && this.player2GUI.alert.text != null){
				this.player2GUI.alert.text.rectTransform.anchoredPosition = Vector3.Lerp(
					this.player2GUI.alert.text.rectTransform.anchoredPosition, 
					this.player2GUI.alert.finalPosition, 
					this.player2GUI.alert.movementSpeed * deltaTime
				);

				if (this.player2AlertTimer > 0f){
					this.player2AlertTimer -= deltaTime;
				}else if (!string.IsNullOrEmpty(this.player2GUI.alert.text.text)){
					this.player2GUI.alert.text.text = string.Empty;
				}
			}

			if (this.mainAlert != null && this.mainAlert.text != null){
				if (this.mainAlertTimer > 0f){
					this.mainAlertTimer -= deltaTime;
				}else if (!string.IsNullOrEmpty(this.mainAlert.text.text)){
					this.mainAlert.text.text = string.Empty;
				}
			}

			
			// Animate life points when it goes down (P1)
			if (this.player1.targetLife > UFE.config.player1Character.currentLifePoints){
				this.player1.targetLife -= this.lifeDownSpeed * deltaTime;
                if (this.player1.targetLife < UFE.config.player1Character.currentLifePoints)
                    this.player1.targetLife = (float)UFE.config.player1Character.currentLifePoints;
			}
			if (this.player1.targetLife < UFE.config.player1Character.currentLifePoints){
                this.player1.targetLife += this.lifeUpSpeed * deltaTime;
                if (this.player1.targetLife > UFE.config.player1Character.currentLifePoints)
                    this.player1.targetLife = (float)UFE.config.player1Character.currentLifePoints;
			}
			
			// Animate life points when it goes down (P2)
			if (this.player2.targetLife > UFE.config.player2Character.currentLifePoints){
                this.player2.targetLife -= this.lifeDownSpeed * deltaTime;
                if (this.player2.targetLife < UFE.config.player2Character.currentLifePoints)
                    this.player2.targetLife = (float)UFE.config.player2Character.currentLifePoints;
			}
			if (this.player2.targetLife < UFE.config.player2Character.currentLifePoints){
                this.player2.targetLife += this.lifeUpSpeed * deltaTime;
                if (this.player2.targetLife > UFE.config.player2Character.currentLifePoints)
                    this.player2.targetLife = (float)UFE.config.player2Character.currentLifePoints;
			}


			bool player1CurrentStartButton = false;
			foreach (KeyValuePair<InputReferences, InputEvents> pair in player1CurrentInputs){
				if (pair.Key.inputType == InputType.Button && pair.Key.engineRelatedButton == ButtonPress.Start){
					player1CurrentStartButton = pair.Value.button;
					break;
				}
			}

			bool player1PreviousStartButton = false;
			foreach (KeyValuePair<InputReferences, InputEvents> pair in player1PreviousInputs){
				if (pair.Key.inputType == InputType.Button && pair.Key.engineRelatedButton == ButtonPress.Start){
					player1PreviousStartButton = pair.Value.button;
					break;
				}
			}

			bool player2CurrentStartButton = false;
			foreach (KeyValuePair<InputReferences, InputEvents> pair in player2CurrentInputs){
				if (pair.Key.inputType == InputType.Button && pair.Key.engineRelatedButton == ButtonPress.Start){
					player2CurrentStartButton = pair.Value.button;
					break;
				}
			}

			bool player2PreviousStartButton = false;
			foreach (KeyValuePair<InputReferences, InputEvents> pair in player2PreviousInputs){
				if (pair.Key.inputType == InputType.Button && pair.Key.engineRelatedButton == ButtonPress.Start){
					player2PreviousStartButton = pair.Value.button;
					break;
				}
			}

			if(
				// Check if both players have their life points above zero...
				UFE.config.player1Character.currentLifePoints > 0 &&
				UFE.config.player2Character.currentLifePoints > 0 &&
				UFE.gameMode != GameMode.NetworkGame &&
				(
					// and at least one of the players have pressed the Start button...
					player1CurrentStartButton && !player1PreviousStartButton ||
					player2CurrentStartButton && !player2PreviousStartButton 
				)
			){
				// In that case, we can process pause menu events
				UFE.PauseGame(!UFE.isPaused());
			}


			// Draw the Life Bars and Gauge Meters using the data stored in UFE.config.guiOptions
			if (this.player1GUI != null && this.player1GUI.lifeBar != null){
				this.player1GUI.lifeBar.fillAmount = this.player1.targetLife / this.player1.totalLife;
			}
			
			if (this.player2GUI != null && this.player2GUI.lifeBar != null){
				this.player2GUI.lifeBar.fillAmount = this.player2.targetLife / this.player2.totalLife;
			}

			if (UFE.config.gameGUI.hasGauge){
				if (this.player1GUI != null && this.player1GUI.gaugeMeter != null){
                    this.player1GUI.gaugeMeter.fillAmount = (float)UFE.config.player1Character.currentGaugePoints / UFE.config.player1Character.maxGaugePoints;
				}

				if (this.player2GUI != null && this.player2GUI.gaugeMeter != null){
                    this.player2GUI.gaugeMeter.fillAmount = (float)UFE.config.player2Character.currentGaugePoints / UFE.config.player2Character.maxGaugePoints;
				}
			}

			if (this.pause != null){
				this.pause.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
			}


			/*
			if (Debug.isDebugBuild){
				player1NameGO.guiText.text = string.Format(
					"{0}\t\t({1},\t{2},\t{3})",
					this.player1.characterName,
					UFE.GetPlayer1ControlsScript().transform.position.x,
					UFE.GetPlayer1ControlsScript().transform.position.y,
					UFE.GetPlayer1ControlsScript().transform.position.z
				);

				player2NameGO.guiText.text = string.Format(
					"{0}\t\t({1},\t{2},\t{3})",
					this.player2.characterName,
					UFE.GetPlayer2ControlsScript().transform.position.x,
					UFE.GetPlayer2ControlsScript().transform.position.y,
					UFE.GetPlayer2ControlsScript().transform.position.z
				);
			}
			*/
		}
	}

	/// <summary>
	/// 界面隐藏时：销毁训练模式按键图标、关闭调试器、暂停状态复位。
	/// </summary>
	public override void OnHide (){
		if (this.player1ButtonPresses != null){
			foreach (List<Image> images in this.player1ButtonPresses){
				if (images != null){
					foreach (Image image in images){
						if (image != null){
							GameObject.Destroy(image.gameObject);
						}
					}
				}
			}
			this.player1ButtonPresses.Clear();
		}

		if (this.player2ButtonPresses != null){
			foreach (List<Image> images in this.player2ButtonPresses){
				if (images != null){
					foreach (Image image in images){
						if (image != null){
							GameObject.Destroy(image.gameObject);
						}
					}
				}
			}
			this.player2ButtonPresses.Clear();
		}

		UFE.debugger1.enabled = false;
		UFE.debugger2.enabled = false;

		this.hiding = true;
		this.OnGamePaused(false);
		base.OnHide ();
	}

	/// <summary>
	/// 界面显示时：复位隐藏标志并按连击数降序排序播报员连击音效。
	/// </summary>
	public override void OnShow (){
		base.OnShow();
		this.hiding = false;

		/*if (UFE.config.debugOptions.debugMode){
			UFE.debugger1.enabled = true;
			UFE.debugger2.enabled = true;
		}else{
			UFE.debugger1.enabled = false;
			UFE.debugger2.enabled = false;
		}*/

		if (this.announcer != null){
			Array.Sort(this.announcer.combos, delegate(ComboAnnouncer c1, ComboAnnouncer c2) {
				return c2.hits.CompareTo(c1.hits);
			});
		}
	}

	/// <summary>
	/// 处理菜单选项：转发给暂停菜单。
	/// </summary>
	/// <param name="option">选项索引。</param>
	/// <param name="player">操作玩家。</param>
	public override void SelectOption(int option, int player){
		if (this.pause != null){
			this.pause.SelectOption(option, player);
		}
	}
	#endregion

	#region protected instance methods
	/// <summary>
	/// 处理提示消息：按消息类型播放对应的播报员/音效（连击/弹反/反击/先手/开战/K.O.），
	/// 其余消息直接替换占位符返回。
	/// </summary>
	/// <param name="msg">提示消息文本。</param>
	/// <param name="controlsScript">相关角色控制脚本（可为 null）。</param>
	/// <returns>替换占位符后的显示文本。</returns>
	protected virtual string ProcessMessage(string msg, ControlsScript controlsScript){
		if (msg == UFE.config.selectedLanguage.combo){
			if (this.announcer != null && !this.muteAnnouncer){
				foreach(ComboAnnouncer comboAnnouncer in this.announcer.combos){
					if (controlsScript.opControlsScript.comboHits >= comboAnnouncer.hits){
						UFE.PlaySound(comboAnnouncer.audio);
						break;
					}
				}
			}
		}else if (msg == UFE.config.selectedLanguage.parry){
			if (this.announcer != null && !this.muteAnnouncer){
				UFE.PlaySound(this.announcer.parry);
			}
			UFE.PlaySound(UFE.config.blockOptions.parrySound);
		}else if (msg == UFE.config.selectedLanguage.counterHit){
			if (this.announcer != null && !this.muteAnnouncer){
				UFE.PlaySound(this.announcer.counterHit);
			}
			UFE.PlaySound(UFE.config.counterHitOptions.sound);
		}else if (msg == UFE.config.selectedLanguage.firstHit){
			if (this.announcer != null && !this.muteAnnouncer){
				UFE.PlaySound(this.announcer.firstHit);
			}
		}else if (msg == UFE.config.selectedLanguage.fight){
			if (this.announcer != null && !this.muteAnnouncer){
				UFE.PlaySound(this.announcer.fight);
			}
		}else if (msg == UFE.config.selectedLanguage.ko){
			if (this.announcer != null && !this.muteAnnouncer && this.announcer.ko != null){
				UFE.PlaySound(this.announcer.ko);
			}
		}else{
			return this.SetStringValues(msg, null);
		}

		return this.SetStringValues(msg, controlsScript);
	}

	/// <summary>
	/// 替换消息中的占位符：%combo%（连击数）、%character%（角色名）、%round%（回合数）。
	/// </summary>
	/// <param name="msg">消息文本。</param>
	/// <param name="controlsScript">相关角色控制脚本（可为 null）。</param>
	/// <returns>替换后的文本。</returns>
	protected virtual string SetStringValues(string msg, ControlsScript controlsScript){
		UFE3D.CharacterInfo character = controlsScript != null ? controlsScript.myInfo : null;
		if (controlsScript != null) msg = msg.Replace("%combo%", controlsScript.opControlsScript.comboHits.ToString());
		if (character != null)		msg = msg.Replace("%character%", character.characterName);
		msg = msg.Replace("%round%", UFE.config.currentRound.ToString());

		return msg;
	}
	#endregion

	#region protected override methods
	/// <summary>
	/// 游戏开始回调：初始化回合标记图片、设置角色名称/头像、计时器与血条/能量条初始值。
	/// </summary>
	/// <param name="player1">玩家1角色。</param>
	/// <param name="player2">玩家2角色。</param>
	/// <param name="stage">场地。</param>
	protected override void OnGameBegin (UFE3D.CharacterInfo player1, UFE3D.CharacterInfo player2, StageOptions stage){
		base.OnGameBegin (player1, player2, stage);

		if (this.wonRounds.NotFinishedRounds == null){
			Debug.LogError("\"Not Finished Rounds\" Sprite not found! Make sure you have set the sprite correctly in the Editor");
		}else if (this.wonRounds.WonRounds == null){
			Debug.LogError("\"Won Rounds\" Sprite not found! Make sure you have set the sprite correctly in the Editor");
		}else if (this.wonRounds.LostRounds == null && this.wonRounds.VisibleImages == DefaultBattleGUI.VisibleImages.AllRounds){
			Debug.LogError("\"Lost Rounds\" Sprite not found! If you want to display Lost Rounds, make sure you have set the sprite correctly in the Editor");
		}else{
			// To calculate the target number of images, check if the "Lost Rounds" Sprite is defined or not
			int targetNumberOfImages = this.wonRounds.GetNumberOfRoundsImages();

			if(
				this.player1GUI != null && 
				this.player1GUI.wonRoundsImages != null && 
				this.player1GUI.wonRoundsImages.Length >= targetNumberOfImages
			){
				for (int i = 0; i < targetNumberOfImages; ++i){
					this.player1GUI.wonRoundsImages[i].enabled = true;
					this.player1GUI.wonRoundsImages[i].sprite = this.wonRounds.NotFinishedRounds;
				}
					
				for (int i = targetNumberOfImages; i < this.player1GUI.wonRoundsImages.Length; ++i){
					this.player1GUI.wonRoundsImages[i].enabled = false;
				}
			}else{
				Debug.LogError(
					"Player 1: not enough \"Won Rounds\" Images not found! " +
					"Expected:" + targetNumberOfImages + " / Found: " + this.player1GUI.wonRoundsImages.Length +
					"\nMake sure you have set the images correctly in the Editor"
				);
			}

			if(
				this.player2GUI != null && 
				this.player2GUI.wonRoundsImages != null && 
				this.player2GUI.wonRoundsImages.Length >= targetNumberOfImages
			){
				for (int i = 0; i < targetNumberOfImages; ++i){
					this.player2GUI.wonRoundsImages[i].enabled = true;
					this.player2GUI.wonRoundsImages[i].sprite = this.wonRounds.NotFinishedRounds;
				}
					
				for (int i = targetNumberOfImages; i < this.player2GUI.wonRoundsImages.Length; ++i){
					this.player2GUI.wonRoundsImages[i].enabled = false;
				}
			}else{
				Debug.LogError(
					"Player 2: not enough \"Won Rounds\" Images not found! " +
					"Expected:" + targetNumberOfImages + " / Found: " + this.player2GUI.wonRoundsImages.Length +
					"\nMake sure you have set the images correctly in the Editor"
				);
			}
		}
		
		// Set the character names
		if (this.player1GUI != null && this.player1GUI.name != null){
			this.player1GUI.name.text = player1.characterName;
		}

		if (this.player2GUI != null && this.player2GUI.name != null){
			this.player2GUI.name.text = player2.characterName;
		}

		// Set the character portraits
		if (this.player1GUI != null && this.player1GUI.portrait != null){
			if (player1.profilePictureSmall != null){
				this.player1GUI.portrait.gameObject.SetActive(true);
				this.player1GUI.portrait.sprite = Sprite.Create(
					player1.profilePictureSmall,
					new Rect(0f, 0f, player1.profilePictureSmall.width, player1.profilePictureSmall.height),
					new Vector2(0.5f * player1.profilePictureSmall.width, 0.5f * player1.profilePictureSmall.height)
				);
			}else{
				this.player1GUI.portrait.gameObject.SetActive(false);
			}
		}
		
		if (this.player2GUI != null && this.player2GUI.portrait != null){
			if (player2.profilePictureSmall != null){
				this.player2GUI.portrait.gameObject.SetActive(true);
				this.player2GUI.portrait.sprite = Sprite.Create(
					player2.profilePictureSmall,
					new Rect(0f, 0f, player2.profilePictureSmall.width, player2.profilePictureSmall.height),
					new Vector2(0.5f * player2.profilePictureSmall.width, 0.5f * player2.profilePictureSmall.height)
				);
			}else{
				this.player2GUI.portrait.gameObject.SetActive(false);
			}
		}

		// If we want to use a Timer, set the default value for the timer
		if (this.timer != null){
			if (UFE.config.roundOptions.hasTimer){
				this.timer.gameObject.SetActive(true);
				this.timer.text = UFE.config.roundOptions._timer.ToString().Replace("Infinity", "∞");
			}else{
				this.timer.gameObject.SetActive(false);
			}
		}

		// Set the max and min values for the Life Bars and the Gauge Meters
		if (this.player1GUI != null && this.player1GUI.lifeBar != null){
			this.player1GUI.lifeBar.fillAmount = this.player1.targetLife / this.player1.totalLife;
		}
		
		if (this.player2GUI != null && this.player2GUI.lifeBar != null){
			this.player2GUI.lifeBar.fillAmount = this.player2.targetLife / this.player2.totalLife;
		}
		
		if (UFE.config.gameGUI.hasGauge){
			if (this.player1GUI != null && this.player1GUI.gaugeMeter != null){
				this.player1GUI.gaugeMeter.gameObject.SetActive(true);
                this.player1GUI.gaugeMeter.fillAmount = (float)UFE.config.player1Character.currentGaugePoints / UFE.config.player1Character.maxGaugePoints;
			}
			
			if (this.player2 != null && this.player2GUI.gaugeMeter != null){
				this.player2GUI.gaugeMeter.gameObject.SetActive(true);
                this.player2GUI.gaugeMeter.fillAmount = (float)UFE.config.player2Character.currentGaugePoints / UFE.config.player2Character.maxGaugePoints;
			}
		}else{
			if (this.player1GUI != null && this.player1GUI.gaugeMeter != null){
				this.player1GUI.gaugeMeter.gameObject.SetActive(false);
			}
			
			if (this.player2GUI != null && this.player2GUI.gaugeMeter != null){
				this.player2GUI.gaugeMeter.gameObject.SetActive(false);
			}
		}
	}

	/// <summary>
	/// 游戏结束回调：清空名称/信息/计时器文本。
	/// </summary>
	/// <param name="winner">获胜角色。</param>
	/// <param name="loser">失败角色。</param>
	protected override void OnGameEnd (UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser){
		base.OnGameEnd (winner, loser);

		if (this.player1GUI.name != null)	this.player1GUI.name.text = string.Empty;
		if (this.player2GUI.name != null)	this.player2GUI.name.text = string.Empty;
		if (this.info != null)				this.info.text = string.Empty;
		if (this.timer != null)				this.timer.text = string.Empty;
	}


	/// <summary>
	/// 游戏暂停回调：暂停时实例化并显示暂停菜单，恢复时销毁并恢复音乐。
	/// </summary>
	/// <param name="isPaused">是否暂停。</param>
	protected override void OnGamePaused (bool isPaused){
		base.OnGamePaused(isPaused);

		if (this.pauseScreen != null){
			if (isPaused){
				this.pause = (UFEScreen) GameObject.Instantiate(this.pauseScreen);
				this.pause.transform.SetParent(UFE.canvas != null ? UFE.canvas.transform : null, false);
				this.pause.OnShow();
			}else if (this.pause != null){
				if (!this.hiding){
					UFE.PlayMusic(UFE.config.selectedStage.music);
				}

				this.pause.OnHide();
				GameObject.Destroy(this.pause.gameObject);
			}
		}
	}

	/// <summary>
	/// 新提示回调：将提示文本显示到对应玩家的提示框（或中央提示框），并播放对应的播报员/音效。
	/// <para>中央提示根据消息类型（回合/最终回合/挑战开始/开战/K.O.）设置不同的显示时长。</para>
	/// </summary>
	/// <param name="msg">提示消息文本。</param>
	/// <param name="player">所属角色（可为 null 表示中央提示）。</param>
	protected override void OnNewAlert (string msg, UFE3D.CharacterInfo player){
		base.OnNewAlert (msg, player);


		// You can use this to have your own custom events when a new text alert is fired from the engine
        if (player != null) {
		    if (player.playerNum == 1){
			    ControlsScript controlsScript = UFE.GetControlsScript(1);
			    string processedMessage = this.ProcessMessage(msg, controlsScript);

			    if (this.player1GUI != null && this.player1GUI.alert != null && this.player1GUI.alert.text != null){
				    this.player1GUI.alert.text.text = processedMessage;

				    if(
					    msg != UFE.config.selectedLanguage.combo ||
					    controlsScript.opControlsScript.comboHits == 2 || 
					    UFE.config.comboOptions.comboDisplayMode == ComboDisplayMode.ShowAfterComboExecution
				    ){
					    this.player1GUI.alert.text.rectTransform.anchoredPosition = this.player1GUI.alert.initialPosition;
				    }
				    this.player1AlertTimer = 2f;
			    }
		    }else {
			    ControlsScript controlsScript = UFE.GetPlayer2ControlsScript();
			    string processedMessage = this.ProcessMessage(msg, controlsScript);

                if (this.player2GUI != null && this.player2GUI.alert != null && this.player2GUI.alert.text != null) {
                    this.player2GUI.alert.text.text = processedMessage;

                    if (
                        msg != UFE.config.selectedLanguage.combo ||
                        controlsScript.opControlsScript.comboHits == 2 ||
                        UFE.config.comboOptions.comboDisplayMode == ComboDisplayMode.ShowAfterComboExecution
                    ) {
                        this.player2GUI.alert.text.rectTransform.anchoredPosition = this.player2GUI.alert.initialPosition;
                    }
                    this.player2AlertTimer = 2f;
                }
			}

        }else{
			string processedMessage = this.ProcessMessage(msg, null);

			if (this.mainAlert != null && this.mainAlert.text != null){
				this.mainAlert.text.text = processedMessage;

				if (msg == UFE.config.selectedLanguage.round || msg == UFE.config.selectedLanguage.finalRound){
                    this.mainAlertTimer = 2f;
                } else if (msg == UFE.config.selectedLanguage.challengeBegins) {
                    this.mainAlertTimer = 2f;
				} else if (msg == UFE.config.selectedLanguage.fight){
					this.mainAlertTimer = 1f;
				} else if (msg == UFE.config.selectedLanguage.ko){
					this.mainAlertTimer = 2f;
				} else{
					this.mainAlertTimer = 60f;
				}
			}
		}
	}

	/// <summary>
	/// 回合开始回调：清空双方提示、显示"第X回合/最终回合/挑战开始"文字并播放对应播报员音效，
	/// 网络对战下显示本地玩家指示箭头。
	/// </summary>
	/// <param name="roundNumber">回合编号。</param>
	protected override void OnRoundBegin(int roundNumber){
		base.OnRoundBegin(roundNumber);

		if (this.player1GUI != null && this.player1GUI.alert != null && this.player1GUI.alert.text != null){
			this.player1GUI.alert.text.text = string.Empty;
		}
		
		if (this.player2GUI != null && this.player2GUI.alert != null && this.player2GUI.alert.text != null){
			this.player2GUI.alert.text.text = string.Empty;
		}

        if (UFE.gameMode == GameMode.ChallengeMode) {
            this.OnNewAlert(UFE.config.selectedLanguage.challengeBegins, null);

        } else if (roundNumber < UFE.config.roundOptions.totalRounds) {
			this.OnNewAlert(UFE.config.selectedLanguage.round, null);

			if (this.announcer != null && !this.muteAnnouncer){
				if (roundNumber == 1) UFE.PlaySound(this.announcer.round1);
				if (roundNumber == 2) UFE.PlaySound(this.announcer.round2);
				if (roundNumber == 3) UFE.PlaySound(this.announcer.round3);
				if (roundNumber > 3) UFE.PlaySound(this.announcer.otherRounds);
			}
			
		}else{
			this.OnNewAlert(UFE.config.selectedLanguage.finalRound, null);

			if (this.announcer != null && !this.muteAnnouncer){
				UFE.PlaySound(this.announcer.finalRound);
			}

        // If network game, point which character the local player is
        if ((UFE.gameMode == GameMode.NetworkGame || UFE.config.debugOptions.emulateNetwork)
            && networkPlayerPointer != null) {
            int localPlayer = 1;
            if (UFE.isConnected) localPlayer = UFE.localPlayerController.player;

            GameObject pointer = new GameObject("Local Pointer");
            pointer.transform.SetParent(UFE.GetControlsScript(localPlayer).transform);
            pointer.transform.localPosition = new Vector3(0, 7, 0);
            SpriteRenderer spriteRenderer = pointer.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = networkPlayerPointer;
            Destroy(pointer, pointerTimer);
        }
		}

        // If network game, point which character the local player is
        if ((UFE.gameMode == GameMode.NetworkGame || UFE.config.debugOptions.emulateNetwork)
            && networkPlayerPointer != null) {
            int localPlayer = 1;
            if (UFE.isConnected) localPlayer = UFE.localPlayerController.player;

            GameObject pointer = new GameObject("Local Pointer");
            pointer.transform.SetParent(UFE.GetControlsScript(localPlayer).transform);
            pointer.transform.localPosition = new Vector3(0, 7, 0);
            SpriteRenderer spriteRenderer = pointer.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = networkPlayerPointer;
            Destroy(pointer, pointerTimer);
        }
	}

	/// <summary>
	/// 回合结束回调：更新双方回合标记贴图（胜/负/未决），播放"玩家获胜/完美胜利"播报员音效，
	/// 显示"完美/胜利/挑战成功"文字提示并切换胜利音乐。
	/// </summary>
	/// <param name="winner">获胜角色。</param>
	/// <param name="loser">失败角色。</param>
	protected override void OnRoundEnd (UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser){
		base.OnRoundEnd (winner, loser);

		// Find out who is the winner and who is the loser...
		int winnerPlayer = winner == this.player1.character ? 1 : 2;
		int loserPlayer = loser == this.player1.character ? 1 : 2;
		PlayerGUI winnerGUI = winnerPlayer == 1 ? this.player1GUI : this.player2GUI;
		PlayerGUI loserGUI = loserPlayer == 1 ? this.player1GUI : this.player2GUI;
		ControlsScript winnerControlsScript = UFE.GetControlsScript(winnerPlayer);

		// Then update the "Won Rounds" sprites...
		if (this.wonRounds.NotFinishedRounds == null){
			Debug.LogError("\"Not Finished Rounds\" Sprite not found! Make sure you have set the sprite correctly in the Editor");
		}else if (this.wonRounds.WonRounds == null){
			Debug.LogError("\"Won Rounds\" Sprite not found! Make sure you have set the sprite correctly in the Editor");
		}else if (this.wonRounds.LostRounds == null && this.wonRounds.VisibleImages == DefaultBattleGUI.VisibleImages.AllRounds){
			Debug.LogError("\"Lost Rounds\" Sprite not found! If you want to display Lost Rounds, make sure you have set the sprite correctly in the Editor");
		}else{
			// To calculate the target number of images, check if the "Lost Rounds" Sprite is defined or not
			int targetNumberOfImages = this.wonRounds.GetNumberOfRoundsImages();

			if (this.wonRounds.VisibleImages == DefaultBattleGUI.VisibleImages.AllRounds){
				// If the "Lost Rounds" sprite is defined, that means that we must display all won and lost rounds...
				if(
					winnerGUI != null && 
					winnerGUI.wonRoundsImages != null && 
					winnerGUI.wonRoundsImages.Length >= targetNumberOfImages
				){
					winnerGUI.wonRoundsImages[UFE.config.currentRound - 1].sprite = this.wonRounds.WonRounds;
				}else{
					Debug.LogError(
						"Player " + winnerPlayer + ": not enough \"Won Rounds\" Images not found! " +
						"Expected:" + targetNumberOfImages + " / Found: " + winnerGUI.wonRoundsImages.Length +
						"\nMake sure you have set the images correctly in the Editor"
					);
				}

				if(
					loserGUI != null && 
					loserGUI.wonRoundsImages != null && 
					loserGUI.wonRoundsImages.Length >= targetNumberOfImages
				){
					loserGUI.wonRoundsImages[UFE.config.currentRound - 1].sprite = this.wonRounds.LostRounds;
				}else{
					Debug.LogError(
						"Player " + winnerPlayer + ": not enough \"Won Rounds\" Images not found! " +
						"Expected:" + targetNumberOfImages + " / Found: " + winnerGUI.wonRoundsImages.Length +
						"\nMake sure you have set the images correctly in the Editor"
					);
				}
			}else{
				// If the "Lost Rounds" sprite is not defined, that means that we must only display won rounds...
				if(
					winnerGUI != null && 
					winnerGUI.wonRoundsImages != null && 
					winnerGUI.wonRoundsImages.Length >= winnerControlsScript.roundsWon
				){
					winnerGUI.wonRoundsImages[winnerControlsScript.roundsWon - 1].sprite = this.wonRounds.WonRounds;
				}else if (UFE.gameMode != GameMode.ChallengeMode) {
					Debug.LogError(
						"Player " + winnerPlayer + ": not enough \"Won Rounds\" Images not found! " +
						"Expected:" + targetNumberOfImages + " / Found: " + winnerGUI.wonRoundsImages.Length +
						"\nMake sure you have set the images correctly in the Editor"
					);
				}
			}
		}

		if (this.announcer != null && !this.muteAnnouncer){
			// Check if it was the last round
			if (winnerControlsScript.roundsWon > Mathf.Ceil(UFE.config.roundOptions.totalRounds/2)){
				if (winnerPlayer == 1) {
					UFE.PlaySound(this.announcer.player1Wins);
				}else{
					UFE.PlaySound(this.announcer.player2Wins);
				}
			}

			// Finally, check if we should play any AudioClip
			if (winnerControlsScript.myInfo.currentLifePoints == winnerControlsScript.myInfo.lifePoints){
				UFE.PlaySound(this.announcer.perfect);
			}
		}

		if (winnerControlsScript.myInfo.currentLifePoints == winnerControlsScript.myInfo.lifePoints){
			this.OnNewAlert(this.SetStringValues(UFE.config.selectedLanguage.perfect, winnerControlsScript), null);
		}

        if (UFE.gameMode != GameMode.ChallengeMode 
            && winnerControlsScript.roundsWon > Mathf.Ceil(UFE.config.roundOptions.totalRounds / 2)) {
			this.OnNewAlert(this.SetStringValues(UFE.config.selectedLanguage.victory, winnerControlsScript), null);
			UFE.PlayMusic(UFE.config.roundOptions.victoryMusic);
		}else if (UFE.gameMode == GameMode.ChallengeMode) {
            this.OnNewAlert(this.SetStringValues(UFE.config.selectedLanguage.challengeEnds, winnerControlsScript), null);
            UFE.PlayMusic(UFE.config.roundOptions.victoryMusic);
        }
	}

	/// <summary>
	/// 计时器更新回调：更新计时器文本（"Infinity" 显示为 ∞）。
	/// </summary>
	/// <param name="time">剩余时间。</param>
	protected override void OnTimer (FPLibrary.Fix64 time){
		base.OnTimer (time);
		if (this.timer != null) this.timer.text = Mathf.Round((float)time).ToString().Replace("Infinity", "∞");
	}

	/// <summary>
	/// 时间到回调：显示"时间到"提示并播放播报员音效。
	/// </summary>
	protected override void OnTimeOver(){
		base.OnTimeOver();
		this.OnNewAlert(this.SetStringValues(UFE.config.selectedLanguage.timeOver, null), null);

		if (this.announcer != null && !this.muteAnnouncer){
			UFE.PlaySound(this.announcer.timeOver);
		}
	}

	/// <summary>
	/// 输入更新回调：训练模式下显示玩家按键图标（每帧图标行按时间轴排列，最多保留 11 行）。
	/// </summary>
	/// <param name="inputReferences">输入引用列表。</param>
	/// <param name="player">玩家编号。</param>
	protected override void OnInput (InputReferences[] inputReferences, int player){
		base.OnInput (inputReferences, player);

		// Fires whenever a player presses a button
		if(
			this.isRunning
			&& inputReferences != null
			&& inputReferences.Length > 0
            && UFE.gameMode == GameMode.TrainingRoom
            && UFE.config.trainingModeOptions.inputInfo
		){
			List<Sprite> activeIconList = new List<Sprite>();
			foreach(InputReferences inputRef in inputReferences){
				if (inputRef != null && inputRef.activeIcon != null){
					Sprite sprite = Sprite.Create(
						inputRef.activeIcon,
						new Rect(0f, 0f, inputRef.activeIcon.width, inputRef.activeIcon.height),
						new Vector2(0.5f * inputRef.activeIcon.width, 0.5f * inputRef.activeIcon.height)
					);
					
					activeIconList.Add(sprite);
				}
			}


			List<List<Image>> playerButtonPresses = null;
			List<InputReferences[]> playerInputReferences = null;

			if (player == 1){
				playerButtonPresses = this.player1ButtonPresses;
				playerInputReferences = player1InputReferences;
			}else if (player == 2){
				playerButtonPresses = this.player2ButtonPresses;
				playerInputReferences = player2InputReferences;
			}

			// If we have at least one icon, show those icons
			if (activeIconList.Count > 0){
				List<Image> images = new List<Image>();

				foreach (Sprite sprite in activeIconList){
					GameObject go = new GameObject("Player " + player + " - Button Press");

                    go.transform.parent = UFE.canvas != null ? UFE.canvas.transform : null;
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;

					Image image = go.AddComponent<Image>();
					image.sprite = sprite;
					images.Add(image);
				}

				playerButtonPresses.Add(images);
			}

			// If we have too many lines, remove the exceeding lines
			while (playerButtonPresses.Count >= 11){
				foreach(Image image in playerButtonPresses[0]){
					if (image != null){
						GameObject.Destroy(image.gameObject);
					}
				}

				playerButtonPresses.RemoveAt(0);
			}

			playerInputReferences.Add(inputReferences);
			while (playerInputReferences.Count >= 11){
				playerInputReferences.RemoveAt(0);
			}


			for(int i = 0; i < playerButtonPresses.Count; ++i){
				int distance = 0;

				foreach(Image image in playerButtonPresses[i]){
					if (image != null && image.rectTransform){
						float x = player == 1 ? 0f : 1f;
						float y = Mathf.Lerp(0.8f, 0.05f, (float)(i) / 11f);

						image.rectTransform.anchorMin = new Vector2(x, y);
						image.rectTransform.anchorMax = image.rectTransform.anchorMin;
						image.rectTransform.anchoredPosition = Vector2.zero;
						image.rectTransform.offsetMax = Vector2.zero;
						image.rectTransform.offsetMin = Vector2.zero;
						image.rectTransform.sizeDelta = new Vector2(image.preferredWidth * 200, image.preferredHeight * 200);

						if (player == 1){
							image.rectTransform.pivot = new Vector2(0f, 0.5f);
							image.rectTransform.anchoredPosition = new Vector2(image.rectTransform.sizeDelta.x * distance, 0f);
						}else{
							image.rectTransform.pivot = new Vector2(1f, 0.5f);
							image.rectTransform.anchoredPosition = new Vector2(-image.rectTransform.sizeDelta.x * distance, 0f);
						}

						++distance;
					}
				}
			}
		}
	}
	#endregion
	/*
	// DEBUG INFORMATION
	public virtual void LateUpdate(){
		if (this.mainAlert != null && this.mainAlert.text != null){
			this.mainAlert.text.text = "TimeScale: " + Time.timeScale;
		}
	}
	*/
}
