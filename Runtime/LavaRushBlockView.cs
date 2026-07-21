using System;
using ReferenceBinding;
using UnityEngine;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Package-owned presentation and interaction boundary for a production Lava Rush block.</summary>
    [AddComponentMenu("ActionFit/Lava Rush Block View")]
    public sealed class LavaRushBlockView : MonoBehaviour
    {
        [Serializable]
        public sealed class Refs
        {
            [SerializeField, RequiredReference("LAVA_RUSH_UI_BLOCK_IMAGE_MISSING"), AutoWireChild("Img_Block")]
            private UI_Image blockImage;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_BLOCK_STAGE_COUNT_MISSING"), AutoWireChild("Txt_StageCount")]
            private UI_Text stageCountText;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_BLOCK_SEAT_COUNT_MISSING"), AutoWireChild("Txt_SeatCount")]
            private UI_Text seatCountText;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_BLOCK_SEAT_MASK_MISSING"), AutoWireChild("Mask_SeatPanel")]
            private UI_Mask seatPanelMask;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_BLOCK_REWARD_ICON_MISSING"), AutoWireChild("Img_Icon")]
            private UI_Image rewardIcon;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_BLOCK_REWARD_COUNT_MISSING"), AutoWireChild("Txt_Count")]
            private UI_Text rewardCountText;

            [SerializeField, RequiredReference("LAVA_RUSH_UI_BLOCK_REWARD_INFO_MISSING"), AutoWireChild("Btn_Info")]
            private UI_Button rewardInfoButton;

            public UI_Image BlockImage => blockImage;
            public UI_Text StageCountText => stageCountText;
            public UI_Text SeatCountText => seatCountText;
            public UI_Mask SeatPanelMask => seatPanelMask;
            public UI_Image RewardIcon => rewardIcon;
            public UI_Text RewardCountText => rewardCountText;
            public UI_Button RewardInfoButton => rewardInfoButton;

            public bool IsComplete => blockImage != null
                && stageCountText != null
                && seatCountText != null
                && seatPanelMask != null
                && rewardIcon != null
                && rewardCountText != null
                && rewardInfoButton != null;
        }

        [SerializeField] private Refs refs = new();

        private Action _rewardInfoRequested;

        public bool IsComplete => refs?.IsComplete == true;
        public RectTransform BlockRectTransform => refs?.BlockImage?.RectTransform;
        public RectTransform RewardCellTransform => refs?.RewardIcon?.transform.parent as RectTransform;
        public Vector3 RewardWorldPosition => RewardCellTransform != null
            ? RewardCellTransform.position
            : default;

        private void Awake()
        {
            refs?.RewardInfoButton?.AddListener(HandleRewardInfoRequested);
        }

        private void OnDestroy()
        {
            refs?.RewardInfoButton?.RemoveListener(HandleRewardInfoRequested);
        }

        public void SetBlockPosition(Vector2 anchoredPosition)
        {
            if (BlockRectTransform != null)
            {
                BlockRectTransform.anchoredPosition = anchoredPosition;
            }
        }

        public void SetRewardCellSize(Vector2 size)
        {
            if (RewardCellTransform != null)
            {
                RewardCellTransform.sizeDelta = size;
            }
        }

        public void SetStageCount(string value)
        {
            if (refs?.StageCountText != null)
            {
                refs.StageCountText.Text = value ?? string.Empty;
            }
        }

        public void SetSeatCount(string value)
        {
            if (refs?.SeatCountText != null)
            {
                refs.SeatCountText.Text = value ?? string.Empty;
            }
        }

        public void SetSeatCountVisible(bool visible)
        {
            refs?.SeatCountText?.gameObject.SetActive(visible);
        }

        public void CollapseSeatPanel()
        {
            if (refs?.SeatPanelMask == null)
            {
                return;
            }

            Vector2 size = refs.SeatPanelMask.RectTransform.sizeDelta;
            size.y = 0f;
            refs.SeatPanelMask.RectTransform.sizeDelta = size;
        }

        public void ExpandSeatPanel(float duration)
        {
            refs?.SeatPanelMask?.Expand(duration);
        }

        public void SetFinalRewardPresentation()
        {
            _rewardInfoRequested = null;
            SetRewardVisible(true);
            refs?.RewardCountText?.gameObject.SetActive(false);
            refs?.RewardInfoButton?.gameObject.SetActive(false);
        }

        public void SetRewardPresentation(
            Sprite icon,
            string amountText,
            bool showAmount,
            bool showInfo,
            Action infoRequested)
        {
            _rewardInfoRequested = infoRequested;
            SetRewardVisible(true);
            if (refs?.RewardIcon != null)
            {
                refs.RewardIcon.Sprite = icon;
            }
            if (refs?.RewardCountText != null)
            {
                refs.RewardCountText.gameObject.SetActive(showAmount);
                if (showAmount)
                {
                    refs.RewardCountText.Text = amountText ?? string.Empty;
                }
            }
            refs?.RewardInfoButton?.gameObject.SetActive(showInfo);
        }

        public void SetRewardVisible(bool visible)
        {
            RewardCellTransform?.gameObject.SetActive(visible);
        }

        private void HandleRewardInfoRequested()
        {
            _rewardInfoRequested?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ReferenceBindingRequests.Enqueue(this);
        }
#endif
    }
}
