using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int attackDamage; // Damage dealt to the player


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Damageable damageable = other.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
                
            }
        }
    }
}
