using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Threading.Tasks;

public class TimeService
{
    #region Fields
    private double m_serverOffsetSeconds;
    private bool m_isSynced;
    #endregion

    #region Properties
    public DateTime ServerUtcNow => m_isSynced
        ? DateTime.UtcNow.AddSeconds(m_serverOffsetSeconds)
        : DateTime.UtcNow;
    #endregion


    public async Task SyncServerTimeAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        PlayFabClientAPI.GetTime(new GetTimeRequest(),
            result =>
            {
                var serverTime = result.Time.ToUniversalTime();
                m_serverOffsetSeconds = (serverTime - DateTime.UtcNow).TotalSeconds;
                m_isSynced = true;
                tcs.TrySetResult(true);
            },
            error =>
            {
                m_isSynced = false;
                tcs.TrySetResult(false);
            });

        await tcs.Task;
    }
}
