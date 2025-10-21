using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent (typeof(UIDocument))]
public class MainGameUIManager : MonoBehaviour
{
    private UIDocument m_MainGameDocument;

    private UIView m_CurrentView;

    private List<UIView> m_AllViews = new List<UIView>();

    private UIView m_PlayView;
    private UIView m_SettingView;
    private UIView m_ArsenalView;
    private UIView m_InspectView;
    private UIView m_TabsView;
    private UIView m_CurrenciesView;
    private UIView m_PreparingForBattleStageView;
    private UIView m_ShopView;

    const string k_PlayViewName = "PlayView";
    const string k_SettingView = "SettingsView";
    const string k_ArsenalViewName = "ArsenalView";
    const string k_TabsViewName = "TabsView";
    const string k_InspectViewName = "InspectView";
    const string k_CurrenciesViewName = "CurrenciesView";
    const string k_PreparingForBattleStageViewName = "PrepareForBattleStageView";
    const string k_ShopView = "ShopView";

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
        MainTabBarEvents.ShopViewShown += OnShopViewShown;

        InspectWeaponEvents.BackToArsenalButtonPressed += OnArsenalViewShown;

        ArsenalEvents.WeaponItemClicked += OnInspectViewShown;

        PlayScreenEvents.PlayBattleStageButtonPressed += OnPreparingForBattleStageShown;
        PlayScreenEvents.SettingsButtonPressed += OnSettingsPanelShown;
        PreparingForBattleStageEvents.LeavePreparingForBattle += OnPlayViewShown;

    }

    private void UnSubscribeFromEvents()
    {
        MainTabBarEvents.PlayScreenShown -= OnPlayViewShown;
        MainTabBarEvents.ArsenalViewShown -= OnArsenalViewShown;
        MainTabBarEvents.ShopViewShown += OnShopViewShown;


        InspectWeaponEvents.BackToArsenalButtonPressed -= OnArsenalViewShown;

        ArsenalEvents.WeaponItemClicked -= OnInspectViewShown;

        PlayScreenEvents.PlayBattleStageButtonPressed -= OnPreparingForBattleStageShown;
        PlayScreenEvents.SettingsButtonPressed -= OnSettingsPanelShown;
        PreparingForBattleStageEvents.LeavePreparingForBattle -= OnPlayViewShown;
    }

    private void SetupViews()
    {
        VisualElement root = m_MainGameDocument.rootVisualElement;

        m_PlayView = new PlayView(root.Q<VisualElement>(k_PlayViewName));
        m_SettingView = new SettingsView(root.Q<VisualElement>(k_SettingView));
        m_ArsenalView = new ArsenalView(root.Q<VisualElement>(k_ArsenalViewName));
        m_InspectView = new InspectView(root.Q<VisualElement>(k_InspectViewName) , m_WeaponInspectPresenter);
        m_TabsView = new TabsView(root.Q<VisualElement>(k_TabsViewName));
        m_CurrenciesView = new CurrenciesView(root.Q<VisualElement>(k_CurrenciesViewName) , false);
        m_PreparingForBattleStageView = new PreparingForBattleStageView(root.Q<VisualElement>(k_PreparingForBattleStageViewName));
        m_ShopView = new ShopView(root.Q<VisualElement>(k_ShopView));

        m_AllViews.Add(m_PlayView);
        m_AllViews.Add(m_SettingView);
        m_AllViews.Add(m_TabsView);
        m_AllViews.Add(m_ArsenalView);
        m_AllViews.Add(m_InspectView);
        m_AllViews.Add(m_CurrenciesView);
        m_AllViews.Add(m_PreparingForBattleStageView);
        m_AllViews.Add(m_ShopView);
    }

    private void OnArsenalViewShown()
    {
        ShowModalView(m_ArsenalView);
    }

    private void OnSettingsPanelShown()
    {
        ShowModalView(m_SettingView);
    }

    private void OnPlayViewShown()
    {
        ShowModalView(m_PlayView);
    }

    private void OnShopViewShown()
    {
        ShowModalView(m_ShopView);
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
