using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float rotationSpeed = 8f; // smooth turn speed

    private Rigidbody _rigidbody;
    private Collider _collider;
    private Animator _animator;
    private NavMeshAgent _agent;
    private bool _isDead = false;
    private float _lastAttackTime = 0f;

    private readonly string _attackTrigger = "Attack";
    private readonly string _deathTrigger = "Death";
    private readonly string _hurtTrigger = "Hurt";

    private float ogBaseOffset = 0.79f; // Original base offset for the NavMeshAgent

    [HideInInspector]
    public string poolID; // ID for object pooling

    private SkinnedMeshRenderer _skinnedRenderer;
    private Material _material;
    private Coroutine _fadeRoutine;
    private float _chanceToDropBuff = 0.3f;
    private void Awake()
    {
        _skinnedRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (_skinnedRenderer != null)
        {
            // This will create a unique instance so fading doesn't affect other enemies
            _material = _skinnedRenderer.material;
        }
    }
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
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
        if (_isDead || player == null || !_agent.enabled) return;

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

    public void TakeDamage(int currentHealth)
    {
        if (_isDead) return;

        if (currentHealth <= 0)
        {
            Die();
            _animator.ResetTrigger(_hurtTrigger);
            return;
        }
        _animator.SetTrigger(_hurtTrigger);
    }

    public void ApplyKnockback(Vector3 force, float duration = 0.3f)
    {
        if (_isDead) return;

        StartCoroutine(KnockbackRoutine(force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector3 force, float duration)
    {
        // Stop AI movement & enable physics
        _agent.enabled = false;
        _rigidbody.isKinematic = false;

        _rigidbody.AddForce(force, ForceMode.Impulse);

        yield return new WaitForSeconds(duration);

        // Stop any remaining velocity
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        // Restore movement
        _rigidbody.isKinematic = true;
        _agent.enabled = true;
    }


    public void Die()
    {
        _isDead = true;
        _agent.isStopped = true;

        if (_collider != null)
            _collider.enabled = false; // Disable collisions

        _animator.SetTrigger(_deathTrigger);

        TryDropBuff();
        AudioManager.Instance.PlaySFX("EnemyDead");
    }

    private void TryDropBuff()
    {
        if (Random.value <= _chanceToDropBuff) // Random.value is 0..1
        {
            // Drop buff at enemy's position
            if(CompareTag("Fish"))
                BuffManager.Instance.SpawnRandom(transform.position + Vector3.up * 0.5f, Quaternion.identity);
            else
                BuffManager.Instance.SpawnRandom(transform.position, Quaternion.identity);
        }
    }


    public void OnDeathAnimationHalf()
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeOut(1.5f)); // Fade duration
   
    }

    public void TurnOnGravityOnDead()
    {
        if (this.CompareTag("Bee"))
        {
            StartCoroutine(FallingDown(0.7f));
        }
    }

    private IEnumerator FallingDown(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            _agent.baseOffset = Mathf.Lerp(ogBaseOffset, 0f, elapsed / duration);

            yield return null;
        }
    }
    public void ResetBaseOffset()
    {
        if (_agent != null && this.CompareTag("Bee"))
        {
            _agent.baseOffset = ogBaseOffset;
        }
    }
    private IEnumerator FadeOut(float duration)
    {
        float elapsed = 0f;
        Color startColor = _material.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);

            Color c = startColor;
            c.a = alpha;
            _material.color = c;

            yield return null;
        }
        OnDeathAnimationEnd();
    }

    public void ResetFade()
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);
        if (_material != null)
        {
            Color c = _material.color;
            c.a = 1f;
            _material.color = c;
        }
    }

    public void ResetEnemy()
    {
        _isDead = false;

        if (_agent != null)
        {
            _agent.enabled = true;
            _agent.isStopped = false;
        }

        if (_collider != null)
            _collider.enabled = true;

        if (_animator != null)
        {
            _animator.ResetTrigger(_deathTrigger);
            _animator.ResetTrigger(_hurtTrigger);
            _animator.ResetTrigger(_attackTrigger);
            _animator.Play("Movement"); // 👈 your default state
        }
    }

    public void OnDeathAnimationEnd()
    {
        EnemyManager.Instance.Release(poolID, gameObject);
    }

    public void ForceDespawn()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.Release(poolID, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
