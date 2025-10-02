using UnityEngine;
using System.Collections;

public class PopIn : MonoBehaviour
{
    public float duration = 0.3f;   // how long the pop animation takes
    public Vector3 targetScale = Vector3.one; // final size
    public float overshoot = 1.2f; 
    public bool skipPop = false; 
    void Start()
    {
        if (skipPop)
        {
            transform.localScale = targetScale;
            return;
        }
        transform.localScale = Vector3.zero; // start tiny
        StartCoroutine(AnimatePop());
    }

    public IEnumerator AnimatePop()
    {
        float halfDuration = duration * 0.6f;
        float restDuration = duration - halfDuration;
        transform.position -= Vector3.forward * 0.1f; // slight forward shift to avoid z-fighting
        // Phase 1: scale up past target
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / halfDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale * overshoot, t);
            yield return null;
        }

        // Phase 2: settle back to target
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / restDuration;
            transform.localScale = Vector3.Lerp(targetScale * overshoot, targetScale, t);
            yield return null;
        }
        transform.position += Vector3.forward * 0.1f; // revert forward shift
    }

}