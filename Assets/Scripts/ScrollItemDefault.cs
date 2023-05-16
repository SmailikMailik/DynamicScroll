using DynamicScroll;
using TMPro;
using UnityEngine;

public class ScrollItemDefault : ScrollItem<ScrollItemData>
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