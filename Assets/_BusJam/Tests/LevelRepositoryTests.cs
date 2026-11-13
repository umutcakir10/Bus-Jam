using System.Collections.Generic;
using BusJam.LevelSystem;
using NUnit.Framework;
using UnityEngine;

namespace BusJam.Tests.EditMode
{
    public class LevelRepositoryTests
    {
        [Test]
        public void JsonRoundTrip_KeepsEveryField()
        {
            var original = new LevelData
            {
                id = "level_test",
                displayName = "Test",
                timeLimitSeconds = 45,
                busCapacity = 3,
                waitingSlotCount = 4,
                rows = new[] { "RRR", "#X." },
                buses = "R",
            };
            var errors = new List<string>();

            bool ok = LevelRepository.TryParse(JsonUtility.ToJson(original, true), out LevelData copy, errors);

            Assert.IsTrue(ok, string.Join("\n", errors));
            Assert.AreEqual(original.id, copy.id);
            Assert.AreEqual(45, copy.timeLimitSeconds);
            Assert.AreEqual(4, copy.waitingSlotCount);
            CollectionAssert.AreEqual(original.rows, copy.rows);
            Assert.AreEqual("R", copy.buses);
        }

        [Test]
        public void TryParse_InvalidLevel_ReturnsErrors()
        {
            var errors = new List<string>();

            bool ok = LevelRepository.TryParse("{\"version\":1,\"id\":\"x\",\"rows\":[\"R\"],\"buses\":\"\"}", out LevelData level, errors);

            Assert.IsFalse(ok);
            Assert.IsNull(level);
            Assert.IsNotEmpty(errors);
        }
    }
}