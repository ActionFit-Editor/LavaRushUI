# ActionFit Lava Rush UI (`com.actionfit.lava-rush.ui`)

A project-neutral UGUI presentation for `com.actionfit.lava-rush`. It includes a standalone PlayerPrefs bootstrap and a generated fallback view, so a clean Unity project can exercise event start, difficulty selection, tutorial acknowledgement, timed stages, progress, results, and idempotent reward claims without importing a prefab or game-specific asset.

## Install

After the public packages are published, add the Git packages to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.actionfit.content-core": "https://github.com/ActionFit-Editor/ContentCore.git#0.2.1",
    "com.actionfit.time": "https://github.com/ActionFit-Editor/Time.git#1.0.3",
    "com.actionfit.lava-rush": "https://github.com/ActionFit-Editor/LavaRush.git#0.1.3",
    "com.actionfit.lava-rush.ui": "https://github.com/ActionFit-Editor/LavaRushUI.git#0.1.3"
  }
}
```

The package also declares `com.unity.ugui@2.0.0`.

## Quick Start

1. Select `Tools > Package > ActionFit Lava Rush UI > Create Demo`.
2. Enter Play Mode.
3. Start the event, select a difficulty, acknowledge the tutorial, and run each stage.
4. Use **+ Progress** or **Resolve Timer** only in the standalone demo. Both actions still call public `LavaRushEngine` commands.

`LavaRushBootstrap` composes `LavaRushEngine` with Content Core PlayerPrefs defaults, a deterministic Monday demo clock, a one-day schedule, and a package-owned demo catalog. Inject a project-owned engine for production.

When no presentation prefab is assigned, `LavaRushPresentation` creates a complete overlay Canvas with solid UGUI shapes, labels, progress, timers, reward summaries, and buttons. It uses a package-local transition curve and has no UI Foundation, DOTween, UniTask, Addressables, project font, localization table, or audio dependency. This avoids global UGUI wrapper type collisions in consuming projects such as CatDetective.

## CatDetective Starter

`Samples~/CatDetective Starter` is an opt-in, project-owned consumer bridge for `AF_CatDetective` Unity `6000.3.9f1`. It stays inert inside the package and may reference CatDetective `Assembly-CSharp` APIs only after import under `Assets/Contents/LavaRush`.

1. Select `Tools > Package > ActionFit Lava Rush UI > Preview CatDetective Starter`.
2. Resolve every exact-version dependency issue and file conflict.
3. Select `Install CatDetective Starter` and approve the create-only file plan.
4. Configure the imported `CatDetectiveLavaRushSettings.asset`.
5. Preview and separately approve the imported CatDetective Addressables registration menu.

The installer never overwrites a differing target file. A same-version repeat import is a no-op, and an Addressables collision blocks registration without changing serialized settings. See the imported sample README for persistence, reward, time, popup, upgrade, removal, and known-atomicity details.

## Production Integration

- Construct `LavaRushEngine` with project-owned persistence, reward, clock, catalog, access, schedule, curve, random, and analytics adapters.
- Pass it to `LavaRushBootstrap.Initialize` together with an existing `LavaRushPresentation`, or use the bootstrap only as a reference action router for a project composition root.
- Implement `ILavaRushUILocalizer`, `ILavaRushUIAudio`, `ILavaRushUIRewardRenderer`, `ILavaRushUIProfileProvider`, or `ILavaRushUIViewHost` only at the consuming-project boundary.
- Apply a `LavaRushUIThemeAsset`, an inline theme, or call `ApplyThemeOverride` before initialization. The optional `com.actionfit.lava-rush.theme.catmerge` package provides a redistribution-safe preset.
- Disable demo action buttons in a custom presentation config when project order and merge adapters own progress.

The presentation reads immutable engine state and raises `LavaRushUIAction` requests. It does not write package JSON, grant inventory directly, resolve schedules, or duplicate stage and reward rules.

## Runtime API

- `LavaRushBootstrap`: standalone composition root and action router.
- `LavaRushPresentation`: generated or prefab-backed UGUI presenter with limited screen/progress hooks.
- `LavaRushUIViewModel`: immutable presentation snapshot.
- `LavaRushUITheme`, `LavaRushUIThemeAsset`, and `LavaRushUIConfig`: Inspector-authored appearance and behavior inputs.
- `ILavaRushUILocalizer`, `ILavaRushUIAudio`, `ILavaRushUIRewardRenderer`, `ILavaRushUIProfileProvider`, and `ILavaRushUIViewHost`: narrow project service boundaries.

## Asset Boundary

This candidate includes no files copied from `Assets/_Project/Content/LavaRush`, no third-party art, no project fonts, and no audio. Existing Cat Merge prefabs, Addressable keys, scripts, `.meta` GUIDs, and binary resources remain untouched. Any later asset move requires a separate rights review and GUID/reference migration.

## Assemblies And Tests

- Runtime: `com.actionfit.lava-rush.ui`
- Editor: `com.actionfit.lava-rush.ui.Editor`
- EditMode tests: `com.actionfit.lava-rush.ui.Editor.Tests`

Run package contract validation, the UI and engine EditMode tests, and isolated Unity validation before release handoff.

## Publishing

Repository visibility metadata is Public. Repository creation, Git push, tagging, catalog registration, and publishing remain manual Custom Package Manager actions.
