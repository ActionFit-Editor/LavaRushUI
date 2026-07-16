---
name: lava-rush-ui-help
description: Explain ActionFit Lava Rush UI, its neutral standalone flow, project service boundaries, CatDetective Starter installer and preflight, imported popup and adapters, optional Addressables registration, and safety boundaries.
---

# Lava Rush UI Help

Answer in the user's language. Explain workflows without importing the CatDetective sample, creating demo objects, changing Addressables, editing project settings, or publishing packages unless the user separately requests that operation.

1. Read `PACKAGE_SKILLS.md` first and treat its generated package identity, skill list, descriptions, and access values as authoritative.
2. Read `Packages/com.actionfit.lava-rush.ui/README.md` and `AI_GUIDE.md` when embedded. If downloaded, resolve `Library/PackageCache/com.actionfit.lava-rush.ui@*` without editing it.
3. Explain that `LavaRushEngine` remains authoritative and that the UI package owns only immutable view mapping, action routing, generated UGUI presentation, replaceable localization/audio/profile/view-host services, and the standalone demo.
4. Explain that `Samples~/CatDetective Starter` remains inert until explicit import. Imported files become project-owned under `Assets/Contents/LavaRush` and are the only files allowed to reference CatDetective `Assembly-CSharp`, `Prefs`, `Main`, `TimeProvider`, `UI_Popup`, and Addressables APIs.
5. Distinguish the read-only preview/preflight from the create-only installer and the separately confirmed Addressables registration. State that any differing target file or address collision blocks the operation without overwrite.
6. Report the exact supported CatDetective dependency versions from the sample README and keep repository, tag, catalog, build, and deployment work outside explanation-only requests.

Always call out that the included catalog and neutral visual are smoke-test defaults, and that production reward termination, popup pooling, localization, audio, Addressables, Android, and iOS behavior require consumer-project QA.
