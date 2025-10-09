using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    public GameObject tile;
    public int tileSize = 8;
    //public GameObject grid;
    public Transform tileSpace;
    public static int width = 64;
    public static int height = 64;
    private int maxWidth = 512;
    private int maxHeight = 512;
    //private bool sizeIsChanged = false;
    public GameObject SpawnTile(int x, int y)
    {
        return Instantiate(tile, new Vector3(x, y, 1), Quaternion.identity, tileSpace);
    }
    public void SpawnGrid(int width, int height)
    {
        for (int x = -width; x < width; x += tileSize)
        {
            for (int y = -height; y < height; y += tileSize)
            {
                GameObject newTile = SpawnTile(x, y);
            }
        }
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
        // Load width and height if they exist
        if (PlayerPrefs.HasKey("TileSpawnerWidth"))
            width = PlayerPrefs.GetInt("TileSpawnerWidth");
        if (PlayerPrefs.HasKey("TileSpawnerHeight"))
            height = PlayerPrefs.GetInt("TileSpawnerHeight");
    }
    void Start()
    {
        GameManager.DestroyOutOfBoundsComponents();
        SpawnGrid(width, height);
    }

    /*void Update()
    {
        int oldHeight = height;
        int oldWidth = width;
        if (!GameManager.IsPaused()) return;
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
    }
    public void DecreaseGridSize()
    {
        if (height > tileSize) height /= 2;
        if (width > tileSize) width /= 2;
        if (GameManager.HasOutOfBoundsComponents())
        {
            height *= 2;
            width *= 2;
            return;
        }
        SetGridSizeChange();
    }
    private void SetGridSizeChange()
    {
        ClearGrid();
        GameManager.DestroyOutOfBoundsComponents();
        SpawnGrid(width, height);
        PlayerPrefs.SetInt("TileSpawnerWidth", width);
        PlayerPrefs.SetInt("TileSpawnerHeight", height);
        PlayerPrefs.Save();
    }
}
