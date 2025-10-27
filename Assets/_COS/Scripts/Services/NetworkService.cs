using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkService
{
    #region Properties
    public bool IsConnected { get; private set; } = true;
    #endregion

    #region Events
    public static event Action OnDisconnected;
    public static event Action OnReconnected;
    #endregion

    #region Fields
    private Coroutine m_monitorRoutine;
    #endregion

    public void StartMonitoring(MonoBehaviour context, Action<bool> onFirstCheck)
    {
        m_monitorRoutine = context.StartCoroutine(MonitorRoutine(onFirstCheck));
    }

    private IEnumerator MonitorRoutine(Action<bool> onFirstCheck)
    {
        yield return CheckInternetConnection(onFirstCheck);

        while (true)
        {
            yield return new WaitForSeconds(5f);
            yield return CheckInternetConnection((connected) =>
            {
                if (!connected)
                {
                    IsConnected = false;
                    OnDisconnected?.Invoke();
                }
                else if (!IsConnected)
                {
                    IsConnected = true;
                    OnReconnected?.Invoke();
                }
            });
        }
    }

    private IEnumerator CheckInternetConnection(Action<bool> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Head("https://clients3.google.com/generate_204"))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            bool connected = request.result == UnityWebRequest.Result.Success;
            callback?.Invoke(connected);
        }
    }


    public void StopMonitoring(MonoBehaviour host)
    {
        if (m_monitorRoutine != null)
            host.StopCoroutine(m_monitorRoutine);
    }
}
