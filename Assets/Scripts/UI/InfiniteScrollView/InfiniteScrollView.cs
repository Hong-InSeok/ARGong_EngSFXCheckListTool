using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EngSFXCheckList.UI.InfiniteScrollView
{
    [RequireComponent(typeof(ScrollRect))]
    public class InfiniteScrollView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private RectTransform content;
        
        [Header("Settings")]
        [SerializeField] private float itemHeight = 80f;
        [SerializeField] private float spacing = 10f;
        [SerializeField] private int poolSize = 20;
        
        [Header("Scroll Settings")]
        [SerializeField] private float decelerationRate = 0.08f;
        [SerializeField] private float scrollSensitivity = 25f;
        [SerializeField] private float elasticity = 0.1f;
        
        private ScrollRect scrollRect;
        private RectTransform viewport;
        private List<IScrollViewItem> itemPool = new List<IScrollViewItem>();
        private List<RectTransform> itemRects = new List<RectTransform>();
        
        private int totalItemCount = 0;
        private int firstVisibleIndex = 0;
        private int lastVisibleIndex = 0;
        
        private float viewportHeight;
        private float contentHeight;
        
        private Action<int, IScrollViewItem> onUpdateItem;
        
        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
            viewport = scrollRect.viewport;
            
            if (content == null)
            {
                content = scrollRect.content;
            }
            
            scrollRect.inertia = true;
            scrollRect.decelerationRate = decelerationRate;
            scrollRect.scrollSensitivity = scrollSensitivity;
            scrollRect.elasticity = elasticity;
        }
        
        public void Initialize(Action<int, IScrollViewItem> updateItemCallback)
        {
            onUpdateItem = updateItemCallback;
            CreateItemPool();
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }
        
        private void CreateItemPool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject itemObj = Instantiate(itemPrefab, content);
                RectTransform rectTransform = itemObj.GetComponent<RectTransform>();
                IScrollViewItem scrollItem = itemObj.GetComponent<IScrollViewItem>();
                
                if (scrollItem == null)
                {
                    Debug.LogError("Item prefab must have a component implementing IScrollViewItem");
                    return;
                }
                
                itemPool.Add(scrollItem);
                itemRects.Add(rectTransform);
                itemObj.SetActive(false);
            }
        }
        
        public void SetTotalItemCount(int count)
        {
            totalItemCount = count;
            
            viewportHeight = viewport.rect.height;
            contentHeight = (itemHeight + spacing) * totalItemCount - spacing;
            
            content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);
            
            firstVisibleIndex = -1;
            lastVisibleIndex = -1;
            
            RefreshVisibleItems();
        }
        
        private void OnScrollValueChanged(Vector2 scrollPosition)
        {
            RefreshVisibleItems();
        }
        
        private void RefreshVisibleItems()
        {
            if (totalItemCount == 0 || itemPool.Count == 0) return;
            
            float scrollY = content.anchoredPosition.y;
            
            int newFirstIndex = Mathf.Max(0, Mathf.FloorToInt(scrollY / (itemHeight + spacing)));
            int visibleCount = Mathf.CeilToInt(viewportHeight / (itemHeight + spacing)) + 2;
            int newLastIndex = Mathf.Min(totalItemCount - 1, newFirstIndex + visibleCount);
            
            if (newFirstIndex == firstVisibleIndex && newLastIndex == lastVisibleIndex)
            {
                return;
            }
            
            for (int i = 0; i < itemPool.Count; i++)
            {
                itemPool[i].SetActive(false);
            }
            
            int poolIndex = 0;
            for (int i = newFirstIndex; i <= newLastIndex && poolIndex < itemPool.Count; i++)
            {
                if (i >= 0 && i < totalItemCount)
                {
                    UpdateItem(poolIndex, i);
                    poolIndex++;
                }
            }
            
            firstVisibleIndex = newFirstIndex;
            lastVisibleIndex = newLastIndex;
        }
        
        private void UpdateItem(int poolIndex, int dataIndex)
        {
            IScrollViewItem item = itemPool[poolIndex];
            RectTransform rectTransform = itemRects[poolIndex];
            
            float yPos = -dataIndex * (itemHeight + spacing);
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, yPos);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, itemHeight);
            
            onUpdateItem?.Invoke(dataIndex, item);
            item.SetActive(true);
        }
        
        public void ScrollToTop()
        {
            content.anchoredPosition = Vector2.zero;
            firstVisibleIndex = -1;
            lastVisibleIndex = -1;
            RefreshVisibleItems();
        }
        
        public void Refresh()
        {
            firstVisibleIndex = -1;
            lastVisibleIndex = -1;
            RefreshVisibleItems();
        }
        
        private void OnDestroy()
        {
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            }
        }
    }
}
