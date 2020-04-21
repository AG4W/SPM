using UnityEngine;

public interface IInteractable
{
    string Prompt { get; }
    float InteractionDistance { get; }
    Vector3 Position { get; }

    void Interact();
}
