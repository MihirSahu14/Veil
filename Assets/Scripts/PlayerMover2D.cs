using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMover2D : MonoBehaviour
{
    public float speed = 4f;
    [Header("Footsteps (optional, loop clip)")]
    [Tooltip("AudioSource with a multi-step loop clip. Will play/stop based on movement.")]
    public AudioSource footstepSource;
    public AudioClip footstepClip;
    [Tooltip("Base pitch for the loop (1 = normal).")]
    public float footstepBasePitch = 1f;
    public float minSpeedForStep = 0.1f;
    [Range(0f, 0.2f)] public float pitchJitter = 0.05f;

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

        HandleFootsteps();
    }

    // Input System (Send Messages) handler
    public void OnMove(UnityEngine.InputSystem.InputValue v)
    {
        move = v.Get<Vector2>();
    }

    void HandleFootsteps()
    {
        if (!footstepSource || !footstepClip) return;

        if (GameManager.Instance && GameManager.Instance.IsGameEnded())
        {
            if (footstepSource.isPlaying) footstepSource.Stop();
            return;
        }

        float mag = rb ? rb.linearVelocity.magnitude : move.magnitude;
        bool shouldPlay = mag >= minSpeedForStep;

        if (shouldPlay)
        {
            if (!footstepSource.isPlaying)
            {
                if (pitchJitter > 0f)
                    footstepSource.pitch = footstepBasePitch + Random.Range(-pitchJitter, pitchJitter);
                else
                    footstepSource.pitch = footstepBasePitch;

                footstepSource.clip = footstepClip;
                footstepSource.loop = true;
                footstepSource.Play();
            }
        }
        else
        {
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }
}
