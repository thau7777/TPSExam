using UnityEngine;

public class GrenadeProjectile : MonoBehaviour
{
    public float explosionRadius = 5f;
    public float knockbackForce = 5;
    public float damage = 50f;
    public LayerMask enemyLayer;
    public float lifetime = 5f;

    private float _timer;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        _timer = lifetime;
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void Explode()
    {
        // Spawn explosion effect
        BulletImpactManager.Instance.Spawn("SmallExplosion", transform.position, Quaternion.identity);

        // Damage & knockback
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<Damageable>(out Damageable damageable))
            {
                damageable.TakeDamage(damage);
            }

            if (hit.TryGetComponent<SwarmAI>(out SwarmAI swarm))
            {
                if (!hit.CompareTag("Fish"))
                    continue;

                Vector3 direction = (hit.transform.position - transform.position).normalized;
                swarm.ApplyKnockback(direction * knockbackForce, 0.5f);
            }
        }

        Destroy(gameObject);
    }

    // Draw debug sphere in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
