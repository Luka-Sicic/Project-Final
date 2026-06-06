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
        
        [Header("Visuals")]
        [Tooltip("Offset added to rotation. If the sprite points its 'side' at the player, adjust this (usually 90 or -90).")]
        public float rotationOffset = -90f;

        [Header("Patrol")]
        public Transform[] patrolPoints;
        public float patrolWaitTime = 1f;

        private Transform _player;
        private AIDestinationSetter _setter;
        private AIPath _aiPath;
        private SpriteRenderer _sprite;
        private Animator _animator;
        private float _stunTimer;
        private int _currentPatrolIndex;
        private float _patrolWaitTimer;

        private Vector3 _lastSeenPosition;
        private bool _isSearching;
        private bool _isWaitingAtLastSeen;
        private float _searchTimer;

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

            if (_aiPath != null) _aiPath.canMove = !_isWaitingAtLastSeen;

            float dist = _player != null ? Vector2.Distance(transform.position, _player.position) : float.MaxValue;
            bool canSee = _player != null && dist <= chaseRange;

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
                _lastSeenPosition = _player.position;
                _isSearching = false;
                _isWaitingAtLastSeen = false;
                if (_setter != null) _setter.target = _player;
                _patrolWaitTimer = 0f;
            }
            else
            {
                if (_setter != null && _setter.target != null)
                {
                    // Transition from chasing to searching
                    _setter.target = null;
                    _aiPath.destination = _lastSeenPosition;
                    _isSearching = true;
                }

                if (_isSearching)
                {
                    if (_aiPath.reachedDestination && !_aiPath.pathPending)
                    {
                        _isSearching = false;
                        _isWaitingAtLastSeen = true;
                        _searchTimer = 3f;
                    }
                }
                else if (_isWaitingAtLastSeen)
                {
                    _searchTimer -= Time.deltaTime;
                    if (_searchTimer <= 0)
                    {
                        _isWaitingAtLastSeen = false;
                    }
                }
                else
                {
                    UpdatePatrol();
                }
            }

            UpdateVisuals();
}

        private void UpdatePatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            _aiPath.destination = patrolPoints[_currentPatrolIndex].position;

            if (_aiPath.reachedDestination && !_aiPath.pathPending)
            {
                _patrolWaitTimer += Time.deltaTime;
                if (_patrolWaitTimer >= patrolWaitTime)
                {
                    _patrolWaitTimer = 0f;
                    _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
                }
            }
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
