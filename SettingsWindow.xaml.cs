using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using WinRT.Interop;

namespace ClickToTranslate
{
    public sealed partial class SettingsWindow : Window
    {
        private readonly AppSettings _settings;
        private MicaController? _micaController;
        private SystemBackdropConfiguration? _backdropConfiguration;

        private static readonly List<LanguageItem> Languages = new()
        {
            new("he", "עברית"), new("en", "אנגלית"), new("ar", "ערבית"),
            new("ru", "רוסית"), new("es", "ספרדית"), new("fr", "צרפתית"),
            new("de", "גרמנית"), new("it", "איטלקית"), new("pt", "פורטוגזית"),
            new("nl", "הולנדית"), new("pl", "פולנית"), new("tr", "טורקית"),
            new("uk", "אוקראינית"), new("zh", "סינית"), new("ja", "יפנית"),
            new("ko", "קוריאנית"), new("hi", "הינדית"), new("th", "תאית"),
            new("vi", "ויאטנמית"), new("id", "אינדונזית")
        };

        public SettingsWindow(AppSettings settings)
        {
            InitializeComponent();
            _settings = settings;

            Title = "הגדרות - ClickToTranslate";
            RootGrid.FlowDirection = FlowDirection.RightToLeft;

            SetupTitleBar();
            TrySetMica();
            SetupWindowSize();

            // שפות
            foreach (var l in Languages)
                TargetLangCombo.Items.Add(new ComboBoxItem { Content = l.Display, Tag = l.Code });

            for (int i = 0; i < TargetLangCombo.Items.Count; i++)
            {
                if (TargetLangCombo.Items[i] is ComboBoxItem c && (string)c.Tag! == _settings.TargetLanguage)
                {
                    TargetLangCombo.SelectedIndex = i;
                    break;
                }
            }
            if (TargetLangCombo.SelectedIndex < 0) TargetLangCombo.SelectedIndex = 0;

            EngineCombo.SelectedIndex = _settings.Engine == TranslationEngine.DeepL ? 1 : 0;
            DeepLKeyBox.Password = _settings.DeepLApiKey;
            DeepLFreeTierToggle.IsOn = _settings.DeepLUseFreeTier;
            AutoCopyToggle.IsOn = _settings.AutoCopyToClipboard;

            foreach (var item in PositionCombo.Items)
            {
                if (item is ComboBoxItem c && (string)c.Tag! == _settings.WindowPosition)
                {
                    PositionCombo.SelectedItem = c;
                    break;
                }
            }
            if (PositionCombo.SelectedItem == null) PositionCombo.SelectedIndex = 0;

            SettingsPathText.Text = $"קובץ הגדרות: {SettingsManager.GetSettingsPath()}";

            UpdateRegisterStatus();

            Closed += (_, _) => { _micaController?.Dispose(); };
        }

        #region TitleBar / Mica / Sizing

        private AppWindow? GetAppWindow()
        {
            var hWnd = WindowNative.GetWindowHandle(this);
            return AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(hWnd));
        }

        private void SetupTitleBar()
        {
            var appWindow = GetAppWindow();
            if (appWindow == null) return;

            // טעינת האייקון של האפליקציה
            try
            {
                var iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    appWindow.SetIcon(iconPath);
                }
            }
            catch { /* אייקון אופציונלי */ }

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var tb = appWindow.TitleBar;
                tb.ExtendsContentIntoTitleBar = true;
                tb.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
                tb.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
                SetTitleBar(AppTitleBar);
            }
        }

        private void TrySetMica()
        {
            if (!MicaController.IsSupported()) return;
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

        private void SetupWindowSize()
        {
            var appWindow = GetAppWindow();
            if (appWindow == null) return;
            var scale = 1.0; // פשוט, אפשר להשתמש ב-GetDpiForWindow אם צריך
            appWindow.Resize(new SizeInt32(560, 720));

            // מרכוז
            var area = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            if (area != null)
            {
                appWindow.Move(new PointInt32(
                    area.WorkArea.X + (area.WorkArea.Width - appWindow.Size.Width) / 2,
                    area.WorkArea.Y + (area.WorkArea.Height - appWindow.Size.Height) / 2));
            }

            if (appWindow.Presenter is OverlappedPresenter op)
            {
                op.IsMaximizable = false;
                op.IsMinimizable = true;
            }
        }

        #endregion

        #region Logic

        private void UpdateRegisterStatus()
        {
            if (UriProtocolHandler.IsRegistered())
            {
                RegisterStatusBar.Severity = InfoBarSeverity.Success;
                RegisterStatusBar.Title = "הפרוטוקול רשום";
                RegisterStatusBar.Message = "Click to Do יכול להפעיל את התוכנה.";
            }
            else
            {
                RegisterStatusBar.Severity = InfoBarSeverity.Warning;
                RegisterStatusBar.Title = "הפרוטוקול עדיין לא רשום";
                RegisterStatusBar.Message = "לחץ על 'רשום פרוטוקול' כדי לאפשר ל-Click to Do לפתוח את התוכנה.";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.Engine = EngineCombo.SelectedIndex == 1 ? TranslationEngine.DeepL : TranslationEngine.Google;

            if (TargetLangCombo.SelectedItem is ComboBoxItem c1 && c1.Tag is string code)
                _settings.TargetLanguage = code;

            _settings.DeepLApiKey = DeepLKeyBox.Password?.Trim() ?? "";
            _settings.DeepLUseFreeTier = DeepLFreeTierToggle.IsOn;
            _settings.AutoCopyToClipboard = AutoCopyToggle.IsOn;

            if (PositionCombo.SelectedItem is ComboBoxItem c2 && c2.Tag is string pos)
                _settings.WindowPosition = pos;

            SettingsManager.Save(_settings);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try { UriProtocolHandler.Register(); UpdateRegisterStatus(); }
            catch (System.Exception ex)
            {
                RegisterStatusBar.Severity = InfoBarSeverity.Error;
                RegisterStatusBar.Title = "שגיאה ברישום";
                RegisterStatusBar.Message = ex.Message;
            }
        }

        private void UnregisterButton_Click(object sender, RoutedEventArgs e)
        {
            try { UriProtocolHandler.Unregister(); UpdateRegisterStatus(); }
            catch (System.Exception ex)
            {
                RegisterStatusBar.Severity = InfoBarSeverity.Error;
                RegisterStatusBar.Title = "שגיאה";
                RegisterStatusBar.Message = ex.Message;
            }
        }

        #endregion

        private class LanguageItem
        {
            public string Code { get; }
            public string Display { get; }
            public LanguageItem(string code, string name)
            {
                Code = code;
                Display = $"{name} ({code})";
            }
        }
    }
}
