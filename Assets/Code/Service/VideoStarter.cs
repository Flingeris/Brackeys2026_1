using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VideoStarter : MonoBehaviour
{
    [Header("Видео")] public string videoFileName;

    [Header("Фейд")] public RawImage videoImage;
    public float fadeBeforeEnd = 2f;
    public float fadeDuration = 1f;

    public static bool isPlayed;

    [SerializeField] private VideoPlayer videoPlayer;
    private AsyncOperation loadOp;


    private void Awake()
    {
        if (isPlayed)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("Нет VideoPlayer на объекте с VideoIntroController");
            return;
        }

        if (videoImage == null)
        {
            videoImage = GetComponent<RawImage>();
        }
        
        if (!G.main.ShowTitle)
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator PlayAndHandleVideo()
    {
        string videoPath = Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = videoPath;

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();

        double length = videoPlayer.length;
        while (double.IsInfinity(length) || length <= 0)
        {
            length = videoPlayer.length;
            yield return null;
        }

        double fadeStartTime = length - fadeBeforeEnd;
        if (fadeStartTime < 0)
            fadeStartTime = 0;

        while (videoPlayer.time < fadeStartTime)
            yield return null;


        StartCoroutine(FadeOutVideo());


        while (videoPlayer.isPlaying)
            yield return null;

        VideoStarter.isPlayed = true;
        Destroy(gameObject);
    }

    private IEnumerator FadeOutVideo()
    {
        if (videoImage == null)
            yield break;

        float t = 0f;
        Color startColor = videoImage.color;
        float startAlpha = startColor.a;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);

            startColor.a = Mathf.Lerp(startAlpha, 0f, k);
            videoImage.color = startColor;

            yield return null;
        }
    }
}