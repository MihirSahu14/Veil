// PauseForwarder.cs  (put on Player)
using UnityEngine;
public class PauseForwarder : MonoBehaviour
{
    public GameState gameState;
    public void OnPause() { if (gameState) gameState.OnPause(); }
}
