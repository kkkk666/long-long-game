using UnityEngine;
using System.Collections.Generic;

namespace Platformer.Mechanics
{
    public class SpawnPointFinder : MonoBehaviour
    {
        private const float MAX_RAYCAST_DISTANCE = 50f;  // Maximum distance to check downward
        private const float SPAWN_HEIGHT_OFFSET = 5f;    // Increased from 2f to 5f to start search higher
        private const float HORIZONTAL_OFFSET = 1f;      // How far to move when unsafe spot is found
        private const int MAX_ATTEMPTS = 50;             // Maximum number of horizontal shifts to try
        private const float PATROL_PATH_BUFFER = 3f;     // Buffer distance from patrol paths
        private const float HAZARD_BUFFER = 2f;          // Buffer distance from hazards
        private const float GROUND_OFFSET = 0.5f;        // How far above ground to spawn
        private const float INITIAL_OFFSET = 3f;         // How far to start searching from death position
        private const float WATER_ZONE_BUFFER = 2f;      // Additional buffer around water zones
        private const float MIN_GROUND_DISTANCE = 1f;    // Minimum distance required between spawn point and ground

        public static Vector2 FindSafeSpawnPoint(Vector2 deathPosition, LayerMask groundLayer, float spawnRadius = 1.5f, bool diedInWater = false)
        {
            // Determine initial search direction based on whether player died in water
            float searchDirection = diedInWater ? 1f : -1f; // 1 for right, -1 for left
            
            // Find the water zone collider at the death position
            Collider2D waterZoneCollider = null;
            if (diedInWater)
            {
                // First try to find water zone by raycasting down from death position
                RaycastHit2D[] hits = Physics2D.RaycastAll(deathPosition, Vector2.down, MAX_RAYCAST_DISTANCE);
                foreach (var hit in hits)
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
                    {
                        waterZoneCollider = hit.collider;
                        break;
                    }
                }

                // If no water zone found by raycast, try OverlapPointAll
                if (waterZoneCollider == null)
                {
                    Collider2D[] colliders = Physics2D.OverlapPointAll(deathPosition);
                    foreach (var collider in colliders)
                    {
                        if (collider.gameObject.layer == LayerMask.NameToLayer("Water"))
                        {
                            waterZoneCollider = collider;
                            break;
                        }
                    }
                }
            }

            // If we found a water zone collider, calculate how far we need to move to get past it
            float requiredOffset = INITIAL_OFFSET;
            if (waterZoneCollider != null)
            {
                Bounds waterBounds = waterZoneCollider.bounds;
                float distanceToEdge;
                
                if (searchDirection > 0) // Moving right
                {
                    distanceToEdge = waterBounds.max.x - deathPosition.x;
                }
                else // Moving left
                {
                    distanceToEdge = deathPosition.x - waterBounds.min.x;
                }
                
                // Add buffer to ensure we're past the water zone
                requiredOffset = distanceToEdge + WATER_ZONE_BUFFER;
            }

            // Start checking from a higher point to avoid terrain features
            Vector2 currentCheckPoint = deathPosition + Vector2.up * SPAWN_HEIGHT_OFFSET + Vector2.right * requiredOffset * searchDirection;

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

            // Cache all hazards
            var hazards = Object.FindObjectsOfType<HazardObject>();
            var hazardPositions = new List<Vector2>();
            foreach (var hazard in hazards)
            {
                hazardPositions.Add(hazard.transform.position);
            }

            // Try to find a safe spot
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
                    Vector2 potentialSpawnPoint = groundHit.point + Vector2.up * GROUND_OFFSET;
                    
                    // Additional check to ensure we're not too close to the ground
                    RaycastHit2D groundCheck = Physics2D.Raycast(
                        potentialSpawnPoint,
                        Vector2.down,
                        MIN_GROUND_DISTANCE,
                        groundLayer
                    );
                    
                    if (groundCheck.collider == null) // If no ground found within minimum distance, it's safe
                    {
                        // Check if this point is safe from patrol paths
                        if (!IsPointNearPatrolPaths(potentialSpawnPoint, patrolPaths))
                        {
                            // Check if this point is safe from hazards
                            if (!IsPointNearHazards(potentialSpawnPoint, hazardPositions))
                            {
                                // Check if this point is safe from spring platforms
                                if (SpringPlatform.IsSafeForRespawn(potentialSpawnPoint, spawnRadius))
                                {
                                    // Verify the spawn point is above ground
                                    RaycastHit2D verifyHit = Physics2D.Raycast(
                                        potentialSpawnPoint + Vector2.up * 0.1f,
                                        Vector2.down,
                                        0.2f,
                                        groundLayer
                                    );

                                    if (verifyHit.collider != null)
                                    {
                                        return potentialSpawnPoint;
                                    }
                                }
                            }
                        }
                    }
                }

                // Move in the appropriate direction and try again
                currentCheckPoint.x += HORIZONTAL_OFFSET * searchDirection;
            }

            // If no safe spot found, try to find a safe spot above and to the appropriate side of the death position
            Vector2 fallbackPosition = deathPosition + Vector2.up * 5f + Vector2.right * requiredOffset * searchDirection;
            
            // Verify the fallback position has ground below it
            RaycastHit2D fallbackHit = Physics2D.Raycast(
                fallbackPosition,
                Vector2.down,
                MAX_RAYCAST_DISTANCE,
                groundLayer
            );

            if (fallbackHit.collider != null)
            {
                return fallbackHit.point + Vector2.up * GROUND_OFFSET;
            }

            // Absolute fallback: Return a position high above and to the appropriate side of the death position
            return deathPosition + Vector2.up * 5f + Vector2.right * requiredOffset * searchDirection;
        }

        [System.Obsolete]
        public static Vector2 FindSafeSpawnPoint(Vector2 deathPosition, LayerMask groundLayer)
        {
            return FindSafeSpawnPoint(deathPosition, groundLayer, 1.5f, false);
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

        private static bool IsPointNearHazards(Vector2 point, List<Vector2> hazardPositions)
        {
            foreach (var hazardPos in hazardPositions)
            {
                float distance = Vector2.Distance(point, hazardPos);
                if (distance < HAZARD_BUFFER)
                    return true;
            }
            return false;
        }

        private static float DistanceToLineSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 line = end - start;
            float len = line.magnitude;
            if (len == 0f) return Vector2.Distance(point, start);
            
            float t = Vector2.Dot(point - start, line) / (len * len);
            if (t < 0f) return Vector2.Distance(point, start);
            if (t > 1f) return Vector2.Distance(point, end);
            
            Vector2 projection = start + t * line;
            return Vector2.Distance(point, projection);
        }
    }
}