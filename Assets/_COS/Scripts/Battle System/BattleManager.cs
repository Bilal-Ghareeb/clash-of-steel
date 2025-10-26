using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int m_maxPointsPerTurn = 4;
    [SerializeField] private int m_initialPlayerBasePoints = 1;
    [SerializeField] private int m_initialEnemyBasePoints = 2;
    [SerializeField] private int m_bankCap = 8;

    [Header("Timing Settings (seconds)")]
    [SerializeField] private float m_turnStartDelay = 1.2f;
    [SerializeField] private float m_revealDelay = 3f;
    [SerializeField] private float m_resultDelay = 1.5f;
    [SerializeField] private float m_countdownDelay = 3.5f;

    [Header("Timing Settings (ms)")]
    [SerializeField] private int m_turnStartDelayMs = 1000;
    [SerializeField] private int m_revealDelayMs = 2000;
    [SerializeField] private int m_resultDelayMs = 1500;
    [SerializeField] private int m_countdownDelayMs = 3500;

    public List<Combatant> PlayerTeam { get; private set; }
    public List<Combatant> EnemyTeam { get; private set; }
    public bool PlayerWon { get; private set; }

    public event Action OnBattleCountdownStarted;
    public event Action OnBattleStarted;
    public event Action OnBattleEnded;
    public event Action<int> OnPlayerAllocationPhaseStarted;
    public event Action<int> OnEnemyAllocationPhaseStarted;
    public event Action OnEnemyFinishedAllocating;
    public event Action<(int attack, int defend), (int attack, int defend)> OnAllocationsRevealed;
    public event Action OnPlayerTurnStarted;
    public event Action OnEnemyTurnStarted;
    public event Action<Combatant, Combatant, float> OnClassComparison;
    public event Action<Combatant, Combatant, int, int> OnCombatantDamaged;
    public event Action<Combatant> OnCombatantDeath;
    public event Action<int, int> OnTurnChanged;
    public event Action<Combatant, Combatant, bool, bool> OnWeaponSwitched;
    public event Action OnPlayerWeaponEntranceCompleted;

    private Combatant m_currentPlayerWeapon;
    private Combatant m_currentEnemyWeapon;
    private int m_playerBasePoints;
    private int m_playerReservedPoints;
    private int m_enemyBasePoints;
    private int m_enemyReservedPoints;
    private int m_activePlayerWeaponIndex;
    private int m_activeEnemyWeaponIndex;
    private int m_turnNumber;

    private TaskCompletionSource<(int attack, int defend, int reserve)> m_playerAllocationTcs;
    private WeaponsHUDController m_weaponHUDController;


    public void Init(WeaponsHUDController controller)
    {
        m_weaponHUDController = controller;
        m_weaponHUDController.OnRequestSwitch += SwitchActivePlayerWeapon;
    }

    private async void Start()
    {
        if (!ValidateBattleSession()) return;

        InitializeBasePoints();
        BuildTeamsFromSession(BattleSessionHolder.CurrentSession);

        await StartBattleSequence();
    }

    private bool ValidateBattleSession()
    {
        var session = BattleSessionHolder.CurrentSession;
        if (session == null)
        {
            Debug.LogError("BattleManager: No BattleSession found. Abort.");
            return false;
        }
        return true;
    }

    private void InitializeBasePoints()
    {
        m_playerBasePoints = Mathf.Clamp(m_initialPlayerBasePoints, 1, m_maxPointsPerTurn);
        m_enemyBasePoints = Mathf.Clamp(m_initialEnemyBasePoints, 1, m_maxPointsPerTurn);
        m_playerReservedPoints = 0;
        m_enemyReservedPoints = 0;
    }

    private async Task StartBattleSequence()
    {
        OnBattleStarted?.Invoke();
        await Task.Delay(500);

        OnBattleCountdownStarted?.Invoke();
        await Task.Delay(m_countdownDelayMs);

        await RunBattleLoop();

        OnBattleEnded?.Invoke();
    }


    private void BuildTeamsFromSession(BattleSession session)
    {
        PlayerTeam = session.playerTeam.Select(dto => CreateCombatantFromDTO(dto, true)).ToList();
        EnemyTeam = session.enemyTeam.Select(dto => CreateCombatantFromDTO(dto, false)).ToList();
    }

    private Combatant CreateCombatantFromDTO(BattleSession.CombatantDTO dto, bool isPlayer)
    {
        var instance = TryGetPlayerWeaponInstance(dto, isPlayer);

        if (instance != null)
            return new Combatant(instance, dto.instanceId, instance.CatalogData.name);

        return CreateFallbackCombatant(dto);
    }

    private WeaponInstanceBase TryGetPlayerWeaponInstance(BattleSession.CombatantDTO dto, bool isPlayer)
    {
        if (!isPlayer || string.IsNullOrEmpty(dto.instanceId)) return null;
        return PlayFabManager.Instance?.EconomyService.PlayerWeapons?
            .FirstOrDefault(w => w.Item?.Id == dto.instanceId) as WeaponInstance;
    }


    private Combatant CreateFallbackCombatant(BattleSession.CombatantDTO dto)
    {
        var weaponData = PlayFabManager.Instance.EconomyService.GetWeaponDataByFriendlyId(dto.friendlyId);
        if (weaponData == null)
        {
            Debug.LogError($"No WeaponData for friendly id {dto.friendlyId}. Creating placeholder.");
            weaponData = new WeaponData
            {
                name = dto.friendlyId ?? "Unknown",
                baseDamage = 10,
                baseHealth = 100,
                rarity = "Common",
                @class = "Sword",
                progressionId = "sword_linear_common"
            };
        }

        var enemyInstance = new EnemyWeaponInstance(dto.friendlyId ?? dto.instanceId ?? dto.friendlyId, weaponData, dto.level);
        return new Combatant(enemyInstance, dto.friendlyId, weaponData.name);
    }


    private async Task RunBattleLoop()
    {
        while (AnyAlive(PlayerTeam) && AnyAlive(EnemyTeam))
        {
            await ExecuteTurnCycle();
        }

        PlayerWon = AnyAlive(PlayerTeam) && !AnyAlive(EnemyTeam);
    }

    private async Task ExecuteTurnCycle()
    {
        m_turnNumber++;

        int availablePlayerPoints = Mathf.Min(m_playerBasePoints + m_playerReservedPoints, m_bankCap);
        OnTurnChanged?.Invoke(m_turnNumber, availablePlayerPoints);

        await ExecutePlayerTurn(availablePlayerPoints);
        if (!AnyAlive(EnemyTeam) || !AnyAlive(PlayerTeam)) return;

        await ExecuteEnemyTurn();
        IncrementBasePoints();
        await Task.Delay(1000);
    }


    private async Task ExecutePlayerTurn(int availablePlayerPoints)
    {
        OnPlayerTurnStarted?.Invoke();
        await Task.Delay(1200);

        m_currentPlayerWeapon = PlayerTeam[m_activePlayerWeaponIndex];
        m_currentEnemyWeapon = EnemyTeam[m_activeEnemyWeaponIndex];

        if (m_currentPlayerWeapon == null || !m_currentPlayerWeapon.IsAlive) return;

        await HandlePlayerAllocationPhase(availablePlayerPoints);
        await HandlePlayerAttackPhase();
    }

    private async Task HandlePlayerAllocationPhase(int availablePlayerPoints)
    {
        m_currentPlayerWeapon.ResetRoundAllocations();
        m_currentPlayerWeapon.StartTurnWithAvailablePoints(availablePlayerPoints, m_bankCap);

        OnPlayerAllocationPhaseStarted?.Invoke(availablePlayerPoints);
        var playerAlloc = await WaitForPlayerAllocationAsync(availablePlayerPoints);

        m_playerReservedPoints = playerAlloc.reserve;
        m_currentPlayerWeapon.TryAllocate(playerAlloc.attack, playerAlloc.defend, playerAlloc.reserve, out _);

        var playerReveal = (m_currentPlayerWeapon.AttackPoints, m_currentEnemyWeapon?.DefendPoints ?? 0);
        OnAllocationsRevealed?.Invoke(playerReveal, (0, 0));

        await Task.Delay(3000);
    }

    private async Task HandlePlayerAttackPhase()
    {
        if (m_currentEnemyWeapon == null || m_currentPlayerWeapon.AttackPoints <= 0)
            return;

        float classMult = ActionResolver.GetClassMultiplier(m_currentPlayerWeapon.ClassType, m_currentEnemyWeapon.ClassType);
        OnClassComparison?.Invoke(m_currentPlayerWeapon, m_currentEnemyWeapon, classMult);

        if (TimelineController.Instance != null)
            await TimelineController.Instance.PlayAttackAsync(m_currentPlayerWeapon);

        int damage = ActionResolver.ResolveDamage(m_currentPlayerWeapon, m_currentEnemyWeapon);
        m_currentEnemyWeapon.CurrentHP = Mathf.Max(0, m_currentEnemyWeapon.CurrentHP - damage);
        OnCombatantDamaged?.Invoke(m_currentPlayerWeapon, m_currentEnemyWeapon, m_currentEnemyWeapon.CurrentHP, damage);

        if (!m_currentEnemyWeapon.IsAlive)
        {
            await TimelineController.Instance.PlayDeathAsync(m_currentEnemyWeapon);
            OnCombatantDeath?.Invoke(m_currentEnemyWeapon);
            await ForceSwitchAfterDeath(false);
        }

        await Task.Delay(1500);
    }


    private async Task ExecuteEnemyTurn()
    {
        OnEnemyTurnStarted?.Invoke();
        await Task.Delay(1200);

        if (m_currentEnemyWeapon == null || m_currentPlayerWeapon == null) return;

        int enemyAvailablePoints = Mathf.Min(m_enemyBasePoints + m_enemyReservedPoints, m_bankCap);
        m_currentEnemyWeapon.StartTurnWithAvailablePoints(enemyAvailablePoints, m_bankCap);

        int attack = enemyAvailablePoints;
        int defend = 0;
        int reserve = 0;

        await Task.Delay(2000);

        m_currentEnemyWeapon.TryAllocate(attack, defend, reserve, out _);
        m_enemyReservedPoints = reserve;

        OnEnemyFinishedAllocating?.Invoke();

        var enemyReveal = (m_currentEnemyWeapon.AttackPoints, m_currentPlayerWeapon.DefendPoints);
        OnAllocationsRevealed?.Invoke((0, 0), enemyReveal);
        await Task.Delay(3000);

        await HandleEnemyAttackPhase();
        await Task.Delay(1500);
    }

    private async Task HandleEnemyAttackPhase()
    {
        if (m_currentEnemyWeapon.AttackPoints <= 0) return;

        float classMult = ActionResolver.GetClassMultiplier(m_currentEnemyWeapon.ClassType, m_currentPlayerWeapon.ClassType);
        OnClassComparison?.Invoke(m_currentEnemyWeapon, m_currentPlayerWeapon, classMult);

        if (TimelineController.Instance != null)
            await TimelineController.Instance.PlayAttackAsync(m_currentEnemyWeapon);

        int damage = ActionResolver.ResolveDamage(m_currentEnemyWeapon, m_currentPlayerWeapon);
        m_currentPlayerWeapon.CurrentHP = Mathf.Max(0, m_currentPlayerWeapon.CurrentHP - damage);
        OnCombatantDamaged?.Invoke(m_currentEnemyWeapon, m_currentPlayerWeapon, m_currentPlayerWeapon.CurrentHP, damage);

        if (!m_currentPlayerWeapon.IsAlive)
        {
            await TimelineController.Instance.PlayDeathAsync(m_currentPlayerWeapon);
            OnCombatantDeath?.Invoke(m_currentPlayerWeapon);
            await ForceSwitchAfterDeath(true);
        }
    }

    private void IncrementBasePoints()
    {
        m_playerBasePoints = Mathf.Min(m_playerBasePoints + 1, m_maxPointsPerTurn);
        m_enemyBasePoints = Mathf.Min(m_enemyBasePoints + 1, m_maxPointsPerTurn);
    }

    private Task<(int attack, int defend, int reserve)> WaitForPlayerAllocationAsync(int availablePoints)
    {
        m_playerAllocationTcs = new TaskCompletionSource<(int, int, int)>();
        return m_playerAllocationTcs.Task;
    }

    private bool AnyAlive(List<Combatant> team) =>
        team != null && team.Any(x => x.IsAlive);

    public int GetCurrentPlayerAvailablePoints() =>
        Mathf.Min(m_playerBasePoints + m_playerReservedPoints, m_bankCap);

    public int GetCurrentEnemyAvailablePoints() =>
        Mathf.Min(m_enemyBasePoints + m_enemyReservedPoints, m_bankCap);

    public int GetPlayerCurrentBasePoints() =>
        m_playerBasePoints;

    public int GetEnemyCurrentBasePoints() =>
    m_enemyBasePoints;

    public async void SwitchActivePlayerWeapon(Combatant incomingCombatant)
    {
        TrySwitchActiveWeapon(incomingCombatant, 1, true, true);
        await TimelineController.Instance.PlayEntranceAsync(incomingCombatant);
        OnPlayerWeaponEntranceCompleted?.Invoke();
    }

    public async void ForceSwitchActiveWeapon(bool isPlayer = true)
    {
        await ForceSwitchAfterDeath(isPlayer);
    }

    private async Task ForceSwitchAfterDeath(bool isPlayer)
    {
        var team = isPlayer ? PlayerTeam : EnemyTeam;
        if (team == null || team.All(w => !w.IsAlive))
            return;

        int activeIndex = isPlayer ? m_activePlayerWeaponIndex : m_activeEnemyWeaponIndex;
        var activeCombatant = team[activeIndex];

        if (team.All(w => !w.IsAlive || w == activeCombatant))
            return;

        for (int i = 0; i < team.Count; i++)
        {
            if (i != activeIndex && team[i].IsAlive)
            {
                var newCombatant = team[i];
                TrySwitchActiveWeapon(newCombatant, 0, false, isPlayer);
                await TimelineController.Instance.PlayEntranceAsync(newCombatant);

                if (isPlayer)
                    OnPlayerWeaponEntranceCompleted?.Invoke();

                return;
            }
        }
    }

    private void TrySwitchActiveWeapon(Combatant incomingCombatant, int switchCost, bool deductCost, bool isPlayer)
    {
        var team = isPlayer ? PlayerTeam : EnemyTeam;
        if (team == null) return;

        int activeIndex = isPlayer ? m_activePlayerWeaponIndex : m_activeEnemyWeaponIndex;
        int newIndex = team.IndexOf(incomingCombatant);

        if (newIndex < 0 || newIndex == activeIndex || !incomingCombatant.IsAlive)
            return;

        var oldActive = team[activeIndex];
        var newActive = team[newIndex];

        int availablePoints = isPlayer
            ? GetCurrentPlayerAvailablePoints()
            : GetCurrentEnemyAvailablePoints();

        if (deductCost && availablePoints < switchCost)
            return;

        if (isPlayer && !oldActive.CanSwitchWeapon)
        {
            Debug.Log("Cannot switch again this turn.");
            return;
        }

        newActive.Switch();

        if (isPlayer)
        {
            m_currentPlayerWeapon = newActive;
            newActive.StartTurnWithAvailablePoints(availablePoints, m_bankCap);
        }
        else
        {
            m_currentEnemyWeapon = newActive;
            newActive.StartTurnWithAvailablePoints(availablePoints, m_bankCap);
        }

        OnWeaponSwitched?.Invoke(newActive, oldActive, deductCost, isPlayer);
        if (isPlayer)
            OnTurnChanged?.Invoke(m_turnNumber, GetCurrentPlayerAvailablePoints());

        team.RemoveAt(newIndex);
        team.Insert(0, newActive);

        if (isPlayer) m_activePlayerWeaponIndex = 0;
        else m_activeEnemyWeaponIndex = 0;
    }


    public bool AllocateForCombatant(Combatant actor, int attack, int defend, int reserve, out string error)
    {
        if (m_playerAllocationTcs == null)
        {
            error = "Not accepting allocations right now.";
            return false;
        }

        int total = attack + defend + reserve;
        if (total > m_bankCap)
        {
            error = $"Requested {total} > bankCap {m_bankCap}";
            return false;
        }

        if (!actor.TryAllocate(attack, defend, reserve, out error))
            return false;

        m_playerAllocationTcs.SetResult((attack, defend, reserve));
        m_playerAllocationTcs = null;
        error = null;
        return true;
    }

    public Combatant GetActivePlayerCombatant()
    {
        if (PlayerTeam == null || PlayerTeam.Count == 0) return null;
        m_activePlayerWeaponIndex = Mathf.Clamp(m_activePlayerWeaponIndex, 0, PlayerTeam.Count - 1);
        return PlayerTeam[m_activePlayerWeaponIndex];
    }

    public Combatant GetActiveEnemyCombatant()
    {
        if (EnemyTeam == null || EnemyTeam.Count == 0) return null;
        m_activeEnemyWeaponIndex = Mathf.Clamp(m_activeEnemyWeaponIndex, 0, EnemyTeam.Count - 1);
        return EnemyTeam[m_activeEnemyWeaponIndex];
    }

    public void EndBattleAndReturnToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
