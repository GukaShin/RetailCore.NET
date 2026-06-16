@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0.."

set GIT=E:\Git\cmd\git.exe
set GIT_AUTHOR_NAME=guka shinjikashvili
set GIT_AUTHOR_EMAIL=120255283+GukaShin@users.noreply.github.com
set GIT_COMMITTER_NAME=guka shinjikashvili
set GIT_COMMITTER_EMAIL=120255283+GukaShin@users.noreply.github.com
set MSG=%TEMP%\retailcore-msg.txt
set HEAD_SHA=

"%GIT%" checkout --orphan rebuild-main 2>nul
"%GIT%" rm -rf --cached . 2>nul

> "%MSG%" echo Add solution scaffold, Docker Compose, and README.
"%GIT%" add .gitignore README.md Directory.Build.props RetailCore.NET.sln docker-compose.yml
call :mkcommit
if errorlevel 1 exit /b 1

> "%MSG%" echo Add domain entities, enums, and shared primitives.
"%GIT%" add src\RetailCore.Domain
call :mkcommit
if errorlevel 1 exit /b 1

> "%MSG%" echo Add API contracts and request/response DTOs.
"%GIT%" add src\RetailCore.Contracts
call :mkcommit
if errorlevel 1 exit /b 1

> "%MSG%" echo Add application abstractions, services, and validators.
"%GIT%" add src\RetailCore.Application
call :mkcommit
if errorlevel 1 exit /b 1

> "%MSG%" echo Add infrastructure: EF Core persistence, Redis cache, JWT auth, and services.
"%GIT%" add src\RetailCore.Infrastructure
call :mkcommit
if errorlevel 1 exit /b 1

> "%MSG%" echo Add ASP.NET Core API with controllers, middleware, and auth.
"%GIT%" add src\RetailCore.Api
call :mkcommit
if errorlevel 1 exit /b 1

> "%MSG%" echo Add unit and integration tests for checkout and domain logic.
"%GIT%" add tests\RetailCore.Tests
call :mkcommit
if errorlevel 1 exit /b 1

"%GIT%" branch -D main 2>nul
"%GIT%" branch -m main
"%GIT%" log --oneline

echo.
"%GIT%" log --format=%%B | findstr /I "co-authored cursor" >nul
if not errorlevel 1 (
  echo ERROR: Co-authored-by trailer found.
  exit /b 1
)
echo OK: clean history with no Cursor co-author

del "%MSG%" 2>nul
endlocal
exit /b 0

:mkcommit
set PARENT=
if defined HEAD_SHA set PARENT=-p !HEAD_SHA!
for /f %%t in ('"%GIT%" write-tree') do set TREE=%%t
for /f %%c in ('"%GIT%" commit-tree !TREE! !PARENT! -F "%MSG%"') do set HEAD_SHA=%%c
"%GIT%" reset --hard !HEAD_SHA!
exit /b 0
