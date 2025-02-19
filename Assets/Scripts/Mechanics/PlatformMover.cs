using UnityEngine;

namespace Platformer.Mechanics
{
    public class MovingPlatform : MonoBehaviour
    {
        public PatrolPath path;
        public float speedMultiplier = 1f;
        public Vector2 Velocity { get; private set; }
        internal PatrolPath.Mover mover;
        internal Collider2D _collider;

        void Awake()
        {
            _collider = GetComponent<Collider2D>();
            tag = "MovingPlatform";
        }

        void FixedUpdate()
        {
            if (path != null)
            {
                if (mover == null) 
                    mover = path.CreateMover(1f * speedMultiplier);
                
                transform.position = mover.Position;
            }
        }
    }
}