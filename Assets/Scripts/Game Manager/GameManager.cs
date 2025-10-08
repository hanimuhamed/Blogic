using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    //public static bool isSimulationRunning = false;
    public float clockTime = 0.25f;
    public TextMeshProUGUI compileText;
    public bool isCompiled = true;
    public static bool hasCircularDependency = false;
    public static float simTime = 0.25f;
    public TMP_InputField refreshRateInput;
    public Button pauseButton;
    public Sprite pauseSprite;
    public Sprite playSprite;
    private bool isPaused = true;

    void Awake()
    {
        //QualitySettings.vSyncCount = 0; 
        //Application.targetFrameRate = 60;
    }
    void Start()
    {
        compileText.color = Color.white;
        refreshRateInput.onEndEdit.AddListener(SetRefreshRate);
        compileText.text = "Press Enter to Compile.";
        //StartCoroutine(RunSimulation());
        //StartCoroutine(RunSimulation());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (Input.GetKeyDown(KeyCode.Space)) TogglePause();
        if (!isPaused) ClockComponent.GlobalClockUpdate(simTime);
        if (Input.GetKeyDown(KeyCode.Return) && !isCompiled) StartCoroutine(RunSimulation());
    }

    public IEnumerator RunSimulation()
    {
        isCompiled = true;
        compileText.text = "Compiling";
        compileText.color = Color.white;
        yield return null; // Wait a frame to show the compiling text
        SourceComponent.Refresh();
        WireComponent.unclusturedWires.Clear();
        int destroyCount = 0;
        foreach (var cluster in WireCluster.allClusters)
        {
            if (cluster == null) continue;
            cluster.connectedNots.Clear();
            Destroy(cluster.gameObject);
            destroyCount++;
            if (destroyCount % 20 == 0) yield return null; // Yield every 20 destroys           
        }
        SourceComponent.allSources.Clear();
        for (int i = -TileSpawner.width; i < TileSpawner.width; i++)
        {
            for (int j = -TileSpawner.height; j < TileSpawner.height; j++)
            {
                GameObject obj = ComponentScript.GetLookUp(i, j);
                if (obj == null) continue;
                else if (obj.GetComponent<WireComponent>() != null)
                {
                    WireComponent.unclusturedWires.Add(obj.GetComponent<WireComponent>());
                }
            }
        }
        while (WireComponent.unclusturedWires.Count > 0)
        {
            var wire = WireComponent.unclusturedWires[0];
            if (wire != null)
            {
                wire.DFSConnect();
            }
        }
        for (int i = -TileSpawner.width; i < TileSpawner.width; i++)
        {
            for (int j = -TileSpawner.height; j < TileSpawner.height; j++)
            {
                GameObject obj = ComponentScript.GetLookUp(i, j);
                if (obj == null) continue;
                else if (obj.GetComponent<InputComponent>() != null ||
                        obj.GetComponent<ClockComponent>() != null)
                {
                    SourceComponent.allSources.Add(obj.GetComponent<InputComponent>());
                    SourceComponent.allSources.Add(obj.GetComponent<ClockComponent>());
                }
                else if (obj.GetComponent<NotComponent>() != null)
                {
                    SourceComponent.allSources.Add(obj.GetComponent<NotComponent>());
                }
            }
        }
        int srcCount = 0;
        foreach (var source in SourceComponent.allSources)
        {
            if (source == null) continue;
            Vector2Int pos = new Vector2Int(
                Mathf.RoundToInt(source.transform.position.x),
                Mathf.RoundToInt(source.transform.position.y)
            );
            if (source is NotComponent)
            {
                if (ComponentScript.GetLookUp(pos.x - 1, pos.y)?.GetComponent<SourceComponent>() != null)
                {
                    source.inputSource = ComponentScript.GetLookUp(pos.x - 1, pos.y).GetComponent<SourceComponent>();
                }     
            }
            while (ComponentScript.GetLookUp(pos.x + 1, pos.y)?.GetComponent<CrossComponent>() != null)
            {
                pos.x += 1;
            }
            if (ComponentScript.GetLookUp(pos.x + 1, pos.y)?.GetComponent<NotComponent>() != null)
            {
                source.connectedNot = ComponentScript.GetLookUp(pos.x + 1, pos.y).GetComponent<NotComponent>();
            }
            srcCount++;
            if (srcCount % 20 == 0) yield return null; 
        }
        foreach (var cluster in WireCluster.allClusters)
        {
            if (cluster == null) continue;
            if (cluster.isInitialized) continue;
            var visited = new Dictionary<(int, int), int>();
            SimulationDriver.Instance.EnqueueRoutine(() => cluster.UpdateNext(visited));
            SimulationDriver.Instance.RunAll();
            srcCount++;
            if (srcCount % 20 == 0) yield return null; // Yield every 20 clusters
        }
        foreach (var source in SourceComponent.allSources)
        {
            if (source == null) continue;
            if (source.IsInitialized()) continue;
            var visited = new Dictionary<(int, int), int>();
            SimulationDriver.Instance.EnqueueRoutine(() => source.UpdateNext(visited));
            SimulationDriver.Instance.RunAll();
            srcCount++;
            if (srcCount % 20 == 0) yield return null; // Yield every 20 sources
        }
        if (!isCompiled) yield break;
        /*if (GameManager.hasCircularDependency) {
            isCompiled = false;
            compileText.text = "Circular dependency detected! Fix the circuit and recompile.";
            hasCircularDependency = false;
            yield break;
        }*/
        compileText.text = "Press Enter to Compile.";
        compileText.enabled = false;
        compileText.color = Color.white;
        SaveManager.SaveLookUp();
        yield break;
    }
    public void SetRefreshRate(string rate)
    {
        if (string.IsNullOrEmpty(rate)) return;

        //Debug.Log("Trying to set simulation refresh rate to " + rate + " Hz.");

        if (float.TryParse(rate, out float result))
        {
            float clamped = Mathf.Clamp(result, 1, 240);
            simTime = 1 / clamped;

            // Update the InputField text in case the user typed a value out of bounds
            refreshRateInput.text = clamped.ToString();

            //Debug.Log("Simulation refresh rate set to " + clamped + " Hz (" + simTime + " seconds per update).");
        }
        else
        {
            //Debug.LogWarning("Invalid refresh rate: " + rate);
            // Optionally reset to the previous valid value
            refreshRateInput.text = (1 / simTime).ToString();
        }
    }
    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            pauseButton.image.sprite = playSprite;
        }
        else
        {
            pauseButton.image.sprite = pauseSprite;
        }
    }
    public bool IsPaused()
    {
        return isPaused;
    }

}