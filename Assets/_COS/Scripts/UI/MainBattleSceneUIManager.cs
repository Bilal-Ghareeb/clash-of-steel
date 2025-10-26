using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainBattleSceneUIManager : MonoBehaviour
{
    [Header("Core Battle Manager")]
    [SerializeField] private BattleManager m_battle;

    [Header("UI Controllers")]
    [SerializeField] private WeaponsHUDController m_weaponsHUDController;
    [SerializeField] private BattleActionsController m_battleActionsController;

    private UIDocument m_BattleUIDocument;

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

        SetupViews();

        m_battle.Init(m_weaponsHUDController);

        ShowModalView(m_WeaponsHUDView);
        ShowModalView(m_BattleActionsView);
    }

    private void SetupViews()
    {
        VisualElement root = m_BattleUIDocument.rootVisualElement;

        m_WeaponsHUDView = new WeaponsHUDView(root.Q<VisualElement>(k_WeaponsHUDView), false);
        m_weaponsHUDController.Setup(m_battle, m_WeaponsHUDView);

        m_BattleActionsView = new BattleActionsView(root.Q(k_BattleActionsView), false);
        m_battleActionsController.Setup(m_battle, m_BattleActionsView);

        m_BattleResultView = new BattleResultView(root.Q<VisualElement>(k_BattleResultView));

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
