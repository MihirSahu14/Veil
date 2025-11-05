using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;   // NEW Input System

public class InstructionsScreen : MonoBehaviour
{
    [SerializeField] string nextSceneName = "GameScene";

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // Works with New Input System
        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
