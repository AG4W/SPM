using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [SerializeField]LayerMask mask;

    [SerializeField]GameObject currentTarget;

    Ray ray;

    void Awake()
    {
        ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
    }

    void Update()
    {
        UpdateCurrentTarget();
    }

    void UpdateCurrentTarget()
    {
        RaycastHit hit;
        Physics.Raycast(ray, out hit, Mathf.Infinity, mask);

        if(hit.transform.gameObject != currentTarget)
        {
            Entity e = hit.transform.root.GetComponent<Entity>();

            if(e != null)
            {
                if(e is InteractableEntity ie)
                    GlobalEvents.Raise(GlobalEvent.OpenInteractPrompt, ie);
            }
        }

        currentTarget = hit.transform.gameObject;
    }
}
