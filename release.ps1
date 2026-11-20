param([Parameter(Mandatory=$true)][ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version, [Parameter(Mandatory=$true)][string]$NotesFile)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    if (!(Test-Path -LiteralPath $NotesFile)) { throw 'Release notes file is missing.' }
    if (git status --porcelain) { throw 'Commit and push changes before releasing.' }
    $branch = git branch --show-current
    if ($branch -ne 'main') { throw 'Release from main.' }
    git fetch origin main
    if ($LASTEXITCODE -ne 0) { throw 'Fetch failed.' }
    if ((git rev-parse HEAD) -ne (git rev-parse origin/main)) { throw 'Push main before releasing.' }
    git show-ref --verify --quiet "refs/tags/v$Version"
    if ($LASTEXITCODE -eq 0) { throw 'Version tag already exists.' }
    & .\check.ps1
    & .\build-installer.ps1 -Version $Version
    $filename = "UsagiPet-Setup-$Version.exe"
    $installerHash = (Get-FileHash -LiteralPath "output/$filename" -Algorithm SHA256).Hash.ToLowerInvariant()
    $assetBase = "https://github.com/JHOO2010/usagi-desktop-pet/releases/download/v$Version/"
    $manifest = @{ tag_name="v$Version"; draft=$false; prerelease=$false; assets=@(
        @{ name=$filename; state='uploaded'; size=(Get-Item -LiteralPath "output/$filename").Length; digest="sha256:$installerHash"; browser_download_url="$assetBase$filename" },
        @{ name="$filename.sha256"; state='uploaded'; browser_download_url="$assetBase$filename.sha256" }
    ) } | ConvertTo-Json -Depth 5
    [IO.File]::WriteAllText((Join-Path $PSScriptRoot 'output/update.json'), $manifest, (New-Object Text.UTF8Encoding($false)))
    gh release view "v$Version" 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) { throw 'Release version already exists.' }
    git tag -a "v$Version" -m "Release v$Version"
    if ($LASTEXITCODE -ne 0) { throw 'Tag creation failed.' }
    git push origin "v$Version"
    if ($LASTEXITCODE -ne 0) { throw 'Tag push failed.' }
    gh release create "v$Version" "output/UsagiPet-Setup-$Version.exe" "output/UsagiPet-Setup-$Version.exe.sha256" "output/update.json" --draft --verify-tag --title "우사기 산책 펫 v$Version" --notes-file $NotesFile
    if ($LASTEXITCODE -ne 0) { throw 'Release upload failed. The pushed tag and draft remain for retry.' }
    gh release edit "v$Version" --draft=false --latest
    if ($LASTEXITCODE -ne 0) { throw 'Release publish failed. The draft remains for retry.' }
} finally { Pop-Location }
