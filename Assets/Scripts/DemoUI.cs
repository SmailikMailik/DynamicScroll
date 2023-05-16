using System.Collections.Generic;
using DynamicScroll;
using UnityEngine;

public class DemoUI : MonoBehaviour
{
    [SerializeField] private ScrollContent _content;
    [SerializeField] private int _itemCount = 50;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        var contentDatas = new List<ScrollItemData>();

        for (var i = 0; i < _itemCount; i++)
        {
            contentDatas.Add(new ScrollItemData(i));
        }

        _content.InitScrollContent(contentDatas);
    }
}