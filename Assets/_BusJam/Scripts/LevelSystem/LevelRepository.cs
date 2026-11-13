using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BusJam.LevelSystem
{
    // Level dosyalarını okuyup yazan tek yer
    
    public static class LevelRepository
    {
        private const string BuiltInFolder = "Levels/";
        private const string EditorFolder = "Assets/_BusJam/Resources/Levels/";

        public static string UserFolder => Path.Combine(Application.persistentDataPath, "levels");

        // Önce kullanıcının kaydettiği dosyaya bakıyoruz eğer yoksa da oyunla gelen dosyayı kullanıyoruz
        public static bool TryLoad(string id, out LevelData level, List<string> errors)
        {
            errors.Clear();
            level = null;

            string userPath = UserPath(id);
            if (!File.Exists(userPath))
                return TryLoadBuiltIn(id, out level, errors);

            string json;
            try
            {
                json = File.ReadAllText(userPath);
            }
            catch (Exception e)
            {
                errors.Add($"Could not read '{userPath}': {e.Message}");
                return false;
            }
            return TryParse(json, out level, errors);
        }

        public static bool TryLoadBuiltIn(string id, out LevelData level, List<string> errors)
        {
            errors.Clear();
            level = null;

            var asset = Resources.Load<TextAsset>(BuiltInFolder + id);
            if (asset == null)
            {
                errors.Add($"Level file '{BuiltInFolder}{id}' not found in Resources.");
                return false;
            }
            return TryParse(asset.text, out level, errors);
        }

        
        public static bool TryParse(string json, out LevelData level, List<string> errors)
        {
            level = null;

            LevelData parsed;
            try
            {
                parsed = JsonUtility.FromJson<LevelData>(json);
            }
            catch (ArgumentException e)
            {
                errors.Clear();
                errors.Add($"Invalid JSON: {e.Message}");
                return false;
            }

            if (!LevelValidator.Validate(parsed, errors))
                return false;

            level = parsed;
            return true;
        }

        
        public static bool TrySave(LevelData level, List<string> errors)
        {
            if (!LevelValidator.Validate(level, errors))
                return false;

            // Id dosya adı olarak kullanılıyor klasör dışına çıkmasın diye sadece harf, rakam ve alt çizgi
            foreach (char c in level.id)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    errors.Add($"Level id '{level.id}' may only contain letters, digits and '_'.");
                    return false;
                }
            }

            string json = JsonUtility.ToJson(level, true);
            try
            {
#if UNITY_EDITOR
                
                string assetPath = EditorFolder + level.id + ".json";
                File.WriteAllText(assetPath, json);
                UnityEditor.AssetDatabase.ImportAsset(assetPath);
#else
                // Build'de Resources salt okunur, cihazın yazılabilir klasörüne yazıyoruz
                Directory.CreateDirectory(UserFolder);
                File.WriteAllText(UserPath(level.id), json);
#endif
            }
            catch (Exception e)
            {
                errors.Add($"Could not save: {e.Message}");
                return false;
            }

            LevelCatalog.Refresh();
            return true;
        }

        public static string[] GetUserIds()
        {
            if (!Directory.Exists(UserFolder))
                return Array.Empty<string>();

            string[] files = Directory.GetFiles(UserFolder, "*.json");
            for (int i = 0; i < files.Length; i++)
                files[i] = Path.GetFileNameWithoutExtension(files[i]);
            return files;
        }

        private static string UserPath(string id) => Path.Combine(UserFolder, id + ".json");
    }
}