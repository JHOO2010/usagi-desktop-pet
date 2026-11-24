$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    & 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:exe /main:Check /resource:packaging\github-black.png,GitHub.Black /resource:packaging\github-white.png,GitHub.White /reference:System.Web.Extensions.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /out:Check.exe Usagi.cs PetSound.cs SoundCues.cs Behavior.cs Social.cs Stumble.cs Wrapping.cs Mischief.cs UpdateCore.cs UpdateUi.cs Startup.cs FrameTiming.cs FrameSurface.cs RapTrack.cs Rap.cs About.cs Settings.cs Theme.cs ThemedNotes.cs SoundModeControl.cs Characters.cs CharacterProfiles.cs BodyGeometry.cs HeadLandmarks.cs ReleaseNotes.cs UpdateCheck.cs CharacterCheck.cs ThemeCheck.cs UiUpgradeCheck.cs AudioFocusCheck.cs Check.cs
    if ($LASTEXITCODE -ne 0) { throw '검증 프로그램 빌드 실패' }
    .\Check.exe
    if ($LASTEXITCODE -ne 0) { throw '동작 검증 실패' }
} finally { Pop-Location }
