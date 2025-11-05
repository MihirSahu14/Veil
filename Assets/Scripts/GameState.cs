// GameState.cs
using UnityEngine;

public class GameState : MonoBehaviour
{
    bool paused;
    public GameObject pauseUI; // optional UI panel

    public void OnPause()
    {
        paused = !paused;
        Time.timeScale = paused ? 0 : 1;
        if (pauseUI) pauseUI.SetActive(paused);
        Debug.Log(paused ? "Paused" : "Unpaused");
    }
}
