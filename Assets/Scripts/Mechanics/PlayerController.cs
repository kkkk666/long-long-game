using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float maxSpeed = 7f;
        public float jumpForce = 7f;
        public float doubleJumpForce = 7f;
        public float sprintSpeedMultiplier = 2.5f;
        
        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.1f;
        public LayerMask groundLayer;

        [Header("Debug")]
        public bool showDebugInfo = true;  // Added for debugging

        [Header("Audio")]
        public AudioClip jumpAudio;
        public AudioClip doubleJumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        [Header("References")]
        public Collider2D collider2d;
        public AudioSource audioSource;
        public Health health;
        public Animator animator;

        // Control and State
        public bool controlEnabled = true;
        public JumpState jumpState = JumpState.Grounded;

        // Properties required by other scripts
        public Vector2 velocity 
        { 
            get => rb.linearVelocity;
            set => rb.linearVelocity = value;
        }
        
        public Bounds Bounds => collider2d.bounds;

        // Private variables
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private bool isGrounded;
        private bool canDoubleJump = false;
        private bool hasDoubleJumped = false;
        private bool isSprinting;
        private Vector2 moveInput;
        private bool isDead;
        private string currentAnimation;
        private readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        // Animation States       
        private const string PLAYER_IDLE = "BabyDragonIdle";
        private const string PLAYER_RUN = "BabyDragonWalk";
        private const string PLAYER_JUMP = "BabyDragonJump";
        private const string PLAYER_FALL = "BabyDragonLand 0";
        private const string PLAYER_SPRINT = "BabyDragonSprint";
        private const string PLAYER_DIE = "BabyDragonDie";

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            // Setup Rigidbody2D for platform movement
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void Update()
        {
            if (isDead) return;

            if (controlEnabled)
            {
                // Input
                moveInput.x = Input.GetAxisRaw("Horizontal");
                isSprinting = Input.GetKey(KeyCode.LeftShift);

                // Handle jumping
                if (Input.GetButtonDown("Jump"))
                {
                    if (isGrounded)
                    {
                        Jump();
                        canDoubleJump = true;  // Enable double jump after initial jump
                        hasDoubleJumped = false;  // Reset double jump state
                        
                        if (showDebugInfo) Debug.Log("First Jump performed");
                    }
                    else if (!isGrounded && canDoubleJump && !hasDoubleJumped)
                    {
                        DoubleJump();
                        hasDoubleJumped = true;
                        canDoubleJump = false;
                        
                        if (showDebugInfo) Debug.Log("Double Jump performed");
                    }
                    else
                    {
                        if (showDebugInfo)
                        {
                            Debug.Log($"Jump failed - Conditions: isGrounded={isGrounded}, canDoubleJump={canDoubleJump}, hasDoubleJumped={hasDoubleJumped}");
                        }
                    }
                }
            }
            else
            {
                moveInput = Vector2.zero;
            }

            // Animation
            UpdateAnimationState();
            UpdateJumpState();

            // Debug visualization of ground check
            if (showDebugInfo)
            {
                // Draw debug lines to show ground check area
                float segments = 16;
                float angleStep = 360f / segments;
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = i * angleStep * Mathf.Deg2Rad;
                    float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
                    
                    Vector2 point1 = groundCheck.position + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1)) * groundCheckRadius;
                    Vector2 point2 = groundCheck.position + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2)) * groundCheckRadius;
                    
                    Debug.DrawLine(point1, point2, isGrounded ? Color.green : Color.red);
            }
        }
        }

  private void FixedUpdate()
{
    if (isDead) return;

    // Ground Check
    bool wasGrounded = isGrounded;
    isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    // Debug ground state changes
    if (showDebugInfo && wasGrounded != isGrounded)
    {
        Debug.Log($"Ground state changed: {wasGrounded} -> {isGrounded}");
    }

    // Reset double jump when landing
    if (isGrounded && !wasGrounded)
    {
        canDoubleJump = false;
        hasDoubleJumped = false;
        if (showDebugInfo) Debug.Log("Reset double jump state on landing");
    }

    // Movement
    if (controlEnabled)
    {
        float currentSpeed = maxSpeed * (isSprinting && isGrounded ? sprintSpeedMultiplier : 1f);
        Vector2 targetVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);

        // Check if colliding with a wall
        bool againstWall = CheckWallCollision();
        if (againstWall && Mathf.Sign(moveInput.x) == Mathf.Sign(GetWallDirection()))
        {
            // If pressing into a wall, don’t apply horizontal velocity
            targetVelocity.x = 0f;
        }

        rb.linearVelocity = targetVelocity;
    }

    

    // Sprite Flip
    if (moveInput.x > 0.01f)
        spriteRenderer.flipX = false;
    else if (moveInput.x < -0.01f)
        spriteRenderer.flipX = true;
}
private bool CheckWallCollision()
{
    // Define the box size and check positions
    Vector2 boxSize = new Vector2(0.1f, collider2d.bounds.size.y * 0.8f);
    Vector2 rightCheck = (Vector2)transform.position + Vector2.right * (collider2d.bounds.extents.x + 0.05f);
    Vector2 leftCheck = (Vector2)transform.position - Vector2.right * (collider2d.bounds.extents.x + 0.05f);

    // Check for colliders on both sides
    Collider2D[] rightHits = Physics2D.OverlapBoxAll(rightCheck, boxSize, 0f, groundLayer);
    Collider2D[] leftHits = Physics2D.OverlapBoxAll(leftCheck, boxSize, 0f, groundLayer);

    // Check if any hit has the "Obstacle" tag
    bool hitRight = false;
    bool hitLeft = false;

    foreach (var hit in rightHits)
    {
        if (hit.CompareTag("Obstacle"))
        {
            hitRight = true;
            break;
        }
    }

    foreach (var hit in leftHits)
    {
        if (hit.CompareTag("Obstacle"))
        {
            hitLeft = true;
            break;
        }
    }

    if (showDebugInfo && (hitRight || hitLeft))
    {
        Debug.Log($"Wall detected - Right: {hitRight}, Left: {hitLeft}");
    }

    return hitRight || hitLeft;
}

// Determine wall direction (1 for right, -1 for left)
private float GetWallDirection()
{
    Vector2 boxSize = new Vector2(0.1f, collider2d.bounds.size.y * 0.8f);
    Vector2 rightCheck = (Vector2)transform.position + Vector2.right * (collider2d.bounds.extents.x + 0.05f);
    Vector2 leftCheck = (Vector2)transform.position - Vector2.right * (collider2d.bounds.extents.x + 0.05f);

    // Check for colliders on both sides
    Collider2D[] rightHits = Physics2D.OverlapBoxAll(rightCheck, boxSize, 0f, groundLayer);
    Collider2D[] leftHits = Physics2D.OverlapBoxAll(leftCheck, boxSize, 0f, groundLayer);

    // Check right side for "Obstacle" tag
    foreach (var hit in rightHits)
    {
        if (hit.CompareTag("Obstacle"))
        {
            return 1f;  // Wall on the right
        }
    }

    // Check left side for "Obstacle" tag
    foreach (var hit in leftHits)
    {
        if (hit.CompareTag("Obstacle"))
        {
            return -1f;  // Wall on the left
        }
    }

    return 0f;  // No wall detected
}

// Optional: Visualize wall checks in the editor
private void OnDrawGizmos()
{
    if (showDebugInfo)
    {
        Vector2 boxSize = new Vector2(0.1f, collider2d.bounds.size.y * 0.8f);
        Vector2 rightCheck = (Vector2)transform.position + Vector2.right * (collider2d.bounds.extents.x + 0.05f);
        Vector2 leftCheck = (Vector2)transform.position - Vector2.right * (collider2d.bounds.extents.x + 0.05f);

        Gizmos.color = Physics2D.OverlapBox(rightCheck, boxSize, 0f, groundLayer) ? Color.red : Color.green;
        Gizmos.DrawWireCube(rightCheck, boxSize);
        Gizmos.color = Physics2D.OverlapBox(leftCheck, boxSize, 0f, groundLayer) ? Color.red : Color.green;
        Gizmos.DrawWireCube(leftCheck, boxSize);
    }
}
        private void Jump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);  // Changed from linearVelocity to velocity
            if (audioSource && jumpAudio)
                audioSource.PlayOneShot(jumpAudio);
        }

        private void DoubleJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);  // Changed from linearVelocity to velocity
            if (audioSource && doubleJumpAudio)
                audioSource.PlayOneShot(doubleJumpAudio);
            else if (audioSource && jumpAudio)
                audioSource.PlayOneShot(jumpAudio);
        }

        public void Bounce(float force)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        }

        public void Bounce(Vector2 direction)
        {
            rb.linearVelocity = direction;
        }

        public void Teleport(Vector3 position)
        {
            rb.position = position;
            rb.linearVelocity = Vector2.zero;
            hasDoubleJumped = false;
            canDoubleJump = false;
        }

        void UpdateJumpState()
        {
            if (isGrounded)
            {
                jumpState = JumpState.Grounded;
            }
            else if (rb.linearVelocity.y > 0)
            {
                jumpState = JumpState.InFlight;
            }
            else if (rb.linearVelocity.y < 0)
            {
                jumpState = JumpState.Falling;
            }
        }

       void UpdateAnimationState()
{
    if (!isGrounded)
    {
        if (rb.linearVelocity.y > 0)
            ChangeAnimationState(PLAYER_JUMP);
        else
            ChangeAnimationState(PLAYER_FALL);
        return;
    }

    if (Mathf.Abs(moveInput.x) > 0f)
    {
        // Only use sprint animation when grounded and sprinting
        ChangeAnimationState(isSprinting && isGrounded ? PLAYER_SPRINT : PLAYER_RUN);
    }
    else
    {
        ChangeAnimationState(PLAYER_IDLE);
    }
}

        void ChangeAnimationState(string newAnimation)
        {
            if (currentAnimation == PLAYER_DIE) return;
            if (currentAnimation == newAnimation) return;
            
            animator.Play(newAnimation);
            currentAnimation = newAnimation;
        }

        public void TriggerDeath()
        {
            isDead = true;
            animator.Play(PLAYER_DIE);
            currentAnimation = PLAYER_DIE;
            rb.linearVelocity = Vector2.zero;
            hasDoubleJumped = false;
            canDoubleJump = false;
        }

        public void ResetDeathState()
        {
            isDead = false;
            currentAnimation = "";
            hasDoubleJumped = false;
            canDoubleJump = false;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("MovingPlatform"))
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y > 0.5f)
                    {
                        transform.SetParent(collision.transform);
                        break;
                    }
                }
            }
        }

       private void OnCollisionExit2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("MovingPlatform"))
    {
        transform.SetParent(null);
    }
    // When leaving collision, recheck grounded state
    if (((1 << collision.gameObject.layer) & groundLayer) != 0)
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
}

        // Optional: Add OnGUI method for real-time debugging
        private void OnGUI()
        {
            if (showDebugInfo)
            {
                GUI.Label(new Rect(10, 10, 200, 20), $"Grounded: {isGrounded}");
                GUI.Label(new Rect(10, 30, 200, 20), $"Can Double Jump: {canDoubleJump}");
                GUI.Label(new Rect(10, 50, 200, 20), $"Has Double Jumped: {hasDoubleJumped}");
                GUI.Label(new Rect(10, 70, 200, 20), $"Velocity Y: {rb.linearVelocity.y:F2}");
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
{
    if (isDead) return;

    // Only consider collisions with ground layer
    if (((1 << collision.gameObject.layer) & groundLayer) != 0)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Check if the collision normal is pointing mostly upward (player is on top)
            if (contact.normal.y > 0.5f) // Normal.y > 0.5 means the surface is below the player
            {
                isGrounded = true;
                return;
            }
        }
        // If no upward normal is found, ensure isGrounded is false unless overlap confirms it
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
}

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Falling,
            Landed
        }
    }
}