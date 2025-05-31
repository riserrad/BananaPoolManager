Write-Host "Starting Azurite..."
Start-Process -NoNewwindow -FilePath "code" -ArgumentList "--command", "Azurite: start" -Wait

Write-Host "Running tests..."
dotnet test