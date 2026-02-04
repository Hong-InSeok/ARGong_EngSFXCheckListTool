using System;

namespace EngSFXCheckList.Data
{
    [Serializable]
    public class AudioFileData
    {
        public int no;
        public string fileName;
        public string filePath;
        public bool existsInBlob;
        
        public AudioFileData(string fileName, string filePath = "", bool existsInBlob = false)
        {
            this.no = 0;
            this.fileName = fileName;
            this.filePath = filePath;
            this.existsInBlob = existsInBlob;
        }
    }
}
