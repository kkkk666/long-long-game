using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class InitialPlayerSpawn : MonoBehaviour
    {
        [SerializeField] private float spawnDelay = 2f;
        private SpriteRenderer playerSprite;

        private void Start()
        {
            var model = Simulation.GetModel<PlatformerModel>();
            playerSprite = model.player.GetComponent<SpriteRenderer>();
            
            // Hide player sprite initially
            playerSprite.enabled = false;
            
            // Start delayed spawn
            StartCoroutine(DelayedSpawn());
        }

        private System.Collections.IEnumerator DelayedSpawn()
        {
            yield return new WaitForSeconds(spawnDelay);
            playerSprite.enabled = true;
        }
    }
} 