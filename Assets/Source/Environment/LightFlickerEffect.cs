using UnityEngine;

using System.Collections;

public class LightFlickerEffect : MonoBehaviour
{
    [SerializeField]Light[] lights;

    [SerializeField]float baseFlickerInterval = 1f;
    [SerializeField]float minFlickerRandomModifier = .1f;
    [SerializeField]float maxFlickerRandomModifier = .3f;

    [SerializeField]float baseLightDuration = .2f;
    [SerializeField]float minLightDurationRandomModifier = .1f;
    [SerializeField]float maxLightDurationRandomModifier = .2f;

    [SerializeField]int minRepetitions = 1;
    [SerializeField]int maxRepetitions = 5;

    [SerializeField]float repetitionDelay = .01f;

    [SerializeField]bool randomizeStart = true;

    float currentFlickerInterval;
    float flickerTimer;

    bool isReady = true;

    void Awake()
    {
        if (lights.Length == 0)
            Debug.LogError(this.name + " with light flicker effect does not have lights assigned, did you forget to assign them?", this.gameObject);

#if UNITY_EDITOR
        for (int i = 0; i < lights.Length; i++)
            if(lights[i].lightmapBakeType != LightmapBakeType.Realtime)
                Debug.LogError("Warning, applying flicker effect to baked light(s) [" + lights[i].name + "] " +
                    "will produce weird artifacts, use realtime lights with LightFlickerEffect.cs >:(", this.gameObject);
#endif

        if (randomizeStart)
            flickerTimer += Random.Range(0f, currentFlickerInterval);
    }
    void Update()
    {
        flickerTimer += Time.deltaTime;

        if(flickerTimer >= currentFlickerInterval && isReady)
            StartCoroutine(ToggleLightAsync());
    }
    IEnumerator ToggleLightAsync()
    {
        isReady = false;
        currentFlickerInterval = baseFlickerInterval + Random.Range(minFlickerRandomModifier, maxFlickerRandomModifier);
        flickerTimer = 0f;

        int repetitions = 0;

        while (repetitions <= Random.Range(minRepetitions, maxRepetitions))
        {
            for (int i = 0; i < lights.Length; i++)
                lights[i].enabled = !lights[i].enabled;

            yield return new WaitForSeconds(baseLightDuration + Random.Range(minLightDurationRandomModifier, maxLightDurationRandomModifier));

            for (int i = 0; i < lights.Length; i++)
                lights[i].enabled = !lights[i].enabled;

            yield return new WaitForSeconds(repetitionDelay);

            repetitions++;
        }

        isReady = true;
    }
}
