# Quick rebuild script for development
# This script rebuilds only what's necessary without a full clean

Write-Host "Rebuilding Canvas.Windows.Forms projects..." -ForegroundColor Cyan

# Build the core library
dotnet build WebForms.Canvas\Canvas.Windows.Forms.csproj

# Build the WASM host
dotnet build WebForms.Canvas.Host\Canvas.Windows.Forms.Host.csproj

# Build the server
dotnet build Canvas.Windows.Forms.Host.Server\Canvas.Windows.Forms.Host.Server.csproj

Write-Host "Build complete! Run the server with:" -ForegroundColor Green
Write-Host "  dotnet run --project Canvas.Windows.Forms.Host.Server" -ForegroundColor Yellow


#dotnet run --project Canvas.Windows.Forms.Host.Server