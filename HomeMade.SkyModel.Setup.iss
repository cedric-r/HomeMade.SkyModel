; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "HomeMade.SkyModel"
#define MyAppVersion "0.0.19"
#define MyAppPublisher "Cedric Raguenaud"
#define MyAppURL "https://github.com/cedric-r/HomeMade.SkyModel"
#define MyAppExeName "HomeMadeSkyModel.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{8BB160E9-D3C4-42D3-B72C-69EC27DA1DB4}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=K:\astro\HomeMadeSkyModel\LICENSE
OutputDir="."
OutputBaseFilename=HomeMade.SkyModel.Setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\HomeMadeSkyModel.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Astrometry.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Astrometry.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Attributes.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Attributes.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Controls.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Controls.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.DeviceInterfaces.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.DeviceInterfaces.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.DriverAccess.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.DriverAccess.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Exceptions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Exceptions.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Internal.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Internal.Extensions.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.SettingsProvider.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.SettingsProvider.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Utilities.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Utilities.Video.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\ASCOM.Utilities.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\CSharpFITS_v1.1.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\HomeMadeSkyModel.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\Newtonsoft.Json.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "K:\astro\HomeMadeSkyModel\HomeMadeSkyModel\bin\Debug\Utils.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "k:\astro\HomeMadeSkyModel\HomeMadeSkyModel\application.ico"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
