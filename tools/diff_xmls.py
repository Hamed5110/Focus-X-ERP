import difflib
import sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace")

base = r"C:\Users\Hamed Ali Khan\Downloads"
files = [
    "Project Tracking III Report Atlas (1).xml",
    "Project Tracking III Report Atlas (2).xml",
    "Project Tracking III Report Atlas (3).xml",
    "Project Tracking III Report Atlas (4).xml",
    "Project Tracking III Report Atlas (5).xml",
]

texts = {}
for f in files:
    with open(f"{base}\\{f}", encoding="utf-8") as fh:
        texts[f] = fh.read().splitlines()

for a, b in zip(files, files[1:]):
    print(f"\n########## DIFF {a}  ->  {b} ##########")
    diff = list(difflib.unified_diff(texts[a], texts[b], lineterm="", n=1))
    if not diff:
        print("  (identical)")
    else:
        for line in diff:
            print("  " + line)
