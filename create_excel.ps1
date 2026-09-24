$targetDir = "d:\Project\Unity Project\EditorDevelop\Assets\ExcelTable"
if (!(Test-Path -Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir | Out-Null
}

$excelPath = Join-Path -Path $targetDir -ChildPath "weapon.xlsx"
if (Test-Path -Path $excelPath) {
    Remove-Item -Path $excelPath -Force
}

try {
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false
    
    $workbook = $excel.Workbooks.Add()
    $worksheet = $workbook.Worksheets.Item(1)
    
    $worksheet.Cells.Item(1, 1).Value2 = "WeaponID"
    $worksheet.Cells.Item(1, 2).Value2 = "WeaponName"
    $worksheet.Cells.Item(1, 3).Value2 = "Atk"
    $worksheet.Cells.Item(1, 4).Value2 = "CritRate"
    
    $worksheet.Cells.Item(2, 1).Value2 = "id"
    $worksheet.Cells.Item(2, 2).Value2 = "name"
    $worksheet.Cells.Item(2, 3).Value2 = "atk"
    $worksheet.Cells.Item(2, 4).Value2 = "critRate"
    
    $worksheet.Cells.Item(3, 1).Value2 = "int"
    $worksheet.Cells.Item(3, 2).Value2 = "string"
    $worksheet.Cells.Item(3, 3).Value2 = "int"
    $worksheet.Cells.Item(3, 4).Value2 = "float"
    
    $worksheet.Cells.Item(4, 1).Value2 = 1001
    $worksheet.Cells.Item(4, 2).Value2 = "Sword"
    $worksheet.Cells.Item(4, 3).Value2 = 10
    $worksheet.Cells.Item(4, 4).Value2 = 0.05
    
    $worksheet.Cells.Item(5, 1).Value2 = 1002
    $worksheet.Cells.Item(5, 2).Value2 = "Iron Sword"
    $worksheet.Cells.Item(5, 3).Value2 = 25
    $worksheet.Cells.Item(5, 4).Value2 = 0.10

    $worksheet.Cells.Item(6, 1).Value2 = 1003
    $worksheet.Cells.Item(6, 2).Value2 = "Dragon Blade"
    $worksheet.Cells.Item(6, 3).Value2 = 999
    $worksheet.Cells.Item(6, 4).Value2 = 0.50
    
    $workbook.SaveAs($excelPath, 51)
    $workbook.Close()
    $excel.Quit()
    
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($worksheet) | Out-Null
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($workbook) | Out-Null
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
    
    Write-Host "Excel file created at: $excelPath"
} catch {
    Write-Host "Failed to create Excel file via COM: $_"
}
