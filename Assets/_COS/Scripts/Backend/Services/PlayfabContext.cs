using PlayFab.EconomyModels;

public class PlayFabContext
{
    #region Properties
    public string PlayFabId { get; set; }
    public string EntityToken { get; set; }
    public EntityKey EntityKey { get; set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(EntityToken);
    #endregion

    public void SetEntityData(string playFabID , EntityKey entityKey)
    {
        EntityKey = entityKey;
        PlayFabId = playFabID;
    }
}
