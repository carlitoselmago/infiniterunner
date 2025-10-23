using UnityEngine;
using System.Collections;

namespace SuperPivot
{
    namespace Samples
    {
        public class RotateAroundPivot : MonoBehaviour
        {
            [Header("Rotation Settings")]
            public float speed = 100f;

            [Header("Light Settings")]
            public Light sphereLight;
            public float normalIntensity = 2f;
            public float brightIntensity = 5f;
            public float fadeDuration = 0.5f;

            void Update()
            {
                // Rotate continuously
                transform.rotation *= Quaternion.Euler(0f, speed * Time.deltaTime, 0f);
            }

            public void BrightUpSphere()
            {
                if (sphereLight == null)
                {
                    Debug.LogWarning("No sphere light assigned to RotateAroundPivot!");
                    return;
                }

                StopAllCoroutines(); // Stop previous fades to prevent overlap
                StartCoroutine(BrightenThenDim());
            }

            private IEnumerator BrightenThenDim()
            {
                // Instantly brighten
                sphereLight.intensity = brightIntensity;

                float elapsed = 0f;

                // Smoothly fade back to normal intensity
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    sphereLight.intensity = Mathf.Lerp(brightIntensity, normalIntensity, elapsed / fadeDuration);
                    yield return null;
                }

                sphereLight.intensity = normalIntensity;
            }
        }
    }
}
