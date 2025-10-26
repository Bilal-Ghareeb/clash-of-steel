using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


public class BattleManager : MonoBehaviour
{
    [SerializeField] private int maxPointsPerTurn = 4;
    [SerializeField] private int initialPlayerBasePoints = 1;
    [SerializeField] private int initialEnemyBasePoints = 2;
    [SerializeField] private int bankCap = 8;

    public List<Combatant> PlayerTeam { get; private set; }
    public List<Combatant> EnemyTeam { get; private set; }

    private Combatant m_currentPlayerWeapon;
    private Combatant m_currentEnemyWeapon;

    private int playerBasePoints;
    private int playerReservedPoints;
    private int enemyBasePoints;
    private int enemyReservedPoints;
    private int activePlayerWeaponIndex = 0;
    private int activeEnemyWeaponIndex = 0;
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

    public event Action<Combatant,Combatant, int , int> OnCombatantDamaged;
    public event Action<Combatant> OnCombatantDeath;

    public event Action<int, int> OnTurnChanged;

    public event Action<Combatant, Combatant , bool , bool> OnWeaponSwitched;
    public event Action OnPlayerWeaponEntranceCompleted;

    private int turnNumber = 0;

    private TaskCompletionSource<(int attack, int defend, int reserve)> _playerAllocationTcs;

    private WeaponsHUDController m_weaponHUDController;

    [Header("Timing Settings (ms)")]
    [SerializeField] private int turnStartDelayMs = 1000;
    [SerializeField] private int revealDelayMs = 2000;    
    [SerializeField] private int resultDelayMs = 1500; 
    [SerializeField] private int countdownDelayMs = 3500;


    public void Init(WeaponsHUDController controller)
    {
        m_weaponHUDController = controller;
        m_weaponHUDController.OnRequestSwitch += SwitchActivePlayerWeapon;
    }

    private async void Start()
    {
        var session = BattleSessionHolder.CurrentSession;
        if (session == null)
        {
            Debug.LogError("BattleManager: No BattleSession found. Abort.");
            return;
        }

        playerBasePoints = Mathf.Clamp(initialPlayerBasePoints, 1, maxPointsPerTurn);
        enemyBasePoints = Mathf.Clamp(initialEnemyBasePoints, 1, maxPointsPerTurn);
        playerReservedPoints = 0;
        enemyReservedPoints = 0;

        BuildTeamsFromSession(session);

        OnBattleStarted?.Invoke();
        await Task.Delay(500);

        OnBattleCountdownStarted?.Invoke();
        await Task.Delay(countdownDelayMs);

        await RunBattleLoop();

        OnBattleEnded?.Invoke();
    }

    private void BuildTeamsFromSession(BattleSession session)
    {
        PlayerTeam = session.playerTeam
            .Select(dto => CreateCombatantFromDTO(dto, true)).ToList();

        EnemyTeam = session.enemyTeam
            .Select(dto => CreateCombatantFromDTO(dto, false))
            .ToList();
    }

    private Combatant CreateCombatantFromDTO(BattleSession.CombatantDTO dto, bool isPlayer)
    {
        WeaponInstanceBase instance = null;
        if (isPlayer && !string.IsNullOrEmpty(dto.instanceId))
        {
            instance = PlayFabManager.Instance?.EconomyService.PlayerWeapons?.FirstOrDefault(w => w.Item?.Id == dto.instanceId) as WeaponInstance;
        }

        if (instance != null)
            return new Combatant(instance, dto.instanceId, instance.CatalogData.name);

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
            turnNumber++;

            int availablePlayerPoints = Mathf.Min(playerBasePoints + playerReservedPoints, bankCap);
            OnTurnChanged?.Invoke(turnNumber, availablePlayerPoints);

            // --------------------------- PLAYER TURN ---------------------------
            OnPlayerTurnStarted?.Invoke();
            await Task.Delay(1200);

            m_currentPlayerWeapon = PlayerTeam[activePlayerWeaponIndex];
            m_currentEnemyWeapon = EnemyTeam[activeEnemyWeaponIndex];

            if (m_currentPlayerWeapon != null && m_currentPlayerWeapon.IsAlive)
            {
                m_currentPlayerWeapon.ResetRoundAllocations();
                m_currentPlayerWeapon.StartTurnWithAvailablePoints(availablePlayerPoints, bankCap);

                OnPlayerAllocationPhaseStarted?.Invoke(availablePlayerPoints);
                var playerAlloc = await WaitForPlayerAllocationAsync(availablePlayerPoints);

                playerReservedPoints = playerAlloc.reserve;
                m_currentPlayerWeapon.TryAllocate(playerAlloc.attack, playerAlloc.defend, playerAlloc.reserve, out _);

                var playerReveal = (m_currentPlayerWeapon.AttackPoints, m_currentEnemyWeapon?.DefendPoints ?? 0);
                OnAllocationsRevealed?.Invoke(playerReveal, (0, 0));
                await Task.Delay(3000);

                if (m_currentEnemyWeapon != null && m_currentPlayerWeapon.AttackPoints > 0)
                {
                    float classMult = ActionResolver.GetClassMultiplier(m_currentPlayerWeapon.ClassType, m_currentEnemyWeapon.ClassType);
                    OnClassComparison?.Invoke(m_currentPlayerWeapon, m_currentEnemyWeapon, classMult);

                    if (TimelineController.Instance != null)
                        await TimelineController.Instance.PlayAttackAsync(m_currentPlayerWeapon);

                    int damage = ActionResolver.ResolveDamage(m_currentPlayerWeapon, m_currentEnemyWeapon);
                    m_currentEnemyWeapon.CurrentHP = Mathf.Max(0, m_currentEnemyWeapon.CurrentHP - damage);
                    OnCombatantDamaged?.Invoke(m_currentPlayerWeapon, m_currentEnemyWeapon, m_currentEnemyWeapon.CurrentHP , damage);

                    if (!m_currentEnemyWeapon.IsAlive)
                    {
                        await TimelineController.Instance.PlayDeathAsync(m_currentEnemyWeapon);
                        OnCombatantDeath?.Invoke(m_currentEnemyWeapon);
                        ForceSwitchActiveWeapon(isPlayer:false);
                    }
                }

                await Task.Delay(1500);
            }

            // --------------------------- ENEMY TURN ---------------------------
            if (!AnyAlive(EnemyTeam) || !AnyAlive(PlayerTeam)) break;

            OnEnemyTurnStarted?.Invoke();
            await Task.Delay(1200);

            if (m_currentEnemyWeapon != null && m_currentPlayerWeapon != null)
            {
                int enemyAvailablePoints = Mathf.Min(enemyBasePoints + enemyReservedPoints, bankCap);
                m_currentEnemyWeapon.StartTurnWithAvailablePoints(enemyAvailablePoints, bankCap);

                int attack = enemyAvailablePoints;
                int defend = 0;
                int reserve = 0;

                await Task.Delay(2000);

                m_currentEnemyWeapon.TryAllocate(attack, defend, reserve, out _);
                enemyReservedPoints = reserve;

                OnEnemyFinishedAllocating?.Invoke();

                var enemyReveal = (m_currentEnemyWeapon.AttackPoints, m_currentPlayerWeapon.DefendPoints);
                OnAllocationsRevealed?.Invoke((0, 0), enemyReveal);
                await Task.Delay(3000);

                if (m_currentEnemyWeapon.AttackPoints > 0)
                {
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
                        ForceSwitchActiveWeapon(isPlayer: true);
                    }
                }

                await Task.Delay(1500);
            }

            // --------------------------- END OF ROUND ---------------------------

            playerBasePoints = Mathf.Min(playerBasePoints + 1, maxPointsPerTurn);
            enemyBasePoints = Mathf.Min(enemyBasePoints + 1, maxPointsPerTurn);

            await Task.Delay(1000);
        }

        PlayerWon = AnyAlive(PlayerTeam) && !AnyAlive(EnemyTeam);
    }


    private Task<(int attack, int defend, int reserve)> WaitForPlayerAllocationAsync(int availablePoints)
    {
        _playerAllocationTcs = new TaskCompletionSource<(int attack, int defend, int reserve)>();
        return _playerAllocationTcs.Task;
    }

    /// <summary>
    /// Called by UI when player finished choosing allocations for the current active combatant.
    /// attack + defend + reserve must be <= availablePoints provided previously.
    /// This keeps the same signature shape as before but returns bool success and an out error message.
    /// </summary>
    public bool AllocateForCombatant(Combatant actor, int attack, int defend, int reserve, out string error)
    {
        // If no TCS active, fail (not waiting)
        if (_playerAllocationTcs == null)
        {
            error = "Not accepting allocations right now.";
            return false;
        }

        int total = attack + defend + reserve;
        // availablePoints was passed to WaitForPlayerAllocationAsync and should be validated by UI too.
        // Here we very defensively accept up to bankCap.
        if (total > bankCap)
        {
            error = $"Requested {total} > bankCap {bankCap}";
            return false;
        }

        // apply allocations onto the actor (so Combatant uses them for later calculation)
        if (!actor.TryAllocate(attack, defend, reserve, out error))
            return false;

        // resolve the TCS so RunBattleLoop continues.
        _playerAllocationTcs.SetResult((attack, defend, reserve));
        _playerAllocationTcs = null;
        error = null;
        return true;
    }

    /// <summary>
    /// Switch the active player weapon to the one at newIndex (if valid).
    /// This does not change Combatant runtime health/state — it's only swapping which one is active.
    /// Fire OnPlayerWeaponSwitched(newActive, oldActive) when successful.
    /// </summary>
    public async void SwitchActivePlayerWeapon(Combatant incomingCombatant)
    {
        TrySwitchActiveWeapon(incomingCombatant, 1, deductCost: true, isPlayer:true);
        await TimelineController.Instance.PlayEntranceAsync(incomingCombatant);
        OnPlayerWeaponEntranceCompleted?.Invoke();
    }

    public async void ForceSwitchActiveWeapon(bool isPlayer = true)
    {
        var team = isPlayer ? PlayerTeam : EnemyTeam;
        if (team == null || team.All(w => !w.IsAlive))
            return;

        int activeIndex = isPlayer ? activePlayerWeaponIndex : activeEnemyWeaponIndex;
        var activeCombatant = team[activeIndex];

        if (team.All(w => !w.IsAlive || w == activeCombatant))
            return;

        for (int i = 0; i < team.Count; i++)
        {
            if (i != activeIndex && team[i].IsAlive)
            {
                var newCombatant = team[i];

                if (isPlayer)
                {
                    TrySwitchActiveWeapon(newCombatant, 0, deductCost: false, isPlayer: true);
                    await TimelineController.Instance.PlayEntranceAsync(newCombatant);
                    OnPlayerWeaponEntranceCompleted?.Invoke();
                }
                else
                {
                    TrySwitchActiveWeapon(newCombatant, 0, deductCost: false, isPlayer: false);
                    await TimelineController.Instance.PlayEntranceAsync(newCombatant);
                }

                return;
            }
        }
    }


    /// <summary>
    /// Handles the full switching process for either player or enemy.
    /// </summary>
    private void TrySwitchActiveWeapon(Combatant incomingCombatant, int switchCost, bool deductCost, bool isPlayer)
    {
        var team = isPlayer ? PlayerTeam : EnemyTeam;
        if (team == null)
            return;

        int activeIndex = isPlayer ? activePlayerWeaponIndex : activeEnemyWeaponIndex;
        int newIndex = team.IndexOf(incomingCombatant);

        if (newIndex < 0 || newIndex >= team.Count)
            return;

        if (newIndex == activeIndex)
            return;

        var oldActive = team[activeIndex];
        var newActive = team[newIndex];

        if (newActive == null || !newActive.IsAlive)
            return;

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
            newActive.StartTurnWithAvailablePoints(availablePoints, bankCap);
        }
        else
        {
            m_currentEnemyWeapon = newActive;
            newActive.StartTurnWithAvailablePoints(availablePoints, bankCap);
        }

        OnWeaponSwitched?.Invoke(newActive, oldActive, deductCost, isPlayer);

        if (isPlayer)
            OnTurnChanged?.Invoke(turnNumber, GetCurrentPlayerAvailablePoints());

        team.RemoveAt(newIndex);
        team.Insert(0, newActive);

        if (isPlayer)
            activePlayerWeaponIndex = 0;
        else
            activeEnemyWeaponIndex = 0;
    }



    private bool AnyAlive(List<Combatant> team) => team != null && team.Any(x => x.IsAlive);
    public int GetCurrentPlayerBasePoints() => playerBasePoints;
    public int GetCurrentEnemyBasePoints() => enemyBasePoints;

    public int GetCurrentPlayerAvailablePoints() => Mathf.Min(playerBasePoints + playerReservedPoints, bankCap);
    public int GetCurrentEnemyAvailablePoints() => Mathf.Min(enemyBasePoints + enemyReservedPoints, bankCap);


    public Combatant GetActivePlayerCombatant()
    {
        if (PlayerTeam == null || PlayerTeam.Count == 0) return null;
        activePlayerWeaponIndex = Mathf.Clamp(activePlayerWeaponIndex, 0, PlayerTeam.Count - 1);
        return PlayerTeam[activePlayerWeaponIndex];
    }

    public Combatant GetActiveEnemyCombatant()
    {
        if (EnemyTeam == null || EnemyTeam.Count == 0) return null;
        activeEnemyWeaponIndex = Mathf.Clamp(activeEnemyWeaponIndex, 0, EnemyTeam.Count - 1);
        return EnemyTeam[activeEnemyWeaponIndex];
    }

    public void EndBattleAndReturnToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
