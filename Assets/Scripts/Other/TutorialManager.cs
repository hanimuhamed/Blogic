using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public List<GameObject> tutorialCircuits = new List<GameObject>();
    private int currentCircuitIndex = 0;
    private float simTime = 0.25f;
    void Show(GameObject circuit)
    {
        SpriteRenderer[] renderers = circuit.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in renderers)
        {
            sr.enabled = true;
        }
        BoxCollider2D[] colliders = circuit.GetComponentsInChildren<BoxCollider2D>();
        foreach (BoxCollider2D bc in colliders)
        {
            bc.enabled = true;
        }
    }
    void Hide(GameObject circuit)
    {
        SpriteRenderer[] renderers = circuit.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in renderers)
        {
            sr.enabled = false;
        }
        BoxCollider2D[] colliders = circuit.GetComponentsInChildren<BoxCollider2D>();
        foreach (BoxCollider2D bc in colliders)
        {
            bc.enabled = false;
        }
    }
    void HideAll(List<GameObject> circuits)
    {
        foreach (GameObject circuit in circuits)
        {
            Hide(circuit);
        }
    }

    public void Next()
    {
        
        currentCircuitIndex++;
        if (currentCircuitIndex >= tutorialCircuits.Count)
        {
            SceneManager.LoadScene("MenuScene");
            return;
        }
        Hide(tutorialCircuits[currentCircuitIndex - 1]);
        Show(tutorialCircuits[currentCircuitIndex]);
    }
    public void Back()
    {
        
        currentCircuitIndex--;
        if (currentCircuitIndex < 0)
        {
            SceneManager.LoadScene("MenuScene");
            return;
        }
        Hide(tutorialCircuits[currentCircuitIndex + 1]);
        Show(tutorialCircuits[currentCircuitIndex]);
    }
    void Start()
    {
        Time.timeScale = 1f;
        HideAll(tutorialCircuits);
        if (tutorialCircuits.Count > 0)
        {
            Show(tutorialCircuits[0]);
        }
    }
    void Update()
    {
        ClockComponent.GlobalClockUpdate(simTime);
    }

}
