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

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;

        sfxPlayer.PlayOneShot(clip);
    }
    public void PlaySfx(AudioID audioID)
    {
        if (!sfxLookup.ContainsKey(audioID)) return;

        var audioData = sfxLookup[audioID];

        sfxPlayer.PlayOneShot(audioData.clip);
    }

    public void PlayMusic(AudioClip clip, bool loop = true,bool fade = false)
    {
        if (clip == null) return;

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
}

public enum AudioID
{
    UIselect,Hit,Fainted,ExpGain
}

[System.Serializable]
public class AudioData
{
    public AudioID id;
    public AudioClip clip;
}