<#!
.SYNOPSIS
  Run backend and frontend (Angular) simultaneously.
.DESCRIPTION
  Task 4.1 script. Use -Restore to restore .NET and install npm deps. Backend on :5000, frontend dev server on :4200.
#>
param(
  [switch]$Restore,
  [int]$Port = 5000,
  [switch]$NoFrontend,
  [switch]$NoBackend
)

Write-Host "== SSE Demo Multi-Run ==" -ForegroundColor Cyan
if ($Restore) {
  Write-Host "Restoring backend..." -ForegroundColor Yellow
  dotnet restore ./backend/src/SseDemo.OrdersService/SseDemo.OrdersService.csproj
  Write-Host "Installing frontend deps..." -ForegroundColor Yellow
  pushd frontend/orders-web
  npm install
  popd
}

if (-not $NoBackend) {
  # Stop previous backend started by this script (pid file)
  $pidFile = Join-Path $PSScriptRoot '.backend.pid'
  if (Test-Path $pidFile) {
    $oldPid = Get-Content $pidFile | Select-Object -First 1
    if ($oldPid -and (Get-Process -Id $oldPid -ErrorAction SilentlyContinue)) {
      Write-Host "Stopping previous backend PID $oldPid" -ForegroundColor Yellow
      Stop-Process -Id $oldPid -Force -ErrorAction SilentlyContinue
    }
    Remove-Item $pidFile -ErrorAction SilentlyContinue
  }

  Write-Host "Starting backend (http://localhost:$Port)" -ForegroundColor Green
  $args = @('run','--project','./backend/src/SseDemo.OrdersService/SseDemo.OrdersService.csproj','--urls',"http://localhost:$Port")
  $backendProcess = Start-Process dotnet -ArgumentList $args -WorkingDirectory $PSScriptRoot -WindowStyle Hidden -PassThru
  $backendProcess.Id | Out-File $pidFile -Encoding ascii -Force
  Start-Sleep -Seconds 3
  try {
    $health = Invoke-WebRequest -UseBasicParsing -Uri "http://localhost:$Port/health" -TimeoutSec 5 -ErrorAction Stop
    Write-Host "Backend health: $($health.StatusCode)" -ForegroundColor DarkGray
  } catch {
    Write-Host "Backend not responding yet (health). Continuing..." -ForegroundColor DarkGray
  }
  Write-Host "Backend PID: $($backendProcess.Id)" -ForegroundColor DarkGray
}

if (-not $NoFrontend) {
  # Start frontend (blocking in current console)
  Write-Host "Starting frontend (http://localhost:4200)" -ForegroundColor Green
  pushd frontend/orders-web
  npm start
  popd
} else {
  Write-Host "Frontend skipped (-NoFrontend)" -ForegroundColor Yellow
}

Write-Host "Tip: To stop backend: if running, (Get-Content .backend.pid | %% { Stop-Process -Id $_ -Force })" -ForegroundColor Cyan
