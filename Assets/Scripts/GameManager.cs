// Assets/Scripts/GameManager.cs
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;   // for Keyboard.current

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Screens")]
    public GameObject winScreen;     // Canvas/WinScreen (disabled at start)
    public GameObject loseScreen;    // Canvas/LoseScreen (disabled at start)

    [Header("Config")]
    public string startScreenSceneName = "StartScene";

    public bool HasKey { get; private set; }

    public event Action<bool> KeyChanged;

    bool _ended;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (winScreen)  winScreen.SetActive(false);
        if (loseScreen) loseScreen.SetActive(false);

        _ended = false;
        HasKey = false;
        KeyChanged?.Invoke(HasKey);
    }

    void Update()
    {
        // only listen for R after win or lose
        if (_ended && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartToStart();
        }
    }

    public void GiveKey()
    {
        HasKey = true;
        KeyChanged?.Invoke(true);
        Debug.Log("🔑 Key obtained");
    }

    public void ConsumeKey()
    {
        HasKey = false;
        KeyChanged?.Invoke(false);
    }

    public void Win()
    {
        if (_ended) return;
        _ended = true;
        if (winScreen) winScreen.SetActive(true);
        Debug.Log("🎉 ESCAPED!");
    }

    public void Lose()
    {
        if (_ended) return;
        _ended = true;
        if (loseScreen) loseScreen.SetActive(true);
        Debug.Log("💀 YOU DIED!");
    }

    // works for both end states (Win or Lose)
    public void RestartToStart()
    {
        HasKey = false;
        _ended = false;
        KeyChanged?.Invoke(false);

        // Try the configured name first
        try
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(startScreenSceneName);
        }
        catch
        {
            // Fallback: first scene in Build Settings
            Debug.LogWarning($"Start scene '{startScreenSceneName}' not in Build Settings. Falling back to index 0.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }


    // helper for external scripts (like RestartOnKey)
    public bool IsGameEnded() => _ended;
}
