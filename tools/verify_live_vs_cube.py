import pandas as pd

C = r'C:\Users\Hamed Ali Khan\Downloads\Trade_Receivables_180_487465_Project_Tracking_III_Report_Atlas487465_ATLAS_ALUMINUM_W_L_L_.xlsx'
S = r'C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\sql_live_export.csv'

cube = pd.read_excel(C, header=5).rename(columns={'Particulars': 'Name'})
sql = pd.read_csv(S)
for df in (cube, sql):
    df['Name'] = df['Name'].astype(str).str.strip()

measures = ['Total Contract Amount', 'Adv. Rct Amount', 'Balance Amount',
            'Sales Job Order', 'Production Note Amount', 'Sales Job Order Balance', 'Plan Value']
for m in measures:
    for df in (cube, sql):
        if m in df.columns:
            df[m] = pd.to_numeric(df[m], errors='coerce').fillna(0).round(0)

only_sql = set(sql.Name) - set(cube.Name)
only_cube = set(cube.Name) - set(sql.Name)
print(f'SQL rows={len(sql)} CUBE rows={len(cube)} onlySQL={len(only_sql)} onlyCUBE={len(only_cube)}')
for n in sorted(only_sql): print('  SQL-only:', n)
for n in sorted(only_cube): print('  CUBE-only:', n)

s, c = sql.set_index('Name'), cube.set_index('Name')
common = sorted(set(sql.Name) & set(cube.Name))
mis = 0
for n in common:
    for m in measures:
        if m not in s.columns or m not in c.columns: continue
        sv, cv = s.loc[n, m], c.loc[n, m]
        if isinstance(sv, pd.Series): sv = sv.sum()
        if isinstance(cv, pd.Series): cv = cv.sum()
        if abs(sv - cv) > 0.5:
            print(f'MISMATCH {n[:45]:47} {m:26} SQL={sv:>10.0f} CUBE={cv:>10.0f}')
            mis += 1
print(f'\nTotal mismatches: {mis} across {len(common)} common accounts')
print('\nTotals:')
for m in measures:
    if m in s.columns and m in c.columns:
        print(f'{m:26} SQL={sql[m].sum():>12.0f} CUBE={cube[m].sum():>12.0f} diff={sql[m].sum()-cube[m].sum():>10.0f}')
