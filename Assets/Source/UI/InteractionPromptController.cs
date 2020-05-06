using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptController : MonoBehaviour
{
    Text prompt;
    IInteractable current;

    void Awake()
    {
        prompt = this.GetComponentInChildren<Text>(true);

        GlobalEvents.Subscribe(GlobalEvent.CurrentInteractableChanged, (object[] args) => {
            if (args[0] is IInteractable interactable && interactable.Prompt.Length > 0)
                OpenInteractPrompt(interactable);
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

        prompt.transform.position = Camera.main.WorldToScreenPoint(current.Position);

        if (Input.GetKeyDown(KeyCode.E))
            current.Interact();
    }
    void OpenInteractPrompt(IInteractable entity)
    {
        if (prompt != null)
        {
            current = entity;

            prompt.text = entity.Prompt;
            prompt.gameObject.SetActive(true);
        }
    }
    void CloseInteractPrompt()
    {
        if (prompt != null)
        {
            prompt.gameObject.SetActive(false);
            current = null;
        }
    }
}
