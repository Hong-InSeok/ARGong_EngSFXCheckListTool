using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EngSFXCheckList.UI.InfiniteScrollView;
using System;

namespace EngSFXCheckList.UI
{
    public class AudioFileListItem : MonoBehaviour, IScrollViewItem
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI noText;
        [SerializeField] private TextMeshProUGUI fileNameText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button itemButton;
        
        [Header("Colors")]
        [SerializeField] private Color evenRowColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        [SerializeField] private Color oddRowColor = new Color(1f, 1f, 1f, 1f);
        
        private int currentIndex;
        private Action onClickCallback;
        
        private void Awake()
        {
            if (itemButton != null)
            {
                itemButton.onClick.AddListener(OnItemClicked);
            }
        }
        
        public void UpdateContent(int index)
        {
            currentIndex = index;
            
            if (backgroundImage != null)
            {
                backgroundImage.color = (index % 2 == 0) ? evenRowColor : oddRowColor;
            }
        }
        
        public void SetData(int no, string fileName)
        {
            if (noText != null)
            {
                noText.text = no.ToString();
            }
            
            if (fileNameText != null)
            {
                fileNameText.text = fileName;
            }
        }
        
        public void SetFileName(string fileName)
        {
            if (fileNameText != null)
            {
                fileNameText.text = fileName;
            }
        }
        
        public void SetClickCallback(Action callback)
        {
            onClickCallback = callback;
        }
        
        private void OnItemClicked()
        {
            onClickCallback?.Invoke();
        }
        
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
