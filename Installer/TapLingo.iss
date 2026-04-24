; ============================================================================
;  TapLingo - Inno Setup Installer Script
; ============================================================================
; כדי להרכיב את המתקין:
;   1. התקן את Inno Setup 6 מ: https://jrsoftware.org/isinfo.php
;   2. הרץ: dotnet publish -c Release -r win-x64 (בתיקיית הפרויקט)
;   3. ערוך את #define SourceFolder למטה לפי נתיב הפרסום
;   4. הרץ Inno Setup על הקובץ הזה, או: iscc TapLingo.iss
;   5. setup.exe ייווצר בתיקיית Installer\Output
; ============================================================================

#define AppName "TapLingo"
#define AppVersion "1.0.0"
#define AppPublisher "abaye"
#define AppExeName "TapLingo.exe"

; נתיב הפלט של dotnet publish - שנה בהתאם לצורך
#define SourceFolder "..\bin\x64\Release\net8.0-windows10.0.19041.0\win-x64"

; הכתובת ממנה להוריד את Windows App Runtime אם חסר
#define WinAppRuntimeUrl "https://aka.ms/windowsappsdk/1.7/latest/windowsappruntimeinstall-x64.exe"

[Setup]
AppId={{9C3B5A8E-1F2D-4A6B-B9C8-7D4E3F2A1B5D}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppSupportURL=https://github.com/abaye123/TapLingo
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=..\Release
OutputBaseFilename=TapLingo-Setup-{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
; התקנה עבור כל המשתמשים - דורש הרשאות אדמין
PrivilegesRequired=admin
MinVersion=10.0.17763
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
LicenseFile=".\license.txt"
InfoAfterFile=".\thanks.txt"

; התקנת shortcuts גם לתפריט התחל וגם לשולחן עבודה (אופציונלי)
ChangesAssociations=yes
UninstallDisplayIcon={app}\{#AppExeName}
SetupIconFile=..\Assets\AppIcon.ico

; אייקונים שיוצגו בלוח הבקרה (Add/Remove Programs)
;WizardImageFile=compiler:WizModernImage-IS.bmp
;WizardSmallImageFile=compiler:WizModernSmallImage-IS.bmp

; שפות
ShowLanguageDialog=auto

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "hebrew"; MessagesFile: "compiler:Languages\Hebrew.isl"

[Tasks]
Name: "desktopicon"; Description: "צור קיצור דרך על שולחן העבודה"; GroupDescription: "קיצורי דרך נוספים:"; Flags: unchecked
Name: "translateshortcut"; Description: "הוסף קיצור דרך 'תרגום עם {#AppName}' (פותח חלונית להזנה ידנית)"; GroupDescription: "קיצורי דרך נוספים:"
Name: "registerprotocol"; Description: "רשום אוטומטית את הפרוטוקול TapLingo:// (לאינטגרציה עם Click to Do)"; GroupDescription: "אינטגרציה:"

[Files]
; כל הקבצים מתיקיית ה-publish - כולל ה-DLLs של Windows App SDK
Source: "{#SourceFolder}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; מתקין Windows App Runtime - ישולב כקובץ משאב
; המשתמש צריך להוריד אותו ידנית ולשים ב-Installer/Redist/
; או להשאיר את הקוד ב-[Code] שיוריד ויתקין בזמן התקנה
;Source: "Redist\windowsappruntimeinstall-x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: NeedsWinAppRuntime

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\Assets\AppIcon.ico"
Name: "{group}\תרגום עם {#AppName}"; Filename: "{app}\{#AppExeName}"; Parameters: "--translate"; IconFilename: "{app}\Assets\AppIcon.ico"; Comment: "פתח את חלונית התרגום להזנה ידנית"; Tasks: translateshortcut
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon; IconFilename: "{app}\Assets\AppIcon.ico"
Name: "{autodesktop}\תרגום עם {#AppName}"; Filename: "{app}\{#AppExeName}"; Parameters: "--translate"; IconFilename: "{app}\Assets\AppIcon.ico"; Comment: "פתח את חלונית התרגום להזנה ידנית"; Tasks: desktopicon; Check: IsTaskSelected('translateshortcut')

[Registry]
; === רישום האפליקציה ברשימת "פתח באמצעות" של Windows (HKLM\...\Applications\TapLingo.exe) ===
; זה מה שגורם ל-Click to Do ולדיאלוג "Open with" הכללי להציע את TapLingo מראש
; בלי שהמשתמש יצטרך לבחור "עוד אפליקציות" ולדפדף ל-Program Files.
Root: HKLM; Subkey: "Software\Classes\Applications\{#AppExeName}"; ValueType: string; ValueName: "FriendlyAppName"; ValueData: "{#AppName}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\Classes\Applications\{#AppExeName}"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "כלי תרגום מהיר עם אינטגרציית Click to Do"
Root: HKLM; Subkey: "Software\Classes\Applications\{#AppExeName}\DefaultIcon"; ValueType: string; ValueData: """{app}\Assets\AppIcon.ico"""
Root: HKLM; Subkey: "Software\Classes\Applications\{#AppExeName}\shell\open\command"; ValueType: string; ValueData: """{app}\{#AppExeName}"" ""%1"""
; תומך גם בפעולת "תרגום" כווריב מפורש - לזיהוי ברור יותר ע"י Click to Do
Root: HKLM; Subkey: "Software\Classes\Applications\{#AppExeName}\shell\translate"; ValueType: string; ValueName: ""; ValueData: "תרגם עם {#AppName}"
Root: HKLM; Subkey: "Software\Classes\Applications\{#AppExeName}\shell\translate\command"; ValueType: string; ValueData: """{app}\{#AppExeName}"" ""%1"""

; === רישום הפרוטוקול TapLingo:// machine-wide (מחליף את --register) ===
Root: HKLM; Subkey: "Software\Classes\TapLingo"; ValueType: string; ValueData: "URL:TapLingo Protocol"; Tasks: registerprotocol; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\Classes\TapLingo"; ValueType: string; ValueName: "URL Protocol"; ValueData: ""; Tasks: registerprotocol
Root: HKLM; Subkey: "Software\Classes\TapLingo\DefaultIcon"; ValueType: string; ValueData: """{app}\Assets\AppIcon.ico"""; Tasks: registerprotocol
Root: HKLM; Subkey: "Software\Classes\TapLingo\shell\open\command"; ValueType: string; ValueData: """{app}\{#AppExeName}"" ""%1"""; Tasks: registerprotocol

; === Capabilities + RegisteredApplications - מכניס את TapLingo להגדרות "אפליקציות ברירת מחדל" של Windows ===
Root: HKLM; Subkey: "Software\{#AppName}\Capabilities"; ValueType: string; ValueName: "ApplicationName"; ValueData: "{#AppName}"; Tasks: registerprotocol; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#AppName}\Capabilities"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "כלי תרגום מהיר עם אינטגרציית Click to Do"; Tasks: registerprotocol
Root: HKLM; Subkey: "Software\{#AppName}\Capabilities"; ValueType: string; ValueName: "ApplicationIcon"; ValueData: """{app}\Assets\AppIcon.ico"""; Tasks: registerprotocol
Root: HKLM; Subkey: "Software\{#AppName}\Capabilities\URLAssociations"; ValueType: string; ValueName: "TapLingo"; ValueData: "TapLingo"; Tasks: registerprotocol
Root: HKLM; Subkey: "Software\RegisteredApplications"; ValueType: string; ValueName: "{#AppName}"; ValueData: "Software\{#AppName}\Capabilities"; Tasks: registerprotocol; Flags: uninsdeletevalue

[Run]
; הפעל את התוכנה בסיום ההתקנה
Filename: "{app}\{#AppExeName}"; \
    Description: "{cm:LaunchProgram,{#AppName}}"; \
    Flags: nowait postinstall skipifsilent

[UninstallRun]
; ניקוי רישום פרוטוקול ברמת המשתמש (HKCU) - אם המשתמש השתמש בכפתור "רשום פרוטוקול" בהגדרות
Filename: "{app}\{#AppExeName}"; \
    Parameters: "--unregister"; \
    Flags: runhidden; \
    RunOnceId: "UnregisterProtocol"

[Code]
// בדיקה אם Windows App Runtime מותקן
// המפתח ברג'יסטרי של Microsoft נוצר אוטומטית בהתקנת ה-runtime
function NeedsWinAppRuntime: Boolean;
var
  SubKey: string;
begin
  Result := True;
  SubKey := 'Software\Microsoft\WindowsAppRuntime\Packages';
  if RegKeyExists(HKLM, SubKey) or RegKeyExists(HKCU, SubKey) then
  begin
    Result := False;
  end;
end;
