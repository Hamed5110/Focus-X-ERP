import json

q = open(r"C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\cloud_diff_query6.sql", encoding="utf-8").read()
expr = "window.__diffQuery6=" + json.dumps(q) + ";'stored len '+window.__diffQuery6.length"
open(r"C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\set_q6.js", "w", encoding="utf-8").write(expr)
print("ok", len(expr))
