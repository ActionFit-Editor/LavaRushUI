# Lava Rush 0.2.1 Consumer Migration

## Release Status

This repository prepares the `0.2.1` release-candidate graph. It does not publish package repositories, create Git tags, append catalog rows, or deploy a build. The Git URLs below become installable only after a separately approved publication flow has created the immutable tags.

The current embedded UI source is `0.2.4`. It includes the serialized action-label localization fix and shared-font move, the `com.actionfit.ui.popup@0.1.2` background physical-input lifetime contract, and `com.actionfit.referencebinding@0.2.2` package-owned Editor processing without consumer enqueue-only `OnValidate`. The exact `0.2.1` installer graph below pins that source and its matching Cat App dependency; do not present it as one-URL installable until a separately approved publication flow creates every immutable tag.

## Exact Candidate Graph

| Package | Exact revision | Role |
| --- | --- | --- |
| `com.actionfit.custompackagemanager` | `1.1.114` | Atomic bundle plan, journal, ownership, repair, release, and bootstrap removal |
| `com.actionfit.content-core` | `0.2.3` | Durable content state and reward contracts |
| `com.actionfit.time` | `1.0.4` | Injected time contracts |
| `com.actionfit.lava-rush` | `0.1.11` | Reusable engine and CSV data |
| `com.actionfit.referencebinding` | `0.2.2` | Package-owned serialized reference processing and validation |
| `com.actionfit.ui.foundation` | `2.0.5` | Global UGUI wrappers used by the original prefabs |
| `com.actionfit.ui.popup` | `0.1.2` | Whole-flow popup queue and background physical-input lifetime owner |
| `com.actionfit.lava-rush.ui` | `0.2.4` | Canonical direct controllers, 14 prefab roles, and 56 original PNGs |
| `com.actionfit.cat.app` | `0.2.2` | Cat product composition and explicit three-key Addressables registration |
| `com.coffee.ui-effect` | `5.10.8` | Original prefab visual dependency |
| `com.coffee.ui-particle` | `4.12.1` | Original prefab visual dependency |
| `com.coffee.softmask-for-ugui` | `3.5.0` | Original prefab visual dependency |
| `com.actionfit.uilighteffector` | full commit `7dab46ec2378209bd1e524c8336b976eccb3df05` | Original prefab visual dependency |
| `jp.hadashikick.vcontainer` | `1.16.8` | UILighting runtime dependency; an installed stable registry version at or above this version is the only compatibility exception |

`com.actionfit.lava-rush.theme.catmerge@0.2.1` is an optional presentation preset. No final Cat Runtime assembly depends on it, so it is not part of the mandatory installer profile.

## Breaking API Change

`LavaRushScreenView` and `LavaRushUIViewModel` are removed production APIs. Do not retain wrappers, copied Runtime scripts, or a second production hierarchy to emulate them.

Use the canonical controller composition instead:

- Load `Runtime/Prefabs/Main/UI_LavaRush.prefab` by the unchanged `UI_LavaRush` key.
- Resolve its `UI_LavaRush` component.
- Initialize it with the one product-owned `LavaRushEngine` and one `LavaRushControllerContext`.
- Use `UI_LavaRush_EventStart`, `UI_LavaRush_Difficulty`, `LavaRushTutorialView`, `UI_LavaRush_Match`, `UI_LavaRush_MatchWin`, `UI_LavaRush_MatchLose`, `UI_LavaRush_MatchEnd`, and `UI_LavaRush_EventEnd` as the serialized direct-controller states.
- Use the unchanged `UI_LavaRush_Icon` and `UI_LavaRush_Cell` identities for EventAccess presentation.

Cat Merge consumers should use `CatLavaRushComposition` as the sole engine/composition authority. The global `LavaRushManager` remains a delegation-only compatibility facade; it must not construct another engine or subscribe to project events.

## Required Cat And Project-Shell Bindings

Cat App owns the product policies and neutral adapters for timing, persistence, catalog conversion, reward receipts, profile roster, audio cues, localization semantics, analytics schema, Order progress, EventAccess, and the one dynamic-controller cache.

The consuming Project Shell still injects physical leaves:

- fixed Cat storage keys, broad flush, generated Lava Rush table rows, and trusted time;
- economy mutation, SDK destinations, generated profile assets, sound clips, and localization tables;
- Addressable handle and outer instance lifecycle, Half canvas, camera, and font policy;
- concrete OrderList, EventAccess, navigation, and scene anchors.

For Cat Merge Cafe the only production seam is `Assets/_Project/_Shared/Main/Base/Main.LavaRush.cs`. Do not recreate production Runtime under `Assets/_Project/Content/LavaRush/Scripts`.

## Preserved Identities And Data

The migration does not rewrite saves or change any of the following:

- package runtime JSON, migration marker, corrupt backup, legacy `lava_rush/*` keys, timer basis, stage/result state, or shared reward receipts;
- `UI_LavaRush` prefab GUID `ffae8bfdd6acf4657b158ff432e5a23b`;
- `UI_LavaRush_Icon` prefab GUID `f7a017bca31e14a2eae90bc3a60cd5e3`;
- `UI_LavaRush_Cell` prefab GUID `800bfcd600b24494eb593e8f6ed492b1`;
- the exact Addressable keys `UI_LavaRush`, `UI_LavaRush_Icon`, and `UI_LavaRush_Cell`;
- the original prefab hierarchy, consumed fileIDs, 14 production roles, and 56 PNG baseline.

## Upgrade Sequence

1. Back up `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/ActionFitContentBundles.json`, `UserSettings/ActionFitPackageManager/ContentBundleTransactions`, Addressables settings, and any embedded UI edits. Commit or otherwise snapshot the consuming project before mutation.
2. Remove or merge project production paths only through the completed one-authority migration. Confirm that no production `LavaRushScreenView`, `LavaRushUIViewModel`, duplicate engine, or `Assets/_Project/Content/LavaRush/Scripts` Runtime remains.
3. After separate publication approval has produced every immutable tag, install `https://github.com/ActionFit-Editor/LavaRushInstaller.git#0.2.1`. Let Custom Package Manager complete package registration, durable ownership reload, and installer self-removal.
4. Run `Tools > Package > Cat App > Lava Rush Addressables > Preview Registration`. This step is read-only.
5. Resolve every collision or incompatible group policy without moving or overwriting user entries. Then run `Apply Missing Entries` and approve the separate create-only operation.
6. Bind the Cat product composition and Project Shell inputs. Compile and validate one engine, one subscription set, one dynamic controller, unchanged saves, and reward replay.
7. If project-specific visual edits are required, use Custom Package Manager to `Embed for Edit` **only** `com.actionfit.lava-rush.ui`. Preserve a compatible embedded package. An older embedded UI is a blocking conflict that requires a manual merge.
8. Install `com.actionfit.lava-rush.theme.catmerge@0.2.1` only when the optional preset is wanted.

Installer install/repair, package import, Editor startup, and batchmode never modify Addressables settings. Addressables apply creates only missing entries in the existing writable bundled default group, preserves all existing entries/groups/labels/addresses, and rolls back entries created by a failed attempt.

## Validation

- Confirm the installer profile contains the exact fourteen required entries above, uses canonical HTTPS repositories and immutable revisions, keeps its own `package.json` dependencies empty, and leaves the Editor asmdef without Runtime package references.
- Exercise first install, same-version repeat, repair, older canonical upgrade, newer canonical preservation, cancellation, dependency conflict, journal recovery, self-removal, release, shared-dependency preservation, and rollback.
- Exercise three missing entries, three matching entries, mixed current/missing entries, address collision, canonical GUID under another address, missing settings/default group, cancelled apply, and injected registration failure.
- Verify all twelve controller script GUIDs, fourteen prefab roles, fifty-six PNGs, consumed fileIDs, three canonical prefab GUIDs, and package-only Runtime references.
- Load legacy active-event, pending-result, pending-reward, and interrupted-receipt fixtures. Confirm no progress loss or duplicate grant.
- Confirm `LavaRushScreenView` and `LavaRushUIViewModel` have no supported production path and no local Runtime copy was introduced.

## Rollback

Before package publication, rollback is a source-control revert to the pre-migration project snapshot; the `0.2.1` Git URLs are not installable evidence until their immutable tags exist.

After publication, use Custom Package Manager's recorded bundle ownership to release or restore the exact pre-install manifest values. It preserves shared dependencies, embedded packages, and user-modified values. Restore the Addressables and project snapshots if the consumer applied new entries, because bundle release intentionally does not mutate Addressables.

The last published one-URL pre-0.2 bundle is installer `0.1.16` with manager `1.1.113`, Content Core `0.2.3`, Time `1.0.4`, engine `0.1.10`, UI Foundation `2.0.4`, UI Popup `0.1.1`, UI `0.1.23`, and the same immutable visual dependency set. Downgrading to it also requires restoring the matching pre-one-authority project Runtime from source control. A package-only downgrade after removing `LavaRushScreenView`/`LavaRushUIViewModel` is unsupported, and keeping both UI architectures active is never a rollback strategy.
