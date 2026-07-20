using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Neutral UGUI presenter that renders immutable Lava Rush engine snapshots.</summary>
    [AddComponentMenu("ActionFit/Lava Rush Presentation")]
    public class LavaRushPresentation : MonoBehaviour
    {
        [Serializable]
        public sealed class Assets
        {
            [SerializeField] private LavaRushUIThemeAsset themeAsset;

            public LavaRushUIThemeAsset ThemeAsset => themeAsset;
        }

        [Serializable]
        public sealed class Settings
        {
            [SerializeField] private LavaRushUITheme theme = new();
            [SerializeField] private LavaRushUIConfig config = new();

            public LavaRushUITheme Theme => theme ?? new LavaRushUITheme();
            public LavaRushUIConfig Config => config ?? new LavaRushUIConfig();
        }

        [Serializable]
        public sealed class Refs
        {
            [SerializeField] private LavaRushUIViewReferences view = new();

            public LavaRushUIViewReferences View => view ?? new LavaRushUIViewReferences();
        }

        [SerializeField] private Assets assets = new();
        [SerializeField] private Settings settings = new();
        [SerializeField] private Refs refs = new();

        private ILavaRushUILocalizer _localizer;
        private ILavaRushUIAudio _audio;
        private ILavaRushUIRewardRenderer _rewardRenderer;
        private ILavaRushUIProfileProvider _profileProvider;
        private LavaRushUITheme _runtimeTheme;
        private RuntimeView _runtimeView;
        private LavaRushUIViewModel _currentModel;
        private LavaRushUIAction _primaryAction;
        private LavaRushUIAction _secondaryAction;
        private LavaRushUIAction _tertiaryAction;
        private Coroutine _screenAnimation;
        private Coroutine _progressAnimation;
        private Vector3 _panelBaselineScale = Vector3.one;
        private Vector3 _progressBaselineScale = Vector3.one;
        private bool _initialized;

        public event Action<LavaRushUIAction> ActionRequested;

        public LavaRushUITheme Theme => _runtimeTheme
            ?? (assets?.ThemeAsset != null ? assets.ThemeAsset.Theme : settings?.Theme ?? new LavaRushUITheme());
        public LavaRushUIConfig Config => settings?.Config ?? new LavaRushUIConfig();
        public LavaRushUIViewModel CurrentModel => _currentModel;
        public bool IsInitialized => _initialized;

        protected LavaRushUIViewReferences InspectorView => refs?.View;

        /// <summary>Applies a caller-owned theme before initialization without changing Inspector-authored data.</summary>
        public void ApplyThemeOverride(LavaRushUITheme theme)
        {
            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }
            if (_initialized)
            {
                throw new InvalidOperationException("A Lava Rush theme override must be applied before initialization.");
            }

            _runtimeTheme = theme;
        }

        /// <summary>Builds missing fallback UI and installs presentation services.</summary>
        public void Initialize(
            ILavaRushUILocalizer localizer = null,
            ILavaRushUIAudio audio = null,
            ILavaRushUIRewardRenderer rewardRenderer = null,
            ILavaRushUIProfileProvider profileProvider = null)
        {
            if (_initialized)
            {
                return;
            }

            _localizer = localizer ?? this as ILavaRushUILocalizer ?? PassthroughLavaRushUILocalizer.Instance;
            _audio = audio ?? this as ILavaRushUIAudio ?? NullLavaRushUIAudio.Instance;
            _rewardRenderer = rewardRenderer ?? this as ILavaRushUIRewardRenderer ?? TextLavaRushUIRewardRenderer.Instance;
            _profileProvider = profileProvider ?? this as ILavaRushUIProfileProvider ?? DefaultLavaRushUIProfileProvider.Instance;
            _runtimeTheme ??= ResolveDefaultTheme();
            LavaRushUIViewReferences inspectorView = refs?.View;
            _runtimeView = inspectorView != null && inspectorView.IsComplete
                ? RuntimeView.From(inspectorView)
                : BuildDefaultView();

            _panelBaselineScale = _runtimeView.Panel.localScale;
            _progressBaselineScale = _runtimeView.ProgressFill.rectTransform.localScale;
            _runtimeView.PrimaryButton.onClick.AddListener(HandlePrimaryAction);
            _runtimeView.SecondaryButton.onClick.AddListener(HandleSecondaryAction);
            _runtimeView.TertiaryButton.onClick.AddListener(HandleTertiaryAction);
            ApplyTheme();
            _initialized = true;
        }

        /// <summary>Renders one immutable engine-derived view model.</summary>
        public void Present(LavaRushUIViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            Initialize(_localizer, _audio);
            LavaRushUIScreen previousScreen = _currentModel?.Screen ?? model.Screen;
            int previousProgress = _currentModel?.Progress ?? model.Progress;
            bool screenChanged = _currentModel == null || previousScreen != model.Screen;
            bool progressIncreased = _currentModel != null && model.Progress > previousProgress;
            _currentModel = model;

            _runtimeView.TitleText.text = Localize(LavaRushUIKeys.Title, Config.Title);
            _runtimeView.ScreenText.text = GetScreenTitle(model.Screen);
            LavaRushUIProfile profile = _profileProvider.GetProfile() ?? DefaultLavaRushUIProfileProvider.Instance.GetProfile();
            _runtimeView.ProfileText.text = FormatLocalized(LavaRushUIKeys.FormatProfile, "Runner: {0}", profile.DisplayName);
            _runtimeView.ProfileText.color = profile.AccentColor;
            _runtimeView.MessageText.text = string.IsNullOrEmpty(model.Message)
                ? BuildDefaultMessage(model)
                : model.Message;
            _runtimeView.StatusText.text = BuildStatus(model);
            _runtimeView.TimerText.text = BuildTimer(model);
            _runtimeView.ProgressText.text = FormatLocalized(
                LavaRushUIKeys.FormatProgress,
                "Progress {0} / {1}",
                model.Progress,
                model.RequiredProgress);
            _runtimeView.ProgressFill.rectTransform.anchorMax = new Vector2(model.ProgressRatio, 1f);
            _runtimeView.RewardText.text = _rewardRenderer.Render(model.Rewards, _localizer);

            ConfigureButton(_runtimeView.PrimaryButton, _runtimeView.PrimaryButtonText, model.Primary, true, out _primaryAction);
            ConfigureButton(_runtimeView.SecondaryButton, _runtimeView.SecondaryButtonText, model.Secondary, false, out _secondaryAction);
            ConfigureButton(_runtimeView.TertiaryButton, _runtimeView.TertiaryButtonText, model.Tertiary, false, out _tertiaryAction);

            if (screenChanged)
            {
                OnScreenTransition(previousScreen, model.Screen);
                _audio.Play(LavaRushUIKeys.AudioScreen);
                StartAnimation(ref _screenAnimation, AnimateScreenTransition(previousScreen, model.Screen), ResetPanelBaseline);
            }
            if (progressIncreased)
            {
                _audio.Play(LavaRushUIKeys.AudioProgress);
                StartAnimation(ref _progressAnimation, AnimateProgressGain(previousProgress, model.Progress), ResetProgressBaseline);
            }
            if (screenChanged && model.Screen == LavaRushUIScreen.Complete)
            {
                _audio.Play(LavaRushUIKeys.AudioReward);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        protected virtual void OnScreenTransition(LavaRushUIScreen previous, LavaRushUIScreen current)
        {
        }

        protected virtual LavaRushUITheme ResolveDefaultTheme()
        {
            return assets?.ThemeAsset != null
                ? assets.ThemeAsset.Theme
                : settings?.Theme ?? new LavaRushUITheme();
        }

        protected virtual IEnumerator AnimateScreenTransition(LavaRushUIScreen previous, LavaRushUIScreen current)
        {
            ResetPanelBaseline();
            float duration = Config.TransitionDurationSeconds;
            if (duration <= 0f)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                float progress = EvaluateOutBack(elapsed / duration);
                _runtimeView.Panel.localScale = Vector3.LerpUnclamped(_panelBaselineScale * 0.92f, _panelBaselineScale, progress);
                yield return null;
            }
            ResetPanelBaseline();
        }

        private static float EvaluateOutBack(float progress)
        {
            const float overshoot = 1.70158f;
            float x = Mathf.Clamp01(progress) - 1f;
            return 1f + (overshoot + 1f) * x * x * x + overshoot * x * x;
        }

        protected virtual IEnumerator AnimateProgressGain(int previous, int current)
        {
            ResetProgressBaseline();
            float duration = Config.ProgressPulseDurationSeconds;
            if (duration <= 0f)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float pulse = Mathf.Sin(normalized * Mathf.PI) * 0.08f;
                _runtimeView.ProgressFill.rectTransform.localScale = _progressBaselineScale * (1f + pulse);
                yield return null;
            }
            ResetProgressBaseline();
        }

        protected virtual string GetScreenTitle(LavaRushUIScreen screen)
        {
            return screen switch
            {
                LavaRushUIScreen.EventStart => Localize(LavaRushUIKeys.ScreenEventStart, "Event Start"),
                LavaRushUIScreen.Difficulty => Localize(LavaRushUIKeys.ScreenDifficulty, "Choose Difficulty"),
                LavaRushUIScreen.Tutorial => Localize(LavaRushUIKeys.ScreenTutorial, "How To Play"),
                LavaRushUIScreen.Match => Localize(LavaRushUIKeys.ScreenMatch, "Escape The Lava"),
                LavaRushUIScreen.Result => Localize(LavaRushUIKeys.ScreenResult, "Stage Result"),
                LavaRushUIScreen.Complete => Localize(LavaRushUIKeys.ScreenComplete, "Rush Complete"),
                LavaRushUIScreen.EventEnd => Localize(LavaRushUIKeys.ScreenEventEnd, "Event End"),
                _ => screen.ToString(),
            };
        }

        private void OnDisable()
        {
            StopAnimation(ref _screenAnimation, ResetPanelBaseline);
            StopAnimation(ref _progressAnimation, ResetProgressBaseline);
        }

        private void OnDestroy()
        {
            if (_runtimeView == null)
            {
                return;
            }

            _runtimeView.PrimaryButton.onClick.RemoveListener(HandlePrimaryAction);
            _runtimeView.SecondaryButton.onClick.RemoveListener(HandleSecondaryAction);
            _runtimeView.TertiaryButton.onClick.RemoveListener(HandleTertiaryAction);
        }

        private void HandlePrimaryAction() => RequestAction(_primaryAction);
        private void HandleSecondaryAction() => RequestAction(_secondaryAction);
        private void HandleTertiaryAction() => RequestAction(_tertiaryAction);

        private void RequestAction(LavaRushUIAction action)
        {
            if (action != LavaRushUIAction.None)
            {
                ActionRequested?.Invoke(action);
            }
        }

        private void ConfigureButton(
            Button button,
            Text label,
            LavaRushUIButtonModel model,
            bool primary,
            out LavaRushUIAction action)
        {
            model ??= LavaRushUIButtonModel.Hidden;
            button.gameObject.SetActive(model.Visible);
            button.interactable = model.Interactable;
            label.text = model.Label;
            action = model.Visible && model.Interactable ? model.Action : LavaRushUIAction.None;

            ColorBlock colors = button.colors;
            colors.normalColor = primary ? Theme.Button : Theme.SecondaryButton;
            colors.highlightedColor = Color.Lerp(colors.normalColor, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(colors.normalColor, Color.black, 0.16f);
            colors.disabledColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, 0.38f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
            if (button.targetGraphic is Image image)
            {
                image.color = colors.normalColor;
            }
        }

        private void ApplyTheme()
        {
            if (_runtimeView.Backdrop != null)
            {
                _runtimeView.Backdrop.color = Theme.Backdrop;
            }
            if (_runtimeView.PanelImage != null)
            {
                _runtimeView.PanelImage.color = Theme.Panel;
            }
            if (_runtimeView.Accent != null)
            {
                _runtimeView.Accent.color = Theme.PanelAccent;
            }

            _runtimeView.TitleText.color = Theme.Text;
            _runtimeView.ScreenText.color = Theme.Lava;
            _runtimeView.MessageText.color = Theme.Text;
            _runtimeView.StatusText.color = Theme.SecondaryText;
            _runtimeView.TimerText.color = Theme.SecondaryText;
            _runtimeView.ProgressText.color = Theme.Text;
            _runtimeView.RewardText.color = Theme.Text;
            _runtimeView.ProgressTrack.color = Theme.ProgressTrack;
            _runtimeView.ProgressFill.color = Theme.Lava;
            _runtimeView.PrimaryButtonText.color = Theme.Text;
            _runtimeView.SecondaryButtonText.color = Theme.Text;
            _runtimeView.TertiaryButtonText.color = Theme.Text;
        }

        private RuntimeView BuildDefaultView()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            Image backdrop = CreateImage("Backdrop", canvasRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform panel = CreateRect("Panel", canvasRect);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(820f, 1240f);
            Image panelImage = panel.gameObject.AddComponent<Image>();

            Image accent = CreateImage(
                "Lava Accent",
                panel,
                new Vector2(0f, 0.88f),
                new Vector2(1f, 1f),
                Vector2.zero,
                Vector2.zero);
            accent.raycastTarget = false;

            Text title = CreateText("Title", panel, font, 58, TextAnchor.MiddleCenter, new Vector2(0f, 0.88f), new Vector2(1f, 0.99f));
            Text screen = CreateText("Screen", panel, font, 38, TextAnchor.MiddleCenter, new Vector2(0.07f, 0.8f), new Vector2(0.93f, 0.88f));
            Text profile = CreateText("Profile", panel, font, 24, TextAnchor.MiddleCenter, new Vector2(0.09f, 0.74f), new Vector2(0.91f, 0.8f));
            Text message = CreateText("Message", panel, font, 28, TextAnchor.MiddleCenter, new Vector2(0.09f, 0.62f), new Vector2(0.91f, 0.74f));
            Text status = CreateText("Status", panel, font, 24, TextAnchor.MiddleCenter, new Vector2(0.09f, 0.53f), new Vector2(0.91f, 0.63f));
            Text timer = CreateText("Timer", panel, font, 25, TextAnchor.MiddleCenter, new Vector2(0.09f, 0.47f), new Vector2(0.91f, 0.54f));

            Image progressTrack = CreateImage(
                "Progress Track",
                panel,
                new Vector2(0.12f, 0.4f),
                new Vector2(0.88f, 0.46f),
                Vector2.zero,
                Vector2.zero);
            progressTrack.raycastTarget = false;
            Image progressFill = CreateImage(
                "Progress Fill",
                progressTrack.rectTransform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            progressFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            progressFill.raycastTarget = false;
            Text progressText = CreateText("Progress Text", panel, font, 24, TextAnchor.MiddleCenter, new Vector2(0.12f, 0.4f), new Vector2(0.88f, 0.46f));
            Text reward = CreateText("Rewards", panel, font, 25, TextAnchor.UpperCenter, new Vector2(0.08f, 0.23f), new Vector2(0.92f, 0.38f));

            Button primaryButton = CreateButton("Primary", panel, font, new Vector2(0.08f, 0.08f), new Vector2(0.38f, 0.19f), out Text primaryText);
            Button secondaryButton = CreateButton("Secondary", panel, font, new Vector2(0.39f, 0.08f), new Vector2(0.69f, 0.19f), out Text secondaryText);
            Button tertiaryButton = CreateButton("Tertiary", panel, font, new Vector2(0.7f, 0.08f), new Vector2(0.92f, 0.19f), out Text tertiaryText);

            EnsureEventSystem(canvasObject.transform);
            return new RuntimeView(
                panel,
                backdrop,
                panelImage,
                accent,
                title,
                screen,
                profile,
                message,
                status,
                timer,
                progressTrack,
                progressFill,
                progressText,
                reward,
                primaryButton,
                primaryText,
                secondaryButton,
                secondaryText,
                tertiaryButton,
                tertiaryText);
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var child = new GameObject(name, typeof(RectTransform));
            RectTransform rect = child.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static Image CreateImage(
            string name,
            RectTransform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect.gameObject.AddComponent<Image>();
        }

        private static Text CreateText(
            string name,
            RectTransform parent,
            Font font,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(
            string name,
            RectTransform parent,
            Font font,
            Vector2 anchorMin,
            Vector2 anchorMax,
            out Text label)
        {
            Image image = CreateImage(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            label = CreateText("Label", image.rectTransform, font, 24, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            return button;
        }

        private static void EnsureEventSystem(Transform parent)
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(parent, false);
        }

        private string BuildDefaultMessage(LavaRushUIViewModel model)
        {
            return model.Screen switch
            {
                LavaRushUIScreen.EventStart => Localize(LavaRushUIKeys.MessageStart, "Start the event before the active window closes."),
                LavaRushUIScreen.Difficulty => Localize(LavaRushUIKeys.MessageDifficulty, "Choose Easy, Normal, or Hard. The engine pins that choice."),
                LavaRushUIScreen.Tutorial => Localize(LavaRushUIKeys.MessageTutorial, "Fill the progress bar before the lava reaches every seat."),
                LavaRushUIScreen.Match when model.StageRemaining > TimeSpan.Zero => Localize(LavaRushUIKeys.MessagePlaying, "Earn progress before the stage timer reaches zero."),
                LavaRushUIScreen.Match => Localize(LavaRushUIKeys.MessageReady, "Start the next stage when you are ready."),
                LavaRushUIScreen.Result when model.Result == LavaRushResult.Win => Localize(LavaRushUIKeys.MessageWin, "You escaped the lava. Confirm the saved reward."),
                LavaRushUIScreen.Result => Localize(LavaRushUIKeys.MessageLose, "The lava caught up. Confirm the result and retry."),
                LavaRushUIScreen.Complete => Localize(LavaRushUIKeys.MessageComplete, "Every stage is complete. Finish the event when ready."),
                LavaRushUIScreen.EventEnd => Localize(LavaRushUIKeys.MessageEventEnd, "The active event window has ended."),
                _ => string.Empty,
            };
        }

        private string BuildStatus(LavaRushUIViewModel model)
        {
            string difficulty = model.Difficulty switch
            {
                1 => "Easy",
                2 => "Normal",
                3 => "Hard",
                _ => "-",
            };
            string stage = FormatLocalized(
                LavaRushUIKeys.FormatStage,
                "Difficulty {0}  |  Stage {1} / {2}",
                difficulty,
                model.Stage,
                model.StageCount);
            string seats = FormatLocalized(
                LavaRushUIKeys.FormatSeats,
                "Seats {0} / {1}",
                model.OccupiedSeats,
                model.SeatCapacity);
            if (model.Screen == LavaRushUIScreen.Result && model.Result == LavaRushResult.Win)
            {
                seats += "  |  " + FormatLocalized(LavaRushUIKeys.FormatRank, "Rank {0}", model.Rank);
            }
            return stage + "\n" + seats;
        }

        private string BuildTimer(LavaRushUIViewModel model)
        {
            string eventTime = FormatLocalized(
                LavaRushUIKeys.FormatEventTime,
                "Event {0}",
                FormatTime(model.EventRemaining));
            if (model.StageRemaining <= TimeSpan.Zero)
            {
                return eventTime;
            }
            return eventTime + "  |  " + FormatLocalized(
                LavaRushUIKeys.FormatStageTime,
                "Stage {0}",
                FormatTime(model.StageRemaining));
        }

        private string Localize(string key, string fallback)
        {
            return _localizer?.Get(key, fallback) ?? fallback ?? string.Empty;
        }

        private string FormatLocalized(string key, string fallback, params object[] arguments)
        {
            string format = Localize(key, fallback);
            try
            {
                return string.Format(format, arguments);
            }
            catch (FormatException)
            {
                return string.Format(fallback, arguments);
            }
        }

        private static string FormatTime(TimeSpan time)
        {
            int totalHours = time.TotalHours >= int.MaxValue ? int.MaxValue : Math.Max(0, (int)time.TotalHours);
            return totalHours > 0
                ? $"{totalHours:00}:{time.Minutes:00}:{time.Seconds:00}"
                : $"{time.Minutes:00}:{time.Seconds:00}";
        }

        private void StartAnimation(ref Coroutine field, IEnumerator animation, Action reset)
        {
            StopAnimation(ref field, reset);
            if (!Application.isPlaying)
            {
                return;
            }
            field = StartCoroutine(animation);
        }

        private void StopAnimation(ref Coroutine field, Action reset)
        {
            if (field != null)
            {
                StopCoroutine(field);
                field = null;
            }
            reset?.Invoke();
        }

        private void ResetPanelBaseline()
        {
            if (_runtimeView != null)
            {
                _runtimeView.Panel.localScale = _panelBaselineScale;
            }
        }

        private void ResetProgressBaseline()
        {
            if (_runtimeView != null)
            {
                _runtimeView.ProgressFill.rectTransform.localScale = _progressBaselineScale;
            }
        }

        private sealed class RuntimeView
        {
            public RuntimeView(
                RectTransform panel,
                Image backdrop,
                Image panelImage,
                Image accent,
                Text titleText,
                Text screenText,
                Text profileText,
                Text messageText,
                Text statusText,
                Text timerText,
                Image progressTrack,
                Image progressFill,
                Text progressText,
                Text rewardText,
                Button primaryButton,
                Text primaryButtonText,
                Button secondaryButton,
                Text secondaryButtonText,
                Button tertiaryButton,
                Text tertiaryButtonText)
            {
                Panel = panel;
                Backdrop = backdrop;
                PanelImage = panelImage;
                Accent = accent;
                TitleText = titleText;
                ScreenText = screenText;
                ProfileText = profileText;
                MessageText = messageText;
                StatusText = statusText;
                TimerText = timerText;
                ProgressTrack = progressTrack;
                ProgressFill = progressFill;
                ProgressText = progressText;
                RewardText = rewardText;
                PrimaryButton = primaryButton;
                PrimaryButtonText = primaryButtonText;
                SecondaryButton = secondaryButton;
                SecondaryButtonText = secondaryButtonText;
                TertiaryButton = tertiaryButton;
                TertiaryButtonText = tertiaryButtonText;
            }

            public RectTransform Panel { get; }
            public Image Backdrop { get; }
            public Image PanelImage { get; }
            public Image Accent { get; }
            public Text TitleText { get; }
            public Text ScreenText { get; }
            public Text ProfileText { get; }
            public Text MessageText { get; }
            public Text StatusText { get; }
            public Text TimerText { get; }
            public Image ProgressTrack { get; }
            public Image ProgressFill { get; }
            public Text ProgressText { get; }
            public Text RewardText { get; }
            public Button PrimaryButton { get; }
            public Text PrimaryButtonText { get; }
            public Button SecondaryButton { get; }
            public Text SecondaryButtonText { get; }
            public Button TertiaryButton { get; }
            public Text TertiaryButtonText { get; }

            public static RuntimeView From(LavaRushUIViewReferences view)
            {
                Image panelImage = view.Panel.GetComponent<Image>();
                return new RuntimeView(
                    view.Panel,
                    view.Backdrop,
                    panelImage,
                    view.Accent,
                    view.TitleText,
                    view.ScreenText,
                    view.ProfileText,
                    view.MessageText,
                    view.StatusText,
                    view.TimerText,
                    view.ProgressTrack,
                    view.ProgressFill,
                    view.ProgressText,
                    view.RewardText,
                    view.PrimaryButton,
                    view.PrimaryButtonText,
                    view.SecondaryButton,
                    view.SecondaryButtonText,
                    view.TertiaryButton,
                    view.TertiaryButtonText);
            }
        }
    }
}
