# TapLingo

כלי תרגום מהיר למסך עבור Windows 11, עם אינטגרציה מלאה ל-**Click to Do**.

סמן טקסט בכל מקום במערכת, פתח את Click to Do ← תרגם עם TapLingo ← חלונית צפה מציגה את התרגום תוך שנייה. אפשר גם לפתוח את חלונית התרגום ישירות ולהזין טקסט ידנית.

## ✨ תכונות עיקריות

- **אינטגרציה עם Click to Do** — מוצג ישירות בהצעות הפעולה של Windows 11 בלי לדפדף לתיקיית Program Files
- **שני מנועי תרגום**:
  - **Google Translate** — חינמי, ללא מפתח, ~100 שפות
  - **DeepL** — תרגום איכותי במיוחד (דורש API key חינמי)
- **~20 שפות יעד** כולל עברית, ערבית, אנגלית, רוסית, ספרדית, צרפתית, גרמנית, סינית, יפנית ועוד
- **זיהוי שפה אוטומטי** של טקסט המקור
- **חלונית צפה (Always-on-top)** — נשארת מעל שאר החלונות, עם רקע Mica שקוף של Windows 11
- **הזנה ידנית** — קיצור דרך "תרגום עם TapLingo" פותח חלונית ריקה להקלדת טקסט
- **מצב כהה/בהיר/לפי המערכת** — ברירת מחדל עוקבת אחר הגדרות Windows
- **מיקום החלונית ניתן להתאמה** — ליד הסמן, במרכז המסך, או באחת מארבע הפינות
- **העתקה אוטומטית ללוח** (אופציונלי) — התרגום מוכן להדבקה ברגע שהוא מופיע
- **תמיכה ב-RTL** — ממשק בעברית עם כפתורי חלון בצד שמאל

## 📥 התקנה

1. הורד את `TapLingo-Setup-1.0.0.exe` מ-[דף ה-Releases](https://github.com/abaye123/TapLingo/releases)
2. הרץ את המתקין (דורש הרשאות אדמין להתקנה עבור כל המשתמשים במחשב)
3. בזמן ההתקנה אפשר לבחור:
   - קיצור דרך לתרגום עם TapLingo (פותח חלונית להזנה ידנית)
   - קיצור דרך על שולחן העבודה
   - רישום אוטומטי של הפרוטוקול `TapLingo://` ושל האפליקציה ב-Click to Do
4. אם **Windows App Runtime 1.7** חסר, המתקין יציע לפתוח את דף ההורדה של Microsoft

### דרישות מערכת

- Windows 10 build 17763 (1809) ומעלה, או Windows 11 (מומלץ — Click to Do זמין רק ב-Windows 11)
- .NET 8 Desktop Runtime
- Windows App Runtime 1.7

## 🚀 שימוש

### דרך Click to Do (Windows 11)
סמן טקסט בכל אפליקציה ← לחץ Win+Click (או פתח את Click to Do) ← בחר "תרגם עם TapLingo" ← החלונית הצפה נפתחת עם התרגום.

### דרך קיצור דרך ידני
לחץ על "תרגום עם TapLingo" בתפריט התחל ← חלונית ריקה נפתחת ← הקלד או הדבק טקסט ← לחץ "תרגם שוב".

### דרך שורת פקודה
```
TapLingo.exe "טקסט לתרגום"
TapLingo.exe --translate                 # חלונית ריקה
TapLingo.exe "TapLingo://translate?text=Hello"
```

### הגדרות
פתח את חלון ההגדרות (קיצור "TapLingo" הראשי) כדי לבחור:
- מנוע תרגום (Google / DeepL)
- שפת יעד ברירת מחדל
- מפתח API של DeepL
- מצב כהה/בהיר/לפי המערכת
- מיקום החלונית
- העתקה אוטומטית ללוח

## 🛠️ בנייה מהמקור

### כלים נדרשים
```powershell
winget install Microsoft.DotNet.SDK.8
winget install JRSoftware.InnoSetup
```

### בנייה
```powershell
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained false
```

התוצר: `bin\x64\Release\net8.0-windows10.0.19041.0\win-x64\publish\`

### יצירת setup.exe
```powershell
cd Installer
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" TapLingo.iss
```

הפלט: `Release\TapLingo-Setup-1.0.0.exe`

## 📁 מבנה הפרויקט

```
TapLingo/
├── TapLingo.csproj              # WinUI 3 unpackaged
├── app.manifest                 # DPI awareness, Windows 11 support
├── Program.cs                   # Main() עם Bootstrap.Initialize של WinAppSDK
├── App.xaml(.cs)                # Application class + CLI args parsing
├── TranslationWindow.xaml(.cs)  # החלונית הצפה
├── SettingsWindow.xaml(.cs)     # חלון ההגדרות
├── TranslationService.cs        # Google + DeepL
├── SettingsManager.cs           # קריאה/כתיבה של settings.json
├── UriProtocolHandler.cs        # רישום הפרוטוקול TapLingo:// (HKCU)
├── ThemeHelper.cs               # מצב כהה/בהיר + כפתורי חלון ב-RTL
├── Assets/                      # AppIcon.ico, AppIcon.png
└── Installer/
    └── TapLingo.iss             # סקריפט Inno Setup
```

## 💾 מיקומי קבצים

- **הגדרות**: `%APPDATA%\TapLingo\settings.json`
- **יומן שגיאות**: `%APPDATA%\TapLingo\errors.log`
- **התוכנה**: `C:\Program Files\TapLingo\`

## 🔧 פתרון בעיות

**"Windows App Runtime לא מותקן"** — הורד והרץ את [המתקין של Microsoft](https://aka.ms/windowsappsdk/1.7/latest/windowsappruntimeinstall-x64.exe), ואז הרץ שוב את TapLingo.

**החלון נראה לבן לגמרי** — Mica לא נתמך במערכת הזו (Windows 10 ישן). האפליקציה תחזור אוטומטית לרקע רגיל.

**התוכנה לא עולה** — בדוק את `%APPDATA%\TapLingo\errors.log`.

**Click to Do לא מציע את TapLingo** — ודא שבזמן ההתקנה סומנה האפשרות "רשום אוטומטית את הפרוטוקול TapLingo://". אם לא, אפשר לרשום ידנית דרך כפתור "רשום פרוטוקול" בחלון ההגדרות.

## 📚 קישורים

- [Windows App SDK documentation](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/)
- [DeepL API (חינמי)](https://www.deepl.com/pro-api)
- [Inno Setup](https://jrsoftware.org/isinfo.php)
