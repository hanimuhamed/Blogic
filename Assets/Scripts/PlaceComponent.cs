using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlaceComponent : MonoBehaviour
{
    public Components components; // Assign in Inspector or find at runtime
    public Transform workspace; // Assign in Inspector or find at runtime
    public Sprite eraserSprite; // Assign in Inspector

    private GameObject ghostObj;
    private GameObject pooledGhostObj;
    private Sprite[] prefabSprites;
    private Camera mainCam;
    private Vector3 lastGhostPos;
    private int lastGhostIndex = -1;
    private bool lastGhostErasing = false;
    private int width;
    private int height;
    private int x;
    private int y;
    private Vector3 mouseWorldPos;
    private Vector3 lastMousePos = Vector3.positiveInfinity;
    private int index;
    private int lastIndex = -1;
    public Texture2D cursorTexture; // Assign in Inspector
    private GameManager gameManager;
    //private List<GameObject>[] pools;
    private int ghostUpdateFrame = 0;
    private Vector3 ghostPos;
    private bool isErasing;
    public MarqueeSelector marqueeSelector;

    void Awake()
    {
        //Application.targetFrameRate = 60;
        //Cursor.SetCursor(cursorTexture, new Vector2(1f, 1f), CursorMode.Auto);
    }
    void Start()
    {
        // Cache all prefab sprites at start
        prefabSprites = new Sprite[components.prefabs.Count];
        //pools = new List<GameObject>[components.prefabs.Count];
        for (int i = 0; i < components.prefabs.Count; i++)
        {
            var sr = components.prefabs[i].GetComponent<SpriteRenderer>();
            prefabSprites[i] = sr != null ? sr.sprite : null;
            //pools[i] = new List<GameObject>();
        }
        mainCam = Camera.main;
        lastGhostPos = Vector3.positiveInfinity;
        width = TileSpawner.width;
        height = TileSpawner.height;
        gameManager = gameObject.GetComponent<GameManager>();
    }

    void Update()
    {
        mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        lastMousePos = Input.mousePosition;
        x = Mathf.RoundToInt(mouseWorldPos.x);
        y = Mathf.RoundToInt(mouseWorldPos.y);
        

        
        if (ComponentScript.componentIndex != lastIndex)
        {
            index = ComponentScript.componentIndex;
            lastIndex = index;
        }
        if (Input.GetMouseButtonDown(1))
        {
            InputComponent input = ComponentScript.GetLookUp(x, y)?.GetComponent<InputComponent>();
            if (input != null)
            {
                input.GetComponent<PopIn>().AnimatePop();
                input.Toggle();   
            }
        }
        if (index == 6) // Marquee mode
        {
            if (pooledGhostObj != null)
            {
                pooledGhostObj.SetActive(false);
            }
            marqueeSelector.HandleMarquee();
            return;
        }
        else
        {
            marqueeSelector.HideMarquee();
            marqueeSelector.ClearSelection();
        }
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() ||
        x < -width || x > width || y < -height || y > height)
        {
            if (pooledGhostObj != null)
            {
                pooledGhostObj.SetActive(false);
            }
            return;
        }
        ghostPos = new Vector3(x, y, 0);
        isErasing = (ComponentScript.componentIndex == 5) || Input.GetMouseButton(1);
        // Return if pointer is over UI



        // If index is 5, use eraser mode (left mouse = erase)
        if (index == 5)
        {
            if (Input.GetMouseButton(0))
            {
                DestroyAtMouse(x, y);
            }
            
        }
        else
        {
            // Paint brush: hold left mouse button to place (replace existing)
            if (Input.GetMouseButton(0))
            {
                PlaceAtMouse(x, y, true); // allow replace
            }
        }
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            DestroyAtMouse(x, y);
        }

        

        ghostUpdateFrame++;
        if (ghostUpdateFrame % 2 == 0)
        {
            
            if (pooledGhostObj == null || lastGhostPos != ghostPos || lastGhostIndex != index || lastGhostErasing != isErasing)
            {
                // Only then update ghost sprite
                UpdateGhostSprite(x, y, index);
                lastGhostPos = ghostPos;
                lastGhostIndex = index;
                lastGhostErasing = isErasing;
            }
        }
    }

    void UpdateGhostSprite(int x, int y, int index)
    {
        Vector3 ghostPos = new Vector3(x, y, 0);
        bool isEraser = index == 5;
        bool isErasing = Input.GetMouseButton(0) && isEraser || Input.GetMouseButton(1) && Input.GetMouseButton(0);
        Sprite spriteToShow = null;
        float alpha = 0.25f;

        if (isEraser || (Input.GetMouseButton(1) && Input.GetMouseButton(0)))
        {
            spriteToShow = eraserSprite;
            if (isErasing) alpha = 0.5f;
        }
        else if (index >= 0 && index < prefabSprites.Length)
        {
            spriteToShow = prefabSprites[index];
        }

        bool needsUpdate = (pooledGhostObj == null) || (pooledGhostObj.transform.position != ghostPos) || (lastGhostIndex != index) || (lastGhostErasing != isErasing);
        if (!needsUpdate) return;

        lastGhostPos = ghostPos;
        lastGhostIndex = index;
        lastGhostErasing = isErasing;

        if (spriteToShow == null)
        {
            if (pooledGhostObj != null)
            {
                pooledGhostObj.SetActive(false);
            }
            return;
        }

        if (pooledGhostObj == null)
        {
            pooledGhostObj = new GameObject("GhostSprite");
            var sr = pooledGhostObj.AddComponent<SpriteRenderer>();
            sr.sprite = spriteToShow;
            sr.color = new Color(1f, 1f, 1f, alpha);
            pooledGhostObj.transform.SetParent(workspace, true);
        }
        else
        {
            pooledGhostObj.SetActive(true);
            var sr = pooledGhostObj.GetComponent<SpriteRenderer>();
            if (sr.sprite != spriteToShow) sr.sprite = spriteToShow;
            if (sr.color.a != alpha) sr.color = new Color(1f, 1f, 1f, alpha);
        }
        pooledGhostObj.transform.position = ghostPos;
    }

    void PlaceAtMouse(int x, int y, bool replace = false)
    {
        // Use cached camera

        // Clamp brush area
        //if (x < -width || x > width || y < -height || y > height) return;

        int index = ComponentScript.componentIndex;
        if (index < 0 || index >= components.prefabs.Count) return;

        GameObject prefab = components.prefabs[index];
        GameObject existing = ComponentScript.GetLookUp(x, y);
        // Only replace if the prefab type is different
        if (existing != null)
        {
            if (replace && !existing.name.StartsWith(prefab.name))
            {
                Destroy(existing);
                InstantiateAndSetLookUp(prefab, x, y);
            }
        }
        else
        {
            InstantiateAndSetLookUp(prefab, x, y);
        }
    }

    void DestroyAtMouse(int x, int y)
    {
        // Use cached camera

        // Clamp brush area
        //if (x < -width || x > width || y < -height || y > height) return;

        GameObject existing = ComponentScript.GetLookUp(x, y);
        if (existing != null)
        {
            Destroy(existing);
            ComponentScript.GetAllLookUp().Remove((x, y));
            SaveManager.SaveLookUp();
            gameManager.compileText.enabled = true;
            gameManager.isCompiled = false;
        }
    }
    void InstantiateAndSetLookUp(GameObject prefab, int x, int y)
    {
        GameObject obj = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity, workspace);
        ComponentScript.SetLookUp(obj);
        SaveManager.SaveLookUp();
        gameManager.compileText.enabled = true;
        gameManager.isCompiled = false;
    }
    /*
    void PoolObject(GameObject obj)
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

    

    int GetPrefabIndex(GameObject obj)
    {
        for (int i = 0; i < ComponentScript.PrefabNames.Length; i++)
        {
            if (obj.name.StartsWith(ComponentScript.PrefabNames[i]))
                return i;
        }
        return -1;
    }
    */
}