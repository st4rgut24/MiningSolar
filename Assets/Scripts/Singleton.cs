using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T  : MonoBehaviour
{
    public static T instance { get; private set; }

    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(this); //Or GameObject as appropriate
            return;
        }
        instance = gameObject.GetComponent<T>();
    }
}
