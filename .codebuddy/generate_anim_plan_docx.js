// 生成"动画系统实现计划"docx 文件
// 输出到 Assets/___Doc/TestDemo动画系统实现计划.docx
const path = require('path');
const fs = require('fs');

// 动态 require 全局 docx 模块
const docxPath = path.join(process.env.APPDATA || '', 'npm/node_modules/docx');
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  Header, Footer, AlignmentType, HeadingLevel, BorderStyle, WidthType,
  ShadingType, PageNumber, PageBreak, TableOfContents, LevelFormat,
  PageOrientation, VerticalAlign
} = require(docxPath);

// === 通用样式 ===
const border = { style: BorderStyle.SINGLE, size: 1, color: "CCCCCC" };
const borders = { top: border, bottom: border, left: border, right: border };
const cellMargins = { top: 60, bottom: 60, left: 100, right: 100 };

// 页面：US Letter, 1 inch margins → content width = 9360 DXA
const PAGE_WIDTH = 12240;
const PAGE_HEIGHT = 15840;
const MARGIN = 1440;
const CONTENT_WIDTH = PAGE_WIDTH - MARGIN * 2; // 9360

// === 辅助函数 ===
function makeCell(text, width, opts = {}) {
  const runs = Array.isArray(text)
    ? text
    : [new TextRun({ text: String(text), bold: !!opts.bold, font: "Arial", size: opts.size || 20, color: opts.color })];
  return new TableCell({
    borders,
    width: { size: width, type: WidthType.DXA },
    shading: opts.shading ? { fill: opts.shading, type: ShadingType.CLEAR } : undefined,
    margins: cellMargins,
    verticalAlign: VerticalAlign.CENTER,
    children: [new Paragraph({ children: runs, alignment: opts.align || AlignmentType.LEFT })],
  });
}

function makeHeaderRow(cells, columnWidths) {
  return new TableRow({
    tableHeader: true,
    children: cells.map((text, i) =>
      makeCell(text, columnWidths[i], { bold: true, shading: "2E75B6", color: "FFFFFF", align: AlignmentType.CENTER })
    ),
  });
}

function makeDataRow(cells, columnWidths, opts = {}) {
  return new TableRow({
    children: cells.map((text, i) =>
      makeCell(text, columnWidths[i], {
        bold: opts.bold,
        shading: opts.shading,
        color: opts.color,
      })
    ),
  });
}

// === 文档内容构建 ===
const children = [];

// 标题
children.push(new Paragraph({
  heading: HeadingLevel.HEADING_1,
  alignment: AlignmentType.CENTER,
  children: [new TextRun({ text: "TestDemo 动画系统实现计划", bold: true, font: "Arial", size: 36 })]
}));
children.push(new Paragraph({
  alignment: AlignmentType.CENTER,
  children: [new TextRun({ text: "生成日期：2026-08-31 | 工作区：g:\\MyUFE", font: "Arial", size: 18, color: "888888" })]
}));
children.push(new Paragraph({ children: [] })); // 空行

// === 1. 概述 ===
children.push(new Paragraph({
  heading: HeadingLevel.HEADING_2,
  children: [new TextRun({ text: "1. 概述", bold: true, font: "Arial", size: 28 })]
}));
children.push(new Paragraph({
  children: [new TextRun({ text: "本计划为 TestDemo 项目接入完整的动画系统，解决移动/攻击/受击时模型无动作的问题。", font: "Arial", size: 22 })]
}));
children.push(new Paragraph({
  children: [new TextRun({ text: "采用 AIScripts + MyScripts 双版本实现约定：AIScripts 版为\u201C老师\u201D参考实现（带详细中文注释），MyScripts 版为用户自己动手实现的版本。Editor 工具脚本属于基础设施，不需要双版本。", font: "Arial", size: 22 })]
}));
children.push(new Paragraph({
  children: [new TextRun({ text: "角色模型：Player = Ethan，Enemy = Robot_Kyle。动画资源均来自 UFE Demo 自带动画。", font: "Arial", size: 22 })]
}));

// === 2. 动画资源映射表 ===
children.push(new Paragraph({ children: [] }));
children.push(new Paragraph({
  heading: HeadingLevel.HEADING_2,
  children: [new TextRun({ text: "2. 动画资源映射表", bold: true, font: "Arial", size: 28 })]
}));

const animMapWidths = [1400, 1200, 4800, 1960];
const animMapRows = [
  makeHeaderRow(["角色", "状态", "动画文件路径", "Animator 参数"], animMapWidths),
  makeDataRow(["Player (Ethan)", "Idle", "E_Basic_Idle.anim", "Speed = 0"], animMapWidths),
  makeDataRow(["Player (Ethan)", "Walk", "E_Basic_Walk_Forward.anim", "Speed = 1 (BlendTree)"], animMapWidths),
  makeDataRow(["Player (Ethan)", "Attack", "E_Stand_N1.anim", "Attack (trigger)"], animMapWidths),
  makeDataRow(["Player (Ethan)", "Hit", "E_Basic_Hit_High_weak.anim", "Hit (trigger)"], animMapWidths),
  makeDataRow(["Player (Ethan)", "Death", "E_Basic_Fall_Back.anim", "Death (trigger)"], animMapWidths),
  makeDataRow(["Enemy (Robot_Kyle)", "Idle", "IdleStanding.anim", "Speed = 0"], animMapWidths),
  makeDataRow(["Enemy (Robot_Kyle)", "Walk", "MoveForward.anim", "Speed = 1 (BlendTree)"], animMapWidths),
  makeDataRow(["Enemy (Robot_Kyle)", "Attack", "PunchStandingLight.anim", "Attack (trigger)"], animMapWidths),
  makeDataRow(["Enemy (Robot_Kyle)", "Hit", "HitStandingLight.anim", "Hit (trigger)"], animMapWidths),
  makeDataRow(["Enemy (Robot_Kyle)", "Death", "FallDown.anim", "Death (trigger)"], animMapWidths),
];
children.push(new Table({
  width: { size: CONTENT_WIDTH, type: WidthType.DXA },
  columnWidths: animMapWidths,
  rows: animMapRows,
}));

// === 3. Animator 状态机结构 ===
children.push(new Paragraph({ children: [] }));
children.push(new Paragraph({
  heading: HeadingLevel.HEADING_2,
  children: [new TextRun({ text: "3. Animator 状态机结构（Player 和 Enemy 结构相同）", bold: true, font: "Arial", size: 28 })]
}));

// 参数表
children.push(new Paragraph({
  children: [new TextRun({ text: "3.1 参数定义", bold: true, font: "Arial", size: 24 })]
}));
const paramWidths = [2000, 1800, 5560];
const paramRows = [
  makeHeaderRow(["参数名", "类型", "用途"], paramWidths),
  makeDataRow(["Speed", "float", "0 = 静止, 1 = 行走, BlendTree 混合 Idle↔Walk"], paramWidths),
  makeDataRow(["Attack", "trigger", "触发攻击动画，播完自动回到 Idle/Walk"], paramWidths),
  makeDataRow(["Hit", "trigger", "触发受击动画，播完自动回到 Idle/Walk"], paramWidths),
  makeDataRow(["Death", "trigger", "触发死亡动画，播完停留最后一帧（无退出转换）"], paramWidths),
];
children.push(new Table({
  width: { size: CONTENT_WIDTH, type: WidthType.DXA },
  columnWidths: paramWidths,
  rows: paramRows,
}));

// 状态表
children.push(new Paragraph({ children: [] }));
children.push(new Paragraph({
  children: [new TextRun({ text: "3.2 状态定义", bold: true, font: "Arial", size: 24 })]
}));
const stateWidths = [1800, 3400, 4160];
const stateRows = [
  makeHeaderRow(["状态名", "Motion", "说明"], stateWidths),
  makeDataRow(["Idle", "IdleClip", "默认状态，Speed < 0.1 时进入"], stateWidths),
  makeDataRow(["Walk", "BlendTree(Speed)", "Speed > 0.1 时进入，0→Idle, 1→Walk 混合"], stateWidths),
  makeDataRow(["Attack", "AttackClip", "由 Attack trigger 触发，播完 90% 回 Idle/Walk"], stateWidths),
  makeDataRow(["Hit", "HitClip", "由 Hit trigger 触发，播完 80% 回 Idle/Walk"], stateWidths),
  makeDataRow(["Death", "DeathClip", "由 Death trigger 触发，无退出转换，停留最后一帧"], stateWidths),
];
children.push(new Table({
  width: { size: CONTENT_WIDTH, type: WidthType.DXA },
  columnWidths: stateWidths,
  rows: stateRows,
}));

// 转换表
children.push(new Paragraph({ children: [] }));
children.push(new Paragraph({
  children: [new TextRun({ text: "3.3 状态转换条件", bold: true, font: "Arial", size: 24 })]
}));
const transWidths = [2200, 2200, 2600, 2360];
const transRows = [
  makeHeaderRow(["From", "To", "条件", "说明"], transWidths),
  makeDataRow(["Idle", "Walk", "Speed > 0.1", "开始移动时切换"], transWidths),
  makeDataRow(["Walk", "Idle", "Speed < 0.1", "停止移动时切换"], transWidths),
  makeDataRow(["Any State", "Attack", "Attack trigger", "无 ExitTime，立即切换"], transWidths),
  makeDataRow(["Any State", "Hit", "Hit trigger", "无 ExitTime，立即切换"], transWidths),
  makeDataRow(["Any State", "Death", "Death trigger", "无 ExitTime，立即切换"], transWidths),
  makeDataRow(["Attack", "Idle", "ExitTime 0.9", "攻击动画播完 90% 自动回 Idle"], transWidths),
  makeDataRow(["Attack", "Walk", "ExitTime 0.9 + Speed > 0.1", "攻击后仍在移动则回 Walk"], transWidths),
  makeDataRow(["Hit", "Idle", "ExitTime 0.8", "受击动画播完 80% 自动回 Idle"], transWidths),
  makeDataRow(["Hit", "Walk", "ExitTime 0.8 + Speed > 0.1", "受击后仍在移动则回 Walk"], transWidths),
  makeDataRow(["Death", "(无退出)", "—", "死亡后停留最后一帧"], transWidths),
];
children.push(new Table({
  width: { size: CONTENT_WIDTH, type: WidthType.DXA },
  columnWidths: transWidths,
  rows: transRows,
}));

// === 4. 实现步骤表 ===
children.push(new Paragraph({ children: [] }));
children.push(new Paragraph({
  heading: HeadingLevel.HEADING_2,
  children: [new TextRun({ text: "4. 实现步骤表（含 Editor 工具脚本）", bold: true, font: "Arial", size: 28 })]
}));
children.push(new Paragraph({
  children: [new TextRun({ text: "说明：步骤编号后缀 A = AIScripts 版（AI 参考实现），M = MyScripts 版（用户自己实现）。步骤 0 为 Editor 工具脚本，步骤 6 为 Unity 场景操作。", font: "Arial", size: 20, color: "666666" })]
}));

const stepWidths = [800, 2600, 1400, 3000, 1560];
const stepHeaderColors = { bold: true, shading: "2E75B6", color: "FFFFFF" };

const stepRows = [
  makeHeaderRow(["步骤", "文件", "命名空间", "改动内容", "学习点"], stepWidths),

  // 步骤 0: Editor 工具脚本
  makeDataRow([
    "0",
    "Assets/Editor/\nCreateTestDemoAnimators.cs",
    "共享\n(Editor 工具，\n无需双版本)",
    "菜单一键生成 PlayerAnimator.controller + EnemyAnimator.controller，自动引用动画资源，搭建状态机（参数/状态/转换/BlendTree）",
    "AnimatorController / BlendTree / State / Transition 的 API 用法"
  ], stepWidths, { shading: "E8F0FE" }),

  // 步骤 1A: AIScripts PlayerController
  makeDataRow([
    "1A",
    "AIScripts/\nPlayerController.cs",
    "AIScripts",
    "新增 Animator 字段，Update 末尾 SetFloat(\"Speed\", 移动中?1:0)",
    "Animator 参数驱动\nBlendTree 混合原理"
  ], stepWidths, { shading: "F0FFF0" }),

  // 步骤 1M: MyScripts PlayerController
  makeDataRow([
    "1M",
    "MyScripts/\nPlayerController.cs",
    "MyScripts",
    "同 1A（用户参照 AIScripts 自己实现）",
    "动手实践"
  ], stepWidths, { shading: "FFFFF0" }),

  // 步骤 2A: AIScripts PlayerAttack
  makeDataRow([
    "2A",
    "AIScripts/\nPlayerAttack.cs",
    "AIScripts",
    "新增 Animator 字段，TryAttack() 中 SetTrigger(\"Attack\")",
    "Trigger 触发器\n攻击动画播放"
  ], stepWidths, { shading: "F0FFF0" }),

  // 步骤 2M: MyScripts PlayerAttack
  makeDataRow([
    "2M",
    "MyScripts/\nPlayerAttack.cs",
    "MyScripts",
    "同 2A（用户参照 AIScripts 自己实现）",
    "动手实践"
  ], stepWidths, { shading: "FFFFF0" }),

  // 步骤 3A: AIScripts SkillController
  makeDataRow([
    "3A",
    "AIScripts/\nSkillController.cs",
    "AIScripts",
    "新增 Animator 字段，ReleaseSkill() 中 SetTrigger(\"Attack\")（复用攻击动画状态）",
    "复用攻击动画状态"
  ], stepWidths, { shading: "F0FFF0" }),

  // 步骤 3M: MyScripts SkillController
  makeDataRow([
    "3M",
    "MyScripts/\nSkillController.cs",
    "MyScripts",
    "同 3A（用户参照 AIScripts 自己实现）",
    "动手实践"
  ], stepWidths, { shading: "FFFFF0" }),

  // 步骤 4A: AIScripts EnemyAI
  makeDataRow([
    "4A",
    "AIScripts/\nEnemyAI.cs",
    "AIScripts",
    "新增 Animator 字段，Idle/Attack 态 SetFloat(\"Speed\",0)，Chase 态 SetFloat(\"Speed\",1)，攻击时 SetTrigger(\"Attack\")",
    "AI 状态机与动画同步"
  ], stepWidths, { shading: "F0FFF0" }),

  // 步骤 4M: MyScripts EnemyAI
  makeDataRow([
    "4M",
    "MyScripts/\nEnemyAI.cs",
    "MyScripts",
    "同 4A（用户参照 AIScripts 自己实现）",
    "动手实践"
  ], stepWidths, { shading: "FFFFF0" }),

  // 步骤 5A: AIScripts Health
  makeDataRow([
    "5A",
    "AIScripts/\nHealth.cs",
    "AIScripts",
    "新增 Animator 字段，TakeDamage 中 SetTrigger(\"Hit\")，Die() 中 SetTrigger(\"Death\")",
    "受击/死亡动画"
  ], stepWidths, { shading: "F0FFF0" }),

  // 步骤 5M: MyScripts Health
  makeDataRow([
    "5M",
    "MyScripts/\nHealth.cs",
    "MyScripts",
    "同 5A（用户参照 AIScripts 自己实现）",
    "动手实践"
  ], stepWidths, { shading: "FFFFF0" }),

  // 步骤 6: Unity 场景操作
  makeDataRow([
    "6",
    "Unity 场景操作",
    "—",
    "1. 运行菜单 TestDemo > Create Animator Controllers\n2. Player Animator 指定 PlayerAnimator\n3. Enemy Animator 指定 EnemyAnimator\n4. 确认 Apply Root Motion 关闭",
    "Animator 组件配置"
  ], stepWidths, { shading: "E8F0FE" }),
];
children.push(new Table({
  width: { size: CONTENT_WIDTH, type: WidthType.DXA },
  columnWidths: stepWidths,
  rows: stepRows,
}));

// === 5. 执行顺序 ===
children.push(new Paragraph({ children: [] }));
children.push(new Paragraph({
  heading: HeadingLevel.HEADING_2,
  children: [new TextRun({ text: "5. 执行顺序", bold: true, font: "Arial", size: 28 })]
}));
children.push(new Paragraph({
  numbering: { reference: "execOrder", level: 0 },
  children: [new TextRun({ text: "AI 先写步骤 0（Editor 脚本，共享工具）", font: "Arial", size: 22 })]
}));
children.push(new Paragraph({
  numbering: { reference: "execOrder", level: 0 },
  children: [new TextRun({ text: "AI 逐个写 AIScripts 版（步骤 1A → 2A → 3A → 4A → 5A），每个都带详细中文注释", font: "Arial", size: 22 })]
}));
children.push(new Paragraph({
  numbering: { reference: "execOrder", level: 0 },
  children: [new TextRun({ text: "用户参照 AIScripts 版，逐个自己写 MyScripts 版（步骤 1M → 2M → 3M → 4M → 5M）", font: "Arial", size: 22 })]
}));
children.push(new Paragraph({
  numbering: { reference: "execOrder", level: 0 },
  children: [new TextRun({ text: "全部代码写完后，AI 帮忙做步骤 6（Unity 场景配置）", font: "Arial", size: 22 })]
}));

// === 6. 双版本约定说明 ===
children.push(new Paragraph({ children: [] }));
children.push(new Paragraph({
  heading: HeadingLevel.HEADING_2,
  children: [new TextRun({ text: "6. 双版本约定说明（永久规则）", bold: true, font: "Arial", size: 28 })]
}));
children.push(new Paragraph({
  children: [new TextRun({ text: "以后每个功能实现都必须同时提供 AIScripts 和 MyScripts 两个版本：", font: "Arial", size: 22, bold: true })]
}));
children.push(new Paragraph({
  numbering: { reference: "convention", level: 0 },
  children: [new TextRun({ text: "AIScripts 版 = \u201C老师\u201D角色：完整参考实现，带详细中文注释，放在 Assets/TestDemo/AIScripts/ 目录（namespace AIScripts）", font: "Arial", size: 22 })]
}));
children.push(new Paragraph({
  numbering: { reference: "convention", level: 0 },
  children: [new TextRun({ text: "MyScripts 版 = \u201C学生\u201D角色：用户自己动手实现的版本，放在 Assets/TestDemo/MyScripts/ 目录（namespace MyScripts）", font: "Arial", size: 22 })]
}));
children.push(new Paragraph({
  numbering: { reference: "convention", level: 0 },
  children: [new TextRun({ text: "两个版本功能对齐、接口一致，只是命名空间不同", font: "Arial", size: 22 })]
}));
children.push(new Paragraph({
  numbering: { reference: "convention", level: 0 },
  children: [new TextRun({ text: "Editor 工具脚本属于基础设施，不需要双版本，放在 Assets/Editor/ 下即可", font: "Arial", size: 22 })]
}));
children.push(new Paragraph({
  numbering: { reference: "convention", level: 0 },
  children: [new TextRun({ text: "用户目的是学习，AI 先写 AIScripts 版作为参考，用户参照后自己写 MyScripts 版", font: "Arial", size: 22 })]
}));

// === 7. 颜色图例 ===
children.push(new Paragraph({ children: [] }));
children.push(new Paragraph({
  heading: HeadingLevel.HEADING_2,
  children: [new TextRun({ text: "7. 步骤表颜色图例", bold: true, font: "Arial", size: 28 })]
}));
const legendWidths = [2000, 3000, 4360];
const legendRows = [
  makeHeaderRow(["颜色", "类型", "说明"], legendWidths),
  makeDataRow(["浅蓝", "Editor 工具 / 场景操作", "共享步骤，不需要双版本"], legendWidths, { shading: "E8F0FE" }),
  makeDataRow(["浅绿", "AIScripts 版（老师）", "AI 参考实现，带详细注释"], legendWidths, { shading: "F0FFF0" }),
  makeDataRow(["浅黄", "MyScripts 版（学生）", "用户参照 AIScripts 自己写"], legendWidths, { shading: "FFFFF0" }),
];
children.push(new Table({
  width: { size: CONTENT_WIDTH, type: WidthType.DXA },
  columnWidths: legendWidths,
  rows: legendRows,
}));

// === 构建文档 ===
const doc = new Document({
  styles: {
    default: { document: { run: { font: "Arial", size: 22 } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 36, bold: true, font: "Arial" },
        paragraph: { spacing: { before: 240, after: 120 }, outlineLevel: 0 } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 28, bold: true, font: "Arial" },
        paragraph: { spacing: { before: 240, after: 120 }, outlineLevel: 1 } },
    ]
  },
  numbering: {
    config: [
      { reference: "execOrder",
        levels: [{ level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 720, hanging: 360 } } } }] },
      { reference: "convention",
        levels: [{ level: 0, format: LevelFormat.BULLET, text: "\u2022", alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 720, hanging: 360 } } } }] },
    ]
  },
  sections: [{
    properties: {
      page: {
        size: { width: PAGE_WIDTH, height: PAGE_HEIGHT },
        margin: { top: MARGIN, right: MARGIN, bottom: MARGIN, left: MARGIN }
      }
    },
    headers: {
      default: new Header({
        children: [new Paragraph({
          alignment: AlignmentType.RIGHT,
          children: [new TextRun({ text: "TestDemo 动画系统实现计划", font: "Arial", size: 16, color: "999999" })]
        })]
      })
    },
    footers: {
      default: new Footer({
        children: [new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [
            new TextRun({ text: "第 ", font: "Arial", size: 16, color: "999999" }),
            new TextRun({ children: [PageNumber.CURRENT], font: "Arial", size: 16, color: "999999" }),
            new TextRun({ text: " 页", font: "Arial", size: 16, color: "999999" }),
          ]
        })]
      })
    },
    children: children,
  }]
});

// === 输出 ===
const outputPath = path.join(__dirname, '..', 'Assets', '___Doc', 'TestDemo动画系统实现计划.docx');
Packer.toBuffer(doc).then(buffer => {
  fs.writeFileSync(outputPath, buffer);
  console.log('OK: ' + outputPath);
  console.log('Size: ' + (buffer.length / 1024).toFixed(1) + ' KB');
}).catch(err => {
  console.error('ERROR:', err.message);
  process.exit(1);
});
