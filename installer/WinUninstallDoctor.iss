[Setup]
AppName=WinUninstallDoctor
AppVersion=1.0.0
DefaultDirName={pf}\WinUninstallDoctor
DefaultGroupName=WinUninstallDoctor
OutputDir=output
OutputBaseFilename=WinUninstallDoctorSetup
Compression=lzma
SolidCompression=yes

[Files]
Source: "..\bin\Release\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\WinUninstallDoctor"; Filename: "{app}\WinUninstallDoctor.exe"
Name: "{commondesktop}\WinUninstallDoctor"; Filename: "{app}\WinUninstallDoctor.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a Desktop icon"; GroupDescription: "Additional icons:"
