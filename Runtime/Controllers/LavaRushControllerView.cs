using System;
using ActionFit.LavaRush.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ActionFit.LavaRush.UI
{
    [Serializable]
    public class LavaRushControllerRefs
    {
        [SerializeField] private LavaRushUIViewReferences view = new();
        [SerializeField] private LavaRushProductionRefs production = new();
        [SerializeField] private Image heroArtwork;
        [SerializeField] private Image rewardArtwork;

        public LavaRushUIViewReferences View => view ?? new LavaRushUIViewReferences();
        public LavaRushProductionRefs Production => production ?? new LavaRushProductionRefs();
        public Image HeroArtwork => heroArtwork;
        public Image RewardArtwork => rewardArtwork;
    }

    [Serializable]
    public sealed class LavaRushProductionRefs
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
    }

    [Serializable]
    public sealed class LavaRushControllerSettings
    {
        [SerializeField] private int role;

        public int LegacyRole => role;
    }

    /// <summary>Shared rendering and callback plumbing; concrete scripts retain production identities.</summary>
    public abstract class LavaRushControllerView : ViewController
    {
        [SerializeField] private LavaRushControllerSettings settings = new();

        private global::UI_LavaRush _owner;
        private LavaRushUIAction _primaryAction;
        private LavaRushUIAction _secondaryAction;
        private LavaRushUIAction _tertiaryAction;
        private bool _buttonsBound;

        protected abstract LavaRushControllerRefs ControllerRefs { get; }
        public abstract LavaRushControllerScreen Screen { get; }

        internal global::UI_LavaRush Owner => _owner;
        internal LavaRushControllerSettings LegacySettings => settings;

        internal void Bind(global::UI_LavaRush owner)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            _owner = owner;
            if (_buttonsBound)
                return;

            LavaRushProductionRefs production = ControllerRefs?.Production;
            if (BindGenericActions)
            {
                production?.PrimaryButton?.AddListener(HandlePrimary);
                production?.SecondaryButton?.AddListener(HandleSecondary);
                production?.TertiaryButton?.AddListener(HandleTertiary);
            }
            OnBound();
            _buttonsBound = true;
        }

        internal void Present(LavaRushControllerSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            LavaRushProductionRefs production = ControllerRefs?.Production;
            if (production == null)
                return;

            SetText(production.TitleText, snapshot.Title);
            SetText(production.ScreenText, ScreenLabel(snapshot.Screen));
            SetText(production.MessageText, snapshot.Message);
            SetText(production.StatusText, Status(snapshot));
            SetText(production.TimerText, LavaRushTimeText.FormatDefault(
                snapshot.Screen == LavaRushControllerScreen.Match
                    ? snapshot.StageRemaining
                    : snapshot.EventRemaining));
            SetText(production.ProgressText, $"{snapshot.Progress} / {snapshot.RequiredProgress}");
            SetText(production.RewardText, _owner?.RenderRewards(snapshot.Rewards));
            SetText(production.ProfileText, _owner?.PlayerProfile.DisplayName);

            if (production.ProgressFill != null)
                production.ProgressFill.fillAmount = snapshot.ProgressRatio;

            _primaryAction = production.PrimaryButton?.Present(snapshot.Primary) ?? LavaRushUIAction.None;
            _secondaryAction = production.SecondaryButton?.Present(snapshot.Secondary) ?? LavaRushUIAction.None;
            _tertiaryAction = production.TertiaryButton?.Present(snapshot.Tertiary) ?? LavaRushUIAction.None;

            bool rewardScreen = snapshot.Screen is LavaRushControllerScreen.MatchWin
                or LavaRushControllerScreen.MatchEnd;
            if (ControllerRefs.HeroArtwork != null)
                ControllerRefs.HeroArtwork.gameObject.SetActive(!rewardScreen);
            if (ControllerRefs.RewardArtwork != null)
                ControllerRefs.RewardArtwork.gameObject.SetActive(rewardScreen);
        }

        internal void Activate(LavaRushControllerSnapshot snapshot, bool screenChanged)
        {
            Present(snapshot);
            if (screenChanged)
                OnShown();
        }

        protected override void OnWillOpen()
        {
            if (_owner != null)
                Present(_owner.CreateSnapshot(Screen));
        }

        /// <summary>Direct-controller callback hook used when the authored hierarchy is bound.</summary>
        protected virtual void OnBound()
        {
        }

        /// <summary>Direct-composition lifecycle hook invoked when this becomes the active screen.</summary>
        protected virtual void OnShown()
        {
        }

        /// <summary>Allows restored direct-controller buttons to own their authored callbacks.</summary>
        protected virtual bool BindGenericActions => true;

        protected override void OnDestroy()
        {
            if (_buttonsBound && BindGenericActions)
            {
                LavaRushProductionRefs production = ControllerRefs?.Production;
                production?.PrimaryButton?.RemoveListener(HandlePrimary);
                production?.SecondaryButton?.RemoveListener(HandleSecondary);
                production?.TertiaryButton?.RemoveListener(HandleTertiary);
            }

            base.OnDestroy();
        }

        private void HandlePrimary() => _owner?.HandleAction(_primaryAction);
        private void HandleSecondary() => _owner?.HandleAction(_secondaryAction);
        private void HandleTertiary() => _owner?.HandleAction(_tertiaryAction);

        private static string ScreenLabel(LavaRushControllerScreen screen)
        {
            return screen switch
            {
                LavaRushControllerScreen.EventStart => "Event Start",
                LavaRushControllerScreen.Difficulty => "Difficulty",
                LavaRushControllerScreen.Tutorial => "Tutorial",
                LavaRushControllerScreen.Match => "Match",
                LavaRushControllerScreen.MatchWin => "Victory",
                LavaRushControllerScreen.MatchLose => "Try Again",
                LavaRushControllerScreen.MatchEnd => "Complete",
                LavaRushControllerScreen.EventEnd => "Event End",
                _ => string.Empty,
            };
        }

        private string Status(LavaRushControllerSnapshot snapshot)
        {
            return snapshot.Screen switch
            {
                LavaRushControllerScreen.Match =>
                    $"Stage {snapshot.Stage}/{snapshot.StageCount}  Rank {snapshot.Rank}",
                LavaRushControllerScreen.MatchWin => $"Rank {snapshot.Rank}",
                LavaRushControllerScreen.MatchLose => FormatRemainingStages(
                    Math.Max(0, snapshot.StageCount - snapshot.Stage)),
                _ => string.Empty,
            };
        }

        private string FormatRemainingStages(int remaining)
        {
            string format = _owner?.LocalizeText(
                LavaRushLocalizationKeys.MatchLoseRemaining,
                "{0} stages remaining") ?? "{0} stages remaining";
            try
            {
                return string.Format(format, remaining);
            }
            catch (FormatException)
            {
                return $"{remaining} stages remaining";
            }
        }

        private static void SetText(UI_Text target, string value)
        {
            if (target != null)
                target.Text = value ?? string.Empty;
        }
    }
}
