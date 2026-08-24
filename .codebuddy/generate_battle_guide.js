const fs = require("fs");
const path = require("path");
const { execSync } = require("child_process");

let globalRoot = "";
try { globalRoot = execSync("npm root -g").toString().trim(); } catch (e) {}
const docx = require(globalRoot ? path.join(globalRoot, "docx") : "docx");

const { Document, Packer, Paragraph, TextRun, HeadingLevel, AlignmentType, PageBreak, TableOfContents, Table, TableRow, TableCell, WidthType, Footer, PageNumber } = docx;

const OUT = path.join("g:\\MyUFE", "Assets", "___Doc", "TestDemo战斗功能开发指南.docx");

// ---------- helpers ----------
const MONO = { ascii: "Consolas", eastAsia: "微软雅黑" };
const BLUE = "1F4E79", DARK = "17365D", GRAY = "404040", LIGHT = "7F7F7F";

function P(children, opts) { return new Paragraph(Object.assign({ children }, opts || {})); }
function gap(after) { return new Paragraph({ children: [], spacing: { after: after || 60 } }); }

// code block: array of lines -> paragraphs with gray shading
function codeBlock(code) {
  const lines = String(code).replace(/\t/g, "    ").split("\n");
  return lines.map(l => new Paragraph({
    shading: { fill: "F5F5F5" },
    indent: { left: 240, right: 120 },
    spacing: { after: 0, before: 0, line: 240 },
    children: [new TextRun({ text: l.length ? l : " ", size: 15, font: MONO, color: "222222" })]
  }));
}

function bullet(text, opts) {
  return P([new TextRun({ text: text, size: 19, color: GRAY })], Object.assign({ bullet: { level: 0 }, spacing: { after: 30 } }, opts));
}

function label(text) {
  return P([new TextRun({ text: text, bold: true, size: 20, color: BLUE })], { spacing: { before: 80, after: 30 } });
}

// step builder
function step(no, title, script, opts) {
  const out = [];
  out.push(new Paragraph({
    heading: HeadingLevel.HEADING_2,
    spacing: { before: 320, after: 60 },
    children: [
      new TextRun({ text: "第 " + no + " 步　" + title, bold: true, size: 24, color: DARK })
    ]
  }));
  if (script) {
    out.push(P([new TextRun({ text: "参考实现：Assets/TestDemo/AIScripts/" + script + "　（namespace AIScripts）", italics: true, size: 16, color: LIGHT })], { spacing: { after: 20 } }));
    out.push(P([new TextRun({ text: "自己敲入：Assets/TestDemo/MyScripts/" + script + "　（namespace MyScripts）", italics: true, size: 16, color: LIGHT })], { spacing: { after: 60 } }));
  }
  if (opts.goal) { out.push(label("目标")); out.push(P([new TextRun({ text: opts.goal, size: 19, color: GRAY })], { spacing: { after: 60 } })); }
  if (opts.relation) {
    out.push(label("脚本关系"));
    (opts.relation).forEach(t => out.push(bullet(t)));
    out.push(gap(20));
  }
  if (opts.unity) {
    out.push(label("Unity 编辑器操作"));
    (opts.unity).forEach(t => out.push(bullet(t)));
    out.push(gap(20));
  }
  if (opts.design) {
    out.push(label("脚本设计"));
    (opts.design).forEach(t => out.push(bullet(t)));
    out.push(gap(20));
  }
  if (opts.code) {
    out.push(label("核心代码骨架"));
    out.push.apply(out, codeBlock(opts.code));
    out.push(gap(20));
  }
  if (opts.verify) {
    out.push(label("验证方法"));
    (opts.verify).forEach(t => out.push(bullet(t)));
    out.push(gap(20));
  }
  if (opts.learn) {
    out.push(label("学习点"));
    (opts.learn).forEach(t => out.push(bullet(t)));
  }
  return out;
}

// =================== content ===================
const children = [];
const today = "2026-08-24";

// ---- cover ----
children.push(P([new TextRun({ text: "TestDemo 战斗功能开发指南", bold: true, size: 52, color: BLUE })], { alignment: AlignmentType.CENTER, spacing: { before: 1600, after: 120 } }));
children.push(P([new TextRun({ text: "从零实现一个简单战斗系统", bold: true, size: 36, color: DARK })], { alignment: AlignmentType.CENTER, spacing: { after: 500 } }));
children.push(P([new TextRun({ text: "场景 test.unity · 角色 Ethan / Robot_Kyle · 共 11 步渐进式开发", size: 22, color: GRAY })], { alignment: AlignmentType.CENTER, spacing: { after: 700 } }));
children.push(P([new TextRun({ text: "配套资源：UFE类图详细说明文档.docx（引擎源码全量解析，开发中随时查阅）", size: 18, color: LIGHT })], { alignment: AlignmentType.CENTER, spacing: { after: 60 } }));
children.push(P([new TextRun({ text: "生成日期：" + today, size: 18, color: LIGHT })], { alignment: AlignmentType.CENTER, spacing: { after: 60 } }));

// ---- guide ----
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(P([new TextRun({ text: "阅读指南", bold: true, size: 32, color: BLUE })], { heading: HeadingLevel.HEADING_1, spacing: { after: 120 } }));
children.push(P([new TextRun({ text: "本文档是 TestDemo 场景从零实现战斗功能的分步开发手册。共 11 步，每一步都是一个可独立运行验证的里程碑：先介绍目标与 Unity 编辑器操作，再给出脚本设计与核心代码骨架，最后是验证方法和学习点。", size: 19, color: GRAY })], { spacing: { after: 60 } }));
children.push(P([new TextRun({ text: "使用建议：", bold: true, size: 19, color: DARK })], { spacing: { after: 40 } }));
children.push(bullet("代码骨架是“设计蓝图”，请自己在编辑器中逐行敲入，遇到卡点再对照文档排查；刻意保留的 TODO 注释是下一步的衔接点。"));
children.push(bullet("每步完成后先运行验证，确认通过再进入下一步；卡住时把代码贴给 AI 一起排查。"));
children.push(bullet("脚本分两个目录、两个命名空间：AI 参考脚本在 Assets/TestDemo/AIScripts/（namespace AIScripts），你自己的实现放在 Assets/TestDemo/MyScripts/（namespace MyScripts）。"));
children.push(bullet("文档中的深度优先顺序：操作手感 → 动画表现 → 战斗规则 → AI → 打击感 → 工程化，见“扩展路线”章节。"));
children.push(bullet("学习工作流：读懂 AIScripts 参考实现 → 在 MyScripts 亲手敲出自己的版本 → 挂载验证。验证时整组使用同一命名空间（推荐全部用 MyScripts 版），AIScripts 版仅作阅读对照，避免混挂不同命名空间导致字段类型不匹配。"));
children.push(bullet("分步供给（重要）：AIScripts 目录每步只保留当前步骤需要的参考脚本，其余不提前提供——防止你看到后面的实现，保证按自己节奏走。每完成一步、验证通过后，向 AI 索取下一步的参考脚本，同时让 AI 讲解该步与已完成脚本的关系。"));

// ---- TOC ----
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(P([new TextRun({ text: "目录", bold: true, size: 32, color: BLUE })], { heading: HeadingLevel.HEADING_1, spacing: { after: 120 } }));
children.push(new TableOfContents("目录", { hyperlink: true, headingStyleRange: "1-2" }));
children.push(new Paragraph({ children: [new PageBreak()] }));

// ---- ch1 env ----
children.push(P([new TextRun({ text: "第一章　环境现状与准备", bold: true, size: 30, color: BLUE })], { heading: HeadingLevel.HEADING_1, spacing: { after: 80 }, pageBreakBefore: true }));

children.push(label("1.1　场景对象清单（test.unity）"));
const envRows = [
  ["Main Camera", "透视相机（FOV 60），位置 (-0.45, 4.78, 11.23)，挂在 CameraFollow 脚本"],
  ["Directional Light", "平行光，当前旋转角度可覆盖训练室"],
  ["TrainingRoom", "训练室地面预制体（位置 0,0.01,-4），战斗场地"],
  ["Ethan", "Unity 标准角色模型（位置 -7.75,0,0），玩家控制对象"],
  ["Robot_Kyle", "机器人模型（位置 4.92,0,0，Y 旋转 -90°），敌人对象"]
];
children.push(new Table({
  width: { size: 100, type: WidthType.PERCENTAGE }, columnWidths: [2200, 6826],
  rows: [
    new TableRow({ tableHeader: true, children: [
      new TableCell({ margins: { top: 40, bottom: 40, left: 100, right: 100 }, shading: { fill: BLUE }, children: [P([new TextRun({ text: "对象", bold: true, size: 17, color: "FFFFFF" })], { spacing: { after: 0 } })] }),
      new TableCell({ margins: { top: 40, bottom: 40, left: 100, right: 100 }, shading: { fill: BLUE }, children: [P([new TextRun({ text: "说明", bold: true, size: 17, color: "FFFFFF" })], { spacing: { after: 0 } })] })
    ] })
  ].concat(envRows.map(r => new TableRow({ children: [
    new TableCell({ margins: { top: 40, bottom: 40, left: 100, right: 100 }, children: [P([new TextRun({ text: r[0], bold: true, size: 17, color: DARK })], { spacing: { after: 0 } })] }),
    new TableCell({ margins: { top: 40, bottom: 40, left: 100, right: 100 }, children: [P([new TextRun({ text: r[1], size: 17, color: GRAY })], { spacing: { after: 0 } })] })
  ] })))
}));
children.push(gap(80));

children.push(label("1.2　关键发现：动画控制器是空的"));
children.push(P([new TextRun({ text: "两个预制体共用的 AnimatorController 是 UFE 引擎自带的 MC_Controller.controller（Assets/UFE/Engine/Resources/）。它只有一个 Mirror 布尔参数，状态机基本为空（Default/State1-4，无实际动画剪辑）——这是 UFE 运行时动态驱动动画的占位控制器。", size: 19, color: GRAY })], { spacing: { after: 40 } }));
children.push(P([new TextRun({ text: "结论：当前模型没有现成的 Idle/Walk/Attack 动画。因此前 11 步用“位移 + 旋转 + 特效”模拟动作表现（第 4 步用前冲模拟挥拳）；真正的骨骼动画接入放到扩展路线的“动作表现”方向，届时可参考 Assets/UFE/Demo/Resources/Characters/ 中 UFE 官方角色，或学习 UFE.cs 的动画驱动方式。", size: 19, color: GRAY })], { spacing: { after: 80 } }));

children.push(label("1.3　工程约定：双目录双命名空间"));
children.push(bullet("参考脚本：Assets/TestDemo/AIScripts/（namespace AIScripts）——AI 为每一步提供的完整可运行参考实现"));
children.push(bullet("你的实现：Assets/TestDemo/MyScripts/（namespace MyScripts）——你照着参考脚本亲手敲的版本"));
children.push(bullet("脚本命名：文件名与类名一致（如 CameraFollow.cs → public class CameraFollow），组件名即类名"));
children.push(bullet("挂载：Inspector → Add Component → 输入类名选择；或从 Project 把脚本拖到 Hierarchy 对象"));
children.push(bullet("命名空间隔离：两个命名空间可存在同名类（AIScripts.PlayerController 与 MyScripts.PlayerController 互不冲突），支持对照运行"));
children.push(bullet("调试：Debug.Log 输出关键信息，养成“先日志后现象”的排错习惯"));
children.push(bullet("提醒：Unity 把 .js 当 UnityScript 拒绝编译，脚本必须 .cs 后缀"));

// ---- ch2 arch ----
children.push(P([new TextRun({ text: "第二章　总体架构", bold: true, size: 30, color: BLUE })], { heading: HeadingLevel.HEADING_1, spacing: { before: 240, after: 80 }, pageBreakBefore: true }));
children.push(P([new TextRun({ text: "下图是 11 步全部完成后的最终全貌，只用于建立整体认知、理解脚本间的关系。实现时请严格按第三章顺序逐块进行，每步只关注该步涉及的一个或几个脚本——过早读后面步骤的实现没有好处。", size: 18, color: LIGHT })], { spacing: { after: 40 } }));
children.push(codeBlock([
  "Assets/TestDemo/AIScripts/  → AI 参考实现（namespace AIScripts）",
  "Assets/TestDemo/MyScripts/  → 你亲手敲的实现（namespace MyScripts）",
  "",
  "Ethan（玩家）                      Robot_Kyle（敌人）",
  "├─ PlayerController.cs   移动       ├─ EnemyAI.cs        状态机(Idle/Chase/Attack)",
  "├─ PlayerAttack.cs       近战攻击   ├─ CharacterController",
  "├─ SkillController.cs    技能+冷却  └─ Health.cs",
  "├─ CharacterController",
  "└─ Health.cs",
  "",
  "Main Camera：CameraFollow.cs（第三人称跟随 + 右键旋转 + 滚轮缩放）",
  "UI（Canvas）：血条(Image.fillAmount) + 技能按钮(Button) + 胜负文本(Text)",
  "GameManager.cs：胜负判定与按 R 重置"
]));
children.push(gap(60));
children.push(P([new TextRun({ text: "依赖关系：技能/攻击脚本通过 GetComponent 或事件访问 Health；UI 只订阅 Health 的事件（解耦）；EnemyAI 通过 Tag 查找玩家。", size: 19, color: GRAY })], { spacing: { after: 60 } }));

// ---- ch3 steps ----
children.push(P([new TextRun({ text: "第三章　分步实现", bold: true, size: 30, color: BLUE })], { heading: HeadingLevel.HEADING_1, spacing: { before: 240, after: 80 }, pageBreakBefore: true }));

// step 1
children.push.apply(children, step(1, "环境确认", null, {
  goal: "确认场景可运行、认识所有对象与组件，为后续挂脚本做准备。",
  relation: [
    "第 1 步不新增脚本，只确认场景对象与组件，为后续挂载做准备。"
  ],
  unity: [
    "打开 Assets/TestDemo/test.unity，点击 Play 运行，确认能看到训练室地面、Ethan、Robot_Kyle、灯光与相机画面。",
    "在 Hierarchy 中展开两个模型，观察骨骼层级（char_ethan_*、char_robot_* 等）与挂载的 Animator 组件。",
    "选中 Main Camera，在 Inspector 中确认 Camera 组件参数（FOV 60、透视模式）。",
    "在 Project 窗口新建文件夹 Assets/TestDemo/Scripts，用于存放后续所有脚本。"
  ],
  verify: ["Play 后画面正常，无报错；熟悉 Scene 与 Game 视图切换。"]
}));

// step 2
children.push.apply(children, step(2, "第三人称摄像机跟随", "CameraFollow.cs", {
  goal: "相机跟随 Ethan，按住鼠标右键拖动旋转视角，滚轮缩放距离——这是动作游戏的基础镜头。",
  relation: [
    "本步新增 CameraFollow.cs，独立完成，不依赖任何其他脚本。",
    "后续衔接：第 3 步的 PlayerController 会反过来依赖它——移动方向以相机朝向为参考系（Camera.main），形成“相机 → 玩家”的协作。"
  ],
  unity: [
    "在 Scripts 目录新建 C# 脚本 CameraFollow.cs（右键 → Create → C# Script）。",
    "把脚本挂到 Main Camera 上，将 target 字段拖入 Ethan。"
  ],
  design: [
    "字段：target(Transform)、distance(相机与角色距离)、rotateSpeed(旋转灵敏度)、pitchMin/pitchMax(俯仰角上下限)、yaw/pitch(内部累计角度)",
    "方法：Start() 初始化角度；Update() 读取鼠标输入；LateUpdate() 定位相机并看向目标"
  ],
  code: [
    "using UnityEngine;",
    "",
    "public class CameraFollow : MonoBehaviour",
    "{",
    "    [Header(\"跟随目标\")]",
    "    public Transform target;",
    "",
    "    [Header(\"视角参数\")]",
    "    public float distance = 6f;      // 相机与目标的距离",
    "    public float rotateSpeed = 3f;   // 旋转灵敏度",
    "    public float pitchMin = -30f;    // 俯仰角下限（防止转到地面以下）",
    "    public float pitchMax = 60f;     // 俯仰角上限",
    "",
    "    float yaw;   // 水平角（绕 Y 轴）",
    "    float pitch; // 俯仰角（绕 X 轴）",
    "",
    "    void Start()",
    "    {",
    "        // 自测：根据 transform 与 target 的初始位置差反算 yaw/pitch，",
    "        // 避免游戏一开始相机角度跳变（可先用固定值 0，后续再优化）",
    "    }",
    "",
    "    void Update()",
    "    {",
    "        // 按住鼠标右键时旋转视角（动作游戏习惯）",
    "        if (Input.GetMouseButton(1))",
    "        {",
    "            yaw   += Input.GetAxis(\"Mouse X\") * rotateSpeed;",
    "            pitch -= Input.GetAxis(\"Mouse Y\") * rotateSpeed;",
    "            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);",
    "        }",
    "        // 鼠标滚轮缩放距离",
    "        distance -= Input.GetAxis(\"Mouse ScrollWheel\") * 2f;",
    "        distance = Mathf.Clamp(distance, 3f, 15f);",
    "    }",
    "",
    "    void LateUpdate()",
    "    {",
    "        // 1) 用欧拉角构造旋转  2) 算出目标后方的偏移点  3) 看向目标",
    "        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);",
    "        transform.position = target.position + rot * Vector3.forward * (-distance);",
    "        transform.rotation = Quaternion.LookRotation(target.position - transform.position);",
    "    }",
    "}"
  ],
  verify: [
    "Play 后相机始终跟随 Ethan；按住右键拖动鼠标视角围绕角色旋转；滚轮缩放且俯仰角被限制在合理范围。",
    "角色移动（下步完成）时画面不抖动——这是用 LateUpdate 而不是 Update 的原因。"
  ],
  learn: ["Update 与 LateUpdate 的执行顺序差异及适用场景", "欧拉角 (yaw/pitch) 与 Quaternion.Euler", "Vector3 与旋转四元数的乘法（旋转向量）"]
}));

// step 3
children.push.apply(children, step(3, "玩家移动控制", "PlayerController.cs", {
  goal: "WASD/方向键控制 Ethan 前后左右移动，角色平滑转向移动方向，基于相机朝向（第三人人称标准做法）。",
  relation: [
    "新增 PlayerController.cs，独立完成。",
    "与第 2 步的协作：CameraFollow 通过 target 跟随 Ethan（它读玩家位置）；本步用 Camera.main 的朝向计算移动方向（它读相机朝向）——双向配合，但两个脚本之间没有直接代码引用。"
  ],
  unity: [
    "给 Ethan 添加 CharacterController 组件（Inspector → Add Component → Character Controller），这是比 Rigidbody 更简单的角色移动方案。",
    "把 PlayerController.cs 挂到 Ethan 上，moveSpeed 设为 5 左右。",
    "第 2 步的 CameraFollow.target 保持指向 Ethan（若因重置丢失请重新拖拽）。"
  ],
  design: [
    "字段：moveSpeed、rotateSpeed、gravity、cc(CharacterController 缓存引用)",
    "方法：Awake() 缓存组件；Update() 读取输入 → 计算移动方向 → 转向 → 移动"
  ],
  code: [
    "using UnityEngine;",
    "",
    "[RequireComponent(typeof(CharacterController))]",
    "public class PlayerController : MonoBehaviour",
    "{",
    "    public float moveSpeed = 5f;",
    "    public float rotateSpeed = 10f;",
    "    public float gravity = 9.8f;",
    "",
    "    CharacterController cc;",
    "",
    "    void Awake() { cc = GetComponent<CharacterController>(); }",
    "",
    "    void Update()",
    "    {",
    "        float h = Input.GetAxisRaw(\"Horizontal\"); // A/D 或 左右方向键",
    "        float v = Input.GetAxisRaw(\"Vertical\");   // W/S 或 上下方向键",
    "",
    "        // 把相机的前/右方向投影到水平面，作为移动参考系",
    "        Transform cam = Camera.main.transform;",
    "        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;",
    "        Vector3 camRight   = Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized;",
    "        Vector3 moveDir = camForward * v + camRight * h;",
    "        if (moveDir.sqrMagnitude > 1f) moveDir = moveDir.normalized; // 防止斜向超速",
    "",
    "        if (moveDir.sqrMagnitude > 0.01f)",
    "        {",
    "            // 平滑转向移动方向",
    "            Quaternion targetRot = Quaternion.LookRotation(moveDir);",
    "            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);",
    "        }",
    "",
    "        // 重力 + 位移（注意必须乘 Time.deltaTime）",
    "        cc.Move((moveDir * moveSpeed + Vector3.down * gravity) * Time.deltaTime);",
    "    }",
    "}"
  ],
  verify: [
    "Play 后 WASD 移动顺畅，Ethan 平滑转向移动方向，不会穿过地面、不会越走越快。",
    "按住鼠标右键旋转相机后，按 W 始终是“远离镜头”方向（基于相机朝向的移动验证）。"
  ],
  learn: ["Input.GetAxisRaw 与 GetAxis 的区别（键盘操作用 Raw 更跟手）", "CharacterController.Move 与重力处理", "Quaternion.LookRotation / Slerp 平滑转向", "Time.deltaTime 保证帧率无关"]
}));

// step 4
children.push.apply(children, step(4, "攻击按键与挥拳表现", "PlayerAttack.cs", {
  goal: "按 J 触发一次攻击：角色向前小冲模拟挥拳，带冷却时间（时间差控制）。",
  relation: [
    "新增 PlayerAttack.cs，当前只做表现（前冲 + 冷却），不依赖其他脚本。",
    "预留衔接：attackDamage 字段已经定义好，真正打中敌人要等到第 6 步新增 AttackHit.cs 并修改本脚本——那是你第一次体验“一个功能改两个脚本”。"
  ],
  unity: ["把 PlayerAttack.cs 挂到 Ethan 上；attackCooldown 设为 0.8 左右。"],
  design: [
    "字段：attackCooldown、lungeSpeed、lungeTime、attackDamage(第 6 步用)、lastAttackTime(上次攻击时间)",
    "方法：TryAttack() 冷却判断；AttackRoutine() 协程模拟前冲；Update() 监听按键"
  ],
  code: [
    "using System.Collections;",
    "using UnityEngine;",
    "",
    "public class PlayerAttack : MonoBehaviour",
    "{",
    "    public float attackCooldown = 0.8f; // 攻击间隔（秒）",
    "    public float lungeSpeed = 12f;      // 前冲速度",
    "    public float lungeTime = 0.15f;     // 前冲持续时间",
    "    public int attackDamage = 10;       // 攻击力（第 6 步接入伤害）",
    "",
    "    float lastAttackTime = -999f;       // 初始化为足够小，保证第一次能攻击",
    "    CharacterController cc;",
    "",
    "    void Awake() { cc = GetComponent<CharacterController>(); }",
    "",
    "    void Update()",
    "    {",
    "        if (Input.GetKeyDown(KeyCode.J)) TryAttack();",
    "    }",
    "",
    "    void TryAttack()",
    "    {",
    "        // 冷却判断：时间差不够则直接返回",
    "        if (Time.time - lastAttackTime < attackCooldown)",
    "        {",
    "            Debug.Log(\"攻击冷却中\");",
    "            return;",
    "        }",
    "        lastAttackTime = Time.time;",
    "        StartCoroutine(AttackRoutine());",
    "    }",
    "",
    "    IEnumerator AttackRoutine()",
    "    {",
    "        // 用一小段前冲模拟挥拳动作",
    "        float end = Time.time + lungeTime;",
    "        while (Time.time < end)",
    "        {",
    "            cc.Move(transform.forward * lungeSpeed * Time.deltaTime);",
    "            yield return null; // 等待下一帧",
    "        }",
    "        // TODO 第 6 步：在这里生成攻击判定，造成伤害",
    "    }",
    "}"
  ],
  verify: [
    "Play 后按 J，Ethan 向前小冲一下（模拟出拳）；快速连按 J，Console 会输出“攻击冷却中”。",
    "冷却时间在 Inspector 里调整 0.3/2.0 感受手感差异。"
  ],
  learn: ["KeyCode 按键监听", "协程 IEnumerator 与 yield return null", "Time.time 做冷却计时（比计数更直观）"]
}));

// step 5
children.push.apply(children, step(5, "生命值系统", "Health.cs", {
  goal: "角色拥有 HP，可受伤、可治疗、可死亡，并通过事件通知外部（为 UI 和胜负判定铺路）。",
  relation: [
    "新增 Health.cs，挂到双方角色，本身不依赖任何脚本。",
    "它是全项目的“数据中枢”：第 6/7 步扣血、第 8 步敌人 AI 打玩家、第 10 步血条 UI、第 11 步胜负判定，全部通过它的事件（OnDamaged/OnDeath）和 TakeDamage 接入——后面每步都会回来找它。"
  ],
  unity: ["把 Health.cs 同时挂到 Ethan 和 Robot_Kyle 上，maxHealth 默认 100。"],
  design: [
    "字段：maxHealth(序列化)、currentHealth(private 序列化)、只读属性 CurrentHealth/IsDead",
    "事件：OnDamaged / OnDeath（Action 委托，UI 和 GameManager 订阅）",
    "方法：TakeDamage(int) / Heal(int) / Die()",
    "注意点：死亡后禁止重复受伤；事件触发在状态变更之后"
  ],
  code: [
    "using System;",
    "using UnityEngine;",
    "",
    "public class Health : MonoBehaviour",
    "{",
    "    public int maxHealth = 100;",
    "    [SerializeField] int currentHealth;",
    "",
    "    public int CurrentHealth { get { return currentHealth; } }",
    "    public bool IsDead { get { return currentHealth <= 0; } }",
    "",
    "    public event Action OnDamaged; // 受伤/治疗时触发（UI 血条订阅）",
    "    public event Action OnDeath;   // 死亡时触发（胜负判定订阅）",
    "",
    "    void Start() { currentHealth = maxHealth; }",
    "",
    "    public void TakeDamage(int damage)",
    "    {",
    "        if (IsDead) return;                 // 死亡后不再受伤",
    "        currentHealth = Mathf.Max(0, currentHealth - damage);",
    "        if (OnDamaged != null) OnDamaged(); // 通知订阅者",
    "        if (IsDead) Die();",
    "    }",
    "",
    "    public void Heal(int amount)",
    "    {",
    "        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);",
    "        if (OnDamaged != null) OnDamaged();",
    "    }",
    "",
    "    void Die()",
    "    {",
    "        if (OnDeath != null) OnDeath();",
    "        // TODO 第 9 步：在这里加死亡表现（变红/淡出）",
    "        CharacterController cc = GetComponent<CharacterController>();",
    "        if (cc != null) cc.enabled = false; // 停止移动",
    "        enabled = false;                    // 停止本组件",
    "    }",
    "}"
  ],
  verify: [
    "先手动验证：在 Console 或临时写一句测试代码调用 GetComponent<Health>().TakeDamage(30)，观察 Inspector 中 currentHealth 变为 70。",
    "连续扣到 0，确认 IsDead 为 true、不再重复扣血。"
  ],
  learn: ["数据封装：public/private + [SerializeField] + 只读属性", "C# event/Action 委托与观察者模式（UI 解耦的基础）", "Mathf.Clamp/Max 边界处理"]
}));

// step 6
children.push.apply(children, step(6, "攻击命中判定（伤害敌人）", "AttackHit.cs", {
  goal: "第 4 步的挥拳真正打中 Robot_Kyle：在面前生成一个短暂的判定球，命中目标则扣血。",
  relation: [
    "两个脚本联动：新增 AttackHit.cs + 修改第 4 步的 PlayerAttack.cs（前冲结束后生成判定球）。",
    "依赖第 5 步的 Health.TakeDamage 扣血；防自伤用 owner + IsChildOf 判断。这是你第一次体验“改一个功能要动多个脚本”——编程不是线性的。"
  ],
  unity: [
    "不需要新 prefab：判定球由代码运行时创建（new GameObject + SphereCollider）。",
    "给 Ethan 在 Tag Manager 里设置标签 Player（后续 AI 用 Tag 查找玩家）。"
  ],
  design: [
    "AttackHit.cs：挂在动态生成的判定球上，字段 damage/owner；OnTriggerEnter 里检查对方 Health 并扣血，lifeTime 后自动销毁",
    "PlayerAttack.AttackRoutine：在前冲结束瞬间生成判定球，位置 = 角色前方 1 米处",
    "防自伤：判定球忽略 owner 自身及其子物体（IsChildOf 判断）"
  ],
  code: [
    "// ---------- AttackHit.cs ----------",
    "using System.Collections;",
    "using UnityEngine;",
    "",
    "public class AttackHit : MonoBehaviour",
    "{",
    "    public int damage = 10;",
    "    public float lifeTime = 0.1f;   // 判定球存在时间",
    "    public GameObject owner;        // 攻击者，防止打到自己",
    "",
    "    void Start() { Destroy(gameObject, lifeTime); }",
    "",
    "    void OnTriggerEnter(Collider other)",
    "    {",
    "        // 忽略攻击者自己",
    "        if (owner != null && other.transform.IsChildOf(owner.transform)) return;",
    "        Health h = other.GetComponentInParent<Health>();",
    "        if (h != null)",
    "        {",
    "            h.TakeDamage(damage);",
    "            // TODO 第 9 步：播放命中特效（粒子/闪屏）",
    "        }",
    "    }",
    "}",
    "",
    "// ---------- PlayerAttack.AttackRoutine 中补全 ----------",
    "// 在协程末尾（前冲结束后）添加：",
    "//   GameObject go = new GameObject(\"AttackHit\");",
    "//   go.transform.position = transform.position + transform.forward * 1f;",
    "//   SphereCollider sc = go.AddComponent<SphereCollider>();",
    "//   sc.isTrigger = true; sc.radius = 0.6f;",
    "//   AttackHit hit = go.AddComponent<AttackHit>();",
    "//   hit.damage = attackDamage;",
    "//   hit.owner = gameObject;"
  ],
  verify: [
    "贴近 Robot_Kyle 按 J，Console 出现敌人 Health 受伤信息（若已打印扣血日志），敌人 HP 归零时进入“死亡”（被禁用移动）。",
    "远离敌人挥拳不命中；攻击自己不会掉血。"
  ],
  learn: ["Trigger 碰撞与 OnTriggerEnter 触发条件（需一方有 Rigidbody 或使用 CharacterController 自身碰撞）", "GetComponentInParent 向上查找组件", "运行时动态创建 GameObject 与销毁"]
}));

// step 7
children.push.apply(children, step(7, "技能系统：冷却 + 远程能量球", "Skill.cs / SkillController.cs / Projectile.cs", {
  goal: "按 K 释放技能：从角色身前发射一个能量球，命中敌人造成伤害，带独立冷却时间（数据驱动设计）。",
  relation: [
    "新增三个脚本：Skill.cs（数据）/ SkillController.cs（行为）/ Projectile.cs（弹体）。",
    "它是第 6 步模式的复用：SkillController 扮演 PlayerAttack（生成对象），Projectile 扮演 AttackHit（命中扣血），同样依赖第 5 步的 Health。"
  ],
  unity: [
    "制作弹体预制体：Hierarchy 新建 Sphere → 命名 EnergyBall → 添加 Rigidbody 并勾选 Is Kinematic → SphereCollider 勾选 Is Trigger → 材质换成发光/高亮色 → 挂 Projectile.cs → 拖回 Project 成为预制体。",
    "把弹体预制体拖到 SkillController 的 projectilePrefab 字段；在 skills 数组里配置一个技能（伤害 20、冷却 3 秒、速度 15）。"
  ],
  design: [
    "Skill.cs：普通可序列化类（数据载体）——name/damage/cooldown/speed/lastUsedTime",
    "SkillController.cs：持有 Skill[] 数组；ReleaseSkill(index) 检查冷却 → 实例化弹体并传入数据",
    "Projectile.cs：飞行（每帧前进）、命中扣血、销毁；owner 防自伤"
  ],
  code: [
    "// ---------- Skill.cs ----------",
    "using UnityEngine;",
    "",
    "[System.Serializable]",
    "public class Skill",
    "{",
    "    public string name = \"能量球\";",
    "    public int damage = 20;",
    "    public float cooldown = 3f;   // 冷却（秒）",
    "    public float speed = 15f;     // 弹体速度",
    "    [HideInInspector] public float lastUsedTime = -999f; // 上次释放时间",
    "}",
    "",
    "// ---------- SkillController.cs ----------",
    "using UnityEngine;",
    "",
    "public class SkillController : MonoBehaviour",
    "{",
    "    public Skill[] skills;              // Inspector 中配置",
    "    public GameObject projectilePrefab; // 弹体预制体",
    "",
    "    void Update()",
    "    {",
    "        if (Input.GetKeyDown(KeyCode.K)) ReleaseSkill(0);",
    "    }",
    "",
    "    public void ReleaseSkill(int index) // 供 UI 按钮调用的公共接口",
    "    {",
    "        if (index < 0 || index >= skills.Length) return;",
    "        Skill s = skills[index];",
    "        if (Time.time - s.lastUsedTime < s.cooldown)",
    "        {",
    "            Debug.Log(s.name + \" 冷却中，还需 \" + Mathf.Ceil(s.cooldown - (Time.time - s.lastUsedTime)) + \" 秒\");",
    "            return;",
    "        }",
    "        s.lastUsedTime = Time.time;",
    "",
    "        // 从角色前方 1 米、高度 1 米处发射",
    "        Vector3 pos = transform.position + transform.forward * 1f + Vector3.up * 1f;",
    "        GameObject go = Instantiate(projectilePrefab, pos, transform.rotation);",
    "        Projectile p = go.GetComponent<Projectile>();",
    "        if (p != null) { p.damage = s.damage; p.speed = s.speed; p.owner = gameObject; }",
    "    }",
    "}",
    "",
    "// ---------- Projectile.cs ----------",
    "using UnityEngine;",
    "",
    "public class Projectile : MonoBehaviour",
    "{",
    "    public int damage = 20;",
    "    public float speed = 15f;",
    "    public GameObject owner;",
    "",
    "    void Update() { transform.Translate(Vector3.forward * speed * Time.deltaTime); }",
    "",
    "    void OnTriggerEnter(Collider other)",
    "    {",
    "        if (owner != null && other.transform.IsChildOf(owner.transform)) return;",
    "        Health h = other.GetComponentInParent<Health>();",
    "        if (h != null) h.TakeDamage(damage);",
    "        Destroy(gameObject); // 命中后消失",
    "    }",
    "}"
  ],
  verify: [
    "按 K 发射能量球直线飞行；命中 Robot_Kyle 扣血；冷却期间按键提示剩余秒数。",
    "把 skills[0].damage 调大、cooldown 调小，观察手感与数值变化，体会“数据驱动”的好处。"
  ],
  learn: ["数据与逻辑分离（Skill 数据类 vs SkillController 行为）", "Instantiate/Destroy 动态生成对象", "未来升级 ScriptableObject 的过渡思路"]
}));

// step 8
children.push.apply(children, step(8, "简单敌人 AI", "EnemyAI.cs", {
  goal: "Robot_Kyle 成为敌人：玩家进入视野范围后追击，贴近后攻击玩家（最简状态机 Idle/Chase/Attack）。",
  relation: [
    "新增 EnemyAI.cs，独立完成。",
    "依赖：通过 Tag \"Player\" 找到玩家（第 6 步已设置标签）；调用玩家 Health.TakeDamage（第 5 步）。这是第一个“主动读取其他对象数据”的脚本——之前都是别人打它，这次轮到它打别人。"
  ],
  unity: [
    "给 Robot_Kyle 添加 CharacterController 和 Health（若未挂）。",
    "把 EnemyAI.cs 挂到 Robot_Kyle；确保 Ethan 的 Tag 是 Player（脚本用 FindGameObjectWithTag 自动找到）。"
  ],
  design: [
    "枚举 AIState { Idle, Chase, Attack } + Update 中 switch 状态机",
    "Idle →（玩家进入 detectRange）→ Chase →（进入 attackRange）→ Attack →（拉开距离）→ Chase；超远 → Idle",
    "移动用 CharacterController.Move + transform.LookAt 面向玩家"
  ],
  code: [
    "using UnityEngine;",
    "",
    "public class EnemyAI : MonoBehaviour",
    "{",
    "    public enum AIState { Idle, Chase, Attack }",
    "",
    "    [Header(\"AI 参数\")]",
    "    public float detectRange = 8f;     // 发现玩家距离",
    "    public float attackRange = 1.8f;   // 攻击距离",
    "    public float moveSpeed = 3f;",
    "    public float attackInterval = 1.2f;// 攻击间隔",
    "    public int attackDamage = 8;",
    "",
    "    public Transform player;           // 玩家引用（运行时自动查找）",
    "    AIState state = AIState.Idle;",
    "    float lastAttackTime;",
    "    CharacterController cc;",
    "",
    "    void Awake()",
    "    {",
    "        cc = GetComponent<CharacterController>();",
    "        if (player == null) player = GameObject.FindGameObjectWithTag(\"Player\").transform;",
    "    }",
    "",
    "    void Update()",
    "    {",
    "        float dist = Vector3.Distance(transform.position, player.position);",
    "        switch (state)",
    "        {",
    "            case AIState.Idle:",
    "                if (dist <= detectRange) state = AIState.Chase;",
    "                break;",
    "",
    "            case AIState.Chase:",
    "                FacePlayer();",
    "                cc.Move(transform.forward * moveSpeed * Time.deltaTime);",
    "                if (dist <= attackRange) state = AIState.Attack;",
    "                else if (dist > detectRange * 1.5f) state = AIState.Idle;",
    "                break;",
    "",
    "            case AIState.Attack:",
    "                FacePlayer();",
    "                if (Time.time - lastAttackTime >= attackInterval)",
    "                {",
    "                    lastAttackTime = Time.time;",
    "                    player.GetComponent<Health>().TakeDamage(attackDamage);",
    "                }",
    "                if (dist > attackRange * 1.2f) state = AIState.Chase;",
    "                break;",
    "        }",
    "    }",
    "",
    "    void FacePlayer()",
    "    {",
    "        // 只旋转 Y 轴，避免敌人倾斜",
    "        Vector3 dir = player.position - transform.position;",
    "        dir.y = 0f;",
    "        if (dir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(dir.normalized);",
    "    }",
    "}"
  ],
  verify: [
    "Play 后操控 Ethan 靠近，Robot_Kyle 追过来并持续攻击，玩家 HP 下降；拉开足够远后敌人返回 Idle。",
    "调整 detectRange/attackInterval 观察 AI 行为变化。"
  ],
  learn: ["状态机（enum + switch）——后续一切战斗逻辑的地基", "Vector3.Distance 距离判断", "FindGameObjectWithTag 查找玩家（或拖引用）"]
}));

// step 9
children.push.apply(children, step(9, "受击反馈（闪红 / 击退 / 命中特效）", "Health.cs 扩展 + DamageFeedback", {
  goal: "打中敌人有“反馈”：受击方闪红并轻微后退，命中瞬间出现特效，战斗手感迈出第一步。",
  relation: [
    "三个脚本联动：修改 Health.cs（TakeDamage 增加来源方向参数）、修改 AttackHit.cs 与 Projectile.cs（换用新签名）。",
    "第 5 步的 Health 是被扩展而非重写——好的设计让旧功能平滑升级，这正是前面用事件解耦的回报。"
  ],
  unity: ["保持现有挂载不变；命中特效可以先用一个快速放大淡出的 Cube，或用粒子系统。"],
  design: [
    "给 Health.TakeDamage 增加一个来源参数：TakeDamage(int damage, Vector3 fromDirection)，用 (自身位置 - 来源位置) 计算击退方向",
    "闪红：协程把 Renderer 颜色改为红色，0.1 秒后恢复——注意必须用 renderer.material（实例化）而不是 sharedMaterial（会污染所有同材质物体）",
    "命中特效：在 AttackHit 与 Projectile 的命中处 Instantiate 特效对象，延迟销毁"
  ],
  code: [
    "// ---------- 在 Health.cs 中追加 ----------",
    "public void TakeDamage(int damage, Vector3 hitDirection)",
    "{",
    "    if (IsDead) return;",
    "    currentHealth = Mathf.Max(0, currentHealth - damage);",
    "    // 击退：沿被击方向推一小段",
    "    CharacterController cc = GetComponent<CharacterController>();",
    "    if (cc != null) cc.Move(hitDirection.normalized * 0.3f);",
    "    // 闪红",
    "    StartCoroutine(FlashRed());",
    "    if (OnDamaged != null) OnDamaged();",
    "    if (IsDead) Die();",
    "}",
    "",
    "IEnumerator FlashRed()",
    "{",
    "    Renderer r = GetComponentInChildren<Renderer>();",
    "    if (r == null) yield break;",
    "    r.material.color = Color.red;      // material：实例化，不影响其他物体",
    "    yield return new WaitForSeconds(0.1f);",
    "    r.material.color = Color.white;",
    "}",
    "",
    "// ---------- 调用方修改（AttackHit / Projectile） ----------",
    "// h.TakeDamage(damage, other.transform.position - transform.position);"
  ],
  verify: ["攻击敌人时它闪红并后退一点；命中处出现特效并消散；手感明显比第 6 步“有感觉”。"],
  learn: ["协程做颜色/数值过渡", "material 与 sharedMaterial 的区别（实例化 vs 共享）", "打击感的构成：反馈（颜色/位移/特效）+ 数值"]
}));

// step 10
children.push.apply(children, step(10, "UI：血条 + 技能按钮", "HealthBarUI.cs + Canvas", {
  goal: "屏幕显示双方血条，血条随事件实时刷新；技能按钮可点击释放技能并显示冷却倒计时。",
  relation: [
    "新增 HealthBarUI.cs，只订阅第 5 步 Health 的事件，不直接读写血量——UI 与逻辑解耦。",
    "技能按钮回调第 7 步的 SkillController.ReleaseSkill(0)：把键盘按键换成 UI 点击，逻辑完全不变。"
  ],
  unity: [
    "Hierarchy 新建 UI → Canvas（Screen Space - Overlay 默认即可）。",
    "创建玩家血条：Canvas 下新建 UI → Image 作背景（灰底）→ 再建一个 Image 作前景（绿/红），把前景 Image 赋给 HealthBarUI.fill。",
    "创建技能按钮：UI → Button，调整位置文字；把按钮 OnClick 事件绑定到 Ethan 的 SkillController.ReleaseSkill(0)（动态绑定代码亦可）。",
    "敌人头顶血条：建一个 World Space 的 Canvas 放在 Robot_Kyle 头顶，同样结构（此步可先只做玩家血条）。"
  ],
  design: [
    "HealthBarUI.cs：订阅目标 Health 的 OnDamaged 事件，事件触发时刷新 Image.fillAmount = CurrentHealth / maxHealth",
    "UI 只读不改：UI 不直接碰血量，通过事件被动刷新——这就是解耦",
    "技能冷却显示：按钮上的 Text 每帧显示剩余秒数（协程或 Update 里读取 Skill.lastUsedTime）"
  ],
  code: [
    "// ---------- HealthBarUI.cs ----------",
    "using UnityEngine;",
    "using UnityEngine.UI;",
    "",
    "public class HealthBarUI : MonoBehaviour",
    "{",
    "    public Image fill;     // 前景条（fillAmount 会变化）",
    "    public Health target; // 要显示的角色",
    "",
    "    void OnEnable()  { if (target != null) target.OnDamaged += Refresh; }",
    "    void OnDisable() { if (target != null) target.OnDamaged -= Refresh; }",
    "",
    "    void Start() { Refresh(); } // 初始化",
    "",
    "    void Refresh()",
    "    {",
    "        if (fill != null && target != null)",
    "            fill.fillAmount = (float)target.CurrentHealth / target.maxHealth;",
    "    }",
    "}",
    "",
    "// ---------- 技能按钮冷却倒计时（挂在按钮上） ----------",
    "// 在 Update 里读取技能的 lastUsedTime 计算剩余时间，写入按钮子物体 Text：",
    "//   float remain = skill.cooldown - (Time.time - skill.lastUsedTime);",
    "//   cooldownText.text = remain > 0 ? remain.ToString(\"0.0\") : \"\";"
  ],
  verify: [
    "受伤后玩家血条实时减少；按钮点击与键盘 K 等效释放技能；冷却期间按钮上显示倒计时数字。",
    "把 HealthBarUI 也挂到敌人头顶 Canvas，观察敌我双血条。"
  ],
  learn: ["Canvas 三种渲染模式（Overlay / Camera / World Space）", "Image.fillAmount 血条实现", "Button.onClick 事件绑定", "UI 与逻辑解耦（事件驱动刷新）"]
}));

// step 11
children.push.apply(children, step(11, "胜负判定与重置", "GameManager.cs", {
  goal: "一方死亡时显示胜负文本，按 R 一键重置双方血量与位置，形成完整可反复游玩的对局闭环。",
  relation: [
    "新增 GameManager.cs，是“总控”：订阅双方 Health.OnDeath（第 5 步）判定胜负。",
    "重置时操作第 3 步的 PlayerController/CharacterController 与第 5 步的 Health——它把前面所有脚本串成一个完整的对局闭环。"
  ],
  unity: [
    "新建空物体 GameManager，挂 GameManager.cs。",
    "把 Ethan/Robot_Kyle 的 Health 拖到 player/enemy 字段；记录初始位置（Start 里自动保存即可）。",
    "在 Canvas 下创建 Text（resultText）默认隐藏。"
  ],
  design: [
    "简单单例：public static GameManager Instance（供其他脚本访问）",
    "订阅双方 OnDeath → 显示对应文本；Update 检测 R 键 → ResetGame()",
    "ResetGame：双方 HP 回满、位置回出生点、重新启用被禁用的组件（如 CharacterController/Health）"
  ],
  code: [
    "using UnityEngine;",
    "using UnityEngine.UI;",
    "",
    "public class GameManager : MonoBehaviour",
    "{",
    "    public static GameManager Instance;",
    "",
    "    public Health player;",
    "    public Health enemy;",
    "    public Text resultText;",
    "",
    "    Vector3 playerStart, enemyStart;",
    "",
    "    void Awake()",
    "    {",
    "        Instance = this;",
    "        playerStart = player.transform.position;",
    "        enemyStart = enemy.transform.position;",
    "        player.OnDeath += () => ShowResult(\"你输了\");",
    "        enemy.OnDeath  += () => ShowResult(\"你赢了\");",
    "    }",
    "",
    "    void Update()",
    "    {",
    "        if (Input.GetKeyDown(KeyCode.R)) ResetGame();",
    "    }",
    "",
    "    void ShowResult(string text)",
    "    {",
    "        resultText.text = text;",
    "        resultText.gameObject.SetActive(true);",
    "        // TODO 扩展：显示 3 秒后自动回到选人/菜单，或冻结双方输入",
    "    }",
    "",
    "    void ResetGame()",
    "    {",
    "        // 自己实现：",
    "        // 1) player/enemy 位置回到出生点",
    "        // 2) Health 回满（重新启用组件并置满血，或加 Restore() 方法）",
    "        // 3) resultText 隐藏",
    "    }",
    "}"
  ],
  verify: ["打死敌人显示“你赢了”；被敌人打死显示“你输了”；按 R 双方满血回出生点，可继续战斗。"],
  learn: ["单例模式（简单版）", "事件订阅与场景级管理", "状态重置（战斗循环的完整闭环）"]
}));

// ---- ch4 extension ----
children.push(P([new TextRun({ text: "第四章　扩展路线（越做越复杂）", bold: true, size: 30, color: BLUE })], { heading: HeadingLevel.HEADING_1, spacing: { before: 240, after: 80 }, pageBreakBefore: true }));
children.push(P([new TextRun({ text: "完成第 11 步后，按以下六个方向逐步加料。每个方向内部按顺序做，每完成一项都是一次完整的“设计-实现-验证”循环。", size: 19, color: GRAY })], { spacing: { after: 60 } }));

const ext = [
  ["① 操作手感", ["输入缓冲：把按键压入队列，在可输入窗口内生效（格斗游戏核心）", "连击：攻击后限时内再次按键进入第二/三段（Combo 状态机）", "前摇/后摇与硬直：攻击有起手与收招时间，受击可进入 Stun 状态", "建议：把 PlayerAttack 的状态机细化成 Idle/Attack1/Attack2/Recover"]],
  ["② 动作表现", ["为模型建立真正的 AnimatorController：Idle/Walk/Run/Attack 状态与过渡", "动画事件（Animation Event）在指定帧触发伤害判定（替代延时生成判定球）", "参考 UFE Demo 角色（Assets/UFE/Demo/Resources/Characters/）与 UFE.cs 的动画驱动方式"]],
  ["③ 战斗规则", ["格挡/闪避：输入方向 + 按键进入无敌/减伤帧", "技能配置化：Skill 升级为 ScriptableObject，编辑器里建多技能", "伤害公式：攻击力/防御/暴击率/暴击伤害，Damage = f(atk, def, crit)", "无敌帧与霸体：动画关键帧期间无视伤害"]],
  ["④ 敌人 AI", ["AI 状态扩展：Idle/Patrol/Chase/Attack/Retreat 多状态", "多敌人：List<Health> 目标选择（最近者）", "Boss 机制：阶段切换（HP 阈值触发新招式）"]],
  ["⑤ 打击感", ["命中停顿（Hitstop）：命中瞬间 Time.timeScale=0 几毫秒", "屏幕震动：相机抖动协程", "粒子特效/拖尾/音效：命中音、技能音", "提示文字：伤害飘字（-20）"]],
  ["⑥ 工程化", ["对象池：技能弹幕复用 GameObject 池，避免频繁 Instantiate/Destroy", "事件总线：全局事件类（GameEvents）解耦技能/UI/AI", "存档设置：音量/键位设置 PlayerPrefs"]]
];
ext.forEach(e => {
  children.push(label(e[0]));
  e[1].forEach(t => children.push(bullet(t)));
  children.push(gap(20));
});

// ---- ch5 pitfalls ----
children.push(P([new TextRun({ text: "第五章　常见坑与排查", bold: true, size: 30, color: BLUE })], { heading: HeadingLevel.HEADING_1, spacing: { before: 240, after: 80 }, pageBreakBefore: true }));
const pit = [
  ["移动速度与帧率相关", "CharacterController.Move / Translate 忘了乘 Time.deltaTime", "运动量 × Time.deltaTime"],
  ["Trigger 检测不到", "双方都没有 Rigidbody；或未勾选 Is Trigger", "至少一方加 Rigidbody（可 IsKinematic）；勾选 Is Trigger"],
  ["闪红把整个场景变红", "用了 renderer.sharedMaterial 改了共享材质", "用 renderer.material（实例化）或改前备份颜色"],
  ["相机抖动/穿透", "相机位置直接赋值、无平滑；或目标在移动中使用 Update", "相机用 LateUpdate；可选 Vector3.SmoothDamp 平滑"],
  ["找不到 Tag \"Player\"", "Tag 未在 Tag Manager 添加", "Edit → Project Settings → Tags 添加 Player，再给 Ethan 设置"],
  ["打到自己", "判定球命中攻击者自身", "owner 字段 + IsChildOf 排除（第 6/7 步已实现）"],
  ["死亡后还能被攻击/还能动", "未在死亡时禁用控制", "Die() 里禁用 CharacterController 与移动脚本（第 5/11 步）"]
];
children.push(new Table({
  width: { size: 100, type: WidthType.PERCENTAGE }, columnWidths: [2200, 2800, 4026],
  rows: [
    new TableRow({ tableHeader: true, children: [
      new TableCell({ margins: { top: 40, bottom: 40, left: 100, right: 100 }, shading: { fill: BLUE }, children: [P([new TextRun({ text: "现象", bold: true, size: 17, color: "FFFFFF" })], { spacing: { after: 0 } })] }),
      new TableCell({ margins: { top: 40, bottom: 40, left: 100, right: 100 }, shading: { fill: BLUE }, children: [P([new TextRun({ text: "原因", bold: true, size: 17, color: "FFFFFF" })], { spacing: { after: 0 } })] }),
      new TableCell({ margins: { top: 40, bottom: 40, left: 100, right: 100 }, shading: { fill: BLUE }, children: [P([new TextRun({ text: "解决", bold: true, size: 17, color: "FFFFFF" })], { spacing: { after: 0 } })] })
    ] })
  ].concat(pit.map(r => new TableRow({ children: [
    new TableCell({ margins: { top: 40, bottom: 40, left: 100, right: 100 }, children: [P([new TextRun({ text: r[0], bold: true, size: 16, color: DARK })], { spacing: { after: 0 } })] }),
    new TableCell({ margins: { top: 40, bottom: 40, left: 100, right: 100 }, children: [P([new TextRun({ text: r[1], size: 16, color: GRAY })], { spacing: { after: 0 } })] }),
    new TableCell({ margins: { top: 40, bottom: 40, left: 100, right: 100 }, children: [P([new TextRun({ text: r[2], size: 16, color: GRAY })], { spacing: { after: 0 } })] })
  ] })))
}));
children.push(gap(80));
children.push(P([new TextRun({ text: "附录　与 UFE 引擎的关系", bold: true, size: 24, color: BLUE })], { spacing: { after: 60 } }));
children.push(P([new TextRun({ text: "本练习实现的 PlayerController / 攻击判定 / 技能冷却 / 敌人 AI，在 UFE 引擎中分别对应 ControlsScript（角色状态机）、HitBoxesScript（命中箱/受击箱判定）、MoveSetScript（招式与输入解析）、RuleBasedAI（模糊逻辑 AI）。学完本练习后，对照 UFE类图详细说明文档.docx 阅读这些类的实现，可以快速理解商业格斗引擎的设计思想。", size: 18, color: GRAY })], { spacing: { after: 40 } }));

// =================== document ===================
const doc = new Document({
  creator: "CodeBuddy",
  title: "TestDemo 战斗功能开发指南",
  description: "从零实现简单战斗系统的 11 步渐进式开发手册",
  features: { updateFields: true },
  styles: {
    default: { document: { run: { font: "微软雅黑", size: 20 } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true, run: { size: 30, bold: true, color: "1F4E79", font: "微软雅黑" }, paragraph: { spacing: { before: 240, after: 80 } } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true, run: { size: 24, bold: true, color: "17365D", font: "微软雅黑" }, paragraph: { spacing: { before: 320, after: 40 } } }
    ]
  },
  sections: [{
    properties: {},
    footers: {
      default: new Footer({
        children: [new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [new TextRun({ children: ["第 ", PageNumber.CURRENT, " 页 / 共 ", PageNumber.TOTAL_PAGES, " 页"], size: 16, color: "7F7F7F" })]
        })]
      })
    },
    children
  }]
});

Packer.toBuffer(doc).then(buf => {
  fs.writeFileSync(OUT, buf);
  console.log("OK -> " + OUT + "  (" + Math.round(buf.length / 1024) + " KB)");
}).catch(e => { console.error("FAIL:", e.message); process.exit(1); });
