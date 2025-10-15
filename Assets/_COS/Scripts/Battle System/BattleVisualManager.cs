using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BattleVisualManager : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnPoint;

    private GameObject playerInstance;
    private GameObject enemyInstance;

    private BattleManager m_Battle;

    private void Start()
    {
        m_Battle = FindAnyObjectByType<BattleManager>();
        if (m_Battle == null)
        {
            return;
        }
        m_Battle.OnBattleStarted += () => SpawnInitialModels(m_Battle);
        m_Battle.OnWeaponSwitched += OnWeaponSwitched;
        m_Battle.OnCombatantDeath += OnCombatantDeath;
    }

    private void OnDestroy()
    {
        if (m_Battle != null)
        {
            m_Battle.OnBattleStarted -= () => SpawnInitialModels(m_Battle);
            m_Battle.OnWeaponSwitched -= OnWeaponSwitched;
            m_Battle.OnCombatantDeath -= OnCombatantDeath;
        }
    }

    private void SpawnInitialModels(BattleManager battle)
    {
        var player = battle.GetActivePlayerCombatant();
        var enemy = battle.GetActiveEnemyCombatant();

        if (player != null && player.InstanceData?.Asset?.WeaponPrefab != null)
        {
            SpawnModelForWeapon(player);
        }
        if (enemy != null && enemy.InstanceData?.Asset?.WeaponPrefab != null)
        {
            SpawnModelForEnemy(enemy);
        }
    }

    private void OnWeaponSwitched(Combatant newActive, Combatant oldActive, bool deductCost , bool isPlayer)
    {
        if (isPlayer)
        {
            if (playerInstance != null)
            {
                Destroy(playerInstance);
                playerInstance = null;
            }
            SpawnModelForWeapon(newActive);
        }
        else
        {
            if (enemyInstance != null)
            {
                Destroy(enemyInstance);
                enemyInstance = null;
            }
            SpawnModelForEnemy(newActive);
        }
    }

    private void OnCombatantDeath(Combatant deadCombatant)
    {
        if (m_Battle.GetActivePlayerCombatant() == deadCombatant)
        {
            if (playerInstance != null)
            {
                Destroy(playerInstance);
                playerInstance = null;
            }
            return;
        }

        if (m_Battle.EnemyTeam != null && m_Battle.EnemyTeam.Contains(deadCombatant))
        {
            if (enemyInstance != null)
            {
                Destroy(enemyInstance);
                enemyInstance = null;
            }
        }
    }

    private void SpawnModelForWeapon(Combatant combatant)
    {
        var prefab = combatant.InstanceData.Asset.WeaponPrefab;
        if (prefab == null) return;
        var instance = Instantiate(prefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        playerInstance = instance;

        combatant.ModelRoot = playerInstance.transform;
        combatant.CombatantAnimator = playerInstance.transform.GetChild(0).GetComponent<Animator>();
    }

    private void SpawnModelForEnemy(Combatant combatant)
    {
        var prefab = combatant.InstanceData.Asset.WeaponPrefab;
        if (prefab == null) return;
        enemyInstance = Instantiate(prefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
        combatant.ModelRoot = enemyInstance.transform;
        combatant.CombatantAnimator = enemyInstance.transform.GetChild(0).GetComponent<Animator>();
    }
}
