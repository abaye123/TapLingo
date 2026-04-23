using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace ClickToTranslate
{
    /// <summary>
    /// רישום ה-URI scheme "clicktotranslate://" ברג'יסטרי של Windows.
    /// זה מאפשר ל-Click to Do (או לכל אפליקציה/דפדפן) להפעיל אותנו
    /// עם URI כמו: clicktotranslate://translate?text=Hello
    /// </summary>
    public static class UriProtocolHandler
    {
        private const string ProtocolName = "clicktotranslate";

        public static void Register()
        {
            var exePath = Process.GetCurrentProcess().MainModule?.FileName
                ?? throw new InvalidOperationException("לא ניתן לקבוע את נתיב ה-EXE");

            // HKEY_CURRENT_USER - לא דורש הרשאות אדמין
            using var root = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProtocolName}");
            root.SetValue("", "URL:ClickToTranslate Protocol");
            root.SetValue("URL Protocol", "");

            using (var iconKey = root.CreateSubKey("DefaultIcon"))
            {
                // מנסים להשתמש בקובץ ה-ico הייעודי; אם לא קיים, חוזרים ל-EXE
                var exeDir = System.IO.Path.GetDirectoryName(exePath);
                var iconFile = exeDir != null
                    ? System.IO.Path.Combine(exeDir, "Assets", "AppIcon.ico")
                    : null;

                if (iconFile != null && System.IO.File.Exists(iconFile))
                {
                    iconKey.SetValue("", $"\"{iconFile}\"");
                }
                else
                {
                    iconKey.SetValue("", $"\"{exePath}\",0");
                }
            }

            using (var shellKey = root.CreateSubKey(@"shell\open\command"))
            {
                // "%1" מייצג את ה-URI המלא שיועבר אלינו
                shellKey.SetValue("", $"\"{exePath}\" \"%1\"");
            }
        }

        public static void Unregister()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{ProtocolName}", false);
            }
            catch { /* אין מה להסיר */ }
        }

        public static bool IsRegistered()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{ProtocolName}");
                return key != null;
            }
            catch { return false; }
        }
    }
}
