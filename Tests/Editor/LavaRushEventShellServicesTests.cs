using System;
using NUnit.Framework;
using UnityEngine;

namespace ActionFit.LavaRush.UI.Tests
{
    public sealed class LavaRushEventShellServicesTests
    {
        [Test]
        public void OrderProgressSource_KeepsTheEnabledLifetimeOnTheSubscriber()
        {
            var source = new TestOrderProgressSource();
            int progress = 0;

            IDisposable lifetime = source.Subscribe(value => progress += value);
            source.Publish(10);
            lifetime.Dispose();
            source.Publish(20);

            Assert.That(progress, Is.EqualTo(10));
            Assert.That(source.SubscriptionCount, Is.Zero);
        }

        [Test]
        public void AccessAndProgressPorts_ExposeOnlyNeutralPresentationState()
        {
            Assert.That(
                typeof(ILavaRushAccessService)
                    .GetProperty(nameof(ILavaRushAccessService.IsEventActive)),
                Is.Not.Null);
            Assert.That(
                typeof(ILavaRushAccessService)
                    .GetProperty(nameof(ILavaRushAccessService.IsEventStarted)),
                Is.Not.Null);
            Assert.That(
                typeof(ILavaRushAccessService)
                    .GetProperty(nameof(ILavaRushAccessService.EventEndTime)),
                Is.Not.Null);
            Assert.That(
                typeof(ILavaRushAccessService)
                    .GetProperty(nameof(ILavaRushAccessService.EventRemainTime)),
                Is.Not.Null);
            Assert.That(
                typeof(ILavaRushProgressView)
                    .GetProperty(nameof(ILavaRushProgressView.TargetProgress))
                    ?.PropertyType,
                Is.EqualTo(typeof(RectTransform)));
        }

        private sealed class TestOrderProgressSource : ILavaRushOrderProgressSource
        {
            private Action<int> _handler;

            public int SubscriptionCount => _handler?.GetInvocationList().Length ?? 0;

            public IDisposable Subscribe(Action<int> onOrderProgress)
            {
                _handler += onOrderProgress;
                return new Subscription(this, onOrderProgress);
            }

            public void Publish(int amount) => _handler?.Invoke(amount);

            private sealed class Subscription : IDisposable
            {
                private TestOrderProgressSource _owner;
                private Action<int> _handler;

                public Subscription(TestOrderProgressSource owner, Action<int> handler)
                {
                    _owner = owner;
                    _handler = handler;
                }

                public void Dispose()
                {
                    TestOrderProgressSource owner = _owner;
                    Action<int> handler = _handler;
                    _owner = null;
                    _handler = null;
                    if (owner != null)
                    {
                        owner._handler -= handler;
                    }
                }
            }
        }
    }
}
