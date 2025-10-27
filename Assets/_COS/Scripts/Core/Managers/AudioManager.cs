using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Categories")]
    public SoundCategory Master;
    public SoundCategory Music;
    public SoundCategory SFX;
    public SoundCategory Ambience;

    private Dictionary<string, AudioSource> activeLoops = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySFX(SoundData data, Vector3? position = null)
    {
        if (SFX.Muted || data == null || data.Clip == null) return;

        AudioSource source = new GameObject($"SFX_{data.ID}").AddComponent<AudioSource>();
        source.transform.position = position ?? Vector3.zero;
        source.clip = data.Clip;
        source.volume = data.Volume * SFX.Volume * Master.Volume;
        source.pitch = data.Pitch;
        source.Play();
        Destroy(source.gameObject, data.Clip.length / Mathf.Abs(data.Pitch));
    }

    public void PlayLoop(SoundData data, SoundCategory category)
    {
        if (data == null || data.Clip == null) return;
        if (activeLoops.ContainsKey(data.ID)) return;

        AudioSource src = new GameObject($"{category.Name}_{data.ID}").AddComponent<AudioSource>();
        src.clip = data.Clip;
        src.loop = true;
        src.volume = data.Volume * category.Volume * Master.Volume;
        src.Play();
        category.RegisterSource(src);
        activeLoops[data.ID] = src;
    }

    public void StopLoop(string id)
    {
        if (activeLoops.TryGetValue(id, out var src))
        {
            Destroy(src.gameObject);
            activeLoops.Remove(id);
        }
    }

    public void StopAllAmbience()
    {
        var keysToRemove = new List<string>();
        foreach (var kvp in activeLoops)
        {
            if (kvp.Value != null && kvp.Value.gameObject.name.StartsWith("Ambience"))
            {
                Destroy(kvp.Value.gameObject);
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
            activeLoops.Remove(key);
    }

}
