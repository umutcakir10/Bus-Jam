using BusJam.Save;
using UnityEditor;
using UnityEngine;

namespace BusJam.EditorTools
{
    // Kaydı test etmek için editör menüsü
    public static class SaveMenu
    {
        [MenuItem("Bus Jam/Delete Save")]
        private static void DeleteSave()
        {
            SaveSystem.Delete();
            Debug.Log("Save deleted, the game will start from level 1.");
        }

        [MenuItem("Bus Jam/Open Save Folder")]
        private static void OpenSaveFolder()
        {
            EditorUtility.RevealInFinder(SaveSystem.FolderPath);
        }
    }
}