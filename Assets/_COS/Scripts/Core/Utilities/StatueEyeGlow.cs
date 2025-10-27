using UnityEngine;
using System.Collections;

public class StatueEyeGlow : MonoBehaviour
{
    [SerializeField] private Light m_eyeLight;
    [SerializeField] private float m_targetIntensity = 5f;
    [SerializeField] private float m_duration = 1f;

    private float m_baseIntensity;

    private void Awake()
    {
        if (m_eyeLight != null)
            m_baseIntensity = m_eyeLight.intensity;
    }

    [ContextMenu("Trigger Glow")]
    public void TriggerGlow()
    {
        StopAllCoroutines();
        StartCoroutine(GlowRoutine());
    }


    private IEnumerator GlowRoutine()
    {
        float halfDuration = m_duration / 2f;

        float t = 0;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            m_eyeLight.intensity = Mathf.Lerp(m_baseIntensity, m_targetIntensity, t / halfDuration);
            yield return null;
        }

        t = 0;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            m_eyeLight.intensity = Mathf.Lerp(m_targetIntensity, m_baseIntensity, t / halfDuration);
            yield return null;
        }
    }
}
