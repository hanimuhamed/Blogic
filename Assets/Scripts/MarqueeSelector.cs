using UnityEngine;

public class MarqueeSelector : MonoBehaviour
{
    public Camera mainCam;
    public Transform workspace;
    public Color marqueeColor = new Color(1f, 1f, 0f, 0.25f); // semi-transparent yellow
    private Vector3 marqueeStart;
    private Vector3 marqueeEnd;
    private bool marqueeActive = false;
    private GameObject marqueeFillObj;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

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

    public void HandleMarquee()
    {
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
            marqueeFillObj.SetActive(false);
    }
}