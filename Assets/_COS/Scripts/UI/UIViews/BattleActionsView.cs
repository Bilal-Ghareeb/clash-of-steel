using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleActionsView : UIView
{
    private BattleManager m_Battle;

    private VisualElement m_ButtonContainer;
    private Button m_AttackButton;
    private Button m_DefendButton;
    private Button m_ReserveButton;

    private Label m_CountdownLabel;

    private int m_AttackPoints;
    private int m_DefendPoints;
    private int m_ReservePoints;

    public BattleActionsView(VisualElement topElement, bool hideOnAwake = true) : base(topElement, hideOnAwake)
    {
    }

    public void InitializeBattleManager(BattleManager battle)
    {
        m_Battle = battle;
        m_Battle.OnBattleCountdownStarted += OnCountdownStarted;
        m_Battle.OnBattleStarted += OnBattleStarted;
    }

    public override void Dispose()
    {
        base.Dispose();

        m_Battle.OnBattleCountdownStarted -= OnCountdownStarted;
        m_Battle.OnBattleStarted -= OnBattleStarted;
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();

        m_ButtonContainer = m_TopElement.Q<VisualElement>("ActionButtonsContainer");

        m_AttackButton = m_TopElement.Q<Button>("AttackButton");
        m_DefendButton = m_TopElement.Q<Button>("DefendButton");
        m_ReserveButton = m_TopElement.Q<Button>("ReserveButton");

        m_CountdownLabel = m_TopElement.Q<Label>("CountdownLabel");

    }

    protected override void RegisterButtonCallbacks()
    {
        if (m_AttackButton != null)
            m_AttackButton.clicked += () => SpendPoint(ref m_AttackPoints, "Attack");

        if (m_DefendButton != null)
            m_DefendButton.clicked += () => SpendPoint(ref m_DefendPoints, "Defend");

        if (m_ReserveButton != null)
            m_ReserveButton.clicked += () => SpendPoint(ref m_ReservePoints, "Reserve");
    }

    private async void OnCountdownStarted()
    {
        if (m_CountdownLabel == null) return;

        m_AttackButton.style.display = DisplayStyle.None;
        m_DefendButton.style.display = DisplayStyle.None;
        m_ReserveButton.style.display = DisplayStyle.None;

        m_CountdownLabel.style.display = DisplayStyle.Flex;
        string[] sequence = { "3", "2", "1", "FIGHT!" };

        foreach (string s in sequence)
        {
            m_CountdownLabel.text = s;
            await Task.Delay(800);
        }

        m_CountdownLabel.style.display = DisplayStyle.None;
    }

    private void SpendPoint(ref int targetValue, string actionName)
    {
        var actor = GetCurrentPlayerCombatant();
        if (actor == null) return;

        int remaining = actor.BankedPoints - (m_AttackPoints + m_DefendPoints + m_ReservePoints);
        if (remaining <= 0)
        {
            Debug.Log("No more points left!");
            FinalizeAllocations(actor);
            return;
        }

        targetValue++;

        remaining = actor.BankedPoints - (m_AttackPoints + m_DefendPoints + m_ReservePoints);
        if (remaining <= 0)
        {
            FinalizeAllocations(actor);
        }
    }

    private void FinalizeAllocations(Combatant actor)
    {
        m_ButtonContainer.style.display = DisplayStyle.None;
        string error;
        bool success = m_Battle.AllocateForCombatant(actor, m_AttackPoints, m_DefendPoints, m_ReservePoints, out error);

        if (!success)
        {
            Debug.LogError("Allocation failed: " + error);
        }

        m_AttackPoints = m_DefendPoints = m_ReservePoints = 0;
    }

    private void OnBattleStarted()
    {
        m_AttackButton.style.display = DisplayStyle.Flex;
        m_DefendButton.style.display = DisplayStyle.Flex;
        m_ReserveButton.style.display = DisplayStyle.Flex;
    }

    private Combatant GetCurrentPlayerCombatant()
    {
        return m_Battle?.PlayerTeam?.FirstOrDefault(c => c.IsAlive);
    }
}
