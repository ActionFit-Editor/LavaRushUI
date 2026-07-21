using System;
using ReferenceBinding;
using TMPro;
using UnityEngine;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Package-owned serialized references for the production Lava Rush access icon.</summary>
    [AddComponentMenu("ActionFit/Lava Rush Access Icon View")]
    public sealed class LavaRushAccessIconView : MonoBehaviour
    {
        [Serializable]
        public sealed class Refs
        {
            [SerializeField, RequiredReference("LAVA_RUSH_UI_ICON_TIMER_MISSING"), AutoWireChild("Txt_Timer")]
            private UI_Text timerText;

            public UI_Text TimerText => timerText;
        }

        [SerializeField] private Refs refs = new();

        public TextMeshProUGUI TimerText => refs?.TimerText?.TMP as TextMeshProUGUI;

#if UNITY_EDITOR
        private void OnValidate()
        {
            ReferenceBindingRequests.Enqueue(this);
        }
#endif
    }
}
