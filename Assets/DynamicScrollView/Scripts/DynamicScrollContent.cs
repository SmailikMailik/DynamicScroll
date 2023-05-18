using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicScrollView
{
    public class DynamicScrollContent : MonoBehaviour
    {
        [SerializeField] private DynamicScrollItem _referenceItem;
        [SerializeField] private Vector2 _spacing = Vector2.zero;

        [Tooltip("It will fill content rect in main axis(horizontal or vertical) automatically. Simply ignores _maxItemCount")]
        [SerializeField] private bool _fillContent;

        [Tooltip("Max rows for horizontal or columns for vertical. If _fillContent is true, value setuped automatically")]
        [Min(1)] [SerializeField] private int _maxItemCount = 1;

        [Tooltip("Items loaded after last visible row for vertical or column for horizontal")]
        [Min(0)] [SerializeField] private int _extraItemCount;

        private readonly List<DynamicScrollItem> _activatedItems = new();
        private readonly List<DynamicScrollItem> _deactivatedItems = new();

        public Vector2 Spacing => _spacing;

        public Action<DynamicScrollItem> ItemActivated { get; set; }
        public Action<DynamicScrollItem> ItemDeactivated { get; set; }

        public DynamicScrollRect DynamicScrollRect { get; private set; }
        public List<DynamicScrollItemData> ContentData { get; private set; }

        public float ItemWidth { get; private set; }
        public float ItemHeight { get; private set; }

        private void Awake()
        {
            ItemWidth = _referenceItem.RectTransform.rect.width;
            ItemHeight = _referenceItem.RectTransform.rect.height;
            _referenceItem.gameObject.SetActive(false);
        }

        public void Init(DynamicScrollRect dynamicScrollRect, List<DynamicScrollItemData> contentData)
        {
            DynamicScrollRect = dynamicScrollRect;
            ContentData = contentData;

            if (DynamicScrollRect.vertical) InitItemsVertical(contentData);
            if (DynamicScrollRect.horizontal) InitItemsHorizontal(contentData);
        }

        public void Clear()
        {
            var activatedItems = new List<DynamicScrollItem>(_activatedItems);

            foreach (var item in activatedItems)
            {
                DeactivateItem(item);
            }
        }

        public bool CanAddNewItemIntoHead()
        {
            return HasActivatedItems() && _activatedItems[0].Index - 1 >= 0;
        }

        public bool CanAddNewItemIntoTail()
        {
            return HasActivatedItems() && _activatedItems[^1].Index < ContentData.Count - 1;
        }

        public Vector2 GetFirstItemPos()
        {
            return HasActivatedItems() ? _activatedItems[0].RectTransform.anchoredPosition : Vector2.zero;
        }

        public Vector2 GetLastItemPos()
        {
            return HasActivatedItems() ? _activatedItems[^1].RectTransform.anchoredPosition : Vector2.zero;
        }

        public void AddIntoHead()
        {
            for (var i = 0; i < _maxItemCount; i++)
            {
                AddItemToHead();
            }
        }

        public void AddIntoTail()
        {
            for (var i = 0; i < _maxItemCount; i++)
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

        public bool AtTheEndOfContent(DynamicScrollItem item)
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

        private void InitItemsVertical(ICollection contentData)
        {
            var itemCount = 0;
            var initialGridSize = CalculateInitialGridSize();

            for (var col = 0; col < initialGridSize.y; col++)
            {
                for (var row = 0; row < initialGridSize.x; row++)
                {
                    if (itemCount == contentData.Count)
                        return;

                    ActivateItem(itemCount);
                    itemCount++;
                }
            }
        }

        private void InitItemsHorizontal(ICollection contentData)
        {
            var itemCount = 0;
            var initialGridSize = CalculateInitialGridSize();

            for (var col = 0; col < initialGridSize.y; col++)
            {
                for (var row = 0; row < initialGridSize.x; row++)
                {
                    if (itemCount == contentData.Count)
                        return;

                    ActivateItem(itemCount);
                    itemCount++;
                }
            }
        }

        private Vector2Int CalculateInitialGridSize()
        {
            var contentSize = DynamicScrollRect.content.rect.size;
            var extraItemCount = _extraItemCount + 1;

            if (DynamicScrollRect.vertical)
            {
                var verticalItemCount = extraItemCount + (int)(contentSize.y / (ItemHeight + _spacing.y));

                if (_fillContent)
                {
                    var horizontalItemCount = (int)((contentSize.x + _spacing.x) / (ItemWidth + _spacing.x));
                    _maxItemCount = horizontalItemCount;
                }

                return new Vector2Int(_maxItemCount, verticalItemCount);
            }

            if (DynamicScrollRect.horizontal)
            {
                var horizontalItemCount = extraItemCount + (int)(contentSize.x / (ItemWidth + _spacing.x));

                if (_fillContent)
                {
                    var verticalItemCount = (int)((contentSize.y + _spacing.y) / (ItemHeight + _spacing.y));
                    _maxItemCount = verticalItemCount;
                }

                return new Vector2Int(horizontalItemCount, _maxItemCount);
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

            if (itemIndex == ContentData.Count)
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

        private DynamicScrollItem ActivateItem(int itemIndex)
        {
            var gridPos = GetGridPosition(itemIndex);
            var anchoredPos = GetAnchoredPosition(gridPos);

            DynamicScrollItem scrollItem;

            if (_deactivatedItems.Count == 0)
                scrollItem = CreateNewScrollItem();
            else
            {
                scrollItem = _deactivatedItems[0];
                _deactivatedItems.Remove(scrollItem);
            }

            scrollItem.gameObject.name = $"{gridPos.x}_{gridPos.y}";
            scrollItem.RectTransform.anchoredPosition = anchoredPos;
            scrollItem.Init(itemIndex, gridPos, ContentData[itemIndex]);

            var insertHead = _activatedItems.Count == 0 ||
                             (_activatedItems.Count > 0 && _activatedItems[0].Index > itemIndex);

            if (insertHead)
                _activatedItems.Insert(0, scrollItem);
            else
                _activatedItems.Add(scrollItem);

            scrollItem.Activate();
            ItemActivated?.Invoke(scrollItem);

            return scrollItem;
        }

        private void DeactivateItem(DynamicScrollItem item)
        {
            _activatedItems.Remove(item);
            _deactivatedItems.Add(item);

            item.Deactivate();
            ItemDeactivated?.Invoke(item);
        }

        private DynamicScrollItem CreateNewScrollItem()
        {
            var item = Instantiate(_referenceItem.gameObject, DynamicScrollRect.content);

            var scrollItem = item.GetComponent<DynamicScrollItem>();
            scrollItem.RectTransform.pivot = new Vector2(0, 1);

            return scrollItem;
        }

        private Vector2 GetGridPosition(int itemIndex)
        {
            var col = itemIndex / _maxItemCount;
            var row = itemIndex - col * _maxItemCount;

            if (DynamicScrollRect.vertical) return new Vector2(row, col);
            if (DynamicScrollRect.horizontal) return new Vector2(col, row);
            return Vector2.zero;
        }

        private Vector2 GetAnchoredPosition(Vector2 gridPosition)
        {
            return new Vector2(
                gridPosition.x * ItemWidth + gridPosition.x * _spacing.x,
                -gridPosition.y * ItemHeight - gridPosition.y * _spacing.y
            );
        }

        private bool HasActivatedItems()
        {
            return _activatedItems is { Count: > 0 };
        }
    }
}