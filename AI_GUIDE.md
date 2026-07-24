# AI Guide - ActionFit Lava Rush UI

This guide ships with the package so an AI assistant preserves the exact production visual baseline and asset boundary in a consuming project.

## Package Identity

- Package ID: `com.actionfit.lava-rush.ui`
- Display name: ActionFit Lava Rush UI
- Repository: `https://github.com/ActionFit-Editor/LavaRushUI.git`
- Repository visibility: Public
- Current package version at generation time: `0.2.4`
- Unity version: `6000.2`
- Declared runtime dependencies: `com.actionfit.content-core@0.2.3`, `com.actionfit.fonts.maplestory@1.0.0`, `com.actionfit.lava-rush@0.1.11`, `com.actionfit.referencebinding@0.2.2`, `com.actionfit.time@1.0.4`, `com.actionfit.ui.foundation@2.0.5`, `com.actionfit.ui.popup@0.1.2`, `com.unity.localization@1.5.5`, `com.unity.modules.animation@1.0.0`, and `com.unity.ugui@2.0.0`
- Required bundle-level visual dependencies: `com.coffee.ui-effect@5.10.8`, `com.coffee.ui-particle@4.12.1`, `com.coffee.softmask-for-ugui@3.5.0`, `com.actionfit.uilighteffector@1.0.0` at full commit `7dab46ec2378209bd1e524c8336b976eccb3df05`, and `jp.hadashikick.vcontainer@1.16.8`

## Purpose And Boundary

The package provides the complete production-equivalent UGUI baseline: all 14 original Lava Rush prefab roles, all 56 original Lava Rush PNGs, required visual dependencies, the restored controller family, and a standalone composition root. Original image bytes and importer behavior are preserved. Completed migration units preserve the original project GUID at one package path with no local duplicate. Runtime-generated replacement screens are unsupported.

The package does not own schedule, stage, timeout, rank, reward, serialization, or recovery rules. Those remain in `LavaRushEngine`. It does not reference `Assembly-CSharp`, Cat Merge gameplay APIs, Addressables, analytics, persistence, DOTween, or UniTask. UI Foundation and copied visual assets are package-owned. Original third-party effect components remain intact and their immutable Git dependencies are installed by the bundle because UPM does not support nested Git URLs in this package's `package.json`.

The optional `Samples~/CatDetective Starter` is the only CatDetective-specific surface. Its scripts remain outside every package assembly until an operator explicitly imports them under the consuming project's `Assets/Contents/LavaRush` path. Imported files are project-owned and may use CatDetective `Assembly-CSharp`, `Prefs`, `Main`, `TimeProvider`, `UI_Popup`, and Addressables APIs.

## Project Router Registration

Requested router entry:

- `Packages/com.actionfit.lava-rush.ui/AI_GUIDE.md` - ActionFit Lava Rush UI owns the production-equivalent UGUI baseline, restored direct controllers, replaceable project service boundaries, and standalone PlayerPrefs demo bootstrap.

## Runtime Architecture

- `UI_LavaRush` is the canonical package controller. It receives a `LavaRushControllerContext`, reads only `LavaRushEngine` state, owns the eight-screen transition family, and routes `LavaRushUIAction` requests to public engine commands.
- `UI_LavaRush` owns one optional UI Popup background-input block for a directly activated visible screen flow. It retains the handle across its eight internal screen transitions and releases it on `HideAll`, disable, or destruction. A normally opened `ViewController` still follows the UI Popup package's own visible-lifetime contract.
- `LavaRushControllerView` and the eight concrete state controllers own serialized view references, immutable `LavaRushControllerSnapshot` rendering, and button callbacks. They do not write persistence or grant rewards.
- `UI_LavaRush_Icon` and `UI_LavaRush_Cell` own package presentation and neutral access/countdown/progress ports. Project EventAccess adapters are separate components.
- `LavaRushBlockView` is the thin package-owned binder for the authored production block prefab. It owns only serialized visual references, presentation setters, and the reward-info callback; item lookup, amount formatting, collection navigation, and concrete player/enemy profile prefabs remain consuming-project adapters. The package owns only the neutral `ILavaRushProfileGroupFactory` creation seam and created-view lifetime.
- `Runtime/Prefabs/Main/UI_LavaRush.prefab` is the engine-backed canonical production composition and directly references all eight state controllers. No compatibility presentation or generated fallback hierarchy participates in production.
- Production controllers use the declared UI Foundation `UI_Text`/`UI_Button` contracts.
- `LavaRushActionTarget` preserves a valid serialized `UI_Text` or `LocalizeStringEvent` result as the action-label authority. The model's English label is only a missing-localization fallback.
- The demo clock, schedule, and catalog are standalone fixtures. Its calendar uses `TimeZoneInfo.Local`; production projects inject their own engine, calendar policy, and project adapters.

## Extension Rules

- Prefer a prefab and serialized theme/config for visual-only customization.
- Edit the role prefab under `Runtime/Prefabs/Base`, `Icon`, `Main`, or `UI`. Keep all mandatory `Image.sprite`, controller `refs`, and canonical Main references valid.
- For project-specific visual work, explicitly use Custom Package Manager `Embed for Edit` on `com.actionfit.lava-rush.ui` only. Never auto-embed it, embed the engine/shared packages for this purpose, or overwrite compatible embedded edits during install, repair, upgrade, or release.
- Compatibility exception: when a consuming project already owns different global `UI_Image`, `UI_Text`, or `UI_Button` sources, perform a read-only GUID/type audit first. Preserve those project scripts and GUIDs, embed UI Foundation project-locally, and set only its Runtime asmdef `autoReferenced` to false so Lava Rush UI keeps its explicit assembly reference without exposing duplicate types to `Assembly-CSharp`. Document this project override; never automate source deletion or prefab/scene migration.
- Use `ILavaRushFrameScheduler`, `ILavaRushCountdownScheduler`, `ILavaRushAudio`, `ILavaRushProfileRoster`, `ILavaRushProfileGroupView`, `ILavaRushUILocalizer`, `ILavaRushUIRewardRenderer`, `ILavaRushOrderProgressSource`, `ILavaRushAccessService`, and `ILavaRushProgressView` at a real cross-assembly project boundary. Do not add a general service locator.
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
- Version `0.1.17` pins engine `0.1.9` so the production presentation and standalone composition use the released canonical CSV catalog boundary. Supported package-prefab fields use stable `RequiredReference` and `AutoWireChild` contracts, while unsupported shapes such as `RectTransform panel` remain under explicit serialized-reference tests. It records all 56 images as single-owner units in `AssetOwnership.json`; every original image resolves at its package path with the original project GUID and no local image duplicate. It also makes `Img_Title Variant.prefab`, `UI_LavaRush_BaseEvent.prefab`, `UI_LavaRush_Icon.prefab`, `UI_LavaRush_Cell.prefab`, and `Content_LavaBlock.prefab` single-owner prefab units while preserving their original GUIDs and consumed local file identifiers. The title and flattened BaseEvent retain their valid legacy consumer identifiers; three previously serialized no-op BaseEvent targets remain intentionally unbound because Unity already ignored them before migration. Package-owned icon/cell/block binders own required presentation references and callbacks without Cat Merge services. Cat Merge retains item lookup, amount formatting, collection navigation, player/enemy profile groups, and EventAccess behavior adapters. Original bytes, importer metadata, rendered presentation behavior, authored prefab values, and the existing Match consumer reference are preserved; the five former local prefab paths, all local image duplicates, and the former `Content_LavaBlock` script are removed after characterization and consumer validation.
- Version `0.1.18` restores the original `UI_Text` components and authored material/localization settings on the access icon timer plus the in-game cell timer, status, and nested localized status text. It preserves their original local file identifiers. The cell Indicator retains its GameObject, Transform, active-state policy, and component file identifier while replacing the broken Animator binding with UI Foundation `ScalePulse`. Package binders and Cat Merge adapters use the concrete `UI_Text` and `ScalePulse` types, while the shared EventAccess countdown formatter, time source, registration, cancellation, and expiry behavior remain unchanged.
- Version `0.1.19` restores the flattened Cell `Txt_ReaminCount` outline and `Txt_RemainTitle` localization/outline Inspector values from the production nested-prefab overrides, normalizes both `UI_Text` assembly identifiers, and removes the two duplicate legacy `LocalizeStringEvent` components. The Cat Merge Difficulty description now explicitly overrides `Img_Desc` with the package-owned original `Popup_textboard.png` instead of the DP preview image; hierarchy, bindings, timers, and interactions remain unchanged.
- Version `0.1.20` classifies every `Runtime/Art/DP` image as a full-screen design preview that production prefabs must not consume. Cat Merge Match End, Match Win, and Match Lose now use the BaseEvent `Img_Desc` original `Popup_textboard.png`; hierarchy, alpha, transforms, bindings, and interactions remain unchanged, and an exhaustive project/package prefab dependency test blocks future DP references.
- Version `0.1.21` restores `Runtime/Prefabs/Base/Img_Title Variant.prefab` as a nested prefab backed only by package-owned internal image and text bases. It preserves the original role GUID and Match-consumed local file identifiers while restoring the package font material, `UI_Text` localization, Outline `0.1`, and authored Underlay color/offset without a duplicate `LocalizeStringEvent`. Regression coverage requires all nested sources and visual dependencies to remain inside packages rather than a consuming project's `Assets` tree.
- Version `0.1.22` restores `Runtime/Prefabs/Base/Content_LavaBlock.prefab` `Mask_SeatPanel.expandedHeight` to its authored RectTransform height of `180`. The completed seat-panel reveal retains vertical mask room around `Img_SeatPanel` instead of shrinking to the image height and clipping its border; original image bytes, hierarchy, references, animation timing, and gameplay behavior remain unchanged.
- Version `0.1.23` completes the final nine single-owner prefab transfers in order: Difficulty, EventEnd, EventStart, Match, MatchEnd, MatchLose, MatchWin, Tutorial, then Main. Each package target preserves the original project GUID and consumed file identifiers, the retired package-copy GUID is remapped, and the verified local prefab path is absent. `StandalonePresentationEvidence.json` points to a canonical-prefab complete-flow test. Version `0.1.29` supersedes the former compatibility composition with direct restored controllers. Cat Merge keeps project-only services and loads the same canonical Main through the preserved `UI_LavaRush` Addressable key.
- Version `0.1.24` points the authored Cell title at the canonical `lavarush_title` General Shared Data ID and retires only the duplicate `lava_rush_icon` ID. Prefab GUIDs, fileIDs, hierarchy, art, and runtime behavior remain unchanged.
- Version `0.1.25` changes only the authored TMP fill color of `Runtime/Prefabs/Icon/UI_LavaRush_Icon.prefab` `Txt_Timer` from opaque white to opaque black. The existing `UI_Text` outline, font, material, text, hierarchy, bindings, GUID, Addressable contract, and runtime behavior remain unchanged; `AssetOwnership.json` records the resulting prefab SHA.
- Version `0.1.26` adds the consumer-owned `ILavaRushFrameScheduler` and `ILavaRushCountdownScheduler` ports, the accumulated-hour `LavaRushTimeText` formatter, and a standalone Unity/ActionFit Time implementation. Cat composition replaces these defaults through `com.actionfit.cat.app`; controller migration remains a later execution unit.
- Version `0.1.27` adds project-neutral audio cues, immutable player/opponent snapshots and roster/view ports, plus 18 semantic localization keys with standalone fallbacks. Cat profile, sound, General-table, and SDK policies stay in `com.actionfit.cat.app` or Project Shell leaves; version `0.1.29` composes these ports into the canonical direct controller.
- Version `0.1.28` adds Main-free Order progress, access-state, and progress-view ports. Cat score/provider/effect policy, explicit EventAccess key/type/slot binding, and the outer Addressable/controller lifetime stay in `com.actionfit.cat.app` or Project Shell; version `0.1.29` binds timing, order, access, and product services at the production controller context.
- Version `0.1.29` completes MCC-1630: twelve original `UI_LavaRush*` controller identities move to `Runtime/Controllers` with their GUIDs preserved, canonical Main/Icon/Cell bind those controllers directly, and the former local controller copies are absent. `UI_LavaRush` uses `LavaRushEngine` as the sole state authority and consumes only neutral timing, audio, profile, profile-group factory, localization, reward, order, and access seams. Cat Merge retains Addressables and product adapters; standalone instantiates the same canonical Main without generated screens. The concrete Cat profile-prefab factory selection remains a final composition handoff rather than a reusable package dependency.
- Version `0.2.0` is the breaking distribution baseline for that restored controller architecture. It pins engine `0.1.11`, ReferenceBinding `0.2.1`, and UI Foundation `2.0.5`; it does not restore `LavaRushScreenView`, `LavaRushUIViewModel`, or a second production compatibility hierarchy. `Documentation~/ConsumerMigration.md` is the canonical upgrade, Cat binding, save/Addressable preservation, validation, and rollback guide.
- Version `0.2.1` declares `com.actionfit.fonts.maplestory@1.0.0`; the preserved LavaRush SDF/material GUIDs resolve from the shared owner, keep atlas padding `8`, and use the one canonical Bold source. Do not restore package-local font binaries.
- Version `0.2.2` directly declares Unity Localization `1.5.5` and preserves serialized localized action labels, with English model labels used only when no usable localized value exists. It does not change keys, locale assets, prefab bindings, or the visual baseline. The `0.2.0` installer candidate does not include this patch until a separately approved graph update and publication flow.
- Version `0.2.3` pins UI Popup `0.1.2` and makes the direct `UI_LavaRush` controller flow retain one background physical-input block across screen transitions. `HideAll`, disable, and destruction release the owned handle idempotently; popup buttons, prefab identities, serialization, visual assets, engine state, and reward behavior remain unchanged. The `0.2.0` installer candidate does not include this patch until a separately approved graph update and publication flow.
- Version `0.2.4` pins `com.actionfit.referencebinding@0.2.2` and relies on its package-owned Editor pump instead of enqueue-only consumer `OnValidate` declarations. The `0.2.3` background physical-input lifetime remains intact; prefabs, serialized references, GUIDs, fileIDs, Addressable identities, engine state, reward behavior, and other runtime behavior remain unchanged.
- Existing project Addressable keys `UI_LavaRush`, `UI_LavaRush_Icon`, and `UI_LavaRush_Cell` remain project-owned compatibility contracts.

## Package Tools Menu

- `Tools/Package/ActionFit Lava Rush UI/Create Demo`: instantiates the package-authored demo prefab. Missing demo/canonical controller assets are reported as errors; generated fallback creation is disabled.
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
- Verify all 14 prefab and 56 image ownership records, original GUIDs, ledger hashes, package-only dependencies, preserved valid local file identifiers and package/project consumers, intentionally unbound BaseEvent stale targets, required access/block bindings, all five Cell `UI_Text` components with the authored localization/outline values and no duplicate `LocalizeStringEvent`, the Cell `ScalePulse` Indicator with no Animator, authored cell animation duration, and absence of every migrated legacy local path.
- Verify the authored prefab has complete serialized references, all visual dependencies stay under this package, and initialization does not create a second fallback Canvas.
- Verify all 14 role prefabs load, every `Image` has a package-owned sprite, the canonical Main directly references eight nested state controllers, the canonical prefab can complete the standalone engine flow, exactly one state controller is active per engine state, and rendered comparison evidence covers start/difficulty/tutorial/match/win/lose/complete/event-end.
- Validate first import, same-version repeat, differing-file conflict, missing dependency, and cancelled Addressables registration in a disposable CatDetective workspace. Never run import validation against a dirty shared checkout.
- In Cat Merge, separately verify existing Addressable UI, event access, save migration, timeout/result recovery, and reward claims because this package does not replace those project adapters automatically.

## Metadata And Release

- `package.json` owns identity, version, Unity version, and exact dependencies.
- `Editor/PackageInfo/ActionFitPackageInfo_SO.asset` owns repository name `LavaRushUI`, Public visibility, Korean description, and the single-version Korean release note.
- Publishing is manual through Custom Package Manager. Do not create a repository, push, tag, or append a catalog row without separate authorization.
