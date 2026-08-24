# 长期记忆 MEMORY.md

## 项目概览
- 工作区 `g:\MyUFE` 是 UFE（Universal Fighting Engine）格斗引擎 Unity 工程，源码位于 `Assets/UFE/Engine/Scripts` 与 `Assets/UFE Addons`。
- 工程根目录的 `ClassDiagram1.cd / ClassDiagram2.cd / ClassDiagram3.cd` 是 Visual Studio 类设计器类图文件，三个文件内容一致，覆盖 213 个源码文件 / 439 个类型 / 4550 个成员。

## 类图分析文档归档位置（重要）
- 所有类图分析产物统一归档在 **`g:\MyUFE\Assets\___Doc\`**（Unity 资源目录，___ 前缀便于排序置顶）：
  - `UFE类图详细说明文档.docx` — 成员级全量解析（213 文件/439 类型/4550 成员，13 个功能模块章节）。2026-08-24 起为表格化排版：每类型一张 3 列表格（类别/成员/说明），类别着色（方法蓝/属性绿/字段棕/事件紫/枚举成员青），H1 部分→H2 文件→H3 类型，页脚页码，TOC 目录域（Word 打开自动更新）
  - `ClassDiagram1_阅读文档.docx` — 早期的阅读文档（结构概览、模块统计、关键类型速查）
  - `members_analysis.json` — 结构化数据源（213 文件 × 4550 成员，含 kind/name/sig/comment），Unity 会按 TextAsset 导入，无害
- 文档生成脚本保留在 **`g:\MyUFE\.codebuddy\generate_docx.js`**（不能移入 Assets，避免 Unity 把 .js 当 UnityScript 编译报错）。重新生成/定制文档：`node g:\MyUFE\.codebuddy\generate_docx.js`，输出到 `Assets\___Doc\UFE类图详细说明文档.docx`（数据源读 `Assets\___Doc\members_analysis.json`）。
- 文档生成链路：Python 脚本解析 .cd XML 提取类型→文件映射 → Python 静态分析 .cs 提取成员+注释（方法签名跨行拼接）→ Node + 全局 docx-js（`npm root -g` 动态 require，docx@9.7.1）生成 docx。
- docx 结构验证技巧：node `execFileSync('tar',['-xOf',file,entry],{maxBuffer:512*1024*1024})` 直接 spawn tar 读 zip 条目（禁止经 cmd 或 node -e 内联传中文路径，会 GBK 乱码，必须写 .js/.py 文件执行）。

## 环境要点
- 系统 Python 为 2.7（临时脚本需 Py2 兼容写法；json 写文件用 ascii 编码；命令行传中文参数会乱码，验证/脚本一律写成 .py 文件再执行）。
- Node v24 可用；全局 npm 模块在 `C:\Users\lzy\AppData\Roaming\npm\node_modules`（docx、xlsx 等）。
- 注意 `___temp` 目录是项目自带的 Assets 副本，勿删。

## TestDemo 学习项目（2026-08-24 起）
- 用户从零学习实现简单战斗系统（场景 `Assets\TestDemo\test.unity`，Ethan 玩家 / Robot_Kyle 敌人），自己敲代码，由简到繁。
- **双目录双命名空间约定**：AI 参考脚本放 `Assets\TestDemo\AIScripts\`（namespace `AIScripts`，11 个完整可运行参考实现：CameraFollow/PlayerController/PlayerAttack/Health/AttackHit/Skill/SkillController/Projectile/EnemyAI/HealthBarUI/GameManager）；用户自己敲的实现放 `Assets\TestDemo\MyScripts\`（namespace `MyScripts`）。两个命名空间可同名类共存，验证时整组用同一命名空间（推荐 MyScripts 版），避免混挂类型不匹配。
- 开发手册：`Assets\___Doc\TestDemo战斗功能开发指南.docx`（11 步渐进式：相机→移动→攻击→生命→命中→技能→AI→受击反馈→UI→胜负判定，每步含 Unity 操作/脚本设计/代码骨架/验证/学习点，代码骨架即 AIScripts 参考的浓缩版）。生成脚本 `.codebuddy\generate_battle_guide.js`，重跑：`node g:\MyUFE\.codebuddy\generate_battle_guide.js`。
- 场景关键事实：两个模型 Animator 共用 UFE 的 MC_Controller.controller（`Assets\UFE\Engine\Resources\`，仅 Mirror 参数、状态机为空），无现成动画 → 前期用位移/特效模拟动作，真动画留扩展阶段。
