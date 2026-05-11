// PrefabPool.cs
// ABOUTME: General-purpose pool for any GameObject prefab using Unity's
// built-in ObjectPool<T>. Teaches: object pooling, the four pool callbacks,
// how pooled objects get a reference back to their pool, and why pooling
// matters for performance in high-frequency spawn scenarios.

using UnityEngine;
using UnityEngine.Pool;

public class PrefabPool : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject prefab;

    [Header("Pool Settings")]
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 20;

    private ObjectPool<GameObject> objectPool;

    private void Awake()
    {
        objectPool = new ObjectPool<GameObject>(
            CreateObject,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPooledObject,
            collectionCheck,
            defaultCapacity,
            maxSize
        );
    }

    public int CountActive => objectPool.CountActive;

    public GameObject Get()
    {
        return objectPool.Get();
    }

    public void Release(GameObject gameObject)
    {
        objectPool.Release(gameObject);
    }

    private GameObject CreateObject()
    {
        GameObject instance = Instantiate(prefab);
        instance.SetActive(false);

    if (instance.TryGetComponent<SoldierController>(out SoldierController soldier))
    {
            soldier.SetPool(this);
        }

        if (instance.TryGetComponent<DangerZoneController>(out DangerZoneController zone))
        {
            zone.SetPool(this);
        }

        return instance;
    }

    private void OnGetFromPool(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }

    private void OnReleaseToPool(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }

    private void OnDestroyPooledObject(GameObject gameObject)
    {
        Destroy(gameObject);
    }
}
