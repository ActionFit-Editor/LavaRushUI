using System;
using ActionFit.Content;
using ReferenceBinding;
using UnityEngine;

namespace ActionFit.LavaRush.UI
{
    /// <summary>
    /// Standalone composition root for the same restored controller family used by production.
    /// It never generates fallback screens or a second presentation hierarchy.
    /// </summary>
    [AddComponentMenu("ActionFit/Lava Rush Bootstrap")]
    public sealed class LavaRushBootstrap : MonoBehaviour
    {
        public const string DefaultDemoContentId = "lava-rush-ui-demo";

        [Serializable]
        public sealed class Assets
        {
            [SerializeField, RequiredReference("LAVA_RUSH_UI_CONTROLLER_PREFAB_MISSING")]
            private global::UI_LavaRush controllerPrefab;

            public global::UI_LavaRush ControllerPrefab => controllerPrefab;
        }

        [Serializable]
        public sealed class Settings
        {
            [SerializeField] private string contentId = DefaultDemoContentId;
            [SerializeField] private bool initializeOnStart = true;

            public string ContentId => string.IsNullOrWhiteSpace(contentId)
                ? DefaultDemoContentId
                : contentId.Trim();
            public bool InitializeOnStart => initializeOnStart;
        }

        [SerializeField] private Assets assets = new();
        [SerializeField] private Settings settings = new();

        private LavaRushEngine _engine;
        private global::UI_LavaRush _controller;
        private LavaRushDemoClock _demoClock;
        private bool _ownsController;

        public event Action CloseRequested;

        public LavaRushEngine Engine => _engine;
        public global::UI_LavaRush Controller => _controller;
        public global::UI_LavaRush ControllerPrefab => assets?.ControllerPrefab;
        public bool InitializeOnStart => settings?.InitializeOnStart ?? true;
        public bool IsInitialized => _engine != null && _controller != null;
        public bool IsVisible => IsInitialized && _controller.gameObject.activeSelf;

#if UNITY_EDITOR
        private void OnValidate() => ReferenceBindingRequests.Enqueue(this);
#endif

        private void Start()
        {
            if (InitializeOnStart && !IsInitialized)
                InitializeDefault();
        }

        private void OnDestroy()
        {
            if (_ownsController && _controller != null)
                Destroy(_controller.gameObject);
        }

        public void InitializeDefault(
            global::UI_LavaRush controller = null,
            ILavaRushFrameScheduler frameScheduler = null,
            ILavaRushCountdownScheduler countdownScheduler = null,
            ILavaRushAudio audio = null,
            ILavaRushUILocalizer localizer = null,
            ILavaRushUIRewardRenderer rewardRenderer = null,
            ILavaRushProfileRoster profiles = null,
            bool restoreEngine = true,
            bool showOnInitialize = true,
            ILavaRushProfileGroupFactory profileGroupFactory = null,
            ILavaRushTutorialFocusSpriteProvider tutorialFocusSprites = null,
            ILavaRushRewardPresentationProvider rewardPresentation = null)
        {
            _demoClock = new LavaRushDemoClock();
            var engine = new LavaRushEngine(
                new PlayerPrefsContentStateStore(),
                new PlayerPrefsContentRewardService(),
                new LavaRushDemoCatalogResolver(),
                _demoClock,
                TimeZoneInfo.Local,
                _demoClock,
                new SystemLavaRushRandom(),
                new LinearLavaRushSeatCurveProvider(),
                settings?.ContentId ?? DefaultDemoContentId,
                new AllowLavaRushAccessPolicy(),
                new LavaRushDemoSchedulePolicy());

            Initialize(
                engine,
                controller,
                frameScheduler,
                countdownScheduler,
                audio,
                localizer,
                rewardRenderer,
                profiles,
                restoreEngine,
                showOnInitialize,
                profileGroupFactory,
                tutorialFocusSprites,
                rewardPresentation);
        }

        public void Initialize(
            LavaRushEngine engine,
            global::UI_LavaRush controller = null,
            ILavaRushFrameScheduler frameScheduler = null,
            ILavaRushCountdownScheduler countdownScheduler = null,
            ILavaRushAudio audio = null,
            ILavaRushUILocalizer localizer = null,
            ILavaRushUIRewardRenderer rewardRenderer = null,
            ILavaRushProfileRoster profiles = null,
            bool restoreEngine = true,
            bool showOnInitialize = true,
            ILavaRushProfileGroupFactory profileGroupFactory = null,
            ILavaRushTutorialFocusSpriteProvider tutorialFocusSprites = null,
            ILavaRushRewardPresentationProvider rewardPresentation = null)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));
            if (IsInitialized)
                throw new InvalidOperationException("Lava Rush UI is already initialized.");

            _controller = controller ?? GetComponentInChildren<global::UI_LavaRush>(true);
            if (_controller == null && assets?.ControllerPrefab != null)
            {
                _controller = Instantiate(assets.ControllerPrefab, transform);
                _ownsController = true;
            }
            if (_controller == null)
                throw new InvalidOperationException(
                    "A restored UI_LavaRush controller prefab is required; fallback generation is disabled.");

            _engine = engine;
            var context = new LavaRushControllerContext(
                engine,
                frameScheduler,
                countdownScheduler,
                audio,
                localizer,
                rewardRenderer,
                profiles,
                profileGroupFactory: profileGroupFactory,
                tutorialFocusSprites: tutorialFocusSprites,
                rewardPresentation: rewardPresentation);
            _controller.Initialize(context, restoreEngine);
            _controller.gameObject.SetActive(showOnInitialize);
            if (showOnInitialize)
                _controller.OpenMatchFlow();
        }

        public void Show()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Lava Rush UI is not initialized.");

            _controller.gameObject.SetActive(true);
            _controller.OpenMatchFlow();
        }

        public void Hide()
        {
            if (!IsInitialized)
                return;

            _controller.gameObject.SetActive(false);
            CloseRequested?.Invoke();
        }
    }
}
