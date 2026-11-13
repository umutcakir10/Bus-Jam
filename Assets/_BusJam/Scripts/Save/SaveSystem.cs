using System;
using System.IO;
using UnityEngine;

namespace BusJam.Save
{
    // Kaydı okuyup yazan tek yer
    // Dosya cihazın yazılabilir klasöründe duruyor, oyun silinene kadar kalıyor
    public static class SaveSystem
    {
        private const string FileName = "save.json";

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static SaveData Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new SaveData();

                return Parse(File.ReadAllText(FilePath));
            }
            catch (Exception e)
            {
                // Kayıt okunamazsa oyun çökmesin ve en baştan başlasın
                Debug.LogWarning($"Could not read save file, starting fresh: {e.Message}");
                return new SaveData();
            }
        }

        public static void Save(SaveData data)
        {
            try
            {
                
                string temp = FilePath + ".tmp";
                File.WriteAllText(temp, JsonUtility.ToJson(data));

                if (File.Exists(FilePath))
                    File.Replace(temp, FilePath, null);
                else
                    File.Move(temp, FilePath);
            }
            catch (Exception e)
            {
                // Kaydedememek oyunu durdurmamalı o yüzden de sadece uyarı yazıyoruz
                Debug.LogWarning($"Could not write save file: {e.Message}");
            }
        }

        
        public static SaveData Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new SaveData();

            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null || data.version != SaveData.CurrentVersion)
                    return new SaveData();
                return data;
            }
            catch (ArgumentException)
            {
                
                return new SaveData();
            }
        }
    }
}