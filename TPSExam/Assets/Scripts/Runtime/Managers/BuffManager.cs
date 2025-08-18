using UnityEngine;
using UnityEngine.Pool;
using System.Linq;
using System.Collections.Generic;

public class BuffManager : PersistentSingleton<BuffManager>
{
    [SerializeField] private BuffPickup prefab;
    private ObjectPool<BuffPickup> pool;

    [SerializeField] private List<Buff> allBuffs;

    protected override void Awake()
    {
        base.Awake();

        // Load all Buff ScriptableObjects from the project
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Buff");
        allBuffs = guids.Select(guid =>
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Buff>(path);
        }).ToList();

        pool = new ObjectPool<BuffPickup>(
            CreateFunc,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPoolObject,
            false, // collectionCheck: disable if you want more speed
            10,    // default capacity
            100    // max size
        );
    }

    private BuffPickup CreateFunc()
    {
        var obj = Instantiate(prefab);
        obj.gameObject.SetActive(false);
        return obj;
    }

    private void OnGetFromPool(BuffPickup obj)
    {
        obj.gameObject.SetActive(true);
    }

    private void OnReleaseToPool(BuffPickup obj)
    {
        obj.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(BuffPickup obj)
    {
        Destroy(obj.gameObject);
    }

    /// <summary>
    /// Spawn a BuffPickup with a specific Buff.
    /// </summary>
    public BuffPickup Spawn(Vector3 position, Buff buff)
    {
        BuffPickup pickup = pool.Get();
        pickup.transform.position = position;
        pickup.Init(buff);
        return pickup;
    }

    /// <summary>
    /// Spawn a BuffPickup with a random Buff.
    /// </summary>
    public BuffPickup SpawnRandom(Vector3 position)
    {
        if (allBuffs == null || allBuffs.Count == 0)
        {
            Debug.LogWarning("No Buffs found in project!");
            return null;
        }

        Buff randomBuff = allBuffs[Random.Range(0, allBuffs.Count)];
        return Spawn(position, randomBuff);
    }

    /// <summary>
    /// Spawn a BuffPickup by BuffType (if you want a specific type).
    /// </summary>
    public BuffPickup SpawnByType(Vector3 position, Buff.BuffType type)
    {
        var candidates = allBuffs.Where(b => b.buffType == type).ToList();
        if (candidates.Count == 0)
        {
            Debug.LogWarning($"No Buff of type {type} found!");
            return null;
        }

        Buff chosenBuff = candidates[Random.Range(0, candidates.Count)];
        return Spawn(position, chosenBuff);
    }

    public void Despawn(BuffPickup pickup)
    {
        pool.Release(pickup);
    }

}
