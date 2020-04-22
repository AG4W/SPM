using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptController : MonoBehaviour
{
    [SerializeField]Text prompt;

    IInteractable current;

    void Awake()
    {
        GlobalEvents.Subscribe(GlobalEvent.CurrentInteractableChanged, (object[] args) => {
            IInteractable entity = args[0] as IInteractable;

            if (entity != null && entity.Prompt.Length > 0)
                OpenInteractPrompt(entity);
            else
                CloseInteractPrompt();
        });

        //start disabled
        CloseInteractPrompt();
    }
    void Update()
    {
        if (current == null)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            current.Interact();
    }
    void OpenInteractPrompt(IInteractable entity)
    {
        current = entity;

        prompt.text = entity.Prompt;
        prompt.gameObject.SetActive(true);
    }
    void CloseInteractPrompt()
    {
        prompt.gameObject.SetActive(false);
        current = null;
    }
}
