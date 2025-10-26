using UnityEngine;

public class SettingsController : MonoBehaviour
{
    private SettingsView m_view;

    public void Setup(SettingsView view)
    {
        m_view = view;
    }

    private void OnEnable()
    {
        SettingsEvents.ScreenEnabled += HandleScreenEnabled;
        SettingsEvents.LinkGoogleClicked += HandleLinkGoogleClicked;
        SettingsEvents.BackClicked += HandleBackClicked;
        SettingsEvents.LangClicked += HandleLanguageButtonClicked;
    }

    private void OnDisable()
    {
        SettingsEvents.ScreenEnabled -= HandleScreenEnabled;
        SettingsEvents.LinkGoogleClicked -= HandleLinkGoogleClicked;
        SettingsEvents.BackClicked -= HandleBackClicked;
        SettingsEvents.LangClicked -= HandleLanguageButtonClicked;
    }

    private void HandleScreenEnabled()
    {
        PlayFabManager.Instance.AuthService.CheckIfGoogleIsLinked(UpdateGoogleButtonState);
    }

    private async void UpdateGoogleButtonState(bool isLinked)
    {
        if (isLinked)
        {
            string linkedText = await LocalizationManager.GetLocalizedLabel("ID_LinkedWIthGoogle", "COS_Strings");
            m_view.SetGoogleButtonState(linkedText, false);
        }
        else
        {
            string linkText = await LocalizationManager.GetLocalizedLabel("ID_LinkWithGoogle", "COS_Strings");
            m_view.SetGoogleButtonState(linkText, true);
        }
    }

    private async void HandleLinkGoogleClicked()
    {
        string linkingText = await LocalizationManager.GetLocalizedLabel("ID_Linking", "COS_Strings");
        m_view.SetGoogleButtonState(linkingText, false);

        PlayFabManager.Instance.AuthService.LinkGooglePlayAccount(async success =>
        {
            if (success)
            {
                string linkedText = await LocalizationManager.GetLocalizedLabel("ID_LinkedWIthGoogle", "COS_Strings");
                m_view.SetGoogleButtonState(linkedText, false);
            }
            else
            {
                string linkText = await LocalizationManager.GetLocalizedLabel("ID_LinkWithGoogle", "COS_Strings");
                m_view.SetGoogleButtonState(linkText, true);
            }
        });
    }

    private async void HandleLanguageButtonClicked()
    {
        string currentLang = LocalizationManager.GetCurrentLocaleCode();
        string newLang = currentLang == "en" ? "ar" : "en";

        await LocalizationManager.SetLocale(newLang);
    }


    private void HandleBackClicked()
    {
        MainTabBarEvents.PlayScreenShown?.Invoke();
    }
}
