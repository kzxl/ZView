using Xunit;
using ZeroUI.Core.Localization;

namespace ZView.Tests
{
    public class LocalizationJsonTests
    {
        [Fact]
        public void LoadLanguageJson_ShouldParseAndRegisterCulture()
        {
            string json = @"{
                ""ZView.TestKey"": ""Giá trị kiểm thử JSON"",
                ""ZView.DynamicGreeting"": ""Xin chào ZeroUniverse""
            }";

            LocalizationManager.LoadLanguageJson("fr-FR", json);
            LocalizationManager.SetLanguage("fr-FR");

            Assert.Equal("Giá trị kiểm thử JSON", LocalizationManager.Get("ZView.TestKey"));
            Assert.Equal("Xin chào ZeroUniverse", LocalizationManager.Get("ZView.DynamicGreeting"));

            // Revert back to Vietnamese
            LocalizationManager.SetLanguage("vi-VN");
        }

        [Fact]
        public void MissingKey_ShouldFallbackToDefaultCulture()
        {
            LocalizationManager.SetLanguage("ja-JP");
            // If a key doesn't exist in ja-JP, it falls back to en-US or defaultValue
            string text = LocalizationManager.Get("NonExistentKey123", defaultValue: "FallbackText");
            Assert.Equal("FallbackText", text);

            LocalizationManager.SetLanguage("vi-VN");
        }
    }
}
