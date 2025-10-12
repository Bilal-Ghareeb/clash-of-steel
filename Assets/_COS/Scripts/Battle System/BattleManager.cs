using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main battle loop. This class has no UI code — only events to hook into.
/// Place on a GameObject in your Battle scene.
/// </summary>
public class BattleManager : MonoBehaviour
{
    // Config
    [SerializeField] private int maxPointsPerTurn = 4;
    [SerializeField] private int initialPlayerPoints = 1;
    [SerializeField] private int initialEnemyPoints = 2;
    [SerializeField] private int bankCap = 8;

    // Runtime
    public Combatant[] PlayerTeam { get; private set; }
    public Combatant[] EnemyTeam { get; private set; }

    private int playerBasePoints; // increases 1 each full half-round until cap = 4
    private int enemyBasePoints;
    private bool isPlayerTurn = true;

    // events
    public event Action OnBattleCountdownStarted;
    public event Action OnBattleStarted;
    public event Action OnBattleEnded;
    public event Action<Combatant , float> OnCombatantDamaged;
    public event Action<Combatant> OnCombatantDeath;
    public event Action<int> OnTurnChanged; // pass turn number or indicator

    private int turnNumber = 0;

    private async void Start()
    {
        // Read session
        var session = BattleSessionHolder.CurrentSession;
        if (session == null)
        {
            Debug.LogError("BattleManager: No BattleSession found. Abort.");
            return;
        }

        // initialize base points
        playerBasePoints = Mathf.Clamp(initialPlayerPoints, 1, maxPointsPerTurn);
        enemyBasePoints = Mathf.Clamp(initialEnemyPoints, 1, maxPointsPerTurn);

        // Build combatants from session
        BuildTeamsFromSession(session);

        OnBattleCountdownStarted?.Invoke();

        await Task.Delay(3500);

        OnBattleStarted?.Invoke();

        await RunBattleLoop();

        OnBattleEnded?.Invoke();

        // Optional: clear session
        BattleSessionHolder.CurrentSession = null;
    }

    private void BuildTeamsFromSession(BattleSession session)
    {
        PlayerTeam = session.playerTeam
            .Select(dto => CreateCombatantFromDTO(dto, true))
            .ToArray();

        EnemyTeam = session.enemyTeam
            .Select(dto => CreateCombatantFromDTO(dto, false))
            .ToArray();
    }

    private Combatant CreateCombatantFromDTO(BattleSession.CombatantDTO dto, bool isPlayer)
    {
        // Try to find a live player weapon instance if it is player-owned and exists in PlayFabManager cache.
        WeaponInstanceBase instance = null;

        if (isPlayer && !string.IsNullOrEmpty(dto.instanceId))
        {
            // If you want to map to the Player inventory instance: implement PlayFabManager.Instance.FindWeaponByInstanceId
            instance = PlayFabManager.Instance?.PlayerWeapons?.FirstOrDefault(w => w.Item?.Id == dto.instanceId) as WeaponInstance;
        }

        if (instance != null)
        {
            // Use the full WeaponInstance (has icon and instance data)
            return new Combatant(instance, dto.instanceId, instance.CatalogData.name);
        }
        else
        {
            // Otherwise create an EnemyWeaponInstance (wrap catalog WeaponData for level provided).
            // We must fetch the catalog WeaponData via PlayFabManager helper.
            var weaponData = PlayFabManager.Instance.GetWeaponDataByFriendlyId(dto.friendlyId);
            if (weaponData == null)
            {
                Debug.LogError($"No WeaponData for friendly id {dto.friendlyId}. Creating placeholder.");
                // Fallback: create a dummy WeaponData object with safe defaults (you can change as needed)
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
    }

    private async Task RunBattleLoop()
    {
        // Keep loop until one team dead
        while (AnyAlive(PlayerTeam) && AnyAlive(EnemyTeam))
        {
            turnNumber++;
            OnTurnChanged?.Invoke(turnNumber);

            Combatant[] currentSide = isPlayerTurn ? PlayerTeam : EnemyTeam;
            Combatant[] opposingSide = isPlayerTurn ? EnemyTeam : PlayerTeam;

            // Compute available points for this side this turn (base + banked)
            int basePoints = isPlayerTurn ? playerBasePoints : enemyBasePoints;

            // Start turn: add basePoints into each alive combatant bank
            foreach (var c in currentSide.Where(x => x.IsAlive))
                c.StartTurnWithAvailablePoints(basePoints, bankCap);

            // For a 2-vs-2 system you might only allow one active combatant attacking per turn.
            // Here we assume "side" acts as a group: we resolve each alive combatant in order.
            foreach (var actor in currentSide)
            {
                if (!actor.IsAlive) continue;

                // Player decision or AI decision
                if (isPlayerTurn)
                {
                    // Pause until UI gives allocations; UI must call back into BattleManager to set allocations.
                    // We expose a simple WaitForPlayerAllocation method (you'll implement UI to call AllocateForCombatant).
                    await WaitForPlayerAllocationAsync(actor);
                }
                else
                {
                    // Simple AI: allocate all banked points to attack
                    int pts = actor.BankedPoints;
                    actor.AllocateUnsafe(pts, 0, 0);
                }

                // Now execute attack phase for this actor on first alive opposing combatant
                var target = opposingSide.FirstOrDefault(t => t.IsAlive);
                if (target == null) break;

                if (actor.AttackPoints > 0)
                {
                    // Play single animation for actor's attack using TimelineController
                    if (TimelineController.Instance != null)
                        await TimelineController.Instance.PlayAttackAnimationAsync(actor, target);
                    // resolve damage
                    int damage = ActionResolver.ResolveDamage(actor, target);
                    if (damage > 0)
                    {
                        target.CurrentHP = Mathf.Max(0, target.CurrentHP - damage);
                        OnCombatantDamaged?.Invoke(target , target.CurrentHP);
                        if (!target.IsAlive) OnCombatantDeath?.Invoke(target);
                    }
                }
                // if actor had zero attack points we still allow defend to matter if opponent attacks later

                // After resolving actor, reset allocations (we keep bank as updated already)
                actor.ResetRoundAllocations();

                // Check battle end early
                if (!AnyAlive(opposingSide) || !AnyAlive(PlayerTeam) || !AnyAlive(EnemyTeam))
                    break;
            }

            // After side finished acting, flip turn and increase base points (capped at maxPointsPerTurn)
            isPlayerTurn = !isPlayerTurn;
            if (isPlayerTurn)
                playerBasePoints = Mathf.Min(playerBasePoints + 1, maxPointsPerTurn);
            else
                enemyBasePoints = Mathf.Min(enemyBasePoints + 1, maxPointsPerTurn);
        }

        // End: determine winner
        bool playerWon = AnyAlive(PlayerTeam) && !AnyAlive(EnemyTeam);
        Debug.Log(playerWon ? "Player wins battle" : "Enemy wins battle");
    }

    // You must implement how the player UI allocates points and calls AllocateForCombatant().
    // This helper awaits a TaskCompletionSource which your UI will complete.
    private TaskCompletionSource<bool> _waitingTcs;
    private Combatant _waitingActor;

    public Task WaitForPlayerAllocationAsync(Combatant actor)
    {
        _waitingActor = actor;
        _waitingTcs = new TaskCompletionSource<bool>();
        // UI should show controls for the specific actor and call BattleManager.Instance.AllocateForCombatant(...)
        return _waitingTcs.Task;
    }

    /// <summary>
    /// Called by UI when player finished choosing allocations for a specific actor.
    /// attack + defend + reserve must sum <= actor.BankedPoints.
    /// </summary>
    public bool AllocateForCombatant(Combatant actor, int attack, int defend, int reserve, out string error)
    {
        if (_waitingActor != actor)
        {
            error = "Actor not waiting for allocation.";
            return false;
        }

        if (!actor.TryAllocate(attack, defend, reserve, out error))
            return false;

        // done waiting
        _waitingTcs?.SetResult(true);
        _waitingTcs = null;
        _waitingActor = null;
        return true;
    }

    private bool AnyAlive(Combatant[] team) => team != null && team.Any(x => x.IsAlive);

    public void EndBattleAndReturnToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
