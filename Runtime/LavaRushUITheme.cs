using System;
using UnityEngine;
using UnityEngine.UI;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Defines the package-owned colors used by the neutral Lava Rush presentation.</summary>
    [Serializable]
    public sealed class LavaRushUITheme
    {
        [SerializeField] private Color backdrop = new Color(0.08f, 0.09f, 0.14f, 0.94f);
        [SerializeField] private Color panel = new Color(0.16f, 0.18f, 0.25f, 1f);
        [SerializeField] private Color panelAccent = new Color(0.31f, 0.12f, 0.08f, 1f);
        [SerializeField] private Color lava = new Color(1f, 0.34f, 0.08f, 1f);
        [SerializeField] private Color progressTrack = new Color(0.08f, 0.06f, 0.08f, 0.9f);
        [SerializeField] private Color text = new Color(1f, 0.96f, 0.88f, 1f);
        [SerializeField] private Color secondaryText = new Color(0.82f, 0.83f, 0.88f, 1f);
        [SerializeField] private Color button = new Color(0.96f, 0.36f, 0.12f, 1f);
        [SerializeField] private Color secondaryButton = new Color(0.29f, 0.31f, 0.4f, 1f);

        public LavaRushUITheme()
        {
        }

        public LavaRushUITheme(
            Color backdrop,
            Color panel,
            Color panelAccent,
            Color lava,
            Color progressTrack,
            Color text,
            Color secondaryText,
            Color button,
            Color secondaryButton)
        {
            this.backdrop = backdrop;
            this.panel = panel;
            this.panelAccent = panelAccent;
            this.lava = lava;
            this.progressTrack = progressTrack;
            this.text = text;
            this.secondaryText = secondaryText;
            this.button = button;
            this.secondaryButton = secondaryButton;
        }

        public Color Backdrop => backdrop;
        public Color Panel => panel;
        public Color PanelAccent => panelAccent;
        public Color Lava => lava;
        public Color ProgressTrack => progressTrack;
        public Color Text => text;
        public Color SecondaryText => secondaryText;
        public Color Button => button;
        public Color SecondaryButton => secondaryButton;
    }

    /// <summary>Stores one reusable Lava Rush UI theme without mutable session state.</summary>
    [CreateAssetMenu(fileName = "LavaRushUITheme", menuName = "ActionFit/Lava Rush/UI Theme")]
    public sealed class LavaRushUIThemeAsset : ScriptableObject
    {
        [SerializeField] private LavaRushUITheme theme = new();

        public LavaRushUITheme Theme => theme ?? new LavaRushUITheme();
    }

    /// <summary>Defines standalone presentation labels, timing, and demo action values.</summary>
    [Serializable]
    public sealed class LavaRushUIConfig
    {
        [SerializeField] private string title = "Lava Rush";
        [SerializeField] private int demoProgressAmount = 25;
        [SerializeField] private float refreshIntervalSeconds = 0.25f;
        [SerializeField] private float transitionDurationSeconds = 0.24f;
        [SerializeField] private float progressPulseDurationSeconds = 0.28f;
        [SerializeField] private bool showDemoActions = true;

        public string Title => string.IsNullOrWhiteSpace(title) ? "Lava Rush" : title;
        public int DemoProgressAmount => Math.Max(1, demoProgressAmount);
        public float RefreshIntervalSeconds => Mathf.Max(0.05f, refreshIntervalSeconds);
        public float TransitionDurationSeconds => Mathf.Max(0f, transitionDurationSeconds);
        public float ProgressPulseDurationSeconds => Mathf.Max(0f, progressPulseDurationSeconds);
        public bool ShowDemoActions => showDemoActions;
    }

    /// <summary>Optional Inspector-authored references for a custom presentation prefab.</summary>
    [Serializable]
    public sealed class LavaRushUIViewReferences
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text screenText;
        [SerializeField] private Text profileText;
        [SerializeField] private Text messageText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text timerText;
        [SerializeField] private Image progressTrack;
        [SerializeField] private Image progressFill;
        [SerializeField] private Text progressText;
        [SerializeField] private Text rewardText;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Text primaryButtonText;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private Text secondaryButtonText;
        [SerializeField] private Button tertiaryButton;
        [SerializeField] private Text tertiaryButtonText;

        public RectTransform Panel => panel;
        public Text TitleText => titleText;
        public Text ScreenText => screenText;
        public Text ProfileText => profileText;
        public Text MessageText => messageText;
        public Text StatusText => statusText;
        public Text TimerText => timerText;
        public Image ProgressTrack => progressTrack;
        public Image ProgressFill => progressFill;
        public Text ProgressText => progressText;
        public Text RewardText => rewardText;
        public Button PrimaryButton => primaryButton;
        public Text PrimaryButtonText => primaryButtonText;
        public Button SecondaryButton => secondaryButton;
        public Text SecondaryButtonText => secondaryButtonText;
        public Button TertiaryButton => tertiaryButton;
        public Text TertiaryButtonText => tertiaryButtonText;

        public bool IsComplete => panel != null
            && titleText != null
            && screenText != null
            && profileText != null
            && messageText != null
            && statusText != null
            && timerText != null
            && progressTrack != null
            && progressFill != null
            && progressText != null
            && rewardText != null
            && primaryButton != null
            && primaryButtonText != null
            && secondaryButton != null
            && secondaryButtonText != null
            && tertiaryButton != null
            && tertiaryButtonText != null;

    }
}
