import os
import sys
import subprocess

def install_and_import(package):
    try:
        import openpyxl
    except ImportError:
        subprocess.check_call([sys.executable, "-m", "pip", "install", package])
        import openpyxl

install_and_import('openpyxl')
import openpyxl

dir_path = r"d:\Project\Unity Project\EditorDevelop\Assets\ExcelTable"
os.makedirs(dir_path, exist_ok=True)

wb = openpyxl.Workbook()
ws = wb.active
ws.title = "Sheet1"

# Row 1: Comments (table.Rows[0])
ws.append(["唯一ID", "武器名称", "攻击力", "是否绑定"])
# Row 2: Variable Names (table.Rows[1])
ws.append(["id", "name", "atk", "isBind"])
# Row 3: Variable Types (table.Rows[2])
ws.append(["int", "string", "float", "bool"])
# Row 4: Data (table.Rows[3])
ws.append(["1", "铁剑", "10.5", "true"])
ws.append(["2", "木弓", "8.0", "false"])

file_path = os.path.join(dir_path, "weapon.xlsx")
wb.save(file_path)
print(f"Successfully created {file_path}")
