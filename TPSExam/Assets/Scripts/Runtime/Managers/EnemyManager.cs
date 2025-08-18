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

    [SerializeField] private float _enemyDamage = 10f;
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

        var pooledEnemy = obj.GetComponent<SwarmAI>();
        pooledEnemy.poolID = id;

        return obj;
    }

    private void OnGetFromPool(GameObject obj)
    {
        obj.SetActive(true);
        var ai = obj.GetComponent<SwarmAI>();
        if (ai != null)
        {
            ai.ResetFade();
            ai.ResetBaseOffset();
            ai.GetComponentInChildren<EnemyAttack>().attackDamage = _enemyDamage;
            ai.GetComponent<Damageable>().ResetHealth();
            var col = ai.GetComponent<Collider>();
            if (col != null)
                col.enabled = true; // Re-enable collider on spawn
        }
    }



    private void OnReleaseToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyPoolObject(GameObject obj)
    {
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
       
        //if(id == "Bee")
        //{
        //    var newPosition = new Vector3(position.x,position.y + 1.3f, position.z);
        //    position = newPosition;
        //}
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

    public void IncreaseEnemyDamage(float percent)
    {
        _enemyDamage += _enemyDamage * (percent / 100f);
    }
}
