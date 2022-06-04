using System.Globalization;
using System.Linq;
using System.Resources;

namespace SimpleJournal.Common
{
    public static class Language
    {
        public static readonly string[] SupportedLanguages = { "en", "de", "nl" };
        public static readonly string DefaultLanguage = "en";
    
        /// <summary>
        /// Returns the given langauge (if it's supported) otherwise it returns the default langauge
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetSupportedLangauge(this string value)
        {
            if (!string.IsNullOrEmpty(value) && SupportedLanguages.Any(l => l == value.ToLower()))
                return value.ToLower();

            return DefaultLanguage;
        }

        /// <summary>
        /// Gets a string resource for a given language
        /// </summary>
        /// <param name="resourceManager"></param>
        /// <param name="resourceName"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string GetString(this ResourceManager resourceManager, string resourceName, string language)
        {
            CultureInfo ci = new CultureInfo(language.GetSupportedLangauge());
            return resourceManager.GetString(resourceName, ci);
        }
    }
}