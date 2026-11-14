using PrimeTween;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class EnemyBrain : MonoBehaviour
{
    [SerializeField] private HealthBehaviour health;
    [SerializeField] private EnemyAttackBase attack;
    [SerializeField] private float aggroRange = 18f;
    [SerializeField] private float loseTargetRange = 30f;
    [SerializeField] private float repathInterval = 0.25f;
    [SerializeField] private float turnSpeed = 8f;
    [SerializeField] private float deathAnimationDuration = 0.25f;

    private NavMeshAgent _agent;
    private PlayerTargetService _targetService;
    private bool _isAggro;
    private bool _isDead;
    private float _nextRepathTime;

    [Inject]
    private void Construct(PlayerTargetService targetService)
    {
        _targetService = targetService;
    }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        if (health == null)
        {
            health = GetComponent<HealthBehaviour>();
        }

        if (attack == null)
        {
            attack = GetComponent<EnemyAttackBase>();
        }
    }

    private void Start()
    {
        if (health != null)
        {
            health.HealthModel.Died += OnDied;
        }

        if (_targetService == null)
        {
            Debug.LogWarning("EnemyBrain: PlayerTargetService was not injected. Spawn enemies through Zenject or place them in the scene.");
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.HealthModel.Died -= OnDied;
        }
    }

    private void Update()
    {
        if (_isDead || _targetService == null || !_targetService.HasTarget)
        {
            StopMoving();
            return;
        }

        Transform target = _targetService.Target;
        float distance = Vector3.Distance(transform.position, target.position);

        if (!_isAggro)
        {
            if (distance > aggroRange)
            {
                StopMoving();
                return;
            }

            _isAggro = true;
        }

        if (distance > loseTargetRange)
        {
            _isAggro = false;
            StopMoving();
            return;
        }

        if (attack != null && distance <= attack.AttackRange)
        {
            StopMoving();
            FaceTarget(target.position);
            attack.TryAttack(target);
            return;
        }

        MoveTo(target.position);
    }

    private void MoveTo(Vector3 position)
    {
        if (!_agent.enabled || !_agent.isOnNavMesh)
        {
            return;
        }

        _agent.isStopped = false;

        if (Time.time < _nextRepathTime)
        {
            return;
        }

        _nextRepathTime = Time.time + repathInterval;
        _agent.SetDestination(position);
    }

    private void StopMoving()
    {
        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
        }
    }

    private void FaceTarget(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void OnDied()
    {
        _isDead = true;
        StopMoving();

        if (_agent != null)
        {
            _agent.enabled = false;
        }

        if (attack != null)
        {
            attack.enabled = false;
        }

        foreach (Collider enemyCollider in GetComponentsInChildren<Collider>())
        {
            enemyCollider.enabled = false;
        }

        PlayDeathAnimation();
    }

    private void PlayDeathAnimation()
    {
        if (deathAnimationDuration <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Tween.Scale(transform, Vector3.zero, deathAnimationDuration, Ease.InQuad)
            .OnComplete(() => Destroy(gameObject), warnIfTargetDestroyed: false);
    }
}
