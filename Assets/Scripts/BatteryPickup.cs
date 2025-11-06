using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BatteryPickup : MonoBehaviour, IInteractable
{
    [Tooltip("Player must be tagged 'Player' and have a Flashlight component.")]
    public string playerTag = "Player";

    [Header("FX (optional)")]
    public AudioSource sfx;
    public GameObject pickupVFX;
    public float destroyDelay = 0.1f;

    SpriteRenderer sr;
    Collider2D col;
    bool consumed;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        // Battery should be NON-trigger (Player has the trigger)
        c.isTrigger = false;
    }

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col) col.isTrigger = false;
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    // ---------- IInteractable ----------
    public bool CanInteract(GameObject interactor)
    {
        if (consumed) return false;
        if (!interactor || !interactor.CompareTag(playerTag)) return false;

        var fl = interactor.GetComponent<Flashlight>();
        if (!fl) return false;

        bool ok = !fl.IsFull();
        // Debug info so we know why it’s not interactable
        if (!ok) Debug.Log("[Battery] Player is already full; cannot interact.");
        return ok;
    }

    public void Interact(GameObject interactor)
    {
        Debug.Log("[Battery] Interact called.");
        if (!CanInteract(interactor)) return;

        var fl = interactor.GetComponent<Flashlight>();
        if (!fl) { Debug.LogWarning("[Battery] No Flashlight on player."); return; }

        fl.RefillToMax();
        Consume();
    }
    // -----------------------------------

    void Consume()
    {
        consumed = true;
        Debug.Log("[Battery] Consumed!");

        if (pickupVFX)
        {
            var v = Instantiate(pickupVFX, transform.position, Quaternion.identity);
            Destroy(v, 1.5f);
        }
        if (sfx) sfx.Play();

        if (sr) sr.enabled = false;
        if (col) col.enabled = false;

        Destroy(gameObject, (sfx && sfx.clip) ? sfx.clip.length : destroyDelay);
    }
}
