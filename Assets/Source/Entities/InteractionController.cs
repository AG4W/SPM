using UnityEngine;

public class InteractionController : MonoBehaviour
{
    IInteractable interactable;

    [SerializeField]LayerMask mask;

    void Awake()
    {
        GlobalEvents.Subscribe(GlobalEvent.OnInteractableStart, (object[] args) => interactable = null);
        GlobalEvents.Subscribe(GlobalEvent.OnInteractableComplete, (object[] args) => interactable = null);
    }
    void Update()
    {
        UpdateCurrentEntity();
    }

    void UpdateCurrentEntity()
    {
        IInteractable lastEntity = interactable;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask);

        if (hit.transform != null)
        {
            IInteractable entity = hit.transform.GetComponent<IInteractable>();

            if (entity != null && Vector3.Distance(this.transform.position, entity.Position) <= entity.InteractionDistance)
                interactable = entity;
            else
                interactable = null;
        }
        else
            interactable = null;

        if (interactable != lastEntity)
            GlobalEvents.Raise(GlobalEvent.CurrentInteractableChanged, interactable, this.transform);
    }
}
