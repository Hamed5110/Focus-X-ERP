import json, re

path = r"C:\Users\Hamed Ali Khan\.cursor\projects\c-Users-Hamed-Ali-Khan-Focus-X-ERP\agent-transcripts\6d990114-33ba-451a-b1bc-9b8907f7853a\6d990114-33ba-451a-b1bc-9b8907f7853a.jsonl"
with open(path, encoding="utf-8", errors="replace") as f:
    lines = f.readlines()

pat = re.compile(r"getContextPath\('([^']+)'")
seen = {}
for i, line in enumerate(lines):
    if "executeServerMethod" in line or "getContextPath" in line:
        for m in pat.finditer(line):
            seen.setdefault(m.group(1), []).append(i)

for k, v in seen.items():
    print(k, "->", v[:12], ("... total " + str(len(v))) if len(v) > 12 else "")
