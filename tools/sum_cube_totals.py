import sys
import openpyxl

FILES = [
    (r"C:\Users\Hamed Ali Khan\Downloads\Trade_Receivables_180_362696_Project_Tracking_I_Report_Atlas362696_ATLAS_ALUMINUM_W_L_L_.xlsx", "PT I (status 1)"),
    (r"C:\Users\Hamed Ali Khan\Downloads\Trade_Receivables_180_314796_Project_Tracking_II_Report_Atlas314796_ATLAS_ALUMINUM_W_L_L_.xlsx", "PT II (status 2)"),
    (r"C:\Users\Hamed Ali Khan\Downloads\Trade_Receivables_180_60810_Project_Tracking_III_Report_Atlas60810_ATLAS_ALUMINUM_W_L_L_.xlsx", "PT III (status 3)"),
]

def num(v):
    if v is None:
        return None
    if isinstance(v, (int, float)):
        return float(v)
    s = str(v).replace(",", "").strip()
    if s in ("", "-"):
        return None
    neg = s.startswith("(") and s.endswith(")")
    s = s.strip("()")
    try:
        x = float(s)
        return -x if neg else x
    except ValueError:
        return None

for path, label in FILES:
    wb = openpyxl.load_workbook(path, data_only=True)
    ws = wb.active
    rows = list(ws.iter_rows(values_only=True))
    print("=" * 100)
    print(f"{label}: {path.split(chr(92))[-1]}")
    # find header row: the one containing 'Particulars' or 'Name'
    hdr_idx = None
    for i, r in enumerate(rows[:15]):
        if r and any(str(c).strip() in ("Name", "Particulars") for c in r if c is not None):
            hdr_idx = i
            break
    if hdr_idx is None:
        print("  header not found; first rows:")
        for r in rows[:6]:
            print("   ", r)
        continue
    hdr = [str(c).strip() if c is not None else "" for c in rows[hdr_idx]]
    print(f"  header row {hdr_idx}: {hdr}")
    idx = {name: j for j, name in enumerate(hdr)}
    # identify data rows (exclude grand total footer)
    data = []
    total_row = None
    for r in rows[hdr_idx + 1:]:
        if r is None or all(c is None for c in r):
            continue
        first = str(r[0]).strip() if r[0] is not None else ""
        if first.lower().startswith("grand") or first.lower().startswith("total"):
            total_row = r
            continue
        data.append(r)
    print(f"  data rows: {len(data)}")
    for col in ("Total Contract Amount", "Adv. Rct Amount", "Balance Amount", "Plan Value"):
        if col in idx:
            j = idx[col]
            s = sum(num(r[j]) or 0 for r in data)
            print(f"  SUM rows [{col}] = {s:,.2f}")
    if total_row is not None:
        print("  Grand Total footer:")
        for col in ("Total Contract Amount", "Adv. Rct Amount", "Balance Amount", "Plan Value"):
            if col in idx:
                print(f"    [{col}] = {num(total_row[idx[col]])}")
