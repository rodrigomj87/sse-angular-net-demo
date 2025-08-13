<#!
.SYNOPSIS
  Run backend and frontend (Angular) simultaneously.
.DESCRIPTION
  Task 4.1 script. Use -Restore to restore .NET and install npm deps. Backend on :5000, frontend dev server on :4200.
#>
param(
  [switch]$Restore
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

# Start backend
Write-Host "Starting backend (http://localhost:5000)" -ForegroundColor Green
powershell -NoLogo -NoProfile -WindowStyle Hidden -Command "dotnet run --project ./backend/src/SseDemo.OrdersService/SseDemo.OrdersService.csproj" | Out-Null &
Start-Sleep -Seconds 2

# Start frontend
Write-Host "Starting frontend (http://localhost:4200)" -ForegroundColor Green
pushd frontend/orders-web
npm start
popd
