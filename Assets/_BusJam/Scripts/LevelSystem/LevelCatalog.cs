using System;
using System.Collections.Generic;
using UnityEngine;

namespace BusJam.LevelSystem
{
    // Oyunla gelen level'ların listesi dosya adına göre sıralı
    // level_001, level_002 diye adlandırdığımız için alfabetik sıra oynanış sırası oluyor
    public static class LevelCatalog
    {
        private const string Folder = "Levels";

        private static string[] _ids;

        public static IReadOnlyList<string> Ids
        {
            get
            {
                if (_ids == null)
                    Refresh();
                return _ids;
            }
        }

        public static int Count => Ids.Count;

        // Level Editor yeni level kaydedince liste yeniden okunacak
        public static void Refresh()
        {
            TextAsset[] assets = Resources.LoadAll<TextAsset>(Folder);
            var ids = new string[assets.Length];
            for (int i = 0; i < assets.Length; i++)
                ids[i] = assets[i].name;

            Array.Sort(ids, StringComparer.Ordinal);
            _ids = ids;
        }
    }
}