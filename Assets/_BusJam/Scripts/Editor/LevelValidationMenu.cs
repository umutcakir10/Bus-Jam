using System;
using System.Collections.Generic;
using BusJam.LevelSystem;
using UnityEditor;
using UnityEngine;

namespace BusJam.EditorTools
{
   
    public static class LevelValidationMenu
    {
        [MenuItem("Bus Jam/Validate All Levels")]
        private static void ValidateAllLevels()
        {
            TextAsset[] assets = Resources.LoadAll<TextAsset>("Levels");
            if (assets.Length == 0)
            {
                Debug.LogWarning("No level files found in Resources/Levels.");
                return;
            }

            
            Array.Sort(assets, (a, b) => string.CompareOrdinal(a.name, b.name));

            var errors = new List<string>();
            int failed = 0;

            foreach (TextAsset asset in assets)
            {
                if (!LevelRepository.TryParse(asset.text, out LevelData level, errors))
                {
                    failed++;
                    Debug.LogError($"✘ {asset.name}\n - {string.Join("\n - ", errors)}", asset);
                    continue;
                }

           
                if (level.id != asset.name)
                {
                    failed++;
                    Debug.LogError($"✘ {asset.name}: id is '{level.id}', expected '{asset.name}'.", asset);
                    continue;
                }

                Debug.Log($"✔ {asset.name}: {level.displayName}, {level.Width}x{level.Height}, " +
                          $"{level.buses.Length} buses, {level.timeLimitSeconds}s", asset);
            }

            if (failed == 0)
                Debug.Log($"All {assets.Length} level(s) are valid.");
            else
                Debug.LogError($"{failed} of {assets.Length} level(s) have problems.");
        }
    }
}