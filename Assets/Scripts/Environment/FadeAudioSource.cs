using System.Collections;
using UnityEngine.Audio;
using UnityEngine;

public static class FadeMixerGroup
{
    public static IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume)
    {
        if (PlayerMove.isOnModernTimes && !PlayerMove.isInTheSandstorm)
            yield break;

        float currentTime = 0f;
        float currentVol;
        audioMixer.GetFloat(exposedParam, out currentVol);

        currentVol = Mathf.Pow(10, currentVol / 20f);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1f);

        while (currentTime < duration)
        {
            if (PlayerMove.isOnModernTimes && !PlayerMove.isInTheSandstorm)
                yield break;

            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20f);
            yield return null;
        }

        audioMixer.SetFloat(exposedParam, Mathf.Log10(targetValue) * 20f);
    }
}
