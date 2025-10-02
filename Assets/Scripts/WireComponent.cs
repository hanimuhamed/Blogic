using UnityEngine;
using System.Collections.Generic;

public class WireComponent : MonoBehaviour
{
    private WireCluster cluster;
    public static List<WireComponent> unclusturedWires = new List<WireComponent>();
    public static Color darkColor = new Color(0.5f, 0.5f, 0.5f);
    //public static Color nullColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    private bool isOn = false;
    public SpriteRenderer sr;
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        //unclusturedWires.Add(this);
        sr.color = darkColor;
    }

    public void SetCluster(WireCluster c)
    {
        cluster = c;
    }

    public void DFSConnect()
    {
        WireCluster newCluster = new GameObject("WireCluster").AddComponent<WireCluster>();
        SetCluster(newCluster);
        WireCluster.allClusters.Add(newCluster);
        var visited = new HashSet<Vector2Int>();
        Vector2Int start = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );
        DFS(start, visited, newCluster);
    }

    private void DFS(Vector2Int pos, HashSet<Vector2Int> visited, WireCluster cluster)
    {
        if (visited.Contains(pos)) return;
        visited.Add(pos);

        GameObject obj = ComponentScript.GetLookUp(pos.x, pos.y);
        if (obj == null) return;
        var wire = obj.GetComponent<WireComponent>();
        if (wire == null) return; // Only continue DFS if on a wire
        unclusturedWires.Remove(wire);
        wire.GetComponent<SpriteRenderer>().color = darkColor;
        cluster.AddWire(wire);

        // Check for InputComponent and NotComponent (any direction)
        foreach (var dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        {
            Vector2Int neighborPos = pos + dir;
            GameObject neighborObj = ComponentScript.GetLookUp(neighborPos.x, neighborPos.y);
            if (neighborObj == null) continue;

            // --- CrossComponent pass-through logic ---
            // If we hit a cross, keep moving in the same direction until we hit a non-cross or null
            while (neighborObj != null && neighborObj.GetComponent<CrossComponent>() != null)
            {
                neighborPos += dir;
                neighborObj = ComponentScript.GetLookUp(neighborPos.x, neighborPos.y);
            }
            if (neighborObj == null) continue;

            var input = neighborObj.GetComponent<InputComponent>();
            if (input != null && !input.connectedClusters.Contains(cluster))
            {
                cluster.AddFromSource(input);
            }
            var clock = neighborObj.GetComponent<ClockComponent>();
            if (clock != null && !clock.connectedClusters.Contains(cluster))
            {
                cluster.AddFromSource(clock);
            }

            var not = neighborObj.GetComponent<NotComponent>();
            if (not != null)
            {
                if (dir == Vector2Int.right)
                {
                    if (!cluster.connectedNots.Contains(not))
                    {
                        cluster.AddToSource(not);
                    }
                }
                else if (dir == Vector2Int.left)
                {
                    cluster.AddFromSource(not);
                }
            }

            // Continue DFS for wires only (after passing through crosses)
            var nextWire = neighborObj.GetComponent<WireComponent>();
            if (nextWire != null && !visited.Contains(neighborPos))
            {
                DFS(neighborPos, visited, cluster);
            }
        }
    }

    public static void ResetUnclusturedWires()
    {
        unclusturedWires.Clear();
        foreach (var obj in ComponentScript.GetAllLookUp().Values)
        {
            if (obj == null) continue;
            var wire = obj.GetComponent<WireComponent>();

            if (wire != null)
            {
                unclusturedWires.Add(wire);
            }
        }
    }
    public bool IsOn()
    {
        return isOn;
    }
    public void SetState(bool state)
    {
        isOn = state;
        if (sr == null) return;
        sr.color = IsOn() ? Color.white : darkColor;
        return;
    }
    public void NullifyState()
    {
        if (sr != null)
            sr.color = darkColor;
        isOn = false;
        return;
    }
    public void OnEnable()
    {
        NullifyState();
    }
}