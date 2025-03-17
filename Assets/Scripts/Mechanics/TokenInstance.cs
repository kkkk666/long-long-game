using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This class contains the data required for implementing token collection mechanics.
    /// It does not perform animation of the token, this is handled in a batch by the 
    /// TokenController in the scene.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TokenInstance : MonoBehaviour
    {
        public AudioClip tokenCollectAudio;
        [Tooltip("If true, animation will start at a random position in the sequence.")]
        public bool randomAnimationStartTime = false;
        [Tooltip("List of frames that make up the animation.")]
        public Sprite[] idleAnimation, collectedAnimation;
        [SerializeField] private ParticleSystem collectionEffect;
        [SerializeField] private CoinFlyAnimation coinFlyAnimation;
        [SerializeField] private int pointValue = 10;

        internal Sprite[] sprites = new Sprite[0];
        internal SpriteRenderer _renderer;
        internal int tokenIndex = -1;
        internal TokenController controller;
        internal int frame = 0;
        internal bool collected = false;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (randomAnimationStartTime)
                frame = Random.Range(0, sprites.Length);
            sprites = idleAnimation;

            if (coinFlyAnimation == null)
            {
                coinFlyAnimation = FindFirstObjectByType<CoinFlyAnimation>();
                if (coinFlyAnimation == null)
                {
                    Debug.LogError("CoinFlyAnimation not found in scene!");
                }
            }
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.gameObject.GetComponent<PlayerController>();
            if (player == null) return;

            if (collected) return;

            // Spawn particles if assigned
            if (collectionEffect != null)
            {
                ParticleSystem particles = Instantiate(collectionEffect, transform.position, Quaternion.identity);
                Destroy(particles.gameObject, particles.main.duration);
            }

            // Play collection sound
            if (tokenCollectAudio != null)
            {
                AudioSource.PlayClipAtPoint(tokenCollectAudio, transform.position);
            }

            // Update animation
            frame = 0;
            sprites = collectedAnimation;
            if (controller != null)
                collected = true;

            // Trigger coin animation if available
            if (coinFlyAnimation != null)
            {
                coinFlyAnimation.StartCoinFlyAnimation(transform.position);
            }

            // Add score
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(pointValue);
            }

            // Trigger the collection event
            var ev = Schedule<PlayerTokenCollision>();
            ev.token = this;
            ev.player = player;

            // Destroy the token
            Destroy(gameObject);
        }
    }
}