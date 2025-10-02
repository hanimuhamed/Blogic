using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NotComponent : SourceComponent
{
    public void Start()
    {
        pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );
        //allSources.Add(this);
        sr = GetComponent<SpriteRenderer>();
        sr.color = darkColor;
        errorHighlight = gameObject.transform.GetChild(0).gameObject;
        errorHighlight.SetActive(false);
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
            SourceComponent source = ComponentScript.GetLookUp(pos.x - 1, pos.y)?.GetComponent<SourceComponent>();
            if (source != null)
            {
                SetState(!source.IsOn());
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
        NotComponent nextNot = ComponentScript.GetLookUp(pos.x + 1, pos.y)?.GetComponent<NotComponent>();
        if (nextNot != null)
        {
            if (localVisited.ContainsKey((gameObject.GetInstanceID(), nextNot.GetInstanceID())))
            {
                if (localVisited[(gameObject.GetInstanceID(), nextNot.GetInstanceID())] >= 0)
                {
                    //Debug.Log("skip " + GetPos());
                    errorHighlight.SetActive(true);
                    GameManager.hasCircularDependency = true;
                    return;
                }
                else 
                {
                    localVisited[(gameObject.GetInstanceID(), nextNot.GetInstanceID())]++;
                }
            }
            else
            {
                localVisited.Add((gameObject.GetInstanceID(), nextNot.GetInstanceID()), 0);
            }
           // Debug.Log("Enqueuing UpdateNext for NOT gate " + nextNot.GetPos());
            SimulationDriver.Instance.EnqueueRoutine(() => nextNot.UpdateNext(localVisited));
        }
        //Debug.Log("Running all enqueued routines: " + GetPos());
        //SimulationDriver.Instance.RunAll();
        return;
    }
}