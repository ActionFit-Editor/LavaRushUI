using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ActionFit.LavaRush.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Package-owned in-game progress cell with explicit access and progress inputs.</summary>
public class UI_LavaRush_Cell : MonoBehaviour, ILavaRushProgressView
{
    [Serializable]
    public class Refs
    {
        public TextMeshProUGUI txtTimer;
        public TextMeshProUGUI txtStatus;
        public Image imgStatusGauge;
        public UI_Image imgTargetProgress;
        public ScalePulse animIndicator;
        public UI_Rect rectRemainText;
        public UI_Text txtRemainCount;
        [Min(0f)] public float animDuration = 0.3f;

        public TextMeshProUGUI TimerText => txtTimer;
        public TextMeshProUGUI StatusText => txtStatus;
        public Image StatusGauge => imgStatusGauge;
        public UI_Image TargetProgress => imgTargetProgress;
        public ScalePulse Indicator => animIndicator;
        public UI_Rect RemainTextRoot => rectRemainText;
        public UI_Text RemainCountText => txtRemainCount;
        public float AnimationDuration => animDuration;
    }

    private static readonly List<UI_LavaRush_Cell> Instances = new();

    [SerializeField] public Refs refs = new();

    private ILavaRushAccessService _access;
    private ILavaRushCountdownScheduler _countdown;
    private Func<(int Current, int Required, int Remain)> _progress;
    private Func<bool> _isStagePlaying;
    private ILavaRushAudio _audio = NullLavaRushAudio.Instance;
    private Action _onCountdownExpired;
    private CancellationTokenSource _countdownCancellation;
    private Coroutine _progressAnimation;
    private Coroutine _arrivalAnimation;
    private int _displayedValue;
    private int _displayedRemain = int.MinValue;
    private bool _wasPlaying;
    private float _remainRefreshElapsed;
    private Vector3 _targetBaseScale = Vector3.one;
    private bool _targetScaleCaptured;

    public static UI_LavaRush_Cell Primary
    {
        get
        {
            for (int index = 0; index < Instances.Count; index++)
                if (Instances[index] != null)
                    return Instances[index];
            return null;
        }
    }

    public UI_Image TargetProgress => refs?.TargetProgress;
    RectTransform ILavaRushProgressView.TargetProgress => TargetProgress?.RectTransform;
    public TextMeshProUGUI TimerText => refs?.TimerText;

    public void Initialize(
        ILavaRushAccessService access,
        Func<(int Current, int Required, int Remain)> progress,
        ILavaRushCountdownScheduler countdown = null,
        Func<bool> isStagePlaying = null,
        ILavaRushAudio audio = null,
        Action onCountdownExpired = null)
    {
        _access = access ?? throw new ArgumentNullException(nameof(access));
        _progress = progress ?? throw new ArgumentNullException(nameof(progress));
        _countdown = countdown;
        _isStagePlaying = isStagePlaying;
        _audio = audio ?? NullLavaRushAudio.Instance;
        _onCountdownExpired = onCountdownExpired;
        SyncImmediate();
        RestartCountdown();
    }

    public static void NotifyProgressArrived()
    {
        for (int index = Instances.Count - 1; index >= 0; index--)
        {
            if (Instances[index] == null)
                Instances.RemoveAt(index);
            else
                Instances[index].AnimateToTarget();
        }
    }

    void ILavaRushProgressView.NotifyProgressArrived() => AnimateToTarget();

    public void SyncImmediate()
    {
        StopProgressAnimation();
        bool playing = IsStagePlaying();
        (int current, int required, int remain) = ReadProgress(playing);
        _displayedValue = current;
        _wasPlaying = playing;
        ApplyProgress(_displayedValue, required);
        ApplyPlayingState(playing, remain);
    }

    public void PlayRewardArrive()
    {
        UI_Image target = TargetProgress;
        if (target == null)
            return;

        CaptureTargetScale(target.RectTransform);
        if (_arrivalAnimation != null)
            StopCoroutine(_arrivalAnimation);
        target.RectTransform.localScale = _targetBaseScale;
        _arrivalAnimation = StartCoroutine(AnimateArrival(target.RectTransform));
        _audio.Play(LavaRushAudioCue.RewardArrive);
    }

    public bool TryOpen()
    {
        if (_access?.IsEventActive != true || !_access.IsEventStarted)
            return false;

        _access.OpenContent();
        return true;
    }

    public void Open() => TryOpen();

    public void Tick(float deltaTime)
    {
        bool playing = IsStagePlaying();
        if (playing != _wasPlaying)
        {
            SyncImmediate();
            return;
        }

        if (!playing)
            return;

        _remainRefreshElapsed += Mathf.Max(0f, deltaTime);
        if (_remainRefreshElapsed < 0.25f)
            return;

        _remainRefreshElapsed = 0f;
        (int Current, int Required, int Remain) progress = ReadProgress(true);
        ApplyRemainCount(progress.Remain);
    }

    private void OnEnable()
    {
        if (!Instances.Contains(this))
            Instances.Add(this);
        CaptureTargetScale(TargetProgress?.RectTransform);
        SyncImmediate();
        RestartCountdown();
    }

    private void OnDisable()
    {
        Instances.Remove(this);
        StopCountdown();
        StopProgressAnimation();
        if (_arrivalAnimation != null)
        {
            StopCoroutine(_arrivalAnimation);
            _arrivalAnimation = null;
        }
        if (_targetScaleCaptured && TargetProgress != null)
            TargetProgress.RectTransform.localScale = _targetBaseScale;
    }

    private void RestartCountdown()
    {
        StopCountdown();
        if (_access == null || _countdown == null || TimerText == null)
            return;

        TimerText.text = "--:--:--";
        if (_access.EventRemainTime <= TimeSpan.Zero && _countdown.TryGetNow(out _))
        {
            _onCountdownExpired?.Invoke();
            return;
        }

        _countdownCancellation = new CancellationTokenSource();
        _countdown.Register(
            TimerText,
            _access.EventEndTime,
            _countdownCancellation.Token,
            _onCountdownExpired,
            LavaRushTimeText.FormatHourMinSec);
    }

    private void StopCountdown()
    {
        _countdownCancellation?.Cancel();
        _countdownCancellation?.Dispose();
        _countdownCancellation = null;
    }

    private bool IsStagePlaying() => _isStagePlaying?.Invoke() ?? false;

    private (int Current, int Required, int Remain) ReadProgress(bool playing)
    {
        (int current, int required, int remain) = _progress?.Invoke() ?? default;
        required = playing ? Mathf.Max(0, required) : 0;
        current = playing ? Mathf.Clamp(current, 0, required) : 0;
        return (current, required, Mathf.Max(0, remain));
    }

    private void AnimateToTarget()
    {
        bool playing = IsStagePlaying();
        (int current, int required, int remain) = ReadProgress(playing);
        _wasPlaying = playing;
        ApplyPlayingState(playing, remain);
        if (!playing || _displayedValue == current || !isActiveAndEnabled)
        {
            _displayedValue = current;
            ApplyProgress(_displayedValue, required);
            return;
        }

        StopProgressAnimation();
        _progressAnimation = StartCoroutine(AnimateProgress(_displayedValue, current, required));
    }

    private IEnumerator AnimateProgress(int start, int target, int required)
    {
        float duration = Mathf.Max(0.01f, refs?.AnimationDuration ?? 0.3f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - normalized, 3f);
            _displayedValue = Mathf.RoundToInt(Mathf.Lerp(start, target, eased));
            ApplyProgress(_displayedValue, required);
            yield return null;
        }

        _displayedValue = target;
        ApplyProgress(_displayedValue, required);
        _progressAnimation = null;
    }

    private IEnumerator AnimateArrival(RectTransform target)
    {
        const float halfDuration = 0.08f;
        float elapsed = 0f;
        while (elapsed < halfDuration && target != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / halfDuration);
            target.localScale = Vector3.Lerp(
                _targetBaseScale,
                _targetBaseScale * 1.05f,
                normalized);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration && target != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / halfDuration);
            target.localScale = Vector3.Lerp(
                _targetBaseScale * 1.05f,
                _targetBaseScale,
                normalized);
            yield return null;
        }

        if (target != null)
            target.localScale = _targetBaseScale;
        _arrivalAnimation = null;
    }

    private void ApplyProgress(int current, int required)
    {
        if (refs?.StatusText != null)
            refs.StatusText.text = $"{Mathf.Clamp(current, 0, required):00}/{required:00}";
        if (refs?.StatusGauge != null)
            refs.StatusGauge.fillAmount = required > 0
                ? Mathf.Clamp01(current / (float)required)
                : 0f;
    }

    private void ApplyPlayingState(bool playing, int remain)
    {
        if (refs?.Indicator != null)
            refs.Indicator.gameObject.SetActive(!playing);
        if (refs?.RemainTextRoot != null)
            refs.RemainTextRoot.gameObject.SetActive(playing);
        if (playing)
            ApplyRemainCount(remain);
        else
            _displayedRemain = int.MinValue;
    }

    private void ApplyRemainCount(int remain)
    {
        remain = Mathf.Max(0, remain);
        if (refs?.RemainCountText == null || remain == _displayedRemain)
            return;

        _displayedRemain = remain;
        refs.RemainCountText.Text = remain.ToString();
    }

    private void CaptureTargetScale(RectTransform target)
    {
        if (_targetScaleCaptured || target == null)
            return;

        _targetBaseScale = target.localScale;
        _targetScaleCaptured = true;
    }

    private void StopProgressAnimation()
    {
        if (_progressAnimation == null)
            return;

        StopCoroutine(_progressAnimation);
        _progressAnimation = null;
    }
}
