#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEditor;
using FPLibrary;
using UFE3D;

/// <summary>
/// 动画映射录制器（AnimationRecorder，编辑器专用）。
/// <para>用途：逐帧录制角色动画期间各判定盒（HitBox）的位置与位移，生成 AnimationMap 数据（动画映射），</para>
/// <para>供运行时以逐帧判定盒映射方式驱动命中检测（useAnimationMaps 模式）。</para>
/// <para>支持基础动作与必杀技/演出招式的录制，并可将结果保存回资源（StanceInfo 或 CharacterInfo）。</para>
/// </summary>
[System.Serializable]
public class AnimationRecorder : MonoBehaviour {
    
	/// <summary>要录制的角色信息。</summary>
    [SerializeField]
    public UFE3D.CharacterInfo characterInfo;
	/// <summary>是否从 Resources 加载姿态（true 用 stanceResourcePath，false 用角色内嵌 moves）。</summary>
    public bool searchResource = false;
	/// <summary>是否烘焙动画速度值到映射数据。</summary>
    public bool bakeSpeedValues = false;
	/// <summary>是否烘焙游戏速度到固定帧率。</summary>
    public bool bakeGameSpeed = false;

	/// <summary>判定盒脚本引用。</summary>
    private HitBoxesScript hitBoxesScript;
	/// <summary>Mecanim 动画控制器引用。</summary>
    private MecanimControl mecanimControl;
	/// <summary>Legacy 动画控制器引用。</summary>
    private LegacyControl legacyControl;
	/// <summary>已加载的招式集合列表。</summary>
    private List<MoveSetData> loadedMoveSets = new List<MoveSetData>();

	/// <summary>Mecanim Animator 组件。</summary>
    private Animator mAnimator;
	/// <summary>Legacy Animation 组件。</summary>
    private Animation lAnimator;
	/// <summary>默认动画片段。</summary>
    private AnimationClip defaultClip;
	/// <summary>实例化的角色模型。</summary>
    private GameObject character;
	/// <summary>当前录制中的基础动作。</summary>
    private BasicMoveInfo currentBasicMove;
	/// <summary>当前录制中的招式。</summary>
    private MoveInfo currentMove;
	/// <summary>当前姿态序号。</summary>
    private int currentStanceNum;
	/// <summary>当前片段序号（基础动作第几个片段）。</summary>
    private int currentClipNum;
	/// <summary>当前招式序号。</summary>
    private int currentMoveNum;
	/// <summary>当前录制帧。</summary>
    private int currentFrame;
	/// <summary>总录制帧数。</summary>
    private int totalFrames;
	/// <summary>起始位置。</summary>
    private Vector3 startingPosition;
	/// <summary>是否正在录制。</summary>
    private bool recording = false;

	/// <summary>
	/// 唤醒：实例化角色模型、初始化动画控制器（Legacy/Mecanim）与判定盒脚本、设置帧率。
	/// </summary>
    void Awake()
    {
        if (characterInfo.characterPrefabStorage == StorageMode.Legacy) {
            character = Instantiate(characterInfo.characterPrefab);
        } else {
            character = GameObject.Instantiate(Resources.Load<GameObject>(characterInfo.prefabResourcePath));
        }
        character.transform.position = new Vector3(0, 0, 0);

        if (searchResource)
        {
            foreach (string path in characterInfo.stanceResourcePath)
            {
                loadedMoveSets.Add(Resources.Load<StanceInfo>(path).ConvertData());
            }
        }
        else
        {
            foreach (MoveSetData moveSetData in characterInfo.moves)
            {
                loadedMoveSets.Add(moveSetData);
            }
        }

        if (characterInfo.animationType == AnimationType.Legacy) {
            lAnimator = character.GetComponent<Animation>();
            if (lAnimator == null) lAnimator = character.AddComponent<Animation>();
            legacyControl = character.AddComponent<LegacyControl>();
            legacyControl.AddClip(loadedMoveSets[0].basicMoves.idle.animMap[0].clip, "default");
            legacyControl.overrideAnimatorUpdate = true;
        } else {
            mAnimator = character.GetComponent<Animator>();
            if (mAnimator == null) mAnimator = character.AddComponent<Animator>();
            mecanimControl = character.AddComponent<MecanimControl>();
            mecanimControl.overrideAnimatorUpdate = true;
            mAnimator.applyRootMotion = false;
            mAnimator.avatar = characterInfo.avatar;
        }
        hitBoxesScript = character.GetComponent<HitBoxesScript>();
        hitBoxesScript.UpdateRenderer();

        UFE.fixedDeltaTime = 1 / (Fix64)UFE.fps;
        UFE.timeScale = bakeGameSpeed? UFE.config._gameSpeed : 1;
    }

	/// <summary>
	/// 固定帧更新：录制中推进动画并映射本帧判定盒位置。
	/// </summary>
    void FixedUpdate() {
        if (recording)
        {
            if (characterInfo.animationType == AnimationType.Legacy) {
                legacyControl.DoFixedUpdate();
            }
            else {
                mecanimControl.DoFixedUpdate();
            }
            MapHitBoxes();
        }
    }

	/// <summary>
	/// 编辑器 GUI：提供姿态选择与"录制基础动作/录制必杀技"按钮。
	/// </summary>
    private void OnGUI() {

        GUI.Box(new Rect(10, 10, 220, 180), "Animation Map Recorder");
        GUI.BeginGroup(new Rect(20, 30, 200, 180));
        {
            GUILayout.Label("Selected Move Set: " + (currentStanceNum + 1));

            if (recording)
            {
                if (GUILayout.Button("Stop Recording")) recording = false;
            }
            else
            {
                string[] selStrings = new string[loadedMoveSets.Count];
                for (int i = 0; i < loadedMoveSets.Count; i++)
                {
                    selStrings[i] = "Stance " + (i + 1);
                }
                currentStanceNum = GUILayout.SelectionGrid(currentStanceNum, selStrings, 3);

                if (GUILayout.Button("Record Basic Moves"))
                {
                    currentBasicMove = loadedMoveSets[currentStanceNum].basicMoves.idle;
                    currentMove = null;
                    currentClipNum = 0;
                    currentFrame = 0;
                    totalFrames = 0;
                    recording = true;
                }

                if (GUILayout.Button("Record Special Moves"))
                {
                    currentBasicMove = null;
                    currentMove = loadedMoveSets[currentStanceNum].attackMoves[0];
                    currentMoveNum = 0;
                    currentFrame = 0;
                    totalFrames = 0;
                    recording = true;
                }
            }

        }
        GUI.EndGroup();
    }

	/// <summary>
	/// 映射判定盒（录制主循环）：按当前录制目标（招式/基础动作）逐帧记录，完成后自动切换到下一目标
	/// （招式按列表顺序、基础动作按固定顺序链），全部完成后保存映射数据到资源。
	/// </summary>
    public void MapHitBoxes() {
        MoveSetData moveSetData = loadedMoveSets[currentStanceNum];

        bool finished = false;
        if (currentMove != null)
        {
            currentMove = MapSpecialMove(currentMove, ref finished);

            if (finished)
            {
                if (moveSetData.cinematicIntro != null && currentMove.name == moveSetData.cinematicIntro.name)
                {
                    if (moveSetData.cinematicOutro != null)
                    {
                        currentMove = MapSpecialMove(moveSetData.cinematicOutro, ref finished);
                    }
                    else
                    {
                        recording = false;
                    }
                }
                else if (moveSetData.cinematicOutro != null && currentMove.name == moveSetData.cinematicOutro.name)
                {
                    recording = false;
                }
                else
                {

                    moveSetData.attackMoves[currentMoveNum] = currentMove;
                    currentMoveNum++;
                    if (currentMoveNum == moveSetData.attackMoves.Length)
                    {
                        if (moveSetData.cinematicIntro == null && moveSetData.cinematicOutro == null)
                        {
                            recording = false;
                        }
                        else if (moveSetData.cinematicIntro != null)
                        {
                            currentMove = MapSpecialMove(moveSetData.cinematicIntro, ref finished);
                        }
                        else
                        {
                            currentMove = MapSpecialMove(moveSetData.cinematicOutro, ref finished);
                        }
                    }
                    else
                    {
                        currentMove = moveSetData.attackMoves[currentMoveNum];
                    }
                }
            }

        }
        else
        {
            currentBasicMove = MapBasicMove(currentBasicMove, ref finished);
            if (finished)
            {
                if (currentBasicMove == moveSetData.basicMoves.idle)
                {
                    currentBasicMove = moveSetData.basicMoves.moveForward;
                }
                else if (currentBasicMove == moveSetData.basicMoves.moveForward)
                {
                    currentBasicMove = moveSetData.basicMoves.moveBack;
                }
                else if (currentBasicMove == moveSetData.basicMoves.moveBack)
                {
                    currentBasicMove = moveSetData.basicMoves.crouching;
                }
                else if (currentBasicMove == moveSetData.basicMoves.crouching)
                {
                    currentBasicMove = moveSetData.basicMoves.takeOff;

                }
                else if (currentBasicMove == moveSetData.basicMoves.takeOff)
                {
                    currentBasicMove = moveSetData.basicMoves.jumpStraight;
                }
                else if (currentBasicMove == moveSetData.basicMoves.jumpStraight)
                {
                    currentBasicMove = moveSetData.basicMoves.jumpBack;
                }
                else if (currentBasicMove == moveSetData.basicMoves.jumpBack)
                {
                    currentBasicMove = moveSetData.basicMoves.jumpForward;
                }
                else if (currentBasicMove == moveSetData.basicMoves.jumpForward)
                {
                    currentBasicMove = moveSetData.basicMoves.fallStraight;
                }
                else if (currentBasicMove == moveSetData.basicMoves.fallStraight)
                {
                    currentBasicMove = moveSetData.basicMoves.fallBack;
                }
                else if (currentBasicMove == moveSetData.basicMoves.fallBack)
                {
                    currentBasicMove = moveSetData.basicMoves.fallForward;
                }
                else if (currentBasicMove == moveSetData.basicMoves.fallForward)
                {
                    currentBasicMove = moveSetData.basicMoves.landing;
                }
                else if (currentBasicMove == moveSetData.basicMoves.landing)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingHighPose;

                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingHighPose)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingHighHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingHighHit)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingLowHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingLowHit)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingCrouchingPose;
                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingCrouchingPose)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingCrouchingHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingCrouchingHit)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingAirPose;
                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingAirPose)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingAirHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingAirHit)
                {
                    currentBasicMove = moveSetData.basicMoves.parryHigh;

                }
                else if (currentBasicMove == moveSetData.basicMoves.parryHigh)
                {
                    currentBasicMove = moveSetData.basicMoves.parryLow;
                }
                else if (currentBasicMove == moveSetData.basicMoves.parryLow)
                {
                    currentBasicMove = moveSetData.basicMoves.parryCrouching;
                }
                else if (currentBasicMove == moveSetData.basicMoves.parryCrouching)
                {
                    currentBasicMove = moveSetData.basicMoves.parryAir;
                }
                else if (currentBasicMove == moveSetData.basicMoves.parryAir)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitHigh;

                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitHigh)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitLow;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitLow)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitCrouching;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitCrouching)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitAir;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitAir)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitKnockBack;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitKnockBack)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitHighKnockdown;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitHighKnockdown)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitMidKnockdown;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitMidKnockdown)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitSweep;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitSweep)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitCrumple;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitCrumple)
                {
                    currentBasicMove = moveSetData.basicMoves.fallDown;
                }
                else if (currentBasicMove == moveSetData.basicMoves.fallDown)
                {
                    currentBasicMove = moveSetData.basicMoves.airRecovery;
                }
                else if (currentBasicMove == moveSetData.basicMoves.airRecovery)
                {
                    currentBasicMove = moveSetData.basicMoves.groundBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.groundBounce)
                {
                    currentBasicMove = moveSetData.basicMoves.standingWallBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standingWallBounce)
                {
                    currentBasicMove = moveSetData.basicMoves.standingWallBounceKnockdown;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standingWallBounceKnockdown)
                {
                    currentBasicMove = moveSetData.basicMoves.airWallBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.airWallBounce)
                {
                    currentBasicMove = moveSetData.basicMoves.fallingFromGroundBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.fallingFromGroundBounce)
                {
                    currentBasicMove = moveSetData.basicMoves.fallingFromAirHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.fallingFromAirHit)
                {
                    currentBasicMove = moveSetData.basicMoves.standUp;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUp)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromAirHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromAirHit)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromKnockBack;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromKnockBack)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromStandingHighHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromStandingHighHit)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromStandingMidHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromStandingMidHit)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromCrumple;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromCrumple)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromSweep;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromSweep)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromStandingWallBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromStandingWallBounce)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromAirWallBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromAirWallBounce)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromGroundBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromGroundBounce)
                {
                    recording = false;
                }
            }
        }

        if (!recording) {
            if (searchResource)
            {
                StanceInfo newStanceInfo = moveSetData.ConvertData();
                StanceInfo reference = Resources.Load<StanceInfo>(characterInfo.stanceResourcePath[currentStanceNum]);
                string path = AssetDatabase.GetAssetPath(reference);
                if (path == "")
                {
                    path = "Assets";
                }
                else if (Path.GetExtension(path) != "")
                {
                    path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(reference)), "");
                }
                string assetPathAndName = path + reference.name + ".asset";

                if (!AssetDatabase.Contains(newStanceInfo)) AssetDatabase.CreateAsset(newStanceInfo, assetPathAndName);
                AssetDatabase.SaveAssets();
            }
            else
            {
                characterInfo.moves[currentStanceNum] = moveSetData;
            }
            EditorUtility.SetDirty(characterInfo);
            Debug.Log("Maps Created");
        }
    }

	/// <summary>
	/// 动画映射初始化：清除旧映射、在动画控制器中注册片段并计算总帧数。
	/// </summary>
	/// <param name="animMap">动画映射数据。</param>
	/// <param name="speed">动画速度。</param>
	/// <returns>初始化后的动画映射。</returns>
    private SerializedAnimationMap AnimationSetup(SerializedAnimationMap animMap, Fix64 speed)
    {
        animMap.animationMaps = new AnimationMap[0];

        if (characterInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.RemoveAllClips();
            legacyControl.AddClip(animMap.clip, animMap.clip.name, speed, WrapMode.Clamp);
            legacyControl.Play(animMap.clip.name, 0, 0);
        }
        else
        {
            mecanimControl.SetDefaultClip(animMap.clip, animMap.clip.name, speed, WrapMode.Clamp, false);
            mecanimControl.currentAnimationData = mecanimControl.defaultAnimation;
            mecanimControl.currentAnimationData.stateName = "State1";
            mecanimControl.currentAnimationData.length = animMap.clip.length;
            mecanimControl.Play(mecanimControl.defaultAnimation);
        }

        animMap.length = animMap.clip.length;
        totalFrames = (int)FPMath.Round((animMap.length / speed) * UFE.fps);

        return animMap;
    }

	/// <summary>
	/// 录制单个必杀技/演出招式的逐帧判定盒映射，完成后标记结束。
	/// </summary>
	/// <param name="moveInfo">招式数据。</param>
	/// <param name="over">输出：是否录制完成。</param>
	/// <returns>更新后的招式数据。</returns>
    private MoveInfo MapSpecialMove(MoveInfo moveInfo, ref bool over)
    {
        bool finished = false;
        if (moveInfo.animMap.clip != null)
        {
            if (currentFrame == 0) {
                Fix64 speed = 1;
                if (bakeSpeedValues && moveInfo.fixedSpeed) {
                    speed = moveInfo._animationSpeed;
                }else if (bakeSpeedValues && !moveInfo.fixedSpeed) {
                    Debug.LogWarning("Speed keyframe is currently not supported on the Animation Recorder.");
                }
                moveInfo.animMap = AnimationSetup(moveInfo.animMap, speed);
                moveInfo.animMap.bakeSpeed = bakeSpeedValues;
            }

            AnimationMap[] animationMaps = moveInfo.animMap.animationMaps;
            moveInfo.animMap.animationMaps = MapFrame(animationMaps, moveInfo.animMap.clip, ref finished, moveInfo.applyRootMotion);

            if (finished)
            {
                Debug.Log("Saved");
                over = true;
            }
        }
        else
        {
            over = true;
        }

        EditorUtility.SetDirty(moveInfo);
        return moveInfo;
    }

	/// <summary>
	/// 录制单个基础动作的逐帧判定盒映射（含最多 6 个片段），完成后标记结束。
	/// </summary>
	/// <param name="basicMove">基础动作数据。</param>
	/// <param name="over">输出：是否录制完成。</param>
	/// <returns>更新后的基础动作数据。</returns>
    private BasicMoveInfo MapBasicMove(BasicMoveInfo basicMove, ref bool over)
    {
        if (currentClipNum > 5)
        {
            currentClipNum = 0;
            over = true;
        }
        else
        {
            bool finished = false;
            if (basicMove.animMap[currentClipNum].clip != null)
            {
                if (currentFrame == 0) {
                    Fix64 speed = bakeSpeedValues ? basicMove._animationSpeed : 1;
                    basicMove.animMap[currentClipNum] = AnimationSetup(basicMove.animMap[currentClipNum], speed);
                    basicMove.animMap[currentClipNum].bakeSpeed = bakeSpeedValues;
                }

                AnimationMap[] animationMaps = basicMove.animMap[currentClipNum].animationMaps;
                basicMove.animMap[currentClipNum].animationMaps = MapFrame(animationMaps, basicMove.animMap[currentClipNum].clip, ref finished);

                if (finished)
                {
                    Debug.Log("Saved");
                    currentClipNum++;
                }
            }
            else
            {
                currentClipNum++;
            }

            over = false;
        }

        return basicMove;
    }

	/// <summary>
	/// 映射当前帧：记录判定盒位置与位移增量到 AnimationMap，推进帧号并更新预览映射。
	/// </summary>
	/// <param name="animationMaps">已有映射列表。</param>
	/// <param name="animationClip">当前动画片段。</param>
	/// <param name="finished">输出：是否完成全部帧。</param>
	/// <param name="applyRootMotion">是否记录根骨骼位移。</param>
	/// <returns>更新后的映射数组。</returns>
    private AnimationMap[] MapFrame(AnimationMap[] animationMaps, AnimationClip animationClip, ref bool finished, bool applyRootMotion = false)
    {
        List<AnimationMap> _animationMaps = new List<AnimationMap>(animationMaps);

        Debug.Log("Mapping " + animationClip.name + " (" + currentFrame + ")");

        AnimationMap animationMap = new AnimationMap();
        animationMap.frame = currentFrame;
        animationMap.hitBoxMaps = hitBoxesScript.GetAnimationMaps();
        if (characterInfo.animationType == AnimationType.Legacy) {
            animationMap.deltaDisplacement = FPVector.ToFPVector(legacyControl.GetDeltaPosition());
        } else {
            animationMap.deltaDisplacement = FPVector.ToFPVector(mAnimator.deltaPosition);
        }

        _animationMaps.Add(animationMap);

        // preview
        hitBoxesScript.animationMaps = _animationMaps.ToArray();
        hitBoxesScript.UpdateMap(currentFrame);
        
        currentFrame++;
        if (currentFrame >= totalFrames)
        {
            currentFrame = 0;
            finished = true;
        }

        return _animationMaps.ToArray();
    }
}
#endif
