using UnityEngine;
using System.Collections;
using FPLibrary;
using UFENetcode;
using UFE3D;

/// <summary>
/// 角色物理脚本（PhysicsScript）。
/// <para>用途：处理角色的移动/跳跃/重力/推挤/弹跳等全部物理模拟，负责水平与垂直方向的力计算和落地检测。</para>
/// <para>所有物理量均使用定点数（Fix64）以保证网络对战各客户端确定性一致。</para>
/// </summary>
public class PhysicsScript : MonoBehaviour {
    #region trackable definitions
	/// <summary>空中停留时间（秒，用于动画速度计算）。</summary>
    public Fix64 airTime;
	/// <summary>当前应用的重力加速度。</summary>
    public Fix64 appliedGravity;
	/// <summary>当前已使用的空中跳跃次数。</summary>
    public int currentAirJumps;
	/// <summary>是否冻结物理（停止一切物理作用）。</summary>
    public bool freeze;
	/// <summary>地面弹跳次数。</summary>
    public int groundBounceTimes;
	/// <summary>水平方向力。</summary>
    public Fix64 horizontalForce;
	/// <summary>是否正在地面弹跳。</summary>
    public bool isGroundBouncing;
	/// <summary>是否正在落地。</summary>
    public bool isLanding;
	/// <summary>是否正在起跳。</summary>
    public bool isTakingOff;
	/// <summary>是否正在墙壁弹跳。</summary>
    public bool isWallBouncing;
	/// <summary>当前移动方向（-1 左 / 1 右）。</summary>
    public Fix64 moveDirection;
	/// <summary>是否覆盖空中动画（强制使用指定动画）。</summary>
    public bool overrideAirAnimation;
	/// <summary>覆盖受击动画的基础动作（眩晕状态下使用）。</summary>
    public BasicMoveInfo overrideStunAnimation;
	/// <summary>垂直方向力（速度）。</summary>
    public Fix64 verticalForce;
	/// <summary>垂直方向总力（用于跳跃弧线计算）。</summary>
    public Fix64 verticalTotalForce;
	/// <summary>墙壁弹跳次数。</summary>
    public int wallBounceTimes;
    #endregion

	/// <summary>角色控制脚本引用。</summary>
    public ControlsScript controlScript;
	/// <summary>招式集合脚本引用。</summary>
    public MoveSetScript moveSetScript;

	/// <summary>本角色世界变换（定点数）快捷属性。</summary>
    private FPTransform worldTransform { get { return controlScript.worldTransform; } set { controlScript.worldTransform = value; } }
	/// <summary>对手世界变换（定点数）快捷属性。</summary>
    private FPTransform opWorldTransform { get { return controlScript.opControlsScript.worldTransform; } set { controlScript.opControlsScript.worldTransform = value; } }

	/// <summary>
	/// 启动：根据角色重量和全局重力初始化重力加速度。
	/// </summary>
    public void Start(){
		appliedGravity = controlScript.myInfo.physics._weight * UFE.config._gravity;
	}
	
	/// <summary>
	/// 地面移动：设置水平力并更新子状态（前进/后退）。
	/// </summary>
	/// <param name="mirror">朝向（1 右 / -1 左）。</param>
	/// <param name="direction">移动方向值。</param>
	public void Move(int mirror, Fix64 direction){
		if (!IsGrounded()) return;
		if (freeze) return;
		if (isTakingOff) return;
        if (isLanding) return;

        if (UFE.config.inputOptions.forceDigitalInput) direction = direction < 0? -1: 1;

		moveDirection = direction;

		if (mirror == 1){
			controlScript.currentSubState = SubStates.MovingForward;
			horizontalForce = controlScript.myInfo.physics._moveForwardSpeed * direction;
		}else{
			controlScript.currentSubState = SubStates.MovingBack;
			horizontalForce = controlScript.myInfo.physics._moveBackSpeed * direction;
		}
	}

	/// <summary>
	/// 使用角色默认跳跃力执行跳跃。
	/// </summary>
    public void Jump() {
        Jump(controlScript.myInfo.physics._jumpForce);
    }

	/// <summary>
	/// 使用指定跳跃力执行跳跃（含二段跳/三段跳逻辑）。
	/// </summary>
	/// <param name="jumpForce">跳跃初速度。</param>
    public void Jump(Fix64 jumpForce) {
		if (isTakingOff && currentAirJumps > 0) return;
		if (controlScript.currentMove != null) return;

		isTakingOff = false;
		isLanding = false;
		controlScript.storedMove = null;
		controlScript.potentialBlock = false;

		if (controlScript.currentState == PossibleStates.Down) return;
		if (controlScript.currentSubState == SubStates.Stunned || controlScript.currentSubState == SubStates.Blocking) return;
		if (currentAirJumps >= controlScript.myInfo.physics.multiJumps) return;
		currentAirJumps ++;
		horizontalForce = controlScript.myInfo.physics._jumpDistance * moveDirection;
		verticalForce = jumpForce;
		setVerticalData(jumpForce);
		//ApplyForces(controlScript.currentMove);
	}

	/// <summary>
	/// 角色当前是否在跳跃中（空中跳跃次数大于0）。
	/// </summary>
	/// <returns>true 表示在空中。</returns>
	public bool IsJumping() {
		return (currentAirJumps > 0);
	}
	
	/// <summary>
	/// 角色当前是否在移动。
	/// </summary>
	/// <returns>true 表示移动方向不为0。</returns>
	public bool IsMoving() {
		return (moveDirection != 0);
	}

	/// <summary>
	/// 重置落地标志。
	/// </summary>
    public void ResetLanding() {
        isLanding = false;
    }

	/// <summary>
	/// 重置水平/垂直方向力。
	/// </summary>
	/// <param name="resetX">是否重置水平力。</param>
	/// <param name="resetY">是否重置垂直力。</param>
	public void ResetForces(bool resetX, bool resetY) {
        if (resetX) {
            horizontalForce = 0;
            moveDirection = 0;
        }
		if (resetY) verticalForce = 0;
	}
	
	/// <summary>
	/// 施加推挤力（受击/击中后的位移），支持累积力与重置下落力规则。
	/// </summary>
	/// <param name="push">要施加的力。</param>
	/// <param name="mirror">朝向（决定水平力方向）。</param>
	public void AddForce(FPVector push, int mirror) {
		push.x *= mirror;
        isGroundBouncing = false;
        isWallBouncing = false;
		if (!controlScript.myInfo.physics.cumulativeForce) {
			horizontalForce = 0;
			verticalForce = 0;
		}
		if (verticalForce < 0 && push.y > 0 && UFE.config.comboOptions.resetFallingForceOnHit) verticalForce = 0;
		horizontalForce += push.x;
		verticalForce += push.y;
		setVerticalData(verticalForce);
	}
	
	/// <summary>
	/// 根据施加力计算空中时间与垂直总力（用于跳跃弧线归一化）。
	/// </summary>
	/// <param name="appliedForce">当前垂直力。</param>
	private void setVerticalData(Fix64 appliedForce) {
        Fix64 maxHeight = (appliedForce * appliedForce) / (appliedGravity * 2);
		maxHeight += worldTransform.position.y;
        airTime = FPMath.Sqrt(maxHeight * 2 / appliedGravity);
		verticalTotalForce = appliedGravity * airTime;
	}

	/// <summary>
	/// 应用新的角色重量（改变重力加速度）。
	/// </summary>
	/// <param name="newWeight">新重量。</param>
	public void ApplyNewWeight(Fix64 newWeight) {
		appliedGravity = newWeight * UFE.config._gravity;
	}

	/// <summary>
	/// 恢复角色默认重量。
	/// </summary>
	public void ResetWeight(){
		appliedGravity = controlScript.myInfo.physics._weight * UFE.config._gravity;
	}
	
	/// <summary>
	/// 计算指定力可产生的空中停留时间。
	/// </summary>
	/// <param name="appliedForce">垂直力。</param>
	/// <returns>空中时间（秒）。</returns>
	public Fix64 GetPossibleAirTime(Fix64 appliedForce) {
        Fix64 maxHeight = (appliedForce * appliedForce) / (appliedGravity * 2);
		maxHeight += worldTransform.position.y;
        return FPMath.Sqrt(maxHeight * 2 / appliedGravity);
	}

	/// <summary>
	/// 强制落地：清零所有力、重置跳跃/弹跳状态并回到站立。
	/// </summary>
	public void ForceGrounded() {
		verticalForce = 0;
		horizontalForce = 0;
		setVerticalData(0);
		currentAirJumps = 0;
		isTakingOff = false;
        isLanding = false;
        isGroundBouncing = false;
        isWallBouncing = false;
		if (worldTransform.position.y != 0) worldTransform.Translate(new FPVector(0, -worldTransform.position.y, 0));
		controlScript.currentState = PossibleStates.Stand;
	}
	
	/// <summary>
	/// 应用当前所有力（无指定招式）。
	/// </summary>
	public void ApplyForces() {
		ApplyForces(null);
	}

	/// <summary>
	/// 应用当前所有力：处理摩擦、重力、墙壁/地面弹跳、拉近（PullIn）、边界限制、落地检测与对应动画切换。
	/// <para>这是物理模拟的核心方法，由 ControlsScript 每固定帧调用。</para>
	/// </summary>
	/// <param name="move">当前执行的招式（用于忽略重力/摩擦等招式属性）。</param>
	public void ApplyForces(MoveInfo move) {
		if (freeze) return;

        controlScript.normalizedJumpArc = (Fix64)1 - ((verticalForce + verticalTotalForce) / (verticalTotalForce * 2));
        

		Fix64 appliedFriction = (moveDirection != 0 || controlScript.myInfo.physics.highMovingFriction) ? 
			UFE.config.selectedStage._groundFriction : controlScript.myInfo.physics._friction;


		if (move != null && move.ignoreFriction) appliedFriction = 0;

		if (controlScript.activePullIn != null){
            worldTransform.position = FPVector.Lerp(worldTransform.position, 
			                                  controlScript.activePullIn.position,
                                              UFE.fixedDeltaTime * controlScript.activePullIn.speed);

			if (controlScript.activePullIn.forceStand && !IsGrounded()) ForceGrounded();

			if (FPVector.Distance(controlScript.activePullIn.position, worldTransform.position) <= controlScript.activePullIn._targetDistance || 
			    controlScript.currentSubState != SubStates.Stunned) {
				controlScript.activePullIn = null;
			}

		}else{
			if (!IsGrounded()) {
				appliedFriction = 0;
				if (verticalForce == 0) verticalForce = -.1;
			}

			if (horizontalForce != 0 && !isTakingOff) {
				if (horizontalForce > 0) {
                    horizontalForce -= appliedFriction * UFE.fixedDeltaTime;
                    horizontalForce = FPMath.Max(0, horizontalForce);
				}else if (horizontalForce < 0) {
                    horizontalForce += appliedFriction * UFE.fixedDeltaTime;
                    horizontalForce = FPMath.Min(0, horizontalForce);
				}
                
                Fix64 leftCameraBounds = opWorldTransform.position.x - (UFE.config.cameraOptions._maxDistance /2);
                Fix64 rightCameraBounds = opWorldTransform.position.x + (UFE.config.cameraOptions._maxDistance /2);

                bool bouncingOnCamera = false;
                if (controlScript.currentHit != null
                    && controlScript.currentHit.bounceOnCameraEdge
                    && (worldTransform.position.x <= leftCameraBounds
                    || worldTransform.position.x >= rightCameraBounds)) {
                    bouncingOnCamera = true;
                }


                if (wallBounceTimes < UFE.config.wallBounceOptions._maximumBounces 
                    && controlScript.currentSubState == SubStates.Stunned
                    && controlScript.currentState != PossibleStates.Down
                    && UFE.config.wallBounceOptions.bounceForce != Sizes.None
                    && FPMath.Abs(horizontalForce) >= UFE.config.wallBounceOptions._minimumBounceForce
                    && (worldTransform.position.x <= UFE.config.selectedStage._leftBoundary
                    || worldTransform.position.x >= UFE.config.selectedStage._rightBoundary || bouncingOnCamera)
                    && controlScript.currentHit != null && controlScript.currentHit.wallBounce
                    && !isWallBouncing) {
                    
                    if (controlScript.currentHit.overrideForcesOnWallBounce) {
                        if (controlScript.currentHit.resetWallBounceHorizontalPush) horizontalForce = 0;
                        if (controlScript.currentHit.resetWallBounceVerticalPush) verticalForce = 0;

                        Fix64 addedH = -controlScript.currentHit._wallBouncePushForce.x;
                        Fix64 addedV = controlScript.currentHit._wallBouncePushForce.y;

                        AddForce(new FPVector(addedH, addedV, 0), controlScript.mirror);

                    } else {
                        if (UFE.config.wallBounceOptions.bounceForce == Sizes.Small) {
                            horizontalForce /= -1.4;
                        } else if (UFE.config.wallBounceOptions.bounceForce == Sizes.Medium) {
                            horizontalForce /= -1.2;
                        } else if (UFE.config.wallBounceOptions.bounceForce == Sizes.High) {
                            horizontalForce *= -1;
                        }
                    }

                    wallBounceTimes++;

                    if (verticalForce > 0 || !IsGrounded()) {
                        if (moveSetScript.basicMoves.airWallBounce.animMap[0].clip != null) {
                            controlScript.currentHitAnimation = moveSetScript.basicMoves.airWallBounce.name;
                        }
                    } else {
                        if (controlScript.currentHit.knockOutOnWallBounce) {
                            moveSetScript.PlayBasicMove(moveSetScript.basicMoves.standingWallBounceKnockdown);
                            controlScript.currentHitAnimation = moveSetScript.basicMoves.standingWallBounceKnockdown.name;
                        } else {
                            moveSetScript.PlayBasicMove(moveSetScript.basicMoves.standingWallBounce);
                            controlScript.currentHitAnimation = moveSetScript.basicMoves.standingWallBounce.name;
                        }
                    }

                    if (UFE.config.wallBounceOptions.bouncePrefab != null) {
                        GameObject pTemp = UFE.SpawnGameObject(UFE.config.wallBounceOptions.bouncePrefab, transform.position, Quaternion.identity, Mathf.RoundToInt(UFE.config.wallBounceOptions.bounceKillTime * UFE.config.fps));
                        pTemp.transform.rotation = UFE.config.wallBounceOptions.bouncePrefab.transform.rotation;
                        if (UFE.config.wallBounceOptions.sticky) pTemp.transform.parent = transform;
                        //pTemp.transform.localPosition = Vector3.zero;
                    }

                    if (UFE.config.wallBounceOptions.shakeCamOnBounce) {
                        controlScript.shakeCameraDensity = UFE.config.wallBounceOptions._shakeDensity;
                    }

                    UFE.PlaySound(UFE.config.wallBounceOptions.bounceSound);
                    isWallBouncing = true;
                }

                worldTransform.Translate((horizontalForce * UFE.fixedDeltaTime), 0, 0);
			}
			
			if (move == null || (move != null && !move.ignoreGravity)){
				if ((verticalForce < 0 && !IsGrounded()) || verticalForce > 0) {
                    verticalForce -= appliedGravity * UFE.fixedDeltaTime;
                    worldTransform.Translate((moveDirection * UFE.fixedDeltaTime) * controlScript.myInfo.physics._jumpDistance, (verticalForce * UFE.fixedDeltaTime), 0);
                } else if (verticalForce < 0 
                    && IsGrounded() 
                    && controlScript.currentSubState != SubStates.Stunned)
                {
					verticalForce = 0;
				}
			}
		}

        Fix64 minDist = opWorldTransform.position.x - UFE.config.cameraOptions._maxDistance;
		Fix64 maxDist = opWorldTransform.position.x + UFE.config.cameraOptions._maxDistance;
        worldTransform.position = new FPVector(FPMath.Clamp(worldTransform.position.x, minDist, maxDist), worldTransform.position.y, worldTransform.position.z);

        worldTransform.position = new FPVector(
            FPMath.Clamp(worldTransform.position.x,
		            UFE.config.selectedStage._leftBoundary,
		            UFE.config.selectedStage._rightBoundary),
            FPMath.Max(worldTransform.position.y, UFE.config.selectedStage._groundHeight),
            worldTransform.position.z);

        if (controlScript.currentState == PossibleStates.Down) return;

		if (IsGrounded() && controlScript.currentState != PossibleStates.Down){
            if (verticalTotalForce != 0) {
				if (groundBounceTimes < UFE.config.groundBounceOptions._maximumBounces 
                    && controlScript.currentSubState == SubStates.Stunned 
                    && UFE.config.groundBounceOptions.bounceForce != Sizes.None 
                    && verticalForce <= -UFE.config.groundBounceOptions._minimumBounceForce
                    && controlScript.currentHit.groundBounce)
                {
                    if (controlScript.currentHit.overrideForcesOnGroundBounce) {
                        if (controlScript.currentHit.resetGroundBounceHorizontalPush) horizontalForce = 0;
                        if (controlScript.currentHit.resetGroundBounceVerticalPush) verticalForce = 0;

                        Fix64 addedH = controlScript.currentHit._groundBouncePushForce.x;
                        Fix64 addedV = controlScript.currentHit._groundBouncePushForce.y;

                        AddForce(new FPVector(addedH, addedV, 0), controlScript.mirror);

                    } else {
                        if (UFE.config.groundBounceOptions.bounceForce == Sizes.Small) {
                            AddForce(new FPVector(0, (-verticalForce / 2.4), 0), 1);
                        } else if (UFE.config.groundBounceOptions.bounceForce == Sizes.Medium) {
                            AddForce(new FPVector(0, (-verticalForce / 1.8), 0), 1);
                        } else if (UFE.config.groundBounceOptions.bounceForce == Sizes.High) {
                            AddForce(new FPVector(0, (-verticalForce / 1.2), 0), 1);
                        }
                    }

					groundBounceTimes ++;

                    if (!isGroundBouncing) {
                        controlScript.stunTime += airTime + UFE.config.knockDownOptions.air._knockedOutTime;

                        if (moveSetScript.basicMoves.groundBounce.animMap[0].clip != null) {
                            controlScript.currentHitAnimation = moveSetScript.basicMoves.groundBounce.name;
                            moveSetScript.PlayBasicMove(moveSetScript.basicMoves.groundBounce);
                        }

						if (UFE.config.groundBounceOptions.bouncePrefab != null) {
							GameObject pTemp = UFE.SpawnGameObject(UFE.config.groundBounceOptions.bouncePrefab, transform.position, Quaternion.identity, Mathf.RoundToInt(UFE.config.groundBounceOptions.bounceKillTime * UFE.config.fps));
                            pTemp.transform.rotation = UFE.config.groundBounceOptions.bouncePrefab.transform.rotation;
                            if (UFE.config.groundBounceOptions.sticky) pTemp.transform.parent = transform;
							//pTemp.transform.localPosition = Vector3.zero;
                        }
						if (UFE.config.groundBounceOptions.shakeCamOnBounce) {
							controlScript.shakeCameraDensity = UFE.config.groundBounceOptions._shakeDensity;
						}
						UFE.PlaySound(UFE.config.groundBounceOptions.bounceSound);
						isGroundBouncing = true;
					}
					return;
				}
				verticalTotalForce = 0;
				airTime = 0;
				moveSetScript.totalAirMoves = 0;
                currentAirJumps = 0;

                BasicMoveInfo airAnimation = null;
                string downAnimation = "";
                
                isGroundBouncing = false;
				groundBounceTimes = 0;

                Fix64 animationSpeed = 0;
                Fix64 delayTime = 0;
				if (controlScript.currentMove != null && controlScript.currentMove.hitAnimationOverride) return;
				if (controlScript.currentSubState == SubStates.Stunned){

                    if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.airRecovery.name)) {
                        controlScript.stunTime = 0;
					    controlScript.currentState = PossibleStates.Stand;

                    } else {
					    controlScript.stunTime = UFE.config.knockDownOptions.air._knockedOutTime + UFE.config.knockDownOptions.air._standUpTime;

                        // Hit Clips
                        if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.getHitKnockBack.name)
                             && moveSetScript.basicMoves.getHitKnockBack.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.getHitKnockBack;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);

                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.getHitHighKnockdown.name)
                             && moveSetScript.basicMoves.getHitHighKnockdown.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.getHitHighKnockdown;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);
                            controlScript.stunTime = UFE.config.knockDownOptions.high._knockedOutTime + UFE.config.knockDownOptions.high._standUpTime;

                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.getHitMidKnockdown.name)
                             && moveSetScript.basicMoves.getHitMidKnockdown.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.getHitMidKnockdown;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);
                            controlScript.stunTime = UFE.config.knockDownOptions.highLow._knockedOutTime + UFE.config.knockDownOptions.highLow._standUpTime;

                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.getHitSweep.name)
                             && moveSetScript.basicMoves.getHitSweep.animMap[1].clip != null) {
                            airAnimation = moveSetScript.basicMoves.getHitSweep;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);
                            controlScript.stunTime = UFE.config.knockDownOptions.sweep._knockedOutTime + UFE.config.knockDownOptions.sweep._standUpTime;

                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.getHitCrumple.name)
                             && moveSetScript.basicMoves.getHitCrumple.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.getHitCrumple;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);

                        // Stage Clips
                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.standingWallBounceKnockdown.name)
                             && moveSetScript.basicMoves.standingWallBounceKnockdown.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.standingWallBounceKnockdown;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);
                            controlScript.stunTime = UFE.config.knockDownOptions.wallbounce._knockedOutTime + UFE.config.knockDownOptions.wallbounce._standUpTime;

                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.airWallBounce.name)
                             && moveSetScript.basicMoves.airWallBounce.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.airWallBounce;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);
                            controlScript.stunTime = UFE.config.knockDownOptions.wallbounce._knockedOutTime + UFE.config.knockDownOptions.wallbounce._standUpTime;

                        // Fall Clips
                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.fallingFromAirHit.name)
                            && moveSetScript.basicMoves.fallingFromAirHit.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.fallingFromAirHit;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);

                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.fallingFromGroundBounce.name)
                            && moveSetScript.basicMoves.fallingFromGroundBounce.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.fallingFromGroundBounce;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);

                        } else {
                            if (moveSetScript.basicMoves.fallDown.animMap[0].clip == null)
                                Debug.LogError("Fall Down From Air Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Fall Down From Air Hit");

                            airAnimation = moveSetScript.basicMoves.fallDown;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 1);
                        }
                        
					    controlScript.currentState = PossibleStates.Down;

                    }

				} else if (controlScript.currentState != PossibleStates.Stand){
                    if (moveSetScript.basicMoves.landing.animMap[0].clip != null
                        && (controlScript.currentMove == null ||
                        (controlScript.currentMove != null && controlScript.currentMove.cancelMoveWheLanding))){

                        controlScript.isAirRecovering = false;
						airAnimation = moveSetScript.basicMoves.landing;
						moveDirection = 0;
                        isLanding = true;
						controlScript.KillCurrentMove();
                        delayTime = (Fix64)controlScript.myInfo.physics.landingDelay / (Fix64)UFE.config.fps;
                        UFE.DelaySynchronizedAction(ResetLanding, delayTime);

                        if (airAnimation.autoSpeed) {
                            animationSpeed = moveSetScript.GetAnimationLength(airAnimation.name) / delayTime;
                        }
					}

					if (controlScript.currentState != PossibleStates.Crouch) controlScript.currentState = PossibleStates.Stand;

				}

				if (airAnimation != null) {
                    if (downAnimation != "") {
                        moveSetScript.PlayBasicMove(airAnimation, downAnimation);
                    } else {
                        moveSetScript.PlayBasicMove(airAnimation);
                    }

                    if (animationSpeed != 0) {
                        moveSetScript.SetAnimationSpeed(airAnimation.name, animationSpeed);
                    }
				}
			}
			
			if (controlScript.currentSubState != SubStates.Stunned 
                && !controlScript.isBlocking && !controlScript.blockStunned 
                && move == null 
                && !isTakingOff 
                && !isLanding 
                && controlScript.currentState == PossibleStates.Stand){
				if (moveDirection > 0 && controlScript.mirror == -1 ||
				    moveDirection < 0 && controlScript.mirror == 1) {
					if (moveSetScript.basicMoves.moveForward.animMap[0].clip == null)
						Debug.LogError("Move Forward animation not found! Make sure you have it set on Character -> Basic Moves -> Move Forward");
					if (!moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.moveForward.name)) {
					    moveSetScript.PlayBasicMove(moveSetScript.basicMoves.moveForward);
					}

				}else if (moveDirection > 0 && controlScript.mirror == 1||
				    moveDirection < 0 && controlScript.mirror == -1) {
					if (moveSetScript.basicMoves.moveBack.animMap[0].clip == null)
						Debug.LogError("Move Back animation not found! Make sure you have it set on Character -> Basic Moves -> Move Back");
					if (!moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.moveBack.name)) {
						moveSetScript.PlayBasicMove(moveSetScript.basicMoves.moveBack);
					}
				}
			}
        } else if (verticalForce > 0 || !IsGrounded()) {
			if (move != null && controlScript.currentState == PossibleStates.Stand)
				controlScript.currentState = PossibleStates.NeutralJump;
			if (move == null && verticalForce/verticalTotalForce > 0 && verticalForce/verticalTotalForce <= 1) {
				if (isGroundBouncing) return;

				if (moveDirection == 0) {
					controlScript.currentState = PossibleStates.NeutralJump;
				}else{
					if (moveDirection > 0 && controlScript.mirror == -1 ||
					    moveDirection < 0 && controlScript.mirror == 1) {
						controlScript.currentState = PossibleStates.ForwardJump;
					}

					if (moveDirection > 0 && controlScript.mirror == 1||
					    moveDirection < 0 && controlScript.mirror == -1) {
						controlScript.currentState = PossibleStates.BackJump;
					}
				}

                BasicMoveInfo airAnimation = moveSetScript.basicMoves.jumpStraight;
				if (controlScript.currentSubState == SubStates.Stunned){
                    if (isWallBouncing && moveSetScript.basicMoves.airWallBounce.animMap[0].clip != null) {
                        airAnimation = moveSetScript.basicMoves.airWallBounce;

                    } else if (moveSetScript.basicMoves.getHitKnockBack.animMap[0].clip != null && 
					    FPMath.Abs(horizontalForce) > UFE.config.comboOptions._knockBackMinForce && 
					    UFE.config.comboOptions._knockBackMinForce > 0){
						airAnimation = moveSetScript.basicMoves.getHitKnockBack;
                        airTime *= (Fix64)2;

					} else {
						if (moveSetScript.basicMoves.getHitAir.animMap[0].clip == null)
							Debug.LogError("Get Hit Air animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Air");

                        airAnimation = moveSetScript.basicMoves.getHitAir;
                    }
                    if (overrideStunAnimation != null) airAnimation = overrideStunAnimation;

                } else if (controlScript.isAirRecovering 
                    && (moveSetScript.basicMoves.airRecovery.animMap[0].clip != null)) {
						airAnimation = moveSetScript.basicMoves.airRecovery;

				} else {
					if (moveSetScript.basicMoves.jumpForward.animMap[0].clip != null && controlScript.currentState == PossibleStates.ForwardJump) {
						airAnimation = moveSetScript.basicMoves.jumpForward;
					} else if (moveSetScript.basicMoves.jumpBack.animMap[0].clip != null && controlScript.currentState == PossibleStates.BackJump) {
						airAnimation = moveSetScript.basicMoves.jumpBack;
					} else {
						if (moveSetScript.basicMoves.jumpStraight.animMap[0].clip == null)
							Debug.LogError("Jump animation not found! Make sure you have it set on Character -> Basic Moves -> Jump Straight");

						airAnimation = moveSetScript.basicMoves.jumpStraight;
					}
				}

                if (!overrideAirAnimation && !moveSetScript.IsAnimationPlaying(airAnimation.name)) {
                    moveSetScript.PlayBasicMove(airAnimation);

                    if (airAnimation.autoSpeed)
                        moveSetScript.SetAnimationNormalizedSpeed(airAnimation.name, (moveSetScript.GetAnimationLength(airAnimation.name) / airTime));
				}

            } else if (move == null && verticalForce / verticalTotalForce <= 0) {

                BasicMoveInfo airAnimation = moveSetScript.basicMoves.fallStraight;
                if (isGroundBouncing && moveSetScript.basicMoves.fallingFromGroundBounce.animMap[0].clip != null) {
                    airAnimation = moveSetScript.basicMoves.fallingFromGroundBounce;

                } else if (isWallBouncing && moveSetScript.basicMoves.airWallBounce.animMap[0].clip != null) {
                    airAnimation = moveSetScript.basicMoves.airWallBounce;

				} else {
					if (controlScript.currentSubState == SubStates.Stunned){
						if (moveSetScript.basicMoves.getHitKnockBack.animMap[0].clip != null &&
                            FPMath.Abs(horizontalForce) > UFE.config.comboOptions._knockBackMinForce && 
						    UFE.config.comboOptions._knockBackMinForce > 0){
							airAnimation = moveSetScript.basicMoves.getHitKnockBack;

                        } else {
                            airAnimation = moveSetScript.basicMoves.getHitAir;
                            if (moveSetScript.basicMoves.fallingFromAirHit.animMap[0].clip != null) {
                                airAnimation = moveSetScript.basicMoves.fallingFromAirHit;

                            } else if (moveSetScript.basicMoves.getHitAir.animMap[0].clip == null) {
                                Debug.LogError("Air Juggle animation not found! Make sure you have it set on Character -> Basic Moves -> Air Juggle");
                            }
                        }
                        if (overrideStunAnimation != null) airAnimation = overrideStunAnimation;

                    } else if (controlScript.isAirRecovering 
                        && (moveSetScript.basicMoves.airRecovery.animMap[0].clip != null)) {
                        airAnimation = moveSetScript.basicMoves.airRecovery;

					} else {
						if (moveSetScript.basicMoves.fallForward.animMap[0].clip != null && controlScript.currentState == PossibleStates.ForwardJump) {
							airAnimation = moveSetScript.basicMoves.fallForward;
						} else if (moveSetScript.basicMoves.fallBack.animMap[0].clip != null && controlScript.currentState == PossibleStates.BackJump) {
							airAnimation = moveSetScript.basicMoves.fallBack;
						} else {
							if (moveSetScript.basicMoves.fallStraight.animMap[0].clip == null)
								Debug.LogError("Fall animation not found! Make sure you have it set on Character -> Basic Moves -> Fall Straight");
							
							airAnimation = moveSetScript.basicMoves.fallStraight;
						}
					}
				}

				if (!overrideAirAnimation && !moveSetScript.IsAnimationPlaying(airAnimation.name)){
                    moveSetScript.PlayBasicMove(airAnimation);

                    if (airAnimation.autoSpeed) {
                        moveSetScript.SetAnimationNormalizedSpeed(airAnimation.name, (moveSetScript.GetAnimationLength(airAnimation.name) / airTime));
                    }
				}
			}
		}
		if (horizontalForce == 0 && verticalForce == 0) moveDirection = 0;
    }

	/// <summary>
	/// 判断角色是否着地（Y 坐标不高于场地地面高度）。
	/// </summary>
	/// <returns>true 表示着地。</returns>
	public bool IsGrounded() {
        if (worldTransform.position.y <= UFE.config.selectedStage._groundHeight) {
            return true;
        }
		/*if (Physics.RaycastAll(worldTransform.position.ToVector() + new Vector3(0, 2f, 0), Vector3.down, 2.02f, groundMask).Length > 0) {
			//if (transform.position.y != 0) transform.Translate(new Vector3(0, -transform.position.y, 0));
            if (worldTransform.position.y != 0) worldTransform.Translate(new FPVector(0, -worldTransform.position.y, 0));
            return true;
		}*/
		return false;
	}
}
