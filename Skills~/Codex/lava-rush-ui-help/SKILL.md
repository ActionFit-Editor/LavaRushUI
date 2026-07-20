---
name: lava-rush-ui-help
description: Explain ActionFit Lava Rush UI, its 14-role modular prefab set, package-owned art, UI-only Embed for Edit workflow, standalone flow, CatDetective Starter, and safety boundaries.
---

# Lava Rush UI Help

Answer in the user's language. Explain workflows without importing the CatDetective sample, creating demo objects, changing Addressables, editing project settings, or publishing packages unless the user separately requests that operation.

1. Read `PACKAGE_SKILLS.md` first and treat its generated package identity, skill list, descriptions, and access values as authoritative.
2. Read `Packages/com.actionfit.lava-rush.ui/README.md` and `AI_GUIDE.md` when embedded. If downloaded, resolve `Library/PackageCache/com.actionfit.lava-rush.ui@*` without editing it.
3. Explain that `LavaRushEngine` remains authoritative and that the UI package owns only immutable view mapping, action routing, the canonical nested `Runtime/Prefabs/Main/UI_LavaRush.prefab`, 14 editable role prefabs, independently authored package sprites/theme, generated missing-or-broken-prefab fallback, replaceable localization/audio/profile/view-host services, and the standalone demo.
4. Explain the customization sequence: install the downloaded bundle first, then explicitly run Custom Package Manager `Embed for Edit` for `com.actionfit.lava-rush.ui` only. State that installers preserve compatible embedded UI edits, report an older embedded UI as a conflict, and never auto-embed or overwrite it.
5. Explain that `Runtime/Prefabs/LavaRushPresentation.prefab` preserves the public compatibility path/GUID while composing the modular main prefab, and that `Documentation~/MigrationCoverage.md` maps all 14 prefab and 56 image roles without copying production assets.
6. Explain that `Samples~/CatDetective Starter` remains inert until explicit import, references the compatibility presentation (and therefore the canonical main/icon/cell composition), and keeps its imported project adapters under `Assets/Contents/LavaRush`. Those imported files are the only files allowed to reference CatDetective `Assembly-CSharp`, `Prefs`, `Main`, `TimeProvider`, `UI_Popup`, and Addressables APIs.
7. Distinguish the read-only preview/preflight from the create-only installer and the separately confirmed Addressables registration. State that any differing target file or address collision blocks the operation without overwrite.
8. Report the exact supported CatDetective dependency versions from the sample README and keep repository, tag, catalog, build, and deployment work outside explanation-only requests.

Always call out that the included catalog, modular prefabs, sprites, and theme are complete editable package defaults; no current production assets are copied or switched. Production reward termination, popup pooling, localization, audio, Addressables, Android, and iOS behavior require consumer-project QA.
