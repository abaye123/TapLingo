# TapLingo (WinUI 3 unpackaged)

גרסת **WinUI 3** של TapLingo - עם Fluent Design מקורי, Mica, פקדי Windows 11 אמיתיים, ו-`setup.exe` יחיד להפצה.

## 🎨 מה השתנה לעומת גרסת WPF?

| תכונה | WPF | WinUI 3 |
|---|---|---|
| **מראה** | עיצוב ידני | Fluent Design מקורי |
| **רקע חלון** | צבע אחיד | **Mica** (שקוף חכם) |
| **פקדים** | Custom Styles | `Expander`, `ToggleSwitch`, `InfoBar`, `ProgressRing` |
| **ScrollBar** | Custom | מקורי של Windows 11 |
| **TitleBar** | Custom מלא | מותאם חצי-מקורי + כפתורי מערכת מקוריים |
| **תמה** | קבועה (כהה) | עוקבת **אוטומטית** אחר Windows (בהיר/כהה) |

## 📋 דרישות הפצה

המשתמש זקוק ל:
- ✅ **Windows 10 1809+** או Windows 11
- ✅ **.NET 8 Desktop Runtime** (~55MB, מופץ אוטומטית על ידי Windows Update)
- ⚠️ **Windows App Runtime 1.7** (~25MB, מתקין שלנו ידאג לזה)

הכל מטופל על ידי ה-`setup.exe` שנייצר.

## 🛠️ בניית הפרויקט

### שלב 1: התקנת כלים
```powershell
# .NET 8 SDK
winget install Microsoft.DotNet.SDK.8

# Inno Setup 6 ליצירת המתקין
winget install JRSoftware.InnoSetup
```

### שלב 2: שחזור packages ובנייה

```powershell
cd TapLingoWinUI
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained false
```

זה ייצור תיקייה עם כל הקבצים תחת:
```
bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\
```

### שלב 3: הורדת Windows App Runtime

הורד את `windowsappruntimeinstall-x64.exe` לתיקיית `Installer\Redist\`:
```powershell
Invoke-WebRequest -Uri "https://aka.ms/windowsappsdk/1.7/latest/windowsappruntimeinstall-x64.exe" `
                  -OutFile "Installer\Redist\windowsappruntimeinstall-x64.exe"
```

### שלב 4: הרכבת ה-setup.exe

```powershell
cd Installer
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" TapLingo.iss
```

או פשוט פתח את `TapLingo.iss` ב-Inno Setup ולחץ על Build.

**הפלט**: `Installer\Output\TapLingo-Setup-1.0.0.exe` (~50MB)

זה ה-**setup.exe היחיד** שתפיץ למשתמשים.

## 🚀 חוויית המשתמש

1. המשתמש מוריד `TapLingo-Setup-1.0.0.exe`.
2. מריץ אותו - רואה מתקין מוכר בסגנון Windows.
3. המתקין מתקין אוטומטית את Windows App Runtime אם חסר.
4. המתקין רושם את הפרוטוקול `TapLingo://` (אם נבחרה האפשרות).
5. בסיום - אפשר לפתוח את התוכנה או להשתמש ישירות דרך Click to Do.

## 📁 מבנה הפרויקט

```
TapLingoWinUI/
├── TapLingo.csproj          # קובץ פרויקט עם WindowsPackageType=None
├── app.manifest                      # DPI awareness, Windows 11 support
├── Program.cs                        # Main() מותאם עם Bootstrap.Initialize
├── App.xaml / App.xaml.cs            # Application class
├── TranslationWindow.xaml/.cs        # חלונית צפה עם Mica
├── SettingsWindow.xaml/.cs           # חלון הגדרות עם Expander + ToggleSwitch
├── TranslationService.cs             # Google + DeepL (ללא שינוי מ-WPF)
├── SettingsManager.cs                # טעינה/שמירת JSON
├── UriProtocolHandler.cs             # רישום ה-scheme ברג'יסטרי
├── Assets/                           # אייקונים (ריק, להוסיף .ico)
└── Installer/
    ├── TapLingo.iss          # סקריפט Inno Setup
    ├── Redist/
    │   ├── README.md
    │   └── windowsappruntimeinstall-x64.exe  # (להוריד ידנית)
    └── Output/
        └── TapLingo-Setup-1.0.0.exe  # ← ה-setup.exe הסופי
```

## ⚡ השוואה: מה עובד/לא עובד ב-unpackaged

### ✅ עובד
- Fluent Design, Mica, Acrylic
- כל הפקדים של WinUI 3
- Custom TitleBar עם כפתורי מערכת מקוריים
- DPI awareness מלא
- URI protocol handler דרך הרג'יסטרי
- Dark/Light theme אוטומטי

### ❌ לא עובד (דורש packaged עם package identity)
- **App Actions API** - דורש package identity. באפליקציה unpackaged אנחנו משתמשים בחלופה: URI scheme שרשום ברג'יסטרי (`TapLingo://`). Click to Do יכול לקרוא לנו דרך "פתח באמצעות..." אבל לא נופיע כפעולה מוצעת אוטומטית.
- Toast notifications עם icon מותאם (עובד בסיסי בלי)
- File type associations דרך manifest
- Start menu tiles מותאמים
- Auto-update דרך App Installer

> 💡 **אם תרצה App Actions אמיתי בעתיד**: יש אפשרות ביניים הנקראת **"packaged with external location"** שמעניקה package identity תוך שמירה על EXE חיצוני. זה יאפשר רישום כ-Action Provider, אבל מצריך manifest ותהליך build מורכב יותר.

## 🎨 אייקון האפליקציה

הלוגו שוכן ב-`Assets\AppIcon.ico` (ו-`AppIcon.png` למקומות בתוך ה-UI).
הוא מופיע ב:
- ✅ **EXE עצמו** (File Explorer, Alt+Tab, Taskbar) - דרך `<ApplicationIcon>`
- ✅ **TitleBar של החלונות** - דרך `AppWindow.SetIcon()`
- ✅ **בתוך ה-UI** (TitleBar + Header של ההגדרות) - דרך `<Image Source="Assets/AppIcon.png"/>`
- ✅ **קיצורי דרך** (Desktop, Start menu) - דרך `IconFilename` ב-Inno
- ✅ **Uninstall entry** בלוח הבקרה - דרך `UninstallDisplayIcon`
- ✅ **Setup.exe עצמו** - דרך `SetupIconFile` ב-Inno
- ✅ **אייקון הפרוטוקול** ברג'יסטרי (לתצוגה ב-"פתח באמצעות")

## 🔧 פתרון בעיות נפוצות

### "חסר Windows App Runtime"
המתקין אמור להתקין אותו אוטומטית. אם זה לא עבד:
```powershell
# התקנה ידנית
.\windowsappruntimeinstall-x64.exe
```

### החלון נראה לבן לגמרי
Mica לא נתמך במערכת (Windows 10 ישנה). האפליקציה תחזור אוטומטית לרקע רגיל.

### האפליקציה לא עולה
בדוק את הלוג ב: `%APPDATA%\TapLingo\errors.log`

## 📚 מקורות

- [Windows App SDK documentation](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/)
- [Inno Setup documentation](https://jrsoftware.org/ishelp/)
- [WinUI 3 Gallery](https://apps.microsoft.com/detail/winui-3-gallery/9P3JFPWWDZRC) - אפליקציה להתרשם מכל הפקדים
