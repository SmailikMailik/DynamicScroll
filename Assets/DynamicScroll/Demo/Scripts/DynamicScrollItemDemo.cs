using TMPro;
using UnityEngine;

namespace DynamicScroll.Demo
{
    public class DynamicScrollItemDemo : DynamicScrollItem<DynamicScrollItemData>
    {
        [SerializeField] private DynamicScrollRect _dynamicScroll;
        [SerializeField] private TextMeshProUGUI _text;

        public void FocusOnItem()
        {
            _dynamicScroll.StartFocus(this);
        }

        protected override void OnInit(DynamicScrollItemData data)
        {
            _text.SetText(data.Index.ToString());
            base.OnInit(data);
        }
    }
}