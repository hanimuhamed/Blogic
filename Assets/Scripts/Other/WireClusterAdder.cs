using UnityEngine;

public class WireClusterAdder : MonoBehaviour
{
    void Awake()
    {
        WireCluster wireCluster = GetComponent<WireCluster>();
        foreach (Transform wire in transform)
        {
            if (wire == null) continue;
            WireComponent wireComp = wire.GetComponent<WireComponent>();
            if (wireComp == null) continue;
            wireCluster.AddWire(wireComp);
        }
    }
}
