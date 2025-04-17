﻿using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using System.Collections;

namespace Platformer.Mechanics
{
    public class PlayerController : MonoBehaviour
    {

        [Header("Movement")]
        public float baseSpeed = 7f;           // Starting speed
        public float maxSpeed = 15f;           // Maximum speed cap
        public float speedIncreaseRate = 0.1f; // How much to increase speed per second
        public float speedModifier = 3f;       // How much player can modify their speed
        public float returnToBaseSpeed = 2f;   // How fast speed returns to base speed
        private float currentBaseSpeed;        // Current base running speed that increases over time
        private float temporarySpeedModifier;  // Temporary speed modification from player input
        public float currentSpeed             // Current actual speed (baseSpeed + temporary modifier)
        {
            get { return currentBaseSpeed + temporarySpeedModifier; }
            private set { /* Empty setter to maintain compatibility */ }
        }
        public float jumpForce = 7f;
        public float doubleJumpForce = 7f;
        public float stompForce = 15f;         // Force applied during stomp
        public float stompDuration = 0.3f;     // Duration of the stomp
        
        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.1f;
        public LayerMask groundLayer;

        [Header("Debug")]
        public bool showDebugInfo = false;  // Added for debugging

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


        [Header("Invulnerability")]
        public bool isInvulnerable = false;
        public float invulnerabilityDuration = 3f;
        public float flashStartInterval = 0.5f;
        public float flashEndInterval = 0.1f;
        private float currentFlashInterval;
        private float lastFlashTime;
        private float invulnerabilityEndTime;
        private Coroutine invulnerabilityCoroutine;
        private int defaultLayer;
        private int invulnerableLayer;

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
        private bool isDead;
        public bool isStomping;
        private float stompEndTime;
        private string currentAnimation;
        private float gameStartTime;
        private readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        // Animation States       
        private const string PLAYER_IDLE = "BabyDragonIdle";
        private const string PLAYER_RUN = "BabyDragonWalk";
        private const string PLAYER_JUMP = "BabyDragonJump";
        private const string PLAYER_DOUBLEJUMP = "BabyDragonDoubleJump";
        private const string PLAYER_FALL = "BabyDragonLand_0";
        private const string PLAYER_SPRINT = "BabyDragonSprint";
        private const string PLAYER_DIE = "BabyDragonDie";
        private const string PLAYER_STOMP = "Stomp"; 

        [Header("Effects")]
        public GameObject doubleJumpEffectPrefab; // Reference to your particle effect prefab
        public GameObject waterSplashEffectPrefab; // Reference to the water splash particle effect prefab
        public GameObject stompImpactEffectPrefab; // Reference to the stomp impact particle effect prefab
        public Vector2 waterSplashOffset = Vector3.zero; // Offset for adjusting water splash spawn position
        public Vector2 stompImpactOffset = Vector3.zero; // Offset for adjusting stomp impact spawn position

        [Header("Touch Controls")]
        public float minSwipeDistance = 50f;        // Minimum distance for a swipe to be detected
        public float maxTapTime = 0.2f;             // Maximum time for a tap to be registered

        // Touch control variables
        private Vector2 touchStart;
        private float touchStartTime;
        private bool isSwiping;

        private void Awake()
        {
            
            defaultLayer = gameObject.layer;
            invulnerableLayer = LayerMask.NameToLayer("InvulnerablePlayer");
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            // Setup Rigidbody2D for platform movement
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Initialize starting speeds
            currentBaseSpeed = baseSpeed;
            temporarySpeedModifier = 0f;
            gameStartTime = Time.time;
            
            // Register to scene loaded event
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            // Unregister from scene loaded event
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        // Reset player state when a new scene is loaded
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            ResetPlayerState();
        }
        
        // Reset all player state variables
        public void ResetPlayerState()
        {
            
            
            // Reset movement state
            currentBaseSpeed = baseSpeed;
            temporarySpeedModifier = 0f;
            gameStartTime = Time.time;
            
            // Reset jump state
            jumpState = JumpState.Grounded;
            isGrounded = false;
            canDoubleJump = false;
            hasDoubleJumped = false;
            
            // Reset stomp state
            isStomping = false;
            
            // Reset death state
            ResetDeathState();
            
            // Reset health
            if (health != null)
            {
                health.ResetHealth();
            }
            
            // Enable control
            controlEnabled = true;
            
            // Reset position if needed
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            
            // Make sure player is active
            gameObject.SetActive(true);
            
            // Reset animation
            ChangeAnimationState(PLAYER_IDLE);
        }

        private void Update()
        {
            if (isDead) return;

            if (controlEnabled)
            {
                // Handle touch input
                HandleTouchInput();

                // Keep existing keyboard input for testing/cross-platform
                if (Input.GetButtonDown("Jump"))
                {
                    HandleJumpInput();
                }

                // Stomp ability (now handled in touch input and keeping keyboard for testing)
                if (!isGrounded && !isStomping && Input.GetAxisRaw("Vertical") < -0.5f)
                {
                    StartStomp();
                }

                // Gradually increase base speed over time
                if (currentBaseSpeed < maxSpeed)
                {
                    currentBaseSpeed += speedIncreaseRate * Time.deltaTime;
                    currentBaseSpeed = Mathf.Min(currentBaseSpeed, maxSpeed);
                }

                // Return temporary speed modifier back to 0
                if (temporarySpeedModifier > 0)
                {
                    temporarySpeedModifier = Mathf.Max(0, temporarySpeedModifier - returnToBaseSpeed * Time.deltaTime);
                }
                else if (temporarySpeedModifier < 0)
                {
                    temporarySpeedModifier = Mathf.Min(0, temporarySpeedModifier + returnToBaseSpeed * Time.deltaTime);
                }
            }

            // Animation
            UpdateAnimationState();
            UpdateJumpState();

            // Debug visualization of ground check
            if (showDebugInfo)
            {
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
// Check if stomp has ended
if (isStomping && Time.time >= stompEndTime)
{
    isStomping = false;
    if (showDebugInfo) Debug.Log("Stomp ended");
}

// Movement
if (controlEnabled)
{
    // Get horizontal input (-1 for left, 1 for right)
    float horizontalInput = Input.GetAxisRaw("Horizontal");
    
    // Calculate speed modification based on input
    float speedModification = horizontalInput * speedModifier;
    
    // Apply speed modification to current speed
    float targetSpeed = currentSpeed + speedModification;
    
    // Ensure speed doesn't go below half of base speed or above maxSpeed
    targetSpeed = Mathf.Clamp(targetSpeed, baseSpeed * 0.5f, maxSpeed);
    
    Vector2 targetVelocity;
    
    if (isStomping)
    {
        // During stomp, maintain vertical velocity and stop horizontal movement
        targetVelocity = new Vector2(0, -stompForce);
    }
    else
    {
        targetVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
    }

    // Check if colliding with a wall
    bool againstWall = CheckWallCollision();
    if (againstWall && GetWallDirection() > 0) // Only check right wall for endless runner
    {
        targetVelocity.x = 0f;
    }

    rb.linearVelocity = targetVelocity;
}

// Always face right for endless runner
spriteRenderer.flipX = false;
            spriteRenderer.flipX = false;
        }

        private bool CheckWallCollision()
        {
            Vector2 boxSize = new Vector2(0.1f, collider2d.bounds.size.y * 0.8f);
            Vector2 rightCheck = (Vector2)transform.position + Vector2.right * (collider2d.bounds.extents.x + 0.05f);
            Vector2 leftCheck = (Vector2)transform.position - Vector2.right * (collider2d.bounds.extents.x + 0.05f);

            Collider2D[] rightHits = Physics2D.OverlapBoxAll(rightCheck, boxSize, 0f, groundLayer);
            Collider2D[] leftHits = Physics2D.OverlapBoxAll(leftCheck, boxSize, 0f, groundLayer);

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

        private float GetWallDirection()
        {
            Vector2 boxSize = new Vector2(0.1f, collider2d.bounds.size.y * 0.8f);
            Vector2 rightCheck = (Vector2)transform.position + Vector2.right * (collider2d.bounds.extents.x + 0.05f);

            Collider2D[] rightHits = Physics2D.OverlapBoxAll(rightCheck, boxSize, 0f, groundLayer);

            foreach (var hit in rightHits)
            {
                if (hit.CompareTag("Obstacle"))
                {
                    return 1f;  // Wall on the right
                }
            }

            return 0f;  // No wall detected
        }

          private IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;
            gameObject.layer = invulnerableLayer;
            Physics2D.IgnoreLayerCollision(invulnerableLayer, LayerMask.NameToLayer("Enemy"), true);
            invulnerabilityEndTime = Time.time + invulnerabilityDuration;
            currentFlashInterval = flashStartInterval;
            lastFlashTime = Time.time;

            while (Time.time < invulnerabilityEndTime)
            {
                float timeLeft = invulnerabilityEndTime - Time.time;
                float progress = 1 - (timeLeft / invulnerabilityDuration);
                currentFlashInterval = Mathf.Lerp(flashStartInterval, flashEndInterval, progress);

                if (Time.time - lastFlashTime >= currentFlashInterval)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                    lastFlashTime = Time.time;
                }
                yield return null;
            }
           
            spriteRenderer.enabled = true;
           
            Physics2D.IgnoreLayerCollision(invulnerableLayer, LayerMask.NameToLayer("Enemy"), false);
            gameObject.layer = defaultLayer;
             isInvulnerable = false;
        }
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (audioSource && jumpAudio)
                audioSource.PlayOneShot(jumpAudio);
        }

        private void DoubleJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
            
            // Play double jump audio
            if (audioSource && doubleJumpAudio)
                audioSource.PlayOneShot(doubleJumpAudio);
            else if (audioSource && jumpAudio)
                audioSource.PlayOneShot(jumpAudio);
            
            // Spawn the double jump effect
            if (doubleJumpEffectPrefab != null)
            {
                // Calculate a position slightly in front of the player based on their velocity
                // This compensates for the player's forward movement
                Vector3 effectPosition = groundCheck.position + new Vector3(currentSpeed * 0.1f, 0, 0);
                
                // Instantiate the effect
                GameObject effect = Instantiate(doubleJumpEffectPrefab, effectPosition, Quaternion.identity);
                
                // Make sure the effect was created successfully
                if (effect != null)
                {
                    // Get the particle system component
                    ParticleSystem particles = effect.GetComponent<ParticleSystem>();
                    if (particles != null)
                    {
                        // Make the particle system inherit the player's velocity
                        var mainModule = particles.main;
                        var inheritVelocity = particles.inheritVelocity;
                        
                        // If the particle system has an inherit velocity module, use it
                        if (inheritVelocity.enabled)
                        {
                            inheritVelocity.mode = ParticleSystemInheritVelocityMode.Initial;
                            // Set a multiplier that looks good for your game
                            inheritVelocity.curveMultiplier = 0.5f;
                        }
                        else
                        {
                            // Otherwise, try to add some initial velocity to the particles
                            var velocityOverLifetime = particles.velocityOverLifetime;
                            if (velocityOverLifetime.enabled)
                            {
                                velocityOverLifetime.x = currentSpeed * 0.5f;
                            }
                        }
                        
                        // Play the particle system
                        particles.Play();
                        
                        // Destroy the game object after the particle system has finished
                        float duration = particles.main.duration + particles.main.startLifetime.constantMax;
                        Destroy(effect, duration);
                    }
                    else
                    {
                        // If no particle system found, destroy after a default time
                        Destroy(effect, 2f);
                    }
                }
            }
        }

        private void StartStomp()
        {
            isStomping = true;
            stompEndTime = Time.time + stompDuration;
            ChangeAnimationState(PLAYER_STOMP);
        }

        void UpdateAnimationState()
        {
            if (isStomping)
            {
                ChangeAnimationState(PLAYER_STOMP);
                return;
            }

            if (!isGrounded)
            {
                if (rb.linearVelocity.y > 0)
                {
                    // Use double jump animation if player has double jumped
                    if (hasDoubleJumped)
                        ChangeAnimationState(PLAYER_DOUBLEJUMP);
                    else
                        ChangeAnimationState(PLAYER_JUMP);
                }
                else
                    ChangeAnimationState(PLAYER_FALL);
                return;
            }

            // Always show running animation when grounded, use sprint for higher speeds
            ChangeAnimationState(currentSpeed > baseSpeed ? PLAYER_SPRINT : PLAYER_RUN);
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
            if (isInvulnerable) return;
            
            isDead = true;
            animator.SetBool("dead", true);
            
            // Check if the death animation is playing
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(PLAYER_DIE))
            {
                ChangeAnimationState(PLAYER_DIE);
            }
            
            rb.linearVelocity = Vector2.zero;
            hasDoubleJumped = false;
            canDoubleJump = false;
            currentSpeed = baseSpeed;
        }

        public void ResetDeathState()
        {
            isDead = false;
            currentAnimation = "";
            hasDoubleJumped = false;
            canDoubleJump = false;
            currentSpeed = baseSpeed;

            if (invulnerabilityCoroutine != null)
                StopCoroutine(invulnerabilityCoroutine);

            invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine());
        }

        public void Bounce(float force)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
            // Enable double jump after bouncing
            canDoubleJump = true;
            hasDoubleJumped = false;
        }

        public void Bounce(Vector2 direction)
        {
            rb.linearVelocity = new Vector2(currentSpeed, direction.y);
            // Enable double jump after bouncing
            canDoubleJump = true;
            hasDoubleJumped = false;
        }

        public void Teleport(Vector3 position)
        {
            rb.position = position;
            rb.linearVelocity = new Vector2(currentSpeed, 0);
            hasDoubleJumped = false;
            canDoubleJump = false;
        }

        void UpdateJumpState()
        {
            if (isGrounded)
                jumpState = JumpState.Grounded;
            else if (rb.linearVelocity.y > 0)
                jumpState = JumpState.InFlight;
            else if (rb.linearVelocity.y < 0)
                jumpState = JumpState.Falling;
        }

         private void OnGUI()
        {
            if (showDebugInfo)
            {
                // Add these to your existing OnGUI
                GUI.Label(new Rect(10, 150, 200, 20), $"Invulnerable: {isInvulnerable}");
                if (isInvulnerable)
                {
                    GUI.Label(new Rect(10, 170, 200, 20), $"Invuln Time Left: {invulnerabilityEndTime - Time.time:F1}s");
                    GUI.Label(new Rect(10, 190, 200, 20), $"Flash Interval: {currentFlashInterval:F3}s");
                }
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (isInvulnerable && collision.gameObject.CompareTag("Enemy")) return;
            
            // Check if this is a landing collision after a stomp
            if (isStomping && collision.contacts.Length > 0)
            {
                // Get the contact point
                ContactPoint2D contact = collision.contacts[0];
                
                // Spawn stomp impact effect
                if (stompImpactEffectPrefab != null)
                {
                    // Calculate spawn position with offset
                    Vector3 spawnPosition = contact.point + (Vector2)stompImpactOffset;
                    
                    // Instantiate the effect
                    GameObject effect = Instantiate(stompImpactEffectPrefab, spawnPosition, Quaternion.identity);
                    
                    // Get the particle system component
                    ParticleSystem particles = effect.GetComponent<ParticleSystem>();
                    if (particles != null)
                    {
                        // Play the particle system
                        particles.Play();
                        
                        // Destroy the game object after the particle system has finished
                        float duration = particles.main.duration + particles.main.startLifetime.constantMax;
                        Destroy(effect, duration);
                    }
                    else
                    {
                        // If no particle system found, destroy after a default time
                        Destroy(effect, 2f);
                    }
                }
            }

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
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            }
        }

        void OnCollisionStay2D(Collision2D collision)
        {
              if (isInvulnerable && collision.gameObject.CompareTag("Enemy")) return;
            if (isDead) return;

            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y > 0.5f)
                    {
                        isGrounded = true;
                        return;
                    }
                }
                isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        touchStart = touch.position;
                        touchStartTime = Time.time;
                        isSwiping = false;
                        break;

                    case TouchPhase.Moved:
                        if (!isSwiping)
                        {
                            float swipeDistance = Vector2.Distance(touch.position, touchStart);
                            if (swipeDistance > minSwipeDistance)
                            {
                                isSwiping = true;
                                Vector2 swipeDirection = touch.position - touchStart;

                                // Determine swipe direction
                                float angle = Mathf.Atan2(swipeDirection.y, swipeDirection.x) * Mathf.Rad2Deg;

                                if (angle > -45 && angle < 45) // Right swipe
                                {
                                    temporarySpeedModifier = speedModifier;
                                }
                                else if (angle > 135 || angle < -135) // Left swipe
                                {
                                    temporarySpeedModifier = -speedModifier;
                                }
                                else if (angle < -45 && angle > -135) // Down swipe
                                {
                                    if (!isGrounded && !isStomping)
                                    {
                                        StartStomp();
                                    }
                                }
                            }
                        }
                        break;

                    case TouchPhase.Ended:
                        float touchDuration = Time.time - touchStartTime;
                        if (!isSwiping && touchDuration < maxTapTime)
                        {
                            // Handle jump and double jump with single taps
                            if (isGrounded)
                            {
                                Jump();
                                canDoubleJump = true;
                                hasDoubleJumped = false;
                                if (showDebugInfo) Debug.Log("First Jump performed");
                            }
                            else if (!isGrounded && canDoubleJump && !hasDoubleJumped)
                            {
                                DoubleJump();
                                hasDoubleJumped = true;
                                canDoubleJump = false;
                                if (showDebugInfo) Debug.Log("Double Jump performed");
                            }
                        }
                        break;
                }
            }
        }

        private void HandleJumpInput()
        {
            if (isGrounded)
            {
                Jump();
                canDoubleJump = true;
                hasDoubleJumped = false;
                
                if (showDebugInfo) Debug.Log("First Jump performed");
            }
            else if (!isGrounded && canDoubleJump && !hasDoubleJumped)
            {
                DoubleJump();
                hasDoubleJumped = true;
                canDoubleJump = false;
                
                if (showDebugInfo) Debug.Log("Double Jump performed");
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