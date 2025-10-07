using UnityEngine; 
using UnityEngine.UIElements;

public class PlayView : UIView
{
    private VisualElement m_PlayLevelButton;

    public PlayView(VisualElement topElement) : base(topElement)
    {

    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();

        m_PlayLevelButton = m_TopElement.Q("Battle_btn");
    }

    protected override void RegisterButtonCallbacks()
    {
        m_PlayLevelButton.RegisterCallback<ClickEvent>(ClickPlayButton);
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    private void ClickPlayButton(ClickEvent evt)
    {
        PlayScreenEvents.PlayBattleStageButtonPressed?.Invoke();
    }
}
