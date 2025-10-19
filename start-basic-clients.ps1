# PowerShell script to start basic gRPC flow clients
# Run with: .\start-basic-clients.ps1

Write-Host "🚀 Starting gRPC Clients - Basic Flows" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "This script starts all clients to test fundamental gRPC flows" -ForegroundColor Cyan

# Function to start a client in a new window
function Start-ClientInNewWindow {
    param(
        [string]$Path,
        [string]$Name,
        [string]$Description
    )
    
    Write-Host "🔄 Starting $Name..." -ForegroundColor Yellow
    Write-Host "   📁 $Path" -ForegroundColor Gray
    Write-Host "   📝 $Description" -ForegroundColor Gray
    
    # Start in a new PowerShell window with a custom title
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$Path'; dotnet run; Write-Host 'Press any key to close...' -ForegroundColor Yellow; $null = `$Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')" -WindowStyle Normal
    
    # Wait a bit for the client to start
    Start-Sleep -Seconds 2
}

# Check that we are in the correct directory
if (-not (Test-Path "GrpcClient")) {
    Write-Host "❌ Error: This script must be run from the GrpcDemo project root" -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Red
    exit 1
}

Write-Host "📁 Working directory: $(Get-Location)" -ForegroundColor Cyan
Write-Host ""

# 1. Interactive Console Client
Start-ClientInNewWindow -Path "$(Get-Location)\GrpcClient" -Name "Interactive Console Client" -Description "Interactive menu to test all gRPC flow types"

# 2. Monitor Client
Start-ClientInNewWindow -Path "$(Get-Location)\GrpcClientMonitor" -Name "Monitor Client" -Description "Real-time monitoring with Server Streaming to receive notifications"

# 3. Stress Test Client
Start-ClientInNewWindow -Path "$(Get-Location)\GrpcClientStressTest" -Name "Stress Test Client" -Description "Load and performance testing with all gRPC patterns"

Write-Host "✅ All basic flow clients are starting..." -ForegroundColor Green
Write-Host ""
Write-Host "🎯 Started clients:" -ForegroundColor Cyan
Write-Host "   📞 Console Client - Complete interactive interface" -ForegroundColor White
Write-Host "   📊 Monitor Client - Server notification monitoring" -ForegroundColor White
Write-Host "   ⚡ Stress Test Client - Performance and load testing" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Required servers:" -ForegroundColor Cyan
Write-Host "   • gRPC Server: https://localhost:5001" -ForegroundColor White
Write-Host "   • Web Interface: https://localhost:5002" -ForegroundColor White
Write-Host ""
Write-Host "💡 Usage tips:" -ForegroundColor Yellow
Write-Host "   • Use Console Client for interactive testing" -ForegroundColor White
Write-Host "   • Monitor Client displays all server notifications" -ForegroundColor White
Write-Host "   • Stress Test Client tests server performance" -ForegroundColor White
Write-Host ""
Write-Host "🚀 To start servers:" -ForegroundColor Cyan
Write-Host "   .\start-basic-flows.ps1" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to close this script..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
