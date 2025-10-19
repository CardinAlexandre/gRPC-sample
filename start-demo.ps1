# Main PowerShell script to start gRPC demonstrations
# Run with: .\start-demo.ps1

Write-Host "🚀 gRPC Demo - Demonstration Mode Selection" -ForegroundColor Green
Write-Host "===========================================" -ForegroundColor Green
Write-Host ""

while ($true) {
    Write-Host "🎯 Choose demonstration mode:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. 📚 Basic gRPC Flows" -ForegroundColor White
    Write-Host "   • Unary, Server Streaming, Client Streaming, Bidirectional" -ForegroundColor Gray
    Write-Host "   • Web visualization interface" -ForegroundColor Gray
    Write-Host "   • Test clients and monitoring" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. 💬 Multi-Client Chat" -ForegroundColor White
    Write-Host "   • Bidirectional streaming" -ForegroundColor Gray
    Write-Host "   • Communication between multiple clients" -ForegroundColor Gray
    Write-Host "   • Shared chat hub" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3. 🔧 Development Tools" -ForegroundColor White
    Write-Host "   • Compile all projects" -ForegroundColor Gray
    Write-Host "   • Clean up processes" -ForegroundColor Gray
    Write-Host ""
    Write-Host "0. ❌ Exit" -ForegroundColor White
    Write-Host ""
    
    Write-Host "Your choice: " -NoNewline -ForegroundColor Yellow
    $choice = Read-Host
    
    switch ($choice) {
        "1" {
            Write-Host ""
            Write-Host "📚 Starting basic gRPC flows..." -ForegroundColor Green
            Write-Host ""
            
            # Check if servers are already running
            $processes = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
            if ($processes) {
                Write-Host "⚠️ Some dotnet processes are already running." -ForegroundColor Yellow
                Write-Host "Do you want to stop them and restart? (y/n): " -NoNewline -ForegroundColor Yellow
                $restart = Read-Host
                if ($restart -eq "y" -or $restart -eq "Y") {
                    Write-Host "🛑 Stopping existing processes..." -ForegroundColor Yellow
                    Get-Process -Name "dotnet" | Stop-Process -Force
                    Start-Sleep -Seconds 2
                }
            }
            
            Write-Host "🚀 Starting servers..." -ForegroundColor Green
            & ".\start-basic-flows.ps1"
            
            Write-Host ""
            Write-Host "💻 Do you want to start test clients? (y/n): " -NoNewline -ForegroundColor Yellow
            $startClients = Read-Host
            if ($startClients -eq "y" -or $startClients -eq "Y") {
                & ".\start-basic-clients.ps1"
            }
            
            break
        }
        "2" {
            Write-Host ""
            Write-Host "💬 Starting multi-client chat..." -ForegroundColor Green
            Write-Host ""
            
            Write-Host "🚀 Starting chat server..." -ForegroundColor Green
            & ".\start-chat-services.ps1"
            
            Write-Host ""
            Write-Host "💻 Do you want to start multiple chat clients? (y/n): " -NoNewline -ForegroundColor Yellow
            $startClients = Read-Host
            if ($startClients -eq "y" -or $startClients -eq "Y") {
                & ".\start-chat-clients.ps1"
            }
            
            break
        }
        "3" {
            Write-Host ""
            Write-Host "🔧 Development tools..." -ForegroundColor Green
            Write-Host ""
            Write-Host "1. Compile all projects" -ForegroundColor White
            Write-Host "2. Clean up processes" -ForegroundColor White
            Write-Host "3. Return to main menu" -ForegroundColor White
            Write-Host ""
            Write-Host "Choice: " -NoNewline -ForegroundColor Yellow
            $toolChoice = Read-Host
            
            switch ($toolChoice) {
                "1" {
                    Write-Host "🔨 Compiling all projects..." -ForegroundColor Green
                    dotnet build
                    Write-Host ""
                    Write-Host "✅ Compilation completed!" -ForegroundColor Green
                    Write-Host ""
                }
                "2" {
                    Write-Host "🛑 Stopping all dotnet processes..." -ForegroundColor Yellow
                    Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
                    Write-Host "✅ Processes stopped!" -ForegroundColor Green
                    Write-Host ""
                }
            }
            break
        }
        "0" {
            Write-Host ""
            Write-Host "👋 Goodbye!" -ForegroundColor Green
            Write-Host "Thank you for testing gRPC Demo!" -ForegroundColor Cyan
            exit 0
        }
        default {
            Write-Host ""
            Write-Host "❌ Invalid choice. Please select 1, 2, 3 or 0." -ForegroundColor Red
            Write-Host ""
        }
    }
    
    Write-Host ""
    Write-Host "Press any key to continue..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    Write-Host ""
}
