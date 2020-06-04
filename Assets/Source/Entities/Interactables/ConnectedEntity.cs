using UnityEngine;

public class ConnectedEntity : Entity, IInteractable
{
    [SerializeField]string prompt;

    [SerializeField]float interactionDistance = 5f;

    [SerializeField]ConnectedEntity[] linkedControllers;
    [SerializeField]GameObject[] connectedObjects;
    [SerializeField]Vector3 promptOffset;

    [SerializeField]bool isRepeatable = true;
    [SerializeField]bool activateOnDestruction = false;

    [SerializeField]AudioSource source;
    [SerializeField]AudioClip[] interactionSoundEffects;

    protected GameObject[] ConnectedEntities { get { return connectedObjects; } }

    public string Prompt { get { return prompt; } }
    public float InteractionDistance { get { return interactionDistance; } }
    public Vector3 Position { get { return this.transform.position; } }
    public bool WantsPrompt { get { return prompt.Length > 0; } }

    protected override void Initalize()
    {
        base.Initalize();


        //om efterbliven designer glömt dragndroppa audiosource
        //sök igenom hierarkin efter en
        if(source == null)
            this.GetComponentInChildren<AudioSource>();

        this.onLinkedStart += OnLinkedStart;
        this.onLinkedAnimate += OnLinkedAnimate;
        this.onLinkedComplete += OnLinkedComplete;
    }

    public void Interact()
    {
        OnInteractStart();

        if (!isRepeatable)
            Destroy(this);
    }

    //vad som händer när denna aktiveras
    public virtual void OnInteractStart()
    {
        GlobalEvents.Raise(GlobalEvent.OnInteractableStart);

        for (int i = 0; i < linkedControllers.Length; i++)
            linkedControllers[i].onLinkedStart?.Invoke(this);

        CreateInteractionStartVFX();
        CreateInteractionStartSFX();
    }
    //behöver inte kallas
    //används mest för att trigga UI update när något animerat klart
    public virtual void OnInteractAnimate()
    {
        for (int i = 0; i < linkedControllers.Length; i++)
            linkedControllers[i].onLinkedAnimate?.Invoke(this);
    }
    public virtual void OnInteractComplete()
    {
        for (int i = 0; i < linkedControllers.Length; i++)
            linkedControllers[i].onLinkedComplete?.Invoke(this);

        GlobalEvents.Raise(GlobalEvent.OnInteractableComplete);
    }

    //Vad som händer när en länkad controller aktiveras
    protected virtual void OnLinkedStart(ConnectedEntity other)
    {

    }
    protected virtual void OnLinkedAnimate(ConnectedEntity other)
    {

    }
    protected virtual void OnLinkedComplete(ConnectedEntity other)
    {

    }

    protected virtual void CreateInteractionStartVFX()
    {

    }
    protected virtual void CreateInteractionStartSFX()
    {
        if (interactionSoundEffects != null && interactionSoundEffects.Length > 0)
        {
                source.PlayOneShot(interactionSoundEffects.Random());
        }
    }

    //Auto-reset on timer
    //Timed interact
    protected override void OnHealthZero()
    {
        if (activateOnDestruction)
            Interact();

        base.OnHealthZero();
    }

    public delegate void LinkedControllerEvent(ConnectedEntity other);
    public LinkedControllerEvent onLinkedStart;
    public LinkedControllerEvent onLinkedAnimate;
    public LinkedControllerEvent onLinkedComplete;
}
