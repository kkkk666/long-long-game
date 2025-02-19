using UnityEngine;

public class BitcoinCollectable : MonoBehaviour
{
    public float floatAmplitude = 0.5f;
    public Vector3 originalPosition;
   
    [SerializeField] private CoinFlyAnimation coinFlyAnimation;
    public ParticleSystem BitcoinParticles;
    [SerializeField] private int pointValue = 10;

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

    // Changed to 2D version
    private void OnCollisionEnter2D(Collision2D collision)
    {
       // Debug.Log($"OnCollisionEnter2D detected with: {collision.gameObject.name}");
    }

    // Changed to 2D version
    private void OnTriggerEnter2D(Collider2D other)
    {
     

        // Spawn particles if assigned
        if (BitcoinParticles != null)
        {
            ParticleSystem particles = Instantiate(BitcoinParticles, transform.position, Quaternion.identity);
            Destroy(particles.gameObject, particles.main.duration);
        }

        // Add score if ScoreManager exists
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(pointValue);
        }

        // Trigger coin animation if available
        if (coinFlyAnimation != null)
        {
            coinFlyAnimation.StartCoinFlyAnimation(transform.position);
        }

        // Destroy the bitcoin
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
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, 
                    circle.radius * Mathf.Max(transform.localScale.x, transform.localScale.y));
            }
        }
    }
}