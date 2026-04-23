using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using WinRT.Interop;

namespace ClickToTranslate
{
    public sealed partial class TranslationWindow : Window
    {
        private readonly AppSettings _settings;
        private string _sourceText;
        private bool _comboInitializing = true;
        private MicaController? _micaController;
        private DesktopAcrylicController? _acrylicController;
        private SystemBackdropConfiguration? _backdropConfiguration;

        private static readonly List<LanguageOption> SupportedLanguages = new()
        {
            new("he", "עברית"), new("en", "אנגלית"), new("ar", "ערבית"),
            new("ru", "רוסית"), new("es", "ספרדית"), new("fr", "צרפתית"),
            new("de", "גרמנית"), new("it", "איטלקית"), new("pt", "פורטוגזית"),
            new("nl", "הולנדית"), new("pl", "פולנית"), new("tr", "טורקית"),
            new("uk", "אוקראינית"), new("zh", "סינית"), new("ja", "יפנית"),
            new("ko", "קוריאנית"), new("hi", "הינדית"), new("th", "תאית"),
            new("vi", "ויאטנמית"), new("id", "אינדונזית")
        };

        public TranslationWindow(string text, AppSettings settings)
        {
            InitializeComponent();
            _settings = settings;
            _sourceText = text;

            // 1. הגדרות החלון הבסיסיות
            Title = "תרגום מהיר - ClickToTranslate";
            SourceTextBox.Text = text;
            EngineLabel.Text = _settings.Engine == TranslationEngine.DeepL ? "DeepL" : "Google";

            // 2. RTL ברמת הרכיב השורש (WinUI מצריך הגדרה נפרדת)
            RootGrid.FlowDirection = FlowDirection.RightToLeft;

            // 3. TitleBar מותאם אישית (נראה חלק עם Mica)
            SetupCustomTitleBar();

            // 4. הפעלת Mica/Acrylic (Fluent background)
            TrySetSystemBackdrop();

            // 5. מילוי קומבו השפות
            PopulateLanguageCombo();

            // 6. גודל ומיקום החלון
            SetupWindowAppearance();

            // 7. topmost (חלונית צפה)
            SetTopmost(true);

            // 8. תרגום ראשוני ברגע שהחלון נטען
            Activated += async (_, _) =>
            {
                if (LoadingPanel.Visibility == Visibility.Visible && string.IsNullOrEmpty(TranslatedTextBox.Text))
                {
                    await TranslateAsync();
                }
            };

            Closed += (_, _) =>
            {
                _micaController?.Dispose();
                _acrylicController?.Dispose();
                _backdropConfiguration = null;
                // יציאה מהאפליקציה כולה
                Application.Current.Exit();
            };
        }

        #region Custom TitleBar

        private void SetupCustomTitleBar()
        {
            var appWindow = GetAppWindow();
            if (appWindow == null) return;

            // טעינת האייקון של האפליקציה עבור TitleBar + Taskbar + Alt+Tab
            try
            {
                var iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    appWindow.SetIcon(iconPath);
                }
            }
            catch { /* אייקון אופציונלי - לא קריטי */ }

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = appWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
                SetTitleBar(AppTitleBar); // הפיכת ה-Grid שלנו לאזור גרירה
            }
            else
            {
                // Fallback למערכות ישנות
                ExtendsContentIntoTitleBar = true;
                SetTitleBar(AppTitleBar);
            }
        }

        #endregion

        #region Mica / Acrylic Backdrop

        private void TrySetSystemBackdrop()
        {
            if (MicaController.IsSupported())
            {
                _backdropConfiguration = new SystemBackdropConfiguration
                {
                    Theme = SystemBackdropTheme.Default,
                    IsInputActive = true
                };
                _micaController = new MicaController();
                _micaController.AddSystemBackdropTarget(
                    this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                _micaController.SetSystemBackdropConfiguration(_backdropConfiguration);
            }
            else if (DesktopAcrylicController.IsSupported())
            {
                _backdropConfiguration = new SystemBackdropConfiguration
                {
                    Theme = SystemBackdropTheme.Default,
                    IsInputActive = true
                };
                _acrylicController = new DesktopAcrylicController();
                _acrylicController.AddSystemBackdropTarget(
                    this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                _acrylicController.SetSystemBackdropConfiguration(_backdropConfiguration);
            }
            // אם אף אחד לא נתמך - פשוט משתמשים ברקע default של המערכת
        }

        #endregion

        #region Window Sizing & Positioning

        private AppWindow? GetAppWindow()
        {
            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(windowId);
        }

        private void SetupWindowAppearance()
        {
            var appWindow = GetAppWindow();
            if (appWindow == null) return;

            // גודל חלון - ב-WinUI 3 הגודל הוא בפיקסלים פיזיים
            var dpi = GetDpiForWindow(WindowNative.GetWindowHandle(this));
            var scale = dpi / 96.0;
            appWindow.Resize(new SizeInt32((int)(480 * scale), (int)(420 * scale)));

            // מיקום לפי הגדרות
            switch (_settings.WindowPosition?.ToLowerInvariant())
            {
                case "center":
                    CenterOnScreen(appWindow);
                    break;
                case "top-right":
                    PositionTopRight(appWindow);
                    break;
                case "cursor":
                default:
                    PositionNearCursor(appWindow);
                    break;
            }

            // הסרת החצים של resize (WinUI 3 מאפשר זאת דרך presenter)
            if (appWindow.Presenter is OverlappedPresenter op)
            {
                op.IsResizable = true;
                op.IsMaximizable = false;
                op.IsMinimizable = false;
            }
        }

        private void CenterOnScreen(AppWindow appWindow)
        {
            var area = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            if (area == null) return;
            var center = new PointInt32(
                area.WorkArea.X + (area.WorkArea.Width - appWindow.Size.Width) / 2,
                area.WorkArea.Y + (area.WorkArea.Height - appWindow.Size.Height) / 2);
            appWindow.Move(center);
        }

        private void PositionTopRight(AppWindow appWindow)
        {
            var area = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            if (area == null) return;
            appWindow.Move(new PointInt32(
                area.WorkArea.X + area.WorkArea.Width - appWindow.Size.Width - 20,
                area.WorkArea.Y + 20));
        }

        private void PositionNearCursor(AppWindow appWindow)
        {
            if (!GetCursorPos(out var pt)) { CenterOnScreen(appWindow); return; }

            var area = DisplayArea.GetFromPoint(new PointInt32(pt.X, pt.Y), DisplayAreaFallback.Primary);
            if (area == null) { CenterOnScreen(appWindow); return; }

            int x = pt.X + 15;
            int y = pt.Y + 15;
            if (x + appWindow.Size.Width > area.WorkArea.X + area.WorkArea.Width)
                x = area.WorkArea.X + area.WorkArea.Width - appWindow.Size.Width - 10;
            if (y + appWindow.Size.Height > area.WorkArea.Y + area.WorkArea.Height)
                y = area.WorkArea.Y + area.WorkArea.Height - appWindow.Size.Height - 10;

            appWindow.Move(new PointInt32(x, y));
        }

        private void SetTopmost(bool topmost)
        {
            var appWindow = GetAppWindow();
            if (appWindow?.Presenter is OverlappedPresenter op)
            {
                op.IsAlwaysOnTop = topmost;
            }
        }

        #endregion

        #region Language Combo + Translation

        private void PopulateLanguageCombo()
        {
            foreach (var lang in SupportedLanguages)
            {
                TargetLangCombo.Items.Add(new ComboBoxItem { Content = lang.Display, Tag = lang.Code });
            }

            // סימון הברירת מחדל
            for (int i = 0; i < TargetLangCombo.Items.Count; i++)
            {
                if (TargetLangCombo.Items[i] is ComboBoxItem cbi && (string)cbi.Tag! == _settings.TargetLanguage)
                {
                    TargetLangCombo.SelectedIndex = i;
                    break;
                }
            }
            if (TargetLangCombo.SelectedIndex < 0) TargetLangCombo.SelectedIndex = 0;
            _comboInitializing = false;
        }

        private async void TargetLangCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_comboInitializing) return;
            if (TargetLangCombo.SelectedItem is ComboBoxItem cbi && cbi.Tag is string code)
            {
                _settings.TargetLanguage = code;
                await TranslateAsync();
            }
        }

        private async Task TranslateAsync()
        {
            try
            {
                LoadingPanel.Visibility = Visibility.Visible;
                TranslatedTextBox.Text = "";
                DetectedLangLabel.Text = "";

                _sourceText = SourceTextBox.Text;
                if (string.IsNullOrWhiteSpace(_sourceText))
                {
                    TranslatedTextBox.Text = "(אין טקסט לתרגום)";
                    return;
                }

                var result = await TranslationService.TranslateAsync(_sourceText, _settings);
                TranslatedTextBox.Text = result.TranslatedText;

                if (!string.IsNullOrEmpty(result.DetectedSourceLanguage))
                    DetectedLangLabel.Text = $"זוהה: {result.DetectedSourceLanguage}";

                if (_settings.AutoCopyToClipboard && !string.IsNullOrEmpty(result.TranslatedText))
                    CopyToClipboard(result.TranslatedText);
            }
            catch (Exception ex)
            {
                TranslatedTextBox.Text = $"⚠️ שגיאה בתרגום:\n{ex.Message}";
            }
            finally
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Button Handlers

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TranslatedTextBox.Text))
            {
                CopyToClipboard(TranslatedTextBox.Text);
                ShowCopiedFeedback();
            }
        }

        private async void RetranslateButton_Click(object sender, RoutedEventArgs e)
        {
            await TranslateAsync();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new SettingsWindow(_settings);
            win.Activate();
        }

        private static void CopyToClipboard(string text)
        {
            try
            {
                var dp = new DataPackage();
                dp.SetText(text);
                Clipboard.SetContent(dp);
            }
            catch { /* clipboard may be busy */ }
        }

        private async void ShowCopiedFeedback()
        {
            var original = TitleText.Text;
            TitleText.Text = "✓ הועתק!";
            await Task.Delay(1200);
            TitleText.Text = original;
        }

        #endregion

        #region Win32 interop

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT p);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hWnd);

        #endregion

        private class LanguageOption
        {
            public string Code { get; }
            public string Display { get; }
            public LanguageOption(string code, string name)
            {
                Code = code;
                Display = $"{name} ({code})";
            }
        }
    }
}
