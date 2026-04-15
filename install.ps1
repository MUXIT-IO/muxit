# Muxit Installer for Windows
# Usage: irm https://raw.githubusercontent.com/muxit-io/muxit/main/install.ps1 | iex

$ErrorActionPreference = 'Stop'
$repo = 'muxit-io/muxit'
$installDir = Join-Path $env:LOCALAPPDATA 'muxit'
$asset = 'muxit-win-x64.zip'

Write-Host ''
Write-Host '  Muxit Installer' -ForegroundColor Cyan
Write-Host '  ===============' -ForegroundColor Cyan
Write-Host ''

# 1. Get latest release info from GitHub API
Write-Host '  Fetching latest release...' -NoNewline
try {
    $release = Invoke-RestMethod "https://api.github.com/repos/$repo/releases/latest" -Headers @{ 'User-Agent' = 'muxit-installer' }
    $tag = $release.tag_name
    $downloadUrl = ($release.assets | Where-Object { $_.name -eq $asset }).browser_download_url
} catch {
    Write-Host ' FAILED' -ForegroundColor Red
    Write-Host "  Could not reach GitHub API. Check your internet connection." -ForegroundColor Red
    exit 1
}

if (-not $downloadUrl) {
    Write-Host ' FAILED' -ForegroundColor Red
    Write-Host "  Asset '$asset' not found in release $tag." -ForegroundColor Red
    exit 1
}
Write-Host " $tag" -ForegroundColor Green

# 2. Check if already installed and same version
$exe = Join-Path $installDir 'muxit.exe'
if (Test-Path $exe) {
    $current = & $exe --version 2>$null
    if ($current -and $current.Contains($tag.TrimStart('v'))) {
        Write-Host "  Already up to date ($tag)." -ForegroundColor Green
        Write-Host ''
        exit 0
    }
    Write-Host "  Updating existing installation..." -ForegroundColor Yellow
} else {
    Write-Host "  Installing to $installDir"
}

# 3. Download
$tempZip = Join-Path $env:TEMP "muxit-$tag.zip"
Write-Host "  Downloading $asset..." -NoNewline
try {
    Invoke-WebRequest -Uri $downloadUrl -OutFile $tempZip -UseBasicParsing
} catch {
    Write-Host ' FAILED' -ForegroundColor Red
    Write-Host "  Download failed: $_" -ForegroundColor Red
    exit 1
}
$sizeMB = [math]::Round((Get-Item $tempZip).Length / 1MB, 1)
Write-Host " ${sizeMB} MB" -ForegroundColor Green

# 4. Extract (preserve workspace if it exists)
$tempExtract = Join-Path $env:TEMP "muxit-extract-$tag"
if (Test-Path $tempExtract) { Remove-Item $tempExtract -Recurse -Force }

Write-Host '  Extracting...' -NoNewline
Expand-Archive -Path $tempZip -DestinationPath $tempExtract -Force
Write-Host ' OK' -ForegroundColor Green

# 5. Install — merge into install dir
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

# Copy binary and non-workspace files (overwrite)
$extractedFiles = Get-ChildItem $tempExtract -File
foreach ($f in $extractedFiles) {
    Copy-Item $f.FullName (Join-Path $installDir $f.Name) -Force
}

# Merge workspace: only copy files that don't already exist (preserve user data)
$extractedWorkspace = Join-Path $tempExtract 'workspace'
if (Test-Path $extractedWorkspace) {
    $existingWorkspace = Join-Path $installDir 'workspace'
    if (-not (Test-Path $existingWorkspace)) {
        # Fresh install — copy entire workspace
        Copy-Item $extractedWorkspace $existingWorkspace -Recurse -Force
    } else {
        # Update — only overwrite driver DLLs, preserve user files
        foreach ($tier in @('community', 'premium')) {
            $srcTier = Join-Path $extractedWorkspace "drivers\$tier"
            $dstTier = Join-Path $existingWorkspace "drivers\$tier"
            if (Test-Path $srcTier) {
                if (-not (Test-Path $dstTier)) { New-Item -ItemType Directory -Path $dstTier -Force | Out-Null }
                Get-ChildItem $srcTier -Filter '*.dll' | ForEach-Object {
                    Copy-Item $_.FullName (Join-Path $dstTier $_.Name) -Force
                }
            }
        }
    }
}

# 6. Add to PATH if not already there
$userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
if ($userPath -notlike "*$installDir*") {
    Write-Host '  Adding to PATH...' -NoNewline
    [Environment]::SetEnvironmentVariable('Path', "$userPath;$installDir", 'User')
    $env:Path = "$env:Path;$installDir"
    Write-Host ' OK' -ForegroundColor Green
}

# 7. Cleanup
Remove-Item $tempZip -Force -ErrorAction SilentlyContinue
Remove-Item $tempExtract -Recurse -Force -ErrorAction SilentlyContinue

# 8. Verify
Write-Host ''
$version = & $exe --version 2>$null
if ($version) {
    Write-Host "  Installed: $version" -ForegroundColor Green
} else {
    Write-Host "  Installed to: $installDir" -ForegroundColor Green
}
Write-Host ''
Write-Host '  Run `muxit` to start, or `muxit --gui` to open in browser.' -ForegroundColor Cyan
Write-Host '  Dashboard: http://localhost:8765' -ForegroundColor Cyan
Write-Host ''
