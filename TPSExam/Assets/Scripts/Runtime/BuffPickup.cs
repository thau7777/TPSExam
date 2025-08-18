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
            //var playerBuffs = other.GetComponent<PlayerBuffHandler>();
            //if (playerBuffs != null)
            //{
            //    playerBuffs.ApplyBuff(buffData);
            //}

            BuffManager.Instance.Despawn(this);
        }
    }
}
