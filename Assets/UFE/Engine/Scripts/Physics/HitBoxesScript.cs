using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using FPLibrary;
using UFE3D;

/// <summary>
/// 判定盒系统（HitBoxesScript）。
/// <para>用途：本文件定义攻击判定盒（HitBox）、受击判定盒（HurtBox）、可格挡区域（BlockArea）、拉近（PullIn）等数据结构，</para>
/// <para>以及 HitBoxesScript 组件——负责命中检测（圆/矩形碰撞）、判定盒镜像、映射更新与 Gizmos 可视化。</para>
/// <para>碰撞检测使用定点数（Fix64）保证网络帧同步确定性。</para>
/// </summary>

/// <summary>
/// 攻击判定盒（HitBox）：招式的攻击范围/身体碰撞体定义。
/// </summary>
[System.Serializable]
public class HitBox: ICloneable {
	/// <summary>默认是否可见（初始显示状态）。</summary>
	public bool defaultVisibility = true;
	/// <summary>绑定的身体部位。</summary>
	public BodyPart bodyPart;
	/// <summary>判定盒类型（高/低）。</summary>
	public HitBoxType type;
	/// <summary>判定盒形状（圆/矩形）。</summary>
	public HitBoxShape shape;
	/// <summary>矩形判定盒尺寸（float 版）。</summary>
	public Rect rect = new Rect(0, 0, 4, 4);
	/// <summary>矩形判定盒尺寸（定点数版，运行时使用）。</summary>
	public FPRect _rect = new FPRect();
	/// <summary>是否跟随角色渲染边界 X。</summary>
	public bool followXBounds;
	/// <summary>是否跟随角色渲染边界 Y。</summary>
	public bool followYBounds;
	/// <summary>圆形判定盒半径（float 版）。</summary>
	public float radius = .5f;
	/// <summary>圆形判定盒半径（定点数版，运行时使用）。</summary>
	public Fix64 _radius = .5;
	/// <summary>判定盒位置偏移（float 版）。</summary>
	public Vector2 offSet;
	/// <summary>判定盒位置偏移（定点数版，运行时使用）。</summary>
	public FPVector _offSet;

	/// <summary>碰撞类型（身体/攻击/投技/无）。</summary>
	public CollisionType collisionType;
	/// <summary>绑定的 Transform（编辑器映射用）。</summary>
	public Transform position;
	/// <summary>映射后的位置（帧映射数据）。</summary>
    public FPVector mappedPosition;

    #region trackable definitions
	/// <summary>当前碰撞状态（0 未命中 / 1 命中，运行时跟踪）。</summary>
    public int state{get;set;}
	/// <summary>渲染器边界（运行时跟踪）。</summary>
    public Rect rendererBounds{get;set;}
	/// <summary>是否隐藏（禁用碰撞检测，运行时跟踪）。</summary>
    public bool hide{get;set;}          // Whether the hit box collisions will be detected
	/// <summary>是否可见（GameObject 层级激活状态，运行时跟踪）。</summary>
    public bool visibility{get;set;}    // Whether the GameObject will be active in the hierarchy
    #endregion

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 受击判定盒（HurtBox）：角色可被攻击命中的部位定义。
/// </summary>
[System.Serializable]
public class HurtBox: ICloneable {
	/// <summary>绑定的身体部位。</summary>
	public BodyPart bodyPart;
	/// <summary>判定盒形状。</summary>
	public HitBoxShape shape;
	/// <summary>矩形尺寸（float 版）。</summary>
	public Rect rect = new Rect(0, 0, 4, 4);
	/// <summary>矩形尺寸（定点数版）。</summary>
	public FPRect _rect = new FPRect();
	/// <summary>是否跟随角色渲染边界 X。</summary>
	public bool followXBounds;
	/// <summary>是否跟随角色渲染边界 Y。</summary>
	public bool followYBounds;
	/// <summary>圆形半径（float 版）。</summary>
	public float radius = .5f;
	/// <summary>圆形半径（定点数版）。</summary>
	public Fix64 _radius = .5;
	/// <summary>位置偏移（float 版）。</summary>
    public Vector2 offSet;
	/// <summary>位置偏移（定点数版）。</summary>
    public FPVector _offSet;

    #region trackable definitions
	/// <summary>是否处于格挡状态（运行时跟踪）。</summary>
    public bool isBlock{get; set;}
	/// <summary>世界位置（定点数，运行时跟踪）。</summary>
    public FPVector position{get;set;}
	/// <summary>渲染器边界（运行时跟踪）。</summary>
    public Rect rendererBounds{get;set;}
    #endregion

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
    public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 可格挡区域（BlockArea）：招式判定帧内可以格挡的区域。
/// </summary>
[System.Serializable]
public class BlockArea {
	/// <summary>生效起始帧。</summary>
	public int activeFramesBegin;
	/// <summary>生效结束帧。</summary>
	public int activeFramesEnds;

	/// <summary>绑定的身体部位。</summary>
	public BodyPart bodyPart;
	/// <summary>形状。</summary>
	public HitBoxShape shape;
	/// <summary>矩形尺寸（float 版）。</summary>
	public Rect rect = new Rect(0, 0, 4, 4);
	/// <summary>矩形尺寸（定点数版）。</summary>
	public FPRect _rect = new FPRect();
	/// <summary>是否跟随角色渲染边界 X。</summary>
	public bool followXBounds;
	/// <summary>是否跟随角色渲染边界 Y。</summary>
	public bool followYBounds;
	/// <summary>圆形半径（float 版）。</summary>
	public float radius = .5f;
	/// <summary>圆形半径（定点数版）。</summary>
	public Fix64 _radius = .5;
	/// <summary>位置偏移（float 版）。</summary>
	public Vector2 offSet;
	/// <summary>位置偏移（定点数版）。</summary>
	public FPVector _offSet;

	/// <summary>世界位置（Inspector 隐藏，运行时使用）。</summary>
	[HideInInspector] public FPVector position;
}

/// <summary>
/// 拉近（PullIn）：投技/演出中将对手或自身拉向目标的配置。
/// </summary>
[System.Serializable]
public class PullIn: ICloneable {
	/// <summary>拉近速度。</summary>
	public int speed = 50;
	/// <summary>拉近期间是否强制目标站立。</summary>
	public bool forceStand = true;
	/// <summary>自身目标身体部位。</summary>
	public BodyPart characterBodyPart;
	/// <summary>对方目标身体部位。</summary>
	public BodyPart enemyBodyPart;
	/// <summary>目标距离（float 版，到达后停止拉近）。</summary>
	public float targetDistance = .5f;
	/// <summary>目标距离（定点数版，运行时使用）。</summary>
	public Fix64 _targetDistance = .5;

	/// <summary>拉近目标位置（运行时设置）。</summary>
    public FPVector position;
	
	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 判定盒脚本（HitBoxesScript）：角色身上的判定盒管理器。
/// <para>负责命中检测（TestCollision）、判定盒镜像、映射位置更新、无敌部位隐藏及编辑器 Gizmos 可视化。</para>
/// </summary>
public class HitBoxesScript : MonoBehaviour {
    
    #region trackable definitions
	/// <summary>本帧是否已命中（运行时跟踪）。</summary>
    public bool isHit;
	/// <summary>攻击判定盒列表。</summary>
    public HitBox[] hitBoxes;
	/// <summary>当前生效的受击判定盒列表。</summary>
    public HurtBox[] activeHurtBoxes;
	/// <summary>当前可格挡区域。</summary>
    public BlockArea blockableArea;
	/// <summary>当前命中确认类型（普通命中/投技）。</summary>
    public HitConfirmType hitConfirmType;
	/// <summary>碰撞盒尺寸（调试用）。</summary>
    public Fix64 collisionBoxSize;
	/// <summary>当前是否已镜像（运行时跟踪）。</summary>
    public bool currentMirror;
	/// <summary>是否烘焙动画速度。</summary>
    public bool bakeSpeed;
	/// <summary>当前动画帧映射列表。</summary>
    public AnimationMap[] animationMaps = new AnimationMap[0];
    #endregion

	/// <summary>角色控制脚本引用（Inspector 隐藏）。</summary>
	[HideInInspector] public ControlsScript controlsScript;
	/// <summary>编辑器用：是否预览反向旋转。</summary>
	[HideInInspector] public bool previewInvertRotation;
	/// <summary>编辑器用：是否预览镜像。</summary>
    [HideInInspector] public bool previewMirror;
	/// <summary>编辑器用：是否显示矩形判定盒位置测试。</summary>
	public bool rectangleHitBoxLocationTest;
	/// <summary>矩形判定盒可视化贴图。</summary>
	public Texture2D rectTexture;

	/// <summary>招式集合脚本引用。</summary>
	public MoveSetScript moveSetScript;
	/// <summary>角色渲染器（用于边界跟随）。</summary>
    private Renderer characterRenderer;
	/// <summary>帧位移增量（动画映射）。</summary>
    private FPVector deltaPosition;

	/// <summary>本角色世界变换（定点数）快捷属性。</summary>
    private FPTransform worldTransform { get { return controlsScript.worldTransform; } set { controlsScript.worldTransform = value; } }
	/// <summary>对手世界变换（定点数）快捷属性。</summary>
    private FPTransform opWorldTransform { get { return controlsScript.opControlsScript.worldTransform; } set { controlsScript.opControlsScript.worldTransform = value; } }

    //[HideInInspector] public Rect characterBounds = new Rect(0,0,0,0);

	/// <summary>
	/// 启动：获取控件脚本引用、为每个招式的无敌部位关联判定盒、创建可视化贴图。
	/// </summary>
    void Start(){
		if (transform.parent != null){
			controlsScript = transform.parent.gameObject.GetComponent<ControlsScript>();
		}
		moveSetScript = GetComponent<MoveSetScript>();
		UpdateRenderer();

		if (moveSetScript != null){
			foreach(MoveInfo move in moveSetScript.moves){
                if (move == null) {
                    Debug.LogWarning("You have empty entries in your move list. Check your special moves under Character Editor.");
                    continue;
                }
				foreach(InvincibleBodyParts invBodyPart in move.invincibleBodyParts){
					List<HitBox> invHitBoxes = new List<HitBox>();
					foreach(BodyPart bodyPart in invBodyPart.bodyParts){
						foreach(HitBox hitBox in hitBoxes){
							if (bodyPart == hitBox.bodyPart) {
								invHitBoxes.Add(hitBox);
								break;
							}
						}
					}
					invBodyPart.hitBoxes = invHitBoxes.ToArray();
				}
			}
		}

        rectangleHitBoxLocationTest = false;
		rectTexture = new Texture2D(1,1);
        rectTexture.SetPixel(0, 0, Color.red);
        rectTexture.Apply();
	}
	
	/// <summary>
	/// 静态碰撞检测：遍历攻击判定盒与受击判定盒，进行圆/矩形交叉测试。
	/// </summary>
	/// <param name="rootPosition">攻击方根位置。</param>
	/// <param name="hitBoxes">攻击判定盒列表。</param>
	/// <param name="hurtBoxes">受击判定盒列表。</param>
	/// <param name="hitConfirmType">命中确认类型（决定是否使用投技判定盒）。</param>
	/// <param name="mirror">攻击方朝向（镜像矩形位置）。</param>
	/// <returns>命中时返回 [受击盒位置, 攻击盒位置, 中点]；未命中返回空数组。</returns>
	public static FPVector[] TestCollision(FPVector rootPosition, HitBox[] hitBoxes, HurtBox[] hurtBoxes, HitConfirmType hitConfirmType, int mirror) {
		foreach (HitBox hitBox in hitBoxes) {
			if (hitBox.hide) continue;
			if (hitBox.collisionType == CollisionType.noCollider) continue;
			if (hitConfirmType == HitConfirmType.Throw && hitBox.collisionType != CollisionType.throwCollider) continue;
			if (hitConfirmType == HitConfirmType.Hit && hitBox.collisionType == CollisionType.throwCollider) continue;

            hitBox.state = 0;
            //drawRect.Clear();
			foreach (HurtBox hurtBox in hurtBoxes) {
                FPVector hurtBoxPosition = hurtBox.position;
                FPVector hitBoxPosition = hitBox.mappedPosition + rootPosition;

				Fix64 dist = 0;
				bool collisionConfirm = false;
				
				if (!UFE.config.detect3D_Hits){
					hurtBoxPosition.z = 0;
					hitBoxPosition.z = 0;
				}
				
				if (hurtBox.shape == HitBoxShape.circle) {
					if (hitBox.shape == HitBoxShape.circle) {
						dist = FPVector.Distance(hurtBoxPosition, hitBoxPosition);
						if (dist <= hurtBox._radius + hitBox._radius) collisionConfirm = true;
						
					}else if (hitBox.shape == HitBoxShape.rectangle){
                        FPRect hitBoxRectanglePosition = hitBox._rect;
                        hitBoxRectanglePosition.x += hitBoxPosition.x;
                        hitBoxRectanglePosition.y += hitBoxPosition.y;
                        hitBoxRectanglePosition.RefreshPoints();

                        if (hitBox.followXBounds) {
                            //hitBoxRectanglePosition.x = hitBox.rendererBounds.x - (hitBox.rect.width / 2);
                            //hitBoxRectanglePosition.width = (hitBox.rendererBounds.width + hitBox.rect.width) - hitBox.rendererBounds.x;
                        }
                        if (hitBox.followYBounds) {
                            //hitBoxRectanglePosition.y = hitBox.rendererBounds.y - (hitBox.rect.height / 2);
                            //hitBoxRectanglePosition.height = (hitBox.rendererBounds.height + hitBox.rect.height) - hitBox.rendererBounds.y;
                        }
                        
                        dist = hitBoxRectanglePosition.DistanceToPoint(hurtBoxPosition);
                        if (hurtBox._radius >= dist) collisionConfirm = true;
                        
                        /*if (collisionConfirm && !hurtBox.isBlock) {
                            Debug.Log("------------------");
                            Debug.Log(hurtBoxPosition);
                            Debug.Log(hitBox.bodyPart + " - " + hitBoxRectanglePosition);
                            Debug.Log("xMin/xMax,yMin/yMax : " + hitBoxRectanglePosition.xMin + "/" + hitBoxRectanglePosition.xMax + ", " + hitBoxRectanglePosition.yMin + "/" + hitBoxRectanglePosition.yMax);
                            Debug.Log(hurtBox.radius + " >= " + dist + " = " + collisionConfirm);
                        }*/
					}
				}else if (hurtBox.shape == HitBoxShape.rectangle) {
                    FPRect hurtBoxRectanglePosition = hurtBox._rect;
                    if (mirror < 0) hurtBoxRectanglePosition.x += hurtBoxRectanglePosition.width;
                    hurtBoxRectanglePosition.x *= mirror;
                    hurtBoxRectanglePosition.x += hurtBoxPosition.x;
                    hurtBoxRectanglePosition.y += hurtBoxPosition.y;
                    hurtBoxRectanglePosition.RefreshPoints();
                    
                    if (hitBox.shape == HitBoxShape.circle){

						if (hurtBox.followXBounds){
							//hurtBoxRectanglePosition.x = hurtBox.rendererBounds.x - (hurtBox.rect.width/2);
							//hurtBoxRectanglePosition.width = (hurtBox.rendererBounds.width + hurtBox.rect.width) - hurtBox.rendererBounds.x;
						}
						if (hurtBox.followYBounds){
							//hurtBoxRectanglePosition.y = hurtBox.rendererBounds.y - (hurtBox.rect.height/2);
							//hurtBoxRectanglePosition.height = (hurtBox.rendererBounds.height + hurtBox.rect.height) - hurtBox.rendererBounds.y;
						}

                        dist = hurtBoxRectanglePosition.DistanceToPoint(hitBoxPosition);
						if (dist <= hitBox._radius) collisionConfirm = true;
						
					}else if (hitBox.shape == HitBoxShape.rectangle){
                        FPRect hitBoxRectanglePosition = hitBox._rect;
                        //if (mirror > 0) hitBoxRectanglePosition.x += hitBoxRectanglePosition.width;
                        //hitBoxRectanglePosition.x *= -mirror;
                        hitBoxRectanglePosition.x += hitBoxPosition.x;
                        hitBoxRectanglePosition.y += hitBoxPosition.y;
                        hitBoxRectanglePosition.RefreshPoints();


						if (hitBox.followXBounds){
							//hitBoxRectanglePosition.x = hitBox.rendererBounds.x - (hitBox.rect.width/2);
							//hitBoxRectanglePosition.width = (hitBox.rendererBounds.width + hitBox.rect.width) - hitBox.rendererBounds.x;
						}
						if (hitBox.followYBounds){
							//hitBoxRectanglePosition.y = hitBox.rendererBounds.y - (hitBox.rect.height/2);
							//hitBoxRectanglePosition.height = (hitBox.rendererBounds.height + hitBox.rect.height) - hitBox.rendererBounds.y;
						}

						if (hurtBox.followXBounds){
							//hurtBoxRectanglePosition.x = hurtBox.rendererBounds.x - (hurtBox.rect.width/2);
							//hurtBoxRectanglePosition.width = (hurtBox.rendererBounds.width + hurtBox.rect.width) - hurtBox.rendererBounds.x;
						}
						if (hurtBox.followYBounds){
							//hurtBoxRectanglePosition.y = hurtBox.rendererBounds.y - (hurtBox.rect.height/2);
							//hurtBoxRectanglePosition.height = (hurtBox.rendererBounds.height + hurtBox.rect.height) - hurtBox.rendererBounds.y;
						}
						
						if (hurtBoxRectanglePosition.Intersects(hitBoxRectanglePosition)) collisionConfirm = true;
					}
				}

				if (collisionConfirm) {
					if (hitConfirmType == HitConfirmType.Hit) {
						hitBox.state = 1;
					}
					return new FPVector[]{hurtBoxPosition, hitBoxPosition, (hurtBoxPosition + hitBoxPosition)/2};
				}
			}
		}

		foreach (HitBox hitBox in hitBoxes) {
			if (hitBox.state == 1) hitBox.state = 0;
		}
		return new FPVector[0];
	}

	/// <summary>
	/// 实例碰撞检测（受击盒数组）：已命中且为普通命中时直接返回空；否则执行碰撞测试。
	/// </summary>
	/// <param name="hurtBoxes">受击判定盒列表。</param>
	/// <param name="hitConfirmType">命中确认类型。</param>
	/// <returns>命中时返回位置数组；未命中返回空数组。</returns>
	public FPVector[] TestCollision(HurtBox[] hurtBoxes, HitConfirmType hitConfirmType) {
        if (isHit && hitConfirmType == HitConfirmType.Hit) return new FPVector[0];
		foreach(HitBox hitbox in this.hitBoxes) if (hitbox.followXBounds || hitbox.followYBounds) hitbox.rendererBounds = GetBounds();
		
		return HitBoxesScript.TestCollision(worldTransform.position, this.hitBoxes, hurtBoxes, hitConfirmType, controlsScript.mirror);
	}

	/// <summary>
	/// 格挡区域碰撞检测：将格挡区域作为特殊受击盒进行碰撞测试。
	/// </summary>
	/// <param name="blockableArea">可格挡区域。</param>
	/// <returns>命中时返回位置数组；未命中返回空数组。</returns>
	public FPVector[] TestCollision(BlockArea blockableArea) {
		HurtBox hurtBox = new HurtBox();
		hurtBox.position = blockableArea.position;
		hurtBox.shape = blockableArea.shape;
		hurtBox._rect = blockableArea._rect;
		hurtBox.followXBounds = blockableArea.followXBounds;
		hurtBox.followYBounds = blockableArea.followYBounds;
		hurtBox._radius = blockableArea._radius;
		hurtBox._offSet = blockableArea._offSet;
        hurtBox.isBlock = true;

		// We use throw confirmation type so the engine doesn't register the state of the stroke hitbox as hit
		return HitBoxesScript.TestCollision(worldTransform.position, this.hitBoxes, new HurtBox[]{hurtBox}, HitConfirmType.Hit, controlsScript.mirror);
	}
	
	/// <summary>
	/// 身体碰撞体推挤检测：计算双方身体碰撞体重叠产生的推挤力。
	/// </summary>
	/// <param name="myRootPosition">本方根位置。</param>
	/// <param name="opRootPosition">对方根位置。</param>
	/// <param name="opHitBoxes">对方身体碰撞体列表。</param>
	/// <returns>总推挤力（重叠深度累积）。</returns>
	public Fix64 TestCollision(FPVector myRootPosition, FPVector opRootPosition, HitBox[] opHitBoxes) {
		Fix64 totalPushForce = 0;
		foreach (HitBox hitBox in hitBoxes) {
			if (hitBox.collisionType != CollisionType.bodyCollider) continue;
			foreach (HitBox opHitBox in opHitBoxes) {
				if (opHitBox.collisionType != CollisionType.bodyCollider) continue;
				FPVector opHitBoxPosition = opHitBox.mappedPosition + opRootPosition;
                FPVector hitBoxPosition = hitBox.mappedPosition + myRootPosition;

				if (!UFE.config.detect3D_Hits){
					opHitBoxPosition.z = 0;
					hitBoxPosition.z = 0;
				}
				Fix64 dist = FPVector.Distance(opHitBoxPosition, hitBoxPosition);
				if (dist <= opHitBox._radius + hitBox._radius) totalPushForce += (opHitBox._radius + hitBox._radius) - dist;
			}
		}
		return totalPushForce;
	}

	/// <summary>
	/// 获取指定身体部位的默认可见性。
	/// </summary>
	/// <param name="bodyPart">身体部位。</param>
	/// <returns>默认可见返回 true。</returns>
	public bool GetDefaultVisibility(BodyPart bodyPart){
		foreach(HitBox hitBox in hitBoxes){
			if (bodyPart == hitBox.bodyPart && hitBox.defaultVisibility) return true;
		}

		return false;
	}


	/// <summary>
	/// 获取指定身体部位的世界位置（运行时用映射位置+根位置；编辑器下用 Transform 位置）。
	/// </summary>
	/// <param name="bodyPart">身体部位。</param>
	/// <returns>部位世界位置（定点数）。</returns>
	public FPVector GetPosition(BodyPart bodyPart){
		foreach(HitBox hitBox in hitBoxes){
            if (bodyPart == hitBox.bodyPart) {
                FPVector newMap = new FPVector();
                if (controlsScript == null) { 
                    // If its running from the editor, load positions from transforms
                    newMap = FPVector.ToFPVector(hitBox.position.position);
                } else {
                    newMap = hitBox.mappedPosition + worldTransform.position;
                }
                return newMap;
            }
		}
		return FPVector.zero;
    }

	/// <summary>
	/// 获取本帧位移增量（使用动画映射时取映射数据，否则取招式脚本的位移）。
	/// </summary>
	/// <returns>位移增量（定点数，已按镜像反转）。</returns>
    public FPVector GetDeltaPosition() {
        if (controlsScript.myInfo.useAnimationMaps)
        {
        return deltaPosition * -controlsScript.mirror;
    }
        else
        {
            return FPVector.ToFPVector(moveSetScript.GetDeltaPosition());
        }
    }

	/// <summary>
	/// 生成当前各判定盒的动画映射（用于烘焙逐帧映射数据）。
	/// </summary>
	/// <returns>HitBoxMap 数组。</returns>
    public HitBoxMap[] GetAnimationMaps() {
        List<HitBoxMap> animMaps = new List<HitBoxMap>();
        foreach (HitBox hitBox in hitBoxes) {
            HitBoxMap animMap = new HitBoxMap();
            animMap.bodyPart = hitBox.bodyPart;
            animMap.mappedPosition = FPVector.ToFPVector(hitBox.position.position);
            animMaps.Add(animMap);
        }

        return animMaps.ToArray();
    }

	/// <summary>
	/// 获取指定身体部位绑定的 Transform。
	/// </summary>
	/// <param name="bodyPart">身体部位。</param>
	/// <returns>对应的 Transform；未找到返回 null。</returns>
    public Transform GetTransform(BodyPart bodyPart){
		foreach(HitBox hitBox in hitBoxes){
			if (bodyPart == hitBox.bodyPart) return hitBox.position;
		}
		return null;
	}

	/// <summary>
	/// 为指定身体部位设置 Transform（编辑器烘焙用）。
	/// </summary>
	/// <param name="bodyPart">身体部位。</param>
	/// <param name="transform">要设置的 Transform。</param>
	public void SetTransform(BodyPart bodyPart, Transform transform){
		foreach(HitBox hitBox in hitBoxes){
			if (bodyPart == hitBox.bodyPart) {
				hitBox.position = transform;
				return;
			}
		}
	}

	/// <summary>
	/// 获取指定身体部位列表对应的判定盒列表。
	/// </summary>
	/// <param name="bodyParts">身体部位数组。</param>
	/// <returns>匹配的 HitBox 数组。</returns>
	public HitBox[] GetHitBoxes(BodyPart[] bodyParts){
		List<HitBox> hitBoxesList = new List<HitBox>();
		foreach(HitBox hitBox in hitBoxes){
			foreach(BodyPart bodyPart in bodyParts){
				if (bodyPart == hitBox.bodyPart) {
					hitBoxesList.Add(hitBox);
					break;
				}
			}
		}

		return hitBoxesList.ToArray();
	}
	
	/// <summary>
	/// 重置本帧命中状态（清空状态并复位 isHit）。
	/// </summary>
	public void ResetHit(){
		//if (!isHit) return;
		foreach (HitBox hitBox in hitBoxes) {
			if (hitBox.state == 1) hitBox.state = 0;
		}
		isHit = false;
	}

	/// <summary>
	/// 获取本帧命中（state==1）的攻击判定盒。
	/// </summary>
	/// <returns>命中的 HitBox；未命中返回 null。</returns>
	public HitBox GetStrokeHitBox(){
		if (!isHit) return null;
		foreach (HitBox hitBox in hitBoxes) {
			if (hitBox.state == 1) return hitBox;
		}
		return null;
	}
	
	/// <summary>
	/// 按身体部位隐藏/显示指定无敌判定盒列表。
	/// </summary>
	/// <param name="invincibleHitBoxes">无敌判定盒列表。</param>
	/// <param name="hide">是否隐藏。</param>
	public void HideHitBoxes(HitBox[] invincibleHitBoxes, bool hide){
		foreach (HitBox invHitBox in invincibleHitBoxes)
        {
            foreach (HitBox hitBox in hitBoxes)
            {
                if (invHitBox.bodyPart == hitBox.bodyPart)
                {
                    hitBox.hide = hide;
                    break;
                }
            }
		}
	}
	
	/// <summary>
	/// 隐藏/显示全部判定盒。
	/// </summary>
	/// <param name="hide">是否隐藏。</param>
	public void HideHitBoxes(bool hide){
		foreach (HitBox hitBox in hitBoxes) {
			hitBox.hide = hide;
		}
	}

	/// <summary>
	/// 镜像左右对称身体部位的判定盒 Transform（交换左右绑定）。
	/// </summary>
	/// <param name="mirror">目标朝向（true 表示需要镜像）。</param>
	public void InvertHitBoxes(bool mirror){
		if (currentMirror == mirror) return;
		currentMirror = mirror;

		foreach (HitBox hitBox in hitBoxes) {
			foreach (HitBox hitBox2 in hitBoxes) {
				if ((hitBox.bodyPart == BodyPart.leftCalf && hitBox2.bodyPart == BodyPart.rightCalf) ||
				    (hitBox.bodyPart == BodyPart.leftFoot && hitBox2.bodyPart == BodyPart.rightFoot) ||
				    (hitBox.bodyPart == BodyPart.leftForearm && hitBox2.bodyPart == BodyPart.rightForearm) ||
				    (hitBox.bodyPart == BodyPart.leftHand && hitBox2.bodyPart == BodyPart.rightHand) ||
				    (hitBox.bodyPart == BodyPart.leftThigh && hitBox2.bodyPart == BodyPart.rightThigh) ||
				    (hitBox.bodyPart == BodyPart.leftUpperArm && hitBox2.bodyPart == BodyPart.rightUpperArm)) 
					invertTransform(hitBox, hitBox2);
			}
		}
	}
	
	/// <summary>
	/// 交换两个判定盒的 Transform（左右镜像用）。
	/// </summary>
	/// <param name="hb1">判定盒1。</param>
	/// <param name="hb2">判定盒2。</param>
	private void invertTransform(HitBox hb1, HitBox hb2){
		Transform hb2Transform = hb2.position;
		hb2.position = hb1.position;
		hb1.position = hb2Transform;
	}
	
	/// <summary>
	/// 在子物体中查找指定名称的 Transform（支持多种命名前缀：mixamorig:/角色名:/无前缀）。
	/// </summary>
	/// <param name="searchString">骨骼名称。</param>
	/// <returns>找到的 Transform；未找到返回 null。</returns>
	public Transform FindTransform(string searchString){
		Transform[] transformChildren = GetComponentsInChildren<Transform>();
		Transform found;
		foreach(Transform transformChild in transformChildren){
			found = transformChild.Find("mixamorig:"+ searchString);
			if (found == null) found = transformChild.Find(gameObject.name + ":" + searchString);
			if (found == null) found = transformChild.Find(searchString);
			if (found != null) return found;
		}
		return null;
	}

	
	/// <summary>
	/// 获取角色渲染器的包围盒。
	/// </summary>
	/// <returns>渲染器边界 Rect（无渲染器返回空 Rect）。</returns>
	public Rect GetBounds(){
		if (characterRenderer != null){
			return new Rect(characterRenderer.bounds.min.x, 
		    	            characterRenderer.bounds.min.y, 
		        	        characterRenderer.bounds.max.x,
		            	    characterRenderer.bounds.max.y);
		}else{
			// alternative bounds
		}

		return new Rect();
	}
	
	/// <summary>
	/// 更新需要跟随渲染边界的受击盒的渲染边界数据。
	/// </summary>
	/// <param name="hurtBoxes">受击盒列表。</param>
	public void UpdateBounds(HurtBox[] hurtBoxes){
		foreach(HurtBox hurtBox in hurtBoxes) if (hurtBox.followXBounds || hurtBox.followYBounds) hurtBox.rendererBounds = GetBounds();
	}

	/// <summary>
	/// 按动画帧更新判定盒映射位置：使用动画映射数据（含镜像处理）或实时从 Transform 计算。
	/// </summary>
	/// <param name="frame">当前动画帧。</param>
    public void UpdateMap(int frame)
    {
        if (controlsScript == null) return;
        if (animationMaps == null && controlsScript.myInfo.useAnimationMaps){
            Debug.LogError("Animation '" + moveSetScript.GetCurrentClipName() + "' has no animation maps");
            return;
        }
        if (controlsScript.myInfo.useAnimationMaps)
        {
        HitBoxMap[] hitBoxMaps = new HitBoxMap[0];
        int highestFrame = 0;
        foreach (AnimationMap map in animationMaps) {
            if (map.frame > highestFrame) highestFrame = map.frame;
            if (map.frame == frame) {
                hitBoxMaps = map.hitBoxMaps;
                deltaPosition = map.deltaDisplacement;
                break;
            }
        }

        // If frame can't be found, cast the highest possible frame
        if (hitBoxMaps.Length == 0) {
            hitBoxMaps = animationMaps[highestFrame].hitBoxMaps;
            deltaPosition = animationMaps[highestFrame].deltaDisplacement;
        }
        

        foreach(HitBoxMap map in hitBoxMaps) {
            foreach (HitBox hitBox in hitBoxes) {
                if (hitBox.bodyPart == map.bodyPart) {
                    hitBox.mappedPosition = map.mappedPosition;
                        if (currentMirror) hitBox.mappedPosition.x += (hitBox.mappedPosition.x * -2);
                    }
                }
            }
        }
        else
        {
            foreach (HitBox hitBox in hitBoxes) {
                hitBox.mappedPosition = FPVector.ToFPVector(hitBox.position.position) - worldTransform.position;
            }
        }
    }

	/// <summary>
	/// 更新角色渲染器引用（当有判定盒需要跟随渲染边界时）。
	/// </summary>
	public void UpdateRenderer(){
		bool confirmUpdate = false;
		foreach(HitBox hitBox in hitBoxes){
			if (hitBox.followXBounds || hitBox.followYBounds) confirmUpdate = true;
		}

		if (moveSetScript != null){
            foreach (MoveInfo move in moveSetScript.moves) {
                if (move == null) {
                    Debug.LogWarning("You have empty entries in your move list. Check your special moves under Character Editor.");
                    continue;
                }
				foreach(Hit hit in move.hits){
					foreach(HurtBox hurtbox in hit.hurtBoxes){
						if (hurtbox.followXBounds || hurtbox.followYBounds) confirmUpdate = true;
					}
				}

				if (move.blockableArea != null && (move.blockableArea.followXBounds || move.blockableArea.followYBounds))
					confirmUpdate = true;
			}
		}

		if (confirmUpdate){
			Renderer[] rendererList = GetComponentsInChildren<Renderer>();
			foreach(Renderer childRenderer in rendererList){
				characterRenderer = childRenderer;
				return;
			}
			Debug.LogWarning("Warning: You are trying to access the character's bounds, but it does not have a renderer.");
		}
	}

	/// <summary>
	/// Gizmos 辅助：绘制矩形边框。
	/// </summary>
	/// <param name="topLeft">左上角。</param>
	/// <param name="bottomLeft">左下角。</param>
	/// <param name="bottomRight">右下角。</param>
	/// <param name="topRight">右上角。</param>
	private void GizmosDrawRectangle(Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight){
		Gizmos.DrawLine(topLeft, bottomLeft);
		Gizmos.DrawLine(bottomLeft, bottomRight);
		Gizmos.DrawLine(bottomRight, topRight);
		Gizmos.DrawLine(topRight, topLeft);
	}

	/// <summary>
	/// 编辑器 Gizmos 可视化：绘制攻击判定盒（HitBox）、受击判定盒（HurtBox）与可格挡区域（BlockArea）的形状。
	/// <para>颜色约定：命中=红、已命中=洋红、身体碰撞体=黄、无碰撞=白、投技=粉、受击盒=青、格挡区域=蓝。</para>
	/// </summary>
	void OnDrawGizmos() {
		// HITBOXES
		if (hitBoxes == null) return;
		int mirrorAdjust = controlsScript != null? controlsScript.mirror : -1;
        Vector3 rootPosition = controlsScript != null ? worldTransform.position.ToVector() : transform.position;


        foreach (HitBox hitBox in hitBoxes) {
			if (hitBox.position == null) continue;
			if (hitBox.hide) continue;
			if (hitBox.state == 1) {
				Gizmos.color = Color.red;
			} else if (isHit){
				Gizmos.color = Color.magenta;
			} else if (hitBox.collisionType == CollisionType.bodyCollider) {	
				Gizmos.color = Color.yellow;
			} else if (hitBox.collisionType == CollisionType.noCollider) {	
				Gizmos.color = Color.white;
			} else if (hitBox.collisionType == CollisionType.throwCollider) {	
				Gizmos.color = new Color(1f, 0, .5f);
			}else{
				Gizmos.color = Color.green;
			}
            
            Vector3 currentPosition = hitBox.mappedPosition.ToVector() + rootPosition;
            if (controlsScript == null) currentPosition = hitBox.position.position;

            if (hitBox.shape == HitBoxShape.rectangle && rectangleHitBoxLocationTest) {
                Rect hitBoxRectPos = new Rect(hitBox.rect);
                hitBoxRectPos.x *= -mirrorAdjust;
                hitBoxRectPos.width *= -mirrorAdjust;

                //Vector3 currentPosition = hitBox.position.position;
                //if (myMoveSetScript.GetCurrentClipName) currentAnimationMap.ContainKey(myMoveSetScript.GetCurrentClipFrame()) currentPosition = currentAnimationMap[myMoveSetScript.GetCurrentClipFrame()];

                hitBoxRectPos.x += currentPosition.x;
                hitBoxRectPos.y += currentPosition.y;
                Gizmos.DrawGUITexture(hitBoxRectPos, rectTexture);
            }


			Vector3 hitBoxPosition = currentPosition + new Vector3((float)hitBox._offSet.x, (float)hitBox._offSet.y, 0);
			if (UFE.config == null || !UFE.config.detect3D_Hits) hitBoxPosition.z = -1;
			if (hitBox.shape == HitBoxShape.circle && hitBox._radius > 0){
				Gizmos.DrawWireSphere(hitBoxPosition, (float)hitBox._radius);
			}else if (hitBox.shape == HitBoxShape.rectangle){

				/*hitBoxPosition = hitBox.position.position;
				Vector3 topLeft = new Vector3(hitBox.rect.x * -mirrorAdjust, hitBox.rect.y) + hitBoxPosition;
				Vector3 topRight = new Vector3((hitBox.rect.x + hitBox.rect.width) * -mirrorAdjust, hitBox.rect.y) + hitBoxPosition;
				Vector3 bottomLeft = new Vector3(hitBox.rect.x * -mirrorAdjust, hitBox.rect.y + hitBox.rect.height) + hitBoxPosition;
				Vector3 bottomRight = new Vector3((hitBox.rect.x + hitBox.rect.width) * -mirrorAdjust, hitBox.rect.y + hitBox.rect.height) + hitBoxPosition;

                Gizmos.color = Color.red;
                GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);*/

                Rect hitBoxRectPosTemp = new Rect(hitBox.rect);
                hitBoxRectPosTemp.x *= -mirrorAdjust;
                hitBoxRectPosTemp.width *= -mirrorAdjust;
                hitBoxRectPosTemp.x += currentPosition.x;
                hitBoxRectPosTemp.y += currentPosition.y;
                Vector3 topLeft = new Vector3(hitBoxRectPosTemp.x, hitBoxRectPosTemp.y);
                Vector3 topRight = new Vector3((hitBoxRectPosTemp.xMax), hitBoxRectPosTemp.y);
                Vector3 bottomLeft = new Vector3(hitBoxRectPosTemp.x, hitBoxRectPosTemp.yMax);
                Vector3 bottomRight = new Vector3((hitBoxRectPosTemp.xMax), hitBoxRectPosTemp.yMax);

				if (hitBox.followXBounds){
					hitBox.rect.x = 0;
					topLeft.x = GetBounds().x - (hitBox.rect.width/2);
					topRight.x = GetBounds().width + (hitBox.rect.width/2);
					bottomLeft.x = GetBounds().x - (hitBox.rect.width/2);
					bottomRight.x = GetBounds().width + (hitBox.rect.width/2);
				}
				
				if (hitBox.followYBounds){
					hitBox.rect.y = 0;
					topLeft.y = GetBounds().height + (hitBox.rect.height/2);
					topRight.y = GetBounds().height + (hitBox.rect.height/2);
					bottomLeft.y = GetBounds().y - (hitBox.rect.height/2);
					bottomRight.y = GetBounds().y - (hitBox.rect.height/2);
				}

				GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
			}
			
			if (hitBox.collisionType != CollisionType.noCollider){
				if (hitBox.type == HitBoxType.low){
					Gizmos.color = Color.red;
				}else{
					Gizmos.color = Color.yellow;
				}
				Gizmos.DrawWireSphere(hitBoxPosition, .1f);
			}
        }

		// COLLISION BOX SIZE
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, (float)collisionBoxSize);


        // HURTBOXES
		if (activeHurtBoxes != null) {
			if (hitConfirmType == HitConfirmType.Throw){
				Gizmos.color = new Color(1f, .5f, 0);
			}else{
				Gizmos.color = Color.cyan;
			}

			foreach (HurtBox hurtBox in activeHurtBoxes) {
				if (GetTransform(hurtBox.bodyPart) != null){
					Vector3 hurtBoxPosition;
					hurtBoxPosition = GetPosition(hurtBox.bodyPart).ToVector();
					if (UFE.config == null || !UFE.config.detect3D_Hits) hurtBoxPosition.z = -1;
					if (hurtBox.shape == HitBoxShape.circle){
						hurtBoxPosition += new Vector3((float)hurtBox._offSet.x * -mirrorAdjust, (float)hurtBox._offSet.y, 0);
						Gizmos.DrawWireSphere(hurtBoxPosition, (float)hurtBox._radius);
					}else{
						Vector3 topLeft = new Vector3(hurtBox.rect.x * -mirrorAdjust, hurtBox.rect.y) + hurtBoxPosition;
						Vector3 topRight = new Vector3((hurtBox.rect.x + hurtBox.rect.width) * -mirrorAdjust, hurtBox.rect.y) + hurtBoxPosition;
						Vector3 bottomLeft = new Vector3(hurtBox.rect.x * -mirrorAdjust, hurtBox.rect.y + hurtBox.rect.height) + hurtBoxPosition;
						Vector3 bottomRight = new Vector3((hurtBox.rect.x + hurtBox.rect.width) * -mirrorAdjust, hurtBox.rect.y + hurtBox.rect.height) + hurtBoxPosition;

						if (hurtBox.followXBounds){
							hurtBox.rect.x = 0;
							topLeft.x = GetBounds().x - (hurtBox.rect.width/2);
							topRight.x = GetBounds().width + (hurtBox.rect.width/2);
							bottomLeft.x = GetBounds().x - (hurtBox.rect.width/2);
							bottomRight.x = GetBounds().width + (hurtBox.rect.width/2);
						}
						
						if (hurtBox.followYBounds){
							hurtBox.rect.y = 0;
							topLeft.y = GetBounds().height + (hurtBox.rect.height/2);
							topRight.y = GetBounds().height + (hurtBox.rect.height/2);
							bottomLeft.y = GetBounds().y - (hurtBox.rect.height/2);
							bottomRight.y = GetBounds().y - (hurtBox.rect.height/2);
						}
						GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
					}
				}
			}
		}
		
		
		// BLOCKBOXES
		if (blockableArea != null){
			Gizmos.color = Color.blue;

			if (GetTransform(blockableArea.bodyPart) != null){
				Vector3 blockableAreaPosition;
				blockableAreaPosition = GetPosition(blockableArea.bodyPart).ToVector();
				if (UFE.config == null || !UFE.config.detect3D_Hits) blockableAreaPosition.z = -1;
				if (blockableArea.shape == HitBoxShape.circle){
					blockableAreaPosition += new Vector3((float)blockableArea._offSet.x * -mirrorAdjust, (float)blockableArea._offSet.y, 0);
					//blockableAreaPosition.x += (blockableArea.radius/2) * -mirrorAdjust;
					Gizmos.DrawWireSphere(blockableAreaPosition, (float)blockableArea._radius);
				}else{
					Vector3 topLeft = new Vector3(blockableArea.rect.x * -mirrorAdjust, blockableArea.rect.y) + blockableAreaPosition;
					Vector3 topRight = new Vector3((blockableArea.rect.x + blockableArea.rect.width) * -mirrorAdjust, blockableArea.rect.y) + blockableAreaPosition;
					Vector3 bottomLeft = new Vector3(blockableArea.rect.x * -mirrorAdjust, blockableArea.rect.y + blockableArea.rect.height) + blockableAreaPosition;
					Vector3 bottomRight = new Vector3((blockableArea.rect.x + blockableArea.rect.width) * -mirrorAdjust, blockableArea.rect.y + blockableArea.rect.height) + blockableAreaPosition;
					GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
				}
			}
		}
    }
}
