using UnityEngine;
using System.Collections;
using Platformer.Mechanics;

public class BreakableObject : MonoBehaviour
{
    [Header("Break Settings")]
    public GameObject breakEffectPrefab;  // Particle effect or animation prefab
    public AudioClip breakSound;
    public float destroyDelay = 0.1f;     // Short delay before destroying the object
    
    [Header("Detection")]
    public LayerMask playerLayer;         // Layer the player is on
    public string stompTag = "PlayerStomp"; // Optional: Use a tag instead of checking velocity
    public bool breakFromBelow = true;    // Whether the object can be broken by hitting it from below
    
    private AudioSource audioSource;
    private bool isBroken = false;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && breakSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if it's the player
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            
            if (player != null)
            {
                bool shouldBreak = false;
                
                // Check if player is stomping
                if (player.isStomping)
                {
                    shouldBreak = true;
                }
                // Check if hit from below
                else if (breakFromBelow)
                {
                    foreach (ContactPoint2D contact in collision.contacts)
                    {
                        // If normal points upward, the collision is from below
                        // (normal points away from the object being hit)
                        if (contact.normal.y > 0.5f)
                        {
                            shouldBreak = true;
                            break;
                        }
                    }
                }
                
                if (shouldBreak)
                {
                    Break();
                }
            }
        }
    }
    
    public void Break()
    {
        if (isBroken) return;
        isBroken = true;
        
        // Play break sound
        if (audioSource != null && breakSound != null)
        {
            audioSource.PlayOneShot(breakSound);
        }
        
        // Spawn break effect
        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Disable renderer and collider immediately
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        
        // Destroy after delay (to allow sound to play)
        Destroy(gameObject, destroyDelay);
    }
}