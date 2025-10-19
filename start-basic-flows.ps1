# PowerShell script to start basic gRPC flow services
# Run with: .\start-basic-flows.ps1

Write-Host "🚀 Starting gRPC Services - Basic Flows" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green
Write-Host "This script starts services to test fundamental gRPC flows" -ForegroundColor Cyan

# Function to start a server in a new window
function Start-ServerInNewWindow {
    param(
        [string]$Path,
        [string]$Name,
        [string]$Port
    )
    
    Write-Host "🔄 Starting $Name on port $Port..." -ForegroundColor Yellow
    
    # Start in a new PowerShell window
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$Path'; dotnet run"
    
    # Wait a bit for the server to start
    Start-Sleep -Seconds 3
}

# Check that we are in the correct directory
if (-not (Test-Path "GrpcServer")) {
    Write-Host "❌ Error: This script must be run from the GrpcDemo project root" -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Red
    exit 1
}

Write-Host "📁 Working directory: $(Get-Location)" -ForegroundColor Cyan
Write-Host ""

# 1. Start the main gRPC server
Start-ServerInNewWindow -Path "$(Get-Location)\GrpcServer" -Name "Main gRPC Server" -Port "5001"

# 2. Start the web visualization interface
Start-ServerInNewWindow -Path "$(Get-Location)\GrpcWebUI" -Name "Web Visualization Interface" -Port "5002"

Write-Host "✅ Basic flow services started!" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Available URLs:" -ForegroundColor Cyan
Write-Host "   • Main gRPC Server: https://localhost:5001" -ForegroundColor White
Write-Host "   • Web Interface: https://localhost:5002" -ForegroundColor White
Write-Host ""
Write-Host "📚 Available gRPC flows:" -ForegroundColor Yellow
Write-Host "   📞 Unary Call - One call, one response" -ForegroundColor White
Write-Host "   📡 Server Streaming - One call, multiple responses" -ForegroundColor White
Write-Host "   📤 Client Streaming - Multiple calls, one response" -ForegroundColor White
Write-Host "   🔄 Bidirectional Streaming - Multiple calls, multiple responses" -ForegroundColor White
Write-Host ""
Write-Host "💻 To test the flows:" -ForegroundColor Cyan
Write-Host "   cd GrpcClient" -ForegroundColor White
Write-Host "   dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "   Or use the web interface at https://localhost:5002" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Or start all test clients:" -ForegroundColor Cyan
Write-Host "   .\start-basic-clients.ps1" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to close this script..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
