# AI Guide - ActionFit Lava Rush UI

This guide ships with the package so an AI assistant can preserve its neutral presentation and asset boundary in a consuming project.

## Package Identity

- Package ID: `com.actionfit.lava-rush.ui`
- Display name: ActionFit Lava Rush UI
- Repository: `https://github.com/ActionFit-Editor/LavaRushUI.git`
- Repository visibility: Public
- Current package version at generation time: `0.1.8`
- Unity version: `6000.2`
- Runtime dependencies: `com.actionfit.content-core@0.2.3`, `com.actionfit.lava-rush@0.1.6`, `com.actionfit.time@1.0.4`, and `com.unity.ugui@2.0.0`

## Purpose And Boundary

The package provides a complete modular neutral UGUI prefab set for all 14 inventoried production roles, ActionFit-owned Lava Rush sprites, a replaceable theme asset, and a standalone composition root. The generated view is retained only as a missing-or-broken-prefab fallback and uses Unity built-in resources and solid UGUI geometry.

The package does not own schedule, stage, timeout, rank, reward, serialization, or recovery rules. Those remain in `LavaRushEngine`. It also does not reference `Assembly-CSharp`, Cat Merge APIs, Addressables, project fonts, localization tables, audio, DOTween, UniTask, or copied project assets.

The optional `Samples~/CatDetective Starter` is the only CatDetective-specific surface. Its scripts remain outside every package assembly until an operator explicitly imports them under the consuming project's `Assets/Contents/LavaRush` path. Imported files are project-owned and may use CatDetective `Assembly-CSharp`, `Prefs`, `Main`, `TimeProvider`, `UI_Popup`, and Addressables APIs.

## Project Router Registration

Requested router entry:

- `Packages/com.actionfit.lava-rush.ui/AI_GUIDE.md` - ActionFit Lava Rush UI owns the neutral UGUI presentation, immutable view model, replaceable project service boundaries, and standalone PlayerPrefs demo bootstrap.

## Runtime Architecture

- `LavaRushBootstrap` constructs or accepts a caller-owned `LavaRushEngine`, subscribes to `StateChanged`, maps public engine reads to `LavaRushUIViewModel`, and routes `LavaRushUIAction` requests to public engine commands.
- `LavaRushPresentation` owns only view hierarchy, rendering, local feedback text, and replaceable screen/progress animations. It must not write engine state or grant rewards.
- `LavaRushScreenView` is the thin package-owned binder for each authored state prefab. It receives immutable view content, activates only its matching screen/result role, and reports button actions through the existing parent callback.
- `Runtime/Prefabs/Main/UI_LavaRush.prefab` is the canonical nested visual composition. `Runtime/Prefabs/LavaRushPresentation.prefab` preserves the published path/GUID as its compatibility composition root. `Runtime/Prefabs/LavaRushDemo.prefab` and the CatDetective sample reference the compatibility root instead of cloning its UI hierarchy.
- `LavaRushUITheme`, `LavaRushUIConfig`, and Inspector view references are runtime-read-only authored inputs. Generated fallback references and theme overrides live in separate runtime fields.
- `ApplyThemeOverride` is valid only before presentation initialization. `ResolveDefaultTheme` is the narrow protected theme extension used by the optional Cat Merge package.
- The fallback view uses `UnityEngine.UI.Text`, solid `Image` geometry, and a package-local transition curve. Do not add TMP resources, project font assets, or global UI Foundation wrapper dependencies to the fallback; projects such as CatDetective already own similarly named global wrappers.
- The demo clock, schedule, and catalog are standalone fixtures. Production projects inject their own engine and project adapters.

## Extension Rules

- Prefer a prefab and serialized theme/config for visual-only customization.
- Edit the role prefab under `Runtime/Prefabs/Base`, `Icon`, `Main`, or `UI` instead of flattening every state into the compatibility root. Keep all mandatory `Image.sprite` and `LavaRushScreenView` references valid.
- For project-specific visual work, explicitly use Custom Package Manager `Embed for Edit` on `com.actionfit.lava-rush.ui` only. Never auto-embed it, embed the engine/shared packages for this purpose, or overwrite compatible embedded edits during install, repair, upgrade, or release.
- Derive from `LavaRushPresentation` only for the documented theme, screen-transition, and progress-animation hooks.
- Use `ILavaRushUILocalizer`, `ILavaRushUIAudio`, `ILavaRushUIRewardRenderer`, `ILavaRushUIProfileProvider`, and `ILavaRushUIViewHost` at a real cross-assembly project boundary. Do not add a general service locator.
- Keep CatDetective adapters in the imported sample. Do not move `Prefs`, `Main.Data`, `Main.Locale`, `Main.Audio`, `TimeProvider`, `UI_Popup`, or Addressables references into Runtime or Editor package assemblies.
- Keep project order/merge conversion, analytics, Addressable loading, navigation, and inventory adapters outside this package.
- Do not bypass `TryStartEvent`, `SelectDifficulty`, `StartStage`, `AddProgress`, `EvaluateStageResult`, `ClaimPendingReward`, `ClearPendingResult`, or `EndEvent` with UI-owned state.
- Preserve the engine's pending reward before allowing result cleanup. A failed claim remains visible and retryable.

## Asset And Compatibility Rules

- The package includes independently authored Lava Rush background, explorer, reward, and deterministic UI geometry PNGs, 14 role prefabs, the compatibility presentation/demo prefabs, and a neutral theme asset. They are editable package defaults, not copies of Cat Merge Cafe assets.
- `Documentation~/MigrationCoverage.md` must keep explicit 14-prefab and 56-image role coverage. Consolidating several product-specific variants into one neutral package sprite is allowed only when the semantic edit slot remains documented.
- Do not move, delete, duplicate, or regenerate files under `Assets/_Project/Content/LavaRush` as part of package maintenance.
- Any future asset inclusion requires ownership/license evidence, one canonical location, preserved `.meta` GUID when identity must remain stable, reference validation, and separate migration approval.
- Existing project Addressable keys `UI_LavaRush`, `UI_LavaRush_Icon`, and `UI_LavaRush_Cell` remain project-owned compatibility contracts.

## Package Tools Menu

- `Tools/Package/ActionFit Lava Rush UI/Create Demo`: instantiates the package-authored demo prefab. If the prefab is unavailable, it creates a `LavaRushBootstrap` that uses the generated fallback.
- `Tools/Package/ActionFit Lava Rush UI/Preview CatDetective Starter`: reports exact dependency gaps, new files, unchanged files, conflicts, and serialized operations without writing.
- `Tools/Package/ActionFit Lava Rush UI/Install CatDetective Starter`: copies only missing files to `Assets/Contents/LavaRush` after confirmation and refuses the whole plan when any target differs.
- `Tools/Package/ActionFit Lava Rush UI/Run CatDetective Starter Preflight`: reports whether the exact dependency set and imported files are current.
- The imported sample adds a separate CatDetective Addressables preview and confirmation menu. It must remain collision-safe and must not silently replace another address or asset entry.
- `Tools/Package/ActionFit Lava Rush UI/README`: opens the installed README.
- The package owns no settings ScriptableObject and exposes no `Setting SO` menu.

## Validation

- Run the package contract validator for `com.actionfit.lava-rush.ui`.
- Run `com.actionfit.lava-rush.ui.Editor.Tests` and the engine tests.
- Compile and test in an isolated Unity project with only declared dependencies.
- Verify the fallback view uses no source or asset below a consuming project's `Assets` folder.
- Verify the authored prefab has complete serialized references, all sprite dependencies stay under this package, and initialization does not create a second fallback Canvas.
- Verify all 14 role prefabs load, every `Image` has a package-owned sprite, the main prefab retains nested dependencies, exactly one of the eight state views is active per model, and rendered screenshots cover start/difficulty/tutorial/match/win/lose/complete/event-end.
- Validate first import, same-version repeat, differing-file conflict, missing dependency, and cancelled Addressables registration in a disposable CatDetective workspace. Never run import validation against a dirty shared checkout.
- In Cat Merge, separately verify existing Addressable UI, event access, save migration, timeout/result recovery, and reward claims because this package does not replace those project adapters automatically.

## Metadata And Release

- `package.json` owns identity, version, Unity version, and exact dependencies.
- `Editor/PackageInfo/ActionFitPackageInfo_SO.asset` owns repository name `LavaRushUI`, Public visibility, Korean description, and the single-version Korean release note.
- Publishing is manual through Custom Package Manager. Do not create a repository, push, tag, or append a catalog row without separate authorization.
