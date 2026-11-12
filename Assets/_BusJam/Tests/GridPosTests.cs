using System.Collections.Generic;
using BusJam.Core;
using NUnit.Framework;

namespace BusJam.Tests.EditMode
{
    public class GridPosTests
    {
        [Test]
        public void Equals_SameCoordinates_AreEqual()
        {
            var a = new GridPos(2, 3);
            var b = new GridPos(2, 3);

            Assert.IsTrue(a == b);
            Assert.IsTrue(a.Equals(b));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equals_DifferentCoordinates_AreNotEqual()
        {
            Assert.IsTrue(new GridPos(2, 3) != new GridPos(3, 2));
        }

        [Test]
        public void HashSet_FindsEqualPosition()
        {
            var set = new HashSet<GridPos> { new GridPos(1, 1) };

            Assert.IsTrue(set.Contains(new GridPos(1, 1)));
            Assert.IsFalse(set.Contains(new GridPos(1, 2)));
        }

        [Test]
        public void Plus_AddsComponents()
        {
            Assert.AreEqual(new GridPos(3, 1), new GridPos(2, 3) + new GridPos(1, -2));
        }
    }
}