# AI Guide - ActionFit Lava Rush UI

This guide ships with the package so an AI assistant preserves the exact production visual baseline and asset boundary in a consuming project.

## Package Identity

- Package ID: `com.actionfit.lava-rush.ui`
- Display name: ActionFit Lava Rush UI
- Repository: `https://github.com/ActionFit-Editor/LavaRushUI.git`
- Repository visibility: Public
- Current package version at generation time: `0.1.11`
- Unity version: `6000.2`
- Declared runtime dependencies: `com.actionfit.content-core@0.2.3`, `com.actionfit.lava-rush@0.1.7`, `com.actionfit.time@1.0.4`, `com.actionfit.ui.foundation@2.0.0`, and `com.unity.ugui@2.0.0`
- Required bundle-level visual dependencies: `com.coffee.ui-effect@5.10.8`, `com.coffee.ui-particle@4.12.1`, `com.coffee.softmask-for-ugui@3.5.0`, `com.actionfit.uilighteffector@1.0.0` at full commit `7dab46ec2378209bd1e524c8336b976eccb3df05`, and `jp.hadashikick.vcontainer@1.16.8`

## Purpose And Boundary

The package provides the complete production-equivalent UGUI baseline: all 14 original Lava Rush prefab roles, all 56 original Lava Rush PNGs, required visual dependencies, and a standalone composition root. Source images are copied byte-for-byte with importer behavior preserved; copied prefab references are remapped to package-owned copies. The generated view is retained only as a diagnostic recovery path and must never hide an incomplete production baseline.

The package does not own schedule, stage, timeout, rank, reward, serialization, or recovery rules. Those remain in `LavaRushEngine`. It does not reference `Assembly-CSharp`, Cat Merge gameplay APIs, Addressables, analytics, persistence, DOTween, or UniTask. UI Foundation and copied visual assets are package-owned. Original third-party effect components remain intact and their immutable Git dependencies are installed by the bundle because UPM does not support nested Git URLs in this package's `package.json`.

The optional `Samples~/CatDetective Starter` is the only CatDetective-specific surface. Its scripts remain outside every package assembly until an operator explicitly imports them under the consuming project's `Assets/Contents/LavaRush` path. Imported files are project-owned and may use CatDetective `Assembly-CSharp`, `Prefs`, `Main`, `TimeProvider`, `UI_Popup`, and Addressables APIs.

## Project Router Registration

Requested router entry:

- `Packages/com.actionfit.lava-rush.ui/AI_GUIDE.md` - ActionFit Lava Rush UI owns the production-equivalent UGUI baseline, immutable view model, replaceable project service boundaries, and standalone PlayerPrefs demo bootstrap.

## Runtime Architecture

- `LavaRushBootstrap` constructs or accepts a caller-owned `LavaRushEngine`, subscribes to `StateChanged`, maps public engine reads to `LavaRushUIViewModel`, and routes `LavaRushUIAction` requests to public engine commands.
- `LavaRushPresentation` owns only view hierarchy, rendering, local feedback text, and replaceable screen/progress animations. It must not write engine state or grant rewards.
- `LavaRushScreenView` is the thin package-owned binder for each authored state prefab. It receives immutable view content, activates only its matching screen/result role, and reports button actions through the existing parent callback.
- `Runtime/Prefabs/Main/UI_LavaRush.prefab` is the canonical nested visual composition. `Runtime/Prefabs/LavaRushPresentation.prefab` preserves the published path/GUID as its compatibility composition root. `Runtime/Prefabs/LavaRushDemo.prefab` and the CatDetective sample reference the compatibility root instead of cloning its UI hierarchy.
- `LavaRushUITheme`, `LavaRushUIConfig`, and Inspector view references are runtime-read-only authored inputs. Generated fallback references and theme overrides live in separate runtime fields.
- `ApplyThemeOverride` is valid only before presentation initialization. `ResolveDefaultTheme` is the narrow protected theme extension used by the optional Cat Merge package.
- Production binders use the declared UI Foundation `UI_Text`/`UI_Button` contracts. The legacy fallback remains isolated on Unity built-ins and is not a valid substitute for missing production assets.
- The demo clock, schedule, and catalog are standalone fixtures. Its calendar uses `TimeZoneInfo.Local`; production projects inject their own engine, calendar policy, and project adapters.

## Extension Rules

- Prefer a prefab and serialized theme/config for visual-only customization.
- Edit the role prefab under `Runtime/Prefabs/Base`, `Icon`, `Main`, or `UI` instead of flattening every state into the compatibility root. Keep all mandatory `Image.sprite` and `LavaRushScreenView` references valid.
- For project-specific visual work, explicitly use Custom Package Manager `Embed for Edit` on `com.actionfit.lava-rush.ui` only. Never auto-embed it, embed the engine/shared packages for this purpose, or overwrite compatible embedded edits during install, repair, upgrade, or release.
- Compatibility exception: when a consuming project already owns different global `UI_Image`, `UI_Text`, or `UI_Button` sources, perform a read-only GUID/type audit first. Preserve those project scripts and GUIDs, embed UI Foundation project-locally, and set only its Runtime asmdef `autoReferenced` to false so Lava Rush UI keeps its explicit assembly reference without exposing duplicate types to `Assembly-CSharp`. Document this project override; never automate source deletion or prefab/scene migration.
- Derive from `LavaRushPresentation` only for the documented theme, screen-transition, and progress-animation hooks.
- Use `ILavaRushUILocalizer`, `ILavaRushUIAudio`, `ILavaRushUIRewardRenderer`, `ILavaRushUIProfileProvider`, and `ILavaRushUIViewHost` at a real cross-assembly project boundary. Do not add a general service locator.
- Keep CatDetective adapters in the imported sample. Do not move `Prefs`, `Main.Data`, `Main.Locale`, `Main.Audio`, `TimeProvider`, `UI_Popup`, or Addressables references into Runtime or Editor package assemblies.
- Keep project order/merge conversion, analytics, Addressable loading, navigation, and inventory adapters outside this package.
- Do not bypass `TryStartEvent`, `SelectDifficulty`, `StartStage`, `AddProgress`, `EvaluateStageResult`, `ClaimPendingReward`, `ClearPendingResult`, or `EndEvent` with UI-owned state.
- Preserve the engine's pending reward before allowing result cleanup. A failed claim remains visible and retryable.

## Asset And Compatibility Rules

- The package baseline is an additive copy of all 14 production prefab roles, all 56 original Lava Rush PNGs, and required visual dependencies. Preserve exact bytes, TextureImporter behavior, hierarchy, transforms, active states, visual references, and interactions.
- Production TMP shader copies must include the original `TMPro.cginc`, `TMPro_Mobile.cginc`, `TMPro_Properties.cginc`, and `TMPro_Surface.cginc` files byte-for-byte so every relative shader include resolves inside the package.
- `Documentation~/MigrationCoverage.md` must keep one-to-one 14-prefab and 56-image coverage. Consolidating variants, omitting roles, or substituting another image is forbidden.
- Never invoke image generation or add AI-generated, synthesized, placeholder, redrawn, or automatically substituted visual assets to the baseline.
- Preserve the original UIEffect, UIParticle, SoftMask, and UILighting components. `Documentation~/ExternalVisualDependencies.md` is the exact dependency contract; never remove a component merely to make a bare package fixture compile.
- If a required original asset cannot be included for any reason, stop and obtain an explicit per-asset decision. Do not silently continue with a partial package.
- Do not move, delete, overwrite, or regenerate files or `.meta` GUIDs under `Assets/_Project/Content/LavaRush`. Package copies use separate GUIDs for coexistence and deterministic reference remapping.
- Any future visual change requires provenance, reference validation, and explicit approval; it must not weaken the production baseline contract.
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
- Compile and test the source-only UI assembly in an isolated Unity project with declared package dependencies, and separately validate every production prefab with all bundle-level visual dependencies registered.
- Verify all 56 package images match source bytes and importer behavior, and all copied visual dependencies resolve below this package rather than a consuming project's `Assets` folder.
- Verify the authored prefab has complete serialized references, all visual dependencies stay under this package, and initialization does not create a second fallback Canvas.
- Verify all 14 role prefabs load, every `Image` has a package-owned sprite, the main prefab retains nested dependencies, exactly one of the eight state views is active per model, and rendered screenshots cover start/difficulty/tutorial/match/win/lose/complete/event-end.
- Validate first import, same-version repeat, differing-file conflict, missing dependency, and cancelled Addressables registration in a disposable CatDetective workspace. Never run import validation against a dirty shared checkout.
- In Cat Merge, separately verify existing Addressable UI, event access, save migration, timeout/result recovery, and reward claims because this package does not replace those project adapters automatically.

## Metadata And Release

- `package.json` owns identity, version, Unity version, and exact dependencies.
- `Editor/PackageInfo/ActionFitPackageInfo_SO.asset` owns repository name `LavaRushUI`, Public visibility, Korean description, and the single-version Korean release note.
- Publishing is manual through Custom Package Manager. Do not create a repository, push, tag, or append a catalog row without separate authorization.
