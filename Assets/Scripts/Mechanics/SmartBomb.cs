using UnityEngine;
using Platformer.Mechanics;
using System.Collections.Generic;

public class SmartBomb : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float collectionRadius = 10f; // Alternative to screen-based collection
    [SerializeField] private bool useScreenBounds = true;  // If true, uses screen bounds instead of radius
    [SerializeField] private float screenBoundsExpansion = 0.1f; // 30% expansion of screen bounds
    [SerializeField] private ParticleSystem collectionEffect;
    [SerializeField] private AudioClip collectionSound;
    
 

    private Camera mainCamera;
    private PlayerController player;

    private void Start()
    {
        mainCamera = Camera.main;
        player = FindFirstObjectByType<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectItems();
            Destroy(gameObject);
        }
    }

    private void CollectItems()
    {
        if (useScreenBounds)
        {
            // Get screen bounds in world space
            Vector2 screenMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector2 screenMax = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));
            
            // Calculate expanded bounds
            float width = screenMax.x - screenMin.x;
            float height = screenMax.y - screenMin.y;
            Vector2 expandedMin = new Vector2(screenMin.x - (width * screenBoundsExpansion), screenMin.y - (height * screenBoundsExpansion));
            Vector2 expandedMax = new Vector2(screenMax.x + (width * screenBoundsExpansion), screenMax.y + (height * screenBoundsExpansion));
            
            // Collect all items in lists
            var tokens = new List<TokenInstance>(FindObjectsByType<TokenInstance>(FindObjectsSortMode.None));
            var jewels = new List<JewelCollectable>(FindObjectsByType<JewelCollectable>(FindObjectsSortMode.None));
            var lives = new List<LivesCollectable>(FindObjectsByType<LivesCollectable>(FindObjectsSortMode.None));

            int collectedTokens = 0;
            int collectedJewels = 0;
            int collectedLives = 0;

            // Process tokens
            foreach (var token in tokens)
            {
                if (!token.gameObject.activeInHierarchy) continue;
                
                Vector2 tokenPos = token.transform.position;
                if (tokenPos.x >= expandedMin.x && tokenPos.x <= expandedMax.x &&
                    tokenPos.y >= expandedMin.y && tokenPos.y <= expandedMax.y)
                {
                    token.OnTriggerEnter2D(player.GetComponent<Collider2D>());
                    if (collectionEffect != null)
                    {
                        var effect = Instantiate(collectionEffect, token.transform.position, Quaternion.identity);
                        Destroy(effect.gameObject, effect.main.duration);
                    }
                    collectedTokens++;
                }
            }

            // Process jewels
            foreach (var jewel in jewels)
            {
                if (!jewel.gameObject.activeInHierarchy) continue;
                
                Vector2 jewelPos = jewel.transform.position;
                if (jewelPos.x >= expandedMin.x && jewelPos.x <= expandedMax.x &&
                    jewelPos.y >= expandedMin.y && jewelPos.y <= expandedMax.y)
                {
                    jewel.OnTriggerEnter2D(player.GetComponent<Collider2D>());
                    if (collectionEffect != null)
                    {
                        var effect = Instantiate(collectionEffect, jewel.transform.position, Quaternion.identity);
                        Destroy(effect.gameObject, effect.main.duration);
                    }
                    collectedJewels++;
                }
            }

            // Process lives
            foreach (var life in lives)
            {
                if (!life.gameObject.activeInHierarchy) continue;
                
                Vector2 lifePos = life.transform.position;
                if (lifePos.x >= expandedMin.x && lifePos.x <= expandedMax.x &&
                    lifePos.y >= expandedMin.y && lifePos.y <= expandedMax.y)
                {
                    life.OnTriggerEnter2D(player.GetComponent<Collider2D>());
                    if (collectionEffect != null)
                    {
                        var effect = Instantiate(collectionEffect, life.transform.position, Quaternion.identity);
                        Destroy(effect.gameObject, effect.main.duration);
                    }
                    collectedLives++;
                }
            }
        }
        else
        {
            // Use radius-based collection instead
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, collectionRadius);

            foreach (var collider in colliders)
            {
                if (!collider.gameObject.activeInHierarchy) continue;

                var token = collider.GetComponent<TokenInstance>();
                if (token != null)
                {
                    token.OnTriggerEnter2D(player.GetComponent<Collider2D>());
                    if (collectionEffect != null)
                    {
                        var effect = Instantiate(collectionEffect, token.transform.position, Quaternion.identity);
                        Destroy(effect.gameObject, effect.main.duration);
                    }
                    continue;
                }

                var jewel = collider.GetComponent<JewelCollectable>();
                if (jewel != null)
                {
                    jewel.OnTriggerEnter2D(player.GetComponent<Collider2D>());
                    if (collectionEffect != null)
                    {
                        var effect = Instantiate(collectionEffect, jewel.transform.position, Quaternion.identity);
                        Destroy(effect.gameObject, effect.main.duration);
                    }
                    continue;
                }

                var life = collider.GetComponent<LivesCollectable>();
                if (life != null)
                {
                    life.OnTriggerEnter2D(player.GetComponent<Collider2D>());
                    if (collectionEffect != null)
                    {
                        var effect = Instantiate(collectionEffect, life.transform.position, Quaternion.identity);
                        Destroy(effect.gameObject, effect.main.duration);
                    }
                }
            }
        }

        // Play collection sound
        if (collectionSound != null && player.audioSource != null)
        {
            player.audioSource.PlayOneShot(collectionSound);
        }

        // Trigger camera shake
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeCamera();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (useScreenBounds)
        {
            // Draw screen bounds in editor
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector2 screenMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
                Vector2 screenMax = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));
                
                // Calculate expanded bounds
                float width = screenMax.x - screenMin.x;
                float height = screenMax.y - screenMin.y;
                Vector2 expandedMin = new Vector2(screenMin.x - (width * screenBoundsExpansion), screenMin.y - (height * screenBoundsExpansion));
                Vector2 expandedMax = new Vector2(screenMax.x + (width * screenBoundsExpansion), screenMax.y + (height * screenBoundsExpansion));
                
                // Draw original bounds in yellow
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(new Vector3(screenMin.x, screenMin.y, 0), new Vector3(screenMax.x, screenMin.y, 0));
                Gizmos.DrawLine(new Vector3(screenMax.x, screenMin.y, 0), new Vector3(screenMax.x, screenMax.y, 0));
                Gizmos.DrawLine(new Vector3(screenMax.x, screenMax.y, 0), new Vector3(screenMin.x, screenMax.y, 0));
                Gizmos.DrawLine(new Vector3(screenMin.x, screenMax.y, 0), new Vector3(screenMin.x, screenMin.y, 0));

                // Draw expanded bounds in green
                Gizmos.color = Color.green;
                Gizmos.DrawLine(new Vector3(expandedMin.x, expandedMin.y, 0), new Vector3(expandedMax.x, expandedMin.y, 0));
                Gizmos.DrawLine(new Vector3(expandedMax.x, expandedMin.y, 0), new Vector3(expandedMax.x, expandedMax.y, 0));
                Gizmos.DrawLine(new Vector3(expandedMax.x, expandedMax.y, 0), new Vector3(expandedMin.x, expandedMax.y, 0));
                Gizmos.DrawLine(new Vector3(expandedMin.x, expandedMax.y, 0), new Vector3(expandedMin.x, expandedMin.y, 0));
            }
        }
        else
        {
            // Draw collection radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, collectionRadius);
        }
    }
#endif
} 