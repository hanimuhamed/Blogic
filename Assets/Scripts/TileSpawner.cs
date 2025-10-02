using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    public GameObject tile;
    public GameObject grid;
    public Transform tileSpace;
    public static int width = 128;
    public static int height = 128;
    public GameObject SpawnTile(int x, int y)
    {
        return Instantiate(tile, new Vector3(x, y, 1), Quaternion.identity, tileSpace);
    }
    public GameObject SpawnGrid(float x, float y)
    {
        return Instantiate(grid, new Vector3(x, y, 2), Quaternion.identity, tileSpace);
    }
    void Start()
    {
        /*for (int x = -width; x <= width; x++)
        {
            for (int y = -height; y <= height; y++)
            {
                GameObject newTile = SpawnTile(x, y);
            }
        }*/
        SpawnGrid(0f, 0f);
    }
}
