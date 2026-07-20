using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Routes a package action through an original production button without changing its artwork.</summary>
    [AddComponentMenu("ActionFit/Lava Rush Action Target")]
    public sealed class LavaRushActionTarget : MonoBehaviour
    {
        [Serializable]
        public sealed class Refs
        {
            [SerializeField] private UI_Button foundationButton;
            [SerializeField] private Button uguiButton;
            [SerializeField] private UI_Text foundationLabel;
            [SerializeField] private Text uguiLabel;

            public UI_Button FoundationButton => foundationButton;
            public Button UGUIButton => uguiButton;
            public UI_Text FoundationLabel => foundationLabel;
            public Text UGUILabel => uguiLabel;
        }

        [SerializeField] private Refs refs = new();

        internal bool IsComplete => refs?.FoundationButton != null || refs?.UGUIButton != null;

        internal void AddListener(UnityAction listener)
        {
            refs?.FoundationButton?.AddListener(listener);
            refs?.UGUIButton?.onClick.AddListener(listener);
        }

        internal void RemoveListener(UnityAction listener)
        {
            refs?.FoundationButton?.RemoveListener(listener);
            refs?.UGUIButton?.onClick.RemoveListener(listener);
        }

        internal LavaRushUIAction Present(LavaRushUIButtonModel model)
        {
            model ??= LavaRushUIButtonModel.Hidden;
            gameObject.SetActive(model.Visible);
            refs?.FoundationButton?.SetInteractable(model.Interactable);
            if (refs?.UGUIButton != null)
            {
                refs.UGUIButton.interactable = model.Interactable;
            }

            if (refs?.FoundationLabel != null)
            {
                refs.FoundationLabel.Text = model.Label;
            }
            if (refs?.UGUILabel != null)
            {
                refs.UGUILabel.text = model.Label;
            }

            return model.Visible && model.Interactable ? model.Action : LavaRushUIAction.None;
        }
    }
}
