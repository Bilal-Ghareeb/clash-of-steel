using UnityEngine.UIElements;

public class PlayView : UIView
{
    private VisualElement m_playLevelButton;
    private VisualElement m_settingsButton;

    public PlayView(VisualElement topElement) : base(topElement)
    {

    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();

        m_playLevelButton = m_TopElement.Q("Battle_btn");
        m_settingsButton = m_TopElement.Q("Settings_btn");
    }

    protected override void RegisterButtonCallbacks()
    {
        m_playLevelButton.RegisterCallback<ClickEvent>(ClickPlayButton);
        m_settingsButton.RegisterCallback<ClickEvent>(ShowSettingsPanel);
    }

    public override void Dispose()
    {
        base.Dispose();
        m_settingsButton.RegisterCallback<ClickEvent>(ShowSettingsPanel);
    }

    private void ClickPlayButton(ClickEvent evt)
    {
        PlayScreenEvents.PlayBattleStageButtonPressed?.Invoke();
    }

    private void ShowSettingsPanel(ClickEvent evt)
    {
        PlayScreenEvents.SettingsButtonPressed?.Invoke();
    }
}
