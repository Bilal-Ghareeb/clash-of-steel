using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class UniqueDisplayNameGenerator : MonoBehaviour
{
    private const int MaxRetries = 5;
    private const string NamePrefix = "ParkOwner";

    public void GenerateAndSetDisplayName(Action<string> onSuccess, Action<string> onFailure)
    {
        TrySetName(0, onSuccess, onFailure);
    }

    private void TrySetName(int attempt, Action<string> onSuccess, Action<string> onFailure)
    {
        if (attempt >= MaxRetries)
        {
            onFailure?.Invoke("Failed to generate unique display name after " + MaxRetries + " attempts.");
            return;
        }

        string newName = GenerateRandomName();

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result =>
            {
                Debug.Log("Unique name set: " + result.DisplayName);
                onSuccess?.Invoke(result.DisplayName);
            },
            error =>
            {
                if (error.Error == PlayFabErrorCode.NameNotAvailable)
                {
                    Debug.LogWarning("Name already taken, retrying... (" + newName + ")");
                    TrySetName(attempt + 1, onSuccess, onFailure);
                }
                else
                {
                    Debug.LogError("Failed to update name: " + error.GenerateErrorReport());
                    onFailure?.Invoke(error.ErrorMessage);
                }
            });
    }

    private string GenerateRandomName()
    {
        return NamePrefix + UnityEngine.Random.Range(1000, 99999);
    }
}
