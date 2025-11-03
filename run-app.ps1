# Launch CredVault MAUI Windows App
$ErrorActionPreference = "Stop"

Write-Host "Building CredVault.Mobile..." -ForegroundColor Cyan
Set-Location "$PSScriptRoot\src\CredVault.Mobile"

# Build the app
dotnet build -f net9.0-windows10.0.19041.0 -c Debug

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nLaunching app..." -ForegroundColor Green
    
    # Deploy and run
    dotnet build -t:Run -f net9.0-windows10.0.19041.0 -c Debug -p:_DoNotCheckMSIX=true
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`nNote: The app may require Windows Developer Mode to be enabled." -ForegroundColor Yellow
        Write-Host "To enable Developer Mode:" -ForegroundColor Yellow
        Write-Host "  Settings > Privacy & Security > For developers > Developer Mode" -ForegroundColor Yellow
        Write-Host "`nAlternatively, you can use Visual Studio to run the app." -ForegroundColor Yellow
    }
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
}

Read-Host "`nPress Enter to exit"
