using DynamicScrollView;
using UnityEngine;

namespace DynamicScrollViewDemo
{
    internal sealed class DynamicScrollDemoSceneUI : MonoBehaviour
    {
        [SerializeField] private DynamicScrollRect _rect;
        [SerializeField] private int _itemCount = 50;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            var contentData = new IDynamicScrollItemData[_itemCount];

            for (var i = 0; i < contentData.Length; i++)
            {
                contentData[i] = new DynamicScrollItemDataDemo(i);
            }

            _rect.Init(contentData);
        }
    }
}