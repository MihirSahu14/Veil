using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour, IStunnable
{
    public enum State { Idle, Patrol, Chase, Stunned }

    #region Targeting
    [Header("Targeting")]
    public float chaseRange = 7f;
    public float loseSightRange = 9f;
    public float stopRange = 0.6f;
    #endregion

    #region Movement
    [Header("Movement")]
    public float moveSpeed = 2.2f;
    public float patrolSpeed = 1.3f;
    public float patrolRadius = 3.0f;
    public Vector2 patrolChangeEvery = new Vector2(1.2f, 2.5f);
    #endregion

    #region Visuals
    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;     // assign or auto-find
    [Tooltip("Sprite used in normal/non-stunned states. If left empty, the current sprite at Awake() will be used.")]
    public Sprite normalSprite;
    [Tooltip("Sprite to show while stunned (e.g., slumped/eyes closed). Optional.")]
    public Sprite stunnedSprite;
    [Tooltip("Optional tint to apply while stunned (used in addition to stunnedSprite, or instead if no sprite provided).")]
    public Color stunnedTint = new Color(1f, 0.6f, 0.6f, 1f);
    #endregion

    #region Catch / Game Over
    [Header("Catch / Game Over")]
    public JumpScareManager jumpScare;    // assign in Inspector (auto-found as backup)
    public float catchGrace = 1.0f;       // seconds after start before catching allowed
    public float catchDistance = 0.45f;   // extra safety: catch if within this distance
    #endregion

    [Header("Facing")]
    public bool spriteFacesLeftByDefault = true;


    // ---------- internals ----------
    State state = State.Patrol;
    Rigidbody2D rb;
    Transform player;
    Vector3 spawnPos;
    bool stunned;
    float stunUntil;
    Color baseColor;
    Vector2 patrolDir;
    float nextPatrolFlip;
    float canCatchAt;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer)
        {
            baseColor = spriteRenderer.color;
            if (!normalSprite) normalSprite = spriteRenderer.sprite;
        }

        spawnPos = transform.position;
        if (!jumpScare) jumpScare = FindFirstObjectByType<JumpScareManager>();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        ScheduleNextPatrolDir();
        state = State.Patrol;

        canCatchAt = Time.time + catchGrace;
    }

    void FixedUpdate()
    {
        if (stunned) { rb.linearVelocity = Vector2.zero; return; }
        if (!player) { PatrolStep(); return; }

        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Patrol:
                if (dist <= chaseRange) state = State.Chase;
                PatrolStep();
                break;

            case State.Chase:
                if (dist > loseSightRange)
                {
                    state = State.Patrol;
                    PatrolStep();
                    break;
                }
                if (dist <= stopRange)
                {
                    rb.linearVelocity = Vector2.zero;
                    break;
                }
                Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
                rb.linearVelocity = dir * moveSpeed;
                FlipSprite(dir.x);
                break;

            case State.Idle:
            default:
                rb.linearVelocity = Vector2.zero;
                break;
        }

        if (Time.time >= canCatchAt && !stunned && player && dist <= catchDistance)
        {
            TriggerCatch("distance");
        }
    }

    void PatrolStep()
    {
        if (Time.time >= nextPatrolFlip) ScheduleNextPatrolDir();

        Vector2 toSpawn = (spawnPos - transform.position);
        float pull = Mathf.Clamp01(toSpawn.magnitude / patrolRadius);
        Vector2 dir = (patrolDir + toSpawn.normalized * 0.3f * pull).normalized;

        rb.linearVelocity = dir * patrolSpeed;
        FlipSprite(dir.x);
    }

    void ScheduleNextPatrolDir()
    {
        patrolDir = Random.insideUnitCircle.normalized;
        nextPatrolFlip = Time.time + Random.Range(patrolChangeEvery.x, patrolChangeEvery.y);
    }

    void FlipSprite(float x)
    {
        if (!spriteRenderer) return;
        if (Mathf.Abs(x) < 0.05f) return;

        bool movingRight = x > 0f;
        if (!spriteFacesLeftByDefault)
            spriteRenderer.flipX = movingRight;   
        else
            spriteRenderer.flipX = !movingRight;  
    }


    // ====== IStunnable ======
    public void Stun(float duration)
    {
        if (duration <= 0) return;

        if (stunned && Time.time < stunUntil)
        {
            stunUntil = Time.time + duration;
            return;
        }

        StopAllCoroutines();
        StartCoroutine(StunCo(duration));
    }

    IEnumerator StunCo(float duration)
    {
        stunned = true;
        state = State.Stunned;
        stunUntil = Time.time + duration;
        rb.linearVelocity = Vector2.zero;

        if (spriteRenderer)
        {
            if (stunnedSprite) spriteRenderer.sprite = stunnedSprite;
            spriteRenderer.color = stunnedTint;
        }

        while (Time.time < stunUntil)
        {
            rb.linearVelocity = Vector2.zero;
            yield return null;
        }

        if (spriteRenderer)
        {
            if (normalSprite) spriteRenderer.sprite = normalSprite;
            spriteRenderer.color = baseColor;
        }

        stunned = false;
        state = State.Patrol;
        ScheduleNextPatrolDir();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (Time.time < canCatchAt || stunned) return;

        Transform hitRoot = col.collider.attachedRigidbody
            ? col.collider.attachedRigidbody.transform
            : col.collider.transform;

        if (hitRoot && hitRoot.CompareTag("Player"))
        {
            TriggerCatch("collision");
        }
        else
        {
            Debug.Log($"Enemy collided with {col.collider.name} (root: {hitRoot?.name})");
        }
    }

    void TriggerCatch(string reason)
    {
        Debug.Log($"Caught player via {reason}");
        if (jumpScare) jumpScare.Trigger();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;     Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(transform.position, loseSightRange);
        Gizmos.color = Color.yellow;  Gizmos.DrawWireSphere(transform.position, stopRange);
        Gizmos.color = Color.cyan;    Gizmos.DrawWireSphere(transform.position, catchDistance);
    }
}
