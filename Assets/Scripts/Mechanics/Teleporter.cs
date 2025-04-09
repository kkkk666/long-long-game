using UnityEngine;

namespace Platformer.Mechanics
{
    public class Teleporter : MonoBehaviour
    {
        [Header("Teleporter Settings")]
        [Tooltip("The linked teleporter that the player will be teleported to")]
        public Teleporter linkedTeleporter;
        
        [Tooltip("Time in seconds before the player can use this teleporter again")]
        public float cooldownTime = 1f;
        
        [Tooltip("Offset from the teleporter's position where the player will be placed")]
        public Vector2 teleportOffset = new Vector2(0f, 0.5f);
        
        [Header("Visual Effects")]
        [Tooltip("Particle effect to play when teleporting")]
        public ParticleSystem teleportEffectPrefab;
        
        [Tooltip("Sound to play when teleporting")]
        public AudioClip teleportSound;
        
        private AudioSource audioSource;
        private float lastTeleportTime;
        private bool isOnCooldown;

        private void Start()
        {
            // Get or add AudioSource component
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Ensure the collider is set as a trigger
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
            else
            {
                Debug.LogError("No collider found on teleporter! Please add a 2D collider set as trigger.");
            }

            // Verify particle system prefab is assigned
            if (teleportEffectPrefab == null)
            {
                Debug.LogError($"No particle system prefab assigned to teleporter {gameObject.name}!");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if the colliding object is the player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !isOnCooldown && linkedTeleporter != null)
            {
                // Check if the linked teleporter is also not on cooldown
                if (!linkedTeleporter.isOnCooldown)
                {
                    TeleportPlayer(player);
                }
            }
        }

        private void TeleportPlayer(PlayerController player)
        {
            Debug.Log($"Teleporting player from {gameObject.name} to {linkedTeleporter.gameObject.name}");
            
            // Play teleport effect if available
            if (teleportEffectPrefab != null)
            {
                Debug.Log($"Instantiating particle system at player position");
                
                // Instantiate a new particle system at the player's position
                ParticleSystem effect = Instantiate(teleportEffectPrefab, player.transform.position, Quaternion.identity);
                
                // Make it a child of the player and maintain its original scale
                effect.transform.SetParent(player.transform);
                effect.transform.localScale = Vector3.one; // Reset scale to 1
                effect.transform.localPosition = Vector3.zero; // Center on player
                
                // Set simulation space to World so particles trail behind
                var main = effect.main;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                
                // Play the effect
                effect.Play();
                
                // Destroy it after 3 seconds
                Destroy(effect.gameObject, 1.5f);
            }
            else
            {
                Debug.LogError($"No particle system prefab assigned to teleporter {gameObject.name}!");
            }
            
            // Play teleport sound if available
            if (teleportSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(teleportSound);
            }
            
            // Calculate the target position
            Vector2 targetPosition = (Vector2)linkedTeleporter.transform.position + linkedTeleporter.teleportOffset;
            
            // Teleport the player
            player.Teleport(targetPosition);
            
            // Set cooldown for both teleporters
            StartCooldown();
            linkedTeleporter.StartCooldown();
        }

        private void StartCooldown()
        {
            isOnCooldown = true;
            lastTeleportTime = Time.time;
            Invoke(nameof(ResetCooldown), cooldownTime);
        }

        private void ResetCooldown()
        {
            isOnCooldown = false;
        }

        private void OnDrawGizmos()
        {
            // Draw a line to the linked teleporter in the editor
            if (linkedTeleporter != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, linkedTeleporter.transform.position);
                
                // Draw the teleport offset position
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere((Vector2)transform.position + teleportOffset, 0.2f);
            }
        }
    }
} 