using System;
using ReferenceBinding;
using UnityEngine;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Typed package binder for one authored Lava Rush reward cell.</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("ActionFit/Lava Rush Reward Cell View")]
    public sealed class LavaRushRewardCellView : MonoBehaviour
    {
        [Serializable]
        public sealed class Refs
        {
            [SerializeField, RequiredReference("LAVA_RUSH_UI_REWARD_ICON_MISSING"), AutoWireChild("Img_Icon")]
            private UI_Image icon;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_REWARD_AMOUNT_MISSING"), AutoWireChild("Txt_Count")]
            private UI_Text amount;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_REWARD_INFO_MISSING"), AutoWireChild("Btn_Info")]
            private UI_Button info;

            public UI_Image Icon => icon;
            public UI_Text Amount => amount;
            public UI_Button Info => info;
            public bool IsComplete => icon != null && amount != null && info != null;
        }

        [SerializeField] private Refs refs = new();

        private Sprite _authoredIcon;
        private Action _infoRequested;
        private bool _listenerRegistered;

        public bool IsComplete => refs?.IsComplete == true;

        internal void SetPresentation(LavaRushRewardPresentation presentation)
        {
            if (presentation == null)
                throw new ArgumentNullException(nameof(presentation));
            if (!IsComplete)
            {
                Debug.LogError(
                    "[LavaRushRewardCellView] SetPresentation: serialized references are incomplete.",
                    this);
                return;
            }

            RegisterListener();
            _infoRequested = presentation.InfoRequested;
            gameObject.SetActive(true);
            refs.Icon.Sprite = presentation.Icon != null ? presentation.Icon : _authoredIcon;
            refs.Amount.gameObject.SetActive(presentation.ShowAmount);
            refs.Amount.Text = presentation.ShowAmount ? presentation.AmountText : string.Empty;
            refs.Info.gameObject.SetActive(
                presentation.ShowInfo && presentation.InfoRequested != null);
        }

        internal void Hide()
        {
            _infoRequested = null;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _infoRequested = null;
            if (_listenerRegistered)
                refs?.Info?.RemoveListener(HandleInfoRequested);
        }

        private void RegisterListener()
        {
            if (_listenerRegistered)
                return;

            _authoredIcon = refs.Icon.Sprite;
            refs.Info.AddListener(HandleInfoRequested);
            _listenerRegistered = true;
        }

        private void HandleInfoRequested() => _infoRequested?.Invoke();

    }
}
