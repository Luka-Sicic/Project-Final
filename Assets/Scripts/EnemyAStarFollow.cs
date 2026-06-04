using UnityEngine;
using Pathfinding;

namespace Project.Scripts
{
    [RequireComponent(typeof(AIPath))]
    [RequireComponent(typeof(AIDestinationSetter))]
    public class EnemyAStarFollow : MonoBehaviour
    {
        [Header("Chase Settings")]
        [Tooltip("The range within which the enemy will start following the player.")]
        public float chaseRange = 5f;

        [Header("Detection Settings")]
        public float fovAngle = 180f;
        public LayerMask obstacleLayer;

        private Transform _playerTransform;
        private AIDestinationSetter _destinationSetter;
        private AIPath _aiPath;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            // Detect GameObject with tag 'Player'
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning($"[EnemyAStarFollow] Player not found with tag 'Player' on {gameObject.name}");
            }

            // Cache components with null checks
            _destinationSetter = GetComponent<AIDestinationSetter>();
            _aiPath = GetComponent<AIPath>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_destinationSetter == null) Debug.LogWarning($"[EnemyAStarFollow] AIDestinationSetter component missing on {gameObject.name}");
            
            if (_aiPath != null)
            {
                _aiPath.enableRotation = false;
            }
            else
            {
                Debug.LogWarning($"[EnemyAStarFollow] AIPath component missing on {gameObject.name}");
            }

            transform.rotation = Quaternion.identity;
        }

        private void Update()
        {
            UpdateChaseLogic();
            UpdateAnimationAndSprite();
        }

        private void UpdateChaseLogic()
        {
            if (_playerTransform == null || _destinationSetter == null || _spriteRenderer == null) return;

            // Calculate distance and direction to player
            Vector2 position = transform.position;
            Vector2 playerPosition = _playerTransform.position;
            float distance = Vector2.Distance(position, playerPosition);

            if (distance > chaseRange)
            {
                _destinationSetter.target = null;
                return;
            }

            Vector2 directionToPlayer = (playerPosition - position).normalized;

            // Determine 'forward' vector
            Vector2 forward = _spriteRenderer.flipX ? Vector2.left : Vector2.right;

            // Check if in FOV
            if (Vector2.Angle(forward, directionToPlayer) <= fovAngle * 0.5f)
            {
                // Perform Raycast for obstacles
                RaycastHit2D hit = Physics2D.Raycast(position, directionToPlayer, distance, obstacleLayer);
                
                if (hit.collider == null)
                {
                    _destinationSetter.target = _playerTransform;
                }
                else
                {
                    _destinationSetter.target = null;
                }
            }
            else
            {
                _destinationSetter.target = null;
            }
        }

        private void UpdateAnimationAndSprite()
        {
            if (_aiPath == null) return;

            // Update Animator 'IsWalking' parameter
            if (_animator != null)
            {
                bool isWalking = _aiPath.velocity.sqrMagnitude > 0.01f;
                _animator.SetBool("IsWalking", isWalking);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chaseRange);

            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null) return;

            Vector3 forward = _spriteRenderer.flipX ? Vector3.left : Vector3.right;
            Vector3 leftBoundary = Quaternion.Euler(0, 0, fovAngle * 0.5f) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, 0, -fovAngle * 0.5f) * forward;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, leftBoundary * chaseRange);
            Gizmos.DrawRay(transform.position, rightBoundary * chaseRange);
        }
    }
}
