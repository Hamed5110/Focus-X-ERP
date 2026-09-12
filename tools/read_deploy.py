import json, sys, re

sys.stdout.reconfigure(encoding="utf-8", errors="replace")

path = r"C:\Users\Hamed Ali Khan\.cursor\projects\c-Users-Hamed-Ali-Khan-Focus-X-ERP\agent-transcripts\6d990114-33ba-451a-b1bc-9b8907f7853a\6d990114-33ba-451a-b1bc-9b8907f7853a.jsonl"
with open(path, encoding="utf-8", errors="replace") as f:
    lines = f.readlines()

# find all lines mentioning saveRDData or SaveRDDefinitionValue, print short context
for i, line in enumerate(lines):
    if "saveRDData" in line or "SaveRDDefinitionValue" in line:
        ev = json.loads(line)
        s = json.dumps(ev, ensure_ascii=False)
        print("=" * 25, "line", i, "len", len(s))
        print(s[:2500])
        print()
