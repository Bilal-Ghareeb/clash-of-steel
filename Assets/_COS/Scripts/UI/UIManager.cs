using System.Collections.Generic;
using Unity.VisualScripting;
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
    private UIView m_InspectView;
    private UIView m_TabsView;
    private UIView m_CurrenciesView;
    private UIView m_PreparingForBattleStageView;

    const string k_PlayViewName = "PlayView";
    const string k_ArsenalViewName = "ArsenalView";
    const string k_TabsViewName = "TabsView";
    const string k_InspectViewName = "InspectView";
    const string k_CurrenciesViewName = "CurrenciesView";
    const string k_PreparingForBattleStageViewName = "PreparingForBattleStageView";

    [SerializeField] private WeaponInspectPresenter m_WeaponInspectPresenter;

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

        InspectWeaponEvents.BackToArsenalButtonPressed += OnArsenalViewShown;

        ArsenalEvents.WeaponItemClicked += OnInspectViewShown;

        PlayScreenEvents.PlayBattleStageButtonPressed += OnPreparingForBattleStageShown;
        PreparingForBattleStageEvents.LeavePreparingForBattle += OnPlayViewShown;

    }

    private void UnSubscribeFromEvents()
    {
        MainTabBarEvents.PlayScreenShown -= OnPlayViewShown;
        MainTabBarEvents.ArsenalViewShown -= OnArsenalViewShown;

        InspectWeaponEvents.BackToArsenalButtonPressed -= OnArsenalViewShown;

        ArsenalEvents.WeaponItemClicked -= OnInspectViewShown;

        PlayScreenEvents.PlayBattleStageButtonPressed -= OnPreparingForBattleStageShown;
        PreparingForBattleStageEvents.LeavePreparingForBattle -= OnPlayViewShown;

    }

    private void SetupViews()
    {
        VisualElement root = m_MainGameDocument.rootVisualElement;

        m_PlayView = new PlayView(root.Q<VisualElement>(k_PlayViewName));
        m_ArsenalView = new ArsenalView(root.Q<VisualElement>(k_ArsenalViewName));
        m_InspectView = new InspectView(root.Q<VisualElement>(k_InspectViewName) , m_WeaponInspectPresenter);
        m_TabsView = new TabsView(root.Q<VisualElement>(k_TabsViewName));
        m_CurrenciesView = new CurrenciesView(root.Q<VisualElement>(k_CurrenciesViewName) , false);
        m_PreparingForBattleStageView = new PreparingForBattleStageView(root.Q<VisualElement>(k_PreparingForBattleStageViewName));

        m_AllViews.Add(m_PlayView);
        m_AllViews.Add(m_TabsView);
        m_AllViews.Add(m_ArsenalView);
        m_AllViews.Add(m_InspectView);
        m_AllViews.Add(m_CurrenciesView);
        m_AllViews.Add(m_PreparingForBattleStageView);
    }

    private void OnArsenalViewShown()
    {
        ShowModalView(m_ArsenalView);
    }

    private void OnPlayViewShown()
    {
        ShowModalView(m_PlayView);
    }

    private void OnInspectViewShown(WeaponItemComponent comp)
    {
        ShowModalView(m_InspectView);
    }

    private void OnPreparingForBattleStageShown()
    {
        ShowModalView(m_PreparingForBattleStageView);
    }

    private void ShowModalView(UIView newView)
    {
        if (m_CurrentView != null && m_CurrentView != m_InspectView && m_CurrentView != m_PreparingForBattleStageView)
        {
            m_CurrentView.Hide();
        }
        else if (m_CurrentView == m_InspectView || m_CurrentView == m_PreparingForBattleStageView)
        {
            m_CurrentView.Hide();
            m_TabsView.Show();
        }

        m_CurrentView = newView;

        if (m_CurrentView != null)
        {
            m_CurrentView.Show();
        }

        if (m_CurrentView == m_InspectView || m_CurrentView == m_PreparingForBattleStageView)
        {
            m_TabsView.Hide();
        }
    }

}
