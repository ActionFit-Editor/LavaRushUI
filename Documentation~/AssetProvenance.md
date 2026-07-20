# Lava Rush UI Asset Provenance

The package defaults are original ActionFit assets authored for the public Lava Rush UI package. No file below `Assets/_Project/Content/LavaRush`, Cat Merge Cafe, CatDetective, a marketplace package, or another product was used as an image input or copied into the package.

## MCC-1551 illustrated assets

| Package asset | Method | Source boundary |
| --- | --- | --- |
| `Runtime/Art/LavaRushBackdrop.png` | OpenAI image generation from an original ActionFit brief; resized for package use | No input image; no existing game art reference |
| `Runtime/Art/LavaRushExplorer.png` | OpenAI image generation from an original ActionFit brief; resized for package use | No input image; original explorer-cat design |
| `Runtime/Art/LavaRushRewardBadge.png` | OpenAI image generation from an original ActionFit brief; resized for package use | No input image; original volcanic reward chest design |

The prompts required original designs, prohibited logos, trademarks, watermarks, text, and copying existing game art. Generated originals remain outside the Unity repository; the optimized package PNGs are the canonical distributable copies.

## Deterministic UI geometry

The following PNGs were generated from ActionFit-authored color/shape code: rounded panels and frames, lava gradients, progress elements, difficulty pips, tutorial steps, timer, stage nodes, result bursts, and lava-block geometry.

- `Runtime/Art/LavaRushAccent.png`
- `Runtime/Art/LavaRushPanel.png`
- `Runtime/Art/LavaRushPrimaryButton.png`
- `Runtime/Art/LavaRushSecondaryButton.png`
- `Runtime/Art/LavaRushProgressTrack.png`
- `Runtime/Art/LavaRushProgressFill.png`
- `Runtime/Art/LavaRushCellFrame.png`
- `Runtime/Art/LavaRushIconFrame.png`
- `Runtime/Art/LavaRushTitleRibbon.png`
- `Runtime/Art/LavaRushLavaBlock.png`
- `Runtime/Art/LavaRushDifficultyEasy.png`
- `Runtime/Art/LavaRushDifficultyNormal.png`
- `Runtime/Art/LavaRushDifficultyHard.png`
- `Runtime/Art/LavaRushTutorialGuide.png`
- `Runtime/Art/LavaRushTimerBadge.png`
- `Runtime/Art/LavaRushStageNode.png`
- `Runtime/Art/LavaRushWinBurst.png`
- `Runtime/Art/LavaRushLoseCrack.png`

## Authored Unity assets

ActionFit authored the 14 modular role prefabs under `Runtime/Prefabs/Base`, `Icon`, `Main`, and `UI`, plus `Runtime/Prefabs/LavaRushPresentation.prefab`, `Runtime/Prefabs/LavaRushDemo.prefab`, and `Runtime/Themes/LavaRushNeutralTheme.asset`. `Documentation~/MigrationCoverage.md` records the production-role inventory and the independent package counterpart for each role.

A consuming project may explicitly use Custom Package Manager `Embed for Edit` on `com.actionfit.lava-rush.ui` and replace these files. Existing production assets remain in their project-owned locations and require separate rights evidence plus explicit GUID/reference migration approval before any move, copy, or production switch.
