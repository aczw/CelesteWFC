using System.Collections;
using UnityEngine;

/// <summary>
///     Basically copy and pasted from <see href="https://github.com/mixandjam/Celeste-Movement">this repository</see>.
///     Just to
///     reiterate, none of this code is original.
/// </summary>
public class Movement : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public float slideSpeed;
    public float wallJumpLerp;
    public float dashSpeed;

    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool isDashing;

    private Rigidbody2D rb;
    private Collision coll;
    private BetterJumping betterJumping;

    private bool groundTouch;
    private bool hasDashed;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collision>();
        betterJumping = GetComponent<BetterJumping>();
    }

    private void Update() {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        var xRaw = Input.GetAxisRaw("Horizontal");
        var yRaw = Input.GetAxisRaw("Vertical");

        var dir = new Vector2(x, y);
        Walk(dir);

        if (coll.onWall && Input.GetButton("Fire3") && canMove) {
            wallGrab = true;
        }

        if (Input.GetButtonUp("Fire3") || !coll.onWall || !canMove) {
            wallGrab = false;
        }

        if (coll.onGround && !isDashing) {
            wallJumped = false;
            betterJumping.enabled = true;
        }

        if (wallGrab && !isDashing) {
            rb.gravityScale = 0f;
            if (x > .2f || x < -.2f) {
                rb.linearVelocityY = 0f;
            }

            var speedModifier = y > 0f ? .5f : 1f;
            rb.linearVelocityY = y * (speed * speedModifier);
        }
        else {
            rb.gravityScale = 3f;
        }

        if (coll.onWall && !coll.onGround) {
            if (x != 0 && !wallGrab) {
                WallSlide();
            }
        }

        if (Input.GetButtonDown("Jump")) {
            if (coll.onGround) {
                Jump(Vector2.up);
            }

            if (coll.onWall && !coll.onGround) {
                WallJump();
            }
        }

        if (Input.GetButtonDown("Fire1") && !hasDashed) {
            if (xRaw != 0 || yRaw != 0) {
                Dash(xRaw, yRaw);
            }
        }

        if (coll.onGround && !groundTouch) {
            GroundTouch();
            groundTouch = true;
        }

        if (!coll.onGround && groundTouch) {
            groundTouch = false;
        }
    }

    private void GroundTouch() {
        hasDashed = false;
        isDashing = false;
    }

    private void Dash(float x, float y) {
        hasDashed = true;

        rb.linearVelocity = Vector2.zero;
        var dir = new Vector2(x, y);

        rb.linearVelocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    private IEnumerator TweenLinearDamping() {
        var elapsed = 0f;
        const float duration = 0.8f;

        while (elapsed <= duration) {
            elapsed += Time.deltaTime;

            var linearDamping = Mathf.Lerp(14f, 0f, elapsed / duration);
            rb.linearDamping = linearDamping;

            yield return null;
        }

        rb.linearDamping = 0f;
    }

    private IEnumerator DashWait() {
        StartCoroutine(GroundDash());
        StartCoroutine(TweenLinearDamping());

        rb.gravityScale = 0;
        betterJumping.enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        rb.gravityScale = 3;
        betterJumping.enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    private IEnumerator GroundDash() {
        yield return new WaitForSeconds(.15f);

        if (coll.onGround) {
            hasDashed = false;
        }
    }

    private void WallJump() {
        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        var wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        Jump(Vector2.up / 1.5f + wallDir / 1.5f);

        wallJumped = true;
    }

    private void WallSlide() {
        if (!canMove) {
            return;
        }

        var pushingWall = false;
        if ((rb.linearVelocity.x > 0 && coll.onRightWall) || (rb.linearVelocity.x < 0 && coll.onLeftWall)) {
            pushingWall = true;
        }

        var push = pushingWall ? 0 : rb.linearVelocity.x;

        rb.linearVelocity = new Vector2(push, -slideSpeed);
    }

    private void Walk(in Vector2 dir) {
        if (!canMove) {
            return;
        }

        if (wallGrab) {
            return;
        }

        if (!wallJumped) {
            rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);
        }
        else {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, new Vector2(dir.x * speed, rb.linearVelocity.y),
                                             wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir) {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity += dir * jumpForce;
    }

    private IEnumerator DisableMovement(float time) {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
}