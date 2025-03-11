using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class PlayerSpawn : Simulation.Event<PlayerSpawn>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        
        // Add death position field
        public Vector2 deathPosition;

        [System.Obsolete]
        public override void Execute()
        {
            var player = model.player;
            player.collider2d.enabled = true;
            player.controlEnabled = false;
            
            // Find safe spawn point
            Vector2 safeSpawnPoint = SpawnPointFinder.FindSafeSpawnPoint(
                deathPosition, 
                player.GetComponent<PlayerController>().groundLayer
            );

            if (player.audioSource && player.respawnAudio)
                player.audioSource.PlayOneShot(player.respawnAudio);
            
            player.health.Increment();
            player.Teleport(safeSpawnPoint);
            player.jumpState = PlayerController.JumpState.Grounded;
            player.GetComponent<PlayerController>().ResetDeathState();
            model.virtualCamera.Follow = player.transform;
            model.virtualCamera.LookAt = player.transform;
            
            // Enable controls after a shorter delay (0.5 seconds)
            Simulation.Schedule<EnablePlayerInput>(0.5f);
        }
    }
}