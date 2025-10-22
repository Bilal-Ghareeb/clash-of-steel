using Newtonsoft.Json;

[System.Serializable]
public class WeaponInstanceData
{
    public int level;
    public string cooldownEndTime;

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
        CooldownEndUtc.HasValue && PlayFabManager.Instance.TimeService.ServerUtcNow < CooldownEndUtc.Value;

    [JsonIgnore]
    public float RemainingCooldownSeconds =>
        IsOnCooldown ? (float)(CooldownEndUtc.Value - PlayFabManager.Instance.TimeService.ServerUtcNow).TotalSeconds : 0f;

}
