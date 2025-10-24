using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreparingForBattleStageController : MonoBehaviour
{
    [SerializeField] private SoundData m_preparingForBattleEntranceSound;

    private void OnEnable()
    {
        PreparingForBattleStageEvents.ScreenEnabled += HandleScreenEnabled;
        PreparingForBattleStageEvents.RequestBeginBattle += HandleBeginBattle;
    }

    private void OnDisable()
    {
        PreparingForBattleStageEvents.ScreenEnabled -= HandleScreenEnabled;
        PreparingForBattleStageEvents.RequestBeginBattle -= HandleBeginBattle;
    }

    private void HandleScreenEnabled()
    {
        AudioManager.Instance.PlaySFX(m_preparingForBattleEntranceSound);
    }

    private void HandleBeginBattle(List<WeaponInstance> playerTeam, List<StageEnemyData> enemies)
    {
        var currentStage = PlayFabManager.Instance.PlayerService?.CurrentStage;
        if (currentStage == null)
        {
            Debug.LogError("Cannot start battle: Current stage not found.");
            return;
        }

        var sessionData = new BattleSession
        {
            stageId = currentStage.id,
            stageName = currentStage.name
        };

        foreach (var weapon in playerTeam)
        {
            if (weapon == null)
                continue;

            sessionData.playerTeam.Add(new BattleSession.CombatantDTO
            {
                friendlyId = weapon.CatalogData.name,
                instanceId = weapon.Item.Id,
                level = weapon.InstanceData.level,
                isPlayerOwned = true
            });
        }

        foreach (var enemy in enemies)
        {
            if (string.IsNullOrEmpty(enemy.weaponId))
            {
                Debug.LogWarning("Enemy weaponId is missing, skipping.");
                continue;
            }

            sessionData.enemyTeam.Add(new BattleSession.CombatantDTO
            {
                friendlyId = enemy.weaponId,
                level = enemy.level,
                isPlayerOwned = false
            });
        }

        BattleSessionHolder.CurrentSession = sessionData;

        SceneManager.LoadScene("BattleScene");
    }

}
