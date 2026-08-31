using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
namespace MyScripts
{
    public class CreateTestDemoAnimators
    {
        // 玩家（Ethan）使用的动画片段
        const string PlayerIdleClip = "Assets/UFE/Demo/Characters/Ethan/Animations/E_Basic_Idle.anim";
        const string PlayerWalkClip = "Assets/UFE/Demo/Characters/Ethan/Animations/E_Basic_Walk_Forward.anim";
        const string PlayerAttackClip = "Assets/UFE/Demo/Characters/Ethan/Animations/E_Stand_N1.anim";
        const string PlayerHitClip = "Assets/UFE/Demo/Characters/Ethan/Animations/E_Basic_Hit_High_weak.anim";
        const string PlayerDeathClip = "Assets/UFE/Demo/Characters/Ethan/Animations/E_Basic_Fall_Back.anim";

        // 敌人（Robot_Kyle）使用的动画片段
        const string EnemyIdleClip = "Assets/UFE/Demo/Characters/Robot_Kyle/Animations/IdleStanding.anim";
        const string EnemyWalkClip = "Assets/UFE/Demo/Characters/Robot_Kyle/Animations/MoveForward.anim";
        const string EnemyAttackClip = "Assets/UFE/Demo/Characters/Robot_Kyle/Animations/PunchStandingLight.anim";
        const string EnemyHitClip = "Assets/UFE/Demo/Characters/Robot_Kyle/Animations/HitStandingLight.anim";
        const string EnemyDeathClip = "Assets/UFE/Demo/Characters/Robot_Kyle/Animations/FallDown.anim";

        // 生成路径（放在 TestDemo 目录，与场景放一起方便管理；两个版本的脚本共用此路径）
        const string PlayerControllerPath = "Assets/TestDemo/PlayerAnimator.controller";
        const string EnemyControllerPath = "Assets/TestDemo/EnemyAnimator.controller";
        [MenuItem("TestDemo/My/Create Animator Controllers")]
        public static void CreateAll()
        {
            bool exists = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerControllerPath) != null || AssetDatabase.LoadAssetAtPath<AnimatorController>(EnemyControllerPath) != null;
            if (exists) 
            {
                if(!EditorUtility.DisplayDialog("生成动画控制器", "" +
                     "已存在 PlayerAnimator / EnemyAnimator，是否覆盖重新生成？\n覆盖后需重新执行 Apply Animators To Scene 恢复场景引用。",
                     "覆盖", "取消"))
                {
                    return;
                }
            }

            CreateController(PlayerControllerPath,
                PlayerIdleClip, PlayerWalkClip, PlayerAttackClip, PlayerHitClip, PlayerDeathClip);
            CreateController(EnemyControllerPath,
                EnemyIdleClip, EnemyWalkClip, EnemyAttackClip, EnemyHitClip, EnemyDeathClip);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("动画控制器生成完成！");
        }

        private static void CreateController(string path, string idleClip, string walkClip, string attackClip, string hitClip, string deathClip)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            var root = controller.layers[0].stateMachine;

            var idleState = CreateState(root, "Idle", idleClip, new Vector3(220, 120, 0));
            var walkState = root.AddState("Walk");
            SetStatePosition(root, walkState, new Vector3(220, 300, 0));
            walkState.motion = CreateWalkBlendTree(controller, idleClip, walkClip);
            var attackState = CreateState(root, "Attack", attackClip, new Vector3(460, 120, 0));
            var hitState = CreateState(root, "Hit", hitClip, new Vector3(460, 300, 0));
            var deathState = CreateState(root, "Death", deathClip, new Vector3(700, 120, 0));

            root.defaultState = idleState;

            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.15f;
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.15f;
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            // ---- Attack / Hit：一次性动作，播完按是否在移动回 Idle 或 Walk ----
            SetupExitTransition(attackState, idleState, 0.9f);
            SetupExitTransition(attackState, walkState, 0.9f, true);
            SetupExitTransition(hitState, idleState, 0.8f);
            SetupExitTransition(hitState, walkState, 0.8f, true);

            // ---- Any State → Attack / Hit / Death：随时可打断当前动作 ----
            SetupAnyStateTransition(root, attackState, "Attack", 0.05f);
            SetupAnyStateTransition(root, hitState, "Hit", 0.05f);
            SetupAnyStateTransition(root, deathState, "Death", 0.15f); // Death 无出口，永久停留

            EditorUtility.SetDirty(controller);
        }

        private static AnimatorState CreateState(AnimatorStateMachine root, string name, string clipPath, Vector3 pos)
        {
            var state = root.AddState(name);
            SetStatePosition(root, state, pos);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if(clip == null)
            {
                Debug.LogError("找不到clip片段 ===>" + clipPath);
            }
            state.motion = clip;
            return state;
        }

        private static void SetStatePosition(AnimatorStateMachine root, AnimatorState state, Vector3 pos)
        {
            var children = root.states;
            for (int i = 0; i < children.Length; i++)
            {
                if(children[i].state == state)
                {
                    children[i].position = pos;
                    root.states = children;
                    return;
                }
            }
        }

        private static BlendTree CreateWalkBlendTree(AnimatorController controller, string idleClipPath, string walkClipPath)
        {
            var tree = new BlendTree();
            tree.name = "WalkBlendTree";
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            tree.AddChild(AssetDatabase.LoadAssetAtPath<AnimationClip>(idleClipPath), 0f);
            tree.AddChild(AssetDatabase.LoadAssetAtPath<AnimationClip>(walkClipPath), 1f);
            AssetDatabase.AddObjectToAsset(tree, controller);
            return tree;
        }

        private static void SetupExitTransition(AnimatorState from, AnimatorState to, float exitTime, bool requireMoving = false) 
        { 
            var t =from.AddTransition(to);
            t.hasExitTime = true;
            t.exitTime = exitTime;
            t.duration = 0.15f;
            if (requireMoving) 
            {
                t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            }
            t.interruptionSource = TransitionInterruptionSource.None;
        }

        private static void SetupAnyStateTransition(AnimatorStateMachine root, AnimatorState to, string triggerName, float duration)
        {
            var t = root.AddAnyStateTransition(to);
            t.hasExitTime = false;
            t.exitTime = duration;
            t.AddCondition(AnimatorConditionMode.If, 0, triggerName);
        }
    }
}