using System;
using System.Collections.Generic;
using UnityEngine;

namespace BusJam.LevelSystem
{
    // Oynanabilir bütün level'ların listesi yani oyunla gelenler ve kullanıcının kaydettikleri
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

        public static void Refresh()
        {
            // SortedSet aynı idyi iki kere eklemiyor ve sıralı tutuyor
            var ids = new SortedSet<string>(StringComparer.Ordinal);
            foreach (TextAsset asset in Resources.LoadAll<TextAsset>(Folder))
                ids.Add(asset.name);
            foreach (string id in LevelRepository.GetUserIds())
                ids.Add(id);

            _ids = new string[ids.Count];
            ids.CopyTo(_ids);
        }

        public static int IndexOf(string id)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Ids[i] == id)
                    return i;
            }
            return -1;
        }

        // Yeni level için boşta olan ilk adı veriyor mesela level_003, level_004 gibi
        public static string NextFreeId()
        {
            for (int n = 1; ; n++)
            {
                string id = $"level_{n:000}";
                if (IndexOf(id) < 0)
                    return id;
            }
        }
    }
}