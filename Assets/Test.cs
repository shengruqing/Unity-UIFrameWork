using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameLogic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 然后显示TestView
        GUIManager.Instance.ShowView<TestView>();
        LogicEventDispatcher.Instance.Send(new DropBomb(100));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            UpdateUI.SendEventMessage("Test测试");
        }
    }
}