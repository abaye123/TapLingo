using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace TapLingo
{
    public partial class App : Application
    {
        private Window? _mainWindow;

        public App()
        {
            InitializeComponent();
            UnhandledException += (_, e) =>
            {
                // רישום שגיאות לקובץ (WinUI 3 unpackaged לא תמיד מציג stack trace)
                TryLogError(e.Exception);
            };
        }

        /// <summary>
        /// הפעלה ראשונה של האפליקציה
        /// </summary>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // טיפול בארגומנטים של שורת פקודה
            var cmdArgs = Environment.GetCommandLineArgs();
            string? textToTranslate = null;

            if (cmdArgs.Length > 1)
            {
                var firstArg = cmdArgs[1];

                // פקודות מיוחדות
                if (firstArg.Equals("--register", StringComparison.OrdinalIgnoreCase))
                {
                    UriProtocolHandler.Register();
                    ShowMessageBox("התוכנה נרשמה בהצלחה כ-protocol handler.\nעכשיו תוכל להשתמש ב-Click to Do.");
                    Exit();
                    return;
                }

                if (firstArg.Equals("--unregister", StringComparison.OrdinalIgnoreCase))
                {
                    UriProtocolHandler.Unregister();
                    ShowMessageBox("הרישום בוטל.");
                    Exit();
                    return;
                }

                textToTranslate = ExtractText(firstArg);
            }

            var settings = SettingsManager.Load();

            if (!string.IsNullOrWhiteSpace(textToTranslate))
            {
                _mainWindow = new TranslationWindow(textToTranslate, settings);
            }
            else
            {
                _mainWindow = new SettingsWindow(settings);
            }

            _mainWindow.Activate();
        }

        /// <summary>
        /// חילוץ טקסט מארגומנט (תומך ב-URI scheme של התוכנה, טקסט ישיר, או נתיב לקובץ txt)
        /// </summary>
        internal static string ExtractText(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg)) return string.Empty;

            // URI scheme: TapLingo://translate?text=Hello
            if (arg.StartsWith("TapLingo:", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(arg);
                    var query = uri.Query;
                    if (!string.IsNullOrEmpty(query))
                    {
                        var queryStr = query.TrimStart('?');
                        foreach (var pair in queryStr.Split('&'))
                        {
                            var kv = pair.Split('=', 2);
                            if (kv.Length == 2 && kv[0].Equals("text", StringComparison.OrdinalIgnoreCase))
                            {
                                return Uri.UnescapeDataString(kv[1]);
                            }
                        }
                    }
                    var path = uri.AbsolutePath.TrimStart('/');
                    return Uri.UnescapeDataString(path);
                }
                catch
                {
                    return arg.Substring("TapLingo:".Length).TrimStart('/');
                }
            }

            // ייתכן שהועבר נתיב לקובץ טקסט
            if (File.Exists(arg) && arg.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                try { return File.ReadAllText(arg); } catch { /* fallthrough */ }
            }

            return arg;
        }

        private static void ShowMessageBox(string message)
        {
            // ב-WinUI 3 אין MessageBox מובנה; משתמשים ב-Win32 P/Invoke
            Native.MessageBoxW(IntPtr.Zero, message, "TapLingo", 0x40 /* MB_ICONINFORMATION */);
        }

        private static void TryLogError(Exception ex)
        {
            try
            {
                var logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "TapLingo");
                Directory.CreateDirectory(logDir);
                var logFile = Path.Combine(logDir, "errors.log");
                File.AppendAllText(logFile,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\n\n");
            }
            catch { /* nothing we can do */ }
        }
    }

    /// <summary>עטיפת P/Invoke קטנה להודעות מערכת</summary>
    internal static class Native
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
    }
}
