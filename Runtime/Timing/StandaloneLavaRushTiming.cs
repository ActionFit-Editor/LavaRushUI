using System;
using System.Collections.Generic;
using System.Threading;
using ActionFit.Time;
using TMPro;
using UnityEngine;

namespace ActionFit.LavaRush.UI
{
    public sealed class StandaloneLavaRushTiming :
        MonoBehaviour,
        ILavaRushFrameScheduler,
        ILavaRushCountdownScheduler
    {
        private readonly List<CountdownSubscription> _countdowns = new List<CountdownSubscription>(16);
        private IClock _clock;
        private TimeZoneInfo _timeZone;
        private float _secondAccumulator;

        private event Action<float> UpdateRequested;
        private event Action<float> LateUpdateRequested;
        private event Action EverySecondRequested;

        public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(Clock.UtcNow, TimeZone);

        private IClock Clock => _clock ?? SystemClock.Instance;
        private TimeZoneInfo TimeZone => _timeZone ?? TimeZoneInfo.Local;

        public void Configure(IClock clock, TimeZoneInfo timeZone)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        }

        public bool TryGetNow(out DateTime now)
        {
            now = Now;
            return true;
        }

        public IDisposable SubscribeUpdate(Action<float> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            UpdateRequested += handler;
            return new Subscription(() => UpdateRequested -= handler);
        }

        public IDisposable SubscribeLateUpdate(Action<float> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            LateUpdateRequested += handler;
            return new Subscription(() => LateUpdateRequested -= handler);
        }

        public IDisposable SubscribeEverySecond(Action handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            EverySecondRequested += handler;
            return new Subscription(() => EverySecondRequested -= handler);
        }

        public void Register(
            TMP_Text target,
            DateTime endTime,
            CancellationToken cancellationToken,
            Action onExpired = null,
            Func<TimeSpan, string> formatter = null)
        {
            if (target == null)
            {
                Debug.LogError("[StandaloneLavaRushTiming] Register called with null target.");
                return;
            }

            Func<TimeSpan, string> resolvedFormatter = formatter ?? LavaRushTimeText.FormatDefault;
            _countdowns.Add(new CountdownSubscription(
                target,
                endTime,
                resolvedFormatter,
                onExpired,
                cancellationToken));

            TimeSpan remaining = endTime - Now;
            target.text = resolvedFormatter(remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero);
        }

        internal void Advance(float deltaTime, float unscaledDeltaTime)
        {
            UpdateRequested?.Invoke(deltaTime);
            _secondAccumulator += unscaledDeltaTime;
            while (_secondAccumulator >= 1f)
            {
                _secondAccumulator -= 1f;
                TickCountdowns();
                EverySecondRequested?.Invoke();
            }
        }

        internal void AdvanceLate(float deltaTime)
        {
            LateUpdateRequested?.Invoke(deltaTime);
        }

        private void Update()
        {
            Advance(UnityEngine.Time.deltaTime, UnityEngine.Time.unscaledDeltaTime);
        }

        private void LateUpdate()
        {
            AdvanceLate(UnityEngine.Time.deltaTime);
        }

        private void TickCountdowns()
        {
            DateTime now = Now;
            for (int index = _countdowns.Count - 1; index >= 0; index--)
            {
                CountdownSubscription countdown = _countdowns[index];
                if (countdown.CancellationToken.IsCancellationRequested || countdown.Target == null)
                {
                    _countdowns.RemoveAt(index);
                    continue;
                }

                TimeSpan remaining = countdown.EndTime - now;
                if (remaining <= TimeSpan.Zero)
                {
                    countdown.Target.text = countdown.Formatter(TimeSpan.Zero);
                    try
                    {
                        countdown.OnExpired?.Invoke();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError($"[StandaloneLavaRushTiming] onExpired threw: {exception}");
                    }

                    _countdowns.RemoveAt(index);
                    continue;
                }

                countdown.Target.text = countdown.Formatter(remaining);
            }
        }

        private void OnDestroy()
        {
            UpdateRequested = null;
            LateUpdateRequested = null;
            EverySecondRequested = null;
            _countdowns.Clear();
        }

        private readonly struct CountdownSubscription
        {
            public CountdownSubscription(
                TMP_Text target,
                DateTime endTime,
                Func<TimeSpan, string> formatter,
                Action onExpired,
                CancellationToken cancellationToken)
            {
                Target = target;
                EndTime = endTime;
                Formatter = formatter;
                OnExpired = onExpired;
                CancellationToken = cancellationToken;
            }

            public TMP_Text Target { get; }
            public DateTime EndTime { get; }
            public Func<TimeSpan, string> Formatter { get; }
            public Action OnExpired { get; }
            public CancellationToken CancellationToken { get; }
        }

        private sealed class Subscription : IDisposable
        {
            private Action _dispose;

            public Subscription(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                Interlocked.Exchange(ref _dispose, null)?.Invoke();
            }
        }
    }
}
