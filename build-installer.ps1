param([ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version = '2.4.19')
$ErrorActionPreference = 'Stop'
& "$PSScriptRoot\build.ps1" -Version $Version
$candidates = @((Join-Path $PSScriptRoot 'packaging\inno\ISCC.exe'), 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe', 'C:\Program Files\Inno Setup 6\ISCC.exe')
$compiler = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (!$compiler) { $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue; if ($command) { $compiler = $command.Source } }
if (!$compiler) { throw 'Inno Setup 6 ISCC.exe is required.' }
& $compiler "/DAppRelease=$Version" "$PSScriptRoot\packaging\usagi.iss"
if ($LASTEXITCODE -ne 0) { throw 'Installer build failed.' }
$file = Join-Path $PSScriptRoot "output\UsagiPet-Setup-$Version.exe"
$hash = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText("$file.sha256", "$hash  $([IO.Path]::GetFileName($file))`n", [Text.Encoding]::ASCII)
Write-Output $file
