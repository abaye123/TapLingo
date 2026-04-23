using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClickToTranslate
{
    public class TranslationResult
    {
        public string TranslatedText { get; set; } = "";
        public string DetectedSourceLanguage { get; set; } = "";
        public string Engine { get; set; } = "";
    }

    public static class TranslationService
    {
        // HttpClient יחיד לאורך חיי האפליקציה (best practice)
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public static async Task<TranslationResult> TranslateAsync(string text, AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("אין טקסט לתרגום");

            return settings.Engine switch
            {
                TranslationEngine.DeepL => await TranslateWithDeepLAsync(text, settings),
                _ => await TranslateWithGoogleAsync(text, settings),
            };
        }

        /// <summary>
        /// תרגום באמצעות Google Translate API הציבורי (ללא מפתח)
        /// משתמש ב-endpoint של gtx שמשמש את Google Translate web
        /// </summary>
        private static async Task<TranslationResult> TranslateWithGoogleAsync(string text, AppSettings settings)
        {
            var source = string.IsNullOrWhiteSpace(settings.SourceLanguage) ? "auto" : settings.SourceLanguage;
            var target = settings.TargetLanguage;
            var encoded = Uri.EscapeDataString(text);

            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx" +
                      $"&sl={source}&tl={target}&dt=t&q={encoded}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            // התשובה היא מערך JSON עם מבנה מקונן:
            // [[[translated, original, ...], ...], null, detectedLang, ...]
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var sb = new StringBuilder();
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var sentences = root[0];
                if (sentences.ValueKind == JsonValueKind.Array)
                {
                    foreach (var sentence in sentences.EnumerateArray())
                    {
                        if (sentence.ValueKind == JsonValueKind.Array && sentence.GetArrayLength() > 0)
                        {
                            var translated = sentence[0].GetString();
                            if (!string.IsNullOrEmpty(translated))
                                sb.Append(translated);
                        }
                    }
                }
            }

            string detectedLang = "";
            if (root.GetArrayLength() > 2 && root[2].ValueKind == JsonValueKind.String)
            {
                detectedLang = root[2].GetString() ?? "";
            }

            return new TranslationResult
            {
                TranslatedText = sb.ToString(),
                DetectedSourceLanguage = detectedLang,
                Engine = "Google Translate"
            };
        }

        /// <summary>
        /// תרגום באמצעות DeepL API
        /// </summary>
        private static async Task<TranslationResult> TranslateWithDeepLAsync(string text, AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.DeepLApiKey))
                throw new InvalidOperationException("נדרש מפתח API של DeepL. הגדר אותו בקובץ ההגדרות.");

            var baseUrl = settings.DeepLUseFreeTier
                ? "https://api-free.deepl.com/v2/translate"
                : "https://api.deepl.com/v2/translate";

            // DeepL דורש קודי שפה באותיות גדולות (EN, DE, HE וכו')
            var target = settings.TargetLanguage.ToUpperInvariant();

            var formData = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>
            {
                new("text", text),
                new("target_lang", target)
            };

            if (!string.IsNullOrWhiteSpace(settings.SourceLanguage) &&
                !settings.SourceLanguage.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                formData.Add(new("source_lang", settings.SourceLanguage.ToUpperInvariant()));
            }

            using var content = new FormUrlEncodedContent(formData);
            using var request = new HttpRequestMessage(HttpMethod.Post, baseUrl) { Content = content };
            request.Headers.Add("Authorization", $"DeepL-Auth-Key {settings.DeepLApiKey}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"DeepL החזיר שגיאה ({(int)response.StatusCode}): {err}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var translations = doc.RootElement.GetProperty("translations");
            if (translations.GetArrayLength() == 0)
                throw new Exception("DeepL לא החזיר תרגום");

            var first = translations[0];
            var translated = first.GetProperty("text").GetString() ?? "";
            var detected = first.TryGetProperty("detected_source_language", out var ds)
                ? ds.GetString() ?? ""
                : "";

            return new TranslationResult
            {
                TranslatedText = translated,
                DetectedSourceLanguage = detected,
                Engine = "DeepL"
            };
        }
    }
}
