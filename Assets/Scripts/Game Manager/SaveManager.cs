using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;

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

    public static int saveSlot = 0;
    public static string SavePath => Application.persistentDataPath + $"/lookup_save_{saveSlot}.json";
    public static string MetaPath => Application.persistentDataPath + $"/lookup_save_meta_{saveSlot}.json";
    public TextMeshProUGUI fileIndicatorText;

    void Awake()
    {
        if (PlayerPrefs.HasKey("SaveSlot"))
            saveSlot = PlayerPrefs.GetInt("SaveSlot");
        else
            saveSlot = 0;

        prefabs = GetComponent<Components>().prefabs;
        ComponentScript.ClearLookUp();
        SourceComponent.allSources.Clear();
        WireCluster.allClusters.Clear();
        WireComponent.allWires.Clear();
        WireComponent.unclusturedWires.Clear();
        LoadLookUp();
        fileIndicatorText.text = $"#{saveSlot}";
    }

    void Update()
    {
        // Listen for LShift + 0-9 to change save slot and reload scene
        for (int i = 0; i <= 9; i++)
        {
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (shiftHeld && Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                saveSlot = i;
                PlayerPrefs.SetInt("SaveSlot", saveSlot); // Save the current slot
                PlayerPrefs.Save();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    public static void SaveLookUp()
    {
        var saveList = new List<SavedObject>();
        foreach (var kvp in ComponentScript.GetAllLookUp())
        {
            var obj = kvp.Value;
            if (obj == null) continue;
            if (!obj) continue;
            int prefabIndex = -1;
            for (int i = 0; i < ComponentScript.PrefabNames.Length; i++)
            {
                if (obj.name.StartsWith(ComponentScript.PrefabNames[i]))
                {
                    prefabIndex = i;
                    break;
                }
            }
            if (prefabIndex == -1) continue;
            saveList.Add(new SavedObject
            {
                x = kvp.Key.Item1,
                y = kvp.Key.Item2,
                prefabIndex = prefabIndex,
                state = obj.GetComponent<SourceComponent>()?.IsOn() ?? false
            });
        }
        string json = JsonUtility.ToJson(new Serialization<List<SavedObject>>(saveList));
        File.WriteAllText(SavePath, json);
    }

    public void LoadLookUp()
    {
        if (!File.Exists(SavePath)) return;
        string json = File.ReadAllText(SavePath);
        if (string.IsNullOrEmpty(json)) return;
        var saveList = JsonUtility.FromJson<Serialization<List<SavedObject>>>(json).target;
        foreach (var saved in saveList)
        {
            if (saved.prefabIndex < 0 || saved.prefabIndex >= prefabs.Count) continue;
            var prefab = prefabs[saved.prefabIndex];
            var obj = Instantiate(prefab, new Vector3(saved.x, saved.y, 0), Quaternion.identity, workspace);
            ComponentScript.SetLookUp(obj);
            var source = obj.GetComponent<SourceComponent>();
            if (source != null)
            {
                source.SetState(saved.state);
                source.isInitialized = true;
            }
        }
    }

    [System.Serializable]
    public class Serialization<T>
    {
        public T target;
        public Serialization(T target) { this.target = target; }
    }
}