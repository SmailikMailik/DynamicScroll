using TMPro;
using UnityEngine;

namespace DynamicScroll.Demo
{
    public class DynamicScrollItemDemo : DynamicScrollItem<ScrollItemData>
    {
        [SerializeField] private DynamicScrollRect _dynamicScroll;
        [SerializeField] private TextMeshProUGUI _text;

        public void FocusOnItem()
        {
            _dynamicScroll.StartFocus(this);
        }

        protected override void InitItemData(ScrollItemData data)
        {
            _text.SetText(data.Index.ToString());
            base.InitItemData(data);
        }
    }
}