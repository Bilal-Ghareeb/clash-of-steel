using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreparingForBattleStageController : MonoBehaviour
{
    [SerializeField] private SoundData m_preparingForBattleEntranceSound;

    private PreparingForBattleStageView m_view;

    private WeaponInstance[] m_playerSelectedWeapons = new WeaponInstance[2];

    public void Setup(PreparingForBattleStageView view)
    {
        m_view = view;
    }

    private void OnEnable()
    {
        PreparingForBattleStageEvents.ScreenEnabled += HandleScreenEnabled;
        PreparingForBattleStageEvents.RequestBeginBattle += HandleBeginBattle;
        PreparingForBattleStageEvents.TeamSlotClicked += HandleTeamSlotClicked;
        PreparingForBattleStageEvents.ArsenalWeaponClicked += HandleArsenalWeaponClicked;
        PreparingForBattleStageEvents.RequestFetchPlayerArsenal += HandleFetchPlayerArsenal;
        PreparingForBattleStageEvents.RequestStageInfo += HandleUpdateStageInfo;
        PreparingForBattleStageEvents.ClearContainers += HandleClearContainers;
    }

    private void OnDisable()
    {
        PreparingForBattleStageEvents.ScreenEnabled -= HandleScreenEnabled;
        PreparingForBattleStageEvents.RequestBeginBattle -= HandleBeginBattle;
        PreparingForBattleStageEvents.TeamSlotClicked -= HandleTeamSlotClicked;
        PreparingForBattleStageEvents.ArsenalWeaponClicked -= HandleArsenalWeaponClicked;
        PreparingForBattleStageEvents.RequestFetchPlayerArsenal -= HandleFetchPlayerArsenal;
        PreparingForBattleStageEvents.RequestStageInfo -= HandleUpdateStageInfo;
        PreparingForBattleStageEvents.ClearContainers -= HandleClearContainers;
    }

    private void HandleScreenEnabled()
    {
        AudioManager.Instance.PlaySFX(m_preparingForBattleEntranceSound);
    }

    private async void HandleBeginBattle()
    {
        var playerTeam = m_playerSelectedWeapons.Where(w => w != null).ToList();
        if (playerTeam.Count == 0)
        {
            Debug.LogWarning("No weapons selected for battle.");
            return;
        }

        var enemies = PlayFabManager.Instance.PlayerService?.CurrentStage?.enemies;
        if (enemies == null)
        {
            Debug.LogError("No enemies found for the current stage.");
            return;
        }

        foreach (var weapon in playerTeam)
        {
            await PlayFabManager.Instance.AzureService.StartWeaponCooldownAsync(
                weapon.Item.Id,
                weapon.Level,
                weapon.CatalogData.rarity,
                weapon.CatalogData.progressionId
            );
        }

        var sessionData = new BattleSession
        {
            stageId = PlayFabManager.Instance.PlayerService.CurrentStage.id,
            stageName = PlayFabManager.Instance.PlayerService.CurrentStage.name
        };

        foreach (var weapon in playerTeam)
        {
            if (weapon == null) continue;
            sessionData.playerTeam.Add(new BattleSession.CombatantDTO
            {
                friendlyId = weapon.CatalogData.GetLocalizedName(),
                instanceId = weapon.Item.Id,
                level = weapon.InstanceData.level,
                isPlayerOwned = true
            });
        }

        foreach (var enemy in enemies)
        {
            if (string.IsNullOrEmpty(enemy.weaponId)) continue;
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

    private void HandleTeamSlotClicked(int slotIndex)
    {
        if (m_playerSelectedWeapons[slotIndex] != null)
        {
            m_view.UpdateWeaponSelection(m_playerSelectedWeapons[slotIndex], false);
            m_playerSelectedWeapons[slotIndex] = null;
            m_view.UpdateTeamSlot(slotIndex, null);
        }
    }

    private void HandleArsenalWeaponClicked(WeaponInstance weapon)
    {
        for (int i = 0; i < m_playerSelectedWeapons.Length; i++)
        {
            if (m_playerSelectedWeapons[i] == null)
            {
                m_view.UpdateWeaponSelection(weapon, true);
                m_playerSelectedWeapons[i] = weapon;
                m_view.UpdateTeamSlot(i, weapon);
                break;
            }
        }
    }

    private void HandleFetchPlayerArsenal()
    {
        var playerWeapons = PlayFabManager.Instance.EconomyService.PlayerWeapons;
        if (playerWeapons == null || playerWeapons.Count == 0)
        {
            Debug.Log("Player has no weapons in arsenal.");
            return;
        }

        m_view.UpdatePlayerArsenal(playerWeapons);

        var currentStage = PlayFabManager.Instance.PlayerService?.CurrentStage;
        if (currentStage == null)
        {
            Debug.LogError("Cannot spawn enemies: current stage not found.");
            return;
        }

        var enemies = new List<EnemyWeaponInstance>();
        foreach (var enemy in currentStage.enemies)
        {
            var weaponData = PlayFabManager.Instance.EconomyService.GetWeaponDataByFriendlyId(enemy.weaponId);
            if (weaponData == null)
            {
                Debug.LogWarning($"No WeaponData found for friendly ID: {enemy.weaponId}");
                continue;
            }
            enemies.Add(new EnemyWeaponInstance(enemy.weaponId, weaponData, enemy.level));
        }
        m_view.UpdateEnemyTeam(enemies);
    }

    private void HandleUpdateStageInfo()
    {
        var currentStage = PlayFabManager.Instance.PlayerService?.CurrentStage;
        m_view.UpdateStageInfo(currentStage.id);
    }

    private void HandleClearContainers()
    {
        m_view.ClearContainers();
    }
}