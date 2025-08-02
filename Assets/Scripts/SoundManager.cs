using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("SFX")]
    [SerializeField] private AudioClip[] sfx;
    [SerializeField] private AudioSource[] sfxSources;
    private int currentSource = 0;

    [Header("Music")]
    [SerializeField] private AudioClip[] music;
    [SerializeField] private AudioSource musicSource;
    private int currentMusic = -1;
    private float timer = 0f;

    public void Play(int index, float volume, bool randPan = false)
    {
        sfxSources[currentSource].clip = sfx[index];
        sfxSources[currentSource].volume = volume;
        sfxSources[currentSource].Play();
        if (randPan)
        {
            sfxSources[currentSource].panStereo = Random.Range(-0.3f, 0.3f);
        }
        currentSource = (currentSource + 1) % (sfxSources.Length);
    }

    public void SetParameter(string parameterName, float value)
    {
        mixer.SetFloat(parameterName, Mathf.Log10(value) * 20);
    }

    private void Update()
    {
        if (music.Length <= 0)
        {
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            int randMusic = Random.Range(0, music.Length);
            if (randMusic == currentMusic)
            {
                randMusic = (randMusic + 1) % music.Length;
            }
            currentMusic = randMusic;
            musicSource.clip = music[currentMusic];
            timer = music[currentMusic].length + 10f;
        }
    }
}
