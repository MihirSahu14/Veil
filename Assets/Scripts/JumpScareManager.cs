using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class JumpScareManager : MonoBehaviour
{
    [Header("UI")]
    public Image jumpScareImage;      // Canvas/JumpScare (Image)

    [Header("Audio (optional)")]
    public AudioSource audioSource;   // Optional
    public AudioClip screamClip;      // Optional

    [Header("Timing")]
    public float fadeTime = 0.35f;    // fade-in speed
    public float holdTime = 1.5f;     // how long to show before reset

    bool playing;

    void Awake()
    {
        // Ensure overlay is invisible and time is normal at scene start
        if (jumpScareImage)
        {
            var c = jumpScareImage.color;
            c.a = 0f;
            jumpScareImage.color = c;
            jumpScareImage.gameObject.SetActive(true);
        }
        Time.timeScale = 1f;
        playing = false;
    }

    public void Trigger()
    {
        if (!playing) StartCoroutine(PlayCo());
    }

    IEnumerator PlayCo()
    {
        playing = true;

        if (screamClip && audioSource)
            audioSource.PlayOneShot(screamClip);

        // Fade in the jumpscare image
        if (jumpScareImage)
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

        // Freeze gameplay (UI still animates with unscaled time)
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(holdTime);

        // Restore and reload scene for MVP
        Time.timeScale = 1f;
        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
    }
}
