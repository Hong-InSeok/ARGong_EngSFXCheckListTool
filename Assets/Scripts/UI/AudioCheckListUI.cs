using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
                if (string.Equals(fileData.type, "sentence") && !string.IsNullOrEmpty(fileData.eng))
                {
                    // string cleanFileName = fileData.fileName.Replace("\"", "").Trim();
                    string[] words = GetNameListOfSentencePartial(fileData.eng).ToArray();
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
            AudioFileData fileData = new AudioFileData(word, "segment");
            fileData.eng = word;
            fileData.filePath = word;
            OnAudioFileItemClicked(fileData, true);
        }
        
        private void OnAudioFileItemClicked(AudioFileData fileData, bool isSegmentedMode = false)
        {
            if (CheckListToolManager.Instance != null)
            {
                CheckListToolManager.Instance.CheckAndPlayAudio(fileData, isSegmentedMode);
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

        #region Partial Audio
        
        private static string[] units = { "zero", "one", "two", "three",  
            "four", "five", "six", "seven", "eight", "nine", "ten", "eleven",  
            "twelve", "thirteen", "fourteen", "fifteen", "sixteen",  
            "seventeen", "eighteen", "nineteen" };
        private static string[] ones = {"first", "second", "third", "fourth", "fifth", 
            "sixth", "seventh", "eighth", "ninth", "tenth", 
            "eleventh", "twelfth", "thirteenth", "fourteenth", "fifteenth", 
            "sixteenth", "seventeenth", "eighteenth", "nineteenth"};
        private static string[] tens = { "", "", "twenty", "thirty", "forty",  
            "fifty", "sixty", "seventy", "eighty", "ninety" };
        
        private static string vowels = "aeiou";
        private static string semiVowels = "wy";
        private static string consonants = "bcdfghjklmnpqrstvxz";

        public static string ChangeExceptionPartial(string partial, bool isUsingGameZone = false)
        {
            if (partial == "a")
            {
                return "a_1571";
            }
            if (partial == "read_past")
            {
                return "red";
            }
            if (partial == "jeju-do")
            {
                return "jejudo";
            }

            if (partial == "p.e" || partial == "p.e.")
            {
                return "pe";
            }

            if (partial == "u.k" || partial == "u.k.")
            {
                return "uk";
            }

            if (partial == "u.s" || partial == "u.s.")
            {
                return "u_s";
            }
            
            if (isUsingGameZone) return partial;
            
            if (IsTimeString(partial))
            {
                return GetTimeString(partial);
            }

            if (CheckIsNumber(partial.Last()))
            {
                return GetNumberWord(int.Parse(partial, NumberStyles.AllowThousands));
            }

            return partial;
        }
        
        public static bool IsTimeString(string word)
        {
            return word.Length > 3 && word.Contains(":");
        }
        
        public static string GetTimeString(string word, bool isForPron = false)
        {
            string[] numbers = word.Split(':');
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = numbers[i].Replace(".", "").Replace("?", "").Replace("!", "").Replace(",", "");
            }
            string hour = GetNumberWord(int.Parse(numbers[0]));
            string minute = int.Parse(numbers[1]) > 0 ? (isForPron ? "_" : " ") + GetNumberWord(int.Parse(numbers[1])) : "";
            // string result = isForPron? $"{hour} {GetNumberWord(int.Parse(numbers[1]))}" : $"{GetNumberWord(int.Parse(numbers[0]))}_{GetNumberWord(int.Parse(numbers[1]))}";
            string result = hour + minute;
            return string.IsNullOrWhiteSpace(result) ? "" : result;
        }
        
        public static string GetNumberWord(int i)
        {
            if (i < 20)  
            {  
                return units[i];
            }
            if (i < 100)  
            {  
                return tens[i / 10] + ((i % 10 > 0) ? $"_{GetNumberWord(i % 10)}" : "");  
            }  
            if (i < 1000)  
            {  
                return units[i / 100] + "_hundred" + (i % 100 > 0 ? $"_{GetNumberWord(i % 100)}" : "");
                // result = units[i / 100] + "_hundred" + ((i % 100 > 0) ? $"_{GetNumberWord(i & 100)}" : "");
            }  
            if (i < 1000000)  
            {           
                return GetNumberWord(i / 1000) + "_thousand" + (i % 1000 > 0 ? $"_{GetNumberWord(i % 1000)}" : "");
            }

            // result = forPron ? result.Replace('_', ' ') : result;
            
            return "";
        }
        
        public static bool CheckIsNumber(char character)
        {
            string numbers = "0123456789";
            return numbers.Contains(character.ToString());
        }

        public static bool CheckIsNumber(string character)
        {
            return character.Length == 1 && CheckIsNumber(character[0]);
        }
        
        public static bool CheckIsAlphabet(char character)
        {
            string alphabets = vowels + semiVowels + consonants;
            return alphabets.Contains(character.ToString().ToLower());
        }

        public static bool CheckIsAlphabet(string character)
        {
            return character.Length == 1 && CheckIsAlphabet(character.ToLower()[0]);
        }
        
        public static List<string> GetNameListOfSentencePartial(string sentence)
        {
            // sentence = sentence.Trim() == "I read many books." ? "I read_past many books." : sentence.Trim();
            // sentence = sentence.Trim() == "My favorite song is \"Lemon Tree.\"" ? "My favorite song is Lemon Tree." : sentence.Trim();

            if (sentence == "My favorite song is \"Lemon Tree.\"")
            {
                Debug.Log("Debugging sentence: " + sentence);
            }
            
            string[] partialSentence = sentence.ToLower().Split(' ');
            var nullPartialList = new List<string>();

            for (int i = 0; i < partialSentence.Length; i++)
            {
                partialSentence[i] = GetSentencePartial(sentence, partialSentence[i]);
                if (!CheckIsAlphabet(partialSentence[i].Last()) && !CheckIsNumber(partialSentence[i].Last()))
                {
                    partialSentence[i] = partialSentence[i].Substring(0, partialSentence[i].Length - 1);
                }
            }

            for (int i = 0; i < partialSentence.Length; i++)
            {
                nullPartialList.Add(Regex.Replace(ChangeExceptionPartial(partialSentence[i]), @"[^a-zA-Z0-9_]", "_"));
            }

            return nullPartialList;
        }
        
        public static string GetSentencePartial(string sentence, string partial)
        {
            string key = partial.Trim().ToLower();
            if (!CheckIsAlphabet(key.Last()) && !CheckIsNumber(key.Last()))
            {
                key = key.Substring(0, key.Length - 1);
            }

            if ((sentence.Trim() == "I read many books." || sentence.Trim() == "I read many books about space.") && key == "read")
            {
                key = "read_past";
            }

            if (sentence.Trim() == "My favorite song is \"Lemon Tree.\"" && key == "\"lemon")
            {
                key = "lemon";
            }
            
            if (sentence.Trim() == "My favorite song is \"Lemon Tree.\"" && key == "tree.\"")
            {
                key = "tree";
            }

            if (sentence.Trim() == "I saw the musical 'The Lion King.'" && key == "'the")
            {
                key = "the";
            }

            if (sentence.Trim() == "I saw the musical 'The Lion King.'" && key == "king.'")
            {
                key = "king";
            }

            if (sentence.Trim() == "I read the book 'Harry Potter.'" && key == "'harry")
            {
                key = "harry";
            }

            if (sentence.Trim() == "I read the book 'Harry Potter.'" && key == "potter.'")
            {
                key = "potter";
            }

            return key;
        }

        #endregion
    }
}
