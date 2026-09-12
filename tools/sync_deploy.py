import io, re, sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace")

main_path = r"C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\III Summary Report.sql"
deploy_path = r"C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\III Summary Report - DEPLOY.sql"

main = io.open(main_path, encoding="utf-8").read()
# strip the leading /* ... */ header comment
m = re.search(r"\*/\s*(SELECT[\s\S]+)$", main)
query = m.group(1).strip()

# escape single quotes for the N'...' literal
escaped = query.replace("'", "''")

deploy = io.open(deploy_path, encoding="utf-8").read()
start = deploy.index("DECLARE @q nvarchar(max) = N'")
end = deploy.index("';", start)
new_deploy = deploy[:start] + "DECLARE @q nvarchar(max) = N'" + escaped + deploy[end:]

io.open(deploy_path, "w", encoding="utf-8", newline="\n").write(new_deploy)
print("DEPLOY query section synced. Query length:", len(escaped))
print("Contains date filter:", "YEAR(GETDATE())" in escaped)
print("Contains grp rule:", "GroupStatus" in escaped)
print("Contains TR filter:", "Trade Receivables" in escaped)
