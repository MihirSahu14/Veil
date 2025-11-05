using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerFacing2D : MonoBehaviour
{
    public enum AimSource { Mouse, LastMove }          // pick how to aim
    [Header("Aim")]
    public AimSource aimSource = AimSource.Mouse;
    [Tooltip("If using LastMove, feed this from your movement script via SetMoveInput().")]
    public Vector2 lastMove;                            // updated by movement script

    [Header("Sprites")]
    public Sprite faceUp;
    public Sprite faceDown;
    public Sprite faceSide;     // use one side sprite and flipX for R/L (recommended)
    public Sprite faceLeft;     // optional: if you prefer distinct L/R, fill both
    public Sprite faceRight;    // optional: otherwise leave null and we’ll use faceSide

    [Header("Tuning")]
    [Tooltip("How much larger one axis must be to win the facing choice.")]
    public float axisBias = 0.1f;

    public Vector2 AimDir { get; private set; } = Vector2.right;

    SpriteRenderer sr;
    Camera cam;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
    }

    void Update()
    {
        // 1) pick an aim direction
        if (aimSource == AimSource.Mouse && cam && Mouse.current != null)
        {
            Vector2 mouse = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 d = mouse - (Vector2)transform.position;
            if (d.sqrMagnitude > 0.0001f) AimDir = d.normalized;
        }
        else
        {
            if (lastMove.sqrMagnitude > 0.0001f) AimDir = lastMove.normalized;
        }

        // 2) convert to 4-way
        Vector2 dAbs = new Vector2(Mathf.Abs(AimDir.x), Mathf.Abs(AimDir.y));
        bool horizontal = dAbs.x > dAbs.y + axisBias;

        if (horizontal)
        {
            // RIGHT vs LEFT
            if (AimDir.x >= 0f)
            {
                if (faceRight) { sr.sprite = faceRight; sr.flipX = false; }
                else if (faceSide) { sr.sprite = faceSide; sr.flipX = false; }
                else if (faceLeft) { sr.sprite = faceLeft; sr.flipX = true; }
            }
            else
            {
                if (faceLeft) { sr.sprite = faceLeft; sr.flipX = false; }
                else if (faceSide) { sr.sprite = faceSide; sr.flipX = true; }
                else if (faceRight) { sr.sprite = faceRight; sr.flipX = true; }
            }
        }
        else
        {
            // UP vs DOWN
            sr.flipX = false;
            sr.sprite = (AimDir.y >= 0f) ? (faceUp ? faceUp : sr.sprite)
                                         : (faceDown ? faceDown : sr.sprite);
        }
    }

    // Optional: call this from your movement script using Input System "Move" vector
    public void SetMoveInput(Vector2 move)
    {
        if (move.sqrMagnitude > 0.0001f) lastMove = move;
    }
}
