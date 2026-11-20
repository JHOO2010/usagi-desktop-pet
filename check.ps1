$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    & 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:exe /main:Check /reference:System.Web.Extensions.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /out:Check.exe Usagi.cs PetSound.cs Behavior.cs Social.cs Stumble.cs Wrapping.cs Mischief.cs UpdateCore.cs UpdateUi.cs Startup.cs FrameTiming.cs FrameSurface.cs UpdateCheck.cs Check.cs
    if ($LASTEXITCODE -ne 0) { throw '검증 프로그램 빌드 실패' }
    .\Check.exe
    if ($LASTEXITCODE -ne 0) { throw '동작 검증 실패' }
} finally { Pop-Location }
