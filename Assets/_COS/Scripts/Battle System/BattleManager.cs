using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// BattleManager (refactored)
/// - Allocation phase (player & enemy choose attack/defend/reserve)
/// - Reveal phase (public allocations shown: attack & defend only)
/// - Execution phase (animations + damage resolution)
/// This is designed for: 1 active player combatant per side, pooled points per turn,
/// animations play once while damage counts multiple times.
/// </summary>
public class BattleManager : MonoBehaviour
{
    [SerializeField] private int maxPointsPerTurn = 4;
    [SerializeField] private int initialPlayerBasePoints = 1;
    [SerializeField] private int initialEnemyBasePoints = 2;
    [SerializeField] private int bankCap = 8;

    public List<Combatant> PlayerTeam { get; private set; }
    public List<Combatant> EnemyTeam { get; private set; }

    private Combatant m_currentPlayerWeapon;

    private int playerBasePoints;
    private int playerReservedPoints;
    private int enemyBasePoints;
    private int enemyReservedPoints;
    private int activePlayerWeaponIndex = 0;

    public event Action OnBattleCountdownStarted;
    public event Action OnBattleStarted;
    public event Action OnBattleEnded;

    public event Action<int> OnPlayerAllocationPhaseStarted;
    public event Action<int> OnEnemyAllocationPhaseStarted;

    public event Action<(int attack, int defend) /* playerPublic */, (int attack, int defend) /* enemyPublic */> OnAllocationsRevealed;

    public event Action OnPlayerTurnStarted;
    public event Action OnEnemyTurnStarted;

    public event Action<Combatant, float> OnCombatantDamaged;
    public event Action<Combatant> OnCombatantDeath;

    public event Action<int, int> OnTurnChanged;

    public event Action<Combatant, Combatant> OnPlayerWeaponSwitched;


    private int turnNumber = 0;

    // Internal: when waiting for player's submission
    private TaskCompletionSource<(int attack, int defend, int reserve)> _playerAllocationTcs;
    private (int attack, int defend, int reserve) _lastEnemyAllocation;

    [Header("Timing Settings (ms)")]
    [SerializeField] private int turnStartDelayMs = 1000;
    [SerializeField] private int revealDelayMs = 2000;    
    [SerializeField] private int resultDelayMs = 1500; 
    [SerializeField] private int countdownDelayMs = 3500;  


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
        BattleSessionHolder.CurrentSession = null;
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
            instance = PlayFabManager.Instance?.PlayerWeapons?.FirstOrDefault(w => w.Item?.Id == dto.instanceId) as WeaponInstance;
        }

        if (instance != null)
            return new Combatant(instance, dto.instanceId, instance.CatalogData.name);

        var weaponData = PlayFabManager.Instance.GetWeaponDataByFriendlyId(dto.friendlyId);
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
            Combatant enemy = EnemyTeam.FirstOrDefault(e => e.IsAlive);

            if (m_currentPlayerWeapon != null && m_currentPlayerWeapon.IsAlive)
            {
                m_currentPlayerWeapon.ResetRoundAllocations();
                m_currentPlayerWeapon.StartTurnWithAvailablePoints(availablePlayerPoints, bankCap);

                // Allow player allocation
                OnPlayerAllocationPhaseStarted?.Invoke(availablePlayerPoints);
                var playerAlloc = await WaitForPlayerAllocationAsync(availablePlayerPoints);

                // Apply reserve logic
                playerReservedPoints = playerAlloc.reserve;
                m_currentPlayerWeapon.TryAllocate(playerAlloc.attack, playerAlloc.defend, playerAlloc.reserve, out _);

                // Show only player's attack vs enemy's defend
                var playerReveal = (m_currentPlayerWeapon.AttackPoints, enemy?.DefendPoints ?? 0);
                OnAllocationsRevealed?.Invoke(playerReveal, (0, 0));
                await Task.Delay(3000);

                // Execute player's attack
                if (enemy != null && m_currentPlayerWeapon.AttackPoints > 0)
                {
                    if (TimelineController.Instance != null)
                        await TimelineController.Instance.PlayAttackAnimationAsync(m_currentPlayerWeapon, enemy);

                    int damage = ActionResolver.ResolveDamage(m_currentPlayerWeapon, enemy);
                    enemy.CurrentHP = Mathf.Max(0, enemy.CurrentHP - damage);
                    OnCombatantDamaged?.Invoke(enemy, enemy.CurrentHP);
                    if (!enemy.IsAlive) OnCombatantDeath?.Invoke(enemy);
                }

                await Task.Delay(800);
            }

            // --------------------------- ENEMY TURN ---------------------------
            if (!AnyAlive(EnemyTeam) || !AnyAlive(PlayerTeam)) break;

            OnEnemyTurnStarted?.Invoke();
            await Task.Delay(1200);

            Combatant actingEnemy = EnemyTeam.FirstOrDefault(e => e.IsAlive);
            Combatant defendingPlayer = GetActivePlayerCombatant();

            if (actingEnemy != null && defendingPlayer != null)
            {
                int enemyAvailablePoints = Mathf.Min(enemyBasePoints + enemyReservedPoints, bankCap);
                actingEnemy.StartTurnWithAvailablePoints(enemyAvailablePoints, bankCap);

                // Enemy simple AI (example: attack everything)
                int attack = enemyAvailablePoints;
                int defend = 0;
                int reserve = 0;
                actingEnemy.TryAllocate(attack, defend, reserve, out _);
                enemyReservedPoints = reserve;

                // Reveal enemy attack vs player defend (from previous turn)
                var enemyReveal = (actingEnemy.AttackPoints, defendingPlayer.DefendPoints);
                OnAllocationsRevealed?.Invoke((0, 0), enemyReveal);
                await Task.Delay(3000);

                // Execute enemy attack
                if (actingEnemy.AttackPoints > 0)
                {
                    if (TimelineController.Instance != null)
                        await TimelineController.Instance.PlayAttackAnimationAsync(actingEnemy, defendingPlayer);

                    int damage = ActionResolver.ResolveDamage(actingEnemy, defendingPlayer);
                    defendingPlayer.CurrentHP = Mathf.Max(0, defendingPlayer.CurrentHP - damage);
                    OnCombatantDamaged?.Invoke(defendingPlayer, defendingPlayer.CurrentHP);
                    if (!defendingPlayer.IsAlive) OnCombatantDeath?.Invoke(defendingPlayer);
                }

                await Task.Delay(800);
            }

            // --------------------------- END OF ROUND ---------------------------

            // Increment base points (like in JW)
            playerBasePoints = Mathf.Min(playerBasePoints + 1, maxPointsPerTurn);
            enemyBasePoints = Mathf.Min(enemyBasePoints + 1, maxPointsPerTurn);

            await Task.Delay(1000);
        }

        bool playerWon = AnyAlive(PlayerTeam) && !AnyAlive(EnemyTeam);
        Debug.Log(playerWon ? "Player wins battle" : "Enemy wins battle");
    }


    // Wait for the UI to call AllocateForCombatant. This returns the submitted allocation.
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
    public bool SwitchActivePlayerWeapon(int newIndex, int switchCost, out string error)
    {
        error = null;
        if (PlayerTeam == null || newIndex < 0 || newIndex >= PlayerTeam.Count)
        {
            error = "Invalid index";
            return false;
        }
        if (newIndex == activePlayerWeaponIndex)
        {
            error = "Already active";
            return false;
        }

        var oldActive = PlayerTeam[activePlayerWeaponIndex];
        var newActive = PlayerTeam[newIndex];

        if (newActive == null || !newActive.IsAlive)
        {
            error = "Target weapon is not available/alive.";
            return false;
        }

        int availablePoints = GetCurrentPlayerAvailablePoints();
        if (availablePoints < switchCost)
        {
            error = "Not enough points to switch.";
            return false;
        }

        PlayerTeam.RemoveAt(newIndex);
        PlayerTeam.Insert(0, newActive);

        if(GetCurrentPlayerAvailablePoints() > 0)
        {
            m_currentPlayerWeapon = newActive;
            newActive.StartTurnWithAvailablePoints(GetCurrentPlayerAvailablePoints() , bankCap);
        }

        OnPlayerWeaponSwitched?.Invoke(newActive, oldActive);
        OnTurnChanged?.Invoke(turnNumber, GetCurrentPlayerAvailablePoints());
        return true;
    }

    private bool AnyAlive(List<Combatant> team) => team != null && team.Any(x => x.IsAlive);
    public int GetCurrentPlayerBasePoints() => playerBasePoints;
    public int GetCurrentPlayerAvailablePoints() => Mathf.Min(playerBasePoints + playerReservedPoints, bankCap);


    public Combatant GetActivePlayerCombatant()
    {
        if (PlayerTeam == null || PlayerTeam.Count == 0) return null;
        activePlayerWeaponIndex = Mathf.Clamp(activePlayerWeaponIndex, 0, PlayerTeam.Count - 1);
        return PlayerTeam[activePlayerWeaponIndex];
    }

    public void EndBattleAndReturnToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
