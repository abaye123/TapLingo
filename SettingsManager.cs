using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TapLingo
{
    public enum TranslationEngine
    {
        Google,
        DeepL
    }

    public enum AppTheme
    {
        System,
        Light,
        Dark
    }

    public class AppSettings
    {
        /// <summary>מנוע התרגום הנוכחי</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TranslationEngine Engine { get; set; } = TranslationEngine.Google;

        /// <summary>ערכת נושא של הממשק (System = לפי מצב המערכת)</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AppTheme Theme { get; set; } = AppTheme.System;

        /// <summary>שפת יעד ברירת מחדל (קוד ISO, למשל "he", "en", "ar")</summary>
        public string TargetLanguage { get; set; } = "he";

        /// <summary>שפת מקור - "auto" לזיהוי אוטומטי</summary>
        public string SourceLanguage { get; set; } = "auto";

        /// <summary>מפתח API של DeepL (נדרש רק אם משתמשים ב-DeepL)</summary>
        public string DeepLApiKey { get; set; } = "";

        /// <summary>האם להשתמש ב-DeepL Free (api-free.deepl.com) או Pro (api.deepl.com)</summary>
        public bool DeepLUseFreeTier { get; set; } = true;

        /// <summary>מיקום החלונית על המסך: "cursor", "center", "top-right"</summary>
        public string WindowPosition { get; set; } = "cursor";

        /// <summary>האם להעתיק אוטומטית את התרגום ללוח</summary>
        public bool AutoCopyToClipboard { get; set; } = false;

        /// <summary>שקיפות החלונית (0.7-1.0)</summary>
        public double WindowOpacity { get; set; } = 0.98;
    }

    public static class SettingsManager
    {
        private static readonly string SettingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TapLingo");

        private static readonly string SettingsPath = Path.Combine(SettingsFolder, "settings.json");

        public static string GetSettingsPath() => SettingsPath;

        public static AppSettings Load()
        {
            try
            {
                if (!Directory.Exists(SettingsFolder))
                {
                    Directory.CreateDirectory(SettingsFolder);
                }

                if (!File.Exists(SettingsPath))
                {
                    var defaults = new AppSettings();
                    Save(defaults);
                    return defaults;
                }

                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                if (!Directory.Exists(SettingsFolder))
                {
                    Directory.CreateDirectory(SettingsFolder);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                // WinUI 3 אין MessageBox מובנה - משתמשים ב-Win32
                Native.MessageBoxW(IntPtr.Zero,
                    $"שגיאה בשמירת ההגדרות: {ex.Message}",
                    "TapLingo",
                    0x10);
            }
        }
    }
}
