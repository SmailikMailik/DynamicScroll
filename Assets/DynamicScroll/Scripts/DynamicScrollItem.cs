using UnityEngine;

namespace DynamicScroll
{
    public class DynamicScrollItem<T> : ScrollItem
        where T : ScrollItemData
    {
        public sealed override void InitItem(int index, Vector2 gridPos, ScrollItemData data)
        {
            Index = index;
            GridIndex = gridPos;

            if (data is T itemData)
                InitItemData(itemData);
        }

        protected virtual void InitItemData(T data) { }
    }

    public abstract class ScrollItem : MonoBehaviour
    {
        public int Index { get; protected set; }
        public Vector2 GridIndex { get; protected set; }

        public RectTransform RectTransform => transform as RectTransform;

        public abstract void InitItem(int index, Vector2 gridPos, ScrollItemData data);

        public void Activated()
        {
            gameObject.SetActive(true);
            ActivatedCustomActions();
        }

        public void Deactivated()
        {
            gameObject.SetActive(false);
            DeactivatedCustomActions();
        }

        protected virtual void ActivatedCustomActions() { }
        protected virtual void DeactivatedCustomActions() { }
    }

    public class ScrollItemData
    {
        public int Index { get; }

        public ScrollItemData(int index)
        {
            Index = index;
        }
    }
}