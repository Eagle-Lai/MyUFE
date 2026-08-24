const { execFileSync } = require("child_process");
const file = "g:\\MyUFE\\Assets\\___Doc\\TestDemo战斗功能开发指南.docx";
const xml = execFileSync("tar", ["-xOf", file, "word/document.xml"], { maxBuffer: 512 * 1024 * 1024 }).toString("utf8");

const checks = [
  ["脚本关系", "脚本关系"],
  ["分步供给", "分步供给"],
  ["最终全貌", "最终全貌"],
  ["数据中枢", "数据中枢"],
  ["改一个功能要动多个脚本", "联动说明(step6)"],
];
let ok = true;
for (const [needle, tag] of checks) {
  const n = xml.split(needle).length - 1;
  const pass = n > 0;
  if (!pass) ok = false;
  console.log((pass ? "PASS" : "FAIL") + "  " + tag + "  count=" + n);
}
// 统计每个步骤是否都有脚本关系小节（共 11 步）
const rel = xml.split("脚本关系").length - 1;
console.log("脚本关系小节总数: " + rel + "（预期 11）");
if (rel < 11) ok = false;
console.log(ok ? "ALL PASS" : "HAS FAILURE");
process.exit(ok ? 0 : 1);
