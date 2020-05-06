using UnityEngine;

public class InteractionController : MonoBehaviour
{
    IInteractable current;

    [SerializeField]LayerMask mask;

    void Awake()
    {
        GlobalEvents.Subscribe(GlobalEvent.OnInteractableStart, (object[] args) => current = null);
        GlobalEvents.Subscribe(GlobalEvent.OnInteractableComplete, (object[] args) => current = null);
    }
    void Update()
    {
        UpdateCurrentEntity();
    }

    void UpdateCurrentEntity()
    {
        IInteractable lastEntity = current;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask);

        if (hit.transform != null)
        {
            IInteractable entity = hit.transform.GetComponent<IInteractable>();

            if (entity != null && Vector3.Distance(this.transform.position, entity.Position) <= entity.InteractionDistance)
                current = entity;
            else
                current = null;
        }
        else
            current = null;

        if (current != lastEntity)
            GlobalEvents.Raise(GlobalEvent.CurrentInteractableChanged, current, this.transform);
    }
}
