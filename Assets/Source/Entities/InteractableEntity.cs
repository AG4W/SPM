using UnityEngine;

public class InteractableEntity : Entity
{
    [SerializeField]float interactionDistance = 2f;
    [Header("not used atm")]
    [SerializeField]float interactionCost = 5f;

    public float InteractionDistance { get { return interactionDistance; } }
    public float InteractionCost { get { return interactionCost; } }

    public virtual void Interact()
    {

    }
}
