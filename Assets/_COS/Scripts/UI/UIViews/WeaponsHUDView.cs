using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class WeaponsHUDView : UIView
{
    private BattleManager m_Battle;

    private VisualTreeAsset m_WeaponItemAsset;
    private VisualElement m_PlayerContainer;
    private VisualElement m_EnemyContainer;

    private readonly Dictionary<Combatant, WeaponItemComponent> m_PlayerCombatantsUI = new();
    private readonly Dictionary<Combatant, WeaponItemComponent> m_EnemyCombatantsUI = new();


    public event Action<Combatant, int> OnRequestSwitch;
    public event Action OnInitialized;
    private bool m_IsPlayerAllocationPhase = false;


    public WeaponsHUDView(VisualElement topElement,bool hideOnAwake = true)
        : base(topElement, hideOnAwake)
    {
        m_WeaponItemAsset = Resources.Load<VisualTreeAsset>("WeaponItem");
    }

    public void InitializeBattleManager(BattleManager battle)
    {
        m_Battle = battle;
        m_Battle.OnBattleStarted += OnBattleStarted;
        m_Battle.OnCombatantDamaged += OnCombatantDamaged;
        m_Battle.OnClassComparison += OnClassComparison;
        m_Battle.OnPlayerWeaponSwitched += OnPlayerWeaponSwitched;

        m_Battle.OnEnemyTurnStarted += ResetAllAttackColors;
        m_Battle.OnPlayerTurnStarted += ResetAllAttackColors;

        m_Battle.OnPlayerAllocationPhaseStarted += (pts) => { m_IsPlayerAllocationPhase = true; };
        m_Battle.OnAllocationsRevealed += (p, e) => { m_IsPlayerAllocationPhase = false; };
        m_Battle.OnTurnChanged += (turn, pts) => { m_IsPlayerAllocationPhase = false; };

        OnInitialized?.Invoke();

    }

    public override void Dispose()
    {
        base.Dispose();
        if (m_Battle != null)
        {
            m_Battle.OnBattleStarted -= OnBattleStarted;
            m_Battle.OnCombatantDamaged -= OnCombatantDamaged;
            m_Battle.OnClassComparison -= OnClassComparison;
            m_Battle.OnPlayerWeaponSwitched -= OnPlayerWeaponSwitched;

            m_Battle.OnEnemyTurnStarted -= ResetAllAttackColors;
            m_Battle.OnPlayerTurnStarted -= ResetAllAttackColors;

            m_Battle.OnPlayerAllocationPhaseStarted -= (pts) => { m_IsPlayerAllocationPhase = true; };
            m_Battle.OnAllocationsRevealed -= (p, e) => { m_IsPlayerAllocationPhase = false; };
            m_Battle.OnTurnChanged -= (turn, pts) => { m_IsPlayerAllocationPhase = false; };
        }
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        m_PlayerContainer = m_TopElement.Q<VisualElement>("PlayerWeaponsContainer");
        m_EnemyContainer = m_TopElement.Q<VisualElement>("EnemyWeaponsContainer");
    }

    private void OnBattleStarted()
    {
        if (m_Battle == null) return;

        CreateWeaponsCardsHUD(m_Battle.PlayerTeam, m_PlayerContainer , m_PlayerCombatantsUI);
        CreateWeaponsCardsHUD(m_Battle.EnemyTeam, m_EnemyContainer , m_EnemyCombatantsUI);
    }

    private void CreateWeaponsCardsHUD(List<Combatant> combatants, VisualElement container, Dictionary<Combatant, WeaponItemComponent> combatantsDictionary)
    {
        foreach (var kvp in combatantsDictionary)
        {
            kvp.Value.UnRegisterButtonCallbacks();
            kvp.Value.OnCustomClick = null;
        }
        combatantsDictionary.Clear();

        container.Clear();
        if (combatants == null || combatants.Count == 0)
            return;

        for (int i = 0; i < combatants.Count; i++)
        {
            var combatant = combatants[i];
            if (combatant?.InstanceData == null)
                continue;

            TemplateContainer itemTemplate = m_WeaponItemAsset.Instantiate();
            var weaponItem = new WeaponItemComponent();
            weaponItem.SetVisualElements(itemTemplate);
            weaponItem.SetGameData(combatant.InstanceData);

            bool isFirst = i == 0;
            float scale = isFirst ? 1f : 0.7f;
            itemTemplate.style.scale = new StyleScale(new Vector2(scale, scale));

            weaponItem.UnRegisterButtonCallbacks();

            if (!isFirst)
            {
                int indexCopy = i;
                weaponItem.OnCustomClick = () => OnWeaponItemClicked(combatant, indexCopy);
                weaponItem.RegisterButtonCallbacks(useCustomClick: true);
            }
            else
            {
                weaponItem.OnCustomClick = null;
            }

            container.Add(itemTemplate);
            combatantsDictionary[combatant] = weaponItem;
        }
    }

    private void OnPlayerWeaponSwitched(Combatant newActive, Combatant oldActive)
    {
        if (m_PlayerCombatantsUI == null) return;

        int newIndex = m_Battle.PlayerTeam.IndexOf(newActive);
        if (newIndex >= 0)
        {
            UpdatePlayerWeaponsOrderAfterSwitch(newIndex);
        }
    }

    public void UpdatePlayerWeaponsOrderAfterSwitch(int newActiveIndex)
    {
        if (m_PlayerCombatantsUI == null || newActiveIndex < 0 || newActiveIndex >= m_Battle.PlayerTeam.Count)
            return;

        var newActiveWeapon = m_Battle.PlayerTeam[newActiveIndex];
        var outgoingWeapon = m_Battle.PlayerTeam[0];

        if (m_PlayerCombatantsUI.TryGetValue(newActiveWeapon, out var newActiveWeaponItem))
        {
            newActiveWeaponItem.UnRegisterButtonCallbacks();
            newActiveWeaponItem.OnCustomClick = null;
        }

        if (m_PlayerCombatantsUI.TryGetValue(outgoingWeapon, out var oldActiveWeaponItem))
        {
            if (outgoingWeapon.IsAlive)
            {
                oldActiveWeaponItem.OnCustomClick = () => OnWeaponItemClicked(outgoingWeapon, newActiveIndex);
                oldActiveWeaponItem.RegisterButtonCallbacks(useCustomClick: true);
            }
            else
            {
                newActiveWeaponItem.UnRegisterButtonCallbacks();
                newActiveWeaponItem.OnCustomClick = null;
            }
        }

        m_PlayerContainer.Clear();

        if (m_PlayerCombatantsUI.TryGetValue(newActiveWeapon, out var activeWeaponItem))
        {
            VisualElement root = activeWeaponItem.Root;
            root.experimental.animation
                .Scale(1f, 500)
                .Ease(Easing.OutCubic);
            m_PlayerContainer.Add(root);
        }

        for (int i = 0; i < m_Battle.PlayerTeam.Count; i++)
        {
            if (i == newActiveIndex)
                continue;

            var combatant = m_Battle.PlayerTeam[i];
            if (!m_PlayerCombatantsUI.TryGetValue(combatant, out var weaponItem))
                continue;

            VisualElement root = weaponItem.Root;
            root.experimental.animation
                .Scale(0.7f, 500)
                .Ease(Easing.OutCubic);
            m_PlayerContainer.Add(root);
        }
    }


    private void OnClassComparison(Combatant attacker, Combatant defender, float multiplier)
    {
        bool hasAdvantage = multiplier > 1.0f;
        bool hasDisadvantage = multiplier < 1.0f;

        if (m_PlayerCombatantsUI.TryGetValue(attacker, out var playerWeaponUI))
        {
            float newAttackValue = attacker.BaseAttack * multiplier;
            playerWeaponUI.UpdateAttackPreview(newAttackValue, hasAdvantage, hasDisadvantage);
        }

        if (m_EnemyCombatantsUI.TryGetValue(attacker, out var enemyWeaponUI))
        {
            float newAttackValue = attacker.BaseAttack * multiplier;
            enemyWeaponUI.UpdateAttackPreview(newAttackValue, hasAdvantage, hasDisadvantage);
        }
    }

    private void ResetAllAttackColors()
    {
        foreach (var item in m_PlayerCombatantsUI.Values)
            item.ResetAttackHudData();
        foreach (var item in m_EnemyCombatantsUI.Values)
            item.ResetAttackHudData();
    }

    private void OnCombatantDamaged( Combatant attacker, Combatant defender, int defenderNewHealth , int attackerDamage)
    {
        if (defender == null)
            return;

        bool isPlayerAttacker = attacker.InstanceData is WeaponInstance;
        bool isEnemyDefender = defender.InstanceData is EnemyWeaponInstance;

        WeaponItemComponent attackerUI = null;
        WeaponItemComponent defenderUI = null;

        if (isPlayerAttacker)
            m_PlayerCombatantsUI.TryGetValue(attacker, out attackerUI);
        else
            m_EnemyCombatantsUI.TryGetValue(attacker, out attackerUI);

        if (isEnemyDefender)
            m_EnemyCombatantsUI.TryGetValue(defender, out defenderUI);
        else
            m_PlayerCombatantsUI.TryGetValue(defender, out defenderUI);

        if (attackerUI == null || defenderUI == null)
        {
            Debug.LogWarning("Missing UI card for combatants in OnCombatantDamaged");
            return;
        }

        if(defenderNewHealth <= 0)
        {
            defenderUI.ApplyDeadCardStyle();
        }

        defenderUI.UpdateHealth(defenderNewHealth);
    }

    private void OnWeaponItemClicked(Combatant combatant, int index)
    {
        if (!m_IsPlayerAllocationPhase) return;
        var active = m_Battle?.GetActivePlayerCombatant();
        if (active == null || combatant == active) return;

        int currentIndex = m_Battle.PlayerTeam.IndexOf(combatant);
        OnRequestSwitch?.Invoke(combatant, currentIndex);
    }

}
