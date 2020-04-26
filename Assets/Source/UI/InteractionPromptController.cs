using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptController : MonoBehaviour
{
    [SerializeField]Text prompt;

    IInteractable current;

    void Awake()
    {
        GlobalEvents.Subscribe(GlobalEvent.CurrentInteractableChanged, (object[] args) => {
            if (args[0] is IInteractable entity && entity.Prompt.Length > 0)
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

        prompt.transform.position = Camera.main.WorldToScreenPoint(current.Position);

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
