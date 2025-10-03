using UnityEngine;
using UnityEngine.EventSystems;

public class TileScript : MonoBehaviour
{
    private bool mouseExit = true;
    public static bool componentChanged = false;
    public GameObject[] components = new GameObject[5];
    public SpriteRenderer[] ghostSprites = new SpriteRenderer[6];
    private GameObject component;
    public Transform tilespace;
    private SpriteRenderer ghostSprite;
    void Start()
    {
        tilespace = GameObject.Find("Tilespace").transform;
        ghostSprite = transform.Find("Ghost").GetComponent<SpriteRenderer>();
    }
    void OnMouseOver()
    {
        ghostSprite.enabled = true;
        ghostSprite.sprite = ghostSprites[ComponentScript.componentIndex].sprite;
        ghostSprite.color = new Color(1f, 1f, 1f, 0.20f); // semi-transparent

        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Click blocked by UI");
            return;
        }
        if (mouseExit)
        {
            if (Input.GetMouseButton(0))
            {
                DestroyTiled(0);
                if (ComponentScript.componentIndex == 5) // Eraser
                {
                    ghostSprite.color = new Color(1f, 1f, 1f, 0.5f);
                    return;
                }
                component = components[ComponentScript.componentIndex];
                GameObject newTile = Instantiate(component, transform.position - Vector3.forward, Quaternion.identity, tilespace);
                ComponentScript.SetLookUp(newTile);
                mouseExit = false;
            }
            else if (Input.GetMouseButton(1))
            {
                ghostSprite.sprite = ghostSprites[5].sprite;
                ghostSprite.color = new Color(1f, 1f, 1f, 0.5f);// switch to eraser icon
                DestroyTiled(0);
            }
        }
    }

    void OnMouseExit()
    {
        ghostSprite.enabled = false;
        mouseExit = true;
    }
    private void DestroyTiled(float time)
    {
        GameObject tiled = ComponentScript.GetLookUp(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (tiled != null)
        {
            Destroy(tiled, time);
        }
    }
}
