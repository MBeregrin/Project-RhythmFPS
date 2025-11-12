using UnityEngine;

// Bu script'in çalışması için bir AudioSource bileşeni ZORUNLUDUR.
[RequireComponent(typeof(AudioSource))]
public class AudioAnalyzer : MonoBehaviour
{
    // === Değişkenler ===
    private AudioSource audioSource; // Müziği çalan bileşen 

    // 'float' değil 'float[]' olmalı, çünkü spektrum verileri bir DİZİ olarak döner.
    private float[] spectrumData;

    // Analiz için kaç adet frekans "dilimi" (örnek) alacağımızı belirler.
    [Header("Analiz Ayarları")]
    public int spectrumDataSize = 128;

    // Herkese açık enerji değeri (örneğin beat detection için kullanılabilir)
    public static float currentBassEnergy; // O anki "Bas Vuruşu" enerjisi.

    private void Awake()
    {
        // 1. AudioSource bileşenini alıyoruz
        audioSource = GetComponent<AudioSource>();

        // 2. Dizi boyutunu belirtiyoruz
        spectrumData = new float[spectrumDataSize];
    }

    private void Update()
    {
        // 3. Spectrum verisini alıyoruz
        // GetSpectrumData, float[] tipinde bir dizi bekler.
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);

        // 4. Enerjiyi hesaplıyoruz (örneğin düşük frekanslarda, "bass" tespiti)
        float anlikEnerji = 0f;
        int bassFrequencyRange = Mathf.Min(10, spectrumDataSize);


        for (int i = 0; i < bassFrequencyRange && i < spectrumData.Length; i++)
        {
            anlikEnerji += spectrumData[i];
        }

        // 5. Değeri diğer script'lerin kullanımına açıyoruz
        currentBassEnergy = anlikEnerji * 1000f;

        // Test için:
        // Debug.Log(currentBassEnergy);
    }
    public void StartMusic(AudioClip clipToPlay)
    {
        if (audioSource == null)
        {
            // (Awake'te zaten alınıyor ama bu ekstra bir güvenlik)
            audioSource = GetComponent<AudioSource>();
        }
        
        if (clipToPlay == null)
        {
            Debug.LogError("AudioAnalyzer: Oynatılacak 'AudioClip' (müzik dosyası) bulunamadı!");
            return;
        }

        audioSource.clip = clipToPlay;
        audioSource.Play();
        Debug.LogWarning($"AudioAnalyzer: Müzik Başlatıldı -> {clipToPlay.name}");
    }
}
