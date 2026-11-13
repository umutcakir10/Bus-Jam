using BusJam.Core;
using BusJam.Gameplay.Domain;
using BusJam.LevelSystem;
using NUnit.Framework;
using System.Collections.Generic;

namespace BusJam.Tests.EditMode
{
    public class LevelTimerTests
    {
        [Test]
        public void Tick_CountsDown()
        {
            var timer = new LevelTimer(60f);
            timer.Start();

            timer.Tick(10f);

            Assert.AreEqual(50f, timer.Remaining, 0.001f);
            Assert.IsTrue(timer.IsRunning);
        }

        [Test]
        public void Tick_ReportsExpiryExactlyOnce()
        {
            var timer = new LevelTimer(1f);
            timer.Start();

            Assert.IsTrue(timer.Tick(1.5f));
            Assert.IsFalse(timer.Tick(1f));
            Assert.AreEqual(0f, timer.Remaining);
            Assert.IsTrue(timer.IsExpired);
        }

        [Test]
        public void Stopped_DoesNotCountDown()
        {
            var timer = new LevelTimer(5f);
            timer.Start();
            timer.Stop();

            timer.Tick(3f);

            Assert.AreEqual(5f, timer.Remaining);
        }

        [Test]
        public void ResolverTimeUp_LosesAndIgnoresTaps()
        {
            var resolver = new MoveResolver(new LevelData
            {
                rows = new[] { "R" },
                buses = "R",
                busCapacity = 1,
                waitingSlotCount = 1,
            });

            resolver.TimeUp();

            Assert.AreEqual(GameState.Lost, resolver.State);
            Assert.AreEqual(LoseReason.TimeUp, resolver.LoseReason);
            Assert.AreEqual(TapResult.Ignored,
                resolver.Tap(new GridPos(0, 0), new List<GridPos>(), new List<MoveEvent>()));
        }

        [Test]
        public void ResolverTimeUp_AfterWin_StaysWon()
        {
            var resolver = new MoveResolver(new LevelData
            {
                rows = new[] { "R" },
                buses = "R",
                busCapacity = 1,
                waitingSlotCount = 1,
            });
            resolver.Tap(new GridPos(0, 0), new List<GridPos>(), new List<MoveEvent>());

            resolver.TimeUp();

            Assert.AreEqual(GameState.Won, resolver.State);
            Assert.AreEqual(LoseReason.None, resolver.LoseReason);
        }
    }
}
