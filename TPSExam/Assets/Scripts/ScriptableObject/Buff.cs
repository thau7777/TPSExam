using UnityEngine;

[CreateAssetMenu(fileName = "New Buff", menuName = "Game/Buff")]
public class Buff : ScriptableObject
{
    public enum BuffType
    {
        Speed,
        BulletDamage,
        GrenadeDamage,
        MaxHealth,
        Ammo,
        ReloadSpeed,
        InfinityBullet,
        InfinityGrenade,
        HealthRegen,
        InstantHeal,
        KillAllEnemies,
    }
    public string buffName;
    [TextArea] public string description;
    public BuffType buffType;
    public Sprite icon; // NEW field for image
}
