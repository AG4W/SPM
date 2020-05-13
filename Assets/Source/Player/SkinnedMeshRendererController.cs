using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshRendererController : MonoBehaviour
{

    SkinnedMeshRenderer[] renderers;

    void Start()
    {
        renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        GlobalEvents.Subscribe(GlobalEvent.SetPlayerAlpha, (object[] args) => SetAlpha((float)args[0]));
    }

    /// <summary>
    /// Sets the alpha level on all the Material of all the Skinned Mesh Renderers on the applied game object.
    /// </summary>
    /// <param name="alpha">The alpha level to be set on the materials [0-1].</param>
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
