using UnityEngine;
using UnityEngine.UIElements;

public class TabsView : UIView
{
    const string k_ButtonInactiveClass = "unselected-tab";
    const string k_ButtonActiveClass = "selected-tab";

    const int k_MoveTime = 150;
    const float k_Spacing = 100f;

    private Button m_PlayViewMenuButton;
    private Button m_ArsenalViewMenuButton;
    private Button m_ShopViewMenuButton;

    private Button m_ActiveButton;
    private bool m_InterruptAnimation;

    VisualElement m_MenuMarker;

    public TabsView(VisualElement topElement) : base(topElement)
    {
        
    }

    public override void Dispose()
    {
        base.Dispose();

        UnregisterButtonCallbacks();
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();

        m_PlayViewMenuButton = m_TopElement.Q<Button>("Home-btn");
        m_ArsenalViewMenuButton = m_TopElement.Q<Button>("Arsenal-btn");
        m_ShopViewMenuButton = m_TopElement.Q<Button>("Shop-btn");

        m_MenuMarker = m_TopElement.Q("Selected-btn-Marker");
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();

        m_PlayViewMenuButton.RegisterCallback<ClickEvent>(ClickPlayViewMenuButton);
        m_ArsenalViewMenuButton.RegisterCallback<ClickEvent>(ClickArsenalViewMenuButton);
        m_ShopViewMenuButton.RegisterCallback<ClickEvent>(ClickShopViewMenuButton);

        m_MenuMarker.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
    }

    protected void UnregisterButtonCallbacks()
    {
        m_PlayViewMenuButton.UnregisterCallback<ClickEvent>(ClickPlayViewMenuButton);
        m_ArsenalViewMenuButton.UnregisterCallback<ClickEvent>(ClickArsenalViewMenuButton);
        m_ShopViewMenuButton.UnregisterCallback<ClickEvent>(ClickShopViewMenuButton);

        m_MenuMarker.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
    }

    private void ClickPlayViewMenuButton(ClickEvent evt)
    {
        ActivateButton(m_PlayViewMenuButton);
        MainTabBarEvents.PlayScreenShown?.Invoke();
        MoveMarkerToClick(evt);
    }

    private void ClickArsenalViewMenuButton(ClickEvent evt)
    {
        ActivateButton(m_ArsenalViewMenuButton);
        MainTabBarEvents.ArsenalViewShown?.Invoke();
        MoveMarkerToClick(evt);
    }

    private void ClickShopViewMenuButton(ClickEvent evt)
    {
        ActivateButton(m_ShopViewMenuButton);

        MoveMarkerToClick(evt);
    }

    private void OnGeometryChangedEvent(GeometryChangedEvent evt)
    {
        if (m_ActiveButton == null)
            m_ActiveButton = m_PlayViewMenuButton;

        ActivateButton(m_ActiveButton);
        MoveMarkerToElement(m_ActiveButton);
    }

    private void ActivateButton(Button menuButton)
    {
        m_ActiveButton = menuButton;

        HighlightElement(menuButton, k_ButtonInactiveClass, k_ButtonActiveClass, m_TopElement);
    }

    private void HighlightElement(VisualElement targetElement, string inactiveClass, string activeClass, VisualElement root)
    {
        if (targetElement == null)
            return;

        VisualElement currentSelection = root.Query<VisualElement>(className: activeClass);

        if (currentSelection == targetElement)
        {
            return;
        }

        currentSelection.RemoveFromClassList(activeClass);
        currentSelection.AddToClassList(inactiveClass);

        targetElement.RemoveFromClassList(inactiveClass);
        targetElement.AddToClassList(activeClass);
    }

    private void MoveMarkerToClick(ClickEvent evt)
    {
        if (evt.propagationPhase == PropagationPhase.BubbleUp)
        {
            MoveMarkerToElement(evt.target as VisualElement);
        }
    }

    private void MoveMarkerToElement(VisualElement targetElement)
    {

        Vector2 targetInWorldSpace = targetElement.parent.LocalToWorld(targetElement.layout.position);

        Vector3 targetInRootSpace = m_MenuMarker.parent.WorldToLocal(targetInWorldSpace);

        Vector3 offset = new Vector3(0f, 0f, 0f);

        Vector3 newPosition = targetInRootSpace - offset;

        int duration = m_InterruptAnimation ? 0 : CalculateDuration(newPosition);

        m_MenuMarker.experimental.animation.Position(targetInRootSpace, duration);

    }

    private int CalculateDuration(Vector3 newPosition)
    {
        Vector3 delta = m_MenuMarker.resolvedStyle.translate - newPosition;

        float distanceInPixels = Mathf.Abs(delta.y / k_Spacing);

        int duration = Mathf.Clamp((int)distanceInPixels * k_MoveTime, k_MoveTime, k_MoveTime * 4);
        return duration;
    }
}
