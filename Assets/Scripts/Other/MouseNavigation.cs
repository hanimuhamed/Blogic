using UnityEngine;

public class MouseNavigation : MonoBehaviour
{
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    private float maxZoom;

    private float minX;
    private float maxX;
    private float minY;
    private float maxY;

    private Vector3 lastMousePosition;
    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
        mainCam.orthographicSize = Mathf.Max(TileSpawner.width, TileSpawner.height) * 1.1f;
        // Cache boundaries once
    }

    void Update()
    {
        /*if (!Input.GetKey(KeyCode.LeftShift))
        {
            return;
        }*/
        Camera cam = mainCam;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 mouseWorldBefore = cam.ScreenToWorldPoint(Input.mousePosition);
            float dynamicZoomSpeed = cam.orthographic ? zoomSpeed * cam.orthographicSize * 0.1f : zoomSpeed * cam.fieldOfView * 0.1f;
            maxZoom = Mathf.Max(TileSpawner.width, TileSpawner.height) * 1.1f; // Set maxZoom based on workspace size
            if (cam.orthographic)
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * dynamicZoomSpeed, minZoom, maxZoom);
            else
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - scroll * dynamicZoomSpeed, minZoom, maxZoom);
            Vector3 mouseWorldAfter = cam.ScreenToWorldPoint(Input.mousePosition);
            cam.transform.position += mouseWorldBefore - mouseWorldAfter;
            ClampCameraPosition(cam);
        }
        // Pan with middle mouse button
        if (Input.GetMouseButtonDown(2))
            lastMousePosition = Input.mousePosition;
        if (Input.GetMouseButton(2))
        {
            Vector3 mouseScreenDelta = Input.mousePosition - lastMousePosition;
            Vector3 move = cam.ScreenToWorldPoint(lastMousePosition) - cam.ScreenToWorldPoint(Input.mousePosition);
            cam.transform.position += move;
            lastMousePosition = Input.mousePosition;
            ClampCameraPosition(cam);
        }
    }

    private void ClampCameraPosition(Camera cam)
    {
        Vector3 pos = cam.transform.position;
        minX = -TileSpawner.width;
        maxX = TileSpawner.width - 1;
        minY = -TileSpawner.height;
        maxY = TileSpawner.height - 1;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        cam.transform.position = pos;
    }
}