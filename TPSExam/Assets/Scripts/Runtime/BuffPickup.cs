using UnityEngine;

public class BuffPickup : MonoBehaviour
{
    [SerializeField] private Buff.BuffType buffType;
    public Buff.BuffType BuffType => buffType;

    // Manager will call this once when creating pooled instance
    public void SetBuffType(Buff.BuffType type)
    {
        buffType = type;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ParticleManager.Instance.Spawn("BuffPickup", transform.position, Quaternion.identity);
            GameManager.Instance.ApplyBuff(buffType);
            BuffManager.Instance.Despawn(this);
            AudioManager.Instance.PlaySFX("BuffPickup");
        }
    }
}
