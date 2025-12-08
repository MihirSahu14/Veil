using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JumpScareManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Full-screen Image for the jump scare art.")]
    public Image jumpScareImage;      // Canvas/JumpScare (Image)
    [Tooltip("Optional Animator on the JumpScare Image for frame animation.")]
    public Animator jumpScareAnimator; // optional: plays sprite animation

    [Header("Audio (optional)")]
    public AudioSource audioSource;   // Optional
    public AudioClip screamClip;      // Optional

    [Header("Timing (unscaled time)")]
    [Tooltip("Seconds to fade in the jump scare image.")]
    public float fadeTime = 0.35f;
    [Tooltip("Seconds to keep the image fully visible.")]
    public float holdTime = 1.50f;
    [Tooltip("Seconds to fade out before showing Lose screen. Set 0 for no fade-out.")]
    public float fadeOutTime = 0.25f;

    bool playing;

    void Awake()
    {
        if (!jumpScareAnimator && jumpScareImage)
            jumpScareAnimator = jumpScareImage.GetComponent<Animator>();

        // Start invisible but enabled (so we can control alpha).
        if (jumpScareImage)
        {
            var c = jumpScareImage.color;
            c.a = 0f;
            jumpScareImage.color = c;
            jumpScareImage.gameObject.SetActive(true);
        }
        playing = false;
        // Ensure gameplay is normal at scene start.
        Time.timeScale = 1f;
    }

    /// <summary>Trigger the jump scare sequence (ignored if already playing).</summary>
    public void Trigger()
    {
        if (!playing) StartCoroutine(PlayCo());
    }

    IEnumerator PlayCo()
    {
        playing = true;

        if (jumpScareAnimator)
            jumpScareAnimator.Play(0, 0, 0f); // restart the jump scare animation

        if (audioSource && screamClip)
            audioSource.PlayOneShot(screamClip);

        // Fade IN
        if (jumpScareImage && fadeTime > 0f)
        {
            var c = jumpScareImage.color;
            for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                c.a = Mathf.Lerp(0f, 1f, t / fadeTime);
                jumpScareImage.color = c;
                yield return null;
            }
            c.a = 1f; jumpScareImage.color = c;
        }
        else
        {
            SetAlpha(1f);
        }

        // Pause gameplay (UI keeps animating on unscaled time)
        Time.timeScale = 0f;

        // Hold at full opacity
        float remain = holdTime;
        while (remain > 0f)
        {
            remain -= Time.unscaledDeltaTime;
            yield return null;
        }

        // Optional fade OUT (cosmetic)
        if (fadeOutTime > 0f)
        {
            var c = jumpScareImage ? jumpScareImage.color : Color.white;
            for (float t = 0f; t < fadeOutTime; t += Time.unscaledDeltaTime)
            {
                c.a = Mathf.Lerp(1f, 0f, t / fadeOutTime);
                if (jumpScareImage) jumpScareImage.color = c;
                yield return null;
            }
            SetAlpha(0f);
        }

        // Show Lose screen; keep game paused. GameManager.RestartToStart() should restore timeScale = 1.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Lose();
        }
        else
        {
            Debug.LogWarning("JumpScareManager: No GameManager in scene; unpausing gameplay as a fallback.");
            Time.timeScale = 1f; // fallback if no Lose screen to show
        }

        playing = false;
    }

    void SetAlpha(float a)
    {
        if (!jumpScareImage) return;
        var c = jumpScareImage.color; c.a = a; jumpScareImage.color = c;
    }
}
