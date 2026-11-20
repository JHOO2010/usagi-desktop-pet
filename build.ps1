param([ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version = '1.6.0')
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Path "$PSScriptRoot\output" -Force | Out-Null
$metadata = '[assembly: System.Reflection.AssemblyTitle("우사기 산책 펫")]' + "`n" + '[assembly: System.Reflection.AssemblyVersion("' + $Version + '.0")]' + "`n" + '[assembly: System.Reflection.AssemblyFileVersion("' + $Version + '.0")]'
[IO.File]::WriteAllText("$PSScriptRoot\output\Version.cs", $metadata, [Text.Encoding]::UTF8)
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:winexe /optimize+ /win32icon:"$PSScriptRoot\packaging\usagi.ico" /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /out:"$PSScriptRoot\우사기.exe" "$PSScriptRoot\Usagi.cs" "$PSScriptRoot\PetSound.cs" "$PSScriptRoot\Behavior.cs" "$PSScriptRoot\Social.cs" "$PSScriptRoot\Stumble.cs" "$PSScriptRoot\Wrapping.cs" "$PSScriptRoot\Mischief.cs" "$PSScriptRoot\output\Version.cs"
if ($LASTEXITCODE -ne 0) { throw '빌드 실패' }
