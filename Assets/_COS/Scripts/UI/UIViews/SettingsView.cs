using UnityEngine;
using UnityEngine.UIElements;

public class SettingsView : UIView
{
    private VisualElement m_settingsPanel;

    private Button m_linkAccountWithGoogleButton;
    private Button m_backButton;
    private Button m_langButton;

    public SettingsView(VisualElement topElement, bool hideOnAwake = true) : base(topElement, hideOnAwake) { }

    protected override void SetVisualElements()
    {
        m_settingsPanel = m_TopElement.Q<VisualElement>("Panel");
        m_linkAccountWithGoogleButton = m_TopElement.Q<Button>("link_google__btn");
        m_backButton = m_TopElement.Q<Button>("back-btn");
        m_langButton = m_TopElement.Q<Button>("lang-btn");
    }

    public override void Show()
    {
        base.Show();
        SettingsEvents.ScreenEnabled?.Invoke();
        m_settingsPanel.transform.scale = new Vector3(0.1f, 0.1f, 0.1f);
        m_settingsPanel.experimental.animation.Scale(1f, 200);
    }

    public void SetGoogleButtonState(string text, bool enabled)
    {
        m_linkAccountWithGoogleButton.text = text;
        m_linkAccountWithGoogleButton.SetEnabled(enabled);
    }

    private void HandleLinkButtonClicked(ClickEvent evt)
    {
       SettingsEvents.LinkGoogleClicked?.Invoke();
    }

    private void HandleBackButtonClicked(ClickEvent evt)
    {
        SettingsEvents.BackClicked?.Invoke();
    }

    private void HandleLangButtonClicked(ClickEvent evt)
    {
        SettingsEvents.LangClicked?.Invoke();
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        m_linkAccountWithGoogleButton.RegisterCallback<ClickEvent>(HandleLinkButtonClicked);
        m_backButton.RegisterCallback<ClickEvent>(HandleBackButtonClicked);
        m_langButton.RegisterCallback<ClickEvent>(HandleLangButtonClicked);
    }

    public override void Dispose()
    {
        base.Dispose();
        m_linkAccountWithGoogleButton.UnregisterCallback<ClickEvent>(HandleLinkButtonClicked);
        m_backButton.UnregisterCallback<ClickEvent>(HandleBackButtonClicked);
        m_langButton.UnregisterCallback<ClickEvent>(HandleLangButtonClicked);
    }
}
