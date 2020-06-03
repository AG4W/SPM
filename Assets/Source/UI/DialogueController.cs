using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    GameObject root;
    Text text;

    AudioSource source;
    float timer;

    Dialogue current;

    void Start()
    {
        root = this.transform.FindRecursively("Dialogue").Find("Root").gameObject;

        source = this.transform.FindRecursively("Dialogue").GetComponentInChildren<AudioSource>();
        text = root.GetComponentInChildren<Text>();

        GlobalEvents.Subscribe(GlobalEvent.PlayDialogue, (object[] args) => PlayDialogue((Dialogue)args[0]));

        root.SetActive(false);
    }
    void Update()
    {
        if (current != null && root.activeSelf)
        {
            timer += Time.deltaTime;

            if (timer >= current.Time)
            {
                if (current.Next)
                    PlayDialogue(current.Next);
                else
                    root.SetActive(false);

                current.Events?.Invoke();
            }
        }
    }

    public void PlayDialogue(Dialogue dialogue)
    {
        if(dialogue.Clip != null)
            source.PlayOneShot(dialogue.Clip);

        current = dialogue;

        timer = 0f;
        text.text = current.Text;
        root.SetActive(true);
    }
}
