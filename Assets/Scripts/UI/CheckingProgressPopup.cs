using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EngSFXCheckList.UI
{
    public class CheckingProgressPopup : MonoBehaviour
    {
        [SerializeField] private Image checkProgressbar;
        [SerializeField] private TextMeshProUGUI txtPercent;

        public void Show()
        {
            gameObject.SetActive(true);
            UpdateProgress(0f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void UpdateProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (checkProgressbar != null)
            {
                checkProgressbar.fillAmount = progress;
            }

            if (txtPercent != null)
            {
                txtPercent.SetText($"{Mathf.RoundToInt(progress * 100f)}%");
            }
        }
    }
}
