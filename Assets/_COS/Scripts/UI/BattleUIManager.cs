using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleUIManager : MonoBehaviour
{
    private BattleManager m_battle;

    private UIDocument m_BattleUIDocument;

    private List<UIView> m_AllViews = new List<UIView>();

    private WeaponsHUDView m_WeaponsHUDView;
    private BattleActionsView m_BattleActionsView;

    const string k_WeaponsHUDView = "WeaponsHUDView";
    const string k_BattleActionsView = "BattleActionsView";

    private void Awake()
    {
        m_BattleUIDocument = GetComponent<UIDocument>();
        m_battle = FindAnyObjectByType<BattleManager>();

        SetupViews();
        SubscribeToEvents();

        ShowModalView(m_WeaponsHUDView);
        ShowModalView(m_BattleActionsView);
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

    }

    private void UnSubscribeFromEvents()
    {

    }

    private void SetupViews()
    {
        VisualElement root = m_BattleUIDocument.rootVisualElement;

        m_WeaponsHUDView = new WeaponsHUDView(root.Q<VisualElement>(k_WeaponsHUDView) , false);
        m_WeaponsHUDView.InitializeBattleManager(m_battle);

        m_BattleActionsView = new BattleActionsView(root.Q(k_BattleActionsView) , false);
        m_BattleActionsView.InitializeBattleManager(m_battle);

        m_AllViews.Add(m_WeaponsHUDView);
        m_AllViews.Add(m_BattleActionsView);
    }


    private void ShowModalView(UIView newView)
    {
        newView.Show();
    }
}
