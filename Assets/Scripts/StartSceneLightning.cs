using System.Collections;
using UnityEngine;

/// <summary>
/// Plays a one-shot lightning animation on the start screen at random intervals.
/// Attach this to a persistent object in the StartScene (e.g., StartScreenRoot).
/// Requires an Animator on the lightning Image with a non-looping default state.
/// </summary>
public class StartSceneLightning : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Animator on the lightning Image (non-looping clip as default state).")]
    public Animator lightningAnimator;

    [Tooltip("Lightning Image GameObject to toggle. If null, uses animator's GameObject.")]
    public GameObject lightningObject;

    [Header("Audio (optional)")]
    public AudioSource audioSource;
    public AudioClip thunderClip;

    [Header("Timing (seconds)")]
    [Min(0f)] public float minDelay = 3f;
    [Min(0f)] public float maxDelay = 8f;

    [Tooltip("Approx length of the lightning animation clip (seconds).")]
    [Min(0f)] public float animationDuration = 0.8f;

    Coroutine loop;

    void Awake()
    {
        if (!lightningObject && lightningAnimator)
            lightningObject = lightningAnimator.gameObject;

        if (lightningObject)
            lightningObject.SetActive(false);
    }

    void OnEnable()
    {
        loop = StartCoroutine(LightningLoop());
    }

    void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);
    }

    IEnumerator LightningLoop()
    {
        while (true)
        {
            float wait = Random.Range(minDelay, maxDelay);
            yield return new WaitForSecondsRealtime(wait);

            if (lightningObject) lightningObject.SetActive(true);
            if (lightningAnimator) lightningAnimator.Play(0, 0, 0f);
            if (audioSource && thunderClip) audioSource.PlayOneShot(thunderClip);

            yield return new WaitForSecondsRealtime(animationDuration);

            if (lightningObject) lightningObject.SetActive(false);
        }
    }
}
