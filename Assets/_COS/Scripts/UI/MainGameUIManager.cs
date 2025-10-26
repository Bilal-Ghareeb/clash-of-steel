using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent (typeof(UIDocument))]
public class MainGameUIManager : MonoBehaviour
{

    [Header("UI Controllers")]
    [SerializeField] private InspectController m_inspectorController;
    [SerializeField] private ShopController m_shopController;
    [SerializeField] private PreparingForBattleStageController m_preparingForBattleStageController;
    [SerializeField] private SettingsController m_settingsController;

    private UIDocument m_MainGameDocument;

    private UIView m_CurrentView;

    private List<UIView> m_AllViews = new List<UIView>();

    private UIView m_PlayView;
    private SettingsView m_SettingView;
    private UIView m_ArsenalView;
    private InspectView m_InspectView;
    private UIView m_TabsView;
    private UIView m_CurrenciesView;
    private PreparingForBattleStageView m_PreparingForBattleStageView;
    private ShopView m_ShopView;
    private ProcessingView m_processingView;

    const string k_PlayViewName = "PlayView";
    const string k_SettingView = "SettingsView";
    const string k_ArsenalViewName = "ArsenalView";
    const string k_TabsViewName = "TabsView";
    const string k_InspectViewName = "InspectView";
    const string k_CurrenciesViewName = "CurrenciesView";
    const string k_PreparingForBattleStageViewName = "PrepareForBattleStageView";
    const string k_ShopView = "ShopView";
    const string k_ProcessingView = "ProcessingView";

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

        InspectWeaponEvents.BackButtonClicked += OnArsenalViewShown;

        ArsenalEvents.WeaponItemClicked += OnInspectViewShown;

        PlayerEvents.PlayBattleStageButtonPressed += OnPreparingForBattleStageShown;
        PlayerEvents.SettingsButtonPressed += OnSettingsPanelShown;
        PreparingForBattleStageEvents.LeavePreparingForBattle += OnPlayViewShown;

        ShopEvents.LootBoxPurchased += OnLootBoxPurchased;
        ShopEvents.LootBoxRewardClaimed += OnLootBoxRewardClaimed;
        ShopEvents.LootBoxPurchaseIntiated += OnLootBoxPurchaseIntiated;
    }

    private void UnSubscribeFromEvents()
    {
        MainTabBarEvents.PlayScreenShown -= OnPlayViewShown;
        MainTabBarEvents.ArsenalViewShown -= OnArsenalViewShown;
        MainTabBarEvents.ShopViewShown += OnShopViewShown;


        InspectWeaponEvents.BackButtonClicked -= OnArsenalViewShown;

        ArsenalEvents.WeaponItemClicked -= OnInspectViewShown;

        PlayerEvents.PlayBattleStageButtonPressed -= OnPreparingForBattleStageShown;
        PlayerEvents.SettingsButtonPressed -= OnSettingsPanelShown;
        PreparingForBattleStageEvents.LeavePreparingForBattle -= OnPlayViewShown;

        ShopEvents.LootBoxPurchased -= OnLootBoxPurchased;
        ShopEvents.LootBoxRewardClaimed -= OnLootBoxRewardClaimed;
        ShopEvents.LootBoxPurchaseIntiated -= OnLootBoxPurchaseIntiated;
    }

    private void SetupViews()
    {
        VisualElement root = m_MainGameDocument.rootVisualElement;

        m_PlayView = new PlayView(root.Q<VisualElement>(k_PlayViewName));
        m_ArsenalView = new ArsenalView(root.Q<VisualElement>(k_ArsenalViewName));

        m_SettingView = new SettingsView(root.Q<VisualElement>(k_SettingView));
        m_settingsController.Setup(m_SettingView);

        m_InspectView = new InspectView(root.Q<VisualElement>(k_InspectViewName));
        m_inspectorController.Setup(m_InspectView);

        m_PreparingForBattleStageView = new PreparingForBattleStageView(root.Q<VisualElement>(k_PreparingForBattleStageViewName));
        m_preparingForBattleStageController.Setup(m_PreparingForBattleStageView);

        m_ShopView = new ShopView(root.Q<VisualElement>(k_ShopView));
        m_shopController.Setup(m_ShopView);

        m_TabsView = new TabsView(root.Q<VisualElement>(k_TabsViewName));
        m_CurrenciesView = new CurrenciesView(root.Q<VisualElement>(k_CurrenciesViewName), false);
        m_processingView = new ProcessingView(root.Q<VisualElement>(k_ProcessingView));

        m_AllViews.Add(m_PlayView);
        m_AllViews.Add(m_SettingView);
        m_AllViews.Add(m_TabsView);
        m_AllViews.Add(m_ArsenalView);
        m_AllViews.Add(m_InspectView);
        m_AllViews.Add(m_CurrenciesView);
        m_AllViews.Add(m_PreparingForBattleStageView);
        m_AllViews.Add(m_ShopView);
        m_AllViews.Add(m_processingView);
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

    private void OnLootBoxPurchaseIntiated(LootBoxData data)
    {
        m_processingView.Show();
    }

    private void OnLootBoxPurchased()
    {
        m_TabsView.Hide();
        m_processingView.Hide();
    }

    private void OnLootBoxRewardClaimed()
    {
        m_TabsView.Show();
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
