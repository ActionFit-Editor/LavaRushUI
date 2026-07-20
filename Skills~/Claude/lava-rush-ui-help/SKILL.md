---
name: lava-rush-ui-help
description: Explain ActionFit Lava Rush UI, its authored prefab and neutral art, UI-only Embed for Edit workflow, standalone flow, project service boundaries, CatDetective Starter, and safety boundaries.
---

# Lava Rush UI Help

Answer in the user's language. Explain workflows without importing the CatDetective sample, creating demo objects, changing Addressables, editing project settings, or publishing packages unless the user separately requests that operation.

1. Read `PACKAGE_SKILLS.md` first and treat its generated package identity, skill list, descriptions, and access values as authoritative.
2. Read `Packages/com.actionfit.lava-rush.ui/README.md` and `AI_GUIDE.md` when embedded. If downloaded, resolve `Library/PackageCache/com.actionfit.lava-rush.ui@*` without editing it.
3. Explain that `LavaRushEngine` remains authoritative and that the UI package owns only immutable view mapping, action routing, the canonical authored UGUI prefab, ActionFit-owned neutral placeholder sprites/theme, generated missing-or-broken-prefab fallback, replaceable localization/audio/profile/view-host services, and the standalone demo.
4. Explain the customization sequence: install the downloaded bundle first, then explicitly run Custom Package Manager `Embed for Edit` for `com.actionfit.lava-rush.ui` only. State that installers preserve compatible embedded UI edits, report an older embedded UI as a conflict, and never auto-embed or overwrite it.
5. Explain that `Samples~/CatDetective Starter` remains inert until explicit import, references the canonical package prefab, and keeps its imported project adapters under `Assets/Contents/LavaRush`. Those imported files are the only files allowed to reference CatDetective `Assembly-CSharp`, `Prefs`, `Main`, `TimeProvider`, `UI_Popup`, and Addressables APIs.
6. Distinguish the read-only preview/preflight from the create-only installer and the separately confirmed Addressables registration. State that any differing target file or address collision blocks the operation without overwrite.
7. Report the exact supported CatDetective dependency versions from the sample README and keep repository, tag, catalog, build, and deployment work outside explanation-only requests.

Always call out that the included catalog, authored prefab, sprites, and theme are editable smoke-test defaults; no current production assets are copied or switched. Production reward termination, popup pooling, localization, audio, Addressables, Android, and iOS behavior require consumer-project QA.
