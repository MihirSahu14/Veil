using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class InstructionsFlow : MonoBehaviour
{
    [Header("UI Images")]
    public GameObject storyImage;        // drag Canvas/StoryImage
    public GameObject instructionsImage; // drag Canvas/InstructionsImage

    [Header("Next Scene")]
    public string nextSceneName = "GameScene";

    enum Phase { Story, Controls }
    Phase phase = Phase.Story;

    // We require a clean key release before accepting the next press.
    bool armedForPress = true;   // ready to accept Enter?
    
    void Awake()
    {
        if (storyImage) storyImage.SetActive(true);
        if (instructionsImage) instructionsImage.SetActive(false);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        bool anyDown    = kb.enterKey.isPressed || kb.numpadEnterKey.isPressed;
        bool pressedNow = kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;

        // Arm only after the key is fully released
        if (!anyDown && !armedForPress) armedForPress = true;

        if (armedForPress && pressedNow)
        {
            if (phase == Phase.Story)
            {
                // Switch to controls image, but disarm until user RELEASES the key
                phase = Phase.Controls;
                if (storyImage) storyImage.SetActive(false);
                if (instructionsImage) instructionsImage.SetActive(true);
                armedForPress = false; // wait for key-up before we accept next press
            }
            else // Phase.Controls
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
