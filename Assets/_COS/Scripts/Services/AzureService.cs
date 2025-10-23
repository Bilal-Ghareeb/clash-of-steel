using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayFab;
using PlayFab.CloudScriptModels;
using PlayFab.EconomyModels;
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

    public async Task ValidateAndGrantPurchaseAsync(string productId, string receiptJson, string signature)
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new ExecuteFunctionRequest
        {
            FunctionName = "ValidateAndGrantPurchase",
            FunctionParameter = new
            {
                productId,
                receiptJson,
                signature
            },
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            async result =>
            {
                await PlayFabManager.Instance.EconomyService.FetchAndCachePlayerInventoryAsync();
                PlayFabManager.Instance.EconomyService.NotifyCurrenciesUpdated();
                tcs.TrySetResult(true);
            },
            error =>
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }

    public async Task<CatalogItem> GrantLootBoxRewardAsync(LootBoxData lootBoxData)
    {
        var tcs = new TaskCompletionSource<CatalogItem>();

        var request = new ExecuteFunctionRequest
        {
            FunctionName = "GrantLootBoxReward",
            FunctionParameter = lootBoxData,
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            async result =>
            {
                try
                {
                    var jObj = JObject.Parse(result.FunctionResult.ToString());

                    var selectedRewardId = jObj["rewardId"]?.ToString();
                    if (string.IsNullOrEmpty(selectedRewardId))
                    {
                        tcs.TrySetException(new Exception("selectedRewardId missing from response."));
                        return;
                    }

                    var getItemResult = PlayFabManager.Instance.EconomyService.GetWeaponCatalogItemByFriendlyId(selectedRewardId);

                    await PlayFabManager.Instance.EconomyService.FetchAndCachePlayerInventoryAsync();
                    PlayFabManager.Instance.EconomyService.NotifyCurrenciesUpdated();

                    tcs.TrySetResult(getItemResult);
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

        return await tcs.Task;
    }

}
