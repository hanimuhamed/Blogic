using UnityEngine;
using System.IO;

public class TileSpawner : MonoBehaviour
{
    public GameObject tile;
    public int tileSize = 8;
    public Transform tileSpace;
    public static int width = 16;
    public static int height = 16;
    private int maxWidth = 1024;
    private int maxHeight = 1024;
    //private bool sizeIsChanged = false;
    public GameObject SpawnTile(float x, float y)
    {
        return Instantiate(tile, new Vector3(x, y, 1), Quaternion.identity, tileSpace);
    }
    public void SpawnGrid(int width, int height)
    {
        GameObject newTile = SpawnTile(-width - 0.5f, -height - 0.5f);
        newTile.transform.localScale = new Vector3(width/16f, height/16f, 1);
    }
    public void ClearGrid()
    {
        foreach (Transform child in tileSpace)
        {
            Destroy(child.gameObject);
        }
    }
    void Awake()
    {
        // Load width and height from save file if it exists
        string path = SaveManager.MetaPath;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var meta = JsonUtility.FromJson<GridMeta>(json);
            if (meta != null)
            {   
                width = meta.width;
                height = meta.height;
                tileSize = meta.tileSize;
            }
        }
        else
        {
            // Save default grid size for new slot
            var meta = new GridMeta { width = width, height = height, tileSize = tileSize };
            string json = JsonUtility.ToJson(meta);
            File.WriteAllText(path, json);
        }
    }
    void Start()
    {
        GameManager.DestroyOutOfBoundsComponents();
        SpawnGrid(width, height);
        CenterCameraOnGrid();
    }

    void CenterCameraOnGrid()
    {
        Camera cam = Camera.main;
        cam.transform.position = new Vector3(0, 0, cam.transform.position.z);
        cam.orthographicSize = Mathf.Max(TileSpawner.width, TileSpawner.height) + 1; // or whatever fits your grid
    }

    /*void Update()
    {
        int oldHeight = height;
        int oldWidth = width;
        if (!GameManager.IsEditMode()) return;
        if (Input.GetKeyDown(KeyCode.UpArrow) && height < maxHeight)
        {
            height *= 2;
            sizeIsChanged = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && height > tileSize)
        {
            height /= 2;
            sizeIsChanged = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && width < maxWidth)
        {
            width *= 2;
            sizeIsChanged = true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && width > tileSize)
        {
            width /= 2;
            sizeIsChanged = true;
        }
        if (sizeIsChanged)
        {
            sizeIsChanged = false;
            if (GameManager.HasOutOfBoundsComponents())
            {
                height = oldHeight;
                width = oldWidth;
                return;
            }
            ClearGrid();
            GameManager.DestroyOutOfBoundsComponents();
            SpawnGrid(width, height);
            PlayerPrefs.SetInt("TileSpawnerWidth", width);
            PlayerPrefs.SetInt("TileSpawnerHeight", height);
            PlayerPrefs.Save();
        }
    }*/
    public void IncreaseGridSize()
    {
        if (height < maxHeight) height *= 2;
        if (width < maxWidth) width *= 2;
        if (GameManager.HasOutOfBoundsComponents())
        {
            height /= 2;
            width /= 2;
            return;
        }
        SetGridSizeChange();
        CenterCameraOnGrid();
    }
    public void DecreaseGridSize()
    {
        if (height >= tileSize) height /= 2;
        if (width >= tileSize) width /= 2;
        if (GameManager.HasOutOfBoundsComponents())
        {
            height *= 2;
            width *= 2;
            return;
        }
        SetGridSizeChange();
        CenterCameraOnGrid();
    }
    private void SetGridSizeChange()
    {
        ClearGrid();
        GameManager.DestroyOutOfBoundsComponents();
        SpawnGrid(width, height);
        // Save width, height, and tileSize to file for current slot
        var meta = new GridMeta { width = width, height = height, tileSize = tileSize };
        string json = JsonUtility.ToJson(meta);
        File.WriteAllText(SaveManager.MetaPath, json);
    }
    [System.Serializable]
    private class GridMeta
    {
        public int width;
        public int height;
        public int tileSize;
    }
}
