using System;
using System.Collections.Generic;
using System.Threading;
using ActionFit.Time;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace ActionFit.LavaRush.UI.Tests
{
    public sealed class LavaRushTimingTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _objects.Count; index++)
            {
                if (_objects[index] != null)
                {
                    UnityEngine.Object.DestroyImmediate(_objects[index]);
                }
            }

            _objects.Clear();
        }

        [Test]
        public void StandaloneTiming_ForwardsFramesCatchesUpSecondsAndDisposesIndependently()
        {
            StandaloneLavaRushTiming timing = CreateTiming(
                new DateTime(2026, 7, 24, 3, 0, 0, DateTimeKind.Utc));
            var updates = new List<float>();
            int lateFrames = 0;
            int seconds = 0;
            IDisposable first = timing.SubscribeUpdate(updates.Add);
            IDisposable second = timing.SubscribeUpdate(updates.Add);
            IDisposable late = timing.SubscribeLateUpdate(_ => lateFrames++);
            IDisposable everySecond = timing.SubscribeEverySecond(() => seconds++);

            timing.Advance(0.25f, 2.25f);
            timing.AdvanceLate(0.25f);
            first.Dispose();
            first.Dispose();
            timing.Advance(0.5f, 0f);
            second.Dispose();
            late.Dispose();
            everySecond.Dispose();
            timing.Advance(1f, 2f);
            timing.AdvanceLate(1f);

            Assert.That(updates, Is.EqualTo(new[] { 0.25f, 0.25f, 0.5f }));
            Assert.That(lateFrames, Is.EqualTo(1));
            Assert.That(seconds, Is.EqualTo(2));
        }

        [Test]
        public void StandaloneTiming_UsesInjectedClockAndCalendarWithoutReadinessFallback()
        {
            DateTime utc = new DateTime(2026, 7, 24, 3, 0, 0, DateTimeKind.Utc);
            TimeZoneInfo zone = TimeZoneInfo.CreateCustomTimeZone(
                "LavaRushTimingTests+09",
                TimeSpan.FromHours(9),
                "LavaRushTimingTests+09",
                "LavaRushTimingTests+09");
            StandaloneLavaRushTiming timing = CreateTiming(utc, zone);

            Assert.That(timing.TryGetNow(out DateTime now), Is.True);
            Assert.That(now, Is.EqualTo(new DateTime(2026, 7, 24, 12, 0, 0)));
            Assert.That(timing.Now, Is.EqualTo(now));
        }

        [Test]
        public void StandaloneCountdown_UpdatesImmediatelyExpiresOnceAndHonorsCancellation()
        {
            DateTime utc = new DateTime(2026, 7, 24, 3, 0, 0, DateTimeKind.Utc);
            var clock = new ManualClock(utc);
            StandaloneLavaRushTiming timing = CreateTiming(clock);
            TMP_Text expiredTarget = CreateText("stale");
            TMP_Text canceledTarget = CreateText("waiting");
            int expired = 0;
            var cancellation = new CancellationTokenSource();
            timing.Register(
                expiredTarget,
                timing.Now.AddSeconds(1),
                CancellationToken.None,
                () => expired++);
            timing.Register(
                canceledTarget,
                timing.Now.AddSeconds(1),
                cancellation.Token,
                () => expired++);

            Assert.That(expiredTarget.text, Is.EqualTo("00:00:01"));
            cancellation.Cancel();
            clock.Advance(TimeSpan.FromSeconds(1));
            timing.Advance(0f, 1f);
            timing.Advance(0f, 1f);

            Assert.That(expiredTarget.text, Is.EqualTo("00:00:00"));
            Assert.That(canceledTarget.text, Is.EqualTo("00:00:01"));
            Assert.That(expired, Is.EqualTo(1));
            cancellation.Dispose();
        }

        [Test]
        public void TimeText_UsesAccumulatedHours()
        {
            TimeSpan remaining = TimeSpan.FromDays(1)
                .Add(TimeSpan.FromHours(4))
                .Add(TimeSpan.FromMinutes(5))
                .Add(TimeSpan.FromSeconds(30));

            Assert.That(
                LavaRushTimeText.FormatHourMinSec(remaining),
                Is.EqualTo("28:05:30"));
        }

        private StandaloneLavaRushTiming CreateTiming(
            DateTime utc,
            TimeZoneInfo timeZone = null)
        {
            return CreateTiming(new ManualClock(utc), timeZone);
        }

        private StandaloneLavaRushTiming CreateTiming(
            IClock clock,
            TimeZoneInfo timeZone = null)
        {
            var gameObject = new GameObject("LavaRushTimingTests");
            _objects.Add(gameObject);
            var timing = gameObject.AddComponent<StandaloneLavaRushTiming>();
            timing.Configure(clock, timeZone ?? TimeZoneInfo.Utc);
            return timing;
        }

        private TMP_Text CreateText(string text)
        {
            var gameObject = new GameObject("LavaRushTimingTests.Text");
            _objects.Add(gameObject);
            var target = gameObject.AddComponent<TextMeshProUGUI>();
            target.text = text;
            return target;
        }
    }
}
