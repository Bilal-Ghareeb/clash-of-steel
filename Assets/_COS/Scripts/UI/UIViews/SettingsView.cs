using UnityEngine;
using UnityEngine.UIElements;

public class SettingsView : UIView
{
    private VisualElement m_settingsPanel;
    private Button m_linkAccountWithGoogleButton;
    private Button m_backButton;

    public SettingsView(VisualElement topElement, bool hideOnAwake = true) : base(topElement, hideOnAwake) { }

    public override void Show()
    {
        base.Show();
        m_settingsPanel.transform.scale = new Vector3(0.1f, 0.1f, 0.1f);
        m_settingsPanel.experimental.animation.Scale(1f, 200);

        PlayFabManager.Instance.CheckIfGoogleIsLinked(UpdateGoogleButtonState);
    }

    private void UpdateGoogleButtonState(bool isLinked)
    {
        if (isLinked)
        {
            m_linkAccountWithGoogleButton.text = "LINKED";
            m_linkAccountWithGoogleButton.SetEnabled(false);
        }
        else
        {
            m_linkAccountWithGoogleButton.text = "LINK GOOGLE";
            m_linkAccountWithGoogleButton.SetEnabled(true);
        }
    }

    private void TryToLinkAccountToGoogle(ClickEvent evt)
    {
        m_linkAccountWithGoogleButton.SetEnabled(false);
        m_linkAccountWithGoogleButton.text = "LINKING...";

        PlayFabManager.Instance.LinkGooglePlayAccount(success =>
        {
            if (success)
            {
                m_linkAccountWithGoogleButton.text = "LINKED";
                m_linkAccountWithGoogleButton.SetEnabled(false);
            }
            else
            {
                m_linkAccountWithGoogleButton.text = "LINK WITH GOOGLE";
                m_linkAccountWithGoogleButton.SetEnabled(true);
            }
        });
    }

    private void CloseSettingsPanel(ClickEvent evt)
    {
        MainTabBarEvents.PlayScreenShown?.Invoke();
    }

    public override void Dispose()
    {
        base.Dispose();
        m_linkAccountWithGoogleButton.UnregisterCallback<ClickEvent>(TryToLinkAccountToGoogle);
        m_backButton.UnregisterCallback<ClickEvent>(CloseSettingsPanel);
    }

    protected override void SetVisualElements()
    {
        m_settingsPanel = m_TopElement.Q<VisualElement>("Panel");
        m_linkAccountWithGoogleButton = m_TopElement.Q<Button>("link_google__btn");
        m_backButton = m_TopElement.Q<Button>("back-btn");
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        m_linkAccountWithGoogleButton.RegisterCallback<ClickEvent>(TryToLinkAccountToGoogle);
        m_backButton.RegisterCallback<ClickEvent>(CloseSettingsPanel);
    }
}
