using System;
using ActionFit.LavaRush.UI;
using Project.Scripts.Game.UI;
using ReferenceBinding;
using UnityEngine;

/// <summary>Original event-end controller identity backed by the engine pending-end state.</summary>
public class UI_LavaRush_EventEnd : LavaRushControllerView, IPopup
{
    [Serializable]
    public class Refs
    {
        public UI_Button btnConfirm;

        [SerializeField, RequiredReference("LAVA_RUSH_EVENT_END_TIMER_MISSING")]
        [AutoWireChild("Txt_Timer")]
        private UI_Text txtTimer;

        public UI_Text TxtTimer => txtTimer;
    }

    public Refs refs = new();
    [SerializeField] private LavaRushControllerRefs controller = new();

    protected override LavaRushControllerRefs ControllerRefs => controller;
    protected override bool BindGenericActions => false;
    public override LavaRushControllerScreen Screen => LavaRushControllerScreen.EventEnd;
    public bool CanOpen => false;

    protected override void OnBound()
    {
        refs?.btnConfirm?.AddListener(OnClickConfirm);
    }

    protected override void OnDestroy()
    {
        refs?.btnConfirm?.RemoveListener(OnClickConfirm);
        base.OnDestroy();
    }


    private void OnClickConfirm() => Owner?.ConfirmEventEnd();
}
