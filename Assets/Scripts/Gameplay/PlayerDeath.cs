using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        
        // Store death position
        public Vector2 deathPosition;

        public override void Execute()
        {
            var player = model.player;
            if (player.health.IsAlive)
            {
                // Store the death position
                deathPosition = player.transform.position;
                // Decrease lives using ScoreManager
                ScoreManager.Instance.RemoveLife();
                
                player.health.Die();
                model.virtualCamera.Follow = null;
                model.virtualCamera.LookAt = null;
                player.controlEnabled = false;

                if (player.audioSource && player.ouchAudio)
                    player.audioSource.PlayOneShot(player.ouchAudio);
                
                player.GetComponent<PlayerController>().TriggerDeath();
                
                // Schedule respawn and pass death position
                var spawnEvent = Simulation.Schedule<PlayerSpawn>(1);
                spawnEvent.deathPosition = deathPosition;
            }
        }
    }
}