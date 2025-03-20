using UnityEngine;
using Platformer.Mechanics;
using System.Collections.Generic;

public class SpringPlatform : MonoBehaviour
{
    [Header("Spring Settings")]
    [SerializeField] private float bounceForce = 20f;
    [SerializeField] private float compressionDuration = 0.2f;
    [SerializeField] private float expansionDuration = 0.1f;
    [SerializeField] private float temporaryInvulnerabilityDuration = 0.5f;
    
    [Header("Animation")]
    [SerializeField] private float compressedScale = 0.5f;
    [SerializeField] private AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private float originalHeight;
    private bool isCompressing = false;
    private bool isExpanding = false;
    private float animationTime = 0f;
    private PlayerController currentPlayer = null;
    private bool isProcessingCollision = false;

    // Static list to track all spring platforms
    private static List<SpringPlatform> allSpringPlatforms = new List<SpringPlatform>();

    private void OnEnable()
    {
        allSpringPlatforms.Add(this);
    }

    private void OnDisable()
    {
        allSpringPlatforms.Remove(this);
    }

    // Static method to check if a position is safe for respawn
    public static bool IsSafeForRespawn(Vector2 position, float checkRadius = 1f)
    {
        foreach (var spring in allSpringPlatforms)
        {
            if (Vector2.Distance(position, spring.transform.position) < checkRadius)
            {
                return false;
            }
        }
        return true;
    }

    private void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;
        originalHeight = GetComponent<Collider2D>().bounds.size.y;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCompressing || isExpanding || isProcessingCollision) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            // Check if the collision is from above
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f) // Player is above the spring
                {
                    isProcessingCollision = true;
                    currentPlayer = player;
                    
                    // Make player invulnerable
                    if (!player.isInvulnerable)
                    {
                        player.StartCoroutine(TemporaryInvulnerability());
                    }

                    // Handle stomp state
                    if (player.isStomping)
                    {
                        player.isStomping = false;
                        bounceForce *= 1.5f; // Give extra bounce force when stomping
                        
                        // Ensure player is above the platform
                        Vector3 playerPos = player.transform.position;
                        playerPos.y = transform.position.y + GetComponent<Collider2D>().bounds.size.y;
                        player.transform.position = playerPos;
                    }
                    
                    StartCompression();
                    break;
                }
            }
        }
    }

    private void StartCompression()
    {
        isCompressing = true;
        isExpanding = false;
        animationTime = 0f;
    }

    private void StartExpansion()
    {
        isCompressing = false;
        isExpanding = true;
        animationTime = 0f;

        if (currentPlayer != null)
        {
            // Apply bounce force
            currentPlayer.Bounce(bounceForce);
            currentPlayer = null;
        }
    }

    private System.Collections.IEnumerator TemporaryInvulnerability()
    {
        currentPlayer.isInvulnerable = true;
        yield return new WaitForSeconds(temporaryInvulnerabilityDuration);
        if (currentPlayer != null)
        {
            currentPlayer.isInvulnerable = false;
        }
    }

    private void Update()
    {
        if (isCompressing)
        {
            animationTime += Time.deltaTime;
            float t = animationTime / compressionDuration;
            
            float scale = Mathf.Lerp(1f, compressedScale, bounceCurve.Evaluate(t));
            ScaleFromBottom(scale);
            
            if (animationTime >= compressionDuration)
            {
                StartExpansion();
            }
        }
        else if (isExpanding)
        {
            animationTime += Time.deltaTime;
            float t = animationTime / expansionDuration;
            
            float scale = Mathf.Lerp(compressedScale, 1f, bounceCurve.Evaluate(t));
            ScaleFromBottom(scale);
            
            if (animationTime >= expansionDuration)
            {
                isExpanding = false;
                transform.localScale = originalScale;
                transform.position = originalPosition;
                isProcessingCollision = false;
            }
        }
    }

    private void ScaleFromBottom(float scaleY)
    {
        transform.localScale = new Vector3(originalScale.x, originalScale.y * scaleY, originalScale.z);
        
        float heightDifference = originalHeight * (1 - scaleY);
        Vector3 newPosition = originalPosition;
        newPosition.y = originalPosition.y - heightDifference * 0.5f;
        transform.position = newPosition;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (currentPlayer != null)
        {
            // Draw the bounce prediction line
            Vector2 start = currentPlayer.transform.position;
            float bounceDistance = bounceForce * Time.fixedDeltaTime * 2f;
            Vector2 end = start + Vector2.up * bounceDistance;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, end);
            
            // Draw the player bounds at the peak
            Gizmos.color = Color.green;
            Vector2 boundsSize = currentPlayer.Bounds.size;
            Gizmos.DrawWireCube(end, boundsSize);
        }
    }
#endif
} 