using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundCategory
{
    public string Name;
    [Range(0f, 1f)] public float Volume = 1f;
    public bool Muted = false;

    private List<AudioSource> m_sources = new List<AudioSource>();

    public void RegisterSource(AudioSource src)
    {
        if (!m_sources.Contains(src))
            m_sources.Add(src);
        ApplySettings(src);
    }

    public void UnRegisterSource(AudioSource src)
    {
        if (m_sources.Contains(src))
            m_sources.Remove(src);
        ApplySettings(src);
    }

    public void ApplySettings(AudioSource src)
    {
        src.mute = Muted;
        src.volume = Volume;
    }

    public void UpdateAll()
    {
        foreach (var src in m_sources)
            ApplySettings(src);
    }
}
