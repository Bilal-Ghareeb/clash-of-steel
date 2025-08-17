//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class PlayFabManager : MonoBehaviour
//{
//    private static PlayFabManager m_instance;
//    public static PlayFabManager Instance {  get { return m_instance; } }

//    private void Awake()
//    {
//        if (m_instance != null && m_instance != this)
//        {
//            Destroy(gameObject);
//        }
//        else
//        {
//            m_instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//    }

//    public void LoginWithCustomID(string customID)
//    {
//        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
//        {
//            PlayFabSettings.staticSettings.TitleId = "161E93";
//        }

//        var request = new LoginWithCustomIDRequest
//        {
//            CustomId = SystemInfo.deviceUniqueIdentifier,
//            CreateAccount = true
//        };

//        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
//    }

//    private void OnLoginSuccess(LoginResult result)
//    {
//        Debug.Log("Successfully logged in with Custom ID: " + result.PlayFabId);
//        SceneManager.LoadScene(1);
//    }

//    private void OnLoginFailure(PlayFabError error)
//    {
//        Debug.LogError("Login failed: " + error.GenerateErrorReport());
//    }

//}
