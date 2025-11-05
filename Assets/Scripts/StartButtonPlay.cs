using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButtonPlay : MonoBehaviour
{
    [Header("Animator on ART")]
    [SerializeField] private Animator animator;          // assign Animator from Art (child)
    [SerializeField] private string triggerName = "Pressed";

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Anim length (seconds)")]
    [SerializeField] private float pressAnimDuration = 0.6f; // set to your Start_Press clip length

    bool clicked = false;

    public void OnStartClicked()
    {
        if (clicked) return;
        clicked = true;

        if (animator) animator.SetTrigger(triggerName);
        StartCoroutine(LoadAfter());
    }

    IEnumerator LoadAfter()
    {
        yield return new WaitForSecondsRealtime(pressAnimDuration);
        SceneManager.LoadScene(gameSceneName);
    }
}
