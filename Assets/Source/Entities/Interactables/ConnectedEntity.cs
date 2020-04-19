using UnityEngine;

public class ConnectedEntity : Entity, IInteractable
{
    [SerializeField]string interactionHeader = "Replace Me";

    [SerializeField]float interactionDistance = 5f;

    [SerializeField]GameObject[] connectedEntities;
    [SerializeField]Vector3 promptOffset;

    [SerializeField]bool isRepeatable = true;
    [SerializeField]bool activateOnDestruction = false;

    [SerializeField]AudioSource source;
    [SerializeField]AudioClip[] interactionSoundEffects;

    protected GameObject[] ConnectedEntities { get { return connectedEntities; } }

    public virtual string InteractionHeader { get { return interactionHeader; } }
    public float InteractionDistance { get { return interactionDistance; } }

    public Vector3 PromptPosition { get { return this.transform.position + promptOffset; } }

    protected override void Initalize()
    {
        base.Initalize();

        if(connectedEntities.Length == 0)
            Debug.LogError(this.name + " does not have any connected entities, did you forget to assign them?", this.gameObject);
        if (this.GetComponent<Collider>() == null)
            Debug.LogError(this.name + " is missing a collider or trigger and will not be interactable.", this.gameObject);

        //om efterbliven designer glömt dragndroppa audiosource
        //sök igenom hierarkin efter en
        if(source == null)
            this.GetComponentInChildren<AudioSource>();
        //ifall det fortfarande inte finns någon, skicka argt meddelande.
        if(source == null)
            Debug.LogWarning(this.name + " is missing an audiosource and will not play any sound!", this.gameObject);
    }

    public void Interact()
    {
        OnInteractionStart();

        if (!isRepeatable)
            Destroy(this);
    }
    protected virtual void OnInteractionStart()
    {
        CreateInteractionVFX();
        CreateInteractionSFX();
    }

    protected virtual void CreateInteractionVFX()
    {

    }
    protected virtual void CreateInteractionSFX()
    {
        if (interactionSoundEffects != null && interactionSoundEffects.Length > 0)
        {
            if(source == null)
                Debug.LogError(this.name + " is attempting to play a sound without an audio source assigned, did you forget to assign it?", this.gameObject);
            else
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
}
