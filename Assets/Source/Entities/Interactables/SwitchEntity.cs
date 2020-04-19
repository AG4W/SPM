using UnityEngine;

public class SwitchEntity : ConnectedEntity
{
    [Header("Switch Settings")]
    [SerializeField]string offHeader = "Offstate - Replace Me";

    bool inOffState = false;

    public override string InteractionHeader
    {
        get
        {
            if (inOffState)
                return offHeader;
            else
                return base.InteractionHeader;
        }
    }

    protected override void OnInteractionStart()
    {
        base.OnInteractionStart();

        for (int i = 0; i < base.ConnectedEntities.Length; i++)
            base.ConnectedEntities[i].SetActive(!base.ConnectedEntities[i].activeSelf);

        inOffState = !inOffState;
    }
}
