using UnityEngine;

[CreateAssetMenu(fileName = "New Buff", menuName = "Game/Buff")]
public class Buff : ScriptableObject
{
    public string buffName;
    [TextArea] public string description;
    public float statBuff;
    public int buffLevel;
    public Sprite icon; // NEW field for image
}
