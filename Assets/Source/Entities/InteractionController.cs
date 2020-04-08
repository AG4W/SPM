using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [SerializeField]InteractableEntity currentEntity;

    [SerializeField]LayerMask mask;

    void Update()
    {
        UpdateCurrentEntity();
    }

    void UpdateCurrentEntity()
    {
        InteractableEntity lastEntity = currentEntity;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask);


        if (hit.transform != null)
        {
            InteractableEntity entity = hit.transform.root.GetComponentInChildren<InteractableEntity>();

            if (entity != null && Vector3.Distance(this.transform.position, entity.transform.position) <= entity.InteractionDistance)
                currentEntity = entity;
            else
                currentEntity = null;
        }
        else
            currentEntity = null;

        if (currentEntity != lastEntity)
            GlobalEvents.Raise(GlobalEvent.CurrentInteractableEntityChanged, currentEntity, this.transform);
    }
}
