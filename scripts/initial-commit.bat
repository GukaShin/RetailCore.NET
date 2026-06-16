@echo off
setlocal
cd /d "%~dp0.."

if not exist .git (
  git init
  git branch -M main
)

git add .

for /f %%i in ('git write-tree') do set TREE=%%i
if "%TREE%"=="" (
  echo Failed to create tree.
  exit /b 1
)

(
echo Initial commit: RetailCore.NET POS foundation vertical slice.
echo.
echo ASP.NET Core API with Clean Architecture, PostgreSQL, Redis, JWT auth, concurrency-safe checkout, and tests.
) > "%TEMP%\retailcore-commit-msg.txt"

for /f %%i in ('git commit-tree %TREE% -F "%TEMP%\retailcore-commit-msg.txt"') do set NEW=%%i
if "%NEW%"=="" (
  echo Failed to create commit.
  exit /b 1
)

git reset --hard %NEW%
del "%TEMP%\retailcore-commit-msg.txt"

git log -1 --format="Author: %%an <%%ae>"
git log -1 --format="%%B"
endlocal
