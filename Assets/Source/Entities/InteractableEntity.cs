using UnityEngine;

public class InteractableEntity : Entity
{
    [SerializeField]float interactionDistance = 2f;
    [SerializeField]VitalValuePair[] interactionCosts;

    [SerializeField]AudioClip[] interactionSoundEffects;

    AudioSource source;

    public float InteractionDistance { get { return interactionDistance; } }

    protected override void Initalize()
    {
        base.Initalize();
        source = this.GetComponentInChildren<AudioSource>();
    }

    public virtual void Interact(Actor interactee)
    {
        ApplyInteractionCosts(interactee);
        PlayInteractionEffects(interactee);
    }

    void ApplyInteractionCosts(Actor interactee)
    {
        if (interactionCosts == null)
            return;

        for (int i = 0; i < interactionCosts.Length; i++)
            interactee.GetVital(interactionCosts[i].Type).Update(interactionCosts[i].Value);
    }
    void PlayInteractionEffects(Actor interactee)
    {
        if (interactionSoundEffects != null && interactionSoundEffects.Length > 0)
            source.PlayOneShot(interactionSoundEffects.Random());
    }
}
