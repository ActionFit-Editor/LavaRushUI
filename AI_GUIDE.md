# AI Guide - ActionFit Lava Rush UI

This guide ships with the package so an AI assistant preserves the exact production visual baseline and asset boundary in a consuming project.

## Package Identity

- Package ID: `com.actionfit.lava-rush.ui`
- Display name: ActionFit Lava Rush UI
- Repository: `https://github.com/ActionFit-Editor/LavaRushUI.git`
- Repository visibility: Public
- Current package version at generation time: `0.1.21`
- Unity version: `6000.2`
- Declared runtime dependencies: `com.actionfit.content-core@0.2.3`, `com.actionfit.lava-rush@0.1.9`, `com.actionfit.referencebinding@0.1.3`, `com.actionfit.time@1.0.4`, `com.actionfit.ui.foundation@2.0.4`, `com.unity.modules.animation@1.0.0`, and `com.unity.ugui@2.0.0`
- Required bundle-level visual dependencies: `com.coffee.ui-effect@5.10.8`, `com.coffee.ui-particle@4.12.1`, `com.coffee.softmask-for-ugui@3.5.0`, `com.actionfit.uilighteffector@1.0.0` at full commit `7dab46ec2378209bd1e524c8336b976eccb3df05`, and `jp.hadashikick.vcontainer@1.16.8`

## Purpose And Boundary

The package provides the complete production-equivalent UGUI baseline: all 14 original Lava Rush prefab roles, all 56 original Lava Rush PNGs, required visual dependencies, and a standalone composition root. Original image bytes and importer behavior are preserved. Completed migration units preserve the original project GUID at one package path with no local duplicate. The generated view is retained only as a diagnostic recovery path and must never hide an incomplete production baseline.

The package does not own schedule, stage, timeout, rank, reward, serialization, or recovery rules. Those remain in `LavaRushEngine`. It does not reference `Assembly-CSharp`, Cat Merge gameplay APIs, Addressables, analytics, persistence, DOTween, or UniTask. UI Foundation and copied visual assets are package-owned. Original third-party effect components remain intact and their immutable Git dependencies are installed by the bundle because UPM does not support nested Git URLs in this package's `package.json`.

The optional `Samples~/CatDetective Starter` is the only CatDetective-specific surface. Its scripts remain outside every package assembly until an operator explicitly imports them under the consuming project's `Assets/Contents/LavaRush` path. Imported files are project-owned and may use CatDetective `Assembly-CSharp`, `Prefs`, `Main`, `TimeProvider`, `UI_Popup`, and Addressables APIs.

## Project Router Registration

Requested router entry:

- `Packages/com.actionfit.lava-rush.ui/AI_GUIDE.md` - ActionFit Lava Rush UI owns the production-equivalent UGUI baseline, immutable view model, replaceable project service boundaries, and standalone PlayerPrefs demo bootstrap.

## Runtime Architecture

- `LavaRushBootstrap` constructs or accepts a caller-owned `LavaRushEngine`, subscribes to `StateChanged`, maps public engine reads to `LavaRushUIViewModel`, and routes `LavaRushUIAction` requests to public engine commands.
- `LavaRushPresentation` owns only view hierarchy, rendering, local feedback text, and replaceable screen/progress animations. It must not write engine state or grant rewards.
- `LavaRushScreenView` is the thin package-owned binder for each authored state prefab. It receives immutable view content, activates only its matching screen/result role, and reports button actions through the existing parent callback.
- `LavaRushBlockView` is the thin package-owned binder for the authored production block prefab. It owns only serialized visual references, presentation setters, and the reward-info callback; item lookup, amount formatting, collection navigation, and player/enemy profile groups remain consuming-project adapters.
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

- The package baseline covers all 14 production prefab roles, all 56 original Lava Rush PNGs, and required visual dependencies. Preserve exact bytes, TextureImporter behavior, hierarchy, transforms, active states, visual references, and interactions.
- Production TMP shader copies must include the original `TMPro.cginc`, `TMPro_Mobile.cginc`, `TMPro_Properties.cginc`, and `TMPro_Surface.cginc` files byte-for-byte so every relative shader include resolves inside the package.
- `Documentation~/MigrationCoverage.md` must keep one-to-one 14-prefab and 56-image coverage. Consolidating variants, omitting roles, or substituting another image is forbidden.
- Never invoke image generation or add AI-generated, synthesized, placeholder, redrawn, or automatically substituted visual assets to the baseline.
- Preserve the original UIEffect, UIParticle, SoftMask, and UILighting components. `Documentation~/ExternalVisualDependencies.md` is the exact dependency contract; never remove a component merely to make a bare package fixture compile.
- If a required original asset cannot be included for any reason, stop and obtain an explicit per-asset decision. Do not silently continue with a partial package.
- Do not create another coexistence copy. Migrate one exact role at a time through the AI Content Package Standard AssetDatabase tool, preserve its original project GUID at the package path, remove its verified local path in the same unit, and validate project/package consumers before continuing. Differing prefab bytes or project dependencies require a content-specific review and cannot be auto-merged.
- Any future visual change requires provenance, reference validation, and explicit approval; it must not weaken the production baseline contract.
- Version `0.1.12` intentionally changes only the enabled `UI_Text` Outline Width in `Runtime/Prefabs/Base/Content_LavaBlock.prefab` to `0.1`; prefab hierarchy, references, source images, and other authored visual settings remain unchanged.
- Version `0.1.13` adds the direct ReferenceBinding dependency and stable required-reference attributes while preserving the original 14-prefab/56-image baseline and the `0.1.12` outline setting.
- Version `0.1.14` makes `Main_icon.png` the first single-owner migration unit. The package path preserves original GUID `756239e4572274b17b3fcae6f4964bdb`; `AssetOwnership.json` records its SHA-256 and the legacy path is absent.
- Version `0.1.15` disables the TMP Bold style in `Runtime/Prefabs/Icon/UI_LavaRush_Cell.prefab` and enables Extra Padding on every packaged TMP component while preserving hierarchy, references, text, materials, and GUIDs.
- Version `0.1.16` disables `Maskable` only on the three staged tutorial `TextMeshProUGUI` components in `Runtime/Prefabs/UI/UI_LavaRush_Match.prefab`. This bypasses incompatible SoftMask material replacement while preserving the packaged legacy localization event and existing `UI_Text` component settings, authored SoftMask parent, hierarchy, references, and progression behavior; the project production prefab keeps its `UI_Text` localization and outline settings.
- Version `0.1.17` pins engine `0.1.9` so the production presentation and standalone composition use the released canonical CSV catalog boundary. It moves the required-reference contract to `LavaRushBootstrap.presentationPrefab`, which is mandatory for standalone creation; supported package-prefab fields use stable `RequiredReference` and `AutoWireChild` contracts, while unsupported shapes such as `RectTransform panel` remain under explicit serialized-reference tests. It records all 56 images as single-owner units in `AssetOwnership.json`; every original image resolves at its package path with the original project GUID and no local image duplicate. It also makes `Img_Title Variant.prefab`, `UI_LavaRush_BaseEvent.prefab`, `UI_LavaRush_Icon.prefab`, `UI_LavaRush_Cell.prefab`, and `Content_LavaBlock.prefab` single-owner prefab units while preserving their original GUIDs and consumed local file identifiers. The title and flattened BaseEvent retain their valid legacy consumer identifiers; three previously serialized no-op BaseEvent targets remain intentionally unbound because Unity already ignored them before migration. `LavaRushAccessIconView`, `LavaRushInGameCellView`, and `LavaRushBlockView` own required presentation references and callbacks without Cat Merge services. Cat Merge retains item lookup, amount formatting, collection navigation, player/enemy profile groups, and EventAccess behavior adapters. Original bytes, importer metadata, rendered presentation behavior, authored prefab values, and the existing Match consumer reference are preserved; the five former local prefab paths, all local image duplicates, and the former `Content_LavaBlock` script are removed after characterization and consumer validation.
- Version `0.1.18` restores the original `UI_Text` components and authored material/localization settings on the access icon timer plus the in-game cell timer, status, and nested localized status text. It preserves their original local file identifiers. The cell Indicator retains its GameObject, Transform, active-state policy, and component file identifier while replacing the broken Animator binding with UI Foundation `ScalePulse`. Package binders and Cat Merge adapters use the concrete `UI_Text` and `ScalePulse` types, while the shared EventAccess countdown formatter, time source, registration, cancellation, and expiry behavior remain unchanged.
- Version `0.1.19` restores the flattened Cell `Txt_ReaminCount` outline and `Txt_RemainTitle` localization/outline Inspector values from the production nested-prefab overrides, normalizes both `UI_Text` assembly identifiers, and removes the two duplicate legacy `LocalizeStringEvent` components. The Cat Merge Difficulty description now explicitly overrides `Img_Desc` with the package-owned original `Popup_textboard.png` instead of the DP preview image; hierarchy, bindings, timers, and interactions remain unchanged.
- Version `0.1.20` classifies every `Runtime/Art/DP` image as a full-screen design preview that production prefabs must not consume. Cat Merge Match End, Match Win, and Match Lose now use the BaseEvent `Img_Desc` original `Popup_textboard.png`; hierarchy, alpha, transforms, bindings, and interactions remain unchanged, and an exhaustive project/package prefab dependency test blocks future DP references.
- Version `0.1.21` restores `Runtime/Prefabs/Base/Img_Title Variant.prefab` as a nested prefab backed only by package-owned internal image and text bases. It preserves the original role GUID and Match-consumed local file identifiers while restoring the package font material, `UI_Text` localization, Outline `0.1`, and authored Underlay color/offset without a duplicate `LocalizeStringEvent`. Regression coverage requires all nested sources and visual dependencies to remain inside packages rather than a consuming project's `Assets` tree.
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
- Verify all 56 package images against completed `AssetOwnership.json` GUID/SHA evidence, and ensure every visual dependency resolves below this package rather than a consuming project's `Assets` folder.
- Verify the completed `Content_LavaBlock.prefab`, `Img_Title Variant.prefab`, `UI_LavaRush_BaseEvent.prefab`, `UI_LavaRush_Icon.prefab`, and `UI_LavaRush_Cell.prefab` ownership records, original GUIDs, package-only dependencies, preserved valid local file identifiers and project consumers, intentionally unbound BaseEvent stale targets, required access/block bindings, all five Cell `UI_Text` components with the authored localization/outline values and no duplicate `LocalizeStringEvent`, the Cell `ScalePulse` Indicator with no Animator, authored cell animation duration, and absence of all five legacy local paths.
- Verify the authored prefab has complete serialized references, all visual dependencies stay under this package, and initialization does not create a second fallback Canvas.
- Verify all 14 role prefabs load, every `Image` has a package-owned sprite, the main prefab retains nested dependencies, exactly one of the eight state views is active per model, and rendered screenshots cover start/difficulty/tutorial/match/win/lose/complete/event-end.
- Validate first import, same-version repeat, differing-file conflict, missing dependency, and cancelled Addressables registration in a disposable CatDetective workspace. Never run import validation against a dirty shared checkout.
- In Cat Merge, separately verify existing Addressable UI, event access, save migration, timeout/result recovery, and reward claims because this package does not replace those project adapters automatically.

## Metadata And Release

- `package.json` owns identity, version, Unity version, and exact dependencies.
- `Editor/PackageInfo/ActionFitPackageInfo_SO.asset` owns repository name `LavaRushUI`, Public visibility, Korean description, and the single-version Korean release note.
- Publishing is manual through Custom Package Manager. Do not create a repository, push, tag, or append a catalog row without separate authorization.
