// Interactor.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    public void OnInteract(InputValue v)
    {
        if (!v.isPressed) return;
        Debug.Log("Interact pressed (E) — we’ll hook this to drawers/batteries soon.");
    }
}
