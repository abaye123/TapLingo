# Windows App Runtime Redistributable

לפני הרכבת ה-installer, הורד את קובץ ה-Windows App Runtime Installer של Microsoft
ושמור אותו בתיקייה הזו כ-`windowsappruntimeinstall-x64.exe`:

## הורדה
- **64-bit (מומלץ)**: https://aka.ms/windowsappsdk/1.7/latest/windowsappruntimeinstall-x64.exe
- **32-bit**: https://aka.ms/windowsappsdk/1.7/latest/windowsappruntimeinstall-x86.exe
- **ARM64**: https://aka.ms/windowsappsdk/1.7/latest/windowsappruntimeinstall-arm64.exe

## גודל
~20-25 MB

## למה צריך את זה?
Windows App SDK Runtime הוא ה-framework שמכיל את WinUI 3 ו-APIs נוספים.
הוא חייב להיות מותקן במחשב לפני שה-EXE שלנו יוכל לרוץ.

בהתקנה הראשונה של ה-installer שלנו, הוא יבדוק אם ה-Runtime כבר מותקן
ויתקין אותו בשקט אם חסר.

## הורדה אוטומטית (אופציונלי)
ניתן גם להגדיר את ה-Inno Setup להוריד את הקובץ בזמן ההתקנה דרך [Code]
עם InetDownload, אבל זה דורש חיבור אינטרנט בעת ההתקנה.
