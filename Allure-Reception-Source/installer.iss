[Setup]
AppId=AllureReceptionDesktop
AppName=Allure Reception
AppVersion=1.0.0
DefaultDirName={localappdata}\Programs\Allure Reception
DefaultGroupName=Allure Reception
PrivilegesRequired=lowest
OutputDir=installer-output
OutputBaseFilename=AllureReceptionSetup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Allure Reception"; Filename: "{app}\AllureReception.exe"
Name: "{autodesktop}\Allure Reception"; Filename: "{app}\AllureReception.exe"

[Run]
Filename: "{app}\AllureReception.exe"; Description: "Open Allure Reception"; Flags: nowait postinstall skipifsilent
