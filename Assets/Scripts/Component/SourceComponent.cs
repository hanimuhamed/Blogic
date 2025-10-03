using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;


public class SourceComponent : MonoBehaviour
{
    public static List<SourceComponent> allSources = new List<SourceComponent>();
    public List<WireCluster> connectedClusters = new List<WireCluster>();
    public WireCluster inputCluster;
    protected bool isOn = false;
    public bool isInitialized = false;
    public bool isVisited = false;
    public Color darkColor = new Color(0.6f, 0.6f, 0.6f);
    protected Vector2Int pos;
    protected SpriteRenderer sr;
    protected GameObject errorHighlight;

    void Start()
    {
        
        //allSources.Add(this);
        sr = GetComponent<SpriteRenderer>();
        sr.color = darkColor;
    }
    public void AddWire(WireCluster wire)
    {
        if (!connectedClusters.Contains(wire))
        {
            connectedClusters.Add(wire);
        }
    }
    public void AddInputCluster(WireCluster cluster)
    {
        if (inputCluster == null)
        {
            inputCluster = cluster;
        }
    }

    /*public void Initialize()
    {
        if (IsInitialized()) return;
        isVisited = true;
        bool flag = false;
        if (inputCluster != null)
        {
            if (inputCluster.IsInitialized() == false)
            {
                if (inputCluster.IsVisited() == false)
                {
                    inputCluster.Initialize();
                }
                else
                {
                    flag = true;
                }
            }
        }
        else
        {
            SourceComponent source = ComponentScript.GetLookUp(pos.x - 1, pos.y)?.GetComponent<SourceComponent>();
            if (inputCluster != null)
            {
                if (inputCluster.IsInitialized() == false)
                {
                    if (inputCluster.IsVisited() == false)
                    {
                        inputCluster.Initialize();
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
        }
        if (flag == true) return;
        isInitialized = true;
        UpdateState();
        foreach (var cluster in connectedClusters)
        {
            if (cluster.IsVisited() == true)
                cluster.Initialize();
        }
        NotComponent nextNot = ComponentScript.GetLookUp(pos.x + 1, pos.y)?.GetComponent<NotComponent>();
        if (nextNot != null)
        {
            if (nextNot.IsVisited() == true)
                nextNot.Initialize();
        }
        foreach (var cluster in connectedClusters)
        {
            if (cluster.IsVisited() == false)
                cluster.Initialize();
        }
        if (nextNot != null)
        {
            if (nextNot.IsVisited() == false)
                nextNot.Initialize();
        }
        return;
    }*/
    public virtual void UpdateState()
    {
        SetState(IsOn());
        return;
    }
    public void SetState(bool state)
    {
        isOn = state;
        //Debug.Log("SetState called at " + pos + " to " + (IsOn() ? "ON" : "OFF"));
        if (sr == null)
        {
            return;
        }
        sr.color = IsOn() ? Color.white : darkColor;
        //Debug.Log("Color change at " + pos + " to " + (IsOn() ? "ON" : "OFF"));
        return;
    }
    public void Toggle()
    {
        Dictionary<(int, int), int> visited = new Dictionary<(int, int), int>();
        SetState(!IsOn());
        SimulationDriver.Instance.EnqueueRoutine(() => UpdateNext(visited));
        SimulationDriver.Instance.RunAll();
        return;
    }
    public virtual void UpdateNext(Dictionary<(int, int), int> visited)
    {
        isInitialized = true;
        Dictionary<(int, int), int> localVisited = new Dictionary<(int, int), int>(visited);
        SetState(IsOn());
        //Debug.Log(pos + " " + !IsOn() + " -> " + IsOn());
        foreach (var cluster in connectedClusters)
        {
            if (cluster == null) continue;
            localVisited.Add((gameObject.GetInstanceID(), cluster.GetInstanceID()), 0);
            //Debug.Log("Enqueuing UpdateNext for cluster " + cluster.GetInstanceID());
            SimulationDriver.Instance.EnqueueRoutine(() => cluster.UpdateNext(localVisited));
        }
        pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );
        NotComponent nextNot = ComponentScript.GetLookUp(pos.x + 1, pos.y)?.GetComponent<NotComponent>();
        if (nextNot != null)
        {
            localVisited.Add((gameObject.GetInstanceID(), nextNot.GetInstanceID()), 0);
            //Debug.Log("Enqueuing UpdateNext for NOT gate " + nextNot.GetPos());
            SimulationDriver.Instance.EnqueueRoutine(() => nextNot.UpdateNext(localVisited));
        }
        //Debug.Log("Running all enqueued routines: " + pos);
        //SimulationDriver.Instance.RunAll();
        return;
    }

    public bool IsOn()
    {
        return isOn == true;
    }
    public bool IsInitialized()
    {
        return isInitialized;
    }
    public bool IsVisited()
    {
        return isVisited;
    }

    public static void Refresh()
    {
        foreach (var cluster in WireCluster.allClusters)
        {
            if (cluster == null) continue;
            cluster.isInitialized = false;
            cluster.isVisited = false;
            //cluster.NullifyState();

        }
        foreach (var source in allSources)
        {
            if (source == null) continue;
            //if (!(source is NotComponent)) continue;
            source.isInitialized = false;
            source.isVisited = false;
            source.connectedClusters.Clear();
            source.inputCluster = null;
            if (!(source is NotComponent)) continue;  
            source.errorHighlight.SetActive(false); 
            //source.NullifyState();
        }
        return;
    }

    /*public void NullifyState()
    {
        if (sr != null)
            sr.color = darkColor;
        isOn = false;
        return;
    }
    public void OnEnable()
    {
        NullifyState();
    }*/
    public Vector2Int GetPos()
    {
        return pos;
    }
}
