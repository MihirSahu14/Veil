// Assets/Scripts/KeyPickup.cs
using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteractable
{
    [Header("FX (optional)")]
    public AudioSource sfx;

    public bool CanInteract(GameObject interactor)
    {
        // Allow pick-up if player doesn't already have the key
        var inv = interactor.GetComponentInParent<PlayerInventory>();
        return inv && !inv.HasKey;
    }

    public void Interact(GameObject interactor)
    {
        var inv = interactor.GetComponentInParent<PlayerInventory>();
        if (!inv) return;

        if (!inv.HasKey)
        {
            inv.GiveKey();
            if (sfx) sfx.Play();
            // disable visuals immediately
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
            Destroy(gameObject, sfx ? sfx.clip.length : 0f);
        }
    }
}
