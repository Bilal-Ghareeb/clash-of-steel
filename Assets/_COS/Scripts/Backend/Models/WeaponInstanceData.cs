using Newtonsoft.Json;

[System.Serializable]
public class WeaponInstanceData
{
    public int level;
    public string cooldownEndTime; // stored as ISO 8601 string, e.g. "2025-10-16T18:00:00Z"

    [JsonIgnore]
    public System.DateTime? CooldownEndUtc
    {
        get
        {
            if (string.IsNullOrEmpty(cooldownEndTime)) return null;

            if (System.DateTimeOffset.TryParse(cooldownEndTime, out var parsedOffset))
                return parsedOffset.UtcDateTime;

            return null;
        }
    }


    [JsonIgnore]
    public bool IsOnCooldown =>
        CooldownEndUtc.HasValue && PlayFabManager.ServerUtcNow < CooldownEndUtc.Value;

    [JsonIgnore]
    public float RemainingCooldownSeconds =>
        IsOnCooldown ? (float)(CooldownEndUtc.Value - PlayFabManager.ServerUtcNow).TotalSeconds : 0f;

}
