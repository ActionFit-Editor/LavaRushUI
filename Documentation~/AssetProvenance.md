# Lava Rush UI Asset Provenance

The package baseline contains the production Lava Rush visuals from Cat Merge Cafe so a consumer can install a working production-equivalent UI, embed only `com.actionfit.lava-rush.ui`, and then replace images for that project. Existing dual-owned rows are migration debt, not a template for creating new copies.

## Original sources

- Prefabs: `Assets/_Project/Content/LavaRush/Prefabs` -> `Runtime/Prefabs`
- Lava Rush PNGs: `Assets/_Project/Content/LavaRush/Images` -> `Runtime/Art`
- Shared visual dependencies referenced by those prefabs: their original project-relative paths -> `Runtime/ProductionDependencies`
- TMP shader includes: `Assets/TextMesh Pro/Shaders/TMPro*.cginc` -> `Runtime/ProductionDependencies/TextMesh Pro/Shaders`

All 56 Lava Rush PNG files preserve their original bytes. TextureImporter settings, including sprite mode/slices, border, pivot, pixels-per-unit, filter/wrap, alpha, mipmaps, compression, and platform overrides, remain equivalent. Each completed migration keeps the original project GUID at the package path and removes the local path in the same AssetDatabase operation. `AssetOwnership.json` is the deterministic completed-unit record.

All 14 production prefab roles are present with their hierarchy, RectTransforms, active states, visual references, fonts, materials, animations, and UI Foundation wrappers preserved. Project-owned gameplay MonoBehaviours are not included in the runtime assembly; `LavaRushScreenView` and `LavaRushActionTarget` bind immutable engine state and callbacks to the preserved production controls.

The copied TMP shaders keep their four original relative include files byte-for-byte. Omitting an include is an incomplete production dependency, not an acceptable project-level TMP fallback.

Original UIEffect, UIParticle, SoftMask, and UILighting components are preserved on the copied prefabs. Their source packages are not copied; the Lava Rush Installer supplies the exact immutable Git dependencies recorded in `ExternalVisualDependencies.md`, including VContainer for UILighting. Removing those components to avoid a dependency would violate the production-equivalent baseline.

Version `0.1.16` keeps the project and package Match prefabs aligned while intentionally setting only `Txt_Tutorial1` through `Txt_Tutorial3` to `Maskable = false`. The package's legacy localization event and existing `UI_Text` component settings, plus the project prefab's `UI_Text` localization/outline configuration, remain unchanged, as do the authored parent SoftMask, hierarchy, references, and staged tutorial behavior.

## Prohibited substitutions

No AI-generated image, synthesized art, neutral placeholder, redrawn approximation, variant consolidation, or automatic replacement is permitted in the package baseline. The previously generated package art was removed. Future content packages must follow the same rule: if any required original visual cannot be included, stop and obtain an explicit per-asset decision instead of silently omitting or replacing it.

No new local/package pair may be created. The first approved unit moved `Assets/_Project/Content/LavaRush/Images/resource/Main_icon.png` to `Runtime/Art/resource/Main_icon.png`, preserved GUID `756239e4572274b17b3fcae6f4964bdb`, remapped the retired package GUID, and removed the local path. Remaining roles must repeat Preview, one-unit transfer, consumer validation, and rollback evidence before they are marked package-owned.
