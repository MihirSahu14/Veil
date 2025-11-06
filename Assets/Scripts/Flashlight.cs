using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    // ===================== UI (Canvas) =====================
    [Header("UI (Canvas Film)")]
    [Tooltip("Your normal dark overlay that follows the player. Will be disabled during a flash.")]
    public Image filmImage;                 // Canvas/Film (Image)  → gets disabled during burst

    [Tooltip("Idle sprite for Film when not flashing. Optional if Film has its own animator.")]
    public Sprite idleSprite;

    [Tooltip("FULL-SCREEN white image used for a quick flash. Alpha must start at 0.")]
    public Image whiteFlash;               // Canvas/WhiteFlash (Image), color alpha = 0 at start

    [Header("Used Film (a separate Image on Canvas placed ABOVE Film)")]
    [Tooltip("A second Image on the Canvas that shows the bright 'used flashlight' art.")]
    public Image usedFilm;                 // Canvas/UsedFilm (Image). Enabled during burst only.

    [Header("Battery HUD (optional)")]
    public BatteryHUD batteryHUD;

    [Tooltip("Directional 'used' sprites (Up/Down/Left/Right).")]
    public Sprite usedUp;
    public Sprite usedDown;
    public Sprite usedLeft;
    public Sprite usedRight;

    // ===================== Charges & Timing =====================
    [Header("Charges & Timing")]
    [Range(0, 4)] public int charges = 4;
    public int maxCharges = 4;
    [Tooltip("Delay before you can use Flashlight again.")]
    public float cooldown = 0.6f;

    [Tooltip("How long the bright UsedFilm should stay on the screen.")]
    public float usedFilmDuration = 10f;

    [Tooltip("White flash fade-in time.")]
    public float whiteUp = 0.08f;
    [Tooltip("White flash fade-out time.")]
    public float whiteDown = 0.12f;

    // ===================== Stun Area (forward box) =====================
    [Header("Stun Area (forward box)")]
    public LayerMask enemyMask;            // set to Enemy layer
    [Tooltip("How far in front of the player the stun box is placed (world units).")]
    public float boxForwardOffset = 0.6f;
    [Tooltip("Length of the box along aim direction.")]
    public float boxLength = 1.0f;
    [Tooltip("Width of the box perpendicular to aim direction.")]
    public float boxWidth = 0.6f;
    [Tooltip("How long enemies remain stunned.")]
    public float stunDuration = 30f;

    // ===================== Debug =====================
    [Header("Debug")]
    public bool debugDraw = false;

    [Header("Optional shared aim")]
    public PlayerFacing2D playerFacing;   // drag Player here if you want to share AimDir
    public bool preferPlayerAim = true;   // set true to use PlayerFacing2D

    // --------------- Internals ---------------
    bool canFlash = true;
    Vector3 filmBaseScale = Vector3.one;

    void Start()
    {
        if (filmImage) filmBaseScale = filmImage.rectTransform.localScale;

        // Ensure white flash is hidden at start
        if (whiteFlash)
        {
            var c = whiteFlash.color; c.a = 0f; whiteFlash.color = c;
        }

        // Ensure UsedFilm is hidden at start
        if (usedFilm) usedFilm.enabled = false;

        // Optional: set film idle sprite
        if (filmImage && idleSprite) filmImage.sprite = idleSprite;

        // Auto-find BatteryHUD if not set
#if UNITY_2023_1_OR_NEWER
        if (!batteryHUD) batteryHUD = FindAnyObjectByType<BatteryHUD>();
#else
        if (!batteryHUD) batteryHUD = FindObjectOfType<BatteryHUD>();
#endif

        // Initialize HUD display
        if (batteryHUD) batteryHUD.Set(charges, maxCharges);
    }

    // ---------- Input System "Send Messages" ----------
    public void OnFlashlight()                                { TryFlash(); }
    public void OnFlashlight(InputValue value)                { if (value.isPressed) TryFlash(); }
    public void OnFlashlight(InputAction.CallbackContext ctx) { if (ctx.performed)   TryFlash(); }
    // ---------------------------------------------------

    void TryFlash()
    {
        if (!canFlash || charges <= 0)
        {
            Debug.Log("⚠️ Flash blocked (cooldown or no charges).");
            return;
        }

        charges = Mathf.Max(0, charges - 1);
        batteryHUD?.Set(charges, maxCharges);

        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        canFlash = false;

        // 1) Quick white flash (visual)
        if (whiteFlash)
        {
            for (float t = 0f; t < whiteUp; t += Time.unscaledDeltaTime)
            {
                SetWhiteAlpha(Mathf.Lerp(0f, 1f, t / whiteUp));
                yield return null;
            }
            SetWhiteAlpha(1f);

            for (float t = 0f; t < whiteDown; t += Time.unscaledDeltaTime)
            {
                SetWhiteAlpha(Mathf.Lerp(1f, 0f, t / whiteDown));
                yield return null;
            }
            SetWhiteAlpha(0f);
        }

        // 2) Show UsedFilm overlay (and hide Film)
        if (usedFilm)
        {
            usedFilm.sprite = PickDirectionalSprite();
            usedFilm.enabled = true;
        }
        if (filmImage) filmImage.enabled = false;

        // 3) Keep stunning enemies while UsedFilm active
        float remain = usedFilmDuration;
        float tick = 0f;
        const float STUN_TICK = 0.15f;

        while (remain > 0f)
        {
            if (tick <= 0f)
            {
                StunEnemiesForwardBox();
                tick = STUN_TICK;
            }
            tick -= Time.unscaledDeltaTime;
            remain -= Time.unscaledDeltaTime;
            yield return null;
        }

        // 4) Hide UsedFilm and restore Film
        if (usedFilm) usedFilm.enabled = false;
        if (filmImage)
        {
            filmImage.enabled = true;
            filmImage.rectTransform.localScale = filmBaseScale;
            if (idleSprite) filmImage.sprite = idleSprite;
        }

        // 5) Cooldown
        yield return new WaitForSeconds(cooldown);
        canFlash = true;
    }

    // ---------------- helpers ----------------
    void SetWhiteAlpha(float a)
    {
        if (!whiteFlash) return;
        var c = whiteFlash.color; c.a = a; whiteFlash.color = c;
    }

    Sprite PickDirectionalSprite()
    {
        Vector2 dir = GetAimDir();
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x >= 0f ? usedRight : usedLeft;
        else
            return dir.y >= 0f ? usedUp : usedDown;
    }

    Vector2 GetAimDir()
    {
        if (preferPlayerAim && playerFacing != null)
            return playerFacing.AimDir;

        if (Camera.main && Mouse.current != null)
        {
            Vector2 m = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 d = m - (Vector2)transform.position;
            if (d.sqrMagnitude > 0.0001f) return d.normalized;
        }
        return Vector2.right;
    }

    void StunEnemiesForwardBox()
    {
        Vector2 forward = GetAimDir();
        Vector2 center = (Vector2)transform.position + forward.normalized * boxForwardOffset;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center,
            new Vector2(boxLength, boxWidth),
            Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg,
            enemyMask
        );

        foreach (var h in hits)
        {
            var s = h.GetComponent<IStunnable>();
            if (s != null)
            {
                s.Stun(stunDuration);
                Debug.Log($"😵 Stunned {h.name} for {stunDuration}s");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!debugDraw) return;

        Vector2 forward = GetAimDir();
        Vector2 center = (Vector2)transform.position + forward.normalized * boxForwardOffset;
        float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;

        Gizmos.color = Color.yellow;
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(boxLength, boxWidth, 0.01f));
        Gizmos.matrix = prev;
    }

    // ---------------- external API ----------------
    public bool IsFull() => charges >= maxCharges;

    public void RefillToMax()
    {
        charges = maxCharges;
        batteryHUD?.Set(charges, maxCharges);
    }
}

// Enemies must implement this to be stunned
public interface IStunnable
{
    void Stun(float duration);
}
