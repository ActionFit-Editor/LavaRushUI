using System;
using System.Collections.Generic;
using ActionFit.Content;
using ActionFit.LavaRush;
using ActionFit.LavaRush.UI;
using UnityEngine;

[Flags]
public enum CatDetectiveLavaRushDays
{
    None = 0,
    Sunday = 1 << 0,
    Monday = 1 << 1,
    Tuesday = 1 << 2,
    Wednesday = 1 << 3,
    Thursday = 1 << 4,
    Friday = 1 << 5,
    Saturday = 1 << 6,
    All = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday,
}

[Serializable]
public sealed class CatDetectiveLavaRushLocalizationRoute
{
    [SerializeField] private string packageKey;
    [SerializeField] private string generalTableKey;

    public string PackageKey => packageKey;
    public string GeneralTableKey => generalTableKey;
}

[Serializable]
public sealed class CatDetectiveLavaRushAudioRoute
{
    [SerializeField] private string packageCue;
    [SerializeField] private SFXType sfx = SFXType.None;

    public string PackageCue => packageCue;
    public SFXType Sfx => sfx;

    public CatDetectiveLavaRushAudioRoute()
    {
    }

    public CatDetectiveLavaRushAudioRoute(string packageCue, SFXType sfx)
    {
        this.packageCue = packageCue;
        this.sfx = sfx;
    }
}

[Serializable]
public sealed class CatDetectiveLavaRushRewardRoute
{
    [SerializeField] private string rewardId;
    [SerializeField] private RewardType rewardType = RewardType.None;
    [SerializeField] private int group;
    [SerializeField] private int value;

    public string RewardId => rewardId;

    public CatDetectiveLavaRushRewardRoute()
    {
    }

    public CatDetectiveLavaRushRewardRoute(string rewardId, RewardType rewardType, int group = 0, int value = 0)
    {
        this.rewardId = rewardId;
        this.rewardType = rewardType;
        this.group = group;
        this.value = value;
    }

    public RewardItem Create(long amount)
    {
        return new RewardItem
        {
            rewardType = rewardType,
            group = group,
            value = value,
            qty = amount,
        };
    }
}

[Serializable]
public sealed class CatDetectiveLavaRushRewardDefinition
{
    [SerializeField] private string rewardId = "gold";
    [SerializeField, Min(1)] private long amount = 100;

    public ContentReward Create() => new(rewardId, Math.Max(1L, amount));
}

[Serializable]
public sealed class CatDetectiveLavaRushStageDefinition
{
    [SerializeField, Min(1)] private int stage = 1;
    [SerializeField, Min(0)] private int capacity;
    [SerializeField, Min(0)] private int requiredProgress = 50;
    [SerializeField, Min(0)] private int minLimitSeconds = 45;
    [SerializeField, Min(0)] private int maxLimitSeconds = 60;
    [SerializeField] private CatDetectiveLavaRushRewardDefinition[] rewards = Array.Empty<CatDetectiveLavaRushRewardDefinition>();

    public LavaRushStageDefinition Create()
    {
        var runtimeRewards = new List<ContentReward>();
        if (rewards != null)
        {
            foreach (CatDetectiveLavaRushRewardDefinition reward in rewards)
            {
                if (reward != null)
                {
                    runtimeRewards.Add(reward.Create());
                }
            }
        }

        return new LavaRushStageDefinition(
            Math.Max(1, stage),
            Math.Max(0, capacity),
            Math.Max(0, requiredProgress),
            Math.Max(0, minLimitSeconds),
            Math.Max(Math.Max(0, minLimitSeconds), maxLimitSeconds),
            runtimeRewards);
    }
}

[Serializable]
public sealed class CatDetectiveLavaRushDifficultyDefinition
{
    [SerializeField, Min(1)] private int difficulty = 1;
    [SerializeField] private CatDetectiveLavaRushStageDefinition[] stages = Array.Empty<CatDetectiveLavaRushStageDefinition>();

    public LavaRushDifficultyDefinition Create()
    {
        var runtimeStages = new List<LavaRushStageDefinition>();
        if (stages != null)
        {
            foreach (CatDetectiveLavaRushStageDefinition stage in stages)
            {
                if (stage != null)
                {
                    runtimeStages.Add(stage.Create());
                }
            }
        }
        return new LavaRushDifficultyDefinition(Math.Max(1, difficulty), runtimeStages);
    }
}

[CreateAssetMenu(
    fileName = "CatDetectiveLavaRushSettings",
    menuName = "ActionFit/Lava Rush/CatDetective Settings")]
public sealed class CatDetectiveLavaRushSettings : ScriptableObject
{
    public const string ResourcesPath = "CatDetectiveLavaRushSettings";

    [Header("Engine")]
    [SerializeField] private string contentId = "catdetective-lava-rush";
    [SerializeField] private bool scheduleEnabled = true;
    [SerializeField] private CatDetectiveLavaRushDays activeDays = CatDetectiveLavaRushDays.All;
    [SerializeField] private string catalogVersion = "catdetective-starter-v1";
    [SerializeField] private string balanceRevision = "starter-2026-07";

    [Header("Presentation")]
    [SerializeField] private string fallbackPlayerName = "Detective";
    [SerializeField] private Color profileAccent = new(0.2f, 0.72f, 0.92f, 1f);

    [Header("Project Adapters")]
    [SerializeField] private CatDetectiveLavaRushLocalizationRoute[] localizationRoutes = Array.Empty<CatDetectiveLavaRushLocalizationRoute>();
    [SerializeField] private CatDetectiveLavaRushAudioRoute[] audioRoutes = Array.Empty<CatDetectiveLavaRushAudioRoute>();
    [SerializeField] private CatDetectiveLavaRushRewardRoute[] rewardRoutes = Array.Empty<CatDetectiveLavaRushRewardRoute>();

    [Header("Catalog")]
    [SerializeField] private CatDetectiveLavaRushDifficultyDefinition[] difficulties = Array.Empty<CatDetectiveLavaRushDifficultyDefinition>();

    public string ContentId => string.IsNullOrWhiteSpace(contentId) ? "catdetective-lava-rush" : contentId.Trim();
    public bool ScheduleEnabled => scheduleEnabled;
    public string FallbackPlayerName => string.IsNullOrWhiteSpace(fallbackPlayerName) ? "Detective" : fallbackPlayerName;
    public Color ProfileAccent => profileAccent;

    public bool IsActiveDay(DayOfWeek dayOfWeek)
    {
        CatDetectiveLavaRushDays flag = dayOfWeek switch
        {
            DayOfWeek.Sunday => CatDetectiveLavaRushDays.Sunday,
            DayOfWeek.Monday => CatDetectiveLavaRushDays.Monday,
            DayOfWeek.Tuesday => CatDetectiveLavaRushDays.Tuesday,
            DayOfWeek.Wednesday => CatDetectiveLavaRushDays.Wednesday,
            DayOfWeek.Thursday => CatDetectiveLavaRushDays.Thursday,
            DayOfWeek.Friday => CatDetectiveLavaRushDays.Friday,
            DayOfWeek.Saturday => CatDetectiveLavaRushDays.Saturday,
            _ => CatDetectiveLavaRushDays.None,
        };
        return (activeDays & flag) != 0;
    }

    public LavaRushCatalog CreateCatalog()
    {
        if (difficulties == null || difficulties.Length == 0)
        {
            return CreateStarterCatalog();
        }

        var runtimeDifficulties = new List<LavaRushDifficultyDefinition>();
        foreach (CatDetectiveLavaRushDifficultyDefinition difficulty in difficulties)
        {
            if (difficulty != null)
            {
                runtimeDifficulties.Add(difficulty.Create());
            }
        }
        return new LavaRushCatalog(catalogVersion, balanceRevision, runtimeDifficulties);
    }

    public bool TryCreateReward(string rewardId, long amount, out RewardItem reward)
    {
        foreach (CatDetectiveLavaRushRewardRoute route in GetRewardRoutes())
        {
            if (route != null && string.Equals(route.RewardId, rewardId, StringComparison.OrdinalIgnoreCase))
            {
                reward = route.Create(amount);
                return reward.rewardType != RewardType.None && reward.qty > 0;
            }
        }

        reward = default;
        return false;
    }

    public bool TryGetGeneralTableKey(string packageKey, out string tableKey)
    {
        if (localizationRoutes != null)
        {
            foreach (CatDetectiveLavaRushLocalizationRoute route in localizationRoutes)
            {
                if (route != null
                    && string.Equals(route.PackageKey, packageKey, StringComparison.Ordinal)
                    && !string.IsNullOrWhiteSpace(route.GeneralTableKey))
                {
                    tableKey = route.GeneralTableKey.Trim();
                    return true;
                }
            }
        }

        tableKey = null;
        return false;
    }

    public bool TryGetSfx(string packageCue, out SFXType sfx)
    {
        foreach (CatDetectiveLavaRushAudioRoute route in GetAudioRoutes())
        {
            if (route != null && string.Equals(route.PackageCue, packageCue, StringComparison.Ordinal))
            {
                sfx = route.Sfx;
                return sfx != SFXType.None;
            }
        }

        sfx = SFXType.None;
        return false;
    }

    private IEnumerable<CatDetectiveLavaRushRewardRoute> GetRewardRoutes()
    {
        if (rewardRoutes != null && rewardRoutes.Length > 0)
        {
            return rewardRoutes;
        }

        return new[]
        {
            CreateRewardRoute("coin", RewardType.Gold),
            CreateRewardRoute("gold", RewardType.Gold),
            CreateRewardRoute("energy", RewardType.Energy),
            CreateRewardRoute("dia", RewardType.Dia),
            CreateRewardRoute("exp", RewardType.Exp),
        };
    }

    private IEnumerable<CatDetectiveLavaRushAudioRoute> GetAudioRoutes()
    {
        if (audioRoutes != null && audioRoutes.Length > 0)
        {
            return audioRoutes;
        }

        return new[]
        {
            new CatDetectiveLavaRushAudioRoute(LavaRushUIKeys.AudioScreen, SFXType.Reward),
            new CatDetectiveLavaRushAudioRoute(LavaRushUIKeys.AudioProgress, SFXType.Progress),
            new CatDetectiveLavaRushAudioRoute(LavaRushUIKeys.AudioReward, SFXType.Reward),
        };
    }

    private static CatDetectiveLavaRushRewardRoute CreateRewardRoute(string rewardId, RewardType rewardType)
    {
        return new CatDetectiveLavaRushRewardRoute(rewardId, rewardType);
    }

    private LavaRushCatalog CreateStarterCatalog()
    {
        return new LavaRushCatalog(
            string.IsNullOrWhiteSpace(catalogVersion) ? "catdetective-starter-v1" : catalogVersion.Trim(),
            string.IsNullOrWhiteSpace(balanceRevision) ? "starter-2026-07" : balanceRevision.Trim(),
            new[]
            {
                CreateStarterDifficulty(1, 60, 100, 50, 65),
                CreateStarterDifficulty(2, 90, 145, 42, 58),
                CreateStarterDifficulty(3, 120, 190, 36, 50),
            });
    }

    private static LavaRushDifficultyDefinition CreateStarterDifficulty(
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
                new LavaRushStageDefinition(2, 10, secondProgress, minLimitSeconds, maxLimitSeconds, new[]
                {
                    new ContentReward("gold", 100L * difficulty),
                }),
                new LavaRushStageDefinition(3, 6, secondProgress + 50, minLimitSeconds, maxLimitSeconds, new[]
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
