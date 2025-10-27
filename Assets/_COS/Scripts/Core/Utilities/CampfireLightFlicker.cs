using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class CampfireLightFlicker : MonoBehaviour
{
    [Header("Intensity Flicker")]
    [SerializeField] private float baseIntensity = 2f;
    [SerializeField] private float flickerRange = 0.5f;
    [SerializeField] private float flickerSpeed = 3f;

    [Header("Color Flicker (optional)")]
    [SerializeField] private bool enableColorFlicker = true;
    [SerializeField] private Color warmColor = new Color(1f, 0.6f, 0.3f);
    [SerializeField] private Color hotColor = new Color(1f, 0.8f, 0.5f);

    [Header("Movement Flicker (optional)")]
    [SerializeField] private bool enableLightMovement = true;
    [SerializeField] private float movementAmplitude = 0.02f;
    [SerializeField] private float movementSpeed = 1.5f;

    private List<Light> lights = new List<Light>();
    private List<Vector3> initialPositions = new List<Vector3>();
    private float randomOffset;

    private void Awake()
    {
        GetComponentsInChildren(lights);

        foreach (var light in lights)
            initialPositions.Add(light.transform.localPosition);

        randomOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float time = Time.time * flickerSpeed + randomOffset;

        for (int i = 0; i < lights.Count; i++)
        {
            var light = lights[i];
            if (light == null) continue;

            float noise = Mathf.PerlinNoise(time, i * 10f);
            float intensity = baseIntensity + (noise - 0.5f) * flickerRange * 2f;
            light.intensity = intensity;

            if (enableColorFlicker)
            {
                float t = Mathf.PerlinNoise(time * 0.7f, i * 10f);
                light.color = Color.Lerp(warmColor, hotColor, t);
            }

            if (enableLightMovement)
            {
                Vector3 startPos = initialPositions[i];
                float moveX = Mathf.PerlinNoise(time * movementSpeed, i) - 0.5f;
                float moveY = Mathf.PerlinNoise(i, time * movementSpeed) - 0.5f;
                light.transform.localPosition = startPos + new Vector3(moveX, moveY, 0f) * movementAmplitude;
            }
        }
    }
}
