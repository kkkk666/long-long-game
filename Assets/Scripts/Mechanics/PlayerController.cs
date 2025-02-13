using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        public float maxSpeed = 7;
        public float jumpTakeOffSpeed = 7;
        public float sprintSpeedMultiplier = 2.5f;
         private bool wasSprintingBeforeJump;
        private float currentHorizontalSpeed;
        public float airDragMultiplier = 0.98f;
        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        public Collider2D collider2d;
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;
        private bool isDead = false;
        private bool jump;
        private bool isSprinting;
        private bool canSprint;   
        private Vector2 move;
        private SpriteRenderer spriteRenderer;
        internal Animator animator;
        private readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        private string currentAnimation;


        // Animation States       
        private const string PLAYER_IDLE = "BabyDragonIdle";
        private const string PLAYER_RUN = "BabyDragonWalk";
        private const string PLAYER_JUMP = "BabyDragonJump";
        private const string PLAYER_FALL = "BabyDragonLand 0";

        private const string PLAYER_SPRINT = "BabyDragonSprint";
         private const string PLAYER_DIE = "BabyDragonDie";


        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = Input.GetAxis("Horizontal");
                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }

            UpdateJumpState();
            UpdateAnimationState();
            base.Update();
        }

     void UpdateAnimationState()
    {
        if (isDead) return;

        // Only allow sprint state when grounded
        isSprinting = IsGrounded && Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f;

        // Air states take priority
        if (!IsGrounded)
        {
            if (velocity.y > 0)
                ChangeAnimationState(PLAYER_JUMP);
            else
                ChangeAnimationState(PLAYER_FALL);
            return;
        }

        // Ground movement states
        float horizontalInput = Input.GetAxis("Horizontal");
        horizontalInput = Mathf.Round(horizontalInput * 100f) / 100f;

        if (Mathf.Abs(horizontalInput) > 0f)
        {
            ChangeAnimationState(isSprinting ? PLAYER_SPRINT : PLAYER_RUN);
        }
        else
        {
            ChangeAnimationState(PLAYER_IDLE);
        }
    }

        public void ChangeAnimationState(string newAnimation)
    {
        // If currently playing death animation, don't change
        if (currentAnimation == PLAYER_DIE) return;
        
        if (currentAnimation == newAnimation) return;
        
        animator.Play(newAnimation, 0, 0f);
        currentAnimation = newAnimation;
    }

     public void TriggerDeath()
    {
        isDead = true;
        // Play death animation and prevent it from transitioning to any other state
        animator.Play(PLAYER_DIE, 0, 0f);
        currentAnimation = PLAYER_DIE;
    }

        public void ResetDeathState()
    {
        isDead = false;
        currentAnimation = "";  // Reset current animation to allow new animations to play
    }
    void UpdateJumpState()
    {
        jump = false;
        switch (jumpState)
        {
            case JumpState.PrepareToJump:
                jumpState = JumpState.Jumping;
                jump = true;
                stopJump = false;
                break;
            case JumpState.Jumping:
                if (!IsGrounded)
                {
                    Schedule<PlayerJumped>().player = this;
                    jumpState = JumpState.InFlight;
                }
                break;
            case JumpState.InFlight:
                if (IsGrounded)
                {
                    Schedule<PlayerLanded>().player = this;
                    jumpState = JumpState.Landed;
                }
                break;
            case JumpState.Landed:
                jumpState = JumpState.Grounded;
                break;
        }
    }

        protected override void ComputeVelocity()
    {
        // Track if we were sprinting before jumping
        if (IsGrounded)
        {
            wasSprintingBeforeJump = Input.GetKey(KeyCode.LeftShift);
            currentHorizontalSpeed = wasSprintingBeforeJump ? maxSpeed * sprintSpeedMultiplier : maxSpeed;
        }
        else
        {
            // In air, gradually reduce speed if it was higher than normal max speed
            if (Mathf.Abs(currentHorizontalSpeed) > maxSpeed)
            {
                currentHorizontalSpeed *= airDragMultiplier;
                // Don't let it go below normal speed
                currentHorizontalSpeed = Mathf.Max(currentHorizontalSpeed, maxSpeed);
            }
        }

        if (jump && IsGrounded)
        {
            velocity.y = jumpTakeOffSpeed * model.jumpModifier;
            jump = false;
        }
        else if (stopJump)
        {
            stopJump = false;
            if (velocity.y > 0)
            {
                velocity.y = velocity.y * model.jumpDeceleration;
            }
        }

        if (move.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (move.x < -0.01f)
            spriteRenderer.flipX = true;

        targetVelocity = move * currentHorizontalSpeed;
    }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}



