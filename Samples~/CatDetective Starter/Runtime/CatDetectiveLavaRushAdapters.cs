using System;
using System.Collections.Generic;
using ActionFit.Content;
using ActionFit.LavaRush;
using ActionFit.LavaRush.UI;
using ActionFit.Time;
using Blossom.Preference;
using UnityEngine;

public sealed class CatDetectiveLavaRushClock : IClock, ILavaRushLegacyLocalClock
{
    public DateTime UtcNow => DateTime.SpecifyKind(TimeProvider.UtcNow, DateTimeKind.Utc);
    public DateTime Now => TimeProvider.Now;
}

public sealed class CatDetectiveLavaRushSchedulePolicy : ILavaRushSchedulePolicy
{
    private readonly CatDetectiveLavaRushSettings _settings;

    public CatDetectiveLavaRushSchedulePolicy(CatDetectiveLavaRushSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public bool IsEnabled => _settings.ScheduleEnabled;
    public bool IsActiveDay(DayOfWeek dayOfWeek) => _settings.IsActiveDay(dayOfWeek);
}

public sealed class CatDetectiveLavaRushCatalogResolver : ILavaRushCatalogResolver
{
    private readonly LavaRushCatalog _catalog;

    public CatDetectiveLavaRushCatalogResolver(CatDetectiveLavaRushSettings settings)
    {
        _catalog = settings != null
            ? settings.CreateCatalog()
            : throw new ArgumentNullException(nameof(settings));
    }

    public LavaRushCatalog Current => _catalog;

    public bool TryResolve(string catalogVersion, string balanceRevision, out LavaRushCatalog catalog)
    {
        bool matches = string.Equals(catalogVersion, _catalog.CatalogVersion, StringComparison.Ordinal)
            && string.Equals(balanceRevision, _catalog.BalanceRevision, StringComparison.Ordinal);
        catalog = matches ? _catalog : null;
        return matches;
    }
}

public sealed class CatDetectiveLavaRushStateStore : IContentStateStore, IFlushableContentStateStore
{
    private const string KeyPrefix = "lava_rush.state.";

    public bool TryLoad(string contentId, out string json)
    {
        json = Prefs.Get<SimplePrefs>().Get(BuildKey(contentId), string.Empty);
        return !string.IsNullOrEmpty(json);
    }

    public void Save(string contentId, string json)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json));
        }
        Prefs.Get<SimplePrefs>().Set(BuildKey(contentId), json);
    }

    public void Delete(string contentId)
    {
        Prefs.Get<SimplePrefs>().DeleteKey(BuildKey(contentId));
        Flush();
    }

    public void Flush()
    {
        Prefs.Save(nameof(SimplePrefs), true);
    }

    private static string BuildKey(string contentId)
    {
        if (string.IsNullOrWhiteSpace(contentId))
        {
            throw new ArgumentException("Content ID is required.", nameof(contentId));
        }
        return KeyPrefix + contentId.Trim();
    }
}

public sealed class CatDetectiveLavaRushRewardService : IContentRewardService
{
    [Serializable]
    private sealed class Ledger
    {
        public List<string> transactionIds = new();
    }

    private const string LedgerKey = "lava_rush.reward_transactions";
    private readonly CatDetectiveLavaRushSettings _settings;

    public CatDetectiveLavaRushRewardService(CatDetectiveLavaRushSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public bool IsAvailable => Prefs.IsInitialized && Main.Data != null;

    public bool HasGranted(string transactionId)
    {
        ValidateTransactionId(transactionId);
        return LoadLedger().transactionIds.Contains(transactionId);
    }

    public bool GrantOnce(string transactionId, IReadOnlyList<ContentReward> rewards)
    {
        ValidateTransactionId(transactionId);
        if (!IsAvailable || rewards == null || rewards.Count == 0)
        {
            return false;
        }

        Ledger ledger = LoadLedger();
        if (ledger.transactionIds.Contains(transactionId))
        {
            return false;
        }

        var projectRewards = new List<RewardItem>(rewards.Count);
        foreach (ContentReward reward in rewards)
        {
            if (reward == null || !_settings.TryCreateReward(reward.RewardId, reward.Amount, out RewardItem projectReward))
            {
                UnityEngine.Debug.LogError(
                    $"[CatDetectiveLavaRushRewardService] Unsupported reward route: {reward?.RewardId ?? "<null>"}.");
                return false;
            }
            projectRewards.Add(projectReward);
        }

        foreach (RewardItem reward in projectRewards)
        {
            if (!Main.Data.AddRewardItem(reward))
            {
                UnityEngine.Debug.LogError(
                    $"[CatDetectiveLavaRushRewardService] Reward grant failed: {reward.rewardType} x{reward.qty}.");
                return false;
            }
        }

        ledger.transactionIds.Add(transactionId);
        SaveLedger(ledger);
        Prefs.SaveAll(true);
        return true;
    }

    private static Ledger LoadLedger()
    {
        string json = Prefs.Get<SimplePrefs>().Get(LedgerKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return new Ledger();
        }

        Ledger ledger = JsonUtility.FromJson<Ledger>(json);
        if (ledger?.transactionIds == null)
        {
            throw new InvalidOperationException("The CatDetective Lava Rush reward ledger is corrupted.");
        }
        return ledger;
    }

    private static void SaveLedger(Ledger ledger)
    {
        Prefs.Get<SimplePrefs>().Set(LedgerKey, JsonUtility.ToJson(ledger));
    }

    private static void ValidateTransactionId(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            throw new ArgumentException("Transaction ID is required.", nameof(transactionId));
        }
    }
}

public sealed class CatDetectiveLavaRushLocalizer : ILavaRushUILocalizer
{
    private readonly CatDetectiveLavaRushSettings _settings;

    public CatDetectiveLavaRushLocalizer(CatDetectiveLavaRushSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public string Get(string key, string fallback)
    {
        if (Main.Locale != null && _settings.TryGetGeneralTableKey(key, out string tableKey))
        {
            return Main.Locale.Get(LocaleTable.General, tableKey);
        }
        if (string.Equals(Main.Locale?.LocaleCode, "ko", StringComparison.OrdinalIgnoreCase))
        {
            return GetKorean(key, fallback);
        }
        return fallback ?? string.Empty;
    }

    private static string GetKorean(string key, string fallback)
    {
        return key switch
        {
            LavaRushUIKeys.Title => "용암 탈출",
            LavaRushUIKeys.ScreenEventStart => "이벤트 시작",
            LavaRushUIKeys.ScreenDifficulty => "난이도 선택",
            LavaRushUIKeys.ScreenTutorial => "플레이 방법",
            LavaRushUIKeys.ScreenMatch => "용암에서 탈출하세요",
            LavaRushUIKeys.ScreenResult => "스테이지 결과",
            LavaRushUIKeys.ScreenComplete => "모든 스테이지 완료",
            LavaRushUIKeys.ScreenEventEnd => "이벤트 종료",
            LavaRushUIKeys.ActionStartEvent => "이벤트 시작",
            LavaRushUIKeys.ActionEasy => "쉬움",
            LavaRushUIKeys.ActionNormal => "보통",
            LavaRushUIKeys.ActionHard => "어려움",
            LavaRushUIKeys.ActionContinue => "계속",
            LavaRushUIKeys.ActionStartStage => "스테이지 시작",
            LavaRushUIKeys.ActionAddProgress => "+ 진행도",
            LavaRushUIKeys.ActionEvaluateStage => "타이머 판정",
            LavaRushUIKeys.ActionClaim => "보상 받기",
            LavaRushUIKeys.ActionRetry => "다시 도전",
            LavaRushUIKeys.ActionEndEvent => "이벤트 종료",
            LavaRushUIKeys.ActionClose => "닫기",
            LavaRushUIKeys.MessageStart => "활성 시간이 끝나기 전에 이벤트를 시작하세요.",
            LavaRushUIKeys.MessageDifficulty => "난이도를 선택하면 이번 이벤트 동안 유지됩니다.",
            LavaRushUIKeys.MessageTutorial => "용암이 모든 발판에 도착하기 전에 진행도를 채우세요.",
            LavaRushUIKeys.MessageReady => "준비가 되면 다음 스테이지를 시작하세요.",
            LavaRushUIKeys.MessagePlaying => "제한 시간이 끝나기 전에 진행도를 획득하세요.",
            LavaRushUIKeys.MessageWin => "용암에서 탈출했습니다. 저장된 보상을 확인하세요.",
            LavaRushUIKeys.MessageLose => "용암이 따라잡았습니다. 결과를 확인하고 다시 도전하세요.",
            LavaRushUIKeys.MessageComplete => "모든 스테이지를 완료했습니다.",
            LavaRushUIKeys.MessageEventEnd => "이벤트 활성 시간이 종료되었습니다.",
            LavaRushUIKeys.FormatEventTime => "이벤트 {0}",
            LavaRushUIKeys.FormatStageTime => "스테이지 {0}",
            LavaRushUIKeys.FormatStage => "난이도 {0}  |  스테이지 {1} / {2}",
            LavaRushUIKeys.FormatProgress => "진행도 {0} / {1}",
            LavaRushUIKeys.FormatSeats => "남은 발판 {0} / {1}",
            LavaRushUIKeys.FormatRank => "순위 {0}",
            LavaRushUIKeys.FormatReward => "{0} x{1}",
            LavaRushUIKeys.FormatProfile => "도전자: {0}",
            LavaRushUIKeys.StatusEventStarted => "이벤트를 시작했습니다.",
            LavaRushUIKeys.StatusEventUnavailable => "현재 이벤트를 시작할 수 없습니다.",
            LavaRushUIKeys.StatusTutorialComplete => "플레이 방법을 확인했습니다.",
            LavaRushUIKeys.StatusStageStarted => "스테이지를 시작했습니다.",
            LavaRushUIKeys.StatusStageUnavailable => "현재 스테이지를 시작할 수 없습니다.",
            LavaRushUIKeys.StatusProgressAdded => "진행도 {0}을 추가했습니다.",
            LavaRushUIKeys.StatusProgressUnavailable => "진행 중인 스테이지에서만 진행도를 추가할 수 있습니다.",
            LavaRushUIKeys.StatusStagePending => "아직 승리 또는 시간 종료 조건에 도달하지 않았습니다.",
            LavaRushUIKeys.StatusRewardClaimed => "보상 지급을 확인했습니다.",
            LavaRushUIKeys.StatusRewardUnavailable => "안전한 보상 서비스가 준비될 때까지 다시 시도할 수 있습니다.",
            LavaRushUIKeys.StatusResultCleared => "스테이지 결과를 확인했습니다.",
            LavaRushUIKeys.StatusEventEnded => "이벤트를 종료했습니다.",
            _ => fallback ?? string.Empty,
        };
    }
}

public sealed class CatDetectiveLavaRushAudio : ILavaRushUIAudio
{
    private readonly CatDetectiveLavaRushSettings _settings;

    public CatDetectiveLavaRushAudio(CatDetectiveLavaRushSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public void Play(string cue)
    {
        if (Main.Audio != null && _settings.TryGetSfx(cue, out SFXType sfx))
        {
            Main.Audio.PlaySFX(sfx);
        }
    }
}

public sealed class CatDetectiveLavaRushProfileProvider : ILavaRushUIProfileProvider
{
    private readonly CatDetectiveLavaRushSettings _settings;

    public CatDetectiveLavaRushProfileProvider(CatDetectiveLavaRushSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public LavaRushUIProfile GetProfile()
    {
        ProfilePrefs profile = Prefs.Get<ProfilePrefs>();
        string displayName = string.IsNullOrWhiteSpace(profile?.Nickname)
            ? _settings.FallbackPlayerName
            : profile.Nickname;
        return new LavaRushUIProfile(displayName, _settings.ProfileAccent);
    }
}
