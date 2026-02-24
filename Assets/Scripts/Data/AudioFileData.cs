using System;

namespace EngSFXCheckList.Data
{
    [Serializable]
    public class AudioFileData
    {
        public int no;
        public string type;
        public string eng;
        public string fileName;
        public string filePath;
        public bool existsInBlob;
        
        public AudioFileData(string fileName, string type="", string filePath = "", bool existsInBlob = false)
        {
            this.no = 0;
            this.type = type;
            this.fileName = fileName;
            this.filePath = filePath;
            this.existsInBlob = existsInBlob;
        }
    }
}
