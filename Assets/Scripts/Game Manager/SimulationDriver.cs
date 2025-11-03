using System.Collections.Generic;
using UnityEngine;
using System;

public class SimulationDriver : MonoBehaviour
{
    private static SimulationDriver _instance;
    public GameManager gameManager;
    public static SimulationDriver Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SimulationDriver");
                _instance = go.AddComponent<SimulationDriver>();
                //DontDestroyOnLoad(go);
                _instance.gameManager = GameObject.FindFirstObjectByType<GameManager>();
            }
            return _instance;
        }
    }

    private readonly Queue<Action> routineQueue = new Queue<Action>();
    
    public void EnqueueRoutine(Action routine)
    {
        routineQueue.Enqueue(routine);
    }

    public void RunAll()
    {
        while (routineQueue.Count > 0 && !GameManager.hasCircularDependency)
        {
            //Debug.Log("Running a routine..." + System.DateTime.Now.ToString("hh:mm:ss.fff"));
            var routine = routineQueue.Dequeue();
            routine?.Invoke();
        }
        //Debug.Log("Clearing remaining routines. Count: " + routineQueue.Count);
        routineQueue.Clear();
        if (GameManager.hasCircularDependency)
        {
            if (gameManager == null) return;
            gameManager.isCompiled = false;
            gameManager.compileText.text = "Circular dependency detected!";
            gameManager.compileText.enabled = true;
            gameManager.compileText.color = Color.yellow;
        }
        return;
    }
}
