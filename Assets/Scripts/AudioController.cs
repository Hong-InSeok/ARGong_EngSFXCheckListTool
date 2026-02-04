using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
    private AudioSource _audioSource;
    private string currentAudioUrl = string.Empty;

    public void Init()
    {
        _audioSource = this.GetComponent<AudioSource>();
    }

    public void PlayAudioFromUrl(string audioUrl)
    {
        if (string.IsNullOrEmpty(audioUrl))
        {
            Debug.LogWarning("Audio URL is empty!");
            return;
        }

        currentAudioUrl = audioUrl;
        StartCoroutine(LoadAndPlayAudio(audioUrl));
    }

    private IEnumerator LoadAndPlayAudio(string url)
    {
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                
                if (clip != null && _audioSource != null)
                {
                    _audioSource.clip = clip;
                    _audioSource.Play();
                    Debug.Log($"Playing audio from: {url}");
                }
            }
            else
            {
                Debug.LogError($"Failed to load audio from {url}: {request.error}");
            }
        }
    }

    public void StopAudio()
    {
        if (_audioSource != null)
        {
            _audioSource.Stop();
        }
    }

    public bool IsPlaying()
    {
        return _audioSource != null && _audioSource.isPlaying;
    }
}
