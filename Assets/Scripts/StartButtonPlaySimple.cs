using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButtonPlaySimple : MonoBehaviour
{
    [Header("Animator on ART")]
    [SerializeField] private Animator animator;          // drag your Art object's Animator
    [SerializeField] private string triggerName = "Pressed";

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "InstructionsScene";

    [Header("Anim length (seconds)")]
    [SerializeField] private float pressAnimDuration = 0.8f; // set to your press animation length

    private bool clicked = false;

    public void OnStartClicked()
    {
        if (clicked) return;
        clicked = true;

        if (animator)
            animator.SetTrigger(triggerName);

        StartCoroutine(LoadAfter());
    }

    private IEnumerator LoadAfter()
    {
        yield return new WaitForSecondsRealtime(pressAnimDuration);
        SceneManager.LoadScene(gameSceneName);
    }
}
