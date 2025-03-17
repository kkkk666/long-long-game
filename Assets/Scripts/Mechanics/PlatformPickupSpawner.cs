using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace Platformer.Mechanics
{
    public class PlatformPickupSpawner : MonoBehaviour
    {
        [System.Serializable]
        public struct PickupType
        {
            public GameObject pickupPrefab;
            [Range(0f, 1f)]
            public float spawnRatio;
            public string name;
        }

        [Header("Pickup Settings")]
        public List<PickupType> pickupTypes = new List<PickupType>();
        
        [Header("Spawn Settings")]
        [Range(0f, 1f)]
        public float noSpawnRatio = 0.3f;  // Chance of spawning nothing
        public float minSpacing = 2f;       // Minimum space between pickups
        public float heightOffset = 1f;     // Height above platform to spawn pickups
        
        private Collider2D platformCollider;
        private Tilemap tilemap;
        private bool hasSpawned = false;

        void Start()
        {
            // Try to get BoxCollider2D first
            platformCollider = GetComponent<BoxCollider2D>();
            
            // If no BoxCollider2D, try TilemapCollider2D
            if (platformCollider == null)
            {
                platformCollider = GetComponent<TilemapCollider2D>();
                tilemap = GetComponent<Tilemap>();
            }
            
            if (platformCollider == null)
            {
                Debug.LogError("No BoxCollider2D or TilemapCollider2D found on " + gameObject.name);
                return;
            }

            SpawnPickups();
        }

        Bounds GetColliderBounds()
        {
            if (tilemap != null)
            {
                // For tilemaps, we need to get the bounds of the actual tiles
                return tilemap.localBounds;
            }
            return platformCollider.bounds;
        }

        void SpawnPickups()
        {
            if (hasSpawned || pickupTypes.Count == 0) return;

            // Get platform bounds
            Bounds bounds = GetColliderBounds();
            float platformWidth = bounds.size.x;
            float leftEdge = bounds.min.x;
            
            if (tilemap != null)
            {
                // Convert local bounds to world space for tilemaps
                leftEdge = transform.TransformPoint(new Vector3(leftEdge, 0, 0)).x;
            }
            
            // Calculate how many pickups can fit with minimum spacing
            int maxPickups = Mathf.FloorToInt(platformWidth / minSpacing);
            if (maxPickups <= 0) return;

            // For each possible pickup position
            for (int i = 0; i < maxPickups; i++)
            {
                // Check if we should spawn nothing based on noSpawnRatio
                if (Random.value < noSpawnRatio) continue;

                // Calculate spawn position
                float xPos = leftEdge + (i + 0.5f) * minSpacing;
                float yPos;
                
                if (tilemap != null)
                {
                    // For tilemaps, we need to raycast to find the actual top surface
                    Vector3 rayStart = new Vector3(xPos, bounds.max.y + 1f, transform.position.z);
                    RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 2f);
                    if (hit.collider != null && hit.collider.gameObject == gameObject)
                    {
                        yPos = hit.point.y + heightOffset;
                    }
                    else
                    {
                        // Skip this position if we can't find the surface
                        continue;
                    }
                }
                else
                {
                    yPos = bounds.max.y + heightOffset;
                }

                Vector3 spawnPos = new Vector3(xPos, yPos, transform.position.z);

                // Select random pickup based on ratios
                float totalRatio = 0f;
                foreach (var pickup in pickupTypes)
                {
                    totalRatio += pickup.spawnRatio;
                }

                float randomValue = Random.value * totalRatio;
                float currentSum = 0f;

                foreach (var pickup in pickupTypes)
                {
                    currentSum += pickup.spawnRatio;
                    if (randomValue <= currentSum)
                    {
                        // Spawn the selected pickup
                        GameObject spawnedPickup = Instantiate(pickup.pickupPrefab, spawnPos, Quaternion.identity);
                        spawnedPickup.transform.parent = transform;
                        break;
                    }
                }
            }

            hasSpawned = true;
        }

        // Optional: Method to manually trigger respawn of pickups
        public void RespawnPickups()
        {
            // Remove any existing pickups
            foreach (Transform child in transform)
            {
                if (child.GetComponent<LivesCollectable>() != null ||
                    child.GetComponent<JewelCollectable>() != null ||
                    child.GetComponent<TokenInstance>() != null)
                {
                    Destroy(child.gameObject);
                }
            }

            hasSpawned = false;
            SpawnPickups();
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            // Try to get references if they're null
            if (platformCollider == null)
            {
                platformCollider = GetComponent<BoxCollider2D>();
                if (platformCollider == null)
                {
                    platformCollider = GetComponent<TilemapCollider2D>();
                    tilemap = GetComponent<Tilemap>();
                }
            }
            if (platformCollider == null) return;

            Bounds bounds = GetColliderBounds();
            float platformWidth = bounds.size.x;
            float leftEdge = bounds.min.x;
            
            if (tilemap != null)
            {
                // Convert local bounds to world space for tilemaps
                leftEdge = transform.TransformPoint(new Vector3(leftEdge, 0, 0)).x;
            }
            
            int maxPickups = Mathf.FloorToInt(platformWidth / minSpacing);
            
            Gizmos.color = Color.yellow;
            for (int i = 0; i < maxPickups; i++)
            {
                float xPos = leftEdge + (i + 0.5f) * minSpacing;
                float yPos;

                if (tilemap != null)
                {
                    // For tilemaps, we need to raycast to find the actual top surface
                    Vector3 rayStart = new Vector3(xPos, bounds.max.y + 1f, transform.position.z);
                    RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 2f);
                    if (hit.collider != null && hit.collider.gameObject == gameObject)
                    {
                        yPos = hit.point.y + heightOffset;
                        Gizmos.DrawLine(rayStart, hit.point);
                        Gizmos.DrawWireSphere(new Vector3(xPos, yPos, transform.position.z), 0.3f);
                    }
                }
                else
                {
                    yPos = bounds.max.y + heightOffset;
                    Gizmos.DrawWireSphere(new Vector3(xPos, yPos, transform.position.z), 0.3f);
                }
            }
        }
#endif
    }
} 