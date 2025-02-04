﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DynamicScrollView
{
    [Serializable]
    public class DynamicScrollRestrictionSettings
    {
        [SerializeField] private float _contentOverflowRange = 125f;

        [SerializeField] [Range(0, 1)] private float _contentDecelerationInOverflow = 0.5f;
        public float ContentOverflowRange => _contentOverflowRange;
        public float ContentDecelerationInOverflow => _contentDecelerationInOverflow;
    }

    [Serializable]
    public class FocusSettings
    {
        [SerializeField] private float _focusOffset;

        [SerializeField] private float _focusDuration = 0.25f;
        public float FocusOffset => _focusOffset;
        public float FocusDuration => _focusDuration;
    }

    public class DynamicScrollRect : ScrollRect
    {
        [SerializeField] private DynamicScrollContent _content;
        [SerializeField] private DynamicScrollRestrictionSettings _restrictionSettings;
        [SerializeField] private FocusSettings _focusSettings;

        private Vector2 _contentStartPos = Vector2.zero;
        private Vector2 _dragCurPosition = Vector2.zero;
        private Vector2 _dragStartPosition = Vector2.zero;
        private Vector2 _lastDragDelta = Vector2.zero;

        private IEnumerator _runBackRoutine;
        private IEnumerator _focusRoutine;

        private bool _isDragging;
        private bool _needRunBack;

        private bool _runBackActive;
        private bool _focusActive;

        protected override void Awake()
        {
            movementType = MovementType.Unrestricted;
            onValueChanged.AddListener(OnScrollRectValueChanged);
            vertical = !horizontal;
            base.Awake();
        }

        protected override void OnDestroy()
        {
            onValueChanged.RemoveListener(OnScrollRectValueChanged);
            base.OnDestroy();
        }

        public void Init(IDynamicScrollItemData[] contentData)
        {
            _content.Init(this, contentData);
        }

        public void ResetContent()
        {
            StopMovement();
            StopRunBackRoutine();
            content.anchoredPosition = Vector2.zero;
        }

        public void StartFocus(DynamicScrollItem focusItem)
        {
            StartFocusItemRoutine(focusItem);
        }

        public void CancelFocus()
        {
            StopFocusItemRoutine();
        }

        private void UpdateItems(Vector2 delta)
        {
            if (vertical) UpdateItemsVertical(delta);
            if (horizontal) UpdateItemsHorizontal(delta);
        }

        private void UpdateItemsVertical(Vector2 delta)
        {
            var positiveDelta = delta.y > 0;

            var anchoredPositionY = content.anchoredPosition.y;
            var spacingY = _content.Spacing.y;
            var itemHeight = _content.ItemHeight;
            var firstItemPosY = _content.GetFirstItemPos().y;
            var lastItemPosY = _content.GetLastItemPos().y;
            var rectHeight = viewport.rect.height;

            if (positiveDelta)
            {
                if (-lastItemPosY - anchoredPositionY <= rectHeight + spacingY)
                    _content.AddIntoTail();

                if (anchoredPositionY - -firstItemPosY >= 2 * itemHeight + spacingY)
                    _content.DeleteFromHead();
            }
            else
            {
                if (anchoredPositionY + firstItemPosY <= itemHeight + spacingY)
                    _content.AddIntoHead();

                if (-lastItemPosY - anchoredPositionY >= rectHeight + itemHeight + spacingY)
                    _content.DeleteFromTail();
            }
        }

        private void UpdateItemsHorizontal(Vector2 delta)
        {
            var positiveDelta = delta.x > 0;

            var anchoredPositionX = content.anchoredPosition.x;
            var spacingX = _content.Spacing.x;
            var itemWidth = _content.ItemWidth;
            var firstItemPosX = _content.GetFirstItemPos().x;
            var lastItemPosX = _content.GetLastItemPos().x;
            var rectWidth = viewport.rect.width;

            if (positiveDelta)
            {
                if (firstItemPosX + anchoredPositionX >= -itemWidth - spacingX)
                    _content.AddIntoHead();

                if (lastItemPosX + anchoredPositionX >= rectWidth + itemWidth + spacingX)
                    _content.DeleteFromTail();
            }
            else
            {
                if (lastItemPosX + anchoredPositionX <= rectWidth + spacingX)
                    _content.AddIntoTail();

                if (firstItemPosX + anchoredPositionX <= -2 * itemWidth - spacingX)
                    _content.DeleteFromHead();
            }
        }

        private bool IsDragValid(Vector2 delta)
        {
            if (vertical) return IsDragValidVertical(delta);
            if (horizontal) return IsDragValidHorizontal(delta);
            return false;
        }

        private bool IsDragValidVertical(Vector2 delta)
        {
            var positiveDelta = delta.y > 0;

            if (positiveDelta)
            {
                var lastItemPos = _content.GetLastItemPos();

                // Calculate local position of last item's end position in viewport rect
                if (!_content.CanAddNewItemIntoTail() &&
                    content.anchoredPosition.y + viewport.rect.height + lastItemPos.y - _content.ItemHeight > 0)
                    return false;
            }
            else
            {
                if (!_content.CanAddNewItemIntoHead() &&
                    content.anchoredPosition.y <= 0)
                    return false;
            }

            return true;
        }

        private bool IsDragValidHorizontal(Vector2 delta)
        {
            var positiveDelta = delta.x > 0;

            if (positiveDelta)
            {
                if (!_content.CanAddNewItemIntoHead() && content.anchoredPosition.x >= 0) return false;
            }
            else
            {
                var lastItemPos = _content.GetLastItemPos();

                // Calculate local position of last item's end position in viewport rect 
                if (!_content.CanAddNewItemIntoTail() &&
                    content.anchoredPosition.x + lastItemPos.x <= viewport.rect.width - _content.ItemWidth)
                    return false;
            }

            return true;
        }

        private Vector2 GetRestrictedContentPositionOnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewRect,
                eventData.position,
                eventData.pressEventCamera, out var localCursor);

            var delta = localCursor - _dragCurPosition;
            var position = CalculateContentPos(localCursor);

            if (vertical)
            {
                var restriction = GetVerticalRestrictionWeight(delta);
                var result = CalculateRestrictedPosition(content.anchoredPosition, position, restriction);
                result.x = content.anchoredPosition.x;
                return result;
            }

            if (horizontal)
            {
                var restriction = GetHorizontalRestrictionWeight(delta);
                var result = CalculateRestrictedPosition(content.anchoredPosition, position, restriction);
                result.y = content.anchoredPosition.y;
                return result;
            }

            return Vector2.zero;
        }

        private Vector2 GetRestrictedContentPositionOnScroll(Vector2 delta)
        {
            var restriction = vertical ? GetVerticalRestrictionWeight(delta) : GetHorizontalRestrictionWeight(delta);
            var deltaPos = velocity * Time.deltaTime;
            var result = Vector2.zero;

            if (vertical)
            {
                deltaPos.x = 0;
                var curPos = content.anchoredPosition;
                var nextPos = curPos + deltaPos;
                result = CalculateRestrictedPosition(curPos, nextPos, restriction);
                result.x = 0;
            }

            if (horizontal)
            {
                deltaPos.y = 0;
                var curPos = content.anchoredPosition;
                var nextPos = curPos + deltaPos;
                result = CalculateRestrictedPosition(curPos, nextPos, restriction);
                result.y = 0;
            }

            velocity *= _restrictionSettings.ContentDecelerationInOverflow;
            return result;
        }

        private float GetVerticalRestrictionWeight(Vector2 delta)
        {
            var positiveDelta = delta.y > 0;
            var maxLimit = _restrictionSettings.ContentOverflowRange;

            if (!positiveDelta)
                return Mathf.Clamp(Mathf.Abs(content.anchoredPosition.y) / maxLimit, 0, 1);

            var lastItemPos = _content.GetLastItemPos();

            if (Mathf.Abs(lastItemPos.y) <= viewport.rect.height - _content.ItemHeight)
            {
                var max = lastItemPos.y + maxLimit;
                var cur = content.anchoredPosition.y + lastItemPos.y;
                var diff = max - cur;

                return 1f - Mathf.Clamp(diff / maxLimit, 0, 1);
            }
            else
            {
                var max = -(viewport.rect.height - maxLimit - _content.ItemHeight);
                var cur = content.anchoredPosition.y + lastItemPos.y;
                var diff = max - cur;

                return 1f - Mathf.Clamp(diff / maxLimit, 0, 1);
            }
        }

        private float GetHorizontalRestrictionWeight(Vector2 delta)
        {
            var positiveDelta = delta.x > 0;
            var maxLimit = _restrictionSettings.ContentOverflowRange;

            if (positiveDelta)
                return Mathf.Clamp(Mathf.Abs(content.anchoredPosition.x) / maxLimit, 0, 1);

            var lastItemPos = _content.GetLastItemPos();

            if (lastItemPos.x <= viewport.rect.width - _content.ItemWidth)
            {
                var max = lastItemPos.x - maxLimit;
                var cur = content.anchoredPosition.x + lastItemPos.x;
                var diff = cur - max;

                return 1f - Mathf.Clamp(diff / maxLimit, 0, 1);
            }
            else
            {
                var max = viewport.rect.width - maxLimit - _content.ItemWidth;
                var cur = content.anchoredPosition.x + lastItemPos.x;
                var diff = cur - max;

                return 1 - Mathf.Clamp(diff / maxLimit, 0, 1);
            }
        }

        private Vector2 CalculateSnapPosition()
        {
            if (vertical)
            {
                if (content.anchoredPosition.y < 0)
                    return Vector2.zero;

                var lastItemPos = _content.GetLastItemPos();

                if (Mathf.Abs(lastItemPos.y) <= viewport.rect.height - _content.ItemHeight)
                    return Vector2.zero;

                var target = -(viewport.rect.height - _content.ItemHeight);
                var cur = content.anchoredPosition.y + lastItemPos.y;
                var diff = target - cur;

                return content.anchoredPosition + new Vector2(0, diff);
            }

            if (horizontal)
            {
                if (content.anchoredPosition.x > 0)
                    return Vector2.zero;

                var lastItemPos = _content.GetLastItemPos();

                if (lastItemPos.x <= viewport.rect.width - _content.ItemWidth)
                    return Vector2.zero;

                var target = viewport.rect.width - _content.ItemWidth;
                var cur = content.anchoredPosition.x + lastItemPos.x;
                var diff = target - cur;

                return content.anchoredPosition + new Vector2(diff, 0);
            }

            return Vector2.zero;
        }

        private Vector2 CalculateContentPos(Vector2 localCursor)
        {
            var dragDelta = localCursor - _dragStartPosition;
            var position = _contentStartPos + dragDelta;
            return position;
        }

        private Vector2 CalculateRestrictedPosition(Vector2 curPos, Vector2 nextPos, float restrictionWeight)
        {
            var weightedPrev = curPos * restrictionWeight;
            var weightedNext = nextPos * (1 - restrictionWeight);
            var result = weightedPrev + weightedNext;
            return result;
        }

        #region Event Callbacks

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);

            StopRunBackRoutine();

            _isDragging = true;

            _contentStartPos = content.anchoredPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewport,
                eventData.position,
                eventData.pressEventCamera,
                out _dragStartPosition);

            _dragCurPosition = _dragStartPosition;

            CancelFocus();
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    viewRect,
                    eventData.position,
                    eventData.pressEventCamera, out var localCursor))
                return;

            StopRunBackRoutine();

            if (!IsDragValid(localCursor - _dragCurPosition))
            {
                var restrictedPos = GetRestrictedContentPositionOnDrag(eventData);

                _needRunBack = true;

                SetContentAnchoredPosition(restrictedPos);

                return;
            }

            UpdateBounds();

            _needRunBack = false;

            _lastDragDelta = localCursor - _dragCurPosition;

            _dragCurPosition = localCursor;

            SetContentAnchoredPosition(CalculateContentPos(localCursor));

            UpdateItems(_lastDragDelta);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);

            _isDragging = false;

            if (_needRunBack)
            {
                StopMovement();

                StartRunBackRoutine();
            }
        }

        private void OnScrollRectValueChanged(Vector2 val)
        {
            if (_runBackActive || _isDragging || _focusActive)
                return;

            var delta = velocity.normalized;

            if (IsDragValid(delta))
            {
                UpdateItems(delta);
                return;
            }

            var contentPos = GetRestrictedContentPositionOnScroll(delta);

            SetContentAnchoredPosition(contentPos);

            if ((velocity * Time.deltaTime).magnitude < 5)
            {
                StopMovement();
                StartRunBackRoutine();
            }
        }

        #endregion

        #region Run Back Routine

        private void StartRunBackRoutine()
        {
            StopRunBackRoutine();
            _runBackRoutine = RunBackProgress();
            StartCoroutine(_runBackRoutine);
        }

        private void StopRunBackRoutine()
        {
            if (_runBackRoutine != null) StopCoroutine(_runBackRoutine);
            _runBackActive = false;
        }

        private IEnumerator RunBackProgress()
        {
            _runBackActive = true;

            var timePassed = 0f;
            var duration = 0.25f;
            var startPos = content.anchoredPosition;
            var endPos = CalculateSnapPosition();

            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                var pos = Vector2.Lerp(startPos, endPos, timePassed / duration);
                SetContentAnchoredPosition(pos);

                yield return null;
            }

            SetContentAnchoredPosition(endPos);
            _runBackActive = false;
        }

        #endregion

        #region Focus Routine

        private Vector2 GetFocusPosition(DynamicScrollItem focusItem)
        {
            var contentPos = content.anchoredPosition;

            if (vertical)
            {
                // focus item above the viewport
                if (contentPos.y + focusItem.RectTransform.anchoredPosition.y > 0)
                {
                    var diff = contentPos.y + focusItem.RectTransform.anchoredPosition.y + _focusSettings.FocusOffset;
                    var focusPos = contentPos - new Vector2(0, diff);
                    focusPos.y = Mathf.Max(focusPos.y, 0);
                    return focusPos;
                }

                // focus item under the viewport
                if (viewport.rect.height +
                    (contentPos.y + focusItem.RectTransform.anchoredPosition.y - _content.ItemHeight) < 0)
                {
                    var diff = -contentPos.y - viewport.rect.height +
                               -focusItem.RectTransform.anchoredPosition.y + _content.ItemHeight +
                               _focusSettings.FocusOffset;

                    if (_content.AtTheEndOfContent(focusItem))
                        return CalculateSnapPosition();

                    var focusPos = contentPos + new Vector2(0, diff);
                    var contentMovementLimit = contentPos.y + _content.GetLastItemPos().y - _content.ItemHeight +
                                               viewport.rect.height;

                    focusPos.y = Mathf.Max(focusPos.y, contentMovementLimit);

                    return focusPos;
                }
            }

            if (horizontal)
            {
                // focus item at the left of the viewport
                if (contentPos.x + focusItem.RectTransform.anchoredPosition.x < 0)
                {
                    var diff = contentPos.x + focusItem.RectTransform.anchoredPosition.x - _focusSettings.FocusOffset;
                    var focusPos = contentPos - new Vector2(0, diff);
                    focusPos.x = Mathf.Max(focusPos.x, 0);
                    return focusPos;
                }

                // focus item at the right of the viewport
                if (viewport.rect.width +
                    (-contentPos.x - focusItem.RectTransform.anchoredPosition.x - _content.ItemWidth) < 0)
                {
                    var diff = -viewport.rect.width + contentPos.x + focusItem.RectTransform.anchoredPosition.x
                        + _content.ItemWidth - _focusSettings.FocusOffset;

                    if (_content.AtTheEndOfContent(focusItem))
                        return CalculateSnapPosition();

                    var focusPos = contentPos + new Vector2(0, diff);
                    var contentMoveLimit = -contentPos.x - _content.GetLastItemPos().x + _content.ItemWidth +
                                           -viewport.rect.width;

                    focusPos.x = Mathf.Max(focusPos.x, contentMoveLimit);

                    return focusPos;
                }
            }

            return content.anchoredPosition;
        }

        private void StartFocusItemRoutine(DynamicScrollItem scrollItem)
        {
            StopFocusItemRoutine();
            _focusRoutine = FocusProgress(GetFocusPosition(scrollItem));
            StartCoroutine(_focusRoutine);
        }

        private void StopFocusItemRoutine()
        {
            if (_focusRoutine != null) StopCoroutine(_focusRoutine);
            _focusActive = false;
        }

        private IEnumerator FocusProgress(Vector2 focusPos)
        {
            _focusActive = true;

            float timePassed = 0;
            var startPos = content.anchoredPosition;

            while (timePassed < _focusSettings.FocusDuration)
            {
                timePassed += Time.deltaTime;

                var pos = Vector2.Lerp(startPos, focusPos, timePassed / _focusSettings.FocusDuration);
                var delta = pos - content.anchoredPosition;

                UpdateItems(delta);
                SetContentAnchoredPosition(pos);

                yield return null;
            }

            SetContentAnchoredPosition(focusPos);
            _focusActive = false;
        }

        #endregion
    }
}