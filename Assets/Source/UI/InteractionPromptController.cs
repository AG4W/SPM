using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptController : MonoBehaviour
{
    Text prompt;
    IInteractable current;

    void Start()
    {
        prompt = this.GetComponentInChildren<Text>();

        GlobalEvents.Subscribe(GlobalEvent.CurrentInteractableChanged, (object[] args) => {
            if (args[0] is IInteractable interactable)
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
        GlobalEvents.Raise(GlobalEvent.SetCrosshairIcon, CrosshairIcon.Interact);

        current = entity;

        prompt.text = entity.Prompt;
        prompt.gameObject.SetActive(true);
    }
    void CloseInteractPrompt()
    {
        GlobalEvents.Raise(GlobalEvent.SetCrosshairIcon, CrosshairIcon.Default);

        prompt.gameObject.SetActive(false);
        current = null;
    }
}
