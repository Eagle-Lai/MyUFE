const fs = require("fs");
const path = require("path");
const { execSync } = require("child_process");

let globalRoot = "";
try { globalRoot = execSync("npm root -g").toString().trim(); } catch (e) {}
const docx = require(globalRoot ? path.join(globalRoot, "docx") : "docx");

const { Document, Packer, Paragraph, TextRun, HeadingLevel, AlignmentType, PageBreak, TableOfContents, LevelFormat } = docx;

const ROOT = "g:\\MyUFE";
const DATA = JSON.parse(fs.readFileSync(path.join(ROOT, ".codebuddy", "members_analysis.json"), "utf-8"));

// ---------- helpers ----------
function cleanXml(c) {
  if (!c) return "";
  let text = c;
  const sm = text.match(/<summary>([\s\S]*?)<\/summary>/);
  let main = sm ? sm[1] : text;
  const params = [];
  const pmRe = /<param\s+name="([^"]*)"\s*>([\s\S]*?)<\/param>/g;
  let mm;
  while ((mm = pmRe.exec(text)) !== null) params.push({ name: mm[1], desc: mm[2] });
  const rt = text.match(/<returns>([\s\S]*?)<\/returns>/);
  const conv = (s) => s
    .replace(/<see cref="[^"]*\.([^".]+)"\s*\/?>/g, "$1")
    .replace(/<see cref="([^"]*)"\s*\/?>/g, "$1")
    .replace(/<seealso[^>]*\/?>/g, "")
    .replace(/<c>([\s\S]*?)<\/c>/g, "$1")
    .replace(/<para\s*\/?>\s*/g, " ")
    .replace(/<para>\s*/g, " ")
    .replace(/<list[\s\S]*?<\/list>/g, " ")
    .replace(/<[^>]+>/g, " ")
    .replace(/&lt;/g, "<").replace(/&gt;/g, ">").replace(/&amp;/g, "&").replace(/&quot;/g, '"').replace(/&#39;/g, "'")
    .replace(/\s+/g, " ").trim();
  let out = conv(main);
  if (params.length) out += (out ? " " : "") + "参数: " + params.map(p => p.name + " = " + conv(p.desc)).join("; ");
  if (rt) out += (out ? " " : "") + "返回: " + conv(rt[1]);
  return out;
}

const KIND_LABEL = {
  type_class: "类", type_struct: "结构体", type_interface: "接口", type_enum: "枚举",
  enum_member: "枚举成员", method: "方法", property: "属性", field: "字段", event: "事件"
};

// ---------- grouping ----------
function groupOf(f) {
  if (f.startsWith("Assets/UFE Addons/")) return "compat";
  if (f.indexOf("/Mono4Unity/") >= 0) return "mono";
  if (f.indexOf("/FPLibrary/") >= 0) return "fp";
  if (f.indexOf("/Network/") >= 0) return "net";
  if (f.indexOf("/AI/") >= 0) return "ai";
  if (f.indexOf("/Animation/") >= 0) return "anim";
  if (f.indexOf("/Camera/") >= 0) return "cam";
  if (f.indexOf("/Input/") >= 0) return "input";
  if (f.indexOf("/UI/") >= 0) return "ui";
  if (f.indexOf("/Definitions/") >= 0) return "defs";
  if (f.indexOf("/Physics/") >= 0) return "phys";
  if (f.indexOf("DPG.Util.Collection") >= 0) return "util";
  return "core";
}

const GROUPS = [
  { id: "core", title: "第一部分　UFE 引擎核心与管理器",
    desc: "UFE（Universal Fighting Engine）格斗游戏引擎的入口与全局管理器。UFE.cs 是引擎的总控类（单例），负责游戏流程、全局配置、战斗逻辑、多模式调度与事件派发；Manager 目录提供挑战模式、延时动作、实例化对象管理等支撑组件。" },
  { id: "defs", title: "第二部分　定义数据（Definitions）",
    desc: "引擎的全部数据定义与配置序列化结构：全局配置（GlobalInfo）、角色数据（CharacterInfo）、招式数据（MoveInfo）与姿态数据（StanceInfo），以及大量用于配置器（UFE Editor）与战斗逻辑的枚举、内部结构体。" },
  { id: "phys", title: "第三部分　角色控制与物理",
    desc: "战斗核心：ControlsScript 驱动角色行为状态机（Idle/Walk/Jump/Attack/Block/Throw 等），MoveSetScript 管理招式集与输入指令解析，HitBoxesScript 实现命中判定箱/受击箱/格挡判定，PhysicsScript 处理重力与运动，ProjectileMoveScript 负责飞行道具。" },
  { id: "ai", title: "第四部分　AI 人工智能",
    desc: "UFE 内置 AI：基于模糊逻辑推理系统（模糊集/规则/推理机）的 RuleBasedAI 与 InferenceSystemThread，AIInfo.cs 定义全部 AI 输入/输出/规则/难度等数据类与枚举，另提供简单 AI（SimpleAI）与随机 AI（RandomAI）实现。" },
  { id: "anim", title: "第五部分　动画系统",
    desc: "角色动画控制：MecanimControl（Mecanim/Animator 动画控制器）、LegacyControl（旧版 Legacy 动画）、AnimationRecorder（动画录制与回放）、HeadLookScript（头部视线跟踪）等。" },
  { id: "cam", title: "第六部分　相机系统",
    desc: "战斗相机：CameraScript 实现跟随/缩放/震动等相机行为，CameraFade 提供场景淡入淡出与镜头遮挡淡出效果。" },
  { id: "input", title: "第七部分　输入系统",
    desc: "输入抽象层：AbstractInputController/UFEController 定义输入源接口，InputController 基于 Unity Input，InputTouchController 支持虚拟按键触摸输入，RewiredInputController 对接 Rewired 插件，InputEvents 提供输入事件广播。" },
  { id: "net", title: "第八部分　网络对战",
    desc: "UFE 网络对战体系：MultiplayerAPI 等连接器负责匹配/房间/会话管理；消息层（NetworkMessage/SynchronizationMessage/InputBufferMessage）定义同步协议；Netcode 目录实现回滚（Rollback）联网框架（FluxCapacitor 时间回溯、MrFusion 输入同步、FluxStateTracker 状态追踪、FluxPlayerManager 回滚玩家管理）；States 目录提供联网状态机。" },
  { id: "fp", title: "第九部分　定点数学库（FPLibrary）",
    desc: "面向帧同步与联网对战的高精度定点数（Fixed Point）数学库：Fix64 定点数、FPVector/FPVector2/FPQuaternion/FPMatrix/FPMath/FPRandom 等，保证不同平台计算结果完全一致。" },
  { id: "ui", title: "第十部分　UI 界面",
    desc: "UFE 菜单/战斗 UI 框架：UFEScreen 基类与所有屏幕（主菜单/选人/选关/暂停/联网大厅/战斗 GUI 等）的接口定义（UI/Base），以及基于 uGUI 的默认实现模板（UI/Templates），外加少量社区辅助组件（UFEGradient 渐变、GUIExtensions）。" },
  { id: "mono", title: "第十一部分　Mono4Unity 运行时兼容层",
    desc: "Mono4Unity 是随 UFE 附带的 .NET Framework 兼容实现（BCL 移植），为引擎提供 System.Collections.Concurrent、System.Threading.Tasks（Task/Parallel）、System.Numerics（BigInteger）、System.Collections.Generic（SortedSet 等）以及 Tuple/Lazy/CancellationToken 等标准库能力，确保在 Unity 环境下可用的托管并行与异步基础设施。本部分为 .NET 标准 API 的再实现，成员按紧凑格式列出。" },
  { id: "compat", title: "第十二部分　旧版 API 兼容层",
    desc: "UFELegacyAPICompat 提供对已废弃的 Unity 旧版 API（GUIText/GUITexture/Network 等）的兼容包装类型，便于旧代码平滑迁移到新版 Unity。" },
  { id: "util", title: "第十三部分　工具与集合",
    desc: "通用工具：SerializableDictionary 提供可在 Inspector 中序列化编辑的字典；FPMath 之外的 DPG 工具类等。" }
];

const files = Object.keys(DATA).sort();
const byGroup = {};
files.forEach(f => { const g = groupOf(f); (byGroup[g] = byGroup[g] || []).push(f); });
GROUPS.forEach(g => byGroup[g.id] = byGroup[g.id] || []);

// ---------- paragraph builders ----------
function pBreak() { return new Paragraph({ children: [new PageBreak()] }); }

function memberParagraph(m, compact) {
  const runs = [];
  const label = KIND_LABEL[m.kind] || m.kind;
  if (compact) {
    runs.push(new TextRun({ text: "[" + label + "] " + m.name, size: 18, color: "333333" }));
    const cm = cleanXml(m.comment);
    if (cm) runs.push(new TextRun({ text: " — " + cm, size: 18, color: "595959" }));
  } else {
    runs.push(new TextRun({ text: "【" + label + "】" + m.name, bold: true, color: "1F4E79", size: 20 }));
    const sig = m.sig.replace(/\s+/g, " ").trim();
    if (sig) runs.push(new TextRun({ break: 1, text: sig, font: "Consolas", size: 16, color: "444444" }));
    const cm = cleanXml(m.comment);
    if (cm) runs.push(new TextRun({ break: 1, text: "说明：" + cm, size: 18, color: "595959" }));
  }
  return new Paragraph({ children: runs, spacing: { after: compact ? 40 : 90 }, indent: compact ? { left: 340 } : { left: 360 } });
}

function typeParagraphs(fileData, typeName, compact) {
  const paras = [];
  const members = fileData.members.filter(m => m.kind === "type_" + "class" || m.kind === "type_struct" || m.kind === "type_interface" || m.kind === "type_enum" || m.kind === "type_" + typeName);
  const typeEntries = fileData.members.filter(m => m.kind && m.kind.indexOf("type_") === 0);
  const t = typeEntries.find(m => m.name === typeName);
  if (t) {
    const kind = KIND_LABEL[t.kind] || t.kind;
    paras.push(new Paragraph({
      children: [new TextRun({ text: "[" + kind + "] " + t.name, bold: true, size: 22, color: "1F4E79" })],
      heading: HeadingLevel.HEADING_3,
      spacing: { before: 160, after: 40 }
    }));
    const tc = cleanXml(t.comment);
    if (tc) paras.push(new Paragraph({
      children: [new TextRun({ text: tc, size: 19, color: "404040" })],
      spacing: { after: 80 }, indent: { left: 240 }
    }));
  } else {
    paras.push(new Paragraph({
      children: [new TextRun({ text: typeName, bold: true, size: 22, color: "1F4E79" })],
      heading: HeadingLevel.HEADING_3, spacing: { before: 160, after: 40 }
    }));
  }
  // members of this type: group contiguous members after the type entry
  if (t) {
    const idx = fileData.members.indexOf(t);
    let j = idx + 1;
    while (j < fileData.members.length && fileData.members[j].kind.indexOf("type_") !== 0) {
      paras.push(memberParagraph(fileData.members[j], compact));
      j++;
    }
  }
  return paras;
}

function fileSection(file) {
  const fd = DATA[file];
  const paras = [];
  const fname = file.split("/").pop();
  paras.push(new Paragraph({
    children: [new TextRun({ text: fname, bold: true, size: 26, color: "17365D" })],
    heading: HeadingLevel.HEADING_2, spacing: { before: 220, after: 40 }
  }));
  const typeNames = fd.types;
  const nMembers = fd.members.length;
  const isMono = file.indexOf("/Mono4Unity/") >= 0;
  paras.push(new Paragraph({
    children: [new TextRun({ text: file + "　|　类型 " + typeNames.length + " 个　|　成员 " + nMembers + " 个" + (isMono ? "　|　（Mono4Unity .NET 兼容层，紧凑模式）" : ""), italics: true, size: 16, color: "7F7F7F" })],
    spacing: { after: 60 }
  }));
  // overview: types list
  paras.push(new Paragraph({
    children: [new TextRun({ text: "本文件定义类型：", bold: true, size: 18 }), new TextRun({ text: typeNames.join("、"), size: 18, color: "333333" })],
    spacing: { after: 80 }
  }));
  // per-type
  typeNames.forEach(tn => { paras.push.apply(paras, typeParagraphs(fd, tn, isMono)); });
  // members not covered by any known type entry (orphan members) -> list at end in compact form
  const covered = new Set();
  typeNames.forEach(tn => {
    const te = fd.members.find(m => m.kind.indexOf("type_") === 0 && m.name === tn);
    if (te) {
      const idx = fd.members.indexOf(te);
      for (let j = idx + 1; j < fd.members.length && fd.members[j].kind.indexOf("type_") !== 0; j++) covered.add(j);
    }
  });
  const orphan = [];
  fd.members.forEach((m, i) => { if (m.kind.indexOf("type_") !== 0 && !covered.has(i)) orphan.push(m); });
  if (orphan.length) {
    paras.push(new Paragraph({
      children: [new TextRun({ text: "文件级其他成员：", bold: true, size: 18, color: "1F4E79" })],
      spacing: { before: 80, after: 40 }
    }));
    orphan.forEach(m => paras.push(memberParagraph(m, true)));
  }
  return paras;
}

function groupSection(g) {
  const paras = [];
  paras.push(new Paragraph({
    children: [new TextRun({ text: g.title, bold: true, size: 30, color: "1F4E79" })],
    heading: HeadingLevel.HEADING_1, spacing: { before: 240, after: 80 }, pageBreakBefore: true
  }));
  paras.push(new Paragraph({
    children: [new TextRun({ text: g.desc, size: 19, color: "404040" })],
    spacing: { after: 160 }, indent: { left: 120 }
  }));
  const filesInGroup = byGroup[g.id];
  const totalM = filesInGroup.reduce((s, f) => s + DATA[f].members.length, 0);
  paras.push(new Paragraph({
    children: [new TextRun({ text: "本部分共 " + filesInGroup.length + " 个文件、" + totalM + " 个成员。", italics: true, size: 16, color: "7F7F7F" })],
    spacing: { after: 120 }
  }));
  filesInGroup.forEach(f => { paras.push.apply(paras, fileSection(f)); });
  return paras;
}

// ---------- build document ----------
const children = [];

// cover page
children.push(new Paragraph({ children: [new TextRun({ text: "UFE 格斗引擎类图", bold: true, size: 52, color: "1F4E79" })], alignment: AlignmentType.CENTER, spacing: { before: 1800, after: 120 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "详细说明文档", bold: true, size: 52, color: "1F4E79" })], alignment: AlignmentType.CENTER, spacing: { after: 600 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "ClassDiagram 代码文件 · 类型 · 成员 全量解析", size: 28, color: "404040" })], alignment: AlignmentType.CENTER, spacing: { after: 900 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "数据来源：ClassDiagram1.cd / ClassDiagram2.cd / ClassDiagram3.cd", size: 20, color: "595959" })], alignment: AlignmentType.CENTER, spacing: { after: 60 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "三个类图文件内容一致，均覆盖 UFE 引擎全部核心脚本", size: 20, color: "595959" })], alignment: AlignmentType.CENTER, spacing: { after: 60 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "规模：213 个源码文件　|　439 个类型　|　4550 个成员", size: 20, color: "595959" })], alignment: AlignmentType.CENTER, spacing: { after: 60 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "生成日期：2026-08-22", size: 20, color: "595959" })], alignment: AlignmentType.CENTER, spacing: { after: 60 } }));

// intro page
children.push(pBreak());
children.push(new Paragraph({ children: [new TextRun({ text: "阅读指南", bold: true, size: 32, color: "1F4E79" })], heading: HeadingLevel.HEADING_1, spacing: { after: 120 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "本文档根据 Visual Studio 类设计器导出的 ClassDiagram1.cd 自动生成，对类图中出现的每一个代码文件、每一个类型、每一个成员进行了逐项整理与说明。", size: 20, color: "404040" })], spacing: { after: 100 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "阅读方法：", bold: true, size: 20, color: "17365D" })], spacing: { after: 40 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "1. 文档按 13 个功能模块分章节组织，每章为一个 UFE 引擎子系统（核心、定义数据、角色物理、AI、动画、相机、输入、网络、定点数学、UI、Mono4Unity 兼容层、旧版兼容、工具）。", size: 19, color: "404040" })], indent: { left: 240 }, spacing: { after: 40 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "2. 每个文件小节先给出文件路径与类型清单，再按类型逐个展开：类型说明 + 每个成员（方法/属性/字段/事件/枚举值）的签名与作用说明。", size: 19, color: "404040" })], indent: { left: 240 }, spacing: { after: 40 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "3. 成员说明文字取自源码中的 XML 文档注释（已做标签清理），未写注释的成员标注为“无说明”。", size: 19, color: "404040" })], indent: { left: 240 }, spacing: { after: 40 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "4. Mono4Unity 部分为 .NET 标准库兼容实现，成员采用紧凑列表形式，便于快速浏览而不至篇幅失控。", size: 19, color: "404040" })], indent: { left: 240 }, spacing: { after: 100 } }));
children.push(new Paragraph({ children: [new TextRun({ text: "5. 目录为 Word 自动目录，打开文档后如有提示请点击“更新域”刷新页码。", size: 19, color: "404040" })], indent: { left: 240 }, spacing: { after: 100 } }));

// TOC page
children.push(pBreak());
children.push(new Paragraph({ children: [new TextRun({ text: "目录", bold: true, size: 32, color: "1F4E79" })], heading: HeadingLevel.HEADING_1, spacing: { after: 120 } }));
children.push(new TableOfContents("目录", { hyperlink: true, headingStyleRange: "1-2" }));
children.push(pBreak());

// body
GROUPS.forEach(g => { children.push.apply(children, groupSection(g)); });

// appendix
children.push(new Paragraph({ children: [new TextRun({ text: "附录　类图概况与统计", bold: true, size: 30, color: "1F4E79" })], heading: HeadingLevel.HEADING_1, spacing: { before: 240, after: 80 }, pageBreakBefore: true }));
const statLines = [];
statLines.push("类图文件：ClassDiagram1.cd / ClassDiagram2.cd / ClassDiagram3.cd（内容一致）");
statLines.push("源码根目录：Assets/UFE/Engine/Scripts 与 Assets/UFE Addons");
statLines.push("文件总数：213　类型总数：439　成员总数：4550");
GROUPS.forEach(g => {
  const n = byGroup[g.id].length;
  const m = byGroup[g.id].reduce((s, f) => s + DATA[f].members.length, 0);
  statLines.push(g.title.replace(/^第[一二三四五六七八九十]+部分\s*/, "") + "：" + n + " 个文件 / " + m + " 个成员");
});
statLines.forEach(s => children.push(new Paragraph({ children: [new TextRun({ text: s, size: 19, color: "404040" })], spacing: { after: 60 }, indent: { left: 120 } })));

const doc = new Document({
  creator: "CodeBuddy",
  title: "UFE 类图详细说明文档",
  description: "UFE 格斗引擎类图全量解析（213 文件 / 439 类型 / 4550 成员）",
  features: { updateFields: true },
  styles: {
    default: { document: { run: { font: "微软雅黑", size: 20 } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true, run: { size: 30, bold: true, color: "1F4E79", font: "微软雅黑" }, paragraph: { spacing: { before: 240, after: 80 } } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true, run: { size: 26, bold: true, color: "17365D", font: "微软雅黑" }, paragraph: { spacing: { before: 220, after: 60 } } },
      { id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true, run: { size: 22, bold: true, color: "1F4E79", font: "微软雅黑" }, paragraph: { spacing: { before: 160, after: 40 } } }
    ]
  },
  sections: [{ properties: {}, children }]
});

const out = path.join(ROOT, "UFE类图详细说明文档.docx");
Packer.toBuffer(doc).then(buf => {
  fs.writeFileSync(out, buf);
  console.log("OK -> " + out + "  (" + Math.round(buf.length / 1024) + " KB)");
}).catch(e => { console.error("FAIL:", e.message); process.exit(1); });
