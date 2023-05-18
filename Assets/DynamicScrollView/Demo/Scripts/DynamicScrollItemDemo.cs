using DynamicScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace DynamicScrollViewDemo
{
    internal sealed class DynamicScrollItemDemo : DynamicScrollItem<DynamicScrollItemDataDemo>
    {
        [SerializeField] private DynamicScrollRect _dynamicScroll;
        [SerializeField] private Text _text;

        public void FocusOnItem()
        {
            _dynamicScroll.StartFocus(this);
        }

        protected override void OnInit(DynamicScrollItemDataDemo data)
        {
            _text.text = data.Index.ToString();
        }
    }
}