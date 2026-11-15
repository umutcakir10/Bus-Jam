using System.Collections.Generic;
using BusJam.Core;
using BusJam.Gameplay.Domain;
using BusJam.LevelSystem;
using NUnit.Framework;

namespace BusJam.Tests.EditMode
{
    public sealed class TutorialHintTests
    {
        [Test]
        public void Hint_PrefersPassengerMatchingTheBus()
        {
            var resolver = new MoveResolver(MakeLevel("R", "BR"));
            Assert.IsTrue(resolver.TryFindHint(new List<GridPos>(), out GridPos hint));
            Assert.AreEqual(new GridPos(1, 0), hint);
        }

        [Test]
        public void Hint_IsEmptyWhenNobodyCanMove()
        {
            var resolver = new MoveResolver(MakeLevel("R", "X", "R"));
            Assert.IsFalse(resolver.TryFindHint(new List<GridPos>(), out _));
        }

        private static LevelData MakeLevel(string buses, params string[] rows)
        {
            return new LevelData
            {
                version = LevelData.CurrentVersion,
                id = "test",
                displayName = "Test",
                timeLimitSeconds = 60,
                busCapacity = 3,
                waitingSlotCount = 5,
                rows = rows,
                buses = buses,
            };
        }
    }
}