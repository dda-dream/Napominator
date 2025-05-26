# Путь к текущей директории
$rootPath = Get-Location

# Получаем все .cs файлы
$files = Get-ChildItem -Path $rootPath -Recurse -Include *.cs

Write-Host "Начинаю обработку файлов..."

$modifiedFiles = @()

foreach ($file in $files) {
    try {
        # Читаем исходное содержимое в кодировке Windows-1251
        $originalContent = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::GetEncoding(1251))

        # Читаем содержимое в UTF-8 для сравнения (без BOM)
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        $utf8Content = [System.IO.File]::ReadAllText($file.FullName, $utf8NoBom)

        # Если содержимое совпадает > пропускаем
        if ($originalContent -eq $utf8Content) {
            Write-Host "Файл '$($file.Name)' уже в корректной кодировке. Пропускаю."
            continue
        }

        Write-Host "Обрабатываю файл: $($file.FullName)"

        # Сохраняем в UTF-8 с BOM
        $utf8WithBom = New-Object System.Text.UTF8Encoding $true
        [System.IO.File]::WriteAllText($file.FullName, $originalContent, $utf8WithBom)

        # Добавляем в список изменённых
        $modifiedFiles += $file.FullName

    }
    catch {
        Write-Warning "Ошибка при обработке файла '$($file.FullName)': $_"
    }
}

# Выводим итоговый список изменённых файлов
if ($modifiedFiles.Count -gt 0) {
    Write-Host "`nБыли изменены следующие файлы:" -ForegroundColor Green
    foreach ($f in $modifiedFiles) {
        Write-Host "- $f"
    }
}
else {
    Write-Host "`nНе было изменено ни одного файла." -ForegroundColor Yellow
}