using System.Collections;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;

public class ResponsiveDesign : MonoSingleton<ResponsiveDesign>
{
    private ScreenOrientation currentOrientation;

    void Awake()
    {
        m_Instance = this;
        DontDestroyOnLoad(this.gameObject);
        currentOrientation = Screen.orientation;
    }


    public int GetNotchHeight()
    {
        var notchWidth = 0;
        var hige = 0;
        hige = (int)((Screen.width - Screen.safeArea.width) / 2.0f);
        if (hige >= 0)
        {
            notchWidth = (hige / Screen.width) * 1920;
        }

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