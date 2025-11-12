using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Source per la musica")]
    [SerializeField] private AudioSource musicSource;

    [Header("Impostazioni")]
    [SerializeField] private bool loopMusic = true;
    [SerializeField] private bool playOnStart = true;

    private static MusicManager instance;

    private void Awake()
    {
        // Singleton: evita duplicati se cambi scena
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        if (musicSource != null)
        {
            musicSource.loop = loopMusic;
            if (playOnStart && !musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
        else
        {
            Debug.LogWarning("[MusicManager] Nessun AudioSource assegnato!");
        }
    }

    /// <summary>
    /// Cambia traccia musicale.
    /// </summary>
    public void ChangeMusic(AudioClip newClip)
    {
        if (musicSource != null && newClip != null)
        {
            musicSource.clip = newClip;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Ferma la musica.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Riprende la musica.
    /// </summary>
    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
}

