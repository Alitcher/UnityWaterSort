using UnityEngine;
using UnityEngine.Pool;

public class BottlePooler : MonoBehaviour
{
    [SerializeField] private ObjectPoolConfig objectPoolConfig;
    public ObjectPool<BottleController> objectPool;
    [SerializeField] private BottleController bottlePrefab;

    void Awake()
    {
        objectPool = new ObjectPool<BottleController>(SpawnBottle, OnGetObjectFromPool, OnReturnObjectToPool, OnDestroyObjectPool, true, objectPoolConfig.defaultCapacity, objectPoolConfig.maxSize);
    }

    private BottleController SpawnBottle()
    {
        BottleController bottle = Instantiate(bottlePrefab, transform.position, transform.rotation, this.transform);
        bottle.SetPool(objectPool);
        bottle.gameObject.SetActive(false);

        return bottle;
    }

    private void OnGetObjectFromPool(BottleController bottle)
    {
        bottle.gameObject.SetActive(true);
    }

    private void OnReturnObjectToPool(BottleController bottle) 
    {
        bottle.gameObject.SetActive(false);
    }

    private void OnDestroyObjectPool(BottleController bottle)
    {
        Destroy(bottle.gameObject);
    }

}
