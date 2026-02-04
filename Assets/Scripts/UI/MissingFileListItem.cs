using UnityEngine;
using TMPro;

namespace EngSFXCheckList.UI.InfiniteScrollView
{
    public class MissingFileListItem : MonoBehaviour, IScrollViewItem
    {
        [SerializeField] private TextMeshProUGUI txtIndex;
        [SerializeField] private TextMeshProUGUI txtFileName;

        private int currentIndex = -1;
        private string currentFileName = string.Empty;

        public void UpdateContent(int index)
        {
            currentIndex = index;
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SetData(int no, string fileName)
        {
            if (txtIndex != null)
            {
                txtIndex.SetText($"{no}");
            }

            if (txtFileName != null)
            {
                txtFileName.SetText(fileName);
            }

            currentFileName = fileName;
        }
    }
}
