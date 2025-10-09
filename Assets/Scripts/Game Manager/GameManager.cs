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
    public TextMeshProUGUI simText;
    public bool isCompiled = true;
    public static bool hasCircularDependency = false;
    public static float simTime = 0.25f;
    public TMP_InputField refreshRateInput;
    public Button pauseButton;
    public Sprite pauseSprite;
    public Sprite playSprite;
    private static bool isPaused = true;

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
        StartCoroutine(Compile());
        //StartCoroutine(RunSimulation());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (Input.GetKeyDown(KeyCode.Space)) TogglePause();
        if (!isPaused)
        {
            ClockComponent.GlobalClockUpdate(simTime);
            simText.text = "Simulation Mode";
        }
        else
        {
            simText.text = "Edit Mode";
        }
        if (Input.GetKeyDown(KeyCode.Return) && !isCompiled) StartCoroutine(Compile());
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S) && isCompiled)
        {
            SaveManager.SaveLookUp();
            StartCoroutine(DisplaySaveText(1.0f));
        }
    }

    public IEnumerator DisplaySaveText(float delay)
    {
        compileText.text = "Saving...";
        compileText.enabled = true;
        yield return new WaitForSeconds(delay);
        compileText.enabled = false;
        compileText.text = "Press Enter to Compile.";
    }
    public IEnumerator Compile()
    {
        isCompiled = true;
        compileText.text = "Compiling...";
        compileText.color = Color.white;
        //Debug.Log("Compilation initiated.");
        yield return null;
        ResetComponents();
        //Debug.Log("Components reset.");
        yield return null;
        DestroyWireClusters();
        //Debug.Log("Old wire clusters destroyed.");
        yield return null;
        CreateWireClusters(); // <-- ERROR
        //Debug.Log("New wire clusters created and connections established.");
        yield return null;
        ConnectSourceToSource();
        //Debug.Log("Sources connected.");
        yield return null;
        UpdateStates();
        //Debug.Log("States updated.");
        yield return null;
        if (!isCompiled) yield break;
        compileText.text = "Press Enter to Compile.";
        compileText.enabled = false;
        compileText.color = Color.white;
        SaveManager.SaveLookUp();
        yield break;
    }

    private void ResetComponents()
    {
        foreach (var source in SourceComponent.allSources)
        {
            if (source == null) continue;
            //source.isInitialized = false;
            source.connectedClusters.Clear();
            source.inputCluster = null;
            if (!(source is NotComponent)) continue;
            source.errorHighlight.SetActive(false);
        }
        WireComponent.unclusturedWires = new List<WireComponent>(WireComponent.allWires);
        return;
    }

    private void DestroyWireClusters()
    {
        foreach (var cluster in WireCluster.allClusters)
        {
            if (cluster == null) continue;
            //cluster.connectedNots.Clear();
            Destroy(cluster.gameObject);
        }
    }
    private void CreateWireClusters()
    {
        while (WireComponent.unclusturedWires.Count > 0)
        {
            var wire = WireComponent.unclusturedWires[0];
            WireComponent.unclusturedWires.RemoveAt(0);
            if (wire != null)
            {
                wire.DFSConnect();
            }
        }
    }
    private void ConnectSourceToSource()
    {
        foreach (var source in SourceComponent.allSources)
        {
            if (source == null) continue;
            source.connectedNot = null;
            source.inputSource = null;
            Vector2Int pos = new Vector2Int(
                Mathf.RoundToInt(source.transform.position.x),
                Mathf.RoundToInt(source.transform.position.y)
            );
            Vector2Int originalPos = new Vector2Int(pos.x, pos.y);
            if (source is NotComponent)
            {
                while (ComponentScript.GetLookUp(pos.x - 1, pos.y)?.GetComponent<CrossComponent>() != null)
                {
                    pos.x -= 1;
                }
                if (ComponentScript.GetLookUp(pos.x - 1, pos.y)?.GetComponent<SourceComponent>() != null)
                {
                    source.inputSource = ComponentScript.GetLookUp(pos.x - 1, pos.y).GetComponent<SourceComponent>();
                }
            }
            pos = new Vector2Int(originalPos.x, originalPos.y);
            while (ComponentScript.GetLookUp(pos.x + 1, pos.y)?.GetComponent<CrossComponent>() != null)
            {
                pos.x += 1;
            }
            if (ComponentScript.GetLookUp(pos.x + 1, pos.y)?.GetComponent<NotComponent>() != null)
            {
                source.connectedNot = ComponentScript.GetLookUp(pos.x + 1, pos.y).GetComponent<NotComponent>();
            }
        }
    }
    private void UpdateStates()
    {
        foreach (var cluster in WireCluster.allClusters)
        {
            if (cluster == null) continue;
            var visited = new Dictionary<(int, int), int>();
            SimulationDriver.Instance.EnqueueRoutine(() => cluster.UpdateNext(visited));
            SimulationDriver.Instance.RunAll();
        }
        foreach (var source in SourceComponent.allSources)
        {
            if (source == null) continue;
            if (source.IsInitialized()) continue;
            var visited = new Dictionary<(int, int), int>();
            SimulationDriver.Instance.EnqueueRoutine(() => source.UpdateNext(visited));
            SimulationDriver.Instance.RunAll();
        }
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
    public static bool IsPaused()
    {
        return isPaused;
    }
    public static bool HasOutOfBoundsComponents()
    {
        foreach (var kvp in ComponentScript.GetAllLookUp())
        {
            var obj = kvp.Value;
            if (obj == null) continue;
            if (!obj) continue;
            int x = Mathf.RoundToInt(obj.transform.position.x);
            int y = Mathf.RoundToInt(obj.transform.position.y);
            if (x < -TileSpawner.width || x >= TileSpawner.width || y < -TileSpawner.height || y >= TileSpawner.height)
            {
                return true;
            }
        }
        return false;
    }
    public static void DestroyOutOfBoundsComponents()
    {
        foreach (var kvp in ComponentScript.GetAllLookUp())
        {
            var obj = kvp.Value;
            if (obj == null) continue;
            if (!obj) continue;
            int x = Mathf.RoundToInt(obj.transform.position.x);
            int y = Mathf.RoundToInt(obj.transform.position.y);
            if (x < -TileSpawner.width || x >= TileSpawner.width || y < -TileSpawner.height || y >= TileSpawner.height)
            {
                Object.Destroy(obj);
            }
        }
    }

}