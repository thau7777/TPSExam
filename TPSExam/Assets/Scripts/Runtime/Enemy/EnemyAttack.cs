using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private Collider _collider;
    public float attackDamage; // Damage dealt to the player
    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Damageable damageable = other.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log($"Player took {attackDamage} damage from enemy attack.");
            }
        }
    }
}
