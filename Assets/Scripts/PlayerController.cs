using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public Vector2 Aim { get; private set; } = Vector2.right;

    private Rigidbody2D rb;
    private Vector2 move;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    public void OnMove(InputValue v) => move = v.Get<Vector2>();

    void Update()
    {
        // aim to mouse, fallback to last move dir
        var m = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        var dir = (Vector2)(m - transform.position);
        if (dir.sqrMagnitude > 0.001f) Aim = dir.normalized;
        else if (move.sqrMagnitude > 0.001f) Aim = move.normalized;
    }

    void FixedUpdate() => rb.linearVelocity = move * moveSpeed;
}