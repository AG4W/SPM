using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptController : MonoBehaviour
{
    [SerializeField]Text prompt;

    void Awake()
    {
        GlobalEvents.Subscribe(GlobalEvent.OpenInteractPrompt, OpenInteractPrompt);
        GlobalEvents.Subscribe(GlobalEvent.CloseInteractPrompt, (object[] args) => CloseInteractPrompt());

        //start disabled
        CloseInteractPrompt();
    }

    void OpenInteractPrompt(object[] args)
    {
        prompt.text = "<color=green>[</color> " + ((Entity)args[0]).Header + " <color=green>]</color>";
        prompt.gameObject.SetActive(true);
    }
    void CloseInteractPrompt()
    {
        prompt.gameObject.SetActive(false);
    }
}
