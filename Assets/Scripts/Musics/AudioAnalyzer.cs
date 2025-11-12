using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioAnalyzer : MonoBehaviour
{
    private AudioSource audioSource;
    private float[] spectrumData;

    [Header("Analysis Settings")]
    public int spectrumDataSize = 128;
    public static float currentBassEnergy;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        spectrumData = new float[spectrumDataSize];
    }

    private void Update()
    {
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);
        float anlikEnerji = 0f;
        int bassFrequencyRange = Mathf.Min(10, spectrumDataSize);
        for (int i = 0; i < bassFrequencyRange && i < spectrumData.Length; i++)
        {
            anlikEnerji += spectrumData[i];
        }
        currentBassEnergy = anlikEnerji * 1000f;
    }
    public void StartMusic(AudioClip clipToPlay)
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (clipToPlay == null)
        {
            return;
        }

        audioSource.clip = clipToPlay;
        audioSource.Play();
    }
}
