using UnityEngine;
using System.Collections;

public class PopIn : MonoBehaviour
{
    private float duration = 0.1f;
    public Vector3 targetScale = Vector3.one;
    public bool skipPop = false;

    void Start()
    {
        if (skipPop)
        {
            transform.localScale = targetScale;
            return;
        }
        transform.localScale = Vector3.zero;
        StartCoroutine(AnimatePop());
    }

    public IEnumerator AnimatePop()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}