using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using EngSFXCheckList.Data;
using EngSFXCheckList.Services;
using EngSFXCheckList.UI;

public class CheckListToolManager : MonoBehaviour
{
    public static CheckListToolManager Instance = null;

    [Header("AudioController Component")][Space(10f)]
    [SerializeField] AudioController audioController;

    [Header("AudioCheckListUI Component")][Space(10f)]
    [SerializeField] AudioCheckListUI audioCheckListUI;

    [Header("ResultPanleUI Component")][Space(10f)]
    [SerializeField] ResultPanelUI resultPanelUI;
    
    [Header("MissingFileCheckUI Component")][Space(10f)]
    [SerializeField] MissingFileCheckUI missingFileCheckUI;
    
    [Header("CheckingProgressPopup Component")][Space(10f)]
    [SerializeField] CheckingProgressPopup checkingProgressPopup;

    [Header("GoogleSheetsService Component")][Space(10f)]
    [SerializeField] GoogleSheetsService googleSheetService;

    public AudioController Audio => audioController;
    public GoogleSheetsService GoogleSheets => googleSheetService;
    public ResultPanelUI ResultPanel => resultPanelUI;
    public MissingFileCheckUI MissingFileCheck => missingFileCheckUI;

    // string BLOB_ROOT_PATH = "https://argame3.blob.core.windows.net/learning-resources-22/audio/{0}.mp3";

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        audioController.Init();
        audioCheckListUI.Init();
        resultPanelUI.Init();
        
        if (missingFileCheckUI != null)
        {
            missingFileCheckUI.Init();
        }
        
        if (checkingProgressPopup != null)
        {
            checkingProgressPopup.Hide();
        }
    }

    public string GetSoundPath(string fileName, bool isSegmentedMode = false)
    {
        string basePath = isSegmentedMode ? "https://argame3.blob.core.windows.net/learning-resources-22/audio/" : "";
        string extension = isSegmentedMode ? ".mp3" : "";
        return $"{basePath}{fileName}{extension}";
    }
    
    public void CheckMissingFiles()
    {
        List<AudioFileData> allFiles = googleSheetService.GetAllAudioFiles();
        StartCoroutine(CheckMissingFilesCoroutine(allFiles));
    }
    
    public void CheckMissingSegmentedFiles(List<string> segmentedWords)
    {
        List<AudioFileData> segmentedFileDataList = new List<AudioFileData>();
        foreach (string word in segmentedWords)
        {
            segmentedFileDataList.Add(new AudioFileData(word, "segment"));
        }
        StartCoroutine(CheckMissingFilesCoroutine(segmentedFileDataList, true));
    }
    
    public void CheckAndPlayAudio(AudioFileData fileData, bool isSegmentedMode = false)
    {
        StartCoroutine(CheckAndPlayAudioCoroutine(fileData, isSegmentedMode));
    }
    
    private IEnumerator CheckAndPlayAudioCoroutine(AudioFileData fileData, bool isSegmentedMode = false)
    {
        string blobUrl = GetSoundPath(fileData.filePath, isSegmentedMode);
        Debug.Log($"Checking file: {blobUrl}");
        bool exists = false;
        
        using (UnityWebRequest request = UnityWebRequest.Head(blobUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                exists = true;
            }
        }
        
        if (resultPanelUI != null)
        {
            resultPanelUI.SetResultUI(exists, fileData.fileName, blobUrl);
        }
        
        Debug.Log($"File check: {fileData.fileName} - Exists: {exists}");
    }
    
    private IEnumerator CheckMissingFilesCoroutine(List<AudioFileData> allFiles, bool isSegmentedMode = false)
    {
        if (checkingProgressPopup != null)
        {
            checkingProgressPopup.Show();
        }
        
        List<AudioFileData> missingFiles = new List<AudioFileData>();
        int totalFiles = allFiles.Count;
        int checkedFiles = 0;
        
        foreach (AudioFileData fileData in allFiles)
        {
            string blobUrl = isSegmentedMode ? GetSoundPath(fileData.fileName, true) : GetSoundPath(fileData.filePath);
            bool exists = false;
            
            using (UnityWebRequest request = UnityWebRequest.Get(blobUrl))
            {
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    exists = true;
                }
            }
            
            if (!exists)
            {
                missingFiles.Add(fileData);
            }
            
            checkedFiles++;
            
            if (checkingProgressPopup != null)
            {
                float progress = (float)checkedFiles / totalFiles;
                checkingProgressPopup.UpdateProgress(progress);
            }
        }
        
        checkingProgressPopup.Hide();
        missingFileCheckUI.SetMissingFiles(missingFiles);

        Debug.Log($"Missing files check completed. Total: {allFiles.Count}, Missing: {missingFiles.Count}");
        if (missingFiles.Count > 0)
        {
            audioCheckListUI.UpdateStatusText($"전체 파일 : {allFiles.Count}개, <color=red>누락된 파일 : {missingFiles.Count}개</color>");
        }
        else
        {
            audioCheckListUI.UpdateStatusText("<color=cyan>누락된 파일이 없습니다.</color>");
        }
        
    }
}
