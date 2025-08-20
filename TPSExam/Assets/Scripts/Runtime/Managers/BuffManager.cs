using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class BuffManager : Singleton<BuffManager>
{
    [System.Serializable]
    public class BuffPrefabEntry
    {
        public Buff.BuffType type;
        public BuffPickup prefab;
        public int defaultCapacity = 5;
        public int maxCapacity = 20;
        public int ratio;
    }

    [SerializeField] private List<BuffPrefabEntry> buffPrefabs = new List<BuffPrefabEntry>();

    private Dictionary<Buff.BuffType, ObjectPool<BuffPickup>> _pools
        = new Dictionary<Buff.BuffType, ObjectPool<BuffPickup>>();
    private Dictionary<BuffPickup, Buff.BuffType> _prefabToType
        = new Dictionary<BuffPickup, Buff.BuffType>();

    protected override void Awake()
    {
        base.Awake();
        foreach (var entry in buffPrefabs)
        {
            if (entry.prefab == null) continue;

            var pool = new ObjectPool<BuffPickup>(
                () => CreateInstance(entry.prefab, entry.type),
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPoolObject,
                false,
                entry.defaultCapacity,
                entry.maxCapacity
            );

            _pools[entry.type] = pool;
            _prefabToType[entry.prefab] = entry.type;
        }
    }

    private BuffPickup CreateInstance(BuffPickup prefab, Buff.BuffType type)
    {
        var obj = Instantiate(prefab);
        obj.SetBuffType(type); // 👈 new helper in BuffPickup (see below)
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

    public BuffPickup SpawnByType(Vector3 position, Quaternion rotation, Buff.BuffType type, Transform parent = null)
    {
        if (!_pools.ContainsKey(type))
        {
            Debug.LogWarning($"No Buff prefab registered for type {type}");
            return null;
        }

        var pickup = _pools[type].Get();
        pickup.transform.SetPositionAndRotation(position, rotation);
        if (parent != null) pickup.transform.SetParent(parent);

        return pickup;
    }

    public BuffPickup SpawnRandom(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (buffPrefabs.Count == 0)
        {
            Debug.LogWarning("No Buff prefabs registered in BuffManager!");
            return null;
        }

        // 1. Calculate total weight
        int totalRatio = 0;
        foreach (var entry in buffPrefabs)
        {
            if (_pools.ContainsKey(entry.type)) // skip invalid
                totalRatio += entry.ratio;
        }

        if (totalRatio <= 0)
        {
            Debug.LogWarning("All buff ratios are zero or invalid!");
            return null;
        }

        // 2. Pick a random number within total weight
        int randomValue = Random.Range(0, totalRatio);
        int cumulative = 0;

        // 3. Find which buff it falls into
        foreach (var entry in buffPrefabs)
        {
            if (!_pools.ContainsKey(entry.type)) continue;

            cumulative += entry.ratio;
            if (randomValue < cumulative)
            {
                return SpawnByType(position, rotation, entry.type, parent);
            }
        }

        return null; // Should never hit here
    }


    public void Despawn(BuffPickup pickup)
    {
        if (pickup == null) return;

        var type = pickup.BuffType;
        if (_pools.ContainsKey(type))
        {
            _pools[type].Release(pickup);
        }
        else
        {
            Debug.LogWarning($"Tried to despawn {pickup.name}, but no pool was found. Destroying instead.");
            Destroy(pickup.gameObject);
        }
    }

    public void ResetManager()
    {
        // Clear all active buffs in the scene
        foreach (var pool in _pools.Values)
        {
            pool.Clear(); // releases + destroys all pooled objects
        }

        _pools.Clear();
        _prefabToType.Clear();

        // Rebuild pools from buffPrefabs
        foreach (var entry in buffPrefabs)
        {
            if (entry.prefab == null) continue;

            var pool = new ObjectPool<BuffPickup>(
                () => CreateInstance(entry.prefab, entry.type),
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPoolObject,
                false,
                entry.defaultCapacity,
                entry.maxCapacity
            );

            _pools[entry.type] = pool;
            _prefabToType[entry.prefab] = entry.type;
        }
    }

}
