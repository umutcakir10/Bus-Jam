using System.Collections.Generic;
using BusJam.Core;
using BusJam.Gameplay.Domain;
using BusJam.LevelSystem;
using NUnit.Framework;

namespace BusJam.Tests
{
    public sealed class HiddenPassengerTests
    {
        [Test]
        public void LowercaseLetter_IsSameColorButHidden()
        {
            Assert.IsTrue(LevelFormat.TryParseColor('r', out PassengerColor color));
            Assert.AreEqual(PassengerColor.Red, color);
            Assert.IsTrue(LevelFormat.IsHidden('r'));
            Assert.IsFalse(LevelFormat.IsHidden('R'));
            Assert.IsFalse(LevelFormat.IsHidden('.'));
        }

        [Test]
        public void Validator_CountsHiddenPassengers()
        {
            var errors = new List<string>();
            bool valid = LevelValidator.Validate(MakeLevel("R", "r", "R"), errors);
            Assert.IsTrue(valid, string.Join("\n", errors));
        }

        [Test]
        public void Validator_RejectsLowercaseBus()
        {
            LevelData level = MakeLevel("R", "r", "R");
            level.buses = "r";
            Assert.IsFalse(LevelValidator.Validate(level, new List<string>()));
        }

        [Test]
        public void HasExitPath_OpensWhenFrontPassengerLeaves()
        {
            var resolver = new MoveResolver(MakeLevel("R", "r", "R"));
            var path = new List<GridPos>();
            var events = new List<MoveEvent>();
            var middle = new GridPos(0, 1);

            Assert.IsFalse(resolver.HasExitPath(middle, path));

            // Hangi ucun ön sıra olduğundan bağımsız olsun diye iki uca da dokunuyoruz
            // Öndeki gidiyor, arkadaki ortadaki yüzünden kapalı kalıyor
            resolver.Tap(new GridPos(0, 0), path, events);
            resolver.Tap(new GridPos(0, 2), path, events);

            Assert.IsTrue(resolver.HasExitPath(middle, path));
        }

        private static LevelData MakeLevel(params string[] rows)
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
                buses = "R",
            };
        }
    }
}