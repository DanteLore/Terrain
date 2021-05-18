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

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentTimeOfDay += (Time.deltaTime / dayLengthSeconds);
        if(currentTimeOfDay >= 1)
            currentTimeOfDay -= 1;

        Debug.Log(currentTimeOfDay);

        sun.transform.localRotation = Quaternion.Euler((currentTimeOfDay * -360f) - 90f, 90f, 90f);

        if(currentTimeOfDay <= 0.2f)
            RenderSettings.fogColor = nighttimeFogColor;
        else if(currentTimeOfDay <= 0.3f)
            RenderSettings.fogColor = Color.Lerp(nighttimeFogColor, daytimeFogColor, Mathf.InverseLerp(0.2f, 0.3f, currentTimeOfDay));
        else if(currentTimeOfDay <= 0.7f)
            RenderSettings.fogColor = daytimeFogColor;
        else if(currentTimeOfDay <= 0.8f)
            RenderSettings.fogColor = Color.Lerp(daytimeFogColor, nighttimeFogColor, Mathf.InverseLerp(0.7f, 0.8f, currentTimeOfDay));
        else
            RenderSettings.fogColor = nighttimeFogColor;
    }
}
