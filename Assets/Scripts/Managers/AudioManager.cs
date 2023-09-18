using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public void Pause()
    {
        audioSource.Pause();
    }

    public void Continue()
    {
        audioSource.Play();
    }

    public void PlayAudio(AudioClip clip, bool shouldLoop)
    {
        audioSource.clip = clip;
        audioSource.loop = shouldLoop;
        audioSource.Play();
    }
}
