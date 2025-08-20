using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class EnemyManager : Singleton<EnemyManager>
{
    [System.Serializable]
    public class EnemyPrefabEntry
    {
        public string id;
        public GameObject prefab;
        public int defaultCapacity = 5;
        public int maxCapacity = 20;
    }

    [SerializeField] private List<EnemyPrefabEntry> enemyPrefabs = new List<EnemyPrefabEntry>();
    private Dictionary<string, ObjectPool<GameObject>> _pools = new Dictionary<string, ObjectPool<GameObject>>();
    [SerializeField] private Transform _enemyParent;
    [SerializeField] private int _enemyDamage = 10;
    [SerializeField] private int _enemyMaxHealth = 100;

    // 🔑 Track active enemies
    private HashSet<GameObject> _activeEnemies = new HashSet<GameObject>();

    private void OnEnable()
    {
        GameManager.Instance.onPickupKillAllEnemiesBuff += KillAllEnemies;
        GameManager.Instance.onMinuteIncrease += IncreaseEnemyStatPerMinute; // Example usage, adjust as needed
    }
    private void OnDisable()
    {
        GameManager.Instance.onPickupKillAllEnemiesBuff -= KillAllEnemies;
        GameManager.Instance.onMinuteIncrease -= IncreaseEnemyStatPerMinute; // Example usage, adjust as needed
    }

    protected override void Awake()
    {
        base.Awake();
        foreach (var entry in enemyPrefabs)
        {
            if (entry.prefab == null || string.IsNullOrEmpty(entry.id))
                continue;

            var pool = new ObjectPool<GameObject>(
                () => CreateEnemyInstance(entry.prefab, entry.id),
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPoolObject,
                false,
                entry.defaultCapacity,
                entry.maxCapacity
            );

            _pools[entry.id] = pool;
        }
    }

    private GameObject CreateEnemyInstance(GameObject prefab, string id)
    {
        var obj = Instantiate(prefab, _enemyParent);
        obj.SetActive(false);

        var pooledEnemy = obj.GetComponent<EnemyAI>();
        pooledEnemy.poolID = id;

        return obj;
    }

    private void OnGetFromPool(GameObject obj)
    {
        obj.SetActive(true);
        _activeEnemies.Add(obj);

        var ai = obj.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.ResetEnemy(); // 👈 Reset dead state & re-enable movement
            ai.ResetFade();
            ai.ResetBaseOffset();

            ai.GetComponentInChildren<EnemyAttack>().attackDamage = _enemyDamage;
            ai.GetComponent<Damageable>().MaxHealth = _enemyMaxHealth;
            ai.GetComponent<Damageable>().ResetHealth();
        }
    }


    private void OnReleaseToPool(GameObject obj)
    {
        obj.SetActive(false);
        _activeEnemies.Remove(obj); // ✅ remove from active set
    }

    private void OnDestroyPoolObject(GameObject obj)
    {
        _activeEnemies.Remove(obj); // ✅ cleanup
        Destroy(obj);
    }

    public GameObject Spawn(string id, Vector3 position, Quaternion rotation)
    {
        if (!_pools.TryGetValue(id, out var pool))
        {
            Debug.LogWarning($"No enemy pool found for ID '{id}'");
            return null;
        }

        var enemy = pool.Get();
        enemy.transform.SetPositionAndRotation(position, rotation);
        return enemy;
    }

    public GameObject SpawnRandom(Vector3 position, Quaternion rotation)
    {
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("No enemy prefabs registered in EnemyManager.");
            return null;
        }

        var entry = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        return Spawn(entry.id, position, rotation);
    }

    public void Release(string id, GameObject enemy)
    {
        GameManager.Instance.OnEnemyDeath(10);
        if (_pools.TryGetValue(id, out var pool))
        {
            pool.Release(enemy);
        }
        else
        {
            Debug.LogWarning($"No pool found for enemy ID '{id}'");
            Destroy(enemy);
        }
    }

    public void IncreaseEnemyStatPerMinute(int percent)
    {
        _enemyDamage += _enemyDamage * (percent / 100);
        _enemyMaxHealth += _enemyMaxHealth * (percent / 100);
    }

    // 🔥 Kill all active enemies
    public void KillAllEnemies()
    {
        // Copy to array first to avoid modifying collection during iteration
        var enemies = new List<GameObject>(_activeEnemies);

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            var ai = enemy.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.Die(); // 👈 let the enemy handle its own cleanup
            }
            else
            {
                // fallback if no SwarmAI is found
                enemy.SetActive(false);
                _activeEnemies.Remove(enemy);
            }
        }

    }

    public void ResetManager()
    {
        // 1. Kill / despawn all active enemies
        var enemies = new List<GameObject>(_activeEnemies);
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            var ai = enemy.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.ForceDespawn(); // 👈 create a helper in EnemyAI to just return to pool without Die()
            }
            else
            {
                enemy.SetActive(false);
            }

            _activeEnemies.Remove(enemy);
        }

        // 2. Clear all pools
        foreach (var pool in _pools.Values)
        {
            pool.Clear(); // destroys everything inside pool
        }

        _pools.Clear();

        // 3. Rebuild pools from enemyPrefabs
        foreach (var entry in enemyPrefabs)
        {
            if (entry.prefab == null || string.IsNullOrEmpty(entry.id))
                continue;

            var pool = new ObjectPool<GameObject>(
                () => CreateEnemyInstance(entry.prefab, entry.id),
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPoolObject,
                false,
                entry.defaultCapacity,
                entry.maxCapacity
            );

            _pools[entry.id] = pool;
        }

        // 4. Reset stats if needed
        _enemyDamage = 10;
        _enemyMaxHealth = 100;
    }
    


}
