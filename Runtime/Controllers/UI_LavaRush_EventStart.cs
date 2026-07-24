using System;
using ActionFit.LavaRush.UI;
using Project.Scripts.Game.UI;
using UnityEngine;

/// <summary>Original event-start controller identity backed by the neutral controller context.</summary>
public class UI_LavaRush_EventStart : LavaRushControllerView, IPopup
{
    [Serializable]
    public class Refs
    {
        public UI_Button btnStart;
        public UI_Text txtTimer;
    }

    public Refs refs = new();
    [SerializeField] private LavaRushControllerRefs controller = new();

    protected override LavaRushControllerRefs ControllerRefs => controller;
    protected override bool BindGenericActions => false;
    public override LavaRushControllerScreen Screen => LavaRushControllerScreen.EventStart;
    public bool CanOpen => Owner != null
        && !Owner.Engine.IsEventStarted
        && !Owner.Engine.PendingEnd
        && Owner.Engine.IsEventDay;

    protected override void OnBound()
    {
        refs?.btnStart?.AddListener(OnClickStart);
    }

    protected override void OnDestroy()
    {
        refs?.btnStart?.RemoveListener(OnClickStart);
        base.OnDestroy();
    }

    private void OnClickStart() => Owner?.ConfirmEventStart();
}
