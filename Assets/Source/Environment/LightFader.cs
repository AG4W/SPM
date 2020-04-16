using UnityEngine;

public class LightFader : MonoBehaviour
{
    [SerializeField]Light[] lights;

    [SerializeField]FadeMode mode = FadeMode.Darken;
    [SerializeField]float fadeSpeed = 2f;

    void Update()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].intensity += (int)mode * fadeSpeed * Time.deltaTime;

            if (lights[i].intensity <= 0f)
                lights[i].intensity = 0f;
        }
    }
}
public enum FadeMode
{
    Brighten = 1,
    Darken = -1
}
