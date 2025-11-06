using UnityEngine;

public class BatteryHUD : MonoBehaviour
{
    [Header("Refs")]
    public Flashlight flashlight;   // optional; auto-finds if empty

    [Tooltip("Index = charges (0..4). Put Battery_0 at index 0 ... Battery_4 at index 4.")]
    public GameObject[] icons = new GameObject[5];

    int shown = -1;

    void Awake()
    {
        if (!flashlight)
        {
#if UNITY_2023_1_OR_NEWER
            flashlight = FindAnyObjectByType<Flashlight>();
#else
            flashlight = FindObjectOfType<Flashlight>();
#endif
        }
    }

    void Start() => RefreshFromFlashlight();

    // Works with Set(charges) or Set(charges, maxCharges)
    public void Set(int charges, int maxCharges = 4)
    {
        int clamped = Mathf.Clamp(charges, 0, Mathf.Min(4, maxCharges));
        if (clamped == shown) return;
        shown = clamped;

        for (int i = 0; i < icons.Length; i++)
            if (icons[i]) icons[i].SetActive(i == clamped);
    }

    public void RefreshFromFlashlight()
    {
        if (flashlight) Set(flashlight.charges, flashlight.maxCharges);
    }
}
