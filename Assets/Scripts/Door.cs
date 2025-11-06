using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public GameManager gameManager;
    public AudioSource lockedSfx;
    public AudioSource openSfx;

    public void Interact(GameObject interactor)
    {
        var inv = interactor.GetComponentInParent<PlayerInventory>();
        if (!inv) return;

        if (inv.HasKey)
        {
            inv.ConsumeKey();
            if (openSfx) openSfx.Play();
            foreach (var c in GetComponentsInChildren<Collider2D>())
                c.enabled = false; // disable all colliders
            gameManager?.Win();
        }
        else
        {
            if (lockedSfx) lockedSfx.Play();
            Debug.Log("🚪 It's locked.");
        }
    }

    public bool CanInteract(GameObject interactor) => true;
}
