using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<AudioData> sfxList;

    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;
    [SerializeField] float fadeDuration;

    AudioClip currentMusic;
    float originalMusicVol;
    public static AudioManager i { get; private set; }

    Dictionary<AudioID, AudioData> sfxLookup;

    private void Start()
    {
        originalMusicVol = musicPlayer.volume;

        sfxLookup = sfxList.ToDictionary(x => x.id);
    }

    private void Awake()
    {
        i = this;
    }

    public void PlaySfx(AudioClip clip, bool pauseMusic = false)
    {
        if (clip == null) return;

        if(pauseMusic)
        {
            musicPlayer.Pause();
            StartCoroutine(UnPauseMusic(clip.length));
        }

        sfxPlayer.PlayOneShot(clip);
    }
    public void PlaySfx(AudioID audioID, bool pauseMusic = false)
    {
        if (!sfxLookup.ContainsKey(audioID)) return;

        var audioData = sfxLookup[audioID];

        PlaySfx(audioData.clip, pauseMusic);
    }

    public void PlayMusic(AudioClip clip, bool loop = true,bool fade = false)
    {
        if (clip == null || currentMusic == clip) return;

        currentMusic = clip;
        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        if (fade)
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion();
        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();

        if (fade)
            yield return musicPlayer.DOFade(originalMusicVol, fadeDuration).WaitForCompletion();
    }

    IEnumerator UnPauseMusic(float delay)
    {
        yield return new WaitForSeconds(delay);

        musicPlayer.volume = 0;

        musicPlayer.UnPause();
        musicPlayer.DOFade(originalMusicVol, fadeDuration);
    }
}

public enum AudioID
{
    UIselect,Hit,Fainted,ExpGain,ItemObtain,PokemonObtain
}

[System.Serializable]
public class AudioData
{
    public AudioID id;
    public AudioClip clip;
}