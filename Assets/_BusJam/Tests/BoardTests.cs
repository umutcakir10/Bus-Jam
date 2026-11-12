using System;
using System.Collections.Generic;
using BusJam.Core;
using BusJam.Gameplay.Domain;
using BusJam.LevelSystem;
using NUnit.Framework;

namespace BusJam.Tests.EditMode
{
    public class BoardTests
    {
       
        private static BoardModel CreateBoard(params string[] rows)
        {
            return new BoardModel(new LevelData { rows = rows });
        }

       

        [Test]
        public void Constructor_ReadsEveryCellType()
        {
            var board = CreateBoard(
                "R.#",
                "BX.");

            Assert.AreEqual(3, board.Width);
            Assert.AreEqual(2, board.Height);
            Assert.AreEqual(2, board.PassengerCount);

            Assert.AreEqual(CellType.Passenger, board.GetCell(new GridPos(0, 0)));
            Assert.AreEqual(PassengerColor.Red, board.GetColor(new GridPos(0, 0)));
            Assert.AreEqual(PassengerColor.Blue, board.GetColor(new GridPos(0, 1)));
            Assert.AreEqual(CellType.Empty, board.GetCell(new GridPos(1, 0)));
            Assert.AreEqual(CellType.Void, board.GetCell(new GridPos(2, 0)));
            Assert.AreEqual(CellType.Obstacle, board.GetCell(new GridPos(1, 1)));
        }

        [Test]
        public void RemovePassenger_EmptiesCellAndDecrementsCount()
        {
            var board = CreateBoard("RB");
            var pos = new GridPos(0, 0);

            board.RemovePassenger(pos);

            Assert.AreEqual(CellType.Empty, board.GetCell(pos));
            Assert.AreEqual(PassengerColor.None, board.GetColor(pos));
            Assert.AreEqual(1, board.PassengerCount);
        }

        [Test]
        public void RemovePassenger_OnEmptyCell_Throws()
        {
            var board = CreateBoard("R.");

            Assert.Throws<InvalidOperationException>(() => board.RemovePassenger(new GridPos(1, 0)));
        }

       

        [Test]
        public void FindExit_FrontRowPassenger_ExitsInPlace()
        {
            var board = CreateBoard(
                "RRR",
                "RRR");
            var path = new List<GridPos>();

            bool found = new BoardPathfinder(board).TryFindExitPath(new GridPos(1, 0), path);

            Assert.IsTrue(found);
            CollectionAssert.AreEqual(new[] { new GridPos(1, 0) }, path);
        }

        [Test]
        public void FindExit_SurroundedPassenger_CannotExit()
        {
            var board = CreateBoard(
                "RRR",
                "RRR");
            var path = new List<GridPos>();

            bool found = new BoardPathfinder(board).TryFindExitPath(new GridPos(1, 1), path);

            Assert.IsFalse(found);
            Assert.AreEqual(0, path.Count);
        }

        [Test]
        public void FindExit_VoidAndObstacle_BothBlock()
        {
            
            var board = CreateBoard(
                "#X",
                "R.");
            var path = new List<GridPos>();

            bool found = new BoardPathfinder(board).TryFindExitPath(new GridPos(0, 1), path);

            Assert.IsFalse(found);
        }

        [Test]
        public void FindExit_OpenColumn_WalksStraightForward()
        {
            var board = CreateBoard(
                "R.R",
                "R.R",
                "RRR");
            var path = new List<GridPos>();

            bool found = new BoardPathfinder(board).TryFindExitPath(new GridPos(1, 2), path);

            Assert.IsTrue(found);
            CollectionAssert.AreEqual(
                new[] { new GridPos(1, 2), new GridPos(1, 1), new GridPos(1, 0) },
                path);
        }

        [Test]
        public void FindExit_BlockedAhead_WalksAroundSideways()
        {
            
            var board = CreateBoard(
                "RR.",
                "X..",
                "R..");
            var path = new List<GridPos>();

            bool found = new BoardPathfinder(board).TryFindExitPath(new GridPos(0, 2), path);

            Assert.IsTrue(found);
            CollectionAssert.AreEqual(
                new[]
                {
                    new GridPos(0, 2), new GridPos(1, 2), new GridPos(1, 1),
                    new GridPos(2, 1), new GridPos(2, 0),
                },
                path);
        }

        [Test]
        public void FindExit_AfterBlockerLeaves_PathOpens()
        {
            var board = CreateBoard(
                "RB",
                "GY");
            var pathfinder = new BoardPathfinder(board);
            var start = new GridPos(0, 1);
            var path = new List<GridPos>();

            Assert.IsFalse(pathfinder.TryFindExitPath(start, path));

            board.RemovePassenger(new GridPos(0, 0));

            Assert.IsTrue(pathfinder.TryFindExitPath(start, path));
            CollectionAssert.AreEqual(new[] { start, new GridPos(0, 0) }, path);
        }
    }
}