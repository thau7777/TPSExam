using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

/// <summary>
/// Persistent singleton that manages multiple particle effect pools.
/// Register particle prefabs in the inspector and spawn them by ID.
/// </summary>
public class ParticleManager : Singleton<ParticleManager>
{
    [System.Serializable]
    public class ParticlePrefabEntry
    {
        public string id; // Identifier (e.g. "Explosion", "MuzzleFlash", "Smoke")
        public ParticleSystem prefab;
        public int defaultCapacity = 5;
        public int maxCapacity = 20;
    }

    [SerializeField] private List<ParticlePrefabEntry> particlePrefabs = new List<ParticlePrefabEntry>();

    private Dictionary<string, ObjectPool<ParticleSystem>> _pools = new Dictionary<string, ObjectPool<ParticleSystem>>();
    [SerializeField] private Transform _particleParent;

    protected override void Awake()
    {
        base.Awake();
        InitializePools();
    }

    private void InitializePools()
    {
        _pools.Clear();

        // Destroy all children under particle parent to avoid duplicates
        if (_particleParent != null)
        {
            for (int i = _particleParent.childCount - 1; i >= 0; i--)
            {
                Destroy(_particleParent.GetChild(i).gameObject);
            }
        }

        // Create a pool for each prefab entry
        foreach (var entry in particlePrefabs)
        {
            if (entry.prefab == null || string.IsNullOrEmpty(entry.id))
                continue;

            var pool = new ObjectPool<ParticleSystem>(
                () => CreateParticleInstance(entry.prefab),
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

    private ParticleSystem CreateParticleInstance(ParticleSystem prefab)
    {
        var obj = Instantiate(prefab, _particleParent);
        obj.gameObject.SetActive(false);
        return obj;
    }

    private void OnGetFromPool(ParticleSystem ps)
    {
        ps.gameObject.SetActive(true);
        ps.Play(true);
    }

    private void OnReleaseToPool(ParticleSystem ps)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(ParticleSystem ps)
    {
        Destroy(ps.gameObject);
    }

    /// <summary>
    /// Spawns a particle effect by ID at the given position and rotation.
    /// Automatically releases it when finished playing.
    /// </summary>
    public ParticleSystem Spawn(string id, Vector3 position, Quaternion rotation)
    {
        if (!_pools.TryGetValue(id, out var pool))
        {
            Debug.LogWarning($"No particle pool found for ID '{id}'");
            return null;
        }

        var ps = pool.Get();
        ps.transform.SetPositionAndRotation(position, rotation);

        // Schedule release after duration
        StartCoroutine(ReleaseAfterDuration(ps, pool));

        return ps;
    }

    private System.Collections.IEnumerator ReleaseAfterDuration(ParticleSystem ps, ObjectPool<ParticleSystem> pool)
    {
        yield return new WaitForSeconds(ps.main.duration);
        pool.Release(ps);
    }

    /// <summary>
    /// Completely clears and rebuilds all particle pools. 
    /// Call this on game restart.
    /// </summary>
    public void ResetManager()
    {
        // Release all particles and destroy pool objects
        foreach (var kvp in _pools)
        {
            // No direct "Clear" API, so we destroy manually
            // Remaining pooled objects will be garbage-collected
        }

        InitializePools();
    }
}
