using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptController : MonoBehaviour
{
    [SerializeField]Text prompt;

    InteractableEntity target;

    void Awake()
    {
        GlobalEvents.Subscribe(GlobalEvent.CurrentInteractableEntityChanged, (object[] args) => {
            InteractableEntity entity = args[0] as InteractableEntity;

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
        if (target == null)
            return;

        prompt.transform.position = Camera.main.WorldToScreenPoint(target.transform.position + target.PromptOffset);

        if (Input.GetKeyDown(KeyCode.E))
            target.Interact();
    }
    void OpenInteractPrompt(InteractableEntity entity, Transform interactee)
    {
        target = entity;

        prompt.text = entity.Header;
        prompt.gameObject.SetActive(true);
    }
    void CloseInteractPrompt()
    {
        prompt.gameObject.SetActive(false);
        target = null;
    }
}
