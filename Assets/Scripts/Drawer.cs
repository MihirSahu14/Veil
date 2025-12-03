// Drawer.cs
using UnityEngine;

public enum DrawerLootType { None, Battery, Key }

public class Drawer : MonoBehaviour, IInteractable
{
    [Header("Contents")]
    [Tooltip("What this drawer currently holds.")]
    public DrawerLootType lootType = DrawerLootType.None;

    [Tooltip("Optional: override icon used in the UI for THIS drawer only.")]
    public Sprite customIcon;

    [Header("FX (optional)")]
    public AudioSource openSfx;
    public AudioSource emptySfx;
    public AudioSource lootSfx;

    public bool CanInteract(GameObject interactor)
    {
        // We could add distance checks/etc here, but PlayerInteractor already filters by radius.
        return true;
    }

    public void Interact(GameObject interactor)
    {
        if (DrawerUIManager.Instance == null)
        {
            Debug.LogWarning("[Drawer] No DrawerUIManager in scene.");
            return;
        }

        if (openSfx) openSfx.Play();
        DrawerUIManager.Instance.OpenDrawer(this, interactor);
    }

    /// <summary>Helper so UI can clear the contents when taken.</summary>
    public void ClearLoot()
    {
        lootType = DrawerLootType.None;
    }
}
