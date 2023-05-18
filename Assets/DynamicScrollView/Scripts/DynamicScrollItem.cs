using UnityEngine;

namespace DynamicScrollView
{
    public class DynamicScrollItem<T> : DynamicScrollItem
        where T : DynamicScrollItemData
    {
        public sealed override void Init(int index, Vector2 gridPos, DynamicScrollItemData data)
        {
            Index = index;
            GridIndex = gridPos;

            if (data is T itemData)
                OnInit(itemData);
        }

        protected virtual void OnInit(T data) { }
    }

    public abstract class DynamicScrollItem : MonoBehaviour
    {
        public int Index { get; protected set; }
        public Vector2 GridIndex { get; protected set; }

        public RectTransform RectTransform => transform as RectTransform;

        public abstract void Init(int index, Vector2 gridPos, DynamicScrollItemData data);

        public void Activate()
        {
            gameObject.SetActive(true);
            OnActivate();
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
            OnDeactivate();
        }

        protected virtual void OnActivate() { }
        protected virtual void OnDeactivate() { }
    }

    public class DynamicScrollItemData
    {
        public DynamicScrollItemData(int index)
        {
            Index = index;
        }

        public int Index { get; }
    }
}