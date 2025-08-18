using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{

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


    public UnityEvent<float> OnDamaged;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }
    public void TakeDamage(float damage)
    {
        if (damage <= 0) return;
        CurrentHealth -= damage;
        OnDamaged?.Invoke(CurrentHealth);
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }

    public void IncreaseMaxHealth(float value)
    {

    }
}
