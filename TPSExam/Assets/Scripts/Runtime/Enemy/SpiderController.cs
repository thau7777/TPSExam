using UnityEngine;
using UnityEngine.AI;

public class SpiderController : MonoBehaviour
{
    public Transform player;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float rotationSpeed = 8f; // smooth turn speed

    private Animator animator;
    private NavMeshAgent agent;
    private bool isDead = false;
    private float lastAttackTime = 0f;

    private readonly string _attackTrigger = "Attack";
    private readonly string _deathTrigger = "Death";

    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Disable automatic agent rotation so we can handle it manually
        agent.updateRotation = false;

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
        if (isDead || player == null) return;

        // Always move toward player
        agent.SetDestination(player.position);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange && (Time.time - lastAttackTime >= attackCooldown))
        {
            lastAttackTime = Time.time;
            animator.SetTrigger(_attackTrigger);
        }

        // Rotate toward movement direction
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void StopMoving()
    {
        if (isDead) return;
        agent.isStopped = true;
    }

    public void StartMoving()
    {
        if (isDead) return;
        agent.isStopped = false;
    }

    public void TakeDamage(int currentHealth)
    {
        if (isDead) return;
        if (currentHealth <= 0)
            Die();
        else
        {
            // TODO: add visible damage feedback
        }
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true;
        animator.SetTrigger(_deathTrigger);
        Destroy(gameObject, 3f); // remove after death animation
    }
}
