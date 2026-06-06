using UnityEngine;
using Pathfinding;

namespace Project.Scripts
{
    [RequireComponent(typeof(AIPath))]
    [RequireComponent(typeof(AIDestinationSetter))]
    public class EnemyShotgunAI : MonoBehaviour
    {
        [Header("Detection & Movement")]
        public float chaseRange = 10f;
        public float shootRange = 7f;
        public float fovAngle = 180f;
        public LayerMask obstacleLayer;
        public float rotationOffset = -90f;

        [Header("Shooting")]
        public GameObject bulletPrefab;
        public Transform firePoint;
        public float fireRate = 1.5f;
        public int pelletCount = 5;
        public float spreadAngle = 20f;
        public float bulletSpeed = 15f;

        [Header("Visuals")]
        public Sprite staticSprite;

        [Header("Patrol")]
        public Transform[] patrolPoints;
        public float patrolWaitTime = 1f;

        private Transform _player;
        private AIDestinationSetter _setter;
        private AIPath _aiPath;
        private float _fireTimer;

        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private float _moveResumeTimer;
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
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_animator != null)
            {
                _animator.enabled = false;
            }

            if (_spriteRenderer != null && staticSprite != null)
            {
                _spriteRenderer.sprite = staticSprite;
            }

            if (_aiPath != null)
            {
                _aiPath.enableRotation = false;
            }

            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        void Update()
        {
            // Update timers
            if (_moveResumeTimer > 0) _moveResumeTimer -= Time.deltaTime;
            if (_fireTimer > 0) _fireTimer -= Time.deltaTime;

            float dist = _player != null ? Vector2.Distance(transform.position, _player.position) : float.MaxValue;
            bool canSee = _player != null && HasLineOfSight() && dist <= chaseRange;

            // Movement and Targeting Logic
            if (canSee)
            {
                _lastSeenPosition = _player.position;
                _isSearching = false;
                _isWaitingAtLastSeen = false;
                if (_setter != null) _setter.target = _player;
                _patrolWaitTimer = 0f;

                if (dist <= shootRange)
                {
                    // Inside shooting range: stop and attack
                    if (_aiPath != null) _aiPath.canMove = false;

                    if (_fireTimer <= 0)
                    {
                        Shoot();
                        _fireTimer = fireRate;
                    }
                }
                else
                {
                    // Outside shooting range but inside chase range: resume movement if recoil over
                    if (_moveResumeTimer <= 0 && _aiPath != null)
                    {
                        _aiPath.canMove = true;
                    }
                }
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
                    if (_moveResumeTimer <= 0 && _aiPath != null)
                    {
                        _aiPath.canMove = true;
                    }

                    if (_aiPath.reachedDestination && !_aiPath.pathPending)
                    {
                        _isSearching = false;
                        _isWaitingAtLastSeen = true;
                        _searchTimer = 3f;
                    }
                }
                else if (_isWaitingAtLastSeen)
                {
                    if (_aiPath != null) _aiPath.canMove = false;

                    _searchTimer -= Time.deltaTime;
                    if (_searchTimer <= 0)
                    {
                        _isWaitingAtLastSeen = false;
                    }
                }
                else
                {
                    if (_moveResumeTimer <= 0 && _aiPath != null)
                    {
                        _aiPath.canMove = true;
                    }
                    UpdatePatrol();
                }
            }

            UpdateRotation();
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

        bool HasLineOfSight()
        {
            Vector2 dirToPlayer = (_player.position - transform.position).normalized;
            float dist = Vector2.Distance(transform.position, _player.position);

            // FOV check
            float currentFacingAngle = (transform.eulerAngles.z - rotationOffset) * Mathf.Deg2Rad;
            Vector2 forward = new Vector2(Mathf.Cos(currentFacingAngle), Mathf.Sin(currentFacingAngle));
            if (Vector2.Angle(forward, dirToPlayer) > fovAngle * 0.5f) return false;

            // Obstacle check
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, dist, obstacleLayer);
            return hit.collider == null || hit.collider.CompareTag("Player");
        }

        void Shoot()
        {
            if (bulletPrefab == null) return;

            // Stop movement when firing
            if (_aiPath != null)
            {
                _aiPath.canMove = false;
                _moveResumeTimer = 0.5f;
            }

            float startAngle = -spreadAngle * 0.5f;
            float angleStep = pelletCount > 1 ? spreadAngle / (pelletCount - 1) : 0;

            for (int i = 0; i < pelletCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                // Rotate firePoint.right by currentAngle
                Quaternion rotation = transform.rotation * Quaternion.Euler(0, 0, currentAngle - rotationOffset);
                
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // In 2D, transform.right is often the forward direction.
                    // But we used rotationOffset. 
                    // Let's use the rotation we just calculated.
                    rb.linearVelocity = (Vector2)(rotation * Vector3.right) * bulletSpeed;
                }
            }
        }

        void UpdateRotation()
        {
            if (_aiPath == null) return;

            Vector2 direction = Vector2.zero;
            if (_setter.target != null)
            {
                // Rotate towards player if we are chasing
                direction = (_player.position - transform.position).normalized;
            }
            else if (_aiPath.velocity.sqrMagnitude > 0.1f)
            {
                // Rotate towards movement direction if not chasing but moving
                direction = _aiPath.velocity.normalized;
            }

            if (direction != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float currentAngle = transform.eulerAngles.z;
                float nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle + rotationOffset, _aiPath.rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, nextAngle);
            }
        }
    }
}
