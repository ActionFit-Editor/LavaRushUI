using System;
using ReferenceBinding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Package-owned serialized references for the production Lava Rush in-game cell.</summary>
    [AddComponentMenu("ActionFit/Lava Rush In-Game Cell View")]
    public sealed class LavaRushInGameCellView : MonoBehaviour
    {
        [Serializable]
        public sealed class Refs
        {
            [SerializeField, RequiredReference("LAVA_RUSH_UI_CELL_TIMER_MISSING"), AutoWireChild("Txt_Timer")]
            private UI_Text timerText;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_CELL_STATUS_MISSING"), AutoWireChild("Txt_Status")]
            private UI_Text statusText;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_CELL_GAUGE_MISSING"), AutoWireChild("fill")]
            private Image statusGauge;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_CELL_TARGET_MISSING"), AutoWireChild("item")]
            private UI_Image targetProgress;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_CELL_INDICATOR_MISSING"), AutoWireChild("Indicator")]
            private ScalePulse indicator;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_CELL_REMAIN_ROOT_MISSING"), AutoWireChild("Rect_RemainText")]
            private UI_Rect remainTextRoot;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_CELL_REMAIN_COUNT_MISSING"), AutoWireChild("Txt_ReaminCount")]
            private UI_Text remainCountText;

            public UI_Text TimerText => timerText;
            public UI_Text StatusText => statusText;
            public Image StatusGauge => statusGauge;
            public UI_Image TargetProgress => targetProgress;
            public ScalePulse Indicator => indicator;
            public UI_Rect RemainTextRoot => remainTextRoot;
            public UI_Text RemainCountText => remainCountText;

            public bool IsComplete => timerText != null
                && statusText != null
                && statusGauge != null
                && targetProgress != null
                && indicator != null
                && remainTextRoot != null
                && remainCountText != null;
        }

        [Serializable]
        public sealed class Settings
        {
            [SerializeField, Min(0f)] private float animationDuration = 0.3f;

            public float AnimationDuration => animationDuration;
        }

        [SerializeField] private Refs refs = new();
        [SerializeField] private Settings settings = new();

        public TextMeshProUGUI TimerText => refs?.TimerText?.TMP as TextMeshProUGUI;
        public TextMeshProUGUI StatusText => refs?.StatusText?.TMP as TextMeshProUGUI;
        public Image StatusGauge => refs?.StatusGauge;
        public UI_Image TargetProgress => refs?.TargetProgress;
        public ScalePulse Indicator => refs?.Indicator;
        public UI_Rect RemainTextRoot => refs?.RemainTextRoot;
        public UI_Text RemainCountText => refs?.RemainCountText;
        public float AnimationDuration => settings?.AnimationDuration ?? 0.3f;
        public bool IsComplete => refs?.IsComplete == true;

#if UNITY_EDITOR
        private void OnValidate()
        {
            ReferenceBindingRequests.Enqueue(this);
        }
#endif
    }
}
