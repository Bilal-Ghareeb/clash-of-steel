using UnityEngine;

public class SceneAmbienceController : MonoBehaviour
{
    [Header("Ambience for this scene")]
    [SerializeField] private SoundData[] ambienceTracks;
    [SerializeField] private bool stopPreviousAmbience = true;

    private void Start()
    {
        foreach (var track in ambienceTracks)
        {
            AudioManager.Instance.PlayLoop(track, AudioManager.Instance.Ambience);
        }
    }

    private void OnDisable()
    {
        AudioManager.Instance.StopAllAmbience();
    }
}
