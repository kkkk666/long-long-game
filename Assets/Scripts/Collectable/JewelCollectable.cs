using UnityEngine;
using Platformer.Mechanics;

public class JewelCollectable : MonoBehaviour
{
    public float floatAmplitude = 0.5f;
    public Vector3 originalPosition;
   
    [SerializeField] private CoinFlyAnimation coinFlyAnimation;
    [SerializeField] private ParticleSystem jewelParticles;

    void Start()
    {
        originalPosition = transform.position;
        
        // Debug checks for 2D collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogError($"No Collider2D found on {gameObject.name}!");
        }
        else
        {
            if (!collider.isTrigger)
            {
                Debug.LogError($"Collider2D on {gameObject.name} is not set as a trigger!");
            }
        }

        if (coinFlyAnimation == null)
        {
            coinFlyAnimation = FindFirstObjectByType<CoinFlyAnimation>();
            if (coinFlyAnimation == null)
            {
                Debug.LogError("CoinFlyAnimation not found in scene!");
            }
        }
    }

    void Update()
    {
        transform.position = new Vector3(
            originalPosition.x,
            originalPosition.y + Mathf.Sin(Time.time) * floatAmplitude,
            originalPosition.z
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.gameObject.GetComponent<PlayerController>();
        if (player == null) return;

        // Spawn particles if assigned
        if (jewelParticles != null)
        {
            ParticleSystem particles = Instantiate(jewelParticles, transform.position, Quaternion.identity);
            Destroy(particles.gameObject, particles.main.duration);
        }

        // Add jewel if ScoreManager exists
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddJewel();
        }

        // Trigger jewel animation if available
        if (coinFlyAnimation != null)
        {
            coinFlyAnimation.StartJewelFlyAnimation(transform.position);
        }

        // Destroy the jewel pickup
        Destroy(gameObject);
    }

    // Modified for 2D visualization
    void OnDrawGizmos()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            // Draw wire circle for CircleCollider2D
            if (collider is CircleCollider2D)
            {
                CircleCollider2D circle = (CircleCollider2D)collider;
                Gizmos.color = Color.cyan; // Changed to cyan to distinguish from other collectables
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, 
                    circle.radius * Mathf.Max(transform.localScale.x, transform.localScale.y));
            }
        }
    }
} 