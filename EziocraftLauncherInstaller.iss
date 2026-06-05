[Setup]
AppName=Eziocraft Launcher
AppVersion=1.0
DefaultDirName={localappdata}\Eziocraft Launcher
DefaultGroupName=Eziocraft Launcher
DisableProgramGroupPage=no
OutputDir=C:\Users\colpi
OutputBaseFilename=EziocraftLauncherSetup
Compression=lzma
SolidCompression=yes
SetupIconFile=C:\Users\colpi\EziocraftLauncher.ico
UninstallDisplayIcon={app}\EziocraftLauncher.exe
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=lowest

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Files]
Source: "C:\Users\colpi\EziocraftLauncher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\colpi\EziocraftLauncher.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\colpi\Downloads\minecraftia\Minecraftia-Regular.ttf"; DestDir: "{app}"; Flags: ignoreversion

[Tasks]
Name: "desktopicon"; Description: "Créer une icône sur le Bureau"; GroupDescription: "Options supplémentaires:"; Flags: unchecked

[Icons]
Name: "{group}\Eziocraft Launcher"; Filename: "{app}\EziocraftLauncher.exe"; IconFilename: "{app}\EziocraftLauncher.ico"
Name: "{commondesktop}\Eziocraft Launcher"; Filename: "{app}\EziocraftLauncher.exe"; IconFilename: "{app}\EziocraftLauncher.ico"; Tasks: desktopicon
Name: "{group}\Désinstaller Eziocraft Launcher"; Filename: "{uninstallexe}"

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\Eziocraft"
Type: filesandordirs; Name: "{localappdata}\Eziocraft Launcher"
