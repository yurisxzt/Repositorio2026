using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AudioManager: singleton that manages a dedicated 2D looping AudioSource (systemSource)
/// and a pool/list of 3D AudioSources (activeSources) for looping 3D sounds.
/// Provides Play/Stop/Pause/Resume and OneShot APIs for both 2D (system) and 3D (active) sounds.
/// </summary>
namespace Script
{
    public class AudioManager : MonoBehaviour
    {
        #region Singleton
        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        #endregion

    [Header("System (2D) Source")]
    [SerializeField] private AudioSource systemSource; // used for 2D looping or one-shots (music, UI)

    [Header("3D Active Sources")]
    [SerializeField] private AudioSource activeSourcePrefab; // prefab for 3D positional sources
    [SerializeField] private List<AudioSource> activeSources = new List<AudioSource>();

    private void Reset()
    {
        // try to setup defaults if missing
        if (systemSource == null)
        {
            var go = new GameObject("Audio_SystemSource");
            go.transform.SetParent(transform, false);
            systemSource = go.AddComponent<AudioSource>();
            systemSource.playOnAwake = false;
            systemSource.spatialBlend = 0f; // 2D
        }

        if (activeSourcePrefab == null)
        {
            var go = new GameObject("Audio_ActiveSource_Prefab");
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f; // 3D
            activeSourcePrefab = src;
            // keep prefab disabled in scene hierarchy
            go.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    #region System (2D) API

    public void PlaySystem(AudioClip clip, bool loop = false, float volume = 1f)
    {
        if (systemSource == null) Reset();
        systemSource.clip = clip;
        systemSource.loop = loop;
        systemSource.volume = volume;
        systemSource.Play();
    }

    public void StopSystem()
    {
        if (systemSource == null) return;
        systemSource.Stop();
    }

    public void PauseSystem()
    {
        if (systemSource == null) return;
        systemSource.Pause();
    }

    public void ResumeSystem()
    {
        if (systemSource == null) return;
        if (systemSource.clip == null) return;
        systemSource.UnPause();
    }

    public void PlaySystemOneShot(AudioClip clip, float volume = 1f)
    {
        if (systemSource == null) Reset();
        systemSource.PlayOneShot(clip, volume);
    }

    #endregion

    #region Active (3D) API

    // Play a looping 3D sound at position; returns the AudioSource used so caller can stop it or modify it
    public AudioSource PlayActive(AudioClip clip, Vector3 position, bool loop = true, float volume = 1f, float minDistance = 1f, float maxDistance = 500f)
    {
        var src = CreateActiveSource(position);
        src.clip = clip;
        src.loop = loop;
        src.volume = volume;
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        src.Play();
        if (!activeSources.Contains(src)) activeSources.Add(src);
        return src;
    }

    public void StopActive(AudioSource source)
    {
        if (source == null) return;
        source.Stop();
        activeSources.Remove(source);
        Destroy(source.gameObject);
    }

    public void PauseActive(AudioSource source)
    {
        source?.Pause();
    }

    public void ResumeActive(AudioSource source)
    {
        if (source == null) return;
        source.UnPause();
    }

    public AudioSource PlayActiveOneShot(AudioClip clip, Vector3 position, float volume = 1f, float minDistance = 1f, float maxDistance = 500f)
    {
        var src = CreateActiveSource(position);
        src.spatialBlend = 1f;
        src.PlayOneShot(clip, volume);
        // destroy after clip length
        Destroy(src.gameObject, clip != null ? clip.length + 0.1f : 5f);
        return src;
    }

    private AudioSource CreateActiveSource(Vector3 position)
    {
        AudioSource src;
        if (activeSourcePrefab != null)
        {
            var go = Instantiate(activeSourcePrefab.gameObject, position, Quaternion.identity, transform);
            go.name = "Audio_ActiveSource";
            go.hideFlags = HideFlags.None;
            src = go.GetComponent<AudioSource>();
            src.playOnAwake = false;
        }
        else
        {
            var go = new GameObject("Audio_ActiveSource");
            go.transform.SetParent(transform, false);
            go.transform.position = position;
            src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f;
        }

        return src;
    }

    #endregion

    #region Utility

    public void StopAllActive()
    {
        for (int i = activeSources.Count - 1; i >= 0; i--)
        {
            var s = activeSources[i];
            if (s != null)
            {
                s.Stop();
                Destroy(s.gameObject);
            }
            activeSources.RemoveAt(i);
        }
    }

    #endregion
    }
}
