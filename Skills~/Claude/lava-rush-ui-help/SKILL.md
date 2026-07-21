---
name: lava-rush-ui-help
description: Explain ActionFit Lava Rush UI, its original production prefab/image baseline, UI-only Embed for Edit workflow, standalone flow, CatDetective Starter, and safety boundaries.
---

# Lava Rush UI Help

Answer in the user's language. Explain workflows without importing the CatDetective sample, creating demo objects, changing Addressables, editing project settings, or publishing packages unless the user separately requests that operation.

1. Read `PACKAGE_SKILLS.md` first and treat its generated package identity, skill list, descriptions, and access values as authoritative.
2. Read `Packages/com.actionfit.lava-rush.ui/README.md` and `AI_GUIDE.md` when embedded. If downloaded, resolve `Library/PackageCache/com.actionfit.lava-rush.ui@*` without editing it.
3. Explain that `LavaRushEngine` remains authoritative and that the UI package owns only immutable view mapping, action routing, the canonical nested `Runtime/Prefabs/Main/UI_LavaRush.prefab`, all 14 original prefab roles and 56 original PNGs, required visual dependencies, replaceable localization/audio/profile/view-host services, and the standalone demo. Generated UI is diagnostic recovery only.
4. Explain the customization sequence: install the downloaded bundle first, then explicitly run Custom Package Manager `Embed for Edit` for `com.actionfit.lava-rush.ui` only. State that installers preserve compatible embedded UI edits, report an older embedded UI as a conflict, and never auto-embed or overwrite it.
5. Explain that `Runtime/Prefabs/LavaRushPresentation.prefab` preserves the public compatibility path/GUID while composing the production-equivalent main prefab, that `Documentation~/MigrationCoverage.md` inventories all 14 prefab and 56 image roles, that `AssetOwnership.json` records all 56 images plus `Content_LavaBlock.prefab`, `Img_Title Variant.prefab`, `UI_LavaRush_BaseEvent.prefab`, `UI_LavaRush_Icon.prefab`, and `UI_LavaRush_Cell.prefab` as completed single-owner transfers with 9 prefab roles remaining, and that `Documentation~/ExternalVisualDependencies.md` pins every original effect component dependency. Never recommend stripping an effect component to avoid installing it.
6. Explain that `Samples~/CatDetective Starter` remains inert until explicit import, references the compatibility presentation (and therefore the canonical main/icon/cell composition), and keeps its imported project adapters under `Assets/Contents/LavaRush`. Those imported files are the only files allowed to reference CatDetective `Assembly-CSharp`, `Prefs`, `Main`, `TimeProvider`, `UI_Popup`, and Addressables APIs.
7. Distinguish the read-only preview/preflight from the create-only installer and the separately confirmed Addressables registration. State that any differing target file or address collision blocks the operation without overwrite.
8. Report the exact supported CatDetective dependency versions from the sample README and keep repository, tag, catalog, build, and deployment work outside explanation-only requests.

Always call out that the original production prefabs and images are the mandatory editable baseline. AI-generated, synthesized, placeholder, consolidated, redrawn, or automatically substituted visual assets are forbidden; if an original cannot be included, packaging stops for an explicit per-asset decision. Never create another local/package pair: migrate one approved asset at a time, preserve its original GUID at the package path, remove the local path in the same unit, and validate consumers before continuing. Production reward termination, popup pooling, localization, audio, Addressables, Android, and iOS behavior require consumer-project QA.
