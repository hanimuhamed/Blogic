using UnityEngine;
using System.Collections.Generic;

public class ComponentScript : MonoBehaviour
{
    public static int componentIndex = 0;
    private static Dictionary<(int, int), GameObject> LookUp = new Dictionary<(int, int), GameObject>();
    public static readonly string[] PrefabNames = { "Input", "Wire", "Not", "Cross", "Clock", "Eraser", "Marquee" };
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
        TileScript.componentChanged = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetComponentIndex(0);
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetComponentIndex(1);
        }
        else if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetComponentIndex(2);
        }
        else if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetComponentIndex(3);
        }
        else if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetComponentIndex(4);
        }
        else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Alpha6))
        {
            SetComponentIndex(5);
        }
        else if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Alpha7))
        {
            SetComponentIndex(6); // 6 is Marquee
        }
    }
}
