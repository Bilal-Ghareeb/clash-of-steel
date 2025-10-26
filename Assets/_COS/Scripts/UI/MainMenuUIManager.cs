using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuUIManager : MonoBehaviour
{
    private UIDocument m_MainMenuDocument;
    private Label m_loadingState;
    private VisualElement m_disconnectedPanel;
    private Button m_disconnectedButton;

    private void Awake()
    {
        m_MainMenuDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        m_loadingState = m_MainMenuDocument.rootVisualElement.Q<Label>("loading-state");
        UpdateLoadingLabel("ID_CONECTING");

        m_disconnectedPanel = m_MainMenuDocument.rootVisualElement.Q<VisualElement>("disconnected_panel");
        m_disconnectedPanel.style.display = DisplayStyle.None;

        m_disconnectedButton = m_MainMenuDocument.rootVisualElement.Q<Button>("reconnect__btn");

        RegisterButtonsCallBacks();
        SubscribeToEvents();
    }

    private void Start()
    {
        if (PlayFabManager.Instance != null &&
            PlayFabManager.Instance.NetworkService != null &&
            !PlayFabManager.Instance.NetworkService.IsConnected)
        {
            ShowDisconnectedPanel();
        }
    }

    private void OnDisable()
    {
        AuthService.OnAuthProgress -= UpdateLoadingLabel;
        if (PlayFabManager.Instance != null)
        {
            NetworkService.OnDisconnected -= ShowDisconnectedPanel;
            NetworkService.OnReconnected -= HideDisconnectedPanel;
        }
    }

    private void RegisterButtonsCallBacks()
    {
        m_disconnectedButton.RegisterCallback<ClickEvent>(RetryConnection);
    }

    private void SubscribeToEvents()
    {
        AuthService.OnAuthProgress += UpdateLoadingLabel;

        NetworkService.OnDisconnected += ShowDisconnectedPanel;
        NetworkService.OnReconnected += HideDisconnectedPanel;
    }

    private void RetryConnection(ClickEvent evt)
    {
        HideDisconnectedPanel();
        UpdateLoadingLabel("ID_RECONNECTING");

        PlayFabManager.Instance.RetryConnection();
    }


    private async void UpdateLoadingLabel(string message)
    {
        if (m_loadingState != null)
            m_loadingState.text = await LocalizationManager.GetLocalizedLabel(message , "COS_Strings");
    }

    private void ShowDisconnectedPanel()
    {
        m_disconnectedPanel.style.display = DisplayStyle.Flex;
        m_disconnectedPanel.transform.scale = new Vector3(0.1f, 0.1f, 0.1f);
        m_disconnectedPanel.experimental.animation.Scale(1f, 200);
    }

    private void HideDisconnectedPanel()
    {
        m_disconnectedPanel.style.display = DisplayStyle.None;
    }
}
