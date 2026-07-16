using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CatDetectiveLavaRushCompositionRoot))]
public sealed class UI_LavaRush : UI_Popup
{
    private CatDetectiveLavaRushCompositionRoot _compositionRoot;

    public override bool AllowDuplicate => false;

    public override bool Initialize()
    {
        bool initialized = base.Initialize();
        _compositionRoot ??= GetComponent<CatDetectiveLavaRushCompositionRoot>();
        return initialized;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _compositionRoot ??= GetComponent<CatDetectiveLavaRushCompositionRoot>();
        if (_compositionRoot == null || !_compositionRoot.Open(this))
        {
            UnityEngine.Debug.LogError("[UI_LavaRush] CatDetective Lava Rush composition failed to open.");
        }
    }

    protected override void OnDisable()
    {
        _compositionRoot?.Release();
        base.OnDisable();
    }

    public override void OnRelease()
    {
        _compositionRoot?.Release();
        base.OnRelease();
    }
}
