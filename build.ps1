param([ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version = '2.4.13')
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Path "$PSScriptRoot\output" -Force | Out-Null
$metadata = '[assembly: System.Reflection.AssemblyTitle("먼작귀 펫")]' + "`n" + '[assembly: System.Reflection.AssemblyVersion("' + $Version + '.0")]' + "`n" + '[assembly: System.Reflection.AssemblyFileVersion("' + $Version + '.0")]'
[IO.File]::WriteAllText("$PSScriptRoot\output\Version.cs", $metadata, [Text.Encoding]::UTF8)
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:winexe /optimize+ /win32icon:"$PSScriptRoot\packaging\usagi.ico" /reference:System.Web.Extensions.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /out:"$PSScriptRoot\우사기.exe" "$PSScriptRoot\Usagi.cs" "$PSScriptRoot\PetSound.cs" "$PSScriptRoot\SoundCues.cs" "$PSScriptRoot\Behavior.cs" "$PSScriptRoot\Social.cs" "$PSScriptRoot\Stumble.cs" "$PSScriptRoot\Wrapping.cs" "$PSScriptRoot\Mischief.cs" "$PSScriptRoot\UpdateCore.cs" "$PSScriptRoot\UpdateUi.cs" "$PSScriptRoot\Startup.cs" "$PSScriptRoot\FrameTiming.cs" "$PSScriptRoot\FrameSurface.cs" "$PSScriptRoot\RapTrack.cs" "$PSScriptRoot\Rap.cs" "$PSScriptRoot\About.cs" "$PSScriptRoot\Settings.cs" "$PSScriptRoot\Theme.cs" "$PSScriptRoot\ThemedNotes.cs" "$PSScriptRoot\SoundModeControl.cs" "$PSScriptRoot\Characters.cs" "$PSScriptRoot\CharacterProfiles.cs" "$PSScriptRoot\BodyGeometry.cs" "$PSScriptRoot\HeadLandmarks.cs" "$PSScriptRoot\ReleaseNotes.cs" "$PSScriptRoot\output\Version.cs"
if ($LASTEXITCODE -ne 0) { throw '빌드 실패' }

& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:winexe /optimize+ /main:UpdateLauncher /reference:System.Web.Extensions.dll /out:"$PSScriptRoot\UsagiUpdate.exe" "$PSScriptRoot\UpdateCore.cs" "$PSScriptRoot\UpdateLauncher.cs"
if ($LASTEXITCODE -ne 0) { throw 'Updater build failed' }
