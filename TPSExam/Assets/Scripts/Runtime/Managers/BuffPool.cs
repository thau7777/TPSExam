using UnityEngine;
using UnityEngine.Pool; // Required for ObjectPool<T>

public class BuffPool : PersistentSingleton<BuffPool>
{
    [SerializeField] private BuffPickup prefab;
    private ObjectPool<BuffPickup> pool;

    protected override void Awake()
    {
        base.Awake();
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

    public BuffPickup Spawn(Vector3 position, Buff buff)
    {
        BuffPickup pickup = pool.Get();
        pickup.transform.position = position;
        pickup.Init(buff);
        return pickup;
    }

    public void Despawn(BuffPickup pickup)
    {
        pool.Release(pickup);
    }
}
