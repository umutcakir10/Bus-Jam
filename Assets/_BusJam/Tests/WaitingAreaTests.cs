using System;
using BusJam.Core;
using BusJam.Gameplay.Domain;
using NUnit.Framework;

namespace BusJam.Tests.EditMode
{
    public class WaitingAreaTests
    {
        [Test]
        public void Add_FillsSlotsLeftToRight()
        {
            var area = new WaitingArea(3);

            Assert.AreEqual(0, area.Add(PassengerColor.Red));
            Assert.AreEqual(1, area.Add(PassengerColor.Blue));
            Assert.AreEqual(2, area.Count);
            Assert.AreEqual(PassengerColor.Blue, area.GetColor(1));
        }

        [Test]
        public void Remove_FreedSlotIsReusedFirst()
        {
            var area = new WaitingArea(3);
            area.Add(PassengerColor.Red);
            area.Add(PassengerColor.Blue);
            area.Add(PassengerColor.Green);

            area.Remove(1);

            Assert.AreEqual(1, area.Add(PassengerColor.Yellow));
            Assert.AreEqual(PassengerColor.Green, area.GetColor(2));
        }

        [Test]
        public void TryFindColor_FindsLeftmostMatch()
        {
            var area = new WaitingArea(4);
            area.Add(PassengerColor.Blue);
            area.Add(PassengerColor.Red);
            area.Add(PassengerColor.Red);

            Assert.IsTrue(area.TryFindColor(PassengerColor.Red, out int slot));
            Assert.AreEqual(1, slot);
            Assert.IsFalse(area.TryFindColor(PassengerColor.Green, out _));
        }

        [Test]
        public void Add_WhenFull_Throws()
        {
            var area = new WaitingArea(1);
            area.Add(PassengerColor.Red);

            Assert.IsTrue(area.IsFull);
            Assert.Throws<InvalidOperationException>(() => area.Add(PassengerColor.Blue));
        }

        [Test]
        public void Remove_EmptySlot_Throws()
        {
            var area = new WaitingArea(2);

            Assert.Throws<InvalidOperationException>(() => area.Remove(0));
        }
    }
}