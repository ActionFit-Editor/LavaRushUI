using System;
using System.Threading;
using ActionFit.LavaRush.UI;
using TMPro;
using UnityEngine;

/// <summary>Package-owned access icon controller; project anchors bind the neutral access port.</summary>
public class UI_LavaRush_Icon : MonoBehaviour
{
    [Serializable]
    public class Refs
    {
        public TextMeshProUGUI txtTimer;

        public TextMeshProUGUI TimerText => txtTimer;
    }

    [SerializeField] public Refs refs = new();

    private ILavaRushAccessService _access;
    private ILavaRushCountdownScheduler _countdown;
    private Action _onCountdownExpired;
    private CancellationTokenSource _countdownCancellation;

    public TextMeshProUGUI TimerText => refs?.TimerText;

    public void Initialize(
        ILavaRushAccessService access,
        ILavaRushCountdownScheduler countdown = null,
        Action onCountdownExpired = null)
    {
        _access = access ?? throw new ArgumentNullException(nameof(access));
        _countdown = countdown;
        _onCountdownExpired = onCountdownExpired;
        RestartCountdown();
    }

    public bool TryOpen()
    {
        if (_access?.IsEventActive != true || !_access.IsEventStarted)
            return false;

        _access.OpenContent();
        return true;
    }

    public void Open() => TryOpen();

    private void OnEnable() => RestartCountdown();

    private void OnDisable() => StopCountdown();

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
}
