using System.Collections;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;

    public static Coroutine Start(IEnumerator routine)
    {
        if (_instance == null)
        {
            var obj = new GameObject("CoroutineRunner");
            GameObject.DontDestroyOnLoad(obj);
            _instance = obj.AddComponent<CoroutineRunner>();
        }
        return _instance.StartCoroutine(routine);
    }

    public static void Stop(Coroutine routine)
    {
        if (_instance != null && routine != null)
            _instance.StopCoroutine(routine);
    }
}
