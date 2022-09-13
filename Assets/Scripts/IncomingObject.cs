using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncomingObject : Movement
{
    [System.Serializable]
    public enum eObjectType
    {
        Attack,
        NotAttack
    }

    public eObjectType ObjectType;
    public IncomingObjectPool Pool;
    
    [SerializeField] float Duration = 15.0f;
    float Timer;

    public void Activate()
    {
        Timer = 0.0f;
        gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        Timer += Time.deltaTime;

        if (Timer > Duration)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        Pool.ReturnToPool(this);
        gameObject.SetActive(false);
    }
}
