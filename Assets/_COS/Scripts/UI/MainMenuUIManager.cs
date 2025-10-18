using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuUIManager : MonoBehaviour
{
    private UIDocument m_MainMenuDocument;
    private Label m_loadingState;

    private void OnEnable()
    {
        m_MainMenuDocument = GetComponent<UIDocument>();
        m_loadingState = m_MainMenuDocument.rootVisualElement.Q<Label>("loading-state");

        AuthService.OnAuthProgress += UpdateLoadingLabel;
    }

    private void OnDisable()
    {
        AuthService.OnAuthProgress -= UpdateLoadingLabel;
    }

    private void UpdateLoadingLabel(string message)
    {
        if (m_loadingState != null)
            m_loadingState.text = message;
    }
}
