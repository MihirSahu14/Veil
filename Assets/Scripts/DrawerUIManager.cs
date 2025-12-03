using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DrawerUIManager : MonoBehaviour
{
    public static DrawerUIManager Instance { get; private set; }

    [Header("Root (panel that covers the whole screen)")]
    public GameObject root;          // Canvas/DrawerUI

    [Header("Animation")]
    public Animator drawerAnimator;  // on DrawerAnim

    [Header("Visuals")]
    public Image drawerBackground;   // optional
    public Image itemIcon;
    public Sprite keySprite;
    public Sprite batterySprite;
    public Sprite emptySprite;
    public Text label;

    // ---------- NEW: scare config ----------
    [Header("Scare (non-lethal)")]
    [Range(0f, 1f)]
    public float scareChance = 0.4f;         // 40% by default
    public Image scareImage;                 // full-screen image above drawer UI
    public AudioSource scareAudio;           // optional
    public AudioClip scareClip;              // optional
    public float scareFadeTime = 0.12f;
    public float scareHoldTime = 0.6f;
    // --------------------------------------

    Drawer currentDrawer;
    GameObject currentInteractor;
    bool isOpen;
    bool openAnimFinished;
    bool scarePlaying;
    float prevTimeScale;

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (root) root.SetActive(false);

        // ensure scare image starts hidden
        if (scareImage)
        {
            var c = scareImage.color;
            c.a = 0f;
            scareImage.color = c;
            scareImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isOpen || !openAnimFinished || scarePlaying) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)
        {
            CloseDrawer();
        }
    }

    public void OpenDrawer(Drawer drawer, GameObject interactor)
    {
        if (!root)
        {
            Debug.LogError("[DrawerUI] Root panel not assigned.");
            return;
        }

        currentDrawer = drawer;
        currentInteractor = interactor;
        isOpen = true;
        openAnimFinished = false;
        scarePlaying = false;

        prevTimeScale = Time.timeScale;   // world runs while anim plays

        if (itemIcon) itemIcon.enabled = false;
        if (label) label.enabled = false;

        root.SetActive(true);

        if (drawerAnimator)
        {
            drawerAnimator.Play(0, 0, 0f);   // restart default state
        }
        else
        {
            OnDrawerOpenAnimFinished();
        }

        RefreshVisuals();
    }

    public void CloseDrawer()
    {
        if (!isOpen) return;

        isOpen = false;
        openAnimFinished = false;
        scarePlaying = false;

        if (root) root.SetActive(false);
        Time.timeScale = prevTimeScale;
        currentDrawer = null;
        currentInteractor = null;
    }

    void RefreshVisuals()
    {
        if (!currentDrawer) return;

        Sprite icon = emptySprite;
        string text = "";
        float targetAlpha = 0f;                 // default: invisible (for Empty)
        Vector3 targetScale = Vector3.one;      // default scale

        if (currentDrawer.lootType == DrawerLootType.Key)
        {
            icon = currentDrawer.customIcon ? currentDrawer.customIcon : keySprite;
            text = "Key";
            targetAlpha = 1f;

            // Make key bigger
            itemIcon.rectTransform.sizeDelta = new Vector2(220f, 220f); // tweak if needed

            targetScale = Vector3.one;
        }

        else if (currentDrawer.lootType == DrawerLootType.Battery)
        {
            icon = currentDrawer.customIcon ? currentDrawer.customIcon : batterySprite;
            text = "Battery";
            targetAlpha = 1f;

            // Make battery bigger
            itemIcon.rectTransform.sizeDelta = new Vector2(2000f, 2000f); // tweak if needed

            float s = 1f; // ignore scale when using sizeDelta
            targetScale = new Vector3(s, s, 1f);
        }

        // else: lootType == Empty -> keep empty sprite, alpha 0, no text

        if (itemIcon)
        {
            itemIcon.sprite = icon;

            // apply alpha
            var c = itemIcon.color;
            c.a = targetAlpha;
            itemIcon.color = c;

            // apply scale
            itemIcon.rectTransform.localScale = targetScale;
        }

        if (label)
        {
            label.text = text;
        }
    }

    // called by DrawerAnimEvents at end of the open animation
    public void OnDrawerOpenAnimFinished()
    {
        openAnimFinished = true;

        // pause world while drawer is open / scare plays
        Time.timeScale = 0f;

        // roll for a scare
        if (!scarePlaying && scareImage && Random.value < scareChance)
        {
            StartCoroutine(ScareRoutine());
        }
        else
        {
            ShowContents();
        }
    }

    void ShowContents()
    {
        if (itemIcon) itemIcon.enabled = true;
        if (label) label.enabled = true;
    }

    System.Collections.IEnumerator ScareRoutine()
    {
        scarePlaying = true;

        // bring scare image on top
        scareImage.gameObject.SetActive(true);
        var c = scareImage.color;
        c.a = 0f;
        scareImage.color = c;

        if (scareAudio && scareClip)
            scareAudio.PlayOneShot(scareClip);

        // fade in
        for (float t = 0f; t < scareFadeTime; t += Time.unscaledDeltaTime)
        {
            c.a = Mathf.Lerp(0f, 1f, t / scareFadeTime);
            scareImage.color = c;
            yield return null;
        }
        c.a = 1f;
        scareImage.color = c;

        // hold
        float remain = scareHoldTime;
        while (remain > 0f)
        {
            remain -= Time.unscaledDeltaTime;
            yield return null;
        }

        // fade out
        for (float t = 0f; t < scareFadeTime; t += Time.unscaledDeltaTime)
        {
            c.a = Mathf.Lerp(1f, 0f, t / scareFadeTime);
            scareImage.color = c;
            yield return null;
        }
        c.a = 0f;
        scareImage.color = c;
        scareImage.gameObject.SetActive(false);

        scarePlaying = false;

        // finally show the drawer contents
        ShowContents();
    }

    // Take / Close buttons unchanged
    public void OnClickTake()
    {
        if (!isOpen || !openAnimFinished || scarePlaying) return;
        if (!currentDrawer || !currentInteractor) return;

        bool tookSomething = false;

        switch (currentDrawer.lootType)
        {
            case DrawerLootType.Key:
                var inv = currentInteractor.GetComponentInParent<PlayerInventory>();
                if (inv && !inv.HasKey)
                {
                    inv.GiveKey();
                    tookSomething = true;
                }
                break;

            case DrawerLootType.Battery:
                var fl = currentInteractor.GetComponentInParent<Flashlight>();
                if (fl && !fl.IsFull())
                {
                    fl.RefillToMax();
                    tookSomething = true;
                }
                break;
        }

        if (tookSomething)
        {
            currentDrawer.ClearLoot();
            RefreshVisuals();
        }

        CloseDrawer();
    }

    public void OnClickClose()
    {
        if (!isOpen || !openAnimFinished || scarePlaying) return;
        CloseDrawer();
    }
}
