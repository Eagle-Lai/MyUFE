# 长期记忆 MEMORY.md

## 项目概览
- 工作区 `g:\MyUFE` 是 UFE（Universal Fighting Engine）格斗引擎 Unity 工程，源码位于 `Assets/UFE/Engine/Scripts` 与 `Assets/UFE Addons`。
- 工程根目录的 `ClassDiagram1.cd / ClassDiagram2.cd / ClassDiagram3.cd` 是 Visual Studio 类设计器类图文件，三个文件内容一致，覆盖 213 个源码文件 / 439 个类型 / 4550 个成员。

## 类图分析文档归档位置（重要）
- 所有类图分析产物统一归档在 **`g:\MyUFE\Assets\___Doc\`**（Unity 资源目录，___ 前缀便于排序置顶）：
  - `UFE类图详细说明文档.docx` — 成员级全量解析（213 文件/439 类型/4550 成员，13 个功能模块章节，每个方法/属性/枚举均含签名+作用说明，说明文字取自源码中文 XML 注释）
  - `ClassDiagram1_阅读文档.docx` — 早期的阅读文档（结构概览、模块统计、关键类型速查）
  - `members_analysis.json` — 结构化数据源（213 文件 × 4550 成员，含 kind/name/sig/comment），Unity 会按 TextAsset 导入，无害
- 文档生成脚本保留在 **`g:\MyUFE\.codebuddy\generate_docx.js`**（不能移入 Assets，避免 Unity 把 .js 当 UnityScript 编译报错）。重新生成/定制文档：`node g:\MyUFE\.codebuddy\generate_docx.js`，输出到工程根目录。
- 文档生成链路：Python 脚本解析 .cd XML 提取类型→文件映射 → Python 静态分析 .cs 提取成员+注释（方法签名跨行拼接）→ Node + 全局 docx-js（`npm root -g` 动态 require，docx@9.7.1）生成 docx。

## 环境要点
- 系统 Python 为 2.7（临时脚本需 Py2 兼容写法；json 写文件用 ascii 编码；命令行传中文参数会乱码，验证/脚本一律写成 .py 文件再执行）。
- Node v24 可用；全局 npm 模块在 `C:\Users\lzy\AppData\Roaming\npm\node_modules`（docx、xlsx 等）。
- 注意 `___temp` 目录是项目自带的 Assets 副本，勿删。
