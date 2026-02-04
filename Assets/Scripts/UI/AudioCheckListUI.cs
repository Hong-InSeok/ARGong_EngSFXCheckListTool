using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EngSFXCheckList.Data;
using EngSFXCheckList.Services;

namespace EngSFXCheckList.UI
{
    public class AudioCheckListUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private InfiniteScrollView.InfiniteScrollView infiniteScrollView;
        [SerializeField] private TMP_InputField searchInputField;
        [SerializeField] private Button searchButton;
        [SerializeField] private GameObject searchResultButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button loadDataButton;
        [SerializeField] private Button totalCompareFileButton;
        [SerializeField] private Button searchEnableButton;
        [SerializeField] private TextMeshProUGUI searchEnableButtonText;
        [SerializeField] private TMP_Dropdown audioTypeDropdown;
        
        private List<AudioFileData> currentDisplayList = new List<AudioFileData>();
        private List<string> segmentedWordsList = new List<string>();
        private bool isSearchMode = false;
        private bool isSegmentedAudioMode = false;

        public void Init()
        {
            InitializeUI();
            SetupEventListeners();
        }

        private void InitializeUI()
        {
            infiniteScrollView.Initialize(OnUpdateScrollItem);
            UpdateStatusText("데이터를 불러오려면 '데이터 로드' 버튼을 클릭하세요.");
            UpdateButtonStates();
        }
        
        private void SetupEventListeners()
        {
            if (loadDataButton != null)
            {
                loadDataButton.onClick.AddListener(OnLoadDataClicked);
            }
            
            if (searchButton != null)
            {
                searchButton.onClick.AddListener(OnSearchClicked);
            }
            
            if (totalCompareFileButton != null)
            {
                totalCompareFileButton.onClick.AddListener(OnTotalCompareFileClicked);
            }
            
            if (searchEnableButton != null)
            {
                searchEnableButton.onClick.AddListener(OnSearchEnableClicked);
            }
            
            if (audioTypeDropdown != null)
            {
                audioTypeDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }
            
            if (searchInputField != null)
            {
                searchInputField.onSubmit.AddListener((text) => OnSearchClicked());
                searchInputField.onValueChanged.AddListener(OnSearchInputValueChanged);
            }
            
            if (CheckListToolManager.Instance.GoogleSheets != null)
            {
                CheckListToolManager.Instance.GoogleSheets.OnDataLoaded += OnDataLoaded;
                CheckListToolManager.Instance.GoogleSheets.OnError += OnDataLoadError;
            }
        }
        
        private void OnLoadDataClicked()
        {
            if (CheckListToolManager.Instance.GoogleSheets != null)
            {
                UpdateStatusText("데이터를 불러오는 중...");
                CheckListToolManager.Instance.GoogleSheets.LoadGoogleSheetData();
            }
        }
        
        private void OnDataLoaded(List<AudioFileData> audioFiles)
        {
            UpdateStatusText($"총 {audioFiles.Count}개의 음원 파일을 불러왔습니다.");
            OnShowAllClicked();
        }
        
        private void OnDataLoadError(string errorMessage)
        {
            UpdateStatusText($"오류: {errorMessage}");
        }
        
        private void OnShowAllClicked()
        {
            isSearchMode = false;
            
            if (CheckListToolManager.Instance.GoogleSheets != null)
            {
                List<AudioFileData> allFiles = CheckListToolManager.Instance.GoogleSheets.GetAllAudioFiles();
                
                if (isSegmentedAudioMode)
                {
                    BuildSegmentedWordsList(allFiles);
                    UpdateStatusText($"전체 {segmentedWordsList.Count}개 단어 표시");
                }
                else
                {
                    currentDisplayList = allFiles;
                    UpdateStatusText($"전체 {currentDisplayList.Count}개 항목 표시");
                }
                
                UpdateScrollView();
                UpdateButtonStates();
            }
        }
        
        private void OnSearchClicked()
        {
            isSearchMode = true;
            
            if (CheckListToolManager.Instance.GoogleSheets != null && searchInputField != null)
            {
                string searchTerm = searchInputField.text;
                
                if (isSegmentedAudioMode)
                {
                    List<AudioFileData> allFiles = CheckListToolManager.Instance.GoogleSheets.GetAllAudioFiles();
                    BuildSegmentedWordsList(allFiles);
                    
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        string lowerSearchTerm = searchTerm.ToLower();
                        segmentedWordsList = segmentedWordsList.FindAll(word => 
                            word.ToLower().StartsWith(lowerSearchTerm));
                        UpdateStatusText($"'{searchTerm}' 검색 결과: {segmentedWordsList.Count}개 단어");
                    }
                    else
                    {
                        UpdateStatusText($"전체 {segmentedWordsList.Count}개 단어 표시");
                    }
                }
                else
                {
                    currentDisplayList = CheckListToolManager.Instance.GoogleSheets.SearchAudioFiles(searchTerm);
                    
                    if (string.IsNullOrWhiteSpace(searchTerm))
                    {
                        UpdateStatusText($"전체 {currentDisplayList.Count}개 항목 표시");
                    }
                    else
                    {
                        UpdateStatusText($"'{searchTerm}' 검색 결과: {currentDisplayList.Count}개 항목");
                    }
                }
                
                UpdateScrollView();
                UpdateButtonStates();
            }
        }
        
        private void OnTotalCompareFileClicked()
        {
            if (isSegmentedAudioMode)
            {
                if (segmentedWordsList.Count == 0)
                {
                    UpdateStatusText("데이터를 먼저 로드해주세요.");
                    return;
                }
                
                UpdateStatusText("분절 음원 파일 존재 여부를 확인하는 중입니다. 잠시만 기다려주세요...");
                CheckListToolManager.Instance.CheckMissingSegmentedFiles(segmentedWordsList);
            }
            else
            {
                if (CheckListToolManager.Instance.GoogleSheets.GetAllAudioFiles().Count == 0)
                {
                    UpdateStatusText("데이터를 먼저 로드해주세요.");
                    return;
                }
                
                UpdateStatusText("파일 존재 여부를 확인하는 중입니다. 잠시만 기다려주세요...");
                CheckListToolManager.Instance.CheckMissingFiles();
            }
        }
        
        private void OnSearchEnableClicked()
        {
            isSearchMode = !isSearchMode;
            
            if (searchInputField != null)
            {
                searchInputField.interactable = isSearchMode;
            }
            
            if (searchResultButton != null)
            {
                searchResultButton.SetActive(isSearchMode);
            }
            
            if (searchEnableButtonText != null)
            {
                searchEnableButtonText.text = isSearchMode ? "이름 검색 비활성화" : "이름 검색 활성화";
            }
            
            if (!isSearchMode)
            {
                OnShowAllClicked();
            }
            
            UpdateButtonStates();
            UpdateStatusText(isSearchMode ? "검색 모드 활성화" : "검색 모드 비활성화");
        }
        
        private void OnSearchInputValueChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                OnShowAllClicked();
            }
        }
        
        private void OnDropdownValueChanged(int value)
        {
            isSegmentedAudioMode = (value == 1);
            
            if (CheckListToolManager.Instance.GoogleSheets != null)
            {
                List<AudioFileData> allFiles = CheckListToolManager.Instance.GoogleSheets.GetAllAudioFiles();
                
                if (isSegmentedAudioMode)
                {
                    BuildSegmentedWordsList(allFiles);
                    UpdateStatusText($"분절 음원: {segmentedWordsList.Count}개 단어 표시");
                }
                else
                {
                    currentDisplayList = allFiles;
                    UpdateStatusText($"전체 음원: {currentDisplayList.Count}개 항목 표시");
                }
                
                UpdateScrollView();
            }
        }
        
        private void BuildSegmentedWordsList(List<AudioFileData> allFiles)
        {
            HashSet<string> uniqueWords = new HashSet<string>();
            
            foreach (AudioFileData fileData in allFiles)
            {
                if (!string.IsNullOrEmpty(fileData.fileName))
                {
                    string cleanFileName = fileData.fileName.Replace("\"", "").Trim();
                    string[] words = cleanFileName.Split('_');
                    foreach (string word in words)
                    {
                        string trimmedWord = word.Trim();
                        if (!string.IsNullOrEmpty(trimmedWord))
                        {
                            uniqueWords.Add(trimmedWord);
                        }
                    }
                }
            }
            
            segmentedWordsList = new List<string>(uniqueWords);
            segmentedWordsList.Sort();
        }
        
        private void UpdateScrollView()
        {
            int totalCount = isSegmentedAudioMode ? segmentedWordsList.Count : currentDisplayList.Count;
            infiniteScrollView.SetTotalItemCount(totalCount);
            infiniteScrollView.ScrollToTop();
        }
        
        private void OnUpdateScrollItem(int index, InfiniteScrollView.IScrollViewItem item)
        {
            AudioFileListItem listItem = item as AudioFileListItem;
            if (listItem == null) return;
            
            listItem.UpdateContent(index);
            
            if (isSegmentedAudioMode)
            {
                if (index >= 0 && index < segmentedWordsList.Count)
                {
                    string word = segmentedWordsList[index];
                    listItem.SetData(index + 1, word);
                    listItem.SetClickCallback(() => OnSegmentedWordClicked(word));
                }
            }
            else
            {
                if (index >= 0 && index < currentDisplayList.Count)
                {
                    AudioFileData fileData = currentDisplayList[index];
                    listItem.SetData(fileData.no, fileData.fileName);
                    listItem.SetClickCallback(() => OnAudioFileItemClicked(fileData));
                }
            }
        }
        
        private void OnSegmentedWordClicked(string word)
        {
            AudioFileData fileData = new AudioFileData(word);
            OnAudioFileItemClicked(fileData);
        }
        
        private void OnAudioFileItemClicked(AudioFileData fileData)
        {
            if (CheckListToolManager.Instance != null)
            {
                CheckListToolManager.Instance.CheckAndPlayAudio(fileData);
            }
        }
        
        public void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
        
        private void UpdateButtonStates()
        {
            if (searchButton != null)
            {
                bool hasData = isSegmentedAudioMode ? segmentedWordsList.Count > 0 : currentDisplayList.Count > 0;
                searchButton.interactable = isSearchMode || hasData;
            }
        }
        
        private void OnDestroy()
        {
            if (CheckListToolManager.Instance.GoogleSheets != null)
            {
                CheckListToolManager.Instance.GoogleSheets.OnDataLoaded -= OnDataLoaded;
                CheckListToolManager.Instance.GoogleSheets.OnError -= OnDataLoadError;
            }
        }
    }
}
