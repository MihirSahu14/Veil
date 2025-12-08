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

    [Header("Sprites (fallback if no Animator)")]
    public Sprite faceUp;
    public Sprite faceDown;
    public Sprite faceSide;     // use one side sprite and flipX for R/L (recommended)
    public Sprite faceLeft;     // optional: if you prefer distinct L/R, fill both
    public Sprite faceRight;    // optional: otherwise leave null and we’ll use faceSide

    [Header("Animator (optional)")]
    [Tooltip("If assigned, we’ll drive these parameters instead of swapping sprites.")]
    public Animator animator;
    [Tooltip("Float parameter for movement speed (0..1).")]
    public string speedParam = "Speed";
    [Tooltip("Int parameter for 4-way direction (0=Right, 1=Left, 2=Up, 3=Down).")]
    public string dirParam = "Dir4";

    [Header("Tuning")]
    [Tooltip("How much larger one axis must be to win the facing choice.")]
    public float axisBias = 0.1f;

    public Vector2 AimDir { get; private set; } = Vector2.right;

    SpriteRenderer sr;
    Camera cam;
    int dirIndex;       // 0=R,1=L,2=U,3=D
    float moveSpeed01;  // 0..1 normalized magnitude from lastMove

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

        // 2) convert to 4-way + dir index
        Vector2 dAbs = new Vector2(Mathf.Abs(AimDir.x), Mathf.Abs(AimDir.y));
        bool horizontal = dAbs.x > dAbs.y + axisBias;

        bool usingAnimator = animator != null;

        if (horizontal)
        {
            // RIGHT vs LEFT
            if (AimDir.x >= 0f)
            {
                dirIndex = 0; // Right
                if (!usingAnimator)
                {
                    if (faceRight) { sr.sprite = faceRight; sr.flipX = false; }
                    else if (faceSide) { sr.sprite = faceSide; sr.flipX = false; }
                    else if (faceLeft) { sr.sprite = faceLeft; sr.flipX = true; }
                }
            }
            else
            {
                dirIndex = 1; // Left
                if (!usingAnimator)
                {
                    if (faceLeft) { sr.sprite = faceLeft; sr.flipX = false; }
                    else if (faceSide) { sr.sprite = faceSide; sr.flipX = true; }
                    else if (faceRight) { sr.sprite = faceRight; sr.flipX = true; }
                }
            }
        }
        else
        {
            // UP vs DOWN
            if (!usingAnimator) sr.flipX = false;
            if (AimDir.y >= 0f)
            {
                dirIndex = 2; // Up
                if (!usingAnimator) sr.sprite = faceUp ? faceUp : sr.sprite;
            }
            else
            {
                dirIndex = 3; // Down
                if (!usingAnimator) sr.sprite = faceDown ? faceDown : sr.sprite;
            }
        }

        // 3) Drive animator if present
        if (animator)
        {
            if (!string.IsNullOrEmpty(speedParam))
                animator.SetFloat(speedParam, moveSpeed01);
            if (!string.IsNullOrEmpty(dirParam))
                animator.SetInteger(dirParam, dirIndex);
        }
    }

    // Optional: call this from your movement script using Input System "Move" vector
    public void SetMoveInput(Vector2 move)
    {
        if (move.sqrMagnitude > 0.0001f)
        {
            lastMove = move;
            moveSpeed01 = Mathf.Clamp01(move.magnitude); // normalized for animator Speed
        }
        else
        {
            moveSpeed01 = 0f;
        }
    }
}
