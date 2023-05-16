using System.Collections.Generic;
using UnityEngine;

namespace DynamicScroll.Demo
{
    public class DynamicScrollDemoSceneUI : MonoBehaviour
    {
        [SerializeField] private DynamicScrollRect _rect;
        [SerializeField] private int _itemCount = 50;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            var contentDatas = new List<DynamicScrollItemData>();

            for (var i = 0; i < _itemCount; i++)
            {
                contentDatas.Add(new DynamicScrollItemData(i));
            }

            _rect.Init(contentDatas);
        }
    }
}