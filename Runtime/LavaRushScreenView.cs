using System;
using UnityEngine;
using UnityEngine.UI;

namespace ActionFit.LavaRush.UI
{
    internal enum LavaRushScreenRole
    {
        EventStart = 0,
        Difficulty = 1,
        Tutorial = 2,
        Match = 3,
        MatchWin = 4,
        MatchLose = 5,
        MatchEnd = 6,
        EventEnd = 7,
    }

    /// <summary>Package-authored screen binder used by the modular Lava Rush prefab set.</summary>
    [AddComponentMenu("ActionFit/Lava Rush Screen View")]
    public sealed class LavaRushScreenView : MonoBehaviour
    {
        [Serializable]
        public sealed class Refs
        {
            [SerializeField] private LavaRushUIViewReferences view = new();
            [SerializeField] private Image heroArtwork;
            [SerializeField] private Image rewardArtwork;

            public LavaRushUIViewReferences View => view ?? new LavaRushUIViewReferences();
            public Image HeroArtwork => heroArtwork;
            public Image RewardArtwork => rewardArtwork;
        }

        [Serializable]
        public sealed class Settings
        {
            [SerializeField] private LavaRushScreenRole role;

            internal LavaRushScreenRole Role => role;
        }

        [SerializeField] private Refs refs = new();
        [SerializeField] private Settings settings = new();

        private Action<LavaRushUIAction> _actionRequested;
        private LavaRushUIAction _primaryAction;
        private LavaRushUIAction _secondaryAction;
        private LavaRushUIAction _tertiaryAction;
        private bool _bound;

        internal RectTransform Panel => refs?.View.Panel;
        internal Image ProgressFill => refs?.View.ProgressFill;
        internal bool IsComplete => refs?.View.IsComplete == true;

        internal void Bind(Action<LavaRushUIAction> actionRequested)
        {
            if (_bound)
            {
                return;
            }

            _actionRequested = actionRequested;
            LavaRushUIViewReferences view = refs?.View;
            if (view == null || !view.IsComplete)
            {
                Debug.LogError($"[LavaRushScreenView] Bind: incomplete references on {name}");
                return;
            }

            view.PrimaryButton.onClick.AddListener(HandlePrimaryAction);
            view.SecondaryButton.onClick.AddListener(HandleSecondaryAction);
            view.TertiaryButton.onClick.AddListener(HandleTertiaryAction);
            _bound = true;
        }

        internal bool Present(
            LavaRushUIViewModel model,
            LavaRushUITheme theme,
            LavaRushUIProfile profile,
            string profileText,
            string title,
            string screenTitle,
            string message,
            string status,
            string timer,
            string progress,
            string reward)
        {
            bool visible = Matches(model);
            gameObject.SetActive(visible);
            if (!IsComplete)
            {
                return false;
            }

            LavaRushUIViewReferences view = refs.View;
            view.TitleText.text = title;
            view.ScreenText.text = screenTitle;
            view.ProfileText.text = profileText;
            view.ProfileText.color = profile?.AccentColor ?? theme.Lava;
            view.MessageText.text = message;
            view.StatusText.text = status;
            view.TimerText.text = timer;
            view.ProgressText.text = progress;
            view.ProgressFill.rectTransform.anchorMax = new Vector2(model.ProgressRatio, 1f);
            view.RewardText.text = reward;

            ApplyTheme(view, theme);
            ConfigureButton(view.PrimaryButton, view.PrimaryButtonText, model.Primary, theme.Button, out _primaryAction);
            ConfigureButton(view.SecondaryButton, view.SecondaryButtonText, model.Secondary, theme.SecondaryButton, out _secondaryAction);
            ConfigureButton(view.TertiaryButton, view.TertiaryButtonText, model.Tertiary, theme.SecondaryButton, out _tertiaryAction);

            bool showRewardArtwork = model.Screen is LavaRushUIScreen.Result or LavaRushUIScreen.Complete;
            if (refs.HeroArtwork != null)
            {
                refs.HeroArtwork.gameObject.SetActive(!showRewardArtwork);
            }
            if (refs.RewardArtwork != null)
            {
                refs.RewardArtwork.gameObject.SetActive(showRewardArtwork);
            }
            return visible;
        }

        internal void Unbind()
        {
            if (!_bound)
            {
                _actionRequested = null;
                return;
            }

            LavaRushUIViewReferences view = refs?.View;
            if (view != null)
            {
                view.PrimaryButton?.onClick.RemoveListener(HandlePrimaryAction);
                view.SecondaryButton?.onClick.RemoveListener(HandleSecondaryAction);
                view.TertiaryButton?.onClick.RemoveListener(HandleTertiaryAction);
            }
            _actionRequested = null;
            _bound = false;
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private bool Matches(LavaRushUIViewModel model)
        {
            return settings.Role switch
            {
                LavaRushScreenRole.EventStart => model.Screen == LavaRushUIScreen.EventStart,
                LavaRushScreenRole.Difficulty => model.Screen == LavaRushUIScreen.Difficulty,
                LavaRushScreenRole.Tutorial => model.Screen == LavaRushUIScreen.Tutorial,
                LavaRushScreenRole.Match => model.Screen == LavaRushUIScreen.Match,
                LavaRushScreenRole.MatchWin => model.Screen == LavaRushUIScreen.Result && model.Result == LavaRushResult.Win,
                LavaRushScreenRole.MatchLose => model.Screen == LavaRushUIScreen.Result && model.Result != LavaRushResult.Win,
                LavaRushScreenRole.MatchEnd => model.Screen == LavaRushUIScreen.Complete,
                LavaRushScreenRole.EventEnd => model.Screen == LavaRushUIScreen.EventEnd,
                _ => false,
            };
        }

        private static void ApplyTheme(LavaRushUIViewReferences view, LavaRushUITheme theme)
        {
            if (view.Backdrop != null)
            {
                view.Backdrop.color = Color.white;
            }
            if (view.Panel != null && view.Panel.TryGetComponent(out Image panelImage))
            {
                panelImage.color = theme.Panel;
            }
            if (view.Accent != null)
            {
                view.Accent.color = theme.PanelAccent;
            }

            view.TitleText.color = theme.Text;
            view.ScreenText.color = theme.Lava;
            view.MessageText.color = theme.Text;
            view.StatusText.color = theme.SecondaryText;
            view.TimerText.color = theme.SecondaryText;
            view.ProgressText.color = theme.Text;
            view.RewardText.color = theme.Text;
            view.ProgressTrack.color = theme.ProgressTrack;
            view.ProgressFill.color = theme.Lava;
        }

        private static void ConfigureButton(
            Button button,
            Text label,
            LavaRushUIButtonModel model,
            Color color,
            out LavaRushUIAction action)
        {
            model ??= LavaRushUIButtonModel.Hidden;
            button.gameObject.SetActive(model.Visible);
            button.interactable = model.Interactable;
            label.text = model.Label;
            label.color = Color.white;
            action = model.Visible && model.Interactable ? model.Action : LavaRushUIAction.None;

            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.16f);
            colors.disabledColor = new Color(color.r, color.g, color.b, 0.38f);
            button.colors = colors;
            if (button.targetGraphic is Image image)
            {
                image.color = color;
            }
        }

        private void HandlePrimaryAction() => RequestAction(_primaryAction);
        private void HandleSecondaryAction() => RequestAction(_secondaryAction);
        private void HandleTertiaryAction() => RequestAction(_tertiaryAction);

        private void RequestAction(LavaRushUIAction action)
        {
            if (action != LavaRushUIAction.None)
            {
                _actionRequested?.Invoke(action);
            }
        }
    }
}
