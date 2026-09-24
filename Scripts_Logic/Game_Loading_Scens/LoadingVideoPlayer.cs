using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
public class LoadingVideoPlayer : MonoBehaviour
{
       public VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene("Main menu");
    }
}
