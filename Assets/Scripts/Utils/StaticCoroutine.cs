using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scripts;

public static class StaticCoroutine
{
    private class CoroutineHolder : MonoBehaviour { }

    //lazy singleton pattern. Note that I don't set it to dontdestroyonload - you usually want coroutines to stop when you load a new scene.
    private static CoroutineHolder _runner;
    private static CoroutineHolder runner
    {
        get
        {
            if (_runner == null)
            {
                _runner = new GameObject("Static coroutine Runner").AddComponent<CoroutineHolder>();
            }
            return _runner;
        }
    }

    public static void StartCoroutine(IEnumerator coroutine)
    {
        runner.StartCoroutine(coroutine);
    }
}