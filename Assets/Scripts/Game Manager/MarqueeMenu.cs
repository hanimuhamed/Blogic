using UnityEngine;

public class MarqueeMenu : MonoBehaviour
{
    public GameObject selectionMenu;
    public GameObject pasteMenu;
    public MarqueeSelector marqueeSelector;

    void Awake()
    {
        pasteMenu.SetActive(false);
        selectionMenu.SetActive(false);
    }
    void Update()
    {
        if (ComponentScript.componentIndex != 6) return;
        if (!GameManager.IsEditMode()) return;
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(2))
        {
            pasteMenu.SetActive(false);
            selectionMenu.SetActive(false);
            return;
        }
        if (!Input.GetMouseButtonUp(1)) return;
        if (marqueeSelector.selectedObjects.Count > 0)
        {
            pasteMenu.SetActive(false);
            selectionMenu.SetActive(true);
            selectionMenu.transform.position = Input.mousePosition;
        }
        else
        {
            if (MarqueeSelector.pasteMode) return;
            if (ClipboardHolder.clipboard.Count == 0) return;
            selectionMenu.SetActive(false);
            pasteMenu.SetActive(true);
            pasteMenu.transform.position = Input.mousePosition;
        }
    }
}