using UnityEngine;

public class InputToggle : MonoBehaviour
{
    private SourceComponent sourceComponent;
    private float simTime = 0.25f;
    void Awake()
    {
        sourceComponent = GetComponent<SourceComponent>();
    }
    void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            //Debug.Log("Initial toggle of input at " + transform.position);
            sourceComponent.Toggle();
        }
    }
    /*void OnEnable()
    {
        for (int i = 0; i < 2; i++)
        {
            Debug.Log("Initial toggle of input at " + transform.position);
            inputComponent.Toggle();
        }
    }*/

    void OnMouseDown()
    {
        if (sourceComponent != null)
        {
            sourceComponent.Toggle();
        }
    }
}
