using UnityEngine;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    [System.Serializable]
    public class SavedObject
    {
        public int x;
        public int y;
        public int prefabIndex;
        public bool state;
    }

    private List<GameObject> prefabs; // Assign all possible prefabs in Inspector in order: Input, Wire, Not, Cross, Clock
    public Transform workspace; // Assign in Inspector or find at runtime

    void Awake()
    {
        //PlayerPrefs.DeleteKey("LookUpSave");
        //PlayerPrefs.Save();
        prefabs = GetComponent<Components>().prefabs;
        LoadLookUp();
    }

    public static void SaveLookUp()
    {
        var saveList = new List<SavedObject>();
        foreach (var kvp in ComponentScript.GetAllLookUp())
        {
            var obj = kvp.Value;
            if (obj == null) continue;
            if (!obj) continue;

            // Find prefab index by comparing prefab type (assumes prefab order matches type index)
            int prefabIndex = -1;
            for (int i = 0; i < ComponentScript.PrefabNames.Length; i++)
            {
                if (obj.name.StartsWith(ComponentScript.PrefabNames[i]))
                {
                    prefabIndex = i;
                    break;
                }
            }
            if (prefabIndex == -1) continue; // Skip if not found

            saveList.Add(new SavedObject
            {
                x = kvp.Key.Item1,
                y = kvp.Key.Item2,
                prefabIndex = prefabIndex,
                state = obj.GetComponent<SourceComponent>()?.IsOn() ?? false
            });
        }
        string json = JsonUtility.ToJson(new Serialization<List<SavedObject>>(saveList));
        PlayerPrefs.SetString("LookUpSave", json);
        PlayerPrefs.Save();
    }

    public void LoadLookUp()
    {
        string json = PlayerPrefs.GetString("LookUpSave", "");
        if (string.IsNullOrEmpty(json)) return;

        var saveList = JsonUtility.FromJson<Serialization<List<SavedObject>>>(json).target;
        foreach (var saved in saveList)
        {
            if (saved.prefabIndex < 0 || saved.prefabIndex >= prefabs.Count) continue;
            var prefab = prefabs[saved.prefabIndex];
            var obj = Instantiate(prefab, new Vector3(saved.x, saved.y, 0), Quaternion.identity, workspace);
            /*var popIn = obj.GetComponent<PopIn>();
            if (popIn != null)
                popIn.skipPop = true;*/
            ComponentScript.SetLookUp(obj);

            var source = obj.GetComponent<SourceComponent>();
            if (source != null)
            {
                //Debug.Log("Restoring state for SourceComponent at (" + saved.x + ", " + saved.y + ") to " + (saved.state ? "ON" : "OFF"));
                source.SetState(saved.state);
                source.isInitialized = true;
            }
        }
    }

    // Helper for serializing lists
    [System.Serializable]
    public class Serialization<T>
    {
        public T target;
        public Serialization(T target) { this.target = target; }
    }
}