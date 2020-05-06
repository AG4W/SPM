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
        {
            Debug.Log("Försöker plocka upp vapen");
            current.Interact();
        }
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
