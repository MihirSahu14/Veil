using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Scan Settings")]
    [Tooltip("Radius to search for interactables around the player.")]
    public float scanRadius = 1.25f;

    [Tooltip("Only colliders on these layers will be considered interactable candidates.")]
    public LayerMask interactableMask;   // Set this to your 'Interactable' layer in Inspector

    [Header("Selection")]
    [Tooltip("Max distance to accept a candidate (useful if scan radius is large).")]
    public float maxPickDistance = 3f;

    [Header("Debug")]
    public bool showGizmo = true;

    private readonly List<IInteractable> _candidates = new List<IInteractable>();
    private IInteractable _current;

    private const int BufferSize = 32;
    private readonly Collider2D[] _hits = new Collider2D[BufferSize];

    void Update()
    {
        FindCandidates();
        PickNearest();
    }

    // Input System bindings (Send Messages)
    public void OnInteract()                                { TryInteract("OnInteract()"); }
    public void OnInteract(InputValue value)                { if (value.isPressed) TryInteract("OnInteract(InputValue)"); }
    public void OnInteract(InputAction.CallbackContext ctx) { if (ctx.performed)  TryInteract("OnInteract(CallbackContext)"); }

    private void TryInteract(string from)
    {
        Debug.Log($"[Interactor] Interact via {from}. Current={_current != null}");
        if (_current != null && _current.CanInteract(gameObject))
            _current.Interact(gameObject);
    }

    private void FindCandidates()
    {
        _candidates.Clear();

        // --- Modern replacement using ContactFilter2D ---
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(interactableMask);
        filter.useTriggers = true;

        int count = Physics2D.OverlapCircle(transform.position, scanRadius, filter, _hits);

        for (int i = 0; i < count; i++)
        {
            var c = _hits[i];
            if (!c) continue;

            var interactable = c.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            if (interactable.CanInteract(gameObject) && !_candidates.Contains(interactable))
                _candidates.Add(interactable);
        }
    }

    private void PickNearest()
    {
        float best = float.PositiveInfinity;
        IInteractable pick = null;

        foreach (var cand in _candidates)
        {
            var mb = cand as MonoBehaviour;
            if (!mb) continue;

            float d2 = (mb.transform.position - transform.position).sqrMagnitude;
            if (d2 < best && d2 <= maxPickDistance * maxPickDistance)
            {
                best = d2;
                pick = cand;
            }
        }

        if (_current != pick)
        {
            _current = pick;
            if (_current != null)
                Debug.Log($"[Interactor] Now targeting: {_current}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}
