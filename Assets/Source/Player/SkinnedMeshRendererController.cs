using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshRendererController : MonoBehaviour
{

    SkinnedMeshRenderer[] renderers;

    int counter = 0;

    void Start()
    {
        // Hämta alla renderer
        // Hämta alla material från alla renderers

        renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

    }

    void Update()
    {
        if (Input.GetKey(KeyCode.N))
            SetAlpha(0f);
        if (Input.GetKey(KeyCode.M))
            SetAlpha(1f);
    }

    void SetAlpha(float alpha)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            for (int j = 0; j < renderers[i].sharedMaterials.Length; j++)
            {
                Color tempColor = renderers[i].sharedMaterials[j].GetColor("_BaseColor");
                tempColor.a = alpha;
                renderers[i].sharedMaterials[j].SetColor("_BaseColor", tempColor);
            }
            
        }
    }
}
