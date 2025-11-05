using UnityEngine;

public class PivotAimByVelocity2D : MonoBehaviour
{
    [Header("Reference to the player movement or Rigidbody2D")]
    public Rigidbody2D playerRb;
    public float minSpeedToUpdate = 0.05f;
    Vector2 lastDir = Vector2.right;  // default facing right

    void Reset()
    {
        if (!playerRb) playerRb = GetComponentInParent<Rigidbody2D>();
    }

    void LateUpdate()
    {
        if (!playerRb) return;

        Vector2 v = playerRb.linearVelocity;
        if (v.sqrMagnitude >= minSpeedToUpdate * minSpeedToUpdate)
            lastDir = v.normalized;

        // Pivot’s UP = facing direction
        transform.up = lastDir;
    }
}
