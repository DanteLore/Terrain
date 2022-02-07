using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;

    public float dayLengthSeconds = 60f;

    [Range(0, 1)]
    public float currentTimeOfDay = 0f;

    public Color daytimeFogColor;

    public Color nighttimeFogColor;

    public bool IsNight { get { return currentTimeOfDay < 0.22f || currentTimeOfDay > 0.78f; }}

    void Start()
    {
        
    }

    void Update()
    {
        currentTimeOfDay += (Time.deltaTime / dayLengthSeconds);
        if (currentTimeOfDay >= 1)
            currentTimeOfDay -= 1;

        sun.transform.localRotation = Quaternion.Euler((currentTimeOfDay * -360f) - 90f, 90f, 90f);

        if (currentTimeOfDay <= 0.2f)
        {
            sun.intensity = 0;
            RenderSettings.fogColor = nighttimeFogColor;
        }
        else if (currentTimeOfDay <= 0.3f)
        {
            float t = Mathf.InverseLerp(0.2f, 0.3f, currentTimeOfDay);
            sun.intensity = Mathf.Lerp(0, 1, t);
            RenderSettings.fogColor = Color.Lerp(nighttimeFogColor, daytimeFogColor, t);
        }
        else if (currentTimeOfDay <= 0.7f)
        {
            sun.intensity = 1;
            RenderSettings.fogColor = daytimeFogColor;
        }
        else if (currentTimeOfDay <= 0.8f)
        {
            float t = Mathf.InverseLerp(0.7f, 0.8f, currentTimeOfDay);
            sun.intensity = Mathf.Lerp(1, 0, t);
            RenderSettings.fogColor = Color.Lerp(daytimeFogColor, nighttimeFogColor, t);
        }
        else
        {
            sun.intensity = 0;
            RenderSettings.fogColor = nighttimeFogColor;
        }
    }
}
