using System;
using ActionFit.LavaRush.UI;
using UnityEngine;

/// <summary>Original all-stages-complete controller identity.</summary>
public class UI_LavaRush_MatchEnd : LavaRushControllerView
{
    [Serializable]
    public class Refs
    {
        public UI_Button btnConfirm;
        public UI_Button btnClose;
    }

    public Refs refs = new();
    [SerializeField] private LavaRushControllerRefs controller = new();

    protected override LavaRushControllerRefs ControllerRefs => controller;
    protected override bool BindGenericActions => false;
    public override LavaRushControllerScreen Screen => LavaRushControllerScreen.MatchEnd;

    protected override void OnBound()
    {
        refs?.btnConfirm?.AddListener(OnClickConfirm);
        refs?.btnClose?.AddListener(OnClickClose);
    }

    protected override void OnDestroy()
    {
        refs?.btnConfirm?.RemoveListener(OnClickConfirm);
        refs?.btnClose?.RemoveListener(OnClickClose);
        base.OnDestroy();
    }

    private void OnClickConfirm() => Owner?.CloseActiveScreen();
    private void OnClickClose() => Owner?.CloseActiveScreen();
}
