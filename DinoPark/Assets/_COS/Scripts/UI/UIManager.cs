using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent (typeof(UIDocument))]
public class UIManager : MonoBehaviour
{
    private UIDocument m_MainGameDocument;

    private UIView m_CurrentView;

    private List<UIView> m_AllViews = new List<UIView>();

    private UIView m_PlayView;
    private UIView m_ArsenalView;

    private UIView m_TabsView;

    const string k_PlayViewName = "PlayView";
    const string k_ArsenalViewName = "ArsenalView";
    const string k_TabsViewName = "TabsView";

    private void OnEnable()
    {
        m_MainGameDocument = GetComponent<UIDocument>();

        SetupViews();
        SubscribeToEvents();

        ShowModalView(m_PlayView);
    }

    private void OnDisable()
    {
        UnSubscribeFromEvents();

        foreach (UIView view in m_AllViews)
        {
            view.Dispose();
        }
    }

    private void SubscribeToEvents()
    {
        MainTabBarEvents.ArsenalViewShown += OnArsenalViewShown;
        MainTabBarEvents.PlayScreenShown += OnPlayViewShown;
    }

    private void UnSubscribeFromEvents()
    {
        MainTabBarEvents.ArsenalViewShown -= OnArsenalViewShown;
    }

    private void SetupViews()
    {
        VisualElement root = m_MainGameDocument.rootVisualElement;

        m_PlayView = new PlayView(root.Q<VisualElement>(k_PlayViewName));
        m_ArsenalView = new ArsenalView(root.Q<VisualElement>(k_ArsenalViewName));

        m_TabsView = new TabsView(root.Q<VisualElement>(k_TabsViewName));

        m_AllViews.Add(m_PlayView);
        m_AllViews.Add(m_TabsView);
        m_AllViews.Add(m_ArsenalView);

        m_PlayView.Show();
        m_TabsView.Show();
    }

    private void OnArsenalViewShown()
    {
        ShowModalView(m_ArsenalView);
    }

    private void OnPlayViewShown()
    {
        ShowModalView(m_PlayView);
    }

    private void ShowModalView(UIView newView)
    {
        if (m_CurrentView != null)
            m_CurrentView.Hide();

        m_CurrentView = newView;

        if (m_CurrentView != null)
        {
            m_CurrentView.Show();
        }
    }

}
