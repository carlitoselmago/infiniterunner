/*using System.Collections;
using UnityEngine;

public class HurtMask : MonoBehaviour
{
    public GameObject hurtMask;

    public IEnumerator Mask()
    {
        hurtMask.SetActive(true);
        yield return new WaitForSeconds(0.55f);
        hurtMask.SetActive(false);
    }
}*/

using System.Collections;
using UnityEngine;

public class HurtMask : MonoBehaviour
{
    public GameObject hurtMask;

    private Coroutine maskRoutine;
    private float maskTimeout = 0.55f;
    private float lastCallTime;

    public void Mask()
    {
        lastCallTime = Time.time;

        // Start the coroutine only once
        if (maskRoutine == null)
        {
            maskRoutine = StartCoroutine(MaskRoutine());
        }
    }

    private IEnumerator MaskRoutine()
    {
        hurtMask.SetActive(true);

        while (true)
        {
            // Wait until no new Mask() calls have occurred for 0.55s
            if (Time.time - lastCallTime >= maskTimeout)
                break;

            yield return null;
        }

        hurtMask.SetActive(false);
        maskRoutine = null;
    }
}
