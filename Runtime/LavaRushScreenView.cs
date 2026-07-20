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
            [SerializeField] private ProductionRefs production = new();
            [SerializeField] private Image heroArtwork;
            [SerializeField] private Image rewardArtwork;

            public LavaRushUIViewReferences View => view ?? new LavaRushUIViewReferences();
            public ProductionRefs Production => production ?? new ProductionRefs();
            public Image HeroArtwork => heroArtwork;
            public Image RewardArtwork => rewardArtwork;
        }

        [Serializable]
        public sealed class ProductionRefs
        {
            [SerializeField] private RectTransform panel;
            [SerializeField] private Image backdrop;
            [SerializeField] private UI_Text titleText;
            [SerializeField] private UI_Text screenText;
            [SerializeField] private UI_Text profileText;
            [SerializeField] private UI_Text messageText;
            [SerializeField] private UI_Text statusText;
            [SerializeField] private UI_Text timerText;
            [SerializeField] private Image progressTrack;
            [SerializeField] private Image progressFill;
            [SerializeField] private UI_Text progressText;
            [SerializeField] private UI_Text rewardText;
            [SerializeField] private LavaRushActionTarget primaryButton;
            [SerializeField] private LavaRushActionTarget secondaryButton;
            [SerializeField] private LavaRushActionTarget tertiaryButton;

            public RectTransform Panel => panel;
            public Image Backdrop => backdrop;
            public UI_Text TitleText => titleText;
            public UI_Text ScreenText => screenText;
            public UI_Text ProfileText => profileText;
            public UI_Text MessageText => messageText;
            public UI_Text StatusText => statusText;
            public UI_Text TimerText => timerText;
            public Image ProgressTrack => progressTrack;
            public Image ProgressFill => progressFill;
            public UI_Text ProgressText => progressText;
            public UI_Text RewardText => rewardText;
            public LavaRushActionTarget PrimaryButton => primaryButton;
            public LavaRushActionTarget SecondaryButton => secondaryButton;
            public LavaRushActionTarget TertiaryButton => tertiaryButton;

            public bool IsComplete => panel != null
                && (primaryButton == null || primaryButton.IsComplete)
                && (secondaryButton == null || secondaryButton.IsComplete)
                && (tertiaryButton == null || tertiaryButton.IsComplete);
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

        internal RectTransform Panel => refs?.Production.IsComplete == true
            ? refs.Production.Panel
            : refs?.View.Panel;
        internal Image ProgressFill => refs?.Production.IsComplete == true
            ? refs.Production.ProgressFill
            : refs?.View.ProgressFill;
        internal bool IsComplete => refs?.Production.IsComplete == true || refs?.View.IsComplete == true;

        internal void Bind(Action<LavaRushUIAction> actionRequested)
        {
            if (_bound)
            {
                return;
            }

            _actionRequested = actionRequested;
            if (!IsComplete)
            {
                Debug.LogError($"[LavaRushScreenView] Bind: incomplete references on {name}");
                return;
            }

            if (refs.Production.IsComplete)
            {
                refs.Production.PrimaryButton?.AddListener(HandlePrimaryAction);
                refs.Production.SecondaryButton?.AddListener(HandleSecondaryAction);
                refs.Production.TertiaryButton?.AddListener(HandleTertiaryAction);
            }
            else
            {
                LavaRushUIViewReferences view = refs.View;
                view.PrimaryButton.onClick.AddListener(HandlePrimaryAction);
                view.SecondaryButton.onClick.AddListener(HandleSecondaryAction);
                view.TertiaryButton.onClick.AddListener(HandleTertiaryAction);
            }
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

            if (refs.Production.IsComplete)
            {
                PresentProduction(
                    model,
                    profileText,
                    title,
                    screenTitle,
                    message,
                    status,
                    timer,
                    progress,
                    reward);
                return visible;
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

            if (refs?.Production.IsComplete == true)
            {
                refs.Production.PrimaryButton?.RemoveListener(HandlePrimaryAction);
                refs.Production.SecondaryButton?.RemoveListener(HandleSecondaryAction);
                refs.Production.TertiaryButton?.RemoveListener(HandleTertiaryAction);
            }
            else
            {
                LavaRushUIViewReferences view = refs?.View;
                view?.PrimaryButton?.onClick.RemoveListener(HandlePrimaryAction);
                view?.SecondaryButton?.onClick.RemoveListener(HandleSecondaryAction);
                view?.TertiaryButton?.onClick.RemoveListener(HandleTertiaryAction);
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

        private void PresentProduction(
            LavaRushUIViewModel model,
            string profileText,
            string title,
            string screenTitle,
            string message,
            string status,
            string timer,
            string progress,
            string reward)
        {
            ProductionRefs production = refs.Production;
            SetText(production.TitleText, title);
            SetText(production.ScreenText, screenTitle);
            SetText(production.ProfileText, profileText);
            SetText(production.MessageText, message);
            SetText(production.StatusText, status);
            SetText(production.TimerText, timer);
            SetText(production.ProgressText, progress);
            SetText(production.RewardText, reward);
            if (production.ProgressFill != null)
            {
                production.ProgressFill.fillAmount = model.ProgressRatio;
            }

            _primaryAction = production.PrimaryButton != null
                ? production.PrimaryButton.Present(model.Primary)
                : LavaRushUIAction.None;
            _secondaryAction = production.SecondaryButton != null
                ? production.SecondaryButton.Present(model.Secondary)
                : LavaRushUIAction.None;
            _tertiaryAction = production.TertiaryButton != null
                ? production.TertiaryButton.Present(model.Tertiary)
                : LavaRushUIAction.None;
        }

        private static void SetText(UI_Text target, string value)
        {
            if (target != null)
            {
                target.Text = value ?? string.Empty;
            }
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
