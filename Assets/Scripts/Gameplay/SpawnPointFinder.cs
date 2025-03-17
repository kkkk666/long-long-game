using UnityEngine;
using System.Collections.Generic;

namespace Platformer.Mechanics
{
    public class SpawnPointFinder : MonoBehaviour
    {
        private const float MAX_RAYCAST_DISTANCE = 50f;  // Maximum distance to check downward
        private const float SPAWN_HEIGHT_OFFSET = 2f;    // How high above the ground to start checking
        private const float HORIZONTAL_OFFSET = 1f;      // How far to move left when unsafe spot is found
        private const int MAX_ATTEMPTS = 50;             // Maximum number of horizontal shifts to try
        private const float PATROL_PATH_BUFFER = 3f;     // Buffer distance from patrol paths

        [System.Obsolete]
        public static Vector2 FindSafeSpawnPoint(Vector2 deathPosition, LayerMask groundLayer)
        {
            Vector2 currentCheckPoint = deathPosition;
            currentCheckPoint.y += SPAWN_HEIGHT_OFFSET;

            // Cache all enemy patrol paths
            var enemies = Object.FindObjectsOfType<EnemyController>();
            var patrolPaths = new List<(Vector2 start, Vector2 end)>();
            
            foreach (var enemy in enemies)
            {
                if (enemy.path != null)
                {
                    Vector2 worldStart = enemy.path.transform.TransformPoint(enemy.path.startPosition);
                    Vector2 worldEnd = enemy.path.transform.TransformPoint(enemy.path.endPosition);
                    patrolPaths.Add((worldStart, worldEnd));
                }
            }

            for (int attempts = 0; attempts < MAX_ATTEMPTS; attempts++)
            {
                // Raycast down to find ground
                RaycastHit2D groundHit = Physics2D.Raycast(
                    currentCheckPoint,
                    Vector2.down,
                    MAX_RAYCAST_DISTANCE,
                    groundLayer
                );

                if (groundHit.collider != null)
                {
                    Vector2 potentialSpawnPoint = groundHit.point + Vector2.up * 0.5f;
                    
                    // Check for immediate enemy collisions
                    Collider2D[] overlaps = Physics2D.OverlapCircleAll(potentialSpawnPoint, 0.5f);
                    bool isUnsafe = false;

                    foreach (Collider2D overlap in overlaps)
                    {
                        if (overlap.CompareTag("Enemy"))  // Enemy tag
                        {
                            isUnsafe = true;
                            break;
                        }
                    }

                    // If no immediate collision, check patrol paths
                    if (!isUnsafe)
                    {
                        isUnsafe = IsPointNearPatrolPaths(potentialSpawnPoint, patrolPaths);
                    }

                    // Check if point is near any spring platforms
                    if (!isUnsafe)
                    {
                        isUnsafe = !SpringPlatform.IsSafeForRespawn(potentialSpawnPoint, 1.5f);
                    }

                    // If spot is safe from both immediate collisions, patrol paths, and spring platforms
                    if (!isUnsafe)
                    {
                        return potentialSpawnPoint;
                    }
                }

                // If unsafe, shift left and try again
                currentCheckPoint.x -= HORIZONTAL_OFFSET;
            }

            // If no safe spot found, return original spawn point
            var spawnPoint = GameObject.FindGameObjectWithTag("Respawn");
            if (spawnPoint != null)
            {
                // Make sure the spawn point is not near any spring platforms
                Vector2 originalSpawnPos = spawnPoint.transform.position;
                if (SpringPlatform.IsSafeForRespawn(originalSpawnPos, 1.5f))
                {
                    return originalSpawnPos;
                }
                else
                {
                    // If spawn point is unsafe, try to find a safe spot above it
                    Vector2 safeSpawnPos = originalSpawnPos;
                    safeSpawnPos.y += 2f;
                    return safeSpawnPos;
                }
            }
            
            // Absolute fallback: Return a position high above the death position
            return deathPosition + Vector2.up * 5f;
        }

        private static bool IsPointNearPatrolPaths(Vector2 point, List<(Vector2 start, Vector2 end)> patrolPaths)
        {
            foreach (var path in patrolPaths)
            {
                // Check distance to line segment
                float distance = DistanceToLineSegment(point, path.start, path.end);
                if (distance < PATROL_PATH_BUFFER)
                    return true;
            }
            return false;
        }

        private static float DistanceToLineSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 line = end - start;
            float len = line.magnitude;
            if (len == 0f) return Vector2.Distance(point, start);

            float t = Mathf.Clamp01(Vector2.Dot(point - start, line) / (len * len));
            Vector2 projection = start + t * line;
            return Vector2.Distance(point, projection);
        }
    }
}