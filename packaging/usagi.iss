#ifndef AppRelease
#define AppRelease "2.4.20"
#endif
[Setup]
AppId={{55757449-1D38-42D2-A5FA-2C4EAE3A3462}
AppName=먼작귀 펫
AppVersion={#AppRelease}
AppPublisher=JH
DefaultDirName={localappdata}\Programs\UsagiPet
DefaultGroupName=먼작귀 펫
PrivilegesRequired=lowest
OutputDir=..\output
OutputBaseFilename=UsagiPet-Setup-{#AppRelease}
SetupIconFile=usagi.ico
UninstallDisplayIcon={app}\우사기.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
DisableProgramGroupPage=yes
MinVersion=10.0
CloseApplications=yes
RestartApplications=no
UninstallDisplayName=먼작귀 펫

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"

[Tasks]
Name: "desktopicon"; Description: "바탕화면에 바로가기 만들기"; Flags: unchecked

[Files]
Source: "..\우사기.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\UsagiUpdate.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\usagi.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\walk.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\poses.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\stumble.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\eating.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\held.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\온글잎 꾹지체.ttf"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\sounds\*.wav"; DestDir: "{app}\sounds"; Flags: ignoreversion
Source: "..\sounds\SOURCES.txt"; DestDir: "{app}\sounds"; Flags: ignoreversion
Source: "설치후_사용법.txt"; DestDir: "{app}"; DestName: "사용법.txt"; Flags: ignoreversion

Source: "..\fonts\*.ttf"; DestDir: "{app}\fonts"; Flags: ignoreversion
Source: "..\fonts\OFL.txt"; DestDir: "{app}\fonts"; Flags: ignoreversion

Source: "..\characters\*.png"; DestDir: "{app}\characters"; Excludes: "chiikawa.png,hachiware.png,momonga.png,kurimanju.png,rakko.png,shisa.png"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\characters\face-landmarks.tsv"; DestDir: "{app}\characters"; Flags: ignoreversion
Source: "..\sounds\characters\*.wav"; DestDir: "{app}\sounds\characters"; Flags: ignoreversion
Source: "..\sounds\actions\*.wav"; DestDir: "{app}\sounds\actions"; Flags: ignoreversion
Source: "..\sounds\reactions\*.wav"; DestDir: "{app}\sounds\reactions"; Flags: ignoreversion

Source: "..\release-notes\v{#AppRelease}.md"; DestDir: "{app}"; DestName: "release-notes.md"; Flags: ignoreversion

[InstallDelete]
Type: files; Name: "{userprograms}\우사기 산책 펫\우사기 산책 펫.lnk"
Type: files; Name: "{userprograms}\우사기 산책 펫\사용법.lnk"
Type: dirifempty; Name: "{userprograms}\우사기 산책 펫"
Type: files; Name: "{autodesktop}\우사기 산책 펫.lnk"


[Icons]
Name: "{group}\먼작귀 펫"; Filename: "{app}\우사기.exe"; WorkingDir: "{app}"
Name: "{group}\사용법"; Filename: "{app}\사용법.txt"
Name: "{autodesktop}\먼작귀 펫"; Filename: "{app}\우사기.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\우사기.exe"; Description: "먼작귀 펫 실행"; Flags: nowait postinstall skipifsilent

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  Command: String;
  Expected: String;
begin
  if CurUninstallStep = usUninstall then
  begin
    Expected := '"' + ExpandConstant('{app}\우사기.exe') + '"';
    if RegQueryStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'UsagiDesktopPet', Command) then
      if CompareText(Command, Expected) = 0 then
        RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'UsagiDesktopPet');
  end;
end;
