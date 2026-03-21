using System.Collections;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;
using Logger = GameLogic.Logger;

public class SDKManager : MonoSingleton<SDKManager>
{
    private ScreenOrientation currentOrientation;
#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject _ajc;
#endif
    void Awake()
    {
        m_Instance = this;
        DontDestroyOnLoad(this.gameObject);
        currentOrientation = Screen.orientation;
    }

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _ajc = new AndroidJavaObject("com.windsoul.androidphoneinfo.Unity2Android");
#endif
    }

    public bool IsNetworkConnect()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return _ajc.Call<bool>("isConnected");
#else
        return false;
#endif
    }

    public int GetNotchHeight()
    {
        var notchWidth = 0;
        var hige = 0;
#if UNITY_ANDROID && !UNITY_EDITOR
        hige = _ajc.Call<int>("getNotchHeight");
#else
        hige = (int)((Screen.width - Screen.safeArea.width) / 2.0f);
#endif
        if (hige >= 0)
        {
            notchWidth = (hige / Screen.width) * 1920;
        }
        Logger.Log("NotchHeight info: " + Screen.width + ":" + Screen.safeArea+" notchWidth:"+notchWidth);

        return notchWidth;
    }

    private void Update()
    {
        var newOrientation = Screen.orientation;
        if (currentOrientation != newOrientation)
        {
            currentOrientation = newOrientation;
            GUIManager.Instance.OnOrientationChanged(currentOrientation);
        }
    }
}