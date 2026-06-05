using UnityEngine;
using Pathfinding;

namespace Project.Scripts
{
    [RequireComponent(typeof(AIPath))]
    [RequireComponent(typeof(AIDestinationSetter))]
    public class EnemyAStarFollow : MonoBehaviour
    {
        [Header("Detection")]
        public float chaseRange = 5f;
        public float fovAngle = 180f;
        public LayerMask obstacleLayer;
        
        [Header("Memory")]
        [Tooltip("Seconds to keep chasing after losing line of sight.")]
        public float memoryTime = 1.5f;

        [Header("Visuals")]
        [Tooltip("Offset added to rotation. If the sprite points its 'side' at the player, adjust this (usually 90 or -90).")]
        public float rotationOffset = -90f;

        private Transform _player;
        private AIDestinationSetter _setter;
        private AIPath _aiPath;
        private SpriteRenderer _sprite;
        private Animator _animator;
        private float _memoryTimer;
        private float _stunTimer;

        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;
            _setter = GetComponent<AIDestinationSetter>();
            _aiPath = GetComponent<AIPath>();
            _sprite = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();

            if (_aiPath != null)
            {
                // Disable AIPath's internal rotation so we can handle it manually with the offset.
                _aiPath.enableRotation = false;
            }

        }

        void Update()
        {
            if (_stunTimer > 0)
            {
                _stunTimer -= Time.deltaTime;
                if (_aiPath != null) _aiPath.canMove = false;
                UpdateVisuals();
                return;
            }

            if (_aiPath != null) _aiPath.canMove = true;

            if (_player == null || _setter == null) return;

            float dist = Vector2.Distance(transform.position, _player.position);
            bool canSee = dist <= chaseRange;

            if (canSee)
            {
                Vector2 dirToPlayer = ((Vector2)_player.position - (Vector2)transform.position).normalized;
                
                // Determine 'forward' based on the transform's current rotation minus the offset.
                float currentFacingAngle = (transform.eulerAngles.z - rotationOffset) * Mathf.Deg2Rad;
                Vector2 forward = new Vector2(Mathf.Cos(currentFacingAngle), Mathf.Sin(currentFacingAngle));

                if (Vector2.Angle(forward, dirToPlayer) > fovAngle * 0.5f)
                {
                    canSee = false;
                }
                else if (Physics2D.Raycast(transform.position, dirToPlayer, dist, obstacleLayer))
                {
                    canSee = false;
                }
            }

            if (canSee)
            {
                _memoryTimer = memoryTime;
                _setter.target = _player;
            }
            else
            {
                _memoryTimer -= Time.deltaTime;
                if (_memoryTimer <= 0) _setter.target = null;
            }

            UpdateVisuals();
        }

        void UpdateVisuals()
        {
            if (_aiPath == null) return;

            if (_animator != null)
            {
                _animator.SetBool("IsWalking", _aiPath.velocity.sqrMagnitude > 0.01f);
            }

            // Smoothly rotate the transform towards the movement direction, plus the offset.
            if (_aiPath.velocity.sqrMagnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(_aiPath.velocity.y, _aiPath.velocity.x) * Mathf.Rad2Deg;
                float currentAngle = transform.eulerAngles.z;
                float nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle + rotationOffset, _aiPath.rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, nextAngle);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chaseRange);
            
            // Draw FOV lines
            float currentFacingAngle = (transform.eulerAngles.z - rotationOffset) * Mathf.Deg2Rad;
            Vector3 forward = new Vector3(Mathf.Cos(currentFacingAngle), Mathf.Sin(currentFacingAngle), 0);
            Vector3 leftRay = Quaternion.Euler(0, 0, fovAngle * 0.5f) * forward;
            Vector3 rightRay = Quaternion.Euler(0, 0, -fovAngle * 0.5f) * forward;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, leftRay * chaseRange);
            Gizmos.DrawRay(transform.position, rightRay * chaseRange);
        }

        public void Stun(float duration)
        {
            _stunTimer = duration;
            if (_aiPath != null) _aiPath.canMove = false;
        }
    }
}
