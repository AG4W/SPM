using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptController : MonoBehaviour
{
    [SerializeField]Text prompt;

    IInteractable interactable;

    void Awake()
    {
        GlobalEvents.Subscribe(GlobalEvent.CurrentInteractableChanged, (object[] args) => {
            IInteractable entity = args[0] as IInteractable;

            if (entity != null)
                OpenInteractPrompt(entity, args[1] as Transform);
            else
                CloseInteractPrompt();
        });

        //start disabled
        CloseInteractPrompt();
    }
    void Update()
    {
        if (interactable == null)
            return;

        prompt.transform.position = Camera.main.WorldToScreenPoint(interactable.PromptPosition);

        if (Input.GetKeyDown(KeyCode.E))
        {
            interactable.Interact();
            GlobalEvents.Raise(GlobalEvent.OnInteractableActivated, interactable);
        }
    }
    void OpenInteractPrompt(IInteractable entity, Transform interactee)
    {
        interactable = entity;

        prompt.text = entity.InteractionHeader;
        prompt.gameObject.SetActive(true);
    }
    void CloseInteractPrompt()
    {
        prompt.gameObject.SetActive(false);
        interactable = null;
    }
}
