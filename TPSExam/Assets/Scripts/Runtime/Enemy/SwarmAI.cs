using UnityEngine;
using UnityEngine.AI;

public class SwarmAI : MonoBehaviour
{
    public Transform player;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float rotationSpeed = 8f; // smooth turn speed

    private Animator _animator;
    private NavMeshAgent _agent;
    private bool _isDead = false;
    private float _lastAttackTime = 0f;

    private readonly string _attackTrigger = "Attack";
    private readonly string _deathTrigger = "Death";

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        // Disable automatic agent rotation so we can handle it manually
        _agent.updateRotation = false;

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogError("No player found! Assign it in Inspector or tag your player as 'Player'.");
            }
        }
    }

    private void Update()
    {
        if (_isDead || player == null) return;

        // Always move toward player
        _agent.SetDestination(player.position);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange && Time.time - _lastAttackTime >= attackCooldown)
        {
            _lastAttackTime = Time.time;
            _animator.SetTrigger(_attackTrigger);
        }

        // Rotate toward movement direction
        if (_agent.velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        Vector3 speed = _agent.velocity;
        speed.y = 0; // Ignore vertical speed for animation

        _animator.SetFloat("Speed", speed.magnitude);

    }

    public void StopMoving()
    {
        if (_isDead) return;
        _agent.isStopped = true;
    }

    public void StartMoving()
    {
        if (_isDead) return;
        _agent.isStopped = false;
    }

    public void TakeDamage(Damageable.DamageType damageType, float currentHealth)
    {
        if (_isDead) return;

        if (currentHealth <= 0)
            Die();
        else if(damageType == Damageable.DamageType.Explosive)
        {
            // TODO: add visible damage feedback
        }
        else // normal damage
        {

        }
    }

    private void Die()
    {
        _isDead = true;
        _agent.isStopped = true;
        _animator.SetTrigger(_deathTrigger);
        Destroy(gameObject, 3f); // remove after death animation
    }
}
