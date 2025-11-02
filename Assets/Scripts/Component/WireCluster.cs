using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class WireCluster : MonoBehaviour
{
    public static List<WireCluster> allClusters = new List<WireCluster>();
    public List<WireComponent> wires = new List<WireComponent>();
    public List<NotComponent> connectedNots = new List<NotComponent>();
    public List<SourceComponent> inputSources = new List<SourceComponent>();
    private bool isOn = false;
    public void AddWire(WireComponent wire)
    {
        if (!wires.Contains(wire))
        {
            wires.Add(wire);
            wire.SetCluster(this);
        }
    }
    public void AddToSource(NotComponent source)
    {
        if (!connectedNots.Contains(source))
        {
            connectedNots.Add(source);
            source.AddInputCluster(this);
        }
    }
    public void AddFromSource(SourceComponent source)
    {
        if (!source.connectedClusters.Contains(this))
        {
            inputSources.Add(source);
            source.connectedClusters.Add(this);
        }
    }

    /*public void Initialize()
    {
        if (IsInitialized()) return;
        isVisited = true;
        bool flag = false;
        foreach (var source in inputSources)
        {
            if (source.IsInitialized() == false)
            {
                if (source.IsVisited() == false)
                {
                    source.Initialize();
                }
                else
                {
                    flag = true;
                }
            }
        }
        if (flag == true)
        {
            foreach (var source in inputSources)
            {
                if (source.IsInitialized() == false)
                {
                    if (source.IsVisited() == false)
                    {
                        source.Initialize();
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            if (flag == true) return;
        }
        isInitialized = true;
        UpdateState();
        foreach (var not in connectedNots)
        {
            if (not.IsVisited() == true)
            {
                not.Initialize();
            }
        }
        foreach (var not in connectedNots)
        {
            if (not.IsVisited() == false)
            {
                not.Initialize();
            }
        }
        return;
    }*/
    public virtual void UpdateState()
    {
        foreach (var source in inputSources)
        {
            if (source == null) continue;
            if (source.IsOn() == true)
            {
                SetState(true);
                return;
            }
        }
        SetState(false);
    }

    public void SetState(bool state)
    {
        isOn = state;
        foreach (var wire in wires)
        {
            if (wire == null) continue;
            wire.SetState(state);
        }
        return;
    }
    public void UpdateNext(Dictionary<(int, int), int> visited)
    {
        bool oldState = IsOn();
        UpdateState();
        //Debug.Log(gameObject.GetInstanceID() + " " + oldState + " -> " + IsOn());
        if (oldState == IsOn()) return;
        foreach (var not in connectedNots)
        {
            if (not == null) continue;
            //Debug.Log("Enqueuing UpdateNext for NOT gate " + not.GetInstanceID());
            SimulationDriver.Instance.EnqueueRoutine(() => not.UpdateNext(visited));
        }
        //Debug.Log("Running all enqueued routines: " + gameObject.GetInstanceID());
        //SimulationDriver.Instance.RunAll();
        return;
    }
    public bool IsOn()
    {
        return isOn == true;
    }
    public void Uncluster()
    {
        foreach (var wire in wires)
        {
            if (wire == null) continue;
            WireComponent.unclusturedWires.Add(wire);
        }
        wires.Clear();
        Destroy(this);
    }

}
