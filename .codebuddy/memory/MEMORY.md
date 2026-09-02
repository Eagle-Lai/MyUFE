# 长期记忆 MEMORY.md

## 项目概览
- 工作区 **`d:\_MyprojectDemo\MyprojectDemo`**（2026-08 由 `g:\MyUFE` 迁移，解决方案改名 `MyprojectDemo.sln`）是 UFE（Universal Fighting Engine）格斗引擎 Unity 工程，源码位于 `Assets/UFE/Engine/Scripts` 与 `Assets/UFE Addons`。
- 工程根目录的 `ClassDiagram1.cd` 是 Visual Studio 类设计器类图文件（167 KB，元素为 `<Class>` + `<TypeIdentifier><FileName>`），仅此一份（原 ClassDiagram2/3.cd 未随迁移保留）；对应统计 213 个源码文件 / 439 个类型 / 4550 个成员。
- 根目录另有三份学习文档：`进阶学习计划.md`、`学习路径.md`、`项目说明文档.md`。

## 类图分析文档归档位置（重要）
- 所有类图分析产物统一归档在 **`Assets\___Doc\`**（Unity 资源目录，___ 前缀便于排序置顶）：
  - `UFE类图详细说明文档.docx` — 成员级全量解析（213 文件/439 类型/4550 成员，13 个功能模块章节）。2026-08-24 起为表格化排版：每类型一张 3 列表格（类别/成员/说明），类别着色（方法蓝/属性绿/字段棕/事件紫/枚举成员青），H1 部分→H2 文件→H3 类型，页脚页码，TOC 目录域（Word 打开自动更新）
  - `ClassDiagram1_阅读文档.docx` — 早期的阅读文档（结构概览、模块统计、关键类型速查）
  - `TestDemo战斗功能开发指南.docx` — TestDemo 开发手册（见下）
  - `members_analysis.json` — 结构化数据源（213 文件 × 4550 成员，含 kind/name/sig/comment），Unity 会按 TextAsset 导入，无害
  - 注意：Word 打开文档时会生成 `~$` 开头的临时锁文件，属正常现象，勿当垃圾清理其 `.meta`
- 文档生成脚本保留在 **`.codebuddy\`**（不能移入 Assets，避免 Unity 把 .js 当 UnityScript 编译报错）：
  - `generate_docx.js` — 重新生成类图文档：`node .codebuddy\generate_docx.js`，输出到 `Assets\___Doc\UFE类图详细说明文档.docx`（数据源读 `Assets\___Doc\members_analysis.json`）
  - `generate_battle_guide.js` — 重新生成战斗开发指南：`node .codebuddy\generate_battle_guide.js`
  - `_verify_guide.js` — 指南 docx 结构验证辅助脚本
- 文档生成链路：Python 脚本解析 .cd XML 提取类型→文件映射 → Python 静态分析 .cs 提取成员+注释（方法签名跨行拼接）→ Node + 全局 docx-js（`npm root -g` 动态 require，docx@9.7.1）生成 docx。
- docx 结构验证技巧：node `execFileSync('tar',['-xOf',file,entry],{maxBuffer:512*1024*1024})` 直接 spawn tar 读 zip 条目（禁止经 cmd 或 node -e 内联传中文路径，会 GBK 乱码，必须写 .js/.py 文件执行）。

## 环境要点
- 系统 Python 为 2.7（临时脚本需 Py2 兼容写法；json 写文件用 ascii 编码；命令行传中文参数会乱码，验证/脚本一律写成 .py 文件再执行）。
- Node v24 可用；全局 npm 模块在 `C:\Users\lzy\AppData\Roaming\npm\node_modules`（docx、xlsx 等）。
- 注意根目录 `___temp\`（`___temp\Assets\...`）是项目自带的 Assets 副本，勿删。迁移后已从 `Assets\___temp` 移到工程根目录。

## TestDemo 学习项目（2026-08-24 起）
- 用户从零学习实现简单战斗系统（场景 `Assets\TestDemo\test.unity`，Ethan 玩家 / Robot_Kyle 敌人），自己敲代码，由简到繁。
- **双目录双命名空间约定（永久规则，2026-08-31 用户明确要求）**：**以后每个功能实现都必须同时提供 AIScripts 和 MyScripts 两个版本**。AIScripts 版是"老师"角色（完整参考实现，带详细中文注释），MyScripts 版是"学生"角色（用户自己动手实现的版本）。两个版本功能对齐、接口一致，只是命名空间不同。AI 先写 AIScripts 版作为参考，用户参照后自己写 MyScripts 版。用户目的是学习，不是让 AI 直接改 MyScripts。
  - AI 参考脚本放 `Assets\TestDemo\AIScripts\`（namespace `AIScripts`，11 个完整可运行参考实现：CameraFollow/PlayerController/PlayerAttack/Health/AttackHit/Skill/SkillController/Projectile/EnemyAI/HealthBarUI/GameManager）
  - 用户自己敲的实现放 `Assets\TestDemo\MyScripts\`（namespace `MyScripts`，**11 个全部完成**：与 AIScripts 完全对齐）
  - 两个命名空间可同名类共存，验证时整组用同一命名空间（推荐 MyScripts 版），避免混挂类型不匹配
  - Editor 工具脚本**也必须双版本**（2026-08-31 用户明确要求，覆盖此前"Editor 脚本无需双版本"的约定）：AI 版放 `Assets\TestDemo\AIScripts\Editor\`（namespace `AIScripts`，菜单前缀 `TestDemo/AI/...`），MyScripts 版由用户写在 `Assets\TestDemo\MyScripts\Editor\`（namespace `MyScripts`，菜单前缀 `TestDemo/My/...`）；Unity 会把任意层级的 Editor 文件夹编译进 Editor 程序集；两版本生成的资产路径共用，执行任一版本菜单即可
- `Assets\TestDemo\Prefabs\` 目录已创建（当前为空），供后续工程化使用。
- 开发手册：`Assets\___Doc\TestDemo战斗功能开发指南.docx`（11 步渐进式：相机→移动→攻击→生命→命中→技能→AI→受击反馈→UI→胜负判定，每步含 Unity 操作/脚本设计/代码骨架/验证/学习点，代码骨架即 AIScripts 参考的浓缩版）。
- 场景关键事实：两个模型 Animator 共用 UFE 的 MC_Controller.controller（`Assets\UFE\Engine\Resources\`，仅 Mirror 参数、状态机为空），无现成动画 → 前期用位移/特效模拟动作，真动画留扩展阶段。

## TestDemo 动画系统接入（2026-08-31 进行中）
- 计划文档：`Assets\___Doc\TestDemo动画系统实现计划.docx`（步骤 0 Editor 工具 + 步骤 1-5 AIScripts/MyScripts 双版本 + 步骤 6 场景配置），生成脚本 `.codebuddy\generate_anim_plan_docx.js`。
- 动画资源映射（已确认全部存在）：Player(Ethan)=E_Basic_Idle / E_Basic_Walk_Forward / E_Stand_N1 / E_Basic_Hit_High_weak / E_Basic_Fall_Back；Enemy(Robot_Kyle)=IdleStanding / MoveForward / PunchStandingLight / HitStandingLight / FallDown，均在 `Assets\UFE\Demo\Characters\<角色>\Animations\`。
- **第 0 步 AI 版已完成**：`Assets\TestDemo\AIScripts\Editor\CreateTestDemoAnimators.cs`（namespace AIScripts，lint 通过；旧单版本 `Assets\Editor\CreateTestDemoAnimators.cs` 待用户手动删除，删除审批多次超时）。菜单 `TestDemo > AI > Create Animator Controllers` 生成 `Assets\TestDemo\PlayerAnimator.controller` + `EnemyAnimator.controller`：4 参数（Speed float / Attack / Hit / Death trigger）+ 5 状态（Idle 默认 / Walk / Attack / Hit / Death）；Walk 为 1D BlendTree（Speed 0→IdleClip、1→WalkClip）；Idle↔Walk 按 Speed 0.1 阈值；Attack/Hit 播完按是否移动回 Idle(exitTime 0.9/0.8)或 Walk(+Speed>0.1)；AnyState trigger 驱动 Attack/Hit/Death，Death 无出口；已存在时弹窗确认覆盖。菜单 `TestDemo > AI > Apply Animators To Scene` 按名字含 Ethan/Robot 自动挂对应控制器并关闭 applyRootMotion。MyScripts 版 Editor 脚本留给用户自己实现（namespace MyScripts，菜单前缀 TestDemo/My/...）。
  - 关键 API 坑：手动 `new BlendTree()` 后必须 `AssetDatabase.AddObjectToAsset(tree, controller)` 嵌入 controller 资产，否则保存后丢失。
  - 关键 API 坑 2（Unity 2020.3.17f1 实测）：`AnimatorState` **没有 `position` 属性**——状态图位置存在 `ChildAnimatorState` 结构体数组 `root.states` 上，需遍历匹配 state 后改 `children[i].position` 并写回 `root.states = children`；**也没有 `clip` 属性**——动画片段直接赋 `state.motion`（AnimationClip 是 Motion 子类）。曾报 3 个 CS1061，已用 SetStatePosition 辅助方法修复。
  - 本机 Unity Editor.log 位置：`C:\Users\lzy\AppData\Local\Unity\Editor\Editor.log`（Local 而非 Roaming），grep "error CS" 可定位 Unity 侧编译错误（IDE lint 不可靠时用）。
- 进度：**第 0 步成功**（controller 已生成）；**第 1-6 步 AI 版全部完成**（PlayerController SetFloat Speed / PlayerAttack SetTrigger Attack / SkillController 复用 Attack / EnemyAI 状态机动画同步 / Health 受击+死亡动画+Restore 动画重置 `anim.Play("Idle",0,0f)`，均为 Animator 字段+Awake 兜底+判空，原逻辑未动，lint 通过）。剩余：①用户自写 MyScripts 版 1-6 步动画改动（目前 MyScripts 尚未开始，MyScripts/Editor 空）；②步骤 6 场景配置（菜单 Apply Animators To Scene 挂控制器，确认 Apply Root Motion 关闭；验证流程：移动/攻击/技能/AI/受击/死亡/按 R 重置双方回 Idle）。
