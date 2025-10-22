using UnityEngine;
using System.Collections.Generic;

public class ComponentScript : MonoBehaviour
{
    public static int componentIndex = 1; // Default to Wire
    private static Dictionary<(int, int), GameObject> LookUp = new Dictionary<(int, int), GameObject>();
    public static readonly string[] PrefabNames = { "Input", "Wire", "Not", "Cross", "Clock", "Eraser", "Marquee" };
    
    public static void ClearLookUp()
    {
        LookUp.Clear();
    }
    public static GameObject GetLookUp(int x, int y)
    {
        try
        {
            return LookUp[(x, y)];
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public static Dictionary<(int, int), GameObject> GetAllLookUp()
    {
        return LookUp;
    }
    public static void SetLookUp(GameObject obj)
    {
        int x = Mathf.RoundToInt(obj.transform.position.x);
        int y = Mathf.RoundToInt(obj.transform.position.y);
        LookUp[(x, y)] = obj;
        //SaveManager.SaveLookUp();
    }
    public static void SetComponentIndex(int index)
    {
        componentIndex = index;
        //TileScript.componentChanged = true;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl)) return;
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Q))
        {
            SetComponentIndex(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.B))
        {
            SetComponentIndex(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.N))
        {
            SetComponentIndex(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.X))
        {
            SetComponentIndex(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.C))
        {
            SetComponentIndex(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.E))
        {
            SetComponentIndex(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.M))
        {
            SetComponentIndex(6); // 6 is Marquee
        }
    }
}
