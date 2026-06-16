# RetailCore local dev startup (Windows PowerShell)
# Usage: powershell -ExecutionPolicy Bypass -File scripts/start-dev.ps1

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")

Write-Host ""
Write-Host "=== RetailCore Dev Startup ===" -ForegroundColor Cyan
Write-Host ""

# 1. Docker
Write-Host "[1/4] Starting PostgreSQL + Redis (Docker)..." -ForegroundColor Yellow
Set-Location $root
try {
    docker compose up -d
    if ($LASTEXITCODE -ne 0) { throw "docker compose failed" }
} catch {
    Write-Host ""
    Write-Host "ERROR: Docker failed. Start Docker Desktop first, then run this script again." -ForegroundColor Red
    Write-Host $_.Exception.Message
    exit 1
}
Write-Host "      Docker OK" -ForegroundColor Green

# 2. Frontend deps
Write-Host "[2/4] Checking frontend dependencies..." -ForegroundColor Yellow
Set-Location (Join-Path $root "frontend")
if (-not (Test-Path "node_modules")) {
    Write-Host "      Running npm install (first time only)..."
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: npm install failed. Install Node.js from https://nodejs.org" -ForegroundColor Red
        exit 1
    }
}
Write-Host "      Frontend deps OK" -ForegroundColor Green

# 3. API in new window
Write-Host "[3/4] Starting API (new terminal window)..." -ForegroundColor Yellow
$apiCmd = "cd '$root'; Write-Host 'RetailCore API - http://localhost:5176' -ForegroundColor Cyan; dotnet run --project src/RetailCore.Api"
Start-Process powershell -ArgumentList "-NoExit", "-Command", $apiCmd

Write-Host "      Waiting for API to start..."
$ready = $false
for ($i = 0; $i -lt 30; $i++) {
    Start-Sleep -Seconds 2
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:5176/swagger/index.html" -UseBasicParsing -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($r.StatusCode -eq 200) { $ready = $true; break }
    } catch {
        try {
            $r = Invoke-WebRequest -Uri "http://localhost:5176/api/categories" -UseBasicParsing -TimeoutSec 2 -ErrorAction SilentlyContinue
            if ($r.StatusCode -eq 200) { $ready = $true; break }
        } catch { }
    }
}
if ($ready) {
    Write-Host "      API OK at http://localhost:5176" -ForegroundColor Green
} else {
    Write-Host '      API still starting - check the API terminal window for errors.' -ForegroundColor Yellow
}

# 4. Frontend in new window
Write-Host "[4/4] Starting frontend (new terminal window)..." -ForegroundColor Yellow
$feCmd = "cd '$root\frontend'; Write-Host 'RetailCore UI - http://localhost:5173' -ForegroundColor Cyan; npm run dev"
Start-Process powershell -ArgumentList "-NoExit", "-Command", $feCmd

Write-Host ""
Write-Host "=== Ready ===" -ForegroundColor Green
Write-Host "  UI:  http://localhost:5173"
Write-Host "  API: http://localhost:5176/swagger"
Write-Host ""
Write-Host 'Login: cashier@retailcore.local / Password123!'
Write-Host "Keep the API and frontend terminal windows open."
Write-Host ""
