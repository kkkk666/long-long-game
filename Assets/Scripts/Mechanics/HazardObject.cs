using UnityEngine;
using Platformer.Mechanics;
using Platformer.Gameplay;
using Platformer.Core;

public class HazardObject : MonoBehaviour
{
    [Header("Ground Check")]
    public LayerMask groundLayer;  // Layer to check for ground
    public float groundCheckDistance = 10f;  // How far to check for ground

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure the collider is set as a trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        // If ground layer not set, try to get it from the player
        if (groundLayer == 0)
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                groundLayer = player.groundLayer;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
       // Debug.Log($"Hazard triggered with: {other.gameObject.name}");
        
        // Check if the colliding object is the player
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        
        if (player != null && !player.isInvulnerable)
        {
           // Debug.Log("Player hit hazard!");
            
            // Find a ground point near the hazard
            Vector2 deathPosition = FindNearestGroundPoint();
            
            // Schedule player death with the found ground position
            var ev = Simulation.Schedule<PlayerDeath>();
            ev.deathPosition = deathPosition;
        }
    }

    private Vector2 FindNearestGroundPoint()
    {
        // Start checking from above the hazard
        Vector2 checkPoint = (Vector2)transform.position + Vector2.up * 2f;
        
        // Raycast down to find ground
        RaycastHit2D groundHit = Physics2D.Raycast(
            checkPoint,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        if (groundHit.collider != null)
        {
            // Return the ground point slightly above the ground
            return groundHit.point + Vector2.up * 0.5f;
        }

        // If no ground found, return the hazard's position
       // Debug.LogWarning("No ground found near hazard, using hazard position");
        return (Vector2)transform.position;
    }
}
