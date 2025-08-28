using UnityEngine.UIElements;

public class InspectView : UIView
{
    private VisualElement m_backButton;

    public InspectView(VisualElement topElement) : base(topElement) 
    {
    }

    public override void Show()
    {
        base.Show();
        InspectWeaponEvents.InspectWeaponViewShown?.Invoke();
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        m_backButton = m_TopElement.Q<VisualElement>("Back-btn");
    }

    protected override void RegisterButtonCallbacks()
    {
        m_backButton.RegisterCallback<ClickEvent>(ReturnToArsenal);
    }

    protected void UnregisterButtonCallbacks()
    {
        m_backButton.UnregisterCallback<ClickEvent>(ReturnToArsenal);
    }

    private void ReturnToArsenal(ClickEvent evt)
    {
        InspectWeaponEvents.BackToArsenalButtonPressed?.Invoke();
    }
}
