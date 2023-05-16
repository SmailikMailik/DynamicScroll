using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicScroll
{
    public class DynamicScrollContent : MonoBehaviour
    {
        [SerializeField] private Vector2 _spacing = Vector2.zero;
        public Vector2 Spacing => _spacing;

        [Tooltip("It will fill content rect in main axis(horizontal or vertical) automatically. Simply ignores _fixedItemCount")]
        [SerializeField] private bool _fillContent = false;

        [Tooltip("If scroll is vertical it is item count in each row vice versa for horizontal")]
        [Min(1)] [SerializeField] private int _fixedItemCount = 1;

        private DynamicScrollRect _dynamicScrollRect;

        public DynamicScrollRect DynamicScrollRect
        {
            get
            {
                if (_dynamicScrollRect == null)
                    _dynamicScrollRect = GetComponent<DynamicScrollRect>();

                return _dynamicScrollRect;
            }
        }

        private ScrollItem _referenceItem;

        private ScrollItem ReferenceItem
        {
            get
            {
                if (_referenceItem == null)
                {
                    _referenceItem = GetComponentInChildren<ScrollItem>();

                    if (_referenceItem == null)
                        throw new Exception(
                            "No Scroll Item found under scroll rect content.You should create reference scroll item under DynamicScroll Content first.");
                }

                return _referenceItem;
            }
        }

        private readonly List<ScrollItem> _activatedItems = new();
        private readonly List<ScrollItem> _deactivatedItems = new();

        public List<ScrollItemData> Datum { get; private set; }

        public float ItemWidth => ReferenceItem.RectTransform.rect.width;
        public float ItemHeight => ReferenceItem.RectTransform.rect.height;

        public Action<ScrollItem> OnItemActivated { get; set; }
        public Action<ScrollItem> OnItemDeactivated { get; set; }

        public void InitScrollContent(List<ScrollItemData> contentDatum)
        {
            Datum = contentDatum;

            if (DynamicScrollRect.vertical) InitItemsVertical(contentDatum);
            if (DynamicScrollRect.horizontal) InitItemsHorizontal(contentDatum);
        }

        public void ClearContent()
        {
            var activatedItems = new List<ScrollItem>(_activatedItems);

            foreach (var item in activatedItems)
            {
                DeactivateItem(item);
            }
        }

        public bool CanAddNewItemIntoTail()
        {
            if (_activatedItems == null || _activatedItems.Count == 0)
                return false;

            return _activatedItems[^1].Index < Datum.Count - 1;
        }

        public bool CanAddNewItemIntoHead()
        {
            if (_activatedItems == null || _activatedItems.Count == 0)
                return false;

            return _activatedItems[0].Index - 1 >= 0;
        }

        public Vector2 GetFirstItemPos()
        {
            if (_activatedItems == null || _activatedItems.Count == 0)
                return Vector2.zero;

            return _activatedItems[0].RectTransform.anchoredPosition;
        }

        public Vector2 GetLastItemPos()
        {
            if (_activatedItems == null || _activatedItems.Count == 0)
                return Vector2.zero;

            return _activatedItems[^1].RectTransform.anchoredPosition;
        }

        public void AddIntoHead()
        {
            for (var i = 0; i < _fixedItemCount; i++)
            {
                AddItemToHead();
            }
        }

        public void AddIntoTail()
        {
            for (var i = 0; i < _fixedItemCount; i++)
            {
                AddItemToTail();
            }
        }

        public void DeleteFromHead()
        {
            if (DynamicScrollRect.vertical)
            {
                var firstRowIndex = (int)_activatedItems[0].GridIndex.y;
                DeleteRow(firstRowIndex);
            }

            if (DynamicScrollRect.horizontal)
            {
                var firstColIndex = (int)_activatedItems[0].GridIndex.x;
                DeleteColumn(firstColIndex);
            }
        }

        public void DeleteFromTail()
        {
            if (DynamicScrollRect.vertical)
            {
                var lastRowIndex = (int)_activatedItems[^1].GridIndex.y;
                DeleteRow(lastRowIndex);
            }

            if (DynamicScrollRect.horizontal)
            {
                var lastColIndex = (int)_activatedItems[^1].GridIndex.x;
                DeleteColumn(lastColIndex);
            }
        }

        public bool AtTheEndOfContent(ScrollItem item)
        {
            if (DynamicScrollRect.vertical)
            {
                var itemAnchoredPositionY = item.RectTransform.anchoredPosition.y;
                var lastActiveItemAnchoredPositionY = _activatedItems[^1].RectTransform.anchoredPosition.y;
                return !CanAddNewItemIntoTail() && itemAnchoredPositionY == lastActiveItemAnchoredPositionY;
            }

            if (DynamicScrollRect.horizontal)
            {
                var itemAnchoredPositionX = item.RectTransform.anchoredPosition.x;
                var lastActiveItemAnchoredPositionX = _activatedItems[^1].RectTransform.anchoredPosition.x;
                return !CanAddNewItemIntoTail() && itemAnchoredPositionX == lastActiveItemAnchoredPositionX;
            }

            return false;
        }

        private void Awake()
        {
            ReferenceItem.gameObject.SetActive(false);
        }

        private void InitItemsVertical(ICollection contentDatum)
        {
            var itemCount = 0;
            var initialGridSize = CalculateInitialGridSize();

            for (var col = 0; col < initialGridSize.y; col++)
            {
                for (var row = 0; row < initialGridSize.x; row++)
                {
                    if (itemCount == contentDatum.Count)
                        return;

                    ActivateItem(itemCount);
                    itemCount++;
                }
            }
        }

        private void InitItemsHorizontal(ICollection contentDatum)
        {
            var itemCount = 0;
            var initialGridSize = CalculateInitialGridSize();

            for (var col = 0; col < initialGridSize.y; col++)
            {
                for (var row = 0; row < initialGridSize.x; row++)
                {
                    if (itemCount == contentDatum.Count)
                        return;

                    ActivateItem(itemCount);
                    itemCount++;
                }
            }
        }

        private Vector2Int CalculateInitialGridSize()
        {
            var contentSize = DynamicScrollRect.content.rect.size;

            if (DynamicScrollRect.vertical)
            {
                var verticalItemCount = 4 + (int)(contentSize.y / (ItemHeight + _spacing.y));

                if (_fillContent)
                {
                    var horizontalItemCount = (int)((contentSize.x + _spacing.x) / (ItemWidth + _spacing.x));
                    _fixedItemCount = horizontalItemCount;
                }

                return new Vector2Int(_fixedItemCount, verticalItemCount);
            }

            if (DynamicScrollRect.horizontal)
            {
                var horizontalItemCount = 4 + (int)(contentSize.x / (ItemWidth + _spacing.x));

                if (_fillContent)
                {
                    var verticalItemCount = (int)((contentSize.y + _spacing.y) / (ItemHeight + _spacing.y));
                    _fixedItemCount = verticalItemCount;
                }

                return new Vector2Int(horizontalItemCount, _fixedItemCount);
            }

            return Vector2Int.zero;
        }

        private void DeleteRow(int rowIndex)
        {
            var items = _activatedItems.FindAll(scrollItem => (int)scrollItem.GridIndex.y == rowIndex);

            foreach (var item in items)
            {
                DeactivateItem(item);
            }
        }

        private void DeleteColumn(int colIndex)
        {
            var items = _activatedItems.FindAll(scrollItem => (int)scrollItem.GridIndex.x == colIndex);

            foreach (var item in items)
            {
                DeactivateItem(item);
            }
        }

        private void AddItemToTail()
        {
            if (!CanAddNewItemIntoTail())
                return;

            var itemIndex = _activatedItems[^1].Index + 1;

            if (itemIndex == Datum.Count)
                return;

            ActivateItem(itemIndex);
        }

        private void AddItemToHead()
        {
            if (!CanAddNewItemIntoHead())
                return;

            var itemIndex = _activatedItems[0].Index - 1;

            if (itemIndex < 0)
                return;

            ActivateItem(itemIndex);
        }

        private ScrollItem ActivateItem(int itemIndex)
        {
            var gridPos = GetGridPosition(itemIndex);
            var anchoredPos = GetAnchoredPosition(gridPos);

            ScrollItem scrollItem;

            if (_deactivatedItems.Count == 0)
            {
                scrollItem = CreateNewScrollItem();
            }
            else
            {
                scrollItem = _deactivatedItems[0];
                _deactivatedItems.Remove(scrollItem);
            }

            scrollItem.gameObject.name = $"{gridPos.x}_{gridPos.y}";
            scrollItem.RectTransform.anchoredPosition = anchoredPos;
            scrollItem.InitItem(itemIndex, gridPos, Datum[itemIndex]);

            var insertHead = _activatedItems.Count == 0 ||
                             _activatedItems.Count > 0 && _activatedItems[0].Index > itemIndex;

            if (insertHead)
                _activatedItems.Insert(0, scrollItem);
            else
                _activatedItems.Add(scrollItem);

            scrollItem.Activated();
            OnItemActivated?.Invoke(scrollItem);

            return scrollItem;
        }

        private void DeactivateItem(ScrollItem item)
        {
            _activatedItems.Remove(item);
            _deactivatedItems.Add(item);

            item.Deactivated();
            OnItemDeactivated?.Invoke(item);
        }

        private ScrollItem CreateNewScrollItem()
        {
            var item = Instantiate(ReferenceItem.gameObject, DynamicScrollRect.content);

            var scrollItem = item.GetComponent<ScrollItem>();
            scrollItem.RectTransform.pivot = new Vector2(0, 1);

            return scrollItem;
        }

        private Vector2 GetGridPosition(int itemIndex)
        {
            var col = itemIndex / _fixedItemCount;
            var row = itemIndex - (col * _fixedItemCount);

            if (DynamicScrollRect.vertical) return new Vector2(row, col);
            if (DynamicScrollRect.horizontal) return new Vector2(col, row);
            return Vector2.zero;
        }

        private Vector2 GetAnchoredPosition(Vector2 gridPosition) => new Vector2
        (
            gridPosition.x * ItemWidth + gridPosition.x * _spacing.x,
            -gridPosition.y * ItemHeight - gridPosition.y * _spacing.y
        );
    }
}