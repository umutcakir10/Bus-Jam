using BusJam.Save;
using NUnit.Framework;
using UnityEngine;

namespace BusJam.Tests.EditMode
{
    public class SaveSystemTests
    {
        [Test]
        public void Parse_ValidJson_ReadsLevel()
        {
            string json = JsonUtility.ToJson(new SaveData { currentLevelId = "level_002" });

            SaveData data = SaveSystem.Parse(json);

            Assert.AreEqual("level_002", data.currentLevelId);
        }

        [Test]
        public void Parse_Empty_ReturnsFreshSave()
        {
            Assert.IsNull(SaveSystem.Parse("").currentLevelId);
            Assert.IsNull(SaveSystem.Parse(null).currentLevelId);
        }

        [Test]
        public void Parse_Corrupt_ReturnsFreshSave()
        {
            SaveData data = SaveSystem.Parse("{ this is not json");

            Assert.IsNull(data.currentLevelId);
        }

        [Test]
        public void Parse_UnknownVersion_ReturnsFreshSave()
        {
            SaveData data = SaveSystem.Parse("{\"version\":99,\"currentLevelId\":\"level_005\"}");

            Assert.IsNull(data.currentLevelId);
        }
    }
}