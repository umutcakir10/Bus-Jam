using System;
using System.Collections.Generic;
using UnityEngine;

namespace BusJam.LevelSystem
{
 
    public static class LevelRepository
    {
        
        private const string BuiltInFolder = "Levels/";

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
    }
}