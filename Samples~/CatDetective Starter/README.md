# CatDetective Lava Rush Starter

This sample is project-owned after import. It connects the reusable Lava Rush engine and neutral UI to the current `AF_CatDetective` `Assembly-CSharp` APIs without adding any CatDetective dependency to a package assembly.

## Supported baseline

- Unity `6000.3.9f1`
- `com.actionfit.content-core@0.2.0`
- `com.actionfit.lava-rush@0.1.1`
- `com.actionfit.lava-rush.ui@0.1.1`
- `com.actionfit.time@1.0.2`
- `com.unity.addressables@2.8.1`
- `com.unity.ugui@2.0.0`

## First use

1. Run `Tools > Package > ActionFit Lava Rush UI > Run CatDetective Starter Preflight`.
2. Review `Resources/CatDetectiveLavaRushSettings.asset`. The included catalog and all-day schedule are smoke-test defaults, not production balance.
3. Run `Tools > Package > ActionFit Lava Rush UI > CatDetective Addressables > Preview Registration`.
4. If the `UI_LavaRush` address is free, run `Register Popup` and approve the serialized Addressables change.
5. Wait until `Main.Resource` has loaded the `base` label, then open the popup with `Main.UI.OpenPopup<UI_LavaRush>()`.

The neutral standalone route remains available through `Tools > Package > ActionFit Lava Rush UI > Create Demo` and does not use CatDetective persistence or rewards.

## Ownership and persistence

- `CatDetectiveLavaRushStateStore` saves engine JSON in `SimplePrefs` under the `lava_rush.state.` prefix.
- `CatDetectiveLavaRushRewardService` maps package reward IDs to `RewardItem`, calls `Main.Data.AddRewardItem`, and records confirmed transaction IDs in `SimplePrefs`.
- `CatDetectiveLavaRushClock` reads only `TimeProvider.UtcNow` and `TimeProvider.Now`; it does not introduce direct device-time reads.
- Localization routes are opt-in mappings to the CatDetective `General` table. Without a mapping, Korean uses starter-owned strings selected through `Main.Locale.LocaleCode` and other locales use the package English fallback, avoiding missing-key errors.
- Audio routes map package cues to `SFXType`; the default screen/progress/reward routes use the existing `Reward` and `Progress` clips and can be replaced in settings.
- The starter catalog, reward routes, schedule days, profile fallback, localization routes, and audio routes remain project-owned in `CatDetectiveLavaRushSettings.asset`.

## Upgrade and conflict behavior

The package installer creates only missing files. Re-running the same version is a no-op. If any target file differs, the entire installation is blocked and the existing project file is preserved. Compare the new sample manually and port desired changes; do not delete or overwrite project-owned code automatically.

Addressables registration is separate from file installation, always previews the prefab, address, label, and group, and refuses an address owned by another asset. It does not replace another entry.

## Removal

Before removing the imported folder, remove the `UI_LavaRush` Addressables entry through the normal Addressables Groups window and verify no game code opens the popup. Removing this folder does not delete existing `SimplePrefs` state or granted reward transaction history.

## Known limitations

- The included visual is the neutral generated UGUI presentation, not final CatDetective art.
- The default catalog is for smoke tests only.
- `Prefs.SaveAll(true)` reduces the reward/ledger persistence window but CatDetective does not expose a single atomic transaction across economy mutation and the separate `SimplePrefs` ledger. Production economy QA must include forced termination around reward confirmation.
- Repository creation, tags, releases, catalog rows, Android/iOS builds, and deployment are not performed by this sample.
