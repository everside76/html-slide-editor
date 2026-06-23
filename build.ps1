<#
  build.ps1 — editor.html + WebView2 DLL을 단일 EXE로 컴파일합니다.
  요구사항: Windows + .NET Framework 4.x (csc.exe) + WebView2 런타임(Win10/11 기본 포함)
  사용법:   PowerShell에서  .\build.ps1
#>
$ErrorActionPreference = 'Stop'
$root  = Split-Path -Parent $MyInvocation.MyCommand.Path
$build = Join-Path $root '_build'
$wv2   = Join-Path $build 'wv2'
$out   = Join-Path $root 'HtmlSlideEditor.exe'
$html  = Join-Path $root 'editor.html'
$src   = Join-Path $root 'src\App.cs'
$ico   = Join-Path $root 'icon.ico'

$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) { throw ".NET Framework C# 컴파일러(csc.exe)를 찾을 수 없습니다: $csc" }

New-Item -ItemType Directory -Force $build | Out-Null

# 1) WebView2 SDK(NuGet) 다운로드 + 추출 (이미 있으면 건너뜀)
$core   = Join-Path $wv2 'lib\net462\Microsoft.Web.WebView2.Core.dll'
$wf     = Join-Path $wv2 'lib\net462\Microsoft.Web.WebView2.WinForms.dll'
$loader = Join-Path $wv2 'runtimes\win-x64\native\WebView2Loader.dll'
if (-not (Test-Path $core)) {
    Write-Host 'WebView2 SDK 다운로드 중...'
    $nupkg = Join-Path $build 'webview2.zip'
    Invoke-WebRequest -Uri 'https://www.nuget.org/api/v2/package/Microsoft.Web.WebView2' -OutFile $nupkg -UseBasicParsing
    if (Test-Path $wv2) { Remove-Item -Recurse -Force $wv2 }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($nupkg, $wv2)
}

# 2) 컴파일 (HTML + DLL 3종을 EXE 리소스로 내장)
Write-Host '컴파일 중...'
$iconArg = @()
if (Test-Path $ico) { $iconArg = @("/win32icon:$ico") }
$cscArgs = @(
    '/target:winexe', '/platform:x64', '/optimize+', '/nologo',
    "/out:$out"
) + $iconArg + @(
    '/r:System.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll', '/r:System.Core.dll',
    "/r:$core", "/r:$wf",
    "/resource:$core,Microsoft.Web.WebView2.Core.dll",
    "/resource:$wf,Microsoft.Web.WebView2.WinForms.dll",
    "/resource:$loader,WebView2Loader.dll",
    "/resource:$html,editor.html",
    $src
)
& $csc @cscArgs
if ($LASTEXITCODE -ne 0) { throw "컴파일 실패 (exit $LASTEXITCODE)" }

'{0:N2} MB  ->  {1}' -f ((Get-Item $out).Length / 1MB), $out
Write-Host '빌드 완료.'
