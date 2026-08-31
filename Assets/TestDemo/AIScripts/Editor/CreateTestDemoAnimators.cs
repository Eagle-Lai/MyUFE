using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AIScripts
{
    /// <summary>
    /// TestDemo 第 0 步（AIScripts 参考版）：一键生成 PlayerAnimator（玩家 Ethan）与 EnemyAnimator（敌人 Robot_Kyle）
    /// 两个动画控制器，自动绑定 UFE Demo 自带的动画片段并搭建状态机。
    ///
    /// 使用方式：
    /// 1. 菜单 TestDemo > AI > Create Animator Controllers —— 生成/覆盖两个 .controller 资产
    /// 2. 菜单 TestDemo > AI > Apply Animators To Scene —— 把生成的控制器自动挂到场景中
    ///    名字含 Ethan / Robot 的模型 Animator 上（并关闭 Apply Root Motion）
    ///
    /// 约定：与 MyScripts/Editor/CreateTestDemoAnimators.cs（你自己的实现）功能完全一致，
    /// 两者生成的 controller 路径相同，执行任意一个版本的菜单即可。
    /// </summary>
    public static class CreateTestDemoAnimators
    {
        // ==================== 配置区：动画资源路径（相对 Assets/） ====================

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

        // ==================== 菜单 1：生成控制器 ====================

        [MenuItem("TestDemo/AI/Create Animator Controllers")]
        public static void CreateAll()
        {
            // 已存在则先询问，避免未经确认覆盖生成物（覆盖后场景引用会丢，需重新执行应用菜单）
            bool exists = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerControllerPath) != null
                       || AssetDatabase.LoadAssetAtPath<AnimatorController>(EnemyControllerPath) != null;
            if (exists)
            {
                if (!EditorUtility.DisplayDialog("生成动画控制器",
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
            Debug.Log("[AIScripts.CreateTestDemoAnimators] 动画控制器生成完成：" + PlayerControllerPath + " / " + EnemyControllerPath);
        }

        /// <summary>
        /// 创建单个动画控制器：4 个参数（Speed/Attack/Hit/Death）+ 5 个状态（Idle/Walk/Attack/Hit/Death）
        /// Idle <-> Walk 由 Speed 阈值切换；Attack/Hit 播完按是否移动回 Idle 或 Walk；Death 播完停留。
        /// </summary>
        static void CreateController(string path, string idleClip, string walkClip, string attackClip, string hitClip, string deathClip)
        {
            // 删除旧的生成物后重建（上面已询问过用户）
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            // ---- 参数 ----
            // Speed：移动量，0 = 站立，>0.1 = 走路（第 1 步的 PlayerController / 第 4 步的 EnemyAI 会 SetFloat 驱动）
            // Attack / Hit / Death：一次性触发器
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            var root = controller.layers[0].stateMachine;

            // ---- 状态：Walk 用 1D BlendTree 混合 Idle/Walk 片段，其余状态直接绑片段 ----
            var idleState = CreateState(root, "Idle", idleClip, new Vector3(220, 120, 0));
            var walkState = root.AddState("Walk");
            SetStatePosition(root, walkState, new Vector3(220, 300, 0));
            walkState.motion = CreateWalkBlendTree(controller, idleClip, walkClip); // Speed 0→Idle、1→Walk
            var attackState = CreateState(root, "Attack", attackClip, new Vector3(460, 120, 0));
            var hitState = CreateState(root, "Hit", hitClip, new Vector3(460, 300, 0));
            var deathState = CreateState(root, "Death", deathClip, new Vector3(700, 120, 0));

            root.defaultState = idleState; // 默认进入站立

            // ---- Idle <-> Walk：由 Speed 参数驱动（无退出时间，参数一变立即过渡）----
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

        /// <summary>创建状态并绑定动画片段（片段缺失时只报错不中断，方便先搭结构）</summary>
        /// <remarks>注意：AnimatorState 没有 position/clip 属性——位置存在 ChildAnimatorState 结构体上，
        /// 动画片段直接赋给 motion 字段（AnimationClip 是 Motion 的子类）。</remarks>
        static AnimatorState CreateState(AnimatorStateMachine root, string name, string clipPath, Vector3 pos)
        {
            var state = root.AddState(name);
            SetStatePosition(root, state, pos);

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                Debug.LogError("[AIScripts.CreateTestDemoAnimators] 找不到动画片段：" + clipPath);
            }
            state.motion = clip;
            return state;
        }

        /// <summary>设置状态在状态机图中的位置（Unity 把位置存在 ChildAnimatorState 结构体数组里，查找替换后必须写回）</summary>
        static void SetStatePosition(AnimatorStateMachine root, AnimatorState state, Vector3 pos)
        {
            var children = root.states;
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].state == state)
                {
                    children[i].position = pos;
                    root.states = children; // 结构体数组改完必须写回才生效
                    return;
                }
            }
        }

        /// <summary>为 Walk 状态创建 1D BlendTree：Speed 0→Idle 片段、1→Walk 片段，走停平滑混合</summary>
        static BlendTree CreateWalkBlendTree(AnimatorController controller, string idleClipPath, string walkClipPath)
        {
            var tree = new BlendTree();
            tree.name = "WalkBlendTree";
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            tree.AddChild(AssetDatabase.LoadAssetAtPath<AnimationClip>(idleClipPath), 0f);
            tree.AddChild(AssetDatabase.LoadAssetAtPath<AnimationClip>(walkClipPath), 1f);
            // 关键：BlendTree 是 SubAsset，必须嵌入 controller 资产，否则保存后丢失
            AssetDatabase.AddObjectToAsset(tree, controller);
            return tree;
        }

        /// <summary>一次性动作的自动退出转换：播放到 exitTime 后过渡回目标状态（requireMoving 时附加 Speed>0.1 条件）</summary>
        static void SetupExitTransition(AnimatorState from, AnimatorState to, float exitTime, bool requireMoving = false)
        {
            var t = from.AddTransition(to);
            t.hasExitTime = true;  // 播完本动画再切
            t.exitTime = exitTime; // 开始过渡的时间点
            t.duration = 0.15f;    // 过渡时长
            if (requireMoving)
            {
                t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            }
            t.interruptionSource = TransitionInterruptionSource.None;
        }

        /// <summary>从 Any State 添加由 Trigger 驱动的转换（无退出时间，随时打断）</summary>
        static void SetupAnyStateTransition(AnimatorStateMachine root, AnimatorState to, string triggerName, float duration)
        {
            var t = root.AddAnyStateTransition(to);
            t.hasExitTime = false;
            t.duration = duration;
            t.AddCondition(AnimatorConditionMode.If, 0, triggerName);
        }

        // ==================== 菜单 2：应用到场景 ====================

        [MenuItem("TestDemo/AI/Apply Animators To Scene")]
        public static void ApplyToScene()
        {
            // 两个控制器都在才继续
            var player = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerControllerPath);
            var enemy = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(EnemyControllerPath);
            if (player == null || enemy == null)
            {
                Debug.LogError("[AIScripts.CreateTestDemoAnimators] 控制器不存在，请先执行菜单 TestDemo > AI > Create Animator Controllers");
                return;
            }

            // 遍历场景中的 Animator，按模型名自动分配控制器
            var animators = Object.FindObjectsOfType<Animator>();
            int applied = 0;
            foreach (var ani in animators)
            {
                string objName = ani.gameObject.name;
                if (objName.Contains("Ethan"))
                {
                    ani.runtimeAnimatorController = player;
                }
                else if (objName.Contains("Robot"))
                {
                    ani.runtimeAnimatorController = enemy;
                }
                else
                {
                    continue; // 其他 Animator（比如 UFE 自己的对象）不动
                }

                // 位移由移动脚本控制，关闭根运动避免动画自带位移叠加
                ani.applyRootMotion = false;
                EditorSceneManager.MarkSceneDirty(ani.gameObject.scene);
                applied++;
                Debug.Log("[AIScripts.CreateTestDemoAnimators] 已为 " + objName + " 挂载 " + ani.runtimeAnimatorController.name);
            }

            if (applied == 0)
            {
                Debug.LogWarning("[AIScripts.CreateTestDemoAnimators] 场景中未找到名字含 Ethan / Robot 的模型，请确认场景已摆放角色");
            }
            else
            {
                Debug.Log("[AIScripts.CreateTestDemoAnimators] 应用完成，共 " + applied + " 个 Animator");
            }
        }
    }
}
