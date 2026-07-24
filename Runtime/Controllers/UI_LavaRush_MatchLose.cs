using System;
using ActionFit.LavaRush.UI;
using UnityEngine;

/// <summary>Original match-lose controller identity.</summary>
public class UI_LavaRush_MatchLose : LavaRushControllerView
{
    [Serializable]
    public class Refs
    {
        public UI_Button btnMatchStart;
        public UI_Button btnMatchLater;
        public UI_Button btnClose;
        public UI_Text txtTimer;
        public UI_Text localizeDesc2;
    }

    public Refs refs = new();
    [SerializeField] private LavaRushControllerRefs controller = new();

    protected override LavaRushControllerRefs ControllerRefs => controller;
    protected override bool BindGenericActions => false;
    public override LavaRushControllerScreen Screen => LavaRushControllerScreen.MatchLose;

    protected override void OnBound()
    {
        refs?.btnMatchStart?.AddListener(OnClickMatchStart);
        refs?.btnMatchLater?.AddListener(OnClickMatchLater);
        refs?.btnClose?.AddListener(OnClickClose);
    }

    protected override void OnDidOpen()
    {
        Owner?.PlayAudio(LavaRushAudioCue.MatchLose);
    }

    protected override void OnDestroy()
    {
        refs?.btnMatchStart?.RemoveListener(OnClickMatchStart);
        refs?.btnMatchLater?.RemoveListener(OnClickMatchLater);
        refs?.btnClose?.RemoveListener(OnClickClose);
        base.OnDestroy();
    }

    private void OnClickMatchStart() => Owner?.StartNextOrRetryStage();
    private void OnClickMatchLater() => Owner?.ReturnToMatch();
    private void OnClickClose() => Owner?.CloseActiveScreen();
}
