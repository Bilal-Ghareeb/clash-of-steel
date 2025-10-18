using PlayFab;
using PlayFab.CloudScriptModels;
using System;
using System.Threading.Tasks;
using UnityEngine;


public class AzureService
{
    #region Events
    public static Action OnBattleStageRewardsClaimed;
    #endregion


    public async Task GrantStageRewardsAsync(int stageId, int gold)
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new ExecuteFunctionRequest
        {
            FunctionName = "GrantStageRewards",
            FunctionParameter = new { stageId, gold },
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            async result =>
            {
                await PlayFabManager.Instance.EconomyService.FetchAndCachePlayerInventoryAsync();
                await PlayFabManager.Instance.PlayerService.FetchPlayerStageProgressAsync();
                await PlayFabManager.Instance.PlayerService.SetCurrentStage();
                OnBattleStageRewardsClaimed?.Invoke();
                tcs.SetResult(true);
            },
            error =>
            {
                tcs.SetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }


    public async Task StartWeaponCooldownAsync(string weaponInstanceId, int level, string rarity, string progressionId)
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new ExecuteFunctionRequest
        {
            FunctionName = "StartWeaponCooldown",
            FunctionParameter = new
            {
                instanceId = weaponInstanceId,
                level,
                rarity,
                progressionId
            },
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            async result =>
            {
                await PlayFabManager.Instance.EconomyService.FetchAndCachePlayerInventoryAsync();
                tcs.SetResult(true);
            },
            error =>
            {
                tcs.SetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }


    public async Task LevelWeaponAsync(string weaponInstanceId, string currencyFriendlyId, int cost)
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new ExecuteFunctionRequest
        {
            FunctionName = "LevelWeapon",
            FunctionParameter = new
            {
                instanceId = weaponInstanceId,
                currencyFriendlyId,
                cost
            },
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            async result =>
            {
                await PlayFabManager.Instance.EconomyService.FetchAndCachePlayerInventoryAsync();
                tcs.SetResult(true);
            },
            error =>
            {
                tcs.SetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }



    public async Task GrantStarterBundleAsync(string weaponFriendlyId)
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new ExecuteFunctionRequest
        {
            FunctionName = "GrantStarterBundle",
            FunctionParameter = new { weaponId = weaponFriendlyId },
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            async result =>
            {
                try
                {
                    await PlayFabManager.Instance.EconomyService.FetchAndCachePlayerInventoryAsync();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            },
            error =>
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }
}
