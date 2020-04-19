using UnityEngine;

public class ToggleEntity : SwitchEntity
{
    public override void OnInteractStart()
    {
        base.OnInteractStart();

        for (int i = 0; i < base.ConnectedEntities.Length; i++)
            base.ConnectedEntities[i].SetActive(!base.ConnectedEntities[i].activeSelf);
    }
}
