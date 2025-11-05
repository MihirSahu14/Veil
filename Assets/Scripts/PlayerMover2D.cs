using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMover2D : MonoBehaviour
{
    public float speed = 4f;
    Rigidbody2D rb;
    Vector2 move;
    PlayerFacing2D facing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        facing = GetComponent<PlayerFacing2D>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = move * speed;
        if (facing) facing.SetMoveInput(move);
    }

    // Input System (Send Messages) handler
    public void OnMove(UnityEngine.InputSystem.InputValue v)
    {
        move = v.Get<Vector2>();
    }
}
