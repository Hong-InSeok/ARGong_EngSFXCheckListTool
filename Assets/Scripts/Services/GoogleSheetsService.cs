using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using EngSFXCheckList.Data;

namespace EngSFXCheckList.Services
{
    public class GoogleSheetsService : MonoBehaviour
    {
        // 구글 시트 링크
        // https://docs.google.com/spreadsheets/d/1Qh1FvPbeIMb2NHPIB6XNAPIASOmTW7OYfRd89hfyo3A/edit?gid=0#gid=0

        // private const string SHEET_ID = "1Qh1FvPbeIMb2NHPIB6XNAPIASOmTW7OYfRd89hfyo3A";
        private const string SHEET_ID = "1gTkHjA7ZNsvd5kjCOA5W8ukb3HcNErzOGzTz-XZBL00";
        private const string GID = "0";
        private const string NO_COLUMN = "No";
        private const string FILENAME_COLUMN = "FileName";
        private const string CATEGORY_COLUMN = "category";
        private const string ENG_COLUMN = "Eng";
        
        public event Action<List<AudioFileData>> OnDataLoaded;
        public event Action<string> OnError;
        
        private List<AudioFileData> allAudioFiles = new List<AudioFileData>();
        
        public void LoadGoogleSheetData()
        {
            StartCoroutine(LoadDataCoroutine());
        }
        
        private IEnumerator LoadDataCoroutine()
        {
            string url = $"https://docs.google.com/spreadsheets/d/{SHEET_ID}/export?format=csv&gid={GID}";
            Debug.Log($"URL : {url}");
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    ParseCSVData(request.downloadHandler.text);
                    OnDataLoaded?.Invoke(allAudioFiles);
                }
                else
                {
                    OnError?.Invoke($"Error loading Google Sheet: {request.error}");
                    Debug.LogError($"Error loading Google Sheet: {request.error}");
                }
            }
        }

        private void ParseCSVData(string csvData)
        {
            allAudioFiles.Clear();

            // 줄바꿈으로 행 분리
            string[] lines = csvData.Split('\n');
            if (lines.Length == 0) return;

            // 헤더 파싱
            string[] headers = lines[0].Split(',');
            int noColumnIndex = -1;
            int audioColumnIndex = -1;
            int filePathIndex = 11;
            int categoryIndex = -1;
            int engColumnIndex = -1;

            for (int i = 0; i < headers.Length; i++)
            {
                // 헤더에서도 공백과 따옴표 제거
                string header = headers[i].Trim().Trim('"');
                if (header.Equals(NO_COLUMN, StringComparison.OrdinalIgnoreCase))
                {
                    noColumnIndex = i;
                }
                else if (header.Equals(FILENAME_COLUMN, StringComparison.OrdinalIgnoreCase))
                {
                    audioColumnIndex = i;
                }
                else if (header.Equals(CATEGORY_COLUMN, StringComparison.OrdinalIgnoreCase))
                {
                    categoryIndex = i;
                }
                else if (header.Equals(ENG_COLUMN, StringComparison.OrdinalIgnoreCase))
                {
                    engColumnIndex = i;
                }
            }

            if (audioColumnIndex == -1)
            {
                OnError?.Invoke($"'{FILENAME_COLUMN}' 컬럼을 찾을 수 없습니다.");
                Debug.LogError($"'{FILENAME_COLUMN}' 컬럼을 찾을 수 없습니다.");
                return;
            }

            // 데이터 파싱 (1번 인덱스부터 끝까지)
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] columns = lines[i].Split(',');
                if (audioColumnIndex < columns.Length)
                {
                    // [중요] .Trim()으로 공백을 먼저 지우고, .Trim('"')으로 따옴표를 지웁니다.
                    string audioFileName = columns[audioColumnIndex].Trim().Trim('"');

                    if (!string.IsNullOrWhiteSpace(audioFileName))
                    {
                        // 따옴표가 제거된 깨끗한 파일명으로 생성
                        AudioFileData fileData = new AudioFileData(audioFileName);

                        if (noColumnIndex != -1 && noColumnIndex < columns.Length)
                        {
                            // NO 컬럼 숫자 데이터도 동일하게 따옴표 제거 후 파싱
                            string noStr = columns[noColumnIndex].Trim().Trim('"');
                            if (int.TryParse(noStr, out int no))
                            {
                                fileData.no = no;
                            }
                        }
                        fileData.eng = columns[engColumnIndex].Trim().Trim('"');
                        fileData.type = columns[categoryIndex];
                        fileData.filePath = columns[filePathIndex];

                        allAudioFiles.Add(fileData);
                    }
                }
            }
        }

        public List<AudioFileData> GetAllAudioFiles()
        {
            return new List<AudioFileData>(allAudioFiles);
        }
        
        public List<AudioFileData> SearchAudioFiles(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return GetAllAudioFiles();
            }
            
            List<AudioFileData> results = new List<AudioFileData>();
            string lowerSearchTerm = searchTerm.ToLower();
            
            foreach (var audioFile in allAudioFiles)
            {
                if (audioFile.fileName.ToLower().Contains(lowerSearchTerm))
                {
                    results.Add(audioFile);
                }
            }
            
            return results;
        }
    }
}
