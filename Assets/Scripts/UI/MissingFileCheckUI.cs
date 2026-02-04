using System.Collections.Generic;
using UnityEngine;
using TMPro;
using EngSFXCheckList.Data;

namespace EngSFXCheckList.UI
{
    public class MissingFileCheckUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private InfiniteScrollView.InfiniteScrollView infiniteScrollView;
        [SerializeField] private TextMeshProUGUI noDataText;

        private List<AudioFileData> missingFiles = new List<AudioFileData>();

        public void Init()
        {
            if (infiniteScrollView != null)
            {
                infiniteScrollView.Initialize(OnUpdateScrollItem);
            }

            ShowNoDataMessage(false);
        }

        public void SetMissingFiles(List<AudioFileData> files)
        {
            missingFiles = files;

            if (missingFiles.Count == 0)
            {
                ShowNoDataMessage(true);
                if (infiniteScrollView != null)
                {
                    infiniteScrollView.SetTotalItemCount(0);
                }
            }
            else
            {
                ShowNoDataMessage(false);
                if (infiniteScrollView != null)
                {
                    infiniteScrollView.SetTotalItemCount(missingFiles.Count);
                    infiniteScrollView.ScrollToTop();
                }
            }

            Debug.Log($"MissingFileCheckUI: Set {missingFiles.Count} missing files");
        }

        private void OnUpdateScrollItem(int index, InfiniteScrollView.IScrollViewItem item)
        {
            if (index >= 0 && index < missingFiles.Count)
            {
                InfiniteScrollView.MissingFileListItem listItem = item as InfiniteScrollView.MissingFileListItem;
                if (listItem != null)
                {
                    listItem.UpdateContent(index);
                    AudioFileData fileData = missingFiles[index];
                    listItem.SetData(index + 1, fileData.fileName);
                }
            }
        }

        private void ShowNoDataMessage(bool show)
        {
            noDataText.gameObject.SetActive(show);

            if (show && missingFiles.Count == 0)
            {
                noDataText.SetText("모든 데이터가 존재합니다.");
            }
        }
    }
}
