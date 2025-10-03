using UnityEngine;

public class ClockComponent : SourceComponent
{
    public float clockTime = 0.25f;
    private static float globalTimer = 0f;
    private static bool globalState = false;
    private bool prevState = false;

    public static void GlobalClockUpdate(float period)
    {
        globalTimer += Time.deltaTime;
        if (globalTimer >= period)
        {
            globalTimer = 0f;
            globalState = !globalState;
        }
    }
    void Update()
    {
        if (prevState != globalState)
        {
            Toggle();
            prevState = globalState;
        }
    }
}