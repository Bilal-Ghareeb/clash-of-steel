using UnityEngine;
using System.Linq;

public class BattleVisualManager : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnPoint;

    private void Start()
    {
        var battle = FindAnyObjectByType<BattleManager>();

        if (battle == null)
        {
            return;
        }

        battle.OnBattleStarted += () => SpawnInitialModels(battle);
    }

    private void SpawnInitialModels(BattleManager battle)
    {
        var player = battle.PlayerTeam?.FirstOrDefault();
        var enemy = battle.EnemyTeam?.FirstOrDefault();

        if (player != null && player.InstanceData?.Asset?.WeaponPrefab != null)
        {
            SpawnModel(player, playerSpawnPoint);
        }

        if (enemy != null && enemy.InstanceData?.Asset?.WeaponPrefab != null)
        {
            SpawnModel(enemy, enemySpawnPoint);
        }
    }

    private void SpawnModel(Combatant combatant, Transform spawnPoint)
    {
        var prefab = combatant.InstanceData.Asset.WeaponPrefab;
        if (prefab == null)
        {
            return;
        }

        var obj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}
