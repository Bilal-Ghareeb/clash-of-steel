using UnityEngine.UIElements;

public class PreparingForBattleStageView : UIView
{
    private Button m_leavePreparingForBattleButton;


    public PreparingForBattleStageView(VisualElement topElement, bool hideOnAwake = true) : base(topElement, hideOnAwake)
    {

    }

    public override void Show()
    {
        base.Show();
        PreparingForBattleStageEvents.PreparingForBattleStageShown?.Invoke();
    }


    public override void Hide()
    {
        base.Hide();
    }

    public override void Dispose()
    {
        base.Dispose();
        UnregisterButtonCallbacks();
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        m_leavePreparingForBattleButton = m_TopElement.Q<Button>("Back-btn");
    }

    protected override void RegisterButtonCallbacks()
    {
        m_leavePreparingForBattleButton.RegisterCallback<ClickEvent>(LeavePreparingForBattle);
    }

    protected void UnregisterButtonCallbacks()
    {
        m_leavePreparingForBattleButton.UnregisterCallback<ClickEvent>(LeavePreparingForBattle);
    }

    private void LeavePreparingForBattle(ClickEvent evt)
    {
        PreparingForBattleStageEvents.LeavePreparingForBattle?.Invoke();
    }
}
