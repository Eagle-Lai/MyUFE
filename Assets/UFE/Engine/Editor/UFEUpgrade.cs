using UnityEngine;
using UnityEditor;
using FPLibrary;
using UFE3D;

/// <summary>
/// UFE 版本升级工具（UFEUpgrade，编辑器专用）。
/// <para>用途：将 UFE 1.x 版本的配置资产（全局/角色/招式）批量升级到 2.0 格式——</para>
/// <para>把旧的 float/Vector 字段值同步到新的定点数（Fix64/FPVector）字段（_前缀），并更新版本号。</para>
/// <para>提供菜单入口：更新全部定义/更新输入定义/更新变量定义。</para>
/// </summary>
public class UFEUpgrade : EditorWindow {

    /// <summary>选中的全局配置资产。</summary>
    private static GlobalInfo globalInfo;
    /// <summary>选中的角色资产。</summary>
    private static UFE3D.CharacterInfo characterInfo;
    /// <summary>选中的招式资产。</summary>
    private static MoveInfo moveInfo;

    /// <summary>
    /// 升级击倒选项：将 float 字段同步到定点数字段。
    /// </summary>
    /// <param name="knockDown">击倒选项。</param>
    /// <returns>升级后的击倒选项。</returns>
    private static SubKnockdownOptions UpgradeKnockdownOptions(SubKnockdownOptions knockDown) {
        knockDown._knockedOutTime = knockDown.knockedOutTime;
        knockDown._standUpTime = knockDown.standUpTime;
        knockDown._predefinedPushForce = FPVector.ToFPVector(knockDown.predefinedPushForce);
        return knockDown;
    }
    /// <summary>
    /// 升级命中类型选项：将 float 字段同步到定点数字段。
    /// </summary>
    /// <param name="hitType">命中类型选项。</param>
    /// <returns>升级后的命中类型选项。</returns>
    private static HitTypeOptions UpgradeHitOptions(HitTypeOptions hitType) {
        hitType._freezingTime = hitType.freezingTime;
        hitType._animationSpeed = hitType.animationSpeed;
        hitType._hitStop = hitType.hitStop;
        hitType._shakeDensity = hitType.shakeDensity;
        return hitType;
    }

    /// <summary>
    /// 获取当前选中的 UFE 资产（全局/角色/招式任一）。
    /// </summary>
    /// <returns>有有效选中资产返回 true，否则弹出提示并返回 false。</returns>
    private static bool RetrieveSelection()
    {
        globalInfo = null;
        characterInfo = null;
        moveInfo = null;
        UnityEngine.Object[] selection = Selection.GetFiltered(typeof(GlobalInfo), SelectionMode.Assets);
        if (selection.Length > 0)
        {
            if (selection[0] == null) return false;
            globalInfo = (GlobalInfo)selection[0];

        }
        selection = Selection.GetFiltered(typeof(UFE3D.CharacterInfo), SelectionMode.Assets);
        if (selection.Length > 0)
        {
            if (selection[0] == null) return false;
            characterInfo = (UFE3D.CharacterInfo)selection[0];

        }
        selection = Selection.GetFiltered(typeof(MoveInfo), SelectionMode.Assets);
        if (selection.Length > 0)
        {
            if (selection[0] == null) return false;
            moveInfo = (MoveInfo)selection[0];

        }

        if (globalInfo == null && characterInfo == null && moveInfo == null)
        {
            EditorUtility.DisplayDialog("UFE Upgrade", "Must be a valid UFE file", "Ok");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 更新全部定义（输入定义 + 变量定义，菜单入口）。
    /// </summary>
    [MenuItem("Assets/UFE 2.0/Update All Definitions")]
    public static void UpdateAll()
    {
        UpdateInputs();
        UpdateVariables();
    }

    /// <summary>
    /// 更新输入定义（菜单入口）：将选中的全局/角色/招式下所有招式的旧输入字段同步到 defaultInputs。
    /// </summary>
    [MenuItem("Assets/UFE 2.0/Update Input Definitions")]
    public static void UpdateInputs()
    {
        if (!RetrieveSelection()) return;

        bool dontAskAgain = false;
        if (globalInfo != null)
        {
            foreach (UFE3D.CharacterInfo character in globalInfo.characters)
            {
                if (character == null) continue;
                foreach (MoveSetData moveSet in character.moves)
                {
                    foreach (MoveInfo move in moveSet.attackMoves)
                    {
                        MoveInputUpdate(move, ref dontAskAgain);
                    }
                }
            }
        }
        else if (characterInfo != null)
        {
            foreach (MoveSetData moveSet in characterInfo.moves)
            {
                foreach (MoveInfo move in moveSet.attackMoves)
                {
                    MoveInputUpdate(move, ref dontAskAgain);
                }
            }
        }
        else if (moveInfo != null)
        {
            MoveInputUpdate(moveInfo, ref dontAskAgain);
        }
    }

    /// <summary>
    /// 升级单个招式的输入定义：将旧输入字段同步到 defaultInputs，弹窗确认覆盖。
    /// </summary>
    /// <param name="move">目标招式。</param>
    /// <param name="dontAskAgain">是否不再询问（"全部覆盖"模式）。</param>
    private static void MoveInputUpdate(MoveInfo move, ref bool dontAskAgain)
    {
        int updateConfirm = dontAskAgain ? 0 : 1;
        if (updateConfirm == 1)
            updateConfirm = EditorUtility.DisplayDialogComplex("Override Input", "Move " + move.name + " already have a default input definition. Override anyway?", "Yes", "No", "Yes to All");

        if (updateConfirm == 0 || updateConfirm == 2)
        {
            if (updateConfirm == 2) dontAskAgain = true;
            move.defaultInputs.chargeMove = move.chargeMove;
            move.defaultInputs._chargeTiming = move._chargeTiming;
            move.defaultInputs.allowInputLeniency = move.allowInputLeniency;
            move.defaultInputs.allowNegativeEdge = move.allowNegativeEdge;
            move.defaultInputs.leniencyBuffer = move.leniencyBuffer;
            move.defaultInputs.onReleaseExecution = move.onReleaseExecution;
            move.defaultInputs.requireButtonPress = move.requireButtonPress;
            move.defaultInputs.onPressExecution = move.onPressExecution;
            move.defaultInputs.buttonSequence = (ButtonPress[])move.buttonSequence.Clone();
            move.defaultInputs.buttonExecution = (ButtonPress[])move.buttonExecution.Clone();

            EditorUtility.SetDirty(move);
            Debug.Log("Move " + move.name + " updated.");
        }
    }

    /// <summary>
    /// 更新变量定义（菜单入口）：将选中的全局/角色/招式的旧 float/Vector 字段同步到定点数字段（_前缀），并校验版本号。
    /// </summary>
    [MenuItem("Assets/UFE 2.0/Update Variable Definitions")]
    public static void UpdateVariables()
    {
        if (!RetrieveSelection()) return;

        bool updateConfirm = true;
        string warningText = "This file seems to be already converted to 2.0. Converting the data again will revert your project to when you originally imported. Continue?";
        if (globalInfo != null)
        {
            if (globalInfo.version >= 2f)
            {
                updateConfirm = false;
                updateConfirm = EditorUtility.DisplayDialog("Global Asset Update", warningText, "Yes", "No");
            }

            if (updateConfirm)
            {
                GlobalUpdate(globalInfo);
            }
        }
        else if (characterInfo != null)
        {
            if (characterInfo.version >= 2f)
            {
                updateConfirm = false;
                updateConfirm = EditorUtility.DisplayDialog("Character Asset Update", warningText, "Yes", "No");
            }

            if (updateConfirm)
            {
                CharacterUpdate(characterInfo);
            }
        }
        else if (moveInfo != null)
        {
            if (moveInfo.version >= 2f)
            {
                updateConfirm = false;
                updateConfirm = EditorUtility.DisplayDialog("Move Asset Update", warningText, "Yes", "No");
            }

            if (updateConfirm)
            {
                SpecialMoveUpdate(moveInfo);
            }
        }
        // End of Update
    }

    /// <summary>
    /// 升级全局配置：将各选项（摄像机/旋转/连击/弹跳/格挡/击倒/命中/场地/反击/回合等）的 float 字段同步到定点数字段，并递归升级角色。
    /// </summary>
    /// <param name="global">全局配置。</param>
    private static void GlobalUpdate(GlobalInfo global)
    {
        global.version = 2f;
        // Camera Options
        global.cameraOptions._maxDistance = global.cameraOptions.maxDistance;
        // Character Rotation Options
        global.characterRotationOptions._rotationSpeed = global.characterRotationOptions.rotationSpeed;
        global.characterRotationOptions._mirrorBlending = global.characterRotationOptions.mirrorBlending;
        // Combo Options
        global.comboOptions._minHitStun = Mathf.RoundToInt(global.comboOptions.minHitStun);
        global.comboOptions._minDamage = global.comboOptions.minDamage;
        global.comboOptions._minPushForce = global.comboOptions.minPushForce;
        global.comboOptions._knockBackMinForce = global.comboOptions.knockBackMinForce;
        global.comboOptions._juggleWeight = global.comboOptions.juggleWeight;
        // Ground Bounce Options
        global.groundBounceOptions._minimumBounceForce = global.groundBounceOptions.minimumBounceForce;
        global.groundBounceOptions._maximumBounces = global.groundBounceOptions.maximumBounces;
        global.groundBounceOptions._shakeDensity = global.groundBounceOptions.shakeDensity;
        // Wall Bounce Options
        global.wallBounceOptions._minimumBounceForce = global.wallBounceOptions.minimumBounceForce;
        global.wallBounceOptions._maximumBounces = global.wallBounceOptions.maximumBounces;
        global.wallBounceOptions._shakeDensity = global.wallBounceOptions.shakeDensity;
        // Block Options
        global.blockOptions._parryTiming = global.blockOptions.parryTiming;
        // Knockdown Options
        global.knockDownOptions.air = UpgradeKnockdownOptions(global.knockDownOptions.air);
        global.knockDownOptions.high = UpgradeKnockdownOptions(global.knockDownOptions.high);
        global.knockDownOptions.highLow = UpgradeKnockdownOptions(global.knockDownOptions.highLow);
        global.knockDownOptions.sweep = UpgradeKnockdownOptions(global.knockDownOptions.sweep);
        global.knockDownOptions.crumple = UpgradeKnockdownOptions(global.knockDownOptions.crumple);
        global.knockDownOptions.wallbounce = UpgradeKnockdownOptions(global.knockDownOptions.wallbounce);
        // Hit Options
        global.hitOptions.weakHit = UpgradeHitOptions(global.hitOptions.weakHit);
        global.hitOptions.mediumHit = UpgradeHitOptions(global.hitOptions.mediumHit);
        global.hitOptions.heavyHit = UpgradeHitOptions(global.hitOptions.heavyHit);
        global.hitOptions.crumpleHit = UpgradeHitOptions(global.hitOptions.crumpleHit);
        global.hitOptions.customHit1 = UpgradeHitOptions(global.hitOptions.customHit1);
        global.hitOptions.customHit2 = UpgradeHitOptions(global.hitOptions.customHit2);
        global.hitOptions.customHit3 = UpgradeHitOptions(global.hitOptions.customHit3);
        // Stage Options
        foreach (StageOptions stage in global.stages)
        {
            stage._groundFriction = stage.groundFriction;
            stage._leftBoundary = stage.leftBoundary;
            stage._rightBoundary = stage.rightBoundary;
        }
        // Counter Hit Options
        global.counterHitOptions._damageIncrease = global.counterHitOptions.damageIncrease;
        global.counterHitOptions._hitStunIncrease = global.counterHitOptions.hitStunIncrease;
        // Round Options
        global.roundOptions._timer = global.roundOptions.timer;
        global.roundOptions._timerSpeed = global.roundOptions.timerSpeed;
        global.roundOptions._p1XPosition = global.roundOptions.p1XPosition;
        global.roundOptions._p2XPosition = global.roundOptions.p2XPosition;
        global.roundOptions._endGameDelay = global.roundOptions.endGameDelay;
        global.roundOptions._newRoundDelay = global.roundOptions.newRoundDelay;
        global.roundOptions._slowMoTimer = global.roundOptions.slowMoTimer;
        global.roundOptions._slowMoSpeed = global.roundOptions.slowMoSpeed;
        // Global Options
        global._gameSpeed = global.gameSpeed;
        global._preloadingTime = global.preloadingTime;
        global._gravity = global.gravity;


        // Character Update
        foreach (UFE3D.CharacterInfo character in global.characters)
        {
            if (character == null) continue;
            CharacterUpdate(character);
        }

        EditorUtility.SetDirty(global);
        Debug.Log("Global Options updated.");
    }

    /// <summary>
    /// 升级角色：同步物理/出招时间等定点数字段，并递归升级基础动作与必杀技。
    /// </summary>
    /// <param name="character">角色信息。</param>
    private static void CharacterUpdate(UFE3D.CharacterInfo character) {
        character.version = 2f;
        character._executionTiming = character.executionTiming;
        character._blendingTime = character.blendingTime;

        // Character Physics
        character.physics._moveForwardSpeed = character.physics.moveForwardSpeed;
        character.physics._moveBackSpeed = character.physics.moveBackSpeed;
        character.physics._friction = character.physics.friction;
        character.physics._minJumpForce = character.physics.minJumpForce;
        character.physics._jumpDistance = character.physics.jumpDistance;
        character.physics._weight = character.physics.weight;
        character.physics._groundCollisionMass = character.physics.groundCollisionMass;

        // Move Set
        if (character.moves != null && character.moves.Length > 0)
        {
            foreach (MoveSetData moveSetData in character.moves)
            {
                // Basic Moves
                BasicMoveUpdate(moveSetData.basicMoves.idle);
                BasicMoveUpdate(moveSetData.basicMoves.moveForward);
                BasicMoveUpdate(moveSetData.basicMoves.moveBack);
                BasicMoveUpdate(moveSetData.basicMoves.crouching);

                BasicMoveUpdate(moveSetData.basicMoves.takeOff);
                BasicMoveUpdate(moveSetData.basicMoves.jumpStraight);
                BasicMoveUpdate(moveSetData.basicMoves.jumpBack);
                BasicMoveUpdate(moveSetData.basicMoves.jumpForward);
                BasicMoveUpdate(moveSetData.basicMoves.fallStraight);
                BasicMoveUpdate(moveSetData.basicMoves.fallBack);
                BasicMoveUpdate(moveSetData.basicMoves.fallForward);
                BasicMoveUpdate(moveSetData.basicMoves.landing);

                BasicMoveUpdate(moveSetData.basicMoves.blockingHighPose);
                BasicMoveUpdate(moveSetData.basicMoves.blockingHighHit);
                BasicMoveUpdate(moveSetData.basicMoves.blockingLowHit);
                BasicMoveUpdate(moveSetData.basicMoves.blockingCrouchingPose);
                BasicMoveUpdate(moveSetData.basicMoves.blockingCrouchingHit);
                BasicMoveUpdate(moveSetData.basicMoves.blockingAirPose);
                BasicMoveUpdate(moveSetData.basicMoves.blockingAirHit);

                BasicMoveUpdate(moveSetData.basicMoves.parryHigh);
                BasicMoveUpdate(moveSetData.basicMoves.parryLow);
                BasicMoveUpdate(moveSetData.basicMoves.parryCrouching);
                BasicMoveUpdate(moveSetData.basicMoves.parryAir);

                BasicMoveUpdate(moveSetData.basicMoves.getHitHigh);
                BasicMoveUpdate(moveSetData.basicMoves.getHitLow);
                BasicMoveUpdate(moveSetData.basicMoves.getHitCrouching);
                BasicMoveUpdate(moveSetData.basicMoves.getHitAir);
                BasicMoveUpdate(moveSetData.basicMoves.getHitKnockBack);
                BasicMoveUpdate(moveSetData.basicMoves.getHitHighKnockdown);
                BasicMoveUpdate(moveSetData.basicMoves.getHitMidKnockdown);
                BasicMoveUpdate(moveSetData.basicMoves.getHitSweep);
                BasicMoveUpdate(moveSetData.basicMoves.getHitCrumple);

                BasicMoveUpdate(moveSetData.basicMoves.fallDown);
                BasicMoveUpdate(moveSetData.basicMoves.groundBounce);
                BasicMoveUpdate(moveSetData.basicMoves.airWallBounce);
                BasicMoveUpdate(moveSetData.basicMoves.fallingFromGroundBounce);
                BasicMoveUpdate(moveSetData.basicMoves.standUp);

                // Special Moves
                foreach (MoveInfo moveInfo in moveSetData.attackMoves)
                {
                    SpecialMoveUpdate(moveInfo);
                }
                SpecialMoveUpdate(moveSetData.cinematicIntro);
                SpecialMoveUpdate(moveSetData.cinematicOutro);
            }
        }

        EditorUtility.SetDirty(character);
        Debug.Log("Character " + character.characterName + " updated.");
    }

    /// <summary>
    /// 升级基础动作：将动画片段同步到动画映射（animMap）并同步定点数字段。
    /// </summary>
    /// <param name="basicMove">基础动作。</param>
    private static void BasicMoveUpdate(BasicMoveInfo basicMove) {
        if (basicMove.clip1 != null) {
            basicMove.animMap[0].clip = basicMove.clip1;
            basicMove.animMap[0].length = basicMove.clip1.length;
        }
        if (basicMove.clip2 != null) {
            basicMove.animMap[1].clip = basicMove.clip2;
            basicMove.animMap[1].length = basicMove.clip2.length;
        }
        if (basicMove.clip3 != null) {
            basicMove.animMap[2].clip = basicMove.clip3;
            basicMove.animMap[2].length = basicMove.clip3.length;
        }
        if (basicMove.clip4 != null) {
            basicMove.animMap[3].clip = basicMove.clip4;
            basicMove.animMap[3].length = basicMove.clip4.length;
        }
        if (basicMove.clip5 != null) {
            basicMove.animMap[4].clip = basicMove.clip5;
            basicMove.animMap[4].length = basicMove.clip5.length;
        }
        if (basicMove.clip6 != null) {
            basicMove.animMap[5].clip = basicMove.clip6;
            basicMove.animMap[5].length = basicMove.clip6.length;
        }

        basicMove._animationSpeed = basicMove.animationSpeed;
        basicMove._restingClipInterval = basicMove.restingClipInterval;
        basicMove._blendingIn = basicMove.blendingIn;
        basicMove._blendingOut = basicMove.blendingOut;
    }

    /// <summary>
    /// 升级必杀技/演出招式：同步能量/融合/蓄力/动画映射/命中/飞行道具/施加力/慢动作/摄像机等全部定点数字段。
    /// </summary>
    /// <param name="move">招式信息。</param>
    private static void SpecialMoveUpdate(MoveInfo move) {
        if (move == null) return;
        move.version = 2f;
        move._gaugeDPS = move.gaugeDPS;
        move._totalDrain = move.totalDrain;
        move._gaugeRequired = move.gaugeRequired;
        move._gaugeUsage = move.gaugeUsage;
        move._gaugeGainOnMiss = move.gaugeGainOnMiss;
        move._gaugeGainOnHit = move.gaugeGainOnHit;
        move._gaugeGainOnBlock = move.gaugeGainOnBlock;
        move._opGaugeGainOnBlock = move.opGaugeGainOnBlock;
        move._opGaugeGainOnParry = move.opGaugeGainOnParry;
        move._opGaugeGainOnHit = move.opGaugeGainOnHit;
        move._blendingIn = move.blendingIn;
        move._blendingOut = move.blendingOut;
        move._chargeTiming = move.chargeTiming;
        move.blockableArea._radius = move.blockableArea.radius;
        move.blockableArea._offSet = FPVector.ToFPVector(move.blockableArea.offSet);
        move._animationSpeed = move.animationSpeed;

        if (move.animationClip == null) {
            Debug.LogWarning("Move " + move.name + " has no animation attached.");
        } else {
            move.animMap.clip = move.animationClip;
            move.animMap.length = move.animationClip.length;
        }

        foreach (Projectile projectile in move.projectiles) {
            projectile._damageOnHit = projectile.damageOnHit;
            projectile._damageOnBlock = projectile.damageOnBlock;
            projectile._castingOffSet = FPVector.ToFPVector(projectile.castingOffSet);
            projectile._pushForce = FPVector.ToFPVector(projectile.pushForce);
            projectile.hurtBox._radius = projectile.hurtBox.radius;
            projectile.hurtBox._offSet = FPVector.ToFPVector(projectile.hurtBox.offSet);
            projectile.hurtBox._rect = new FPRect(projectile.hurtBox.rect);
        }

        foreach (AppliedForce aForce in move.appliedForces) {
            aForce._force = FPVector.ToFPVector(aForce.force);
        }

        foreach (Hit hit in move.hits) {
            hit._newHitBlendingIn = hit.newHitBlendingIn;
            hit._newJuggleWeight = hit.newJuggleWeight;
            hit._hitStunOnHit = hit.hitStunOnHit;
            hit._hitStunOnBlock = hit.hitStunOnBlock;
            hit._damageOnHit = hit.damageOnHit;
            hit._damageOnBlock = hit.damageOnBlock;
            hit._newMovementSpeed = hit.newMovementSpeed;
            hit._newRotationSpeed = hit.newRotationSpeed;
            hit._cameraSpeedDuration = hit.cameraSpeedDuration;
            hit._pushForce = FPVector.ToFPVector(hit.pushForce);
            hit._pushForceAir = FPVector.ToFPVector(hit.pushForceAir);
            hit._appliedForce = FPVector.ToFPVector(hit.appliedForce);
            hit._groundBouncePushForce = FPVector.ToFPVector(hit.groundBouncePushForce);
            hit._wallBouncePushForce = FPVector.ToFPVector(hit.wallBouncePushForce);
            foreach (HurtBox hurtBox in hit.hurtBoxes) {
                hurtBox._radius = hurtBox.radius;
                hurtBox._offSet = FPVector.ToFPVector(hurtBox.offSet);
                hurtBox._rect = new FPRect(hurtBox.rect);
            }
            hit.pullEnemyIn._targetDistance = hit.pullEnemyIn.targetDistance;
            hit.pullSelfIn._targetDistance = hit.pullSelfIn.targetDistance;

            if (hit.overrideHitEffects) {
                hit.hitEffects = UpgradeHitOptions(hit.hitEffects);
            }
        }

        foreach (SlowMoEffect slowMo in move.slowMoEffects) {
            slowMo._duration = slowMo.duration;
            slowMo._percentage = slowMo.percentage;
        }

        foreach (CameraMovement camMove in move.cameraMovements) {
            camMove._duration = camMove.duration;
            camMove._myAnimationSpeed = camMove.myAnimationSpeed;
            camMove._opAnimationSpeed = camMove.opAnimationSpeed;
        }

        foreach (OpponentOverride opOvr in move.opponentOverride) {
            opOvr._stunTime = opOvr.stunTime;
            opOvr._position = FPVector.ToFPVector(opOvr.position);
        }

        foreach (AnimSpeedKeyFrame animKey in move.animSpeedKeyFrame) {
            animKey._speed = animKey.speed;
        }


        EditorUtility.SetDirty(move);
        Debug.Log("Move " + move.name + " updated.");
    }
}
