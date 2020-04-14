using UnityEngine;

using System.Collections;

public class InteractableEntity : Entity
{
    [SerializeField]GameObject[] connectedEntities;
    [SerializeField]Vector3 promptOffset;

    [SerializeField]float interactionDistance = 5f;
    [SerializeField]float interactionDelay = 0f;

    [SerializeField]bool isRepeatable = true;
    [SerializeField]bool activateOnDestruction = false;

    [SerializeField]AudioSource source;
    [SerializeField]AudioClip[] interactionSoundEffects;

    protected GameObject[] ConnectedEntities { get { return connectedEntities; } }
    public Vector3 PromptOffset { get { return promptOffset; } }

    public float InteractionDistance { get { return interactionDistance; } }

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

    protected override void OnHealthZero()
    {
        if (activateOnDestruction)
            Interact();

        base.OnHealthZero();
    }

    public void Interact()
    {
        OnInteractionStart();
        this.StartCoroutine(InteractAsync());
    }

    protected virtual void CreateOnInteractVFX()
    {

    }
    protected virtual void CreateOnInteractSFX()
    {
        if (interactionSoundEffects != null && interactionSoundEffects.Length > 0)
        {
            if(source == null)
                Debug.LogError(this.name + " is attempting to play a sound without an audio source assigned, did you forget to assign it?", this.gameObject);
            else
                source.PlayOneShot(interactionSoundEffects.Random());
        }
    }
    
    protected virtual void OnInteractionStart()
    {
        CreateOnInteractVFX();
        CreateOnInteractSFX();
    }
    protected virtual void OnInteractionDelayComplete()
    {
        
    }

    //Auto-reset on timer
    //Timed interact

    IEnumerator InteractAsync()
    {
        yield return new WaitForSeconds(interactionDelay);
        OnInteractionDelayComplete();

        if (!isRepeatable)
            Destroy(this);
    }
}
