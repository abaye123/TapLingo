using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.DynamicDependency;

namespace TapLingo
{
    /// <summary>
    /// נקודת כניסה מותאמת אישית. נדרשת ב-WinUI 3 unpackaged כי:
    /// 1. חייבים לאתחל את ה-Bootstrapper לפני יצירת ה-App
    /// 2. חייבים STA (Single-Threaded Apartment) עבור WinUI
    /// 3. רוצים לטפל ב-single instance
    /// </summary>
    public static class Program
    {
        // מגדיר את הגרסה של Windows App Runtime שאנחנו דורשים
        // חייב להתאים לגרסת ה-NuGet ב-csproj (1.7 = major.minor)
        private static readonly uint RequiredMajorMinor = 0x00010007; // 1.7

        [STAThread]
        public static int Main(string[] args)
        {
            // 1. וידוא שרק מופע אחד של האפליקציה רץ (חשוב לחלונית התרגום)
            using var mutex = new Mutex(true, "TapLingo_SingleInstance_Mutex_{8F3A2C7E-4B5D-4F1A-9C8E-3D2B1A5F6E7D}", out bool isNewInstance);
            if (!isNewInstance)
            {
                // כבר רץ מופע אחר - פשוט יוצאים
                // (בחלק מהתרחישים נרצה להפנות את הארגומנטים למופע הקיים, אבל
                // לחלונית תרגום עצמאית זה פחות חשוב)
                return 0;
            }

            // 2. אתחול ה-Windows App SDK Bootstrapper
            // זה מה שמחבר את האפליקציה ל-runtime של Windows App SDK המותקן במערכת
            try
            {
                Bootstrap.Initialize(RequiredMajorMinor);
            }
            catch (Exception ex)
            {
                // אם ה-runtime לא מותקן, נותנים הודעה ברורה
                MessageBoxW(IntPtr.Zero,
                    "חסר Windows App Runtime במערכת.\n\n" +
                    "אנא התקן את Microsoft Windows App Runtime 1.7 מהכתובת:\n" +
                    "https://aka.ms/windowsappsdk/1.7/latest/windowsappruntimeinstall-x64.exe\n\n" +
                    $"פרטי שגיאה:\n{ex.Message}",
                    "TapLingo - שגיאת התקנה",
                    0x10 /* MB_ICONERROR */);
                return 1;
            }

            try
            {
                // 3. הפעלת WinUI XAML בצורה הרגילה
                Application.Start((p) =>
                {
                    var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
                        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    _ = new App();
                });

                return 0;
            }
            finally
            {
                // 4. ניקוי ה-bootstrapper בסיום
                Bootstrap.Shutdown();
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
    }
}
