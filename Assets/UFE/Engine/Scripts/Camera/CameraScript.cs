using UnityEngine;
using System.Collections;

/// <summary>
/// 战斗摄像机脚本（CameraScript）。
/// <para>用途：控制对战摄像机的跟随、缩放、旋转、注视（LookAt）、电影化演出移动与速度覆盖。</para>
/// <para>支持两种模式：自由摄像机（freeCamera，演出用）与自动跟随模式（默认，以两角色中点为中心）。</para>
/// </summary>
public class CameraScript : MonoBehaviour {

    #region trackable definitions
	/// <summary>电影化演出期间是否冻结摄像机。</summary>
    public bool cinematicFreeze;
	/// <summary>当前注视位置。</summary>
    public Vector3 currentLookAtPosition;
	/// <summary>自由摄像机移动速度。</summary>
    public float freeCameraSpeed;
	/// <summary>最后持有摄像机控制权的角色名。</summary>
    public string lastOwner;
	/// <summary>是否停止摄像机移动（演出用）。</summary>
    public bool killCamMove;
	/// <summary>摄像机移动速度（当前值）。</summary>
    public float movementSpeed;
	/// <summary>摄像机旋转速度（当前值）。</summary>
    public float rotationSpeed;
	/// <summary>标准距离（两角色初始距离）。</summary>
    public float standardDistance;
	/// <summary>标准地面高度。</summary>
    public float standardGroundHeight;
	/// <summary>目标位置（自由模式）。</summary>
    public Vector3 targetPosition;
	/// <summary>目标旋转（自由模式）。</summary>
    public Quaternion targetRotation;
	/// <summary>目标视野（自由模式）。</summary>
    public float targetFieldOfView;
    #endregion


	/// <summary>玩家灯光（演出用）。</summary>
    public GameObject playerLight;
	/// <summary>玩家1根 Transform。</summary>
    public Transform player1;
	/// <summary>玩家2根 Transform。</summary>
	public Transform player2;
	
	/// <summary>
	/// 启动：查找玩家灯光与双方角色，初始化标准距离/速度并重置摄像机。
	/// </summary>
	void Start(){
		playerLight = GameObject.Find("Player Light");
		player1 = GameObject.Find("Player1").transform;
		player2 = GameObject.Find("Player2").transform;

		ResetCam();
		//standardZoom = UFE.config.cameraOptions.initialDistance.z;
        standardDistance = Vector3.Distance(player1.position, player2.position);
        movementSpeed = UFE.config.cameraOptions.movementSpeed;
        rotationSpeed = UFE.config.cameraOptions.rotationSpeed;
        UFE.freeCamera = false;

	}

	/// <summary>
	/// 重置摄像机为配置的初始位置/旋转/视野。
	/// </summary>
	public void ResetCam(){
		Camera.main.transform.localPosition = UFE.config.cameraOptions.initialDistance;
		Camera.main.transform.position = UFE.config.cameraOptions.initialDistance;
		Camera.main.transform.localRotation = Quaternion.Euler(UFE.config.cameraOptions.initialRotation);
		Camera.main.fieldOfView = UFE.config.cameraOptions.initialFieldOfView;
		//standardGroundHeight = Camera.main.transform.position.y;

	}

	/// <summary>
	/// 按距离线性插值移动（向 B 方向移动 speed 比例）。
	/// </summary>
	/// <param name="A">起点。</param>
	/// <param name="B">目标点。</param>
	/// <param name="speed">移动速度。</param>
	/// <returns>插值后的位置。</returns>
	public Vector3 LerpByDistance(Vector3 A, Vector3 B, float speed){
		Vector3 P = speed * (float)UFE.fixedDeltaTime * Vector3.Normalize(B - A) + A;
		return P;
	}

	/// <summary>
	/// 摄像机固定帧更新：自由模式向目标位置/旋转/视野平滑移动；
	/// 默认模式以两角色中点为中心自动跟随并缩放（按距离调整 Z 轴），支持注视与运动传感器/鼠标控制。
	/// </summary>
	public void DoFixedUpdate() {
		if (killCamMove) return;
		if (UFE.freeCamera) {
			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFieldOfView, (float)UFE.fixedDeltaTime * freeCameraSpeed * 1.8f);
			Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, targetPosition, (float)UFE.fixedDeltaTime * freeCameraSpeed * 1.8f);
			Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, targetRotation, (float)UFE.fixedDeltaTime * freeCameraSpeed * 1.8f);

		}else{
			Vector3 newPosition = ((player1.position + player2.position)/2) + UFE.config.cameraOptions.initialDistance;
			if (UFE.config.cameraOptions.followJumpingCharacter) 
				newPosition.y += Mathf.Abs(player1.position.y - player2.position.y)/2;

			newPosition.x = Mathf.Clamp(newPosition.x, 
				(float)UFE.config.selectedStage._leftBoundary + 8,
                (float)UFE.config.selectedStage._rightBoundary - 8);

            newPosition.z = UFE.config.cameraOptions.initialDistance.z - Vector3.Distance(player1.position, player2.position) + standardDistance;
			newPosition.z = Mathf.Clamp(newPosition.z, -UFE.config.cameraOptions.maxZoom, -UFE.config.cameraOptions.minZoom);

            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, UFE.config.cameraOptions.initialFieldOfView, (float)UFE.fixedDeltaTime * movementSpeed);
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newPosition, (float)UFE.fixedDeltaTime * movementSpeed);
			Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, Quaternion.Euler(UFE.config.cameraOptions.initialRotation), (float)UFE.fixedDeltaTime * UFE.config.cameraOptions.movementSpeed);

			if (Camera.main.transform.localRotation == Quaternion.Euler(UFE.config.cameraOptions.initialRotation))
				UFE.normalizedCam = true;

			if (playerLight != null) playerLight.GetComponent<Light>().enabled = false;

			if (UFE.config.cameraOptions.enableLookAt) {
                //Vector3 lookAtPosition = ((player1.position + player2.position)/2);
                //lookAtPosition.y += UFE.config.cameraOptions.heightOffSet;

                Vector3 newLookAtPosition = ((player1.position + player2.position) / 2) + UFE.config.cameraOptions.rotationOffSet;

                if (UFE.config.cameraOptions.motionSensor != MotionSensor.None) {
                    Vector3 acceleration = Input.acceleration;
                    if (UFE.config.cameraOptions.motionSensor == MotionSensor.Gyroscope && SystemInfo.supportsGyroscope) acceleration = Input.gyro.gravity;

#if UNITY_STANDALONE || UNITY_EDITOR
                    if (Input.mousePresent) {
                        Vector3 mouseXY = new Vector3(Input.mousePosition.x - Screen.width / 2, Input.mousePosition.y - Screen.height / 2, 0);
                        acceleration = mouseXY / 1000;
                    }
#endif
                    acceleration *= UFE.config.cameraOptions.motionSensibility;
                    newLookAtPosition -= acceleration;

                    newPosition.y += acceleration.y;
                    Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newPosition, (float)UFE.fixedDeltaTime * movementSpeed);
                }

                currentLookAtPosition = Vector3.Lerp(currentLookAtPosition,
                    newLookAtPosition, 
                    (float)UFE.fixedDeltaTime * rotationSpeed);


                Camera.main.transform.LookAt(currentLookAtPosition, Vector3.up);
			}

		}
	}

	/// <summary>
	/// 移动摄像机到指定位置/旋转/视野（电影化演出用，切换为自由模式并记录控制者）。
	/// </summary>
	/// <param name="targetPos">目标位置。</param>
	/// <param name="targetRot">目标旋转（欧拉角）。</param>
	/// <param name="targetFOV">目标视野。</param>
	/// <param name="speed">移动速度。</param>
	/// <param name="owner">控制者角色名。</param>
	public void MoveCameraToLocation(Vector3 targetPos, Vector3 targetRot, float targetFOV, float speed, string owner){
		targetFieldOfView = targetFOV;
		targetPosition = targetPos;
		targetRotation = Quaternion.Euler(targetRot);
		freeCameraSpeed = speed;
		UFE.freeCamera = true;
		UFE.normalizedCam = false;
		lastOwner = owner;
		if (playerLight != null) playerLight.GetComponent<Light>().enabled = true;
	}
	
	/// <summary>
	/// 禁用摄像机。
	/// </summary>
	public void DisableCam(){
		Camera.main.enabled = false;
	}

	/// <summary>
	/// 释放摄像机：恢复启用、退出自由模式并清空控制者。
	/// </summary>
	public void ReleaseCam(){
		Camera.main.enabled = true;
		cinematicFreeze = false;
		UFE.freeCamera = false;
		lastOwner = "";
	}

	/// <summary>
	/// 临时覆盖摄像机移动/旋转速度（演出用）。
	/// </summary>
	/// <param name="newMovement">新移动速度。</param>
	/// <param name="newRotation">新旋转速度。</param>
    public void OverrideSpeed(float newMovement, float newRotation) {
        movementSpeed = newMovement;
        rotationSpeed = newRotation;
    }

	/// <summary>
	/// 恢复摄像机默认移动/旋转速度。
	/// </summary>
    public void RestoreSpeed() {
        movementSpeed = UFE.config.cameraOptions.movementSpeed;
        rotationSpeed = UFE.config.cameraOptions.rotationSpeed;
    }

	/// <summary>
	/// 设置摄像机控制者。
	/// </summary>
	/// <param name="owner">控制者角色名。</param>
	public void SetCameraOwner(string owner){
		lastOwner = owner;
	}

	/// <summary>
	/// 获取当前摄像机控制者。
	/// </summary>
	/// <returns>控制者角色名。</returns>
	public string GetCameraOwner(){
		return lastOwner;
	}

	/// <summary>
	/// 计算某位置相对于指定原点的本地坐标（投影到原点的右/上/前轴）。
	/// </summary>
	/// <param name="origin">参考原点。</param>
	/// <param name="position">目标位置。</param>
	/// <returns>相对坐标。</returns>
	public Vector3 GetRelativePosition(Transform origin, Vector3 position) {
		Vector3 distance = position - origin.position;
		Vector3 relativePosition = Vector3.zero;
		relativePosition.x = Vector3.Dot(distance, origin.right.normalized);
		relativePosition.y = Vector3.Dot(distance, origin.up.normalized);
		relativePosition.z = Vector3.Dot(distance, origin.forward.normalized);
		
		return relativePosition;
	}

	/// <summary>
	/// 编辑器 Gizmos：绘制摄像机视野左右边界线（按最大距离）。
	/// </summary>
    void OnDrawGizmos() {
        Vector3 cameraLeftBounds = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z));
        Vector3 cameraRightBounds = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, -Camera.main.transform.position.z));

        cameraLeftBounds.x = Camera.main.transform.position.x - ((float)UFE.config.cameraOptions._maxDistance / 2);
        cameraRightBounds.x = Camera.main.transform.position.x + ((float)UFE.config.cameraOptions._maxDistance / 2);

        Gizmos.DrawLine(cameraLeftBounds, cameraLeftBounds + new Vector3(0, 15, 0));
        Gizmos.DrawLine(cameraRightBounds, cameraRightBounds + new Vector3(0, 15, 0));
        //Gizmos.DrawWireSphere(cameraRightBounds, 1);
    }
}
