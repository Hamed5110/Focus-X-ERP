import pandas as pd
import numpy as np

Q = r'C:\Users\Hamed Ali Khan\Downloads\III_testing397653_ATLAS_ALUMINUM_W_L_L_.xlsx'
C = r'C:\Users\Hamed Ali Khan\Downloads\Trade_Receivables_180_487465_Project_Tracking_III_Report_Atlas487465_ATLAS_ALUMINUM_W_L_L_.xlsx'

def load(path, name_col):
    df = pd.read_excel(path, header=5)
    df = df.rename(columns={df.columns[0]: 'Name'})
    df['Name'] = df['Name'].astype(str).str.strip()
    df = df[~df['Name'].isin(['nan', 'None', '', 'Total', 'Grand Total'])]
    df = df[~df['Name'].str.contains('Total', case=False, na=False)]
    return df

sql = load(Q, 'Name')
cube = load(C, 'Particulars')

print('SQL rows:', len(sql), '| CUBE rows:', len(cube))

measures = ['Total Contract Amount', 'Adv. Rct Amount', 'Balance Amount',
            'Sales Job Order', 'Production Note Amount', 'Sales Job Order Balance', 'Plan Value']

for m in measures:
    for df in (sql, cube):
        if m in df.columns:
            df[m] = pd.to_numeric(df[m], errors='coerce').fillna(0).round(0)

only_sql = set(sql.Name) - set(cube.Name)
only_cube = set(cube.Name) - set(sql.Name)
print('\nOnly in SQL:', len(only_sql))
for n in sorted(only_sql): print('  SQL-only:', n)
print('Only in CUBE:', len(only_cube))
for n in sorted(only_cube): print('  CUBE-only:', n)

common = sorted(set(sql.Name) & set(cube.Name))
print('\nCommon accounts:', len(common))

s = sql.set_index('Name')
c = cube.set_index('Name')

print('\n=== MISMATCHES (common accounts) ===')
any_mis = False
for n in common:
    for m in measures:
        if m not in s.columns or m not in c.columns:
            continue
        sv, cv = s.loc[n, m], c.loc[n, m]
        if isinstance(sv, pd.Series): sv = sv.sum()
        if isinstance(cv, pd.Series): cv = cv.sum()
        if abs(sv - cv) > 0.5:
            print(f'{n[:45]:47} {m:26} SQL={sv:>12.0f} CUBE={cv:>12.0f} diff={sv-cv:>12.0f}')
            any_mis = True
if not any_mis:
    print('(none — all common accounts match)')

print('\n=== TOTALS (common accounts only) ===')
for m in measures:
    if m in s.columns and m in c.columns:
        st = sum(s.loc[n, m].sum() if isinstance(s.loc[n, m], pd.Series) else s.loc[n, m] for n in common)
        ct = sum(c.loc[n, m].sum() if isinstance(c.loc[n, m], pd.Series) else c.loc[n, m] for n in common)
        print(f'{m:26} SQL={st:>12.0f} CUBE={ct:>12.0f} diff={st-ct:>12.0f}')

print('\n=== TOTALS (all rows each side) ===')
for m in measures:
    if m in sql.columns and m in cube.columns:
        print(f'{m:26} SQL={sql[m].sum():>12.0f} CUBE={cube[m].sum():>12.0f}')
