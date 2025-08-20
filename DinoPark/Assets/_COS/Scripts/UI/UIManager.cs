using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent (typeof(UIDocument))]
public class UIManager : MonoBehaviour
{
    private UIDocument m_MainGameDocument;

    private UIView m_CurrentView;

    // List of all UIViews
    private List<UIView> m_AllViews = new List<UIView>();

    // Modal screens
    private UIView m_PlayView;  // Landing screen

    // Toolbars
    private UIView m_TabsView;  // Navigation bar for menu screens

    const string k_PlayViewName = "PlayView";
    const string k_TabsViewName = "TabsView";

    private void OnEnable()
    {
        m_MainGameDocument = GetComponent<UIDocument>();

        SetupViews();

        // Start with the home screen
        ShowModalView(m_PlayView);

    }

    private void OnDisable()
    {
        foreach (UIView view in m_AllViews)
        {
            view.Dispose();
        }
    }


    private void SetupViews()
    {
        VisualElement root = m_MainGameDocument.rootVisualElement;

        // Create full-screen modal views: HomeView, CharView, InfoView, ShopView, MailView
        m_PlayView = new PlayView(root.Q<VisualElement>(k_PlayViewName)); // Landing modal screen


        // Toolbars 

        m_TabsView = new TabsView(root.Q<VisualElement>(k_TabsViewName)); // Screen selection toolbar

        // Track modal UI Views in a List for disposal 
        m_AllViews.Add(m_PlayView);
        m_AllViews.Add(m_TabsView);

        // UI Views enabled by default
        m_PlayView.Show();
        m_TabsView.Show();
    }

    // Toggle modal screens on/off
    void ShowModalView(UIView newView)
    {
        if (m_CurrentView != null)
            m_CurrentView.Hide();

        m_CurrentView = newView;

        // Show the screen and notify any listeners that the main menu has updated

        if (m_CurrentView != null)
        {
            m_CurrentView.Show();
        }
    }

}
