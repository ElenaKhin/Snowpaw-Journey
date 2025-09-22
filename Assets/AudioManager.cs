using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;   // looped BGM
    [SerializeField] private AudioSource sfxSource;     // one-shots (UI clicks, etc.)

    [Header("Clips")]
    [SerializeField] private AudioClip background;
    [SerializeField] private AudioClip death;
    [SerializeField] private AudioClip click;
    [SerializeField] private AudioClip hurt;
    [SerializeField] private AudioClip attack;
    [SerializeField] private AudioClip enemyHurt;
    [SerializeField] private AudioClip gameOver;



    private void Start()
    {
        if (musicSource && background)
        {
            musicSource.clip = background;
            musicSource.loop = true;

            // Reduce volume here
            musicSource.volume = 0.1f; // 30% of max volume
            musicSource.Play();
        }
    }

    // ---- Music controls ----
    public void PauseMusic()
    {
        if (musicSource && musicSource.isPlaying)
            musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource && musicSource.clip)
            musicSource.UnPause();
    }

    public void StopMusic()
    {
        if (musicSource) musicSource.Stop();
    }

    // ---- SFX helpers (optional) ----
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource && clip) sfxSource.PlayOneShot(clip);
    }

    public void PlayClick() => PlaySFX(click);
    public void PlayDeath() => PlaySFX(death);
    public void PlayHurt()  => PlaySFX(hurt);
    public void PlayAttack()  => PlaySFX(attack);
    public void PlayEnemyHurt() => PlaySFX(enemyHurt);
    public void PlayGameOver() => PlaySFX(gameOver);

}
