using UnityEngine;

public interface IInteractable
{
    string Prompt { get; }
    float InteractionDistance { get; }
    bool WantsPrompt { get; }

    Vector3 Position { get; }

    void Interact();
}
