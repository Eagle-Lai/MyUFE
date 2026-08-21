using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// 头部注视系统（HeadLookScript）。
/// <para>用途：让角色的头部/颈部骨骼链平滑地转向注视目标（如看向对手），增强角色表现力。</para>
/// <para>通过多段骨骼弯曲（BendingSegment）实现，LateUpdate 中计算水平/垂直角度并分发旋转到各关节。</para>
/// </summary>

/// <summary>
/// 弯曲骨骼段：头部注视中一段可弯曲的骨骼链（如 颈部→头部）。
/// </summary>
[System.Serializable]
public class BendingSegment: ICloneable {
	/// <summary>骨骼链起始关节。</summary>
	public Transform firstTransform;
	/// <summary>骨骼链末端关节。</summary>
	public Transform lastTransform;
	/// <summary>绑定的身体部位。</summary>
	public BodyPart bodyPart = BodyPart.head;
	/// <summary>角度差阈值（低于该角度不弯曲）。</summary>
	public float thresholdAngleDifference = 0;
	/// <summary>弯曲倍率（0~1，越小越不明显）。</summary>
	public float bendingMultiplier = 0.7f;
	/// <summary>最大角度差（超过部分不再增加弯曲）。</summary>
	public float maxAngleDifference = 30;
	/// <summary>最大弯曲角度。</summary>
	public float maxBendingAngle = 80;
	/// <summary>响应速度（角度插值速度）。</summary>
	public float responsiveness = 4;
	/// <summary>水平角度（内部计算）。</summary>
	internal float angleH;
	/// <summary>垂直角度（内部计算）。</summary>
	internal float angleV;
	/// <summary>当前朝上方向（内部计算）。</summary>
	internal Vector3 dirUp;
	/// <summary>参考注视方向（内部计算）。</summary>
	internal Vector3 referenceLookDir;
	/// <summary>参考朝上方向（内部计算）。</summary>
	internal Vector3 referenceUpDir;
	/// <summary>骨骼链长度（关节数）。</summary>
	internal int chainLength;
	/// <summary>各关节原始旋转（用于恢复动画）。</summary>
	internal Quaternion[] origRotations;

	/// <summary>默认构造函数。</summary>
	public BendingSegment(){}

	/// <summary>
	/// 拷贝构造函数：复制所有参数与原始旋转数组。
	/// </summary>
	/// <param name="other">源段。</param>
	public BendingSegment(BendingSegment other){
		this.firstTransform = other.firstTransform;
		this.lastTransform = other.lastTransform;
		this.bodyPart = other.bodyPart;
		this.thresholdAngleDifference = other.thresholdAngleDifference;
		this.bendingMultiplier = other.bendingMultiplier;
		this.maxAngleDifference = other.maxAngleDifference;
		this.maxBendingAngle = other.maxBendingAngle;
		this.responsiveness = other.responsiveness;
		this.angleH = other.angleH;
		this.angleV = other.angleV;
		this.dirUp = other.dirUp;
		this.referenceLookDir = other.referenceLookDir;
		this.referenceUpDir = other.referenceUpDir;
		this.chainLength = other.chainLength;
		this.origRotations = new Quaternion[other.origRotations.Length];

		for (int i = 0; i < this.origRotations.Length; ++i){
			this.origRotations[i] = other.origRotations[i];
		}
	}

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 不受影响的关节：头部注视中不参与弯曲（或部分影响）的关节。
/// </summary>
[System.Serializable]
public class NonAffectedJoints: ICloneable {
	/// <summary>关节 Transform。</summary>
	public Transform joint;
	/// <summary>绑定的身体部位。</summary>
	public BodyPart bodyPart;
	/// <summary>影响系数（0=完全不受影响，1=完全跟随）。</summary>
	public float effect = 0;

	/// <summary>默认构造函数。</summary>
	public NonAffectedJoints(){}

	/// <summary>
	/// 拷贝构造函数。
	/// </summary>
	/// <param name="other">源对象。</param>
	public NonAffectedJoints(NonAffectedJoints other){
		this.joint = other.joint;
		this.bodyPart = other.bodyPart;
		this.effect = other.effect;
	}

	/// <summary>
	/// 深拷贝当前对象（ICloneable 实现）。
	/// </summary>
	/// <returns>克隆出的新对象实例。</returns>
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

/// <summary>
/// 头部注视脚本（HeadLookScript）。
/// <para>在 LateUpdate 中根据目标位置计算各骨骼段的水平/垂直弯曲角度，并将旋转平滑分发到骨骼链关节。</para>
/// <para>支持覆盖动画（overrideAnimation）与不受影响关节的修正。</para>
/// </summary>
public class HeadLookScript : MonoBehaviour {
	
	/// <summary>根节点（用于参考方向）。</summary>
	public Transform rootNode;
	/// <summary>弯曲骨骼段列表。</summary>
	public BendingSegment[] segments;
	/// <summary>不受影响的关节列表。</summary>
	public NonAffectedJoints[] nonAffectedJoints;
	/// <summary>头部注视向量（局部空间前方向）。</summary>
	public Vector3 headLookVector = Vector3.forward;
	/// <summary>头部朝上向量（局部空间上方向）。</summary>
	public Vector3 headUpVector = Vector3.up;
	/// <summary>注视目标的世界位置。</summary>
	public Vector3 target = Vector3.zero;
	/// <summary>注视效果强度（0~1）。</summary>
	public float effect = 1;
	/// <summary>是否覆盖动画（强制执行头部旋转）。</summary>
	public bool overrideAnimation = false;
	
	/// <summary>
	/// 启动：初始化各骨骼段的参考方向、链长度与原始旋转。
	/// </summary>
	void Start () {
		if (rootNode == null) {
			rootNode = transform;
		}
		
		// Setup segments
		foreach (BendingSegment segment in segments) {
			Quaternion parentRot = segment.firstTransform.parent.rotation;
			Quaternion parentRotInv = Quaternion.Inverse(parentRot);
			segment.referenceLookDir =
				parentRotInv * rootNode.rotation * headLookVector.normalized;
			segment.referenceUpDir =
				parentRotInv * rootNode.rotation * headUpVector.normalized;
			segment.angleH = 0;
			segment.angleV = 0;
			segment.dirUp = segment.referenceUpDir;
			
			segment.chainLength = 1;
			Transform t = segment.lastTransform;
			while (t != segment.firstTransform && t != t.root) {
				segment.chainLength++;
				t = t.parent;
			}
			
			segment.origRotations = new Quaternion[segment.chainLength];
			t = segment.lastTransform;
			for (int i=segment.chainLength-1; i>=0; i--) {
				segment.origRotations[i] = t.localRotation;
				t = t.parent;
			}
		}
	}
	
	/// <summary>
	/// 每帧末尾更新：计算并应用头部注视旋转。
	/// <para>对每段骨骼计算水平/垂直角度（含阈值/倍率/最大角度限制），平滑插值后分发到链上所有关节；</para>
	/// <para>最后按影响系数修正不受影响关节的方向。</para>
	/// </summary>
	void LateUpdate () {
		if (Time.deltaTime == 0)
			return;
		
		// Remember initial directions of joints that should not be affected
		Vector3[] jointDirections = new Vector3[nonAffectedJoints.Length];
		for (int i=0; i<nonAffectedJoints.Length; i++) {
			foreach (Transform child in nonAffectedJoints[i].joint) {
				jointDirections[i] = child.position - nonAffectedJoints[i].joint.position;
				break;
			}
		}
		
		// Handle each segment
		foreach (BendingSegment segment in segments) {
			Transform t = segment.lastTransform;
			if (overrideAnimation) {
				for (int i=segment.chainLength-1; i>=0; i--) {
					t.localRotation = segment.origRotations[i];
					t = t.parent;
				}
			}
			
			Quaternion parentRot = segment.firstTransform.parent.rotation;
			Quaternion parentRotInv = Quaternion.Inverse(parentRot);
			
			// Desired look direction in world space
			Vector3 lookDirWorld = (target - segment.lastTransform.position).normalized;
			
			// Desired look directions in neck parent space
			Vector3 lookDirGoal = (parentRotInv * lookDirWorld);
			
			// Get the horizontal and vertical rotation angle to look at the target
			float hAngle = AngleAroundAxis(
				segment.referenceLookDir, lookDirGoal, segment.referenceUpDir
			);
			
			Vector3 rightOfTarget = Vector3.Cross(segment.referenceUpDir, lookDirGoal);
			
			Vector3 lookDirGoalinHPlane =
				lookDirGoal - Vector3.Project(lookDirGoal, segment.referenceUpDir);
			
			float vAngle = AngleAroundAxis(
				lookDirGoalinHPlane, lookDirGoal, rightOfTarget
			);
			
			// Handle threshold angle difference, bending multiplier,
			// and max angle difference here
			float hAngleThr = Mathf.Max(
				0, Mathf.Abs(hAngle) - segment.thresholdAngleDifference
			) * Mathf.Sign(hAngle);
			
			float vAngleThr = Mathf.Max(
				0, Mathf.Abs(vAngle) - segment.thresholdAngleDifference
			) * Mathf.Sign(vAngle);
			
			hAngle = Mathf.Max(
				Mathf.Abs(hAngleThr) * Mathf.Abs(segment.bendingMultiplier),
				Mathf.Abs(hAngle) - segment.maxAngleDifference
			) * Mathf.Sign(hAngle) * Mathf.Sign(segment.bendingMultiplier);
			
			vAngle = Mathf.Max(
				Mathf.Abs(vAngleThr) * Mathf.Abs(segment.bendingMultiplier),
				Mathf.Abs(vAngle) - segment.maxAngleDifference
			) * Mathf.Sign(vAngle) * Mathf.Sign(segment.bendingMultiplier);
			
			// Handle max bending angle here
			hAngle = Mathf.Clamp(hAngle, -segment.maxBendingAngle, segment.maxBendingAngle);
			vAngle = Mathf.Clamp(vAngle, -segment.maxBendingAngle, segment.maxBendingAngle);
			
			Vector3 referenceRightDir =
				Vector3.Cross(segment.referenceUpDir, segment.referenceLookDir);
			
			// Lerp angles
			segment.angleH = Mathf.Lerp(
				segment.angleH, hAngle, Time.deltaTime * segment.responsiveness
			);
			segment.angleV = Mathf.Lerp(
				segment.angleV, vAngle, Time.deltaTime * segment.responsiveness
			);
			
			// Get direction
			lookDirGoal = Quaternion.AngleAxis(segment.angleH, segment.referenceUpDir)
				* Quaternion.AngleAxis(segment.angleV, referenceRightDir)
				* segment.referenceLookDir;
			
			// Make look and up perpendicular
			Vector3 upDirGoal = segment.referenceUpDir;
			Vector3.OrthoNormalize(ref lookDirGoal, ref upDirGoal);
			
			// Interpolated look and up directions in neck parent space
			Vector3 lookDir = lookDirGoal;
			segment.dirUp = Vector3.Slerp(segment.dirUp, upDirGoal, Time.deltaTime*5);
			Vector3.OrthoNormalize(ref lookDir, ref segment.dirUp);
			
			// Look rotation in world space
			Quaternion lookRot = (
				(parentRot * Quaternion.LookRotation(lookDir, segment.dirUp))
				* Quaternion.Inverse(
					parentRot * Quaternion.LookRotation(
						segment.referenceLookDir, segment.referenceUpDir
					)
				)
			);
			
			// Distribute rotation over all joints in segment
			Quaternion dividedRotation =
				Quaternion.Slerp(Quaternion.identity, lookRot, effect / segment.chainLength);
			t = segment.lastTransform;
			for (int i=0; i<segment.chainLength; i++) {
				t.rotation = dividedRotation * t.rotation;
				t = t.parent;
			}
		}
		
		// Handle non affected joints
		for (int i=0; i<nonAffectedJoints.Length; i++) {
			Vector3 newJointDirection = Vector3.zero;
			
			foreach (Transform child in nonAffectedJoints[i].joint) {
				newJointDirection = child.position - nonAffectedJoints[i].joint.position;
				break;
			}
			
			Vector3 combinedJointDirection = Vector3.Slerp(
				jointDirections[i], newJointDirection, nonAffectedJoints[i].effect
			);
			
			nonAffectedJoints[i].joint.rotation = Quaternion.FromToRotation(
				newJointDirection, combinedJointDirection
			) * nonAffectedJoints[i].joint.rotation;
		}
	}
	
	// The angle between dirA and dirB around axis
	/// <summary>
	/// 计算 dirA 到 dirB 绕指定轴的有符号角度。
	/// </summary>
	/// <param name="dirA">方向 A。</param>
	/// <param name="dirB">方向 B。</param>
	/// <param name="axis">旋转轴。</param>
	/// <returns>有符号角度（绕轴正方向为正）。</returns>
	public static float AngleAroundAxis (Vector3 dirA, Vector3 dirB, Vector3 axis) {
		// Project A and B onto the plane orthogonal target axis
		dirA = dirA - Vector3.Project(dirA, axis);
		dirB = dirB - Vector3.Project(dirB, axis);
		
		// Find (positive) angle between A and B
		float angle = Vector3.Angle(dirA, dirB);
		
		// Return angle multiplied with 1 or -1
		return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
	}
}
