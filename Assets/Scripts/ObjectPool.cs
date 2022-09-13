using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] T ObjectToPool;
    [SerializeField] int NumberToPool;
    [SerializeField] Transform ParentTransform;
    Queue<T> PooledObjects;

    void Awake()
    {
        PooledObjects = new Queue<T>();

        PooledObjects.Enqueue(ObjectToPool);
        ObjectToPool.gameObject.SetActive(false);

        for (int i = 0; i < NumberToPool; i++)
        {
            AddNewToPool();
        }
    }

    void AddNewToPool()
    {
        var newObject = Instantiate(ObjectToPool, ParentTransform);
        PooledObjects.Enqueue(newObject);
        newObject.gameObject.SetActive(false);
    }

    public T GetFromPool()
    {
        if (PooledObjects.Count > 0)
        {
            return PooledObjects.Dequeue();
        }
        else
        {
            return Instantiate(ObjectToPool, ParentTransform);
        }
    }

    public void ReturnToPool(T returnedObject)
    {
        returnedObject.gameObject.SetActive(false);
        PooledObjects.Enqueue(returnedObject);
    }
}