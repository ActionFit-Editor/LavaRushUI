# Lava Rush UI Asset Provenance

The package baseline contains the production Lava Rush visuals from Cat Merge Cafe so a consumer can install a working production-equivalent UI, embed only `com.actionfit.lava-rush.ui`, and then replace images for that project. MCC-1581 removed the final nine local prefab duplicates; every inventoried prefab and image role now has one package owner.

## Original sources

- Prefabs: `Assets/_Project/Content/LavaRush/Prefabs` -> `Runtime/Prefabs`
- Lava Rush PNGs: `Assets/_Project/Content/LavaRush/Images` -> `Runtime/Art`
- Shared visual dependencies referenced by those prefabs: their original project-relative paths -> `Runtime/ProductionDependencies`
- TMP shader includes: `Assets/TextMesh Pro/Shaders/TMPro*.cginc` -> `Runtime/ProductionDependencies/TextMesh Pro/Shaders`

All 56 Lava Rush PNG files preserve their original bytes. TextureImporter settings, including sprite mode/slices, border, pivot, pixels-per-unit, filter/wrap, alpha, mipmaps, compression, and platform overrides, remain equivalent. Each completed migration keeps the original project GUID at the package path and removes the local path in the same AssetDatabase operation. `AssetOwnership.json` is the deterministic completed-unit record.

All 14 production prefab roles are present with their hierarchy, RectTransforms, active states,
visual references, fonts, materials, animations, and UI Foundation wrappers preserved. Version
`0.1.29` also transfers the twelve original-GUID `UI_LavaRush*` controller identities into
`Runtime/Controllers`. Those controllers bind immutable engine snapshots and neutral service
ports to the preserved production controls; project-owned gameplay types are not included in the
runtime assembly.

The copied TMP shaders keep their four original relative include files byte-for-byte. Omitting an include is an incomplete production dependency, not an acceptable project-level TMP fallback.

Original UIEffect, UIParticle, SoftMask, and UILighting components are preserved on the copied prefabs. Their source packages are not copied; the Lava Rush Installer supplies the exact immutable Git dependencies recorded in `ExternalVisualDependencies.md`, including VContainer for UILighting. Removing those components to avoid a dependency would violate the production-equivalent baseline.

Version `0.1.23` makes `Runtime/Prefabs/Main/UI_LavaRush.prefab` the canonical production asset,
preserves the nine original project GUIDs and consumed file identifiers at their package paths,
and removes the final local prefab duplicates. Version `0.1.29` replaces the intermediate
compatibility composition with direct `UI_LavaRush` and eight state controller references. Cat
Merge supplies only product services, reward, analytics, Addressables, navigation, and
EventAccess adapters. The package standalone complete-flow test and
`StandalonePresentationEvidence.json` prove the same canonical prefab reaches final reward
completion through `LavaRushBootstrap` without a generated replacement hierarchy.

Version `0.2.0` is the breaking distribution line for that completed direct-controller baseline.
It changes no inventoried prefab, image, controller, or consumed file identity. The exact
candidate package graph, preserved Addressable GUIDs/keys, consumer bindings, unsupported legacy
view-model path, migration order, and rollback pins are recorded in `ConsumerMigration.md`.

## Prohibited substitutions

No AI-generated image, synthesized art, neutral placeholder, redrawn approximation, variant consolidation, or automatic replacement is permitted in the package baseline. The previously generated package art was removed. Future content packages must follow the same rule: if any required original visual cannot be included, stop and obtain an explicit per-asset decision instead of silently omitting or replacing it.

No new local/package pair may be created. Every completed unit followed Preview, one-unit AssetDatabase transfer, original-GUID preservation, retired-GUID remap, consumer validation, local-path removal, ledger hashing, and rollback-on-failure. A future role change must repeat that sequence instead of recreating a project/package coexistence copy.
