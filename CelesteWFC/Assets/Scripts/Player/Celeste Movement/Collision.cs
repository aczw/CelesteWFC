using UnityEngine;

/// <summary>
///     Basically copy and pasted from <see href="https://github.com/mixandjam/Celeste-Movement">this repository</see>.
///     Just to
///     reiterate, none of this code is original.
/// </summary>
public class Collision : MonoBehaviour
{
    public LayerMask platformLayer;

    public bool onGround;
    public bool onWall;
    public bool onRightWall;
    public bool onLeftWall;

    public float collisionRadius;
    public Vector2 bottomOffset, rightOffset, leftOffset;

    private void Update() {
        onGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, collisionRadius, platformLayer);
        onWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, platformLayer)
                 || Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, platformLayer);

        onRightWall =
            Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, platformLayer);
        onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, platformLayer);
    }
}