using System.Collections.Generic;
using BusJam.Core;
using BusJam.Gameplay.Domain;
using BusJam.LevelSystem;
using NUnit.Framework;

namespace BusJam.Tests.EditMode
{
    public class MoveResolverTests
    {
        private readonly List<GridPos> _path = new List<GridPos>();
        private readonly List<MoveEvent> _events = new List<MoveEvent>();

        private static MoveResolver Create(string[] rows, string buses, int capacity = 3, int slots = 5)
        {
            return new MoveResolver(new LevelData
            {
                rows = rows,
                buses = buses,
                busCapacity = capacity,
                waitingSlotCount = slots,
            });
        }

        private TapResult Tap(MoveResolver resolver, int x, int y) =>
            resolver.Tap(new GridPos(x, y), _path, _events);

        [Test]
        public void Tap_EmptyCell_Ignored()
        {
            var resolver = Create(new[] { "R." }, "R", capacity: 1);

            Assert.AreEqual(TapResult.Ignored, Tap(resolver, 1, 0));
        }

        [Test]
        public void Tap_BoxedInPassenger_BlockedAndNothingChanges()
        {
            var resolver = Create(new[] { "RRR", "RRR" }, "RR");

            Assert.AreEqual(TapResult.Blocked, Tap(resolver, 1, 1));
            Assert.AreEqual(6, resolver.Board.PassengerCount);
            Assert.AreEqual(0, _events.Count);
        }

        [Test]
        public void Tap_MatchingColor_BoardsBus()
        {
            var resolver = Create(new[] { "RRB" }, "R", capacity: 3, slots: 2);

            Assert.AreEqual(TapResult.Moved, Tap(resolver, 0, 0));
            Assert.AreEqual(1, _events.Count);
            Assert.AreEqual(MoveEventType.ToBus, _events[0].Type);
            Assert.AreEqual(0, _events[0].Seat);
            Assert.AreEqual(1, resolver.Buses.SeatsTaken);
        }

        [Test]
        public void Tap_OtherColor_GoesToSlot()
        {
            var resolver = Create(new[] { "RB" }, "RB", capacity: 1, slots: 2);

            Tap(resolver, 1, 0);

            Assert.AreEqual(MoveEventType.ToSlot, _events[0].Type);
            Assert.AreEqual(0, _events[0].Slot);
            Assert.AreEqual(PassengerColor.Blue, resolver.Waiting.GetColor(0));
        }

        [Test]
        public void LastSeatTaken_BusDeparts()
        {
            var resolver = Create(new[] { "RB" }, "RB", capacity: 1, slots: 2);

            Tap(resolver, 0, 0);

            Assert.AreEqual(2, _events.Count);
            Assert.AreEqual(MoveEventType.BusDeparted, _events[1].Type);
            Assert.AreEqual(0, _events[1].BusIndex);
            Assert.AreEqual(PassengerColor.Blue, resolver.Buses.CurrentColor);
        }

        [Test]
        public void NextBus_PicksUpWaitingPassenger_InTheSameTap()
        {
            
            var resolver = Create(new[] { "BR" }, "RB", capacity: 1, slots: 2);
            Tap(resolver, 0, 0);

            Tap(resolver, 1, 0);

            Assert.AreEqual(4, _events.Count);
            Assert.AreEqual(MoveEventType.ToBus, _events[0].Type);
            Assert.AreEqual(MoveEventType.BusDeparted, _events[1].Type);
            Assert.AreEqual(MoveEventType.SlotToBus, _events[2].Type);
            Assert.AreEqual(0, _events[2].Slot);
            Assert.AreEqual(1, _events[2].BusIndex);
            Assert.AreEqual(MoveEventType.BusDeparted, _events[3].Type);
            Assert.IsTrue(resolver.Waiting.IsEmpty);
            Assert.AreEqual(GameState.Won, resolver.State);
        }

        [Test]
        public void WaitingAreaFull_Lost_AndFurtherTapsIgnored()
        {
            var resolver = Create(new[] { "BR" }, "RB", capacity: 1, slots: 1);

            Tap(resolver, 0, 0);

            Assert.AreEqual(GameState.Lost, resolver.State);
            Assert.AreEqual(TapResult.Ignored, Tap(resolver, 1, 0));
        }

        [Test]
        public void Level001_PlayedThrough_Won()
        {
            var resolver = Create(new[] { "RRBR", "BRRB", "R..." }, "RBR");

            
            Tap(resolver, 0, 0);
            Tap(resolver, 1, 0);
            Tap(resolver, 3, 0);
            
            Tap(resolver, 2, 0);
            Tap(resolver, 0, 1);
            Tap(resolver, 3, 1);
         
            Tap(resolver, 1, 1);
            Tap(resolver, 2, 1);
            Tap(resolver, 0, 2);

            Assert.AreEqual(GameState.Won, resolver.State);
            Assert.AreEqual(0, resolver.Board.PassengerCount);
            Assert.IsTrue(resolver.Waiting.IsEmpty);
        }
    }
}