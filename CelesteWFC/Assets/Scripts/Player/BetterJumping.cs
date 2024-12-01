using UnityEngine;

/// <summary>
///     Adapted from <see href="https://www.youtube.com/watch?v=7KiK0Aqtmzc">this YouTube video</see>.
/// </summary>
public class BetterJumping : MonoBehaviour
{
    public float fallMultiplier;
    public float lowJumpMultiplier;

    private Rigidbody2D rb;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        if (rb.linearVelocityY < 0) {
            rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime);
        }
        else if (rb.linearVelocityY > 0 && !Input.GetButton("Jump")) {
            rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.deltaTime);
        }
    }
}