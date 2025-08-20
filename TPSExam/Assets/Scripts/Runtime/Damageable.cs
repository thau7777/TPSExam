using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [SerializeField]
    private int _currentHealth;

    [SerializeField]
    private int _maxHealth = 100;
    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = value;
            // ✅ Always clamp current health if max changes
            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
        }
    }

    public int CurrentHealth
    {
        get => _currentHealth;
        private set => _currentHealth = Mathf.Clamp(value, 0, _maxHealth);
    }

    public UnityEvent<int> OnDamaged;

    private void Awake()
    {
        // ✅ Always initialize CurrentHealth from MaxHealth
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
    public void Heal(int amount)
    {
        if (amount <= 0) return;

        CurrentHealth += amount; // Clamp keeps it <= MaxHealth
    }
}
