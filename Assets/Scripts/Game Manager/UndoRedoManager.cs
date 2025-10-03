using UnityEngine;
using System.Collections.Generic;

public class UndoRedoManager : MonoBehaviour
{
    public Components components; // Assign in Inspector

    private Stack<string> undoStack = new Stack<string>();
    private Stack<string> redoStack = new Stack<string>();
    private string lastState = "";

    // Pool: one list per prefab index
    private List<GameObject>[] pools;
    private GameManager gameManager;
    private float undoRedoCooldown = 0.25f; // seconds
    private float undoRedoTimer = 0f;

    void Start()
    {
        // Initialize pools
        pools = new List<GameObject>[components.prefabs.Count];
        for (int i = 0; i < pools.Length; i++)
            pools[i] = new List<GameObject>();

        lastState = SerializeLookUp();
        undoStack.Push(lastState);
        gameManager = gameObject.GetComponent<GameManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            string currentState = SerializeLookUp();
            if (currentState != lastState)
            {
                undoStack.Push(currentState);
                redoStack.Clear();
                lastState = currentState;
            }
        }
        undoRedoTimer += Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (undoRedoTimer < undoRedoCooldown)
            {
                return;
            }
            else if (Input.GetKey(KeyCode.Y) || (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Z)))
            {
                Redo();
                undoRedoTimer = 0f;
            }
            else if (Input.GetKey(KeyCode.Z))
            {
                Undo();
                undoRedoTimer = 0f;
            }
        }
    }
    private void Undo()
    {
        if (undoStack.Count > 1)
        {
            redoStack.Push(undoStack.Pop());
            string prevState = undoStack.Peek();
            ApplyStateDelta(prevState);
            lastState = prevState;
            gameManager.compileText.enabled = true;
            gameManager.isCompiled = false;
        }
    }
    private void Redo()
    {
        if (redoStack.Count > 0)
        {
            string redoState = redoStack.Pop();
            undoStack.Push(redoState);
            ApplyStateDelta(redoState);
            lastState = redoState;
            gameManager.compileText.enabled = true;
            gameManager.isCompiled = false;
        }
    }

    private string SerializeLookUp()
    {
        var lookup = ComponentScript.GetAllLookUp();
        var saveList = new List<SaveManager.SavedObject>();
        foreach (var kvp in lookup)
        {
            var obj = kvp.Value;
            if (obj == null) continue;
            int prefabIndex = GetPrefabIndex(obj);
            if (prefabIndex == -1) continue;
            saveList.Add(new SaveManager.SavedObject
            {
                x = kvp.Key.Item1,
                y = kvp.Key.Item2,
                prefabIndex = prefabIndex
            });
        }
        return JsonUtility.ToJson(new SaveManager.Serialization<List<SaveManager.SavedObject>>(saveList));
    }

    // Only update changed objects for fast undo/redo, with pooling
    private void ApplyStateDelta(string json)
    {
        var lookup = ComponentScript.GetAllLookUp();
        var saveList = JsonUtility.FromJson<SaveManager.Serialization<List<SaveManager.SavedObject>>>(json).target;
        var prefabs = components.prefabs;

        // Build dictionaries for fast comparison
        var targetDict = new Dictionary<(int, int), int>();
        foreach (var saved in saveList)
            targetDict[(saved.x, saved.y)] = saved.prefabIndex;

        var toRemove = new List<(int, int)>();
        // Remove or pool objects not in target state or with different prefab
        foreach (var kvp in lookup)
        {
            if (!targetDict.TryGetValue(kvp.Key, out int targetPrefab) || GetPrefabIndex(kvp.Value) != targetPrefab)
            {
                PoolObject(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove)
            lookup.Remove(key);

        // Add missing objects (reuse from pool if possible)
        foreach (var saved in saveList)
        {
            var key = (saved.x, saved.y);
            if (!lookup.ContainsKey(key))
            {
                if (saved.prefabIndex < 0 || saved.prefabIndex >= prefabs.Count) continue;
                var obj = GetFromPoolOrInstantiate(saved.prefabIndex, new Vector3(saved.x, saved.y, 0));
                var popIn = obj.GetComponent<PopIn>();
                if (popIn != null)
                    popIn.skipPop = true;
                ComponentScript.SetLookUp(obj);
            }
        }
    }

    private int GetPrefabIndex(GameObject obj)
    {
        for (int i = 0; i < ComponentScript.PrefabNames.Length; i++)
        {
            if (obj.name.StartsWith(ComponentScript.PrefabNames[i]))
                return i;
        }
        return -1;
    }

    // Pooling helpers
    private void PoolObject(GameObject obj)
    {
        if (obj == null) return;
        int prefabIndex = GetPrefabIndex(obj);
        if (prefabIndex >= 0 && prefabIndex < pools.Length)
        {
            obj.SetActive(false);
            pools[prefabIndex].Add(obj);
        }
        else
        {
            Destroy(obj);
        }
    }

    private GameObject GetFromPoolOrInstantiate(int prefabIndex, Vector3 pos)
    {
        GameObject obj;
        if (pools[prefabIndex].Count > 0)
        {
            obj = pools[prefabIndex][pools[prefabIndex].Count - 1];
            pools[prefabIndex].RemoveAt(pools[prefabIndex].Count - 1);
            obj.transform.position = pos;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(components.prefabs[prefabIndex], pos, Quaternion.identity);
        }
        return obj;
    }
}