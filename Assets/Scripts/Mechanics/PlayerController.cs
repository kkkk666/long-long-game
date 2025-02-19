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
                float currentSpeed = maxSpeed * (isSprinting ? sprintSpeedMultiplier : 1f);
                rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
            }

            // Sprite Flip
            if (moveInput.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (moveInput.x < -0.01f)
                spriteRenderer.flipX = true;
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
                ChangeAnimationState(isSprinting ? PLAYER_SPRINT : PLAYER_RUN);
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

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("MovingPlatform"))
            {
                transform.SetParent(null);
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