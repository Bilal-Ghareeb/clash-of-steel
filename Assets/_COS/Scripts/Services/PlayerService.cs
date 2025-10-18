using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerService
{
    #region Fields
    private List<StageData> m_stages;
    private StageData m_currentStage;
    private int m_currentStageId = 1;
    #endregion

    #region Properties
    public string DisplayName { get; private set; }
    public int CurrentStageId => m_currentStageId;
    public StageData CurrentStage => m_currentStage;
    #endregion

    #region Events
    public event Action<StageData> OnStageChanged;
    #endregion

    public async Task CheckOrAssignDisplayNameAsync(string playFabId)
    {
        var tcs = new TaskCompletionSource<bool>();

        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest { PlayFabId = playFabId },
            result =>
            {
                var hasName = result?.PlayerProfile != null &&
                              !string.IsNullOrEmpty(result.PlayerProfile.DisplayName);

                if (hasName)
                {
                    Debug.Log($"Existing DisplayName: {result.PlayerProfile.DisplayName}");
                    tcs.TrySetResult(true);
                }
                else
                {
                    AssignNewDisplayName(() => tcs.TrySetResult(true));
                }
            },
            error =>
            {
                Debug.LogError("GetPlayerProfile failed: " + error.GenerateErrorReport());
                tcs.TrySetResult(true);
            });

        await tcs.Task;
    }

    private void AssignNewDisplayName(Action onComplete)
    {
        var randomName = "Gladiator" + UnityEngine.Random.Range(1000, 9999);
        PlayFabClientAPI.UpdateUserTitleDisplayName(
            new UpdateUserTitleDisplayNameRequest { DisplayName = randomName },
            result =>
            {
                Debug.Log($"Assigned new DisplayName: {randomName}");
                onComplete?.Invoke();
            },
            error =>
            {
                Debug.LogError("Failed to assign DisplayName: " + error.GenerateErrorReport());
                onComplete?.Invoke();
            });
    }

    public async Task FetchPlayerStageProgressAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.TryGetValue("CurrentStageId", out var stageData))
                {
                    if (int.TryParse(stageData.Value, out int stageId))
                        m_currentStageId = stageId;
                    else
                        m_currentStageId = 1;
                }
                else
                {
                    m_currentStageId = 1;
                }

                Debug.Log($"Fetched player current stage: {m_currentStageId}");
                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError("Failed to fetch player stage progress: " + error.GenerateErrorReport());
                m_currentStageId = 1;
                tcs.SetResult(false);
            });

        await tcs.Task;
    }

    public async Task SetCurrentStage()
    {
        m_stages = PlayFabManager.Instance.EconomyService.BattleStages.ToList();

        await FetchPlayerStageProgressAsync();

        int currentId = CurrentStageId;
        m_currentStage = m_stages.FirstOrDefault(s => s.id == currentId) ?? m_stages.FirstOrDefault();

        Debug.Log($"StageManager initialized with current stage: {m_currentStage?.name}");
        OnStageChanged?.Invoke(m_currentStage);
    }
}
