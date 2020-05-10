using UnityEngine;

public class BlinkEffect : MonoBehaviour
{
    [SerializeField]float onTime;
    [SerializeField]float offTime;

    [SerializeField]bool startOn = true;

    [SerializeField]GameObject[] objects;

    float timer;
    float threshold;

    void Awake()
    {
        threshold = startOn ? onTime : offTime;

        for (int i = 0; i < objects.Length; i++)
            objects[i].SetActive(startOn);
    }
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= threshold)
            Toggle();
    }
    void Toggle()
    {
        for (int i = 0; i < objects.Length; i++)
            objects[i].SetActive(!objects[i].activeSelf);

        threshold = threshold == onTime ? offTime : onTime;
        timer = 0f;
    }
}
