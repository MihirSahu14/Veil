using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ThunderManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Full-screen Image used for lightning flashes. Alpha must start at 0.")]
    public Image thunderFlash;      // Canvas/ThunderFlash

    [Header("Darkness Control")]
    [Tooltip("Reference to the player's Flashlight script so we can toggle the Film overlay.")]
    public Flashlight flashlight;   // drag Player's Flashlight here

    [Header("Audio (optional)")]
    public AudioSource audioSource; // optional
    public AudioClip thunderClip;   // optional

    [Header("Timing")]
    [Tooltip("Minimum seconds between thunder flashes.")]
    public float minDelay = 6f;

    [Tooltip("Maximum seconds between thunder flashes.")]
    public float maxDelay = 18f;

    [Tooltip("Seconds to ramp up brightness.")]
    public float flashUpTime = 0.08f;

    [Tooltip("Seconds to fade back down.")]
    public float flashDownTime = 0.4f;

    [Range(0f, 1f)]
    [Tooltip("Peak alpha of the white flash.")]
    public float maxAlpha = 0.8f;

    void Awake()
    {
        if (thunderFlash)
        {
            var c = thunderFlash.color;
            c.a = 0f;
            thunderFlash.color = c;
        }
    }

    void Start()
    {
        if (!thunderFlash)
        {
            Debug.LogWarning("[ThunderManager] No thunderFlash Image assigned.");
            return;
        }

        StartCoroutine(ThunderLoop());
    }

    IEnumerator ThunderLoop()
    {
        while (true)
        {
            // wait random time in real seconds (ignore timeScale)
            float wait = Random.Range(minDelay, maxDelay);
            yield return new WaitForSecondsRealtime(wait);

            // 1–2 flickers feels good
            int flashes = Random.Range(1, 3);
            for (int i = 0; i < flashes; i++)
            {
                yield return SingleFlash();
                yield return new WaitForSecondsRealtime(Random.Range(0.05f, 0.2f));
            }

            if (audioSource && thunderClip)
                audioSource.PlayOneShot(thunderClip);
        }
    }

    IEnumerator SingleFlash()
    {
        if (!thunderFlash) yield break;

        // ---- 1) Turn OFF the black Film so player sees the whole room ----
        bool hadFlashlight = flashlight != null;
        bool hadFilm = hadFlashlight && flashlight.filmImage != null;
        bool prevFilmEnabled = false;

        if (hadFilm)
        {
            prevFilmEnabled = flashlight.filmImage.enabled;
            flashlight.filmImage.enabled = false;   // remove the black overlay
        }

        // ---- 2) White flash on top for drama ----
        // fade up
        float t = 0f;
        while (t < flashUpTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / flashUpTime);
            SetAlpha(Mathf.Lerp(0f, maxAlpha, k));
            yield return null;
        }

        // fade down
        t = 0f;
        while (t < flashDownTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / flashDownTime);
            SetAlpha(Mathf.Lerp(maxAlpha, 0f, k));
            yield return null;
        }

        SetAlpha(0f);

        // ---- 3) Restore Film state so darkness comes back ----
        if (hadFilm)
        {
            flashlight.filmImage.enabled = prevFilmEnabled;
        }
    }

    void SetAlpha(float a)
    {
        var c = thunderFlash.color;
        c.a = a;
        thunderFlash.color = c;
    }
}
