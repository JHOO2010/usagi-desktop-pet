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
    gh release view "v$Version" 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) { throw 'Release version already exists.' }
    git tag -a "v$Version" -m "Release v$Version"
    if ($LASTEXITCODE -ne 0) { throw 'Tag creation failed.' }
    git push origin "v$Version"
    if ($LASTEXITCODE -ne 0) { throw 'Tag push failed.' }
    gh release create "v$Version" "output/UsagiPet-Setup-$Version.exe" "output/UsagiPet-Setup-$Version.exe.sha256" --draft --verify-tag --title "우사기 산책 펫 v$Version" --notes-file $NotesFile
    if ($LASTEXITCODE -ne 0) { throw 'Release upload failed. The pushed tag and draft remain for retry.' }
    gh release edit "v$Version" --draft=false --latest
    if ($LASTEXITCODE -ne 0) { throw 'Release publish failed. The draft remains for retry.' }
} finally { Pop-Location }
