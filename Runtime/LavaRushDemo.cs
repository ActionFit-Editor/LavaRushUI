using System;
using System.Collections.Generic;
using ActionFit.Content;
using ActionFit.Time;

namespace ActionFit.LavaRush.UI
{
    internal sealed class LavaRushDemoClock : IClock, ILavaRushLegacyLocalClock
    {
        private DateTime _utcNow = new(2026, 1, 5, 3, 0, 0, DateTimeKind.Utc);

        public DateTime UtcNow => _utcNow;
        public DateTime Now => DateTime.SpecifyKind(_utcNow.AddHours(9d), DateTimeKind.Unspecified);

        public void Advance(TimeSpan duration)
        {
            _utcNow = _utcNow.Add(duration);
        }
    }

    internal sealed class LavaRushDemoSchedulePolicy : ILavaRushSchedulePolicy
    {
        public bool IsEnabled => true;

        public bool IsActiveDay(DayOfWeek dayOfWeek) => dayOfWeek == DayOfWeek.Monday;
    }

    internal sealed class LavaRushDemoCatalogResolver : ILavaRushCatalogResolver
    {
        private readonly LavaRushCatalog _catalog = CreateCatalog();

        public LavaRushCatalog Current => _catalog;

        public bool TryResolve(string catalogVersion, string balanceRevision, out LavaRushCatalog catalog)
        {
            bool matches = string.Equals(catalogVersion, _catalog.CatalogVersion, StringComparison.Ordinal)
                && string.Equals(balanceRevision, _catalog.BalanceRevision, StringComparison.Ordinal);
            catalog = matches ? _catalog : null;
            return matches;
        }

        private static LavaRushCatalog CreateCatalog()
        {
            return new LavaRushCatalog(
                "lava-rush-ui-demo-v1",
                "neutral-2026-07",
                new[]
                {
                    CreateDifficulty(1, 70, 110, 45, 60),
                    CreateDifficulty(2, 100, 160, 38, 52),
                    CreateDifficulty(3, 135, 210, 32, 45),
                });
        }

        private static LavaRushDifficultyDefinition CreateDifficulty(
            int difficulty,
            int firstProgress,
            int secondProgress,
            int minLimitSeconds,
            int maxLimitSeconds)
        {
            return new LavaRushDifficultyDefinition(
                difficulty,
                new[]
                {
                    new LavaRushStageDefinition(1, 0, firstProgress, minLimitSeconds, maxLimitSeconds, Array.Empty<ContentReward>()),
                    new LavaRushStageDefinition(2, 12, secondProgress, minLimitSeconds, maxLimitSeconds, new[]
                    {
                        new ContentReward("coin", 100L * difficulty),
                    }),
                    new LavaRushStageDefinition(3, 7, secondProgress + 60, minLimitSeconds, maxLimitSeconds, new[]
                    {
                        new ContentReward("energy", 2L * difficulty),
                    }),
                    new LavaRushStageDefinition(4, 3, 0, 0, 0, new[]
                    {
                        new ContentReward("dia", 3L * difficulty),
                    }),
                });
        }
    }
}
