const fs = require("fs");
const path = require("path");
const { execSync } = require("child_process");

let globalRoot = "";
try { globalRoot = execSync("npm root -g").toString().trim(); } catch (e) {}
const docx = require(globalRoot ? path.join(globalRoot, "docx") : "docx");

const { Document, Packer, Paragraph, TextRun, HeadingLevel, AlignmentType, PageBreak, TableOfContents, Table, TableRow, TableCell, WidthType, Footer, PageNumber } = docx;

const ROOT = "g:\\MyUFE";
const OUT = path.join(ROOT, "Assets", "___Doc", "UFE类图详细说明文档.docx");
const DATA = JSON.parse(fs.readFileSync(path.join(ROOT, "Assets", "___Doc", "members_analysis.json"), "utf-8"));

// ---------- XML comment cleaning ----------
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

// ---------- labels / colors ----------
const KIND_LABEL = {
  type_class: "类", type_struct: "结构体", type_interface: "接口", type_enum: "枚举",
  enum_member: "枚举成员", method: "方法", property: "属性", field: "字段", event: "事件"
};
const KIND_COLOR = {
  method: "1F4E79",   // deep blue
  property: "2E7D32", // green
  field: "8D6E63",    // brown
  event: "6A1B9A",    // purple
  enum_member: "00695C" // teal
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

// ---------- helpers ----------
function para(children, opts) {
  return new Paragraph(Object.assign({ children }, opts || {}));
}

function gap(after) {
  return new Paragraph({ children: [], spacing: { after: after || 60 } });
}

function matchTypeEntry(fd, tn) {
  return fd.members.find(m => m.kind.indexOf("type_") === 0 && (m.name === tn || (tn && tn.startsWith(m.name + "<"))));
}

function collectMembers(fd, t) {
  const out = [];
  if (!t) return out;
  const idx = fd.members.indexOf(t);
  for (let j = idx + 1; j < fd.members.length && fd.members[j].kind.indexOf("type_") !== 0; j++) {
    out.push(fd.members[j]);
  }
  return out;
}

// ---------- table builders ----------
function cell(children, opts) {
  return new TableCell(Object.assign({
    margins: { top: 50, bottom: 50, left: 100, right: 100 }
  }, opts || {}, { children: Array.isArray(children) ? children : [children] }));
}

function memberTable(members) {
  const header = new TableRow({
    tableHeader: true,
    children: [
      cell([para([new TextRun({ text: "类别", bold: true, size: 17, color: "FFFFFF" })], { alignment: AlignmentType.CENTER, spacing: { after: 0 } })], { shading: { fill: "1F4E79" } }),
      cell([para([new TextRun({ text: "成员", bold: true, size: 17, color: "FFFFFF" })], { spacing: { after: 0 } })], { shading: { fill: "1F4E79" } }),
      cell([para([new TextRun({ text: "说明", bold: true, size: 17, color: "FFFFFF" })], { spacing: { after: 0 } })], { shading: { fill: "1F4E79" } })
    ]
  });
  const rows = [header];
  members.forEach(m => {
    const label = KIND_LABEL[m.kind] || m.kind;
    const color = KIND_COLOR[m.kind] || "444444";
    const nameRuns = [new TextRun({ text: m.name, bold: true, size: 18, color: "17365D" })];
    const sig = (m.sig || "").replace(/\s+/g, " ").trim();
    if (sig) nameRuns.push(new TextRun({ break: 1, text: sig, size: 14, color: "808080", font: "Consolas" }));
    const cm = cleanXml(m.comment);
    const cmRuns = cm
      ? [new TextRun({ text: cm, size: 17, color: "404040" })]
      : [new TextRun({ text: "（无说明）", size: 16, italics: true, color: "A6A6A6" })];
    rows.push(new TableRow({
      children: [
        cell([para([new TextRun({ text: label, bold: true, size: 16, color: color })], { alignment: AlignmentType.CENTER, spacing: { after: 0 } })]),
        cell([para(nameRuns, { spacing: { after: 0 } })]),
        cell([para(cmRuns, { spacing: { after: 0 } })])
      ]
    }));
  });
  return new Table({
    width: { size: 100, type: WidthType.PERCENTAGE },
    columnWidths: [1240, 3400, 4386],
    rows
  });
}

// ---------- section builders ----------
function typeSection(fd, tn) {
  const paras = [];
  const t = matchTypeEntry(fd, tn);
  const kind = t ? (KIND_LABEL[t.kind] || "类型") : "类型";
  paras.push(new Paragraph({
    heading: HeadingLevel.HEADING_3,
    spacing: { before: 260, after: 40 },
    children: [
      new TextRun({ text: "◆ " + kind + "　", bold: true, size: 22, color: "1F4E79" }),
      new TextRun({ text: tn, bold: true, size: 22, color: "17365D" })
    ]
  }));
  if (t && t.sig) {
    paras.push(para([new TextRun({ text: t.sig.replace(/\s+/g, " ").trim(), size: 15, color: "595959", font: "Consolas" })], { indent: { left: 220 }, spacing: { after: 30 } }));
  }
  const tc = cleanXml(t && t.comment);
  if (tc) {
    paras.push(para([new TextRun({ text: tc, size: 19, color: "404040" })], { indent: { left: 220 }, spacing: { after: 60 } }));
  }
  const members = collectMembers(fd, t);
  if (members.length) {
    paras.push(memberTable(members));
    paras.push(gap(80));
  }
  return paras;
}

function fileSection(file) {
  const fd = DATA[file];
  const paras = [];
  const fname = file.split("/").pop();
  paras.push(new Paragraph({
    heading: HeadingLevel.HEADING_2,
    spacing: { before: 320, after: 40 },
    children: [new TextRun({ text: fname, bold: true, size: 24, color: "17365D" })]
  }));
  paras.push(para([
    new TextRun({ text: file + "　|　类型 " + fd.types.length + " 个　|　成员 " + fd.members.length + " 个", italics: true, size: 15, color: "7F7F7F" })
  ], { spacing: { after: 40 } }));
  paras.push(para([
    new TextRun({ text: "定义类型：", bold: true, size: 18, color: "1F4E79" }),
    new TextRun({ text: fd.types.join("、"), size: 18, color: "333333" })
  ], { spacing: { after: 60 } }));
  fd.types.forEach(tn => { paras.push.apply(paras, typeSection(fd, tn)); });

  // orphan members (not following any type entry)
  const covered = new Set();
  fd.types.forEach(tn => {
    const te = matchTypeEntry(fd, tn);
    if (te) {
      const idx = fd.members.indexOf(te);
      for (let j = idx + 1; j < fd.members.length && fd.members[j].kind.indexOf("type_") !== 0; j++) covered.add(j);
    }
  });
  const orphan = [];
  fd.members.forEach((m, i) => { if (m.kind.indexOf("type_") !== 0 && !covered.has(i)) orphan.push(m); });
  if (orphan.length) {
    paras.push(para([new TextRun({ text: "文件级其他成员", bold: true, size: 18, color: "1F4E79" })], { spacing: { before: 100, after: 40 } }));
    paras.push(memberTable(orphan));
    paras.push(gap(80));
  }
  return paras;
}

function groupSection(g) {
  const paras = [];
  paras.push(new Paragraph({
    heading: HeadingLevel.HEADING_1,
    spacing: { before: 240, after: 80 },
    pageBreakBefore: true,
    children: [new TextRun({ text: g.title, bold: true, size: 30, color: "1F4E79" })]
  }));
  paras.push(para([new TextRun({ text: g.desc, size: 19, color: "404040" })], { indent: { left: 120 }, spacing: { after: 140 } }));
  const filesInGroup = byGroup[g.id];
  const totalM = filesInGroup.reduce((s, f) => s + DATA[f].members.length, 0);
  paras.push(para([
    new TextRun({ text: "本部分共 " + filesInGroup.length + " 个文件、" + totalM + " 个成员。", italics: true, size: 16, color: "7F7F7F" })
  ], { spacing: { after: 120 } }));
  filesInGroup.forEach(f => { paras.push.apply(paras, fileSection(f)); });
  return paras;
}

// ---------- document ----------
const children = [];
const today = "2026-08-24";

// cover
children.push(para([new TextRun({ text: "UFE 格斗引擎类图", bold: true, size: 52, color: "1F4E79" })], { alignment: AlignmentType.CENTER, spacing: { before: 1800, after: 120 } }));
children.push(para([new TextRun({ text: "详细说明文档", bold: true, size: 52, color: "1F4E79" })], { alignment: AlignmentType.CENTER, spacing: { after: 600 } }));
children.push(para([new TextRun({ text: "ClassDiagram 代码文件 · 类型 · 成员 全量解析", size: 28, color: "404040" })], { alignment: AlignmentType.CENTER, spacing: { after: 900 } }));
children.push(para([new TextRun({ text: "数据来源：ClassDiagram1.cd / ClassDiagram2.cd / ClassDiagram3.cd", size: 20, color: "595959" })], { alignment: AlignmentType.CENTER, spacing: { after: 60 } }));
children.push(para([new TextRun({ text: "三个类图文件内容一致，均覆盖 UFE 引擎全部核心脚本", size: 20, color: "595959" })], { alignment: AlignmentType.CENTER, spacing: { after: 60 } }));
children.push(para([new TextRun({ text: "规模：213 个源码文件　|　439 个类型　|　4550 个成员", size: 20, color: "595959" })], { alignment: AlignmentType.CENTER, spacing: { after: 60 } }));
children.push(para([new TextRun({ text: "生成日期：" + today, size: 20, color: "595959" })], { alignment: AlignmentType.CENTER, spacing: { after: 60 } }));

// reading guide
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(para([new TextRun({ text: "阅读指南", bold: true, size: 32, color: "1F4E79" })], { heading: HeadingLevel.HEADING_1, spacing: { after: 120 } }));
children.push(para([new TextRun({ text: "本文档根据 Visual Studio 类设计器导出的 ClassDiagram1.cd 自动生成，对类图中出现的每一个代码文件、每一个类型、每一个成员进行了逐项整理与说明。", size: 20, color: "404040" })], { spacing: { after: 100 } }));
children.push(para([new TextRun({ text: "阅读方法：", bold: true, size: 20, color: "17365D" })], { spacing: { after: 40 } }));
children.push(para([new TextRun({ text: "1. 文档按 13 个功能模块分章节组织（一级标题），每章为一个 UFE 引擎子系统；每个代码文件为二级标题，每个类型为三级标题。", size: 19, color: "404040" })], { indent: { left: 240 }, spacing: { after: 40 } }));
children.push(para([new TextRun({ text: "2. 每个类型以一张表格列出全部成员，共 3 列：类别 / 成员 / 说明。类别列以颜色区分：方法（蓝）、属性（绿）、字段（棕）、事件（紫）、枚举成员（青）。", size: 19, color: "404040" })], { indent: { left: 240 }, spacing: { after: 40 } }));
children.push(para([new TextRun({ text: "3. 成员列第一行为成员名，第二行为完整签名（含修饰符、参数、返回类型）。", size: 19, color: "404040" })], { indent: { left: 240 }, spacing: { after: 40 } }));
children.push(para([new TextRun({ text: "4. 说明列取自源码 XML 文档注释（已清理标签），未写注释的成员显示“（无说明）”。", size: 19, color: "404040" })], { indent: { left: 240 }, spacing: { after: 40 } }));
children.push(para([new TextRun({ text: "5. 目录为 Word 自动目录，打开文档后若提示，请按 Ctrl+A 全选后按 F9 更新页码；或点击“引用 → 更新目录”。", size: 19, color: "404040" })], { indent: { left: 240 }, spacing: { after: 100 } }));

// TOC
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(para([new TextRun({ text: "目录", bold: true, size: 32, color: "1F4E79" })], { heading: HeadingLevel.HEADING_1, spacing: { after: 120 } }));
children.push(new TableOfContents("目录", { hyperlink: true, headingStyleRange: "1-3" }));
children.push(new Paragraph({ children: [new PageBreak()] }));

// body
GROUPS.forEach(g => { children.push.apply(children, groupSection(g)); });

// appendix
children.push(new Paragraph({ heading: HeadingLevel.HEADING_1, spacing: { before: 240, after: 80 }, pageBreakBefore: true, children: [new TextRun({ text: "附录　类图概况与统计", bold: true, size: 30, color: "1F4E79" })] }));
const statLines = [];
statLines.push("类图文件：ClassDiagram1.cd / ClassDiagram2.cd / ClassDiagram3.cd（内容一致）");
statLines.push("源码根目录：Assets/UFE/Engine/Scripts 与 Assets/UFE Addons");
statLines.push("文件总数：213　类型总数：439　成员总数：4550");
statLines.push("成员构成：字段 2247　方法 1138　枚举成员 474　属性 136　事件 39");
GROUPS.forEach(g => {
  const n = byGroup[g.id].length;
  const m = byGroup[g.id].reduce((s, f) => s + DATA[f].members.length, 0);
  statLines.push(g.title.replace(/^第[一二三四五六七八九十]+部分\s*/, "") + "：" + n + " 个文件 / " + m + " 个成员");
});
statLines.forEach(s => children.push(para([new TextRun({ text: s, size: 19, color: "404040" })], { spacing: { after: 60 }, indent: { left: 120 } })));

const doc = new Document({
  creator: "CodeBuddy",
  title: "UFE 类图详细说明文档",
  description: "UFE 格斗引擎类图全量解析（213 文件 / 439 类型 / 4550 成员）",
  features: { updateFields: true },
  styles: {
    default: { document: { run: { font: "微软雅黑", size: 20 } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true, run: { size: 30, bold: true, color: "1F4E79", font: "微软雅黑" }, paragraph: { spacing: { before: 240, after: 80 } } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true, run: { size: 24, bold: true, color: "17365D", font: "微软雅黑" }, paragraph: { spacing: { before: 320, after: 40 } } },
      { id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true, run: { size: 22, bold: true, color: "1F4E79", font: "微软雅黑" }, paragraph: { spacing: { before: 260, after: 40 } } }
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
