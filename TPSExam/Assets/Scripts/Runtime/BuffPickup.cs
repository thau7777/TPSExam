using UnityEngine;

public class BuffPickup : MonoBehaviour
{
    public Buff buffData;

    public void Init(Buff buff)
    {
        buffData = buff;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.ApplyBuff(buffData.buffType);

            BuffManager.Instance.Despawn(this);
        }
    }

}
