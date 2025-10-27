using UnityEngine;
using UnityEngine.UIElements;

public class UIAudioBinder : MonoBehaviour
{
    [Header("UI Document to scan")]
    [SerializeField] private UIDocument m_uiDocument;

    [Header("Default click sound")]
    [SerializeField] private SoundData m_defaultClickSound;

    private void OnEnable()
    {
        if (m_uiDocument == null)
            m_uiDocument = GetComponent<UIDocument>();

        if (m_uiDocument == null || m_uiDocument.rootVisualElement == null)
        {
            Debug.LogWarning("UIAudioBinder: No UIDocument found!");
            return;
        }

        BindButtonSounds(m_uiDocument.rootVisualElement);
    }

    private void BindButtonSounds(VisualElement root)
    {
        var buttons = root.Query<Button>().ToList();

        foreach (var button in buttons)
        {
            button.RegisterCallback<ClickEvent>(evt =>
            {
                SoundData sound = null;

                sound = m_defaultClickSound;

                if (sound != null)
                    AudioManager.Instance.PlaySFX(sound);
            });
        }
    }
}
