using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultPanelUI : MonoBehaviour
{
    [Header("결과 텍스트")]
    [SerializeField] TextMeshProUGUI txtResult;
    [SerializeField] TextMeshProUGUI txtBlobStroagePath;

    [Header("오디오 재생 버튼")]
    [SerializeField] Button btnPlaySFX;

    string[] strResults = new string[2]
    {
        "<color=#00FFFF>{0}.mp3 파일이 존재 합니다.</color>",
        "<color=#FF0000>{0}.mp3 파일이 존재하지 않습니다. 확인 부탁드립니다.</color>"
    };

    private string currentAudioUrl = string.Empty;

    public void Init()
    {
        txtResult.SetText("");
        txtBlobStroagePath.SetText("");
        
        if (btnPlaySFX != null)
        {
            btnPlaySFX.gameObject.SetActive(false);
        }
    }

    public void SetResultUI(bool bResult, string fileName, string blobStroagePath)
    {
        string formatted = string.Format(strResults[bResult ? 0 : 1], fileName);
        txtResult.SetText(formatted);

        txtBlobStroagePath.gameObject.SetActive(bResult);
        txtBlobStroagePath.SetText(blobStroagePath);
        
        currentAudioUrl = blobStroagePath;
        
        if (btnPlaySFX != null)
        {
            btnPlaySFX.gameObject.SetActive(bResult);
        }
    }

    public void OnClick_PlaySFX()
    {
        if (!string.IsNullOrEmpty(currentAudioUrl) && CheckListToolManager.Instance != null)
        {
            CheckListToolManager.Instance.Audio.PlayAudioFromUrl(currentAudioUrl);
        }
    }
}
