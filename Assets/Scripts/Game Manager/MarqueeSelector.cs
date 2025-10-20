using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ClipboardEntry
{
    public int prefabIndex;
    public Vector2Int relativePos;
}
public class MarqueeSelector : MonoBehaviour
{
    #region variables
    public Camera mainCam;
    public Transform workspace;
    public Color marqueeColor = new Color(1f, 1f, 0f, 0.25f); // semi-transparent yellow
    private Vector3 marqueeStart;
    private Vector3 marqueeEnd;
    private bool marqueeActive = false;
    private GameObject marqueeFillObj;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    public List<GameObject> selectedObjects = new List<GameObject>();
    private bool isDragging = false;
    private Vector3 dragStartMouseWorld;
    private Dictionary<GameObject, Vector3> dragOffsets = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, (int, int)> originalPositions = new Dictionary<GameObject, (int, int)>();
    public GameManager gameManager;
    private List<ClipboardEntry> clipboard = new List<ClipboardEntry>();
    private Vector2Int clipboardOrigin;
    private bool pasteMode = false;
    private List<GameObject> ghostGroup = new List<GameObject>();
    public Components components;
    private bool isDuplicateMode = false;
    #endregion variables
    void Start()
    {
        marqueeFillObj = new GameObject("MarqueeFill");
        marqueeFillObj.transform.SetParent(workspace, false);
        meshFilter = marqueeFillObj.AddComponent<MeshFilter>();
        meshRenderer = marqueeFillObj.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.material.color = marqueeColor;
        marqueeFillObj.SetActive(false);
    }
    void Update()
    {
        if (!GameManager.IsEditMode()) return;
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.C)) CopySelection();
            if (Input.GetKeyDown(KeyCode.X)) CutSelection();
            if (Input.GetKeyDown(KeyCode.V)) PasteClipboard();
            if (Input.GetKeyDown(KeyCode.D)) DuplicateSelection();
            if (Input.GetKeyDown(KeyCode.A))
            {
                ComponentScript.SetComponentIndex(6); // Set to Marquee
                SelectAll();
            }
        }
        if (Input.GetKeyDown(KeyCode.Delete)) DeleteSelection();
        if (Input.GetKeyDown(KeyCode.H)) HideMarquee();
        if (Input.GetMouseButtonDown(1)) // Right click cancels marquee, paste, drag
        {
            HideMarquee();
            ClearSelection();
            CancelPasteMode();
            isDragging = false;
        }
        if (pasteMode)
            UpdatePasteGhost();
    }
    public void HandleMarquee(Vector3 mouseWorld)
    {
        if (isDuplicateMode) return; // Disable marquee when duplicating
        //mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        // Start drag if mouse is over a selected object and left mouse down
        if (!isDragging && Input.GetMouseButtonDown(0))
        {
            // Calculate selection bounding box
            if (selectedObjects.Count > 0)
            {
                float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
                foreach (var obj in selectedObjects)
                {
                    Vector3 pos = obj.transform.position;
                    if (pos.x < minX) minX = pos.x;
                    if (pos.x > maxX) maxX = pos.x;
                    if (pos.y < minY) minY = pos.y;
                    if (pos.y > maxY) maxY = pos.y;
                }
                Vector3 mouse = mouseWorld;
                // Check if mouse is inside the bounding box
                if (mouse.x >= minX && mouse.x <= maxX && mouse.y >= minY && mouse.y <= maxY)
                {
                    // Begin drag (rest of your drag logic)
                    isDragging = true;
                    dragStartMouseWorld = mouseWorld;
                    dragOffsets.Clear();
                    originalPositions.Clear();
                    foreach (var sel in selectedObjects)
                    {
                        dragOffsets[sel] = sel.transform.position - mouseWorld;
                        int ox = Mathf.RoundToInt(sel.transform.position.x);
                        int oy = Mathf.RoundToInt(sel.transform.position.y);
                        originalPositions[sel] = (ox, oy);
                        // Remove from lookup so we can move freely
                        var key = (ox, oy);
                        if (ComponentScript.GetAllLookUp().ContainsKey(key) && ComponentScript.GetAllLookUp()[key] == sel)
                            ComponentScript.GetAllLookUp().Remove(key);
                    }
                    return;
                }
            }
        }

        // Dragging logic
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mouseDelta = mouseWorld - dragStartMouseWorld;

            // Calculate selection bounds
            int minOffsetX = int.MaxValue, maxOffsetX = int.MinValue;
            int minOffsetY = int.MaxValue, maxOffsetY = int.MinValue;
            foreach (var sel in selectedObjects)
            {
                if (!dragOffsets.ContainsKey(sel)) continue;
                int offsetX = Mathf.RoundToInt(dragOffsets[sel].x);
                int offsetY = Mathf.RoundToInt(dragOffsets[sel].y);
                if (offsetX < minOffsetX) minOffsetX = offsetX;
                if (offsetX > maxOffsetX) maxOffsetX = offsetX;
                if (offsetY < minOffsetY) minOffsetY = offsetY;
                if (offsetY > maxOffsetY) maxOffsetY = offsetY;
            }
            int mouseX = Mathf.RoundToInt(mouseWorld.x);
            int mouseY = Mathf.RoundToInt(mouseWorld.y);

            int clampedX = Mathf.Clamp(mouseX,
                -TileSpawner.width - minOffsetX,
                TileSpawner.width - 1 - maxOffsetX);
            int clampedY = Mathf.Clamp(mouseY,
                -TileSpawner.height - minOffsetY,
                TileSpawner.height - 1 - maxOffsetY);

            Vector3 clampedMouseWorld = new Vector3(clampedX, clampedY, 0);

            foreach (var sel in selectedObjects)
            {
                if (!dragOffsets.ContainsKey(sel)) continue;
                Vector3 newPos = clampedMouseWorld + dragOffsets[sel];
                newPos.x = Mathf.Round(newPos.x);
                newPos.y = Mathf.Round(newPos.y);
                newPos.z = -1;
                sel.transform.position = newPos;
            }
            return;
        }

        // End drag
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            // Place in lookup, replacing any existing objects
            foreach (var sel in selectedObjects)
            {
                int x = Mathf.RoundToInt(sel.transform.position.x);
                int y = Mathf.RoundToInt(sel.transform.position.y);
                var key = (x, y);
                // Replace existing
                if (ComponentScript.GetAllLookUp().ContainsKey(key) && ComponentScript.GetAllLookUp()[key] != sel)
                {
                    var toDestroy = ComponentScript.GetAllLookUp()[key];
                    if (toDestroy != null) Destroy(toDestroy);
                }
                ComponentScript.GetAllLookUp()[key] = sel;
            }
            //gameManager.compileText.enabled = true;
            gameManager.isCompiled = false;
            return;
        }

        // If not dragging, allow marquee as before
        if (!isDragging)
        {
            // ...existing marquee code...
            if (Input.GetMouseButtonDown(0))
            {
                marqueeActive = true;
                marqueeStart = mainCam.ScreenToWorldPoint(Input.mousePosition);
                marqueeStart.z = 0;
            }
            if (Input.GetMouseButton(0) && marqueeActive)
            {
                marqueeEnd = mainCam.ScreenToWorldPoint(Input.mousePosition);
                marqueeEnd.z = 0;
                DrawFilledMarquee(marqueeStart, marqueeEnd);
            }
            if (Input.GetMouseButtonUp(0) && marqueeActive)
            {
                marqueeActive = false;
                marqueeFillObj.SetActive(false);
                SelectInRect(marqueeStart, marqueeEnd);
            }
        }
    }
    private void DrawFilledMarquee(Vector3 start, Vector3 end)
    {
        marqueeFillObj.SetActive(true);

        Vector3 min = Vector3.Min(start, end);
        Vector3 max = Vector3.Max(start, end);

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(min.x, min.y, 0),
            new Vector3(max.x, min.y, 0),
            new Vector3(max.x, max.y, 0),
            new Vector3(min.x, max.y, 0)
        };

        int[] tris = new int[6] { 0, 1, 2, 2, 3, 0 };
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(0,1)
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
    public void HideMarquee()
    {
        if (marqueeFillObj != null)
        {
            //Debug.Log("Marquee hidden");
            marqueeFillObj.SetActive(false);
        }
    }
    public void SelectInRect(Vector3 start, Vector3 end)
    {
        selectedObjects.Clear();

        Vector3 min = Vector3.Min(start, end);
        Vector3 max = Vector3.Max(start, end);

        foreach (var obj in ComponentScript.GetAllLookUp().Values)
        {
            if (obj == null) continue;
            // Only select SourceComponent or WireComponent
            if (obj.GetComponent<SourceComponent>() == null && obj.GetComponent<WireComponent>() == null && obj.GetComponent<CrossComponent>() == null)
                continue;

            Vector3 pos = obj.transform.position;
            if (pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y)
            {
                selectedObjects.Add(obj);
                // Enable highlight child
                var highlight = obj.transform.Find("Highlight");
                if (highlight != null)
                    highlight.gameObject.SetActive(true);
            }
            else
            {
                // Disable highlight if not selected
                var highlight = obj.transform.Find("Highlight");
                if (highlight != null)
                    highlight.gameObject.SetActive(false);
            }
        }
    }
    public void ClearSelection()
    {
        foreach (var obj in selectedObjects)
        {
            if (obj == null) continue;
            var highlight = obj.transform.Find("Highlight");
            if (highlight != null)
                highlight.gameObject.SetActive(false);
        }
        selectedObjects.Clear();
    }
    public void CopySelection()
    {
        clipboard.Clear();
        if (selectedObjects.Count == 0) return;
        // Use the top-left as the origin
        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var obj in selectedObjects)
        {
            int x = Mathf.RoundToInt(obj.transform.position.x);
            int y = Mathf.RoundToInt(obj.transform.position.y);
            if (x < minX) minX = x;
            if (y < minY) minY = y;
        }
        clipboardOrigin = new Vector2Int(minX, minY);

        foreach (var obj in selectedObjects)
        {
            int x = Mathf.RoundToInt(obj.transform.position.x);
            int y = Mathf.RoundToInt(obj.transform.position.y);
            int prefabIndex = GetPrefabIndex(obj);
            clipboard.Add(new ClipboardEntry
            {
                prefabIndex = prefabIndex,
                relativePos = new Vector2Int(x - minX, y - minY)
            });
        }
    }
    public void CutSelection()
    {
        CopySelection();
        DeleteSelection();
    }
    public void DeleteSelection()
    {
        foreach (var obj in selectedObjects)
        {
            int x = Mathf.RoundToInt(obj.transform.position.x);
            int y = Mathf.RoundToInt(obj.transform.position.y);
            var key = (x, y);
            if (ComponentScript.GetAllLookUp().ContainsKey(key) && ComponentScript.GetAllLookUp()[key] == obj)
                ComponentScript.GetAllLookUp().Remove(key);
            Destroy(obj);
        }
        selectedObjects.Clear();
        //gameManager.compileText.enabled = true;
        gameManager.isCompiled = false;
    }
    public void DuplicateSelection()
    {
        CopySelection();
        isDuplicateMode = true;
        StartPasteMode(useClipboard: true, useSelectionAsGhost: true);
    }
    public void PasteClipboard()
    {
        if (clipboard.Count == 0) return;
        isDuplicateMode = false;
        StartPasteMode(useClipboard: true, useSelectionAsGhost: false);
    }
    private void StartPasteMode(bool useClipboard, bool useSelectionAsGhost)
    {
        pasteMode = true;
        ClearGhostGroup();
        List<ClipboardEntry> source = useClipboard ? clipboard : new List<ClipboardEntry>();
        if (useSelectionAsGhost)
        {
            // Use selected objects as ghost group for duplicate
            int minX = int.MaxValue, minY = int.MaxValue;
            foreach (var obj in selectedObjects)
            {
                int x = Mathf.RoundToInt(obj.transform.position.x);
                int y = Mathf.RoundToInt(obj.transform.position.y);
                if (x < minX) minX = x;
                if (y < minY) minY = y;
            }
            clipboardOrigin = new Vector2Int(minX, minY);
            source.Clear();
            foreach (var obj in selectedObjects)
            {
                int x = Mathf.RoundToInt(obj.transform.position.x);
                int y = Mathf.RoundToInt(obj.transform.position.y);
                int prefabIndex = GetPrefabIndex(obj);
                source.Add(new ClipboardEntry
                {
                    prefabIndex = prefabIndex,
                    relativePos = new Vector2Int(x - minX, y - minY)
                });
            }
        }

        // Instantiate ghost group (semi-transparent)
        foreach (var entry in source)
        {
            var prefab = components.prefabs[entry.prefabIndex];
            var ghost = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            //SetGhostVisual(ghost);
            ghostGroup.Add(ghost);
        }
        ClearSelection();
    }
    public void UpdatePasteGhost()
    {
        if (!pasteMode || clipboard.Count == 0) return;
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        int mx = Mathf.RoundToInt(mouseWorld.x);
        int my = Mathf.RoundToInt(mouseWorld.y);

        // Check bounds
        bool canPaste = true;
        for (int i = 0; i < clipboard.Count; i++)
        {
            int x = mx + clipboard[i].relativePos.x;
            int y = my + clipboard[i].relativePos.y;
            if (x < -TileSpawner.width || x >= TileSpawner.width || y < -TileSpawner.height || y >= TileSpawner.height)
            {
                canPaste = false;
                break;
            }
        }

        // Move ghost group
        for (int i = 0; i < ghostGroup.Count; i++)
        {
            int x = mx + clipboard[i].relativePos.x;
            int y = my + clipboard[i].relativePos.y;
            ghostGroup[i].transform.position = new Vector3(x, y, -1);
            ghostGroup[i].SetActive(canPaste);
        }

        // Place on click
        if (canPaste && Input.GetMouseButtonDown(0))
        {
            PlaceClipboardAt(mx, my);
            if (!isDuplicateMode)
            {
                pasteMode = false;
                ClearGhostGroup();
            }
            //gameManager.compileText.enabled = true;
            gameManager.isCompiled = false;
        }
        // Cancel paste on right click
        if (Input.GetMouseButtonDown(1))
        {
            HideMarquee();
            ClearSelection();
            CancelPasteMode();
        }
    }
    public void CancelPasteMode()
    {
        pasteMode = false;
        isDuplicateMode = false;
        ClearGhostGroup();
    }
    private void PlaceClipboardAt(int mx, int my)
    {
        // Place all objects, replacing existing
        List<GameObject> newSelection = new List<GameObject>();
        for (int i = 0; i < clipboard.Count; i++)
        {
            int x = mx + clipboard[i].relativePos.x;
            int y = my + clipboard[i].relativePos.y;
            var key = (x, y);
            if (ComponentScript.GetAllLookUp().ContainsKey(key))
            {
                var toDestroy = ComponentScript.GetAllLookUp()[key];
                if (toDestroy != null) Destroy(toDestroy);
            }
            var prefab = components.prefabs[clipboard[i].prefabIndex];
            var obj = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
            ComponentScript.SetLookUp(obj);
            newSelection.Add(obj);
        }
        selectedObjects = newSelection;
    }
    private void ClearGhostGroup()
    {
        foreach (var g in ghostGroup)
            Destroy(g);
        ghostGroup.Clear();
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
    private void SelectAll()
    {
        ClearSelection();
        SelectInRect(new Vector3(-TileSpawner.width, -TileSpawner.height, 0), new Vector3(TileSpawner.width - 1, TileSpawner.height - 1, 0));
        //Debug.Log("Selected all: " + selectedObjects.Count + " objects.");
    }
}