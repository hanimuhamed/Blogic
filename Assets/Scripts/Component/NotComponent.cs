using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NotComponent : SourceComponent
{

    public void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = darkColor;
        errorHighlight = gameObject.transform.GetChild(0).gameObject;
        errorHighlight.SetActive(false);
        allSources.Add(this);
        /*pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );*/
    }
    public override void UpdateState()
    {
        if (inputCluster != null)
        {
            //Debug.Log("Input Cluster State: " + inputCluster.IsOn());
            SetState(!inputCluster.IsOn());
        }
        else
        {
            //Debug.Log("No input cluster at " + pos);
            if (inputSource != null)
            {
                SetState(!inputSource.IsOn());
            }
            else
            {
                //Debug.Log("No input source found for NOT gate at " + pos);
                SetState(true);
            }
        }
        return;
    }
    public override void UpdateNext(Dictionary<(int, int), int> visited)
    {
        Dictionary<(int, int), int> localVisited = new Dictionary<(int, int), int>(visited);
        bool oldState = IsOn();
        UpdateState();
        //Debug.Log(pos + " " + oldState + " -> " + IsOn());
        if (oldState == IsOn()) return;
        foreach (var cluster in connectedClusters)
        {
            if (cluster == null) continue;
            if (localVisited.ContainsKey((gameObject.GetInstanceID(), cluster.GetInstanceID())))
            {
                if (localVisited[(gameObject.GetInstanceID(), cluster.GetInstanceID())] >= 0)
                {
                    //Debug.Log("skip " + GetPos());
                    GameManager.hasCircularDependency = true;
                    errorHighlight.SetActive(true);
                    continue;
                }
                else
                {
                    localVisited[(gameObject.GetInstanceID(), cluster.GetInstanceID())]++;
                }
            }
            else
            {
                localVisited.Add((gameObject.GetInstanceID(), cluster.GetInstanceID()), 0);
            }
            //Debug.Log("Enqueuing UpdateNext for cluster " + cluster.GetInstanceID());
            SimulationDriver.Instance.EnqueueRoutine(() => cluster.UpdateNext(localVisited));
        }
        if (connectedNot != null)
        {
            if (localVisited.ContainsKey((gameObject.GetInstanceID(), connectedNot.GetInstanceID())))
            {
                if (localVisited[(gameObject.GetInstanceID(), connectedNot.GetInstanceID())] >= 0)
                {
                    //Debug.Log("skip " + GetPos());
                    errorHighlight.SetActive(true);
                    GameManager.hasCircularDependency = true;
                    return;
                }
                else 
                {
                    localVisited[(gameObject.GetInstanceID(), connectedNot.GetInstanceID())]++;
                }
            }
            else
            {
                localVisited.Add((gameObject.GetInstanceID(), connectedNot.GetInstanceID()), 0);
            }
           // Debug.Log("Enqueuing UpdateNext for NOT gate " + connectedNot.GetPos());
            SimulationDriver.Instance.EnqueueRoutine(() => connectedNot.UpdateNext(localVisited));
        }
        //Debug.Log("Running all enqueued routines: " + GetPos());
        //SimulationDriver.Instance.RunAll();
        return;
    }
}