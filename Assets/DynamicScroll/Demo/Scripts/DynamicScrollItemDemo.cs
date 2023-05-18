using UnityEngine;
using UnityEngine.UI;

namespace DynamicScroll.Demo
{
    public class DynamicScrollItemDemo : DynamicScrollItem<DynamicScrollItemData>
    {
        [SerializeField] private DynamicScrollRect _dynamicScroll;
        [SerializeField] private Text _text;

        public void FocusOnItem()
        {
            _dynamicScroll.StartFocus(this);
        }

        protected override void OnInit(DynamicScrollItemData data)
        {
            _text.text = data.Index.ToString();
            base.OnInit(data);
        }
    }
}