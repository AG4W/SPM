using UnityEngine;

public interface IInteractable
{
    string InteractionHeader { get; }
    float InteractionDistance { get; }

    Vector3 PromptPosition { get; }

    void Interact();
}
