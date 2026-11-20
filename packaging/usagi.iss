#ifndef AppRelease
#define AppRelease "1.4.0"
#endif
[Setup]
AppId={{55757449-1D38-42D2-A5FA-2C4EAE3A3462}
AppName=우사기 산책 펫
AppVersion={#AppRelease}
AppPublisher=JH
DefaultDirName={localappdata}\Programs\UsagiPet
DefaultGroupName=우사기 산책 펫
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
UninstallDisplayName=우사기 산책 펫

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"

[Tasks]
Name: "desktopicon"; Description: "바탕화면에 바로가기 만들기"; Flags: unchecked

[Files]
Source: "..\우사기.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\usagi.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\walk.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\poses.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\stumble.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\eating.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\온글잎 꾹지체.ttf"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\sounds\*.wav"; DestDir: "{app}\sounds"; Flags: ignoreversion
Source: "..\sounds\SOURCES.txt"; DestDir: "{app}\sounds"; Flags: ignoreversion
Source: "설치후_사용법.txt"; DestDir: "{app}"; DestName: "사용법.txt"; Flags: ignoreversion

[Icons]
Name: "{group}\우사기 산책 펫"; Filename: "{app}\우사기.exe"; WorkingDir: "{app}"
Name: "{group}\사용법"; Filename: "{app}\사용법.txt"
Name: "{autodesktop}\우사기 산책 펫"; Filename: "{app}\우사기.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\우사기.exe"; Description: "우사기 산책 펫 실행"; Flags: nowait postinstall skipifsilent
