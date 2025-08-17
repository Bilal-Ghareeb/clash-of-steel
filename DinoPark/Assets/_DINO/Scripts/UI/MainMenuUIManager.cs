using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuUIManager : MonoBehaviour
{
    private UIDocument m_MainMenuDocument;
    private VisualElement m_LoginWithGoogleButton;

    public string customPlayerID = "myUniquePlayerID_123";

    private void OnEnable()
    {
        m_MainMenuDocument = GetComponent<UIDocument>();

        SetupLoginButton();
        RegisterButtonCallbacks();
    }

    private void SetupLoginButton()
    {
        VisualElement root = m_MainMenuDocument.rootVisualElement;
        m_LoginWithGoogleButton = root.Q("LoginWithGoogle_btn");
    }

    protected void RegisterButtonCallbacks()
    {
        m_LoginWithGoogleButton.RegisterCallback<ClickEvent>(ClickLoginWithGoogleButton);
    }

    private void ClickLoginWithGoogleButton(ClickEvent evt)
    {
       //PlayFabManager.Instance.LoginWithCustomID(customPlayerID);
    }

    public void OnDisable()
    {
        m_LoginWithGoogleButton.UnregisterCallback<ClickEvent>(ClickLoginWithGoogleButton);
    }
}
