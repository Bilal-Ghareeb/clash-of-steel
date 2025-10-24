using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sound Data", fileName = "NewSoundData")]
public class SoundData : ScriptableObject
{
    public string ID;
    public AudioClip Clip;
    [Range(0f, 1f)] public float Volume = 1f;
    [Range(-3f, 3f)] public float Pitch = 1f;
    public bool IsLoop;
}
