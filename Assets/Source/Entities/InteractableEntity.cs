using UnityEngine;

public class InteractableEntity : Entity
{
    [SerializeField]float interactionDistance = 5f;

    [SerializeField]bool isRepeatable = true;

    [SerializeField]AudioClip[] interactionSoundEffects;

    [SerializeField]GameObject[] connectedEntities;
    [SerializeField]Vector3 promptOffset;

    [SerializeField]bool activateOnDestruction = false;

    AudioSource source;

    public Vector3 PromptOffset { get { return promptOffset; } }

    public float InteractionDistance { get { return interactionDistance; } }

    protected override void Initalize()
    {
        base.Initalize();
        source = this.GetComponentInChildren<AudioSource>();
    }

    protected override void OnHealthZero()
    {
        if (activateOnDestruction)
            Interact();

        base.OnHealthZero();
    }

    public virtual void Interact()
    {
        PlayInteractionEffects();

        for (int i = 0; i < connectedEntities.Length; i++)
            connectedEntities[i].SetActive(!connectedEntities[i].activeSelf);

        if (!isRepeatable)
            Destroy(this);
    }

    void PlayInteractionEffects()
    {
        if (interactionSoundEffects != null && interactionSoundEffects.Length > 0)
            source.PlayOneShot(interactionSoundEffects.Random());
    }
}
