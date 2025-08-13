using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    public enum DamageType
    {
        Normal,
        Explosive,
    }

    [SerializeField]
    private float _currentHealth;
    public float CurrentHealth
    {
        get { return _currentHealth; }
        set
        {
            _currentHealth = value;
        }
    }

    [SerializeField]
    private float _maxHealth = 100;
    public float MaxHealth
    {
        get { return _maxHealth; }
        set
        {
            _maxHealth = value;
        }
    }


    public UnityEvent<DamageType,float> OnDamaged;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }
    public void TakeDamage(DamageType damageType,float damage)
    {
        if (damage <= 0) return;
        CurrentHealth -= damage;
        OnDamaged?.Invoke(damageType, CurrentHealth);
    }
}
