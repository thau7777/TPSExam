using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{

    [SerializeField]
    private int _currentHealth;
    public int CurrentHealth
    {
        get { return _currentHealth; }
        set
        {
            _currentHealth = value;
        }
    }

    [SerializeField]
    private int _maxHealth = 100;
    public int MaxHealth
    {
        get { return _maxHealth; }
        set
        {
            _maxHealth = value;
        }
    }


    public UnityEvent<int> OnDamaged;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }
    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        CurrentHealth -= damage;
        OnDamaged?.Invoke(CurrentHealth);
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }
}
