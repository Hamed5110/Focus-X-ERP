import pandas as pd

CUBES = {
    'PT I  (12:22)': r'C:\Users\Hamed Ali Khan\Downloads\Trade_Receivables_180_362696_Project_Tracking_I_Report_Atlas362696_ATLAS_ALUMINUM_W_L_L_.xlsx',
    'PT II (12:24)': r'C:\Users\Hamed Ali Khan\Downloads\Trade_Receivables_180_314796_Project_Tracking_II_Report_Atlas314796_ATLAS_ALUMINUM_W_L_L_.xlsx',
    'PT III(11:38)': r'C:\Users\Hamed Ali Khan\Downloads\Trade_Receivables_180_60810_Project_Tracking_III_Report_Atlas60810_ATLAS_ALUMINUM_W_L_L_.xlsx',
    'PT III(old 487465)': r'C:\Users\Hamed Ali Khan\Downloads\Trade_Receivables_180_487465_Project_Tracking_III_Report_Atlas487465_ATLAS_ALUMINUM_W_L_L_.xlsx',
}

codes = ['AC-2071', 'AC-6277', 'AC-7074', 'AC-7287',   # cube-only in PT III 60810
         'AC-3758', 'AC-506', 'AC-5850', 'AC-6584', 'AC-6698',  # SQL-only status 3
         'AC-4441', 'AC-5780']  # SQL-only status 1 with amounts

for label, path in CUBES.items():
    try:
        cube = pd.read_excel(path, header=5)
    except Exception as e:
        print(f'{label}: cannot read ({e})')
        continue
    cube['Code'] = cube['Code'].astype(str).str.strip()
    present = [c for c in codes if c in set(cube.Code)]
    print(f'{label}: present -> {present if present else "none of them"}')
