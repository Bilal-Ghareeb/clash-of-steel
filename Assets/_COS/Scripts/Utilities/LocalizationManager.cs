using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;

public static class LocalizationManager
{
    private const string PlayerPrefsKey = "selected_locale";

    public static async Task InitializeLocaleFromPrefsOrDefault()
    {
        await LocalizationSettings.InitializationOperation.Task;

        string savedLang = PlayerPrefs.GetString("selected_locale", "en");
        var locale = LocalizationSettings.AvailableLocales.Locales
            .Find(l => l.Identifier.Code == savedLang);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
        else
        {
            Debug.LogWarning($"Locale '{savedLang}' not found. Falling back to English.");
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales
                .Find(l => l.Identifier.Code == "en");
        }
    }

    public static async Task<List<string>> GetLocalizedArsenalDropDownOptions(string[] keys, string tableName)
    {
        await LocalizationSettings.InitializationOperation.Task;

        var localizedOptions = new List<string>();
        foreach (var key in keys)
        {
            var entry = await LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, key).Task;
            localizedOptions.Add(entry);
        }

        return localizedOptions;
    }

    public static async Task<string> GetLocalizedLabel(string key, string tableName)
    {
        await LocalizationSettings.InitializationOperation.Task;

        var value = await LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, key).Task;
        return value;
    }

    public static async Task SetLocale(string localeCode)
    {
        await LocalizationSettings.InitializationOperation.Task;

        var locale = LocalizationSettings.AvailableLocales.Locales
            .Find(l => l.Identifier.Code == localeCode);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString(PlayerPrefsKey, localeCode);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning($"Locale '{localeCode}' not found.");
        }
    }


    public static async Task LoadSavedLocaleOrDefault()
    {
        await LocalizationSettings.InitializationOperation.Task;

        string savedCode = PlayerPrefs.GetString(PlayerPrefsKey, Application.systemLanguage.ToString().ToLower());

        var locale = LocalizationSettings.AvailableLocales.Locales
            .Find(l => l.Identifier.Code == savedCode);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
        else
        {
            Debug.LogWarning($"Saved locale '{savedCode}' not found. Falling back to default.");
        }
    }

    public static string GetCurrentLocaleCode()
    {
        return LocalizationSettings.SelectedLocale?.Identifier.Code ?? "en";
    }
}
