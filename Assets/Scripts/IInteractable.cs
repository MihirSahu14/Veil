using UnityEngine;

public interface IInteractable
{
    // Return true if this object can currently be interacted with by 'interactor' (your Player).
    bool CanInteract(GameObject interactor);

    // Perform the interaction.
    void Interact(GameObject interactor);
}
