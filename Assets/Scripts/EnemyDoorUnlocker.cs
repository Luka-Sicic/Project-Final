using UnityEngine;
using Pathfinding;

namespace Project.Scripts
{
    /// <summary>
    /// Allows an enemy to unlock a door when they collide with it while trying to move.
    /// </summary>
    public class EnemyDoorUnlocker : MonoBehaviour
    {
        private AIPath _aiPath;

        private void Awake()
        {
            _aiPath = GetComponent<AIPath>();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Only unlock if we are trying to move and not incapacitated
            if (_aiPath != null && !_aiPath.canMove) return;

            // Check if the collided object has a Door component
            Door door = collision.gameObject.GetComponent<Door>();
            
            if (door != null && door.isLocked)
            {
                // Unlock the door
                door.UnlockDoor();
                Debug.Log($"[EnemyDoorUnlocker] {name} unlocked door: {door.name}");
            }
        }
    }
}
