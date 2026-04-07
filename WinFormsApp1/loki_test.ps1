# Мониторинг логов в реальном времени
Write-Host "Мониторинг логов Loki в реальном времени..." -ForegroundColor Green

while ($true) {
    try {
        $query = @{
            query = '{app="winforms-app"}'
            limit = 1
            direction = "forward"
        } | ConvertTo-Json
        
        $response = Invoke-RestMethod -Uri "http://10.66.66.49:3100/loki/api/v1/query" `
            -Method Post `
            -Body $query `
            -ContentType "application/json" `
            -ErrorAction Stop
        
        if ($response.data.result.Count -gt 0) {
            $log = $response.data.result[0].values[0]
            $timestamp = [DateTimeOffset]::FromUnixTimeMilliseconds([long]($log[0].Substring(0,13)))
            $message = $log[1]
            
            Write-Host "$($timestamp.ToString('HH:mm:ss.fff')) - $message" -ForegroundColor Cyan
        }
        
        Start-Sleep -Milliseconds 100
    }
    catch {
        Write-Host "Ошибка: $_" -ForegroundColor Red
        Start-Sleep -Seconds 1
    }
}
