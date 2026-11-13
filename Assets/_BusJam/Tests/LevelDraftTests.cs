using BusJam.Core;
using BusJam.LevelSystem;
using NUnit.Framework;
using System.Collections.Generic;

namespace BusJam.Tests.EditMode
{
    public class LevelDraftTests
    {
        [Test]
        public void CreateNew_IsEmptyGrid()
        {
            var draft = LevelDraft.CreateNew("level_009", 3, 2);

            Assert.AreEqual(3, draft.Width);
            Assert.AreEqual(2, draft.Height);
            Assert.AreEqual('.', draft.GetCell(2, 1));
            Assert.AreEqual("Level 9", draft.DisplayName);
        }

        [Test]
        public void Resize_KeepsExistingCellsAndFillsNewOnes()
        {
            var draft = LevelDraft.CreateNew("level_001", 2, 2);
            draft.Paint(1, 1, 'R');

            draft.Resize(3, 3);

            Assert.AreEqual('R', draft.GetCell(1, 1));
            Assert.AreEqual('.', draft.GetCell(2, 2));
        }

        [Test]
        public void Resize_Shrinking_CropsAndClamps()
        {
            var draft = LevelDraft.CreateNew("level_001", 3, 3);

            draft.Resize(0, 99);

            Assert.AreEqual(1, draft.Width);
            Assert.AreEqual(LevelValidator.MaxHeight, draft.Height);
        }

        [Test]
        public void SetBuses_KeepsOnlyColorLetters()
        {
            var draft = LevelDraft.CreateNew("level_001");

            draft.SetBuses("r b, x-g");

            Assert.AreEqual("RBG", draft.Buses);
        }

        [Test]
        public void Counts_PassengersAndBusesPerColor()
        {
            var draft = LevelDraft.CreateNew("level_001", 3, 1);
            draft.Paint(0, 0, 'R');
            draft.Paint(1, 0, 'R');
            draft.SetBuses("RBR");

            Assert.AreEqual(2, draft.CountPassengers(PassengerColor.Red));
            Assert.AreEqual(2, draft.CountBuses(PassengerColor.Red));
            Assert.AreEqual(1, draft.CountBuses(PassengerColor.Blue));
        }

        [Test]
        public void ToLevelData_ValidDraft_PassesValidator()
        {
            var draft = LevelDraft.CreateNew("level_050", 3, 1);
            draft.Paint(0, 0, 'R');
            draft.Paint(1, 0, 'R');
            draft.Paint(2, 0, 'R');
            draft.SetBuses("R");

            var errors = new List<string>();
            bool ok = LevelValidator.Validate(draft.ToLevelData(), errors);

            Assert.IsTrue(ok, string.Join("\n", errors));
        }

        [Test]
        public void FromLevel_ThenToLevelData_RoundTrips()
        {
            var original = new LevelData
            {
                id = "level_002",
                rows = new[] { "RB", "#X" },
                buses = "RB",
                timeLimitSeconds = 30,
                waitingSlotCount = 4,
                busCapacity = 3,
            };

            LevelData copy = LevelDraft.FromLevel(original).ToLevelData();

            CollectionAssert.AreEqual(original.rows, copy.rows);
            Assert.AreEqual(30, copy.timeLimitSeconds);
            Assert.AreEqual(4, copy.waitingSlotCount);
        }
    }
}