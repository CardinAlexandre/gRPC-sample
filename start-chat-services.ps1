# PowerShell script to start gRPC chat services
# Run with: .\start-chat-services.ps1

Write-Host "💬 Starting gRPC Services - Chat Hub" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host "This script starts services to test multi-client chat" -ForegroundColor Cyan

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
if (-not (Test-Path "GrpcChatServer")) {
    Write-Host "❌ Error: This script must be run from the GrpcDemo project root" -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Red
    exit 1
}

Write-Host "📁 Working directory: $(Get-Location)" -ForegroundColor Cyan
Write-Host ""

# Start the chat server
Start-ServerInNewWindow -Path "$(Get-Location)\GrpcChatServer" -Name "Chat Hub Server" -Port "5003"

Write-Host "✅ Chat service started!" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Available URL:" -ForegroundColor Cyan
Write-Host "   • Chat Server: https://localhost:5003" -ForegroundColor White
Write-Host ""
Write-Host "💬 Chat features:" -ForegroundColor Yellow
Write-Host "   🔄 Bidirectional streaming" -ForegroundColor White
Write-Host "   👥 Simultaneous multi-clients" -ForegroundColor White
Write-Host "   🔔 Connection/disconnection notifications" -ForegroundColor White
Write-Host "   💬 Real-time messages" -ForegroundColor White
Write-Host ""
Write-Host "💻 To test chat:" -ForegroundColor Cyan
Write-Host "   .\start-chat-clients.ps1" -ForegroundColor White
Write-Host ""
Write-Host "   Or start multiple clients manually:" -ForegroundColor White
Write-Host "   cd GrpcClientChatHub" -ForegroundColor White
Write-Host "   dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to close this script..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
