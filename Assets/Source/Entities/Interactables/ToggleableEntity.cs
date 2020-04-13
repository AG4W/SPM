public class ToggleableEntity : InteractableEntity
{
    protected override void OnInteractionDelayComplete()
    {
        base.OnInteractionDelayComplete();

        for (int i = 0; i < base.ConnectedEntities.Length; i++)
            base.ConnectedEntities[i].SetActive(!base.ConnectedEntities[i].activeSelf);
    }
}
