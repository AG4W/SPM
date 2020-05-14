using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class SimpleScreen : MonoBehaviour
{
    int textureIndex = -1;

    [SerializeField]float timePerScreen = 5f;
    float timer;

    [SerializeField]Texture[] screens;
    [SerializeField]bool randomizeOrder = true;

    [SerializeField]Light[] lights;
    [SerializeField]MeshRenderer[] renderers;

    Material[] materials;

    void Awake()
    {
        materials = new Material[renderers.Length];

        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = Instantiate(renderers[i].material);
            renderers[i].material = materials[i];
        }

        UpdateScreens();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= timePerScreen)
            UpdateScreens();
    }
    void UpdateScreens()
    {
        timer = 0f;

        if (textureIndex < screens.Length - 1)
            textureIndex++;
        else
            textureIndex = 0;

        Texture t = randomizeOrder ? screens.Random() : screens[textureIndex];

        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetTexture("_BaseColorMap", t);
            materials[i].SetTexture("_EmissiveColorMap", t);
        }
        for (int i = 0; i < lights.Length; i++)
            lights[i].GetComponent<HDAdditionalLightData>().SetCookie(t);
    }
}
