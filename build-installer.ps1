<#
  build-installer.ps1 — NSIS로 설치 프로그램(Setup.exe)을 빌드합니다.
  먼저 build.ps1 로 HtmlSlideEditor.exe 를 만들어 두어야 합니다.
  NSIS가 없으면 포터블 버전을 자동으로 내려받습니다(설치 불필요).
  사용법: .\build-installer.ps1
#>
$ErrorActionPreference = 'Stop'
$root  = Split-Path -Parent $MyInvocation.MyCommand.Path
$build = Join-Path $root '_build'
$nsis  = Join-Path $build 'nsis'
$mk    = Join-Path $nsis 'makensis.exe'
$nsi   = Join-Path $root 'installer.nsi'

if (-not (Test-Path (Join-Path $root 'HtmlSlideEditor.exe'))) {
    throw "HtmlSlideEditor.exe 가 없습니다. 먼저 .\build.ps1 을 실행하세요."
}

New-Item -ItemType Directory -Force $build | Out-Null

# NSIS(포터블) 준비
if (-not (Test-Path $mk)) {
    Write-Host 'NSIS(포터블) 다운로드 중...'
    $zip = Join-Path $build 'nsis.zip'
    $mirrors = @(
        'https://master.dl.sourceforge.net/project/nsis/NSIS%203/3.10/nsis-3.10.zip?viasf=1',
        'https://cytranet.dl.sourceforge.net/project/nsis/NSIS%203/3.10/nsis-3.10.zip?viasf=1',
        'https://netcologne.dl.sourceforge.net/project/nsis/NSIS%203/3.10/nsis-3.10.zip?viasf=1'
    )
    $ok = $false
    foreach ($u in $mirrors) {
        try {
            & curl.exe -L --fail --silent --show-error -o $zip $u
            if ((Test-Path $zip) -and ((Get-Item $zip).Length -gt 1MB)) { $ok = $true; break }
        } catch {
        }
    }
    if (-not $ok) {
        throw "NSIS 다운로드 실패. 수동 설치 후 다시 시도하세요: https://nsis.sourceforge.io/"
    }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    if (Test-Path $nsis) { Remove-Item -Recurse -Force $nsis }
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zip, $build)
    $inner = Get-ChildItem $build -Directory -Filter 'nsis-*' | Select-Object -First 1
    if ($inner) { Rename-Item $inner.FullName $nsis }
}

Write-Host '설치 프로그램 컴파일 중...'
Push-Location $root
& $mk /V2 $nsi
$code = $LASTEXITCODE
Pop-Location
if ($code -ne 0) { throw "NSIS 컴파일 실패 (exit $code)" }

$setup = Get-ChildItem $root -Filter 'HtmlSlideEditor-Setup-*.exe' | Sort-Object LastWriteTime | Select-Object -Last 1
'{0:N2} MB  ->  {1}' -f ($setup.Length / 1MB), $setup.FullName
Write-Host '설치 프로그램 빌드 완료.'
