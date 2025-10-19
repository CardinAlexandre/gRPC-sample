# PowerShell script to start gRPC chat clients
# Run with: .\start-chat-clients.ps1

Write-Host "💬 Starting gRPC Clients - Chat Hub" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host "This script starts multiple chat clients to test multi-client communication" -ForegroundColor Cyan

# Function to start a client in a new window
function Start-ChatClientInNewWindow {
    param(
        [string]$Path,
        [string]$ClientNumber
    )
    
    Write-Host "🔄 Starting Chat Client #$ClientNumber..." -ForegroundColor Yellow
    
    # Start in a new PowerShell window with a custom title
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$Path'; Write-Host 'Chat Client #$ClientNumber' -ForegroundColor Cyan; dotnet run; Write-Host 'Press any key to close...' -ForegroundColor Yellow; $null = `$Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')" -WindowStyle Normal
    
    # Wait a bit for the client to start
    Start-Sleep -Seconds 2
}

# Check that we are in the correct directory
if (-not (Test-Path "GrpcClientChatHub")) {
    Write-Host "❌ Error: This script must be run from the GrpcDemo project root" -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Red
    exit 1
}

Write-Host "📁 Working directory: $(Get-Location)" -ForegroundColor Cyan
Write-Host ""

# Start multiple chat clients
Start-ChatClientInNewWindow -Path "$(Get-Location)\GrpcClientChatHub" -ClientNumber "1"
Start-ChatClientInNewWindow -Path "$(Get-Location)\GrpcClientChatHub" -ClientNumber "2"
Start-ChatClientInNewWindow -Path "$(Get-Location)\GrpcClientChatHub" -ClientNumber "3"

Write-Host "✅ Chat clients started!" -ForegroundColor Green
Write-Host ""
Write-Host "🎯 Started clients:" -ForegroundColor Cyan
Write-Host "   💬 Chat Client #1 - First participant" -ForegroundColor White
Write-Host "   💬 Chat Client #2 - Second participant" -ForegroundColor White
Write-Host "   💬 Chat Client #3 - Third participant" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Required server:" -ForegroundColor Cyan
Write-Host "   • Chat Server: https://localhost:5003" -ForegroundColor White
Write-Host ""
Write-Host "💡 Multi-client chat test:" -ForegroundColor Yellow
Write-Host "   • Each client can send messages" -ForegroundColor White
Write-Host "   • All clients receive messages from others" -ForegroundColor White
Write-Host "   • Connection/disconnection notifications are visible" -ForegroundColor White
Write-Host "   • Messages are colored and timestamped" -ForegroundColor White
Write-Host ""
Write-Host "🚀 To start the chat server:" -ForegroundColor Cyan
Write-Host "   .\start-chat-services.ps1" -ForegroundColor White
Write-Host ""
Write-Host "💬 Now you can test chat between multiple clients!" -ForegroundColor Green
Write-Host ""
Write-Host "Press any key to close this script..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
