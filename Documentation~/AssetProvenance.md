# Lava Rush UI Asset Provenance

The package baseline is an additive copy of the production Lava Rush visuals already present in Cat Merge Cafe. It exists so a consumer can install a working production-equivalent UI, embed only `com.actionfit.lava-rush.ui`, and then replace images for that project.

## Original sources

- Prefabs: `Assets/_Project/Content/LavaRush/Prefabs` -> `Runtime/Prefabs`
- Lava Rush PNGs: `Assets/_Project/Content/LavaRush/Images` -> `Runtime/Art`
- Shared visual dependencies referenced by those prefabs: their original project-relative paths -> `Runtime/ProductionDependencies`
- TMP shader includes: `Assets/TextMesh Pro/Shaders/TMPro*.cginc` -> `Runtime/ProductionDependencies/TextMesh Pro/Shaders`

All 56 Lava Rush PNG files are copied byte-for-byte. TextureImporter settings, including sprite mode/slices, border, pivot, pixels-per-unit, filter/wrap, alpha, mipmaps, compression, and platform overrides, remain equivalent. Package copies receive new GUIDs only so they can coexist with the unchanged source assets; prefab references are deterministically remapped to package copies.

All 14 production prefab roles are copied with their hierarchy, RectTransforms, active states, visual references, fonts, materials, animations, and UI Foundation wrappers preserved. Project-owned gameplay MonoBehaviours are not copied into the runtime assembly; `LavaRushScreenView` and `LavaRushActionTarget` bind immutable engine state and callbacks to the preserved production controls.

The copied TMP shaders keep their four original relative include files byte-for-byte. Omitting an include is an incomplete production dependency, not an acceptable project-level TMP fallback.

Original UIEffect, UIParticle, SoftMask, and UILighting components are preserved on the copied prefabs. Their source packages are not copied; the Lava Rush Installer supplies the exact immutable Git dependencies recorded in `ExternalVisualDependencies.md`, including VContainer for UILighting. Removing those components to avoid a dependency would violate the production-equivalent baseline.

## Prohibited substitutions

No AI-generated image, synthesized art, neutral placeholder, redrawn approximation, variant consolidation, or automatic replacement is permitted in the package baseline. The previously generated package art was removed. Future content packages must follow the same rule: if any required original visual cannot be included, stop and obtain an explicit per-asset decision instead of silently omitting or replacing it.

The source prefabs, images, `.meta` GUIDs, and Addressable keys remain unchanged. Copying them into this package does not authorize deleting, moving, or switching the production source; those are separate migration decisions.
