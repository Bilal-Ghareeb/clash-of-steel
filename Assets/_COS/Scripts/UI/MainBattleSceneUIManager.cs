using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainBattleSceneUIManager : MonoBehaviour
{
    private UIDocument m_BattleUIDocument;
    private BattleManager m_battle;

    private List<UIView> m_AllViews = new List<UIView>();

    private WeaponsHUDView m_WeaponsHUDView;
    private BattleActionsView m_BattleActionsView;
    private BattleResultView m_BattleResultView;

    const string k_WeaponsHUDView = "WeaponsHUDView";
    const string k_BattleActionsView = "BattleActionsView";
    const string k_BattleResultView = "BattleResultView";

    private void Awake()
    {
        m_BattleUIDocument = GetComponent<UIDocument>();
        m_battle = FindAnyObjectByType<BattleManager>();

        SetupViews();

        ShowModalView(m_WeaponsHUDView);
        ShowModalView(m_BattleActionsView);
    }

    private void SetupViews()
    {
        VisualElement root = m_BattleUIDocument.rootVisualElement;

        m_WeaponsHUDView = new WeaponsHUDView(root.Q<VisualElement>(k_WeaponsHUDView), false);
        m_BattleActionsView = new BattleActionsView(root.Q(k_BattleActionsView), false);
        m_BattleResultView = new BattleResultView(root.Q<VisualElement>(k_BattleResultView));

        m_WeaponsHUDView.InitializeBattleManager(m_battle);
        m_BattleActionsView.InitializeBattleManager(m_battle);

        m_battle.Init(m_WeaponsHUDView);

        m_AllViews.Add(m_WeaponsHUDView);
        m_AllViews.Add(m_BattleActionsView);
        m_AllViews.Add(m_BattleResultView);
    }

    private void OnDisable()
    {
        foreach (UIView view in m_AllViews)
            view.Dispose();
    }

    public BattleResultView GetResultView() => m_BattleResultView;

    private void ShowModalView(UIView view) => view.Show();
}
