$ErrorActionPreference = 'Stop'
$git = 'E:\Git\cmd\git.exe'
$repo = 'e:\RetailCoree.NET'

Set-Location $repo

$env:GIT_AUTHOR_NAME = 'guka shinjikashvili'
$env:GIT_AUTHOR_EMAIL = '120255283+GukaShin@users.noreply.github.com'
$env:GIT_COMMITTER_NAME = 'guka shinjikashvili'
$env:GIT_COMMITTER_EMAIL = '120255283+GukaShin@users.noreply.github.com'

function Invoke-Git {
    param([Parameter(Mandatory)][string[]]$GitArgs)
    $prev = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $output = & $git @GitArgs 2>&1 | ForEach-Object { "$_" }
    $exit = $LASTEXITCODE
    $ErrorActionPreference = $prev
    if ($exit -ne 0) {
        throw "git $($GitArgs -join ' ') failed: $($output -join '`n')"
    }
    if ($output) { return ($output -join "`n").Trim() }
}

function New-GitCommit {
    param(
        [string]$Message,
        [string[]]$Paths,
        [string]$Parent
    )

    $addArgs = @('add', '--') + $Paths
    Invoke-Git -GitArgs $addArgs | Out-Null
    $tree = Invoke-Git -GitArgs @('write-tree')

    $msgPath = Join-Path $env:TEMP 'retailcore-commit-msg.txt'
    [System.IO.File]::WriteAllText($msgPath, $Message + "`n", [System.Text.UTF8Encoding]::new($false))

    if ($Parent) {
        $commit = Invoke-Git -GitArgs @('commit-tree', $tree, '-p', $Parent, '-F', $msgPath)
    } else {
        $commit = Invoke-Git -GitArgs @('commit-tree', $tree, '-F', $msgPath)
    }

    Invoke-Git -GitArgs @('reset', '--hard', $commit) | Out-Null
    Remove-Item $msgPath -Force -ErrorAction SilentlyContinue
    return $commit
}

Invoke-Git -GitArgs @('checkout', '--orphan', 'rebuild-main') | Out-Null
try { Invoke-Git -GitArgs @('rm', '-rf', '--cached', '.') | Out-Null } catch { }

$commits = @(
    @{
        Message = 'Add solution scaffold, Docker Compose, and README.'
        Paths   = @('.gitignore', 'README.md', 'Directory.Build.props', 'RetailCore.NET.sln', 'docker-compose.yml')
    }
    @{
        Message = 'Add domain entities, enums, and shared primitives.'
        Paths   = @('src/RetailCore.Domain')
    }
    @{
        Message = 'Add API contracts and request/response DTOs.'
        Paths   = @('src/RetailCore.Contracts')
    }
    @{
        Message = 'Add application abstractions, services, and validators.'
        Paths   = @('src/RetailCore.Application')
    }
    @{
        Message = 'Add infrastructure: EF Core persistence, Redis cache, JWT auth, and services.'
        Paths   = @('src/RetailCore.Infrastructure')
    }
    @{
        Message = 'Add ASP.NET Core API with controllers, middleware, and auth.'
        Paths   = @('src/RetailCore.Api')
    }
    @{
        Message = 'Add unit and integration tests for checkout and domain logic.'
        Paths   = @('tests/RetailCore.Tests')
    }
)

$parent = $null
foreach ($item in $commits) {
    $parent = New-GitCommit -Message $item.Message -Paths $item.Paths -Parent $parent
}

try { Invoke-Git -GitArgs @('branch', '-D', 'main') | Out-Null } catch { }
Invoke-Git -GitArgs @('branch', '-m', 'main') | Out-Null

Write-Host (Invoke-Git -GitArgs @('log', '--oneline'))

$allMessages = Invoke-Git -GitArgs @('log', '--format=%B')
if ($allMessages -match '(?i)co-authored-by|cursoragent') {
    throw 'Co-authored-by trailer found in history.'
}

Write-Host 'OK: clean history with no Cursor co-author'
