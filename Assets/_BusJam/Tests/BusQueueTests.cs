using System;
using BusJam.Core;
using BusJam.Gameplay.Domain;
using BusJam.LevelSystem;
using NUnit.Framework;

namespace BusJam.Tests.EditMode
{
    public class BusQueueTests
    {
        private static BusQueue CreateQueue(string buses, int capacity = 3)
        {
            return new BusQueue(new LevelData { buses = buses, busCapacity = capacity });
        }

        [Test]
        public void Constructor_FirstBusIsAtTheStop()
        {
            var queue = CreateQueue("RBR");

            Assert.AreEqual(3, queue.Count);
            Assert.AreEqual(3, queue.Capacity);
            Assert.AreEqual(PassengerColor.Red, queue.CurrentColor);
            Assert.AreEqual(0, queue.SeatsTaken);
        }

        [Test]
        public void CanBoard_OnlyTheStopBusColor()
        {
            var queue = CreateQueue("RB");

            Assert.IsTrue(queue.CanBoard(PassengerColor.Red));
            Assert.IsFalse(queue.CanBoard(PassengerColor.Blue));
        }

        [Test]
        public void Board_GivesSeatsInOrderUntilFull()
        {
            var queue = CreateQueue("R");

            Assert.AreEqual(0, queue.Board(PassengerColor.Red));
            Assert.AreEqual(1, queue.Board(PassengerColor.Red));
            Assert.AreEqual(2, queue.Board(PassengerColor.Red));
            Assert.IsTrue(queue.IsCurrentFull);
            Assert.IsFalse(queue.CanBoard(PassengerColor.Red));
        }

        [Test]
        public void Board_WrongColor_Throws()
        {
            var queue = CreateQueue("R");

            Assert.Throws<InvalidOperationException>(() => queue.Board(PassengerColor.Blue));
        }

        [Test]
        public void Depart_NextBusArrivesEmpty()
        {
            var queue = CreateQueue("RB", capacity: 1);
            queue.Board(PassengerColor.Red);

            queue.Depart();

            Assert.AreEqual(PassengerColor.Blue, queue.CurrentColor);
            Assert.AreEqual(0, queue.SeatsTaken);
        }

        [Test]
        public void Depart_NotFull_Throws()
        {
            var queue = CreateQueue("RB");
            queue.Board(PassengerColor.Red);

            Assert.Throws<InvalidOperationException>(() => queue.Depart());
        }

        [Test]
        public void LastBusDeparts_AllDeparted()
        {
            var queue = CreateQueue("R", capacity: 1);
            queue.Board(PassengerColor.Red);

            queue.Depart();

            Assert.IsTrue(queue.AllDeparted);
            Assert.AreEqual(PassengerColor.None, queue.CurrentColor);
            Assert.IsFalse(queue.CanBoard(PassengerColor.Red));
        }
    }
}