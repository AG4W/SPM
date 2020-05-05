using UnityEngine;

using System.Collections;

public class FlickerEffect : MonoBehaviour
{
    [SerializeField]GameObject[] objects;

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
        if (objects.Length == 0)
            Debug.LogError(this.name + " with light flicker effect does not have lights assigned, did you forget to assign them?", this.gameObject);
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
            for (int i = 0; i < objects.Length; i++)
                objects[i].SetActive(!objects[i].activeSelf);

            yield return new WaitForSeconds(baseLightDuration + Random.Range(minLightDurationRandomModifier, maxLightDurationRandomModifier));

            for (int i = 0; i < objects.Length; i++)
                objects[i].SetActive(!objects[i].activeSelf);

            yield return new WaitForSeconds(repetitionDelay);

            repetitions++;
        }

        isReady = true;
    }
}
