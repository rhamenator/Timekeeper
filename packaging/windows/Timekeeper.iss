#ifndef MyAppName
  #define MyAppName "Timekeeper"
#endif
#ifndef MyAppVersion
  #define MyAppVersion "0.1.0"
#endif
#ifndef MyAppPublisher
  #define MyAppPublisher "Timekeeper Contributors"
#endif
#ifndef MyAppExeName
  #define MyAppExeName "Timekeeper.Web.exe"
#endif

[Setup]
AppId={{8F6552E0-9510-4A65-B3A9-64715E3AE26F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Timekeeper
DefaultGroupName=Timekeeper
DisableProgramGroupPage=yes
OutputDir=..\..\artifacts\installers
OutputBaseFilename=Timekeeper-setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\..\Timekeeper.Web\Assets\timekeeper.ico

[Files]
Source: "..\..\artifacts\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Timekeeper"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Timekeeper"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Timekeeper"; Flags: nowait postinstall skipifsilent
