using System.Collections.Generic;
using UnityEngine;

namespace DynamicScroll.Demo
{
    public class DynamicScrollDemoSceneUI : MonoBehaviour
    {
        [SerializeField] private DynamicScrollContent _content;
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
}