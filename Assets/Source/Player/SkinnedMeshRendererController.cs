using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshRendererController : MonoBehaviour
{

    Material[][] materials;

    void Start()
    {
        // Cache all the SkinnedMeshRenderers in the components children
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        materials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
                materials[i] = renderers[i].sharedMaterials;
        }

        GlobalEvents.Subscribe(GlobalEvent.SetPlayerAlpha, (object[] args) => SetAlpha((float)args[0]));
    }

    /// <summary>
    /// Sets the alpha level on all the Material of all the Skinned Mesh Renderers on the applied game object.
    /// </summary>
    /// <param name="alpha">The alpha level to be set on the materials [0-1].</param>
    void SetAlpha(float alpha)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            for (int j = 0; j < materials[i].Length; j++)
            {
                Color tempColor = materials[i][j].GetColor("_BaseColor");
                tempColor.a = alpha;
                materials[i][j].SetColor("_BaseColor", tempColor);
            }
        }
    }
}
