using System.IO;
using Game.Data;
using UnityEngine;

namespace Game.Tool
{
    public class JsonManager 
    {
        #region Singleton

        private static JsonManager instance;

        public static JsonManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new JsonManager();
                return instance;
            }
        }
        
        private JsonManager(){}

        #endregion

        public BubbleGeneratorData bubbleGeneratorData;
        
        public void Load(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open);
            StreamReader sr = new StreamReader(fileStream);
            string content = sr.ReadToEnd();
            // Debug.Log(content);

            bubbleGeneratorData = JsonUtility.FromJson<BubbleGeneratorData>(content);
            
            sr.Close();
            fileStream.Close();
        }
    }
}
