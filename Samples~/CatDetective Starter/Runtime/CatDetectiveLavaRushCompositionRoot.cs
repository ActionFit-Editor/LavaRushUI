using System;
using ActionFit.LavaRush;
using ActionFit.LavaRush.UI;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CatDetectiveLavaRushCompositionRoot : MonoBehaviour
{
    [SerializeField] private CatDetectiveLavaRushSettings settings;

    private LavaRushBootstrap _bootstrap;
    private UI_LavaRush _owner;

    public bool Open(UI_LavaRush owner)
    {
        _owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
        if (_bootstrap == null && !Initialize())
        {
            return false;
        }

        if (!_bootstrap.IsVisible)
        {
            _bootstrap.Show();
        }
        return true;
    }

    public void Release()
    {
        if (_bootstrap?.Presentation != null)
        {
            _bootstrap.Presentation.Hide();
        }
        _owner = null;
    }

    private bool Initialize()
    {
        settings ??= Resources.Load<CatDetectiveLavaRushSettings>(CatDetectiveLavaRushSettings.ResourcesPath);
        if (settings == null)
        {
            UnityEngine.Debug.LogError(
                $"[CatDetectiveLavaRushCompositionRoot] Missing Resources/{CatDetectiveLavaRushSettings.ResourcesPath}.asset.");
            return false;
        }

        try
        {
            _bootstrap = GetComponent<LavaRushBootstrap>();
            if (_bootstrap == null)
            {
                _bootstrap = gameObject.AddComponent<LavaRushBootstrap>();
            }

            var clock = new CatDetectiveLavaRushClock();
            var engine = new LavaRushEngine(
                new CatDetectiveLavaRushStateStore(),
                new CatDetectiveLavaRushRewardService(settings),
                new CatDetectiveLavaRushCatalogResolver(settings),
                clock,
                clock,
                new SystemLavaRushRandom(),
                new LinearLavaRushSeatCurveProvider(),
                settings.ContentId,
                new AllowLavaRushAccessPolicy(),
                new CatDetectiveLavaRushSchedulePolicy(settings));

            _bootstrap.CloseRequested += HandleCloseRequested;
            _bootstrap.Initialize(
                engine,
                localizer: new CatDetectiveLavaRushLocalizer(settings),
                audio: new CatDetectiveLavaRushAudio(settings),
                profileProvider: new CatDetectiveLavaRushProfileProvider(settings));
            return true;
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogError(
                $"[CatDetectiveLavaRushCompositionRoot] Initialization failed: {exception}");
            return false;
        }
    }

    private void HandleCloseRequested()
    {
        _owner?.Close();
    }

    private void OnDestroy()
    {
        if (_bootstrap != null)
        {
            _bootstrap.CloseRequested -= HandleCloseRequested;
        }
    }
}
