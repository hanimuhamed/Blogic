using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;


public class SourceComponent : MonoBehaviour
{
    public static List<SourceComponent> allSources = new List<SourceComponent>();
    public List<WireCluster> connectedClusters = new List<WireCluster>();
    public WireCluster inputCluster;
    public NotComponent connectedNot;
    public SourceComponent inputSource;
    protected bool isOn = false;
    public bool isInitialized = false;
    public Color darkColor = new Color(0.45f, 0.45f, 0.45f);
    //protected Vector2Int pos;
    protected SpriteRenderer sr;
    public GameObject errorHighlight;

    void Awake()
    {

        allSources.Add(this);
        sr = GetComponent<SpriteRenderer>();
        sr.color = darkColor;
        /*pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );*/
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
            //Debug.LogWarning("SpriteRenderer is null on " + gameObject.name);
            return;
        }
        sr.color = IsOn() ? Color.white : darkColor;
        //Debug.Log("SetState: Color change at " + /*pos*/"(" + transform.position.x + ", " + transform.position.y + ")" + " to " + (IsOn() ? "ON" : "OFF"));
        //Debug.Log("Color change at " + pos + " to " + (IsOn() ? "ON" : "OFF"));
        return;
    }
    public void Toggle()
    {
        SetState(!IsOn());
        Dictionary<(int, int), int> visited = new Dictionary<(int, int), int>();
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
        if (connectedNot != null)
        {
            localVisited.Add((gameObject.GetInstanceID(), connectedNot.GetInstanceID()), 0);
            //Debug.Log("Enqueuing UpdateNext for NOT gate " + connectedNot.GetPos());
            SimulationDriver.Instance.EnqueueRoutine(() => connectedNot.UpdateNext(localVisited));
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
    /*public Vector2Int GetPos()
    {
        return pos;
    }*/
    public void SetConnectedNot(NotComponent not)
    {
        connectedNot = not;
    }
}
