# Lava Rush UI Migration Coverage

This matrix records MCC-1551 visual-role coverage without copying any file from `Assets/_Project/Content/LavaRush`. The left side is inventory evidence only. The right side is the independently authored, redistribution-safe package counterpart used by the canonical presentation.

## Prefab roles (14/14)

| Production role | Package-owned counterpart |
| --- | --- |
| `Prefabs/Base/Content_LavaBlock.prefab` | `Runtime/Prefabs/Base/Content_LavaBlock.prefab` |
| `Prefabs/Base/Img_Title Variant.prefab` | `Runtime/Prefabs/Base/Img_Title Variant.prefab` |
| `Prefabs/Base/UI_LavaRush_BaseEvent.prefab` | `Runtime/Prefabs/Base/UI_LavaRush_BaseEvent.prefab` |
| `Prefabs/Icon/UI_LavaRush_Cell.prefab` | `Runtime/Prefabs/Icon/UI_LavaRush_Cell.prefab` |
| `Prefabs/Icon/UI_LavaRush_Icon.prefab` | `Runtime/Prefabs/Icon/UI_LavaRush_Icon.prefab` |
| `Prefabs/Main/UI_LavaRush.prefab` | `Runtime/Prefabs/Main/UI_LavaRush.prefab` |
| `Prefabs/UI/UI_LavaRush_Difficulty.prefab` | `Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab` |
| `Prefabs/UI/UI_LavaRush_EventEnd.prefab` | `Runtime/Prefabs/UI/UI_LavaRush_EventEnd.prefab` |
| `Prefabs/UI/UI_LavaRush_EventStart.prefab` | `Runtime/Prefabs/UI/UI_LavaRush_EventStart.prefab` |
| `Prefabs/UI/UI_LavaRush_Match.prefab` | `Runtime/Prefabs/UI/UI_LavaRush_Match.prefab` |
| `Prefabs/UI/UI_LavaRush_MatchEnd.prefab` | `Runtime/Prefabs/UI/UI_LavaRush_MatchEnd.prefab` |
| `Prefabs/UI/UI_LavaRush_MatchLose.prefab` | `Runtime/Prefabs/UI/UI_LavaRush_MatchLose.prefab` |
| `Prefabs/UI/UI_LavaRush_MatchWin.prefab` | `Runtime/Prefabs/UI/UI_LavaRush_MatchWin.prefab` |
| `Prefabs/UI/UI_LavaRush_Tutorial.prefab` | `Runtime/Prefabs/UI/UI_LavaRush_Tutorial.prefab` |

`Runtime/Prefabs/Main/UI_LavaRush.prefab` composes the state prefabs as nested prefab instances. `Runtime/Prefabs/LavaRushPresentation.prefab` retains its published path and GUID and wraps that main prefab as the compatibility composition root. `Runtime/Prefabs/LavaRushDemo.prefab` retains its published path and GUID and continues to reference the presentation.

## Image roles (56/56)

Several product-specific character/color variants intentionally map to one neutral package asset. The package preserves the semantic slot and edit point; a consuming project can replace or split those slots after `Embed for Edit` without importing Cat Merge Cafe binaries.

| Production image role | Package-owned counterpart |
| --- | --- |
| `Images/Colorcode/001_1_C.png` | `Runtime/Art/LavaRushDifficultyEasy.png` |
| `Images/Colorcode/001_2_C.png` | `Runtime/Art/LavaRushDifficultyNormal.png` |
| `Images/Colorcode/001_3_C.png` | `Runtime/Art/LavaRushDifficultyHard.png` |
| `Images/Colorcode/002_C.png` | `Runtime/Art/LavaRushStageNode.png` |
| `Images/Colorcode/003_1_C.png` | `Runtime/Art/LavaRushWinBurst.png` |
| `Images/Colorcode/003_2_C.png` | `Runtime/Art/LavaRushLoseCrack.png` |
| `Images/Colorcode/004_C.png` | `Runtime/Art/LavaRushTimerBadge.png` |
| `Images/DP/001_1.png` | `Runtime/Art/LavaRushExplorer.png` |
| `Images/DP/001_2.png` | `Runtime/Art/LavaRushExplorer.png` |
| `Images/DP/001_3.png` | `Runtime/Art/LavaRushExplorer.png` |
| `Images/DP/002_1.png` | `Runtime/Art/LavaRushExplorer.png` |
| `Images/DP/002_2.png` | `Runtime/Art/LavaRushExplorer.png` |
| `Images/DP/003_1.png` | `Runtime/Art/LavaRushExplorer.png` |
| `Images/DP/003_2.png` | `Runtime/Art/LavaRushExplorer.png` |
| `Images/DP/004.png` | `Runtime/Art/LavaRushExplorer.png` |
| `Images/resource/BG.png` | `Runtime/Art/LavaRushBackdrop.png` |
| `Images/resource/Badge.png` | `Runtime/Art/LavaRushIconFrame.png` |
| `Images/resource/Bottom_board.png` | `Runtime/Art/LavaRushPanel.png` |
| `Images/resource/Box_final.png` | `Runtime/Art/LavaRushRewardBadge.png` |
| `Images/resource/Bridge.png` | `Runtime/Art/LavaRushStageNode.png` |
| `Images/resource/Bridge_shadow.png` | `Runtime/Art/LavaRushPanel.png` |
| `Images/resource/Btn_green.png` | `Runtime/Art/LavaRushSecondaryButton.png` |
| `Images/resource/Btn_yellow.png` | `Runtime/Art/LavaRushPrimaryButton.png` |
| `Images/resource/Cat_person.png` | `Runtime/Art/LavaRushExplorer.png` |
| `Images/resource/Chest_easy.png` | `Runtime/Art/LavaRushRewardBadge.png` |
| `Images/resource/Chest_hard.png` | `Runtime/Art/LavaRushRewardBadge.png` |
| `Images/resource/Chest_normal.png` | `Runtime/Art/LavaRushRewardBadge.png` |
| `Images/resource/Grand_board.png` | `Runtime/Art/LavaRushPanel.png` |
| `Images/resource/Jewel.png` | `Runtime/Art/LavaRushRewardBadge.png` |
| `Images/resource/Lava_block.png` | `Runtime/Art/LavaRushLavaBlock.png` |
| `Images/resource/Level_Select.png` | `Runtime/Art/LavaRushTitleRibbon.png` |
| `Images/resource/Level_board.png` | `Runtime/Art/LavaRushCellFrame.png` |
| `Images/resource/Level_difficulty_easy.png` | `Runtime/Art/LavaRushDifficultyEasy.png` |
| `Images/resource/Level_difficulty_hard.png` | `Runtime/Art/LavaRushDifficultyHard.png` |
| `Images/resource/Level_difficulty_normal.png` | `Runtime/Art/LavaRushDifficultyNormal.png` |
| `Images/resource/Main_icon.png` | `Runtime/Art/LavaRushExplorer.png` + `Runtime/Art/LavaRushIconFrame.png` |
| `Images/resource/Popup_image_A.png` | `Runtime/Art/LavaRushExplorer.png` |
| `Images/resource/Popup_image_B.png` | `Runtime/Art/LavaRushRewardBadge.png` |
| `Images/resource/Popup_textboard.png` | `Runtime/Art/LavaRushPanel.png` |
| `Images/resource/Reward_box_B.png` | `Runtime/Art/LavaRushRewardBadge.png` |
| `Images/resource/Reward_box_F.png` | `Runtime/Art/LavaRushRewardBadge.png` |
| `Images/resource/Reward_box_combined.png` | `Runtime/Art/LavaRushRewardBadge.png` |
| `Images/resource/Stack_bar.png` | `Runtime/Art/LavaRushProgressTrack.png` |
| `Images/resource/Stack_in.png` | `Runtime/Art/LavaRushProgressFill.png` |
| `Images/resource/Title_CN.png` | `Runtime/Art/LavaRushTitleRibbon.png` + runtime-localized text |
| `Images/resource/Title_EN.png` | `Runtime/Art/LavaRushTitleRibbon.png` + runtime-localized text |
| `Images/resource/Title_JP.png` | `Runtime/Art/LavaRushTitleRibbon.png` + runtime-localized text |
| `Images/resource/Title_KR.png` | `Runtime/Art/LavaRushTitleRibbon.png` + runtime-localized text |
| `Images/resource/Title_TW.png` | `Runtime/Art/LavaRushTitleRibbon.png` + runtime-localized text |
| `Images/resource/Top_title.png` | `Runtime/Art/LavaRushAccent.png` + `Runtime/Art/LavaRushTitleRibbon.png` |
| `Images/resource/Tutorial_box.png` | `Runtime/Art/LavaRushPanel.png` |
| `Images/resource/Tutorial_cha.png` | `Runtime/Art/LavaRushTutorialGuide.png` |
| `Images/resource/Ui_timer.png` | `Runtime/Art/LavaRushTimerBadge.png` |
| `Images/resource/btn_i.png` | `Runtime/Art/LavaRushSecondaryButton.png` |
| `Images/resource/icon_i.png` | `Runtime/Art/LavaRushIconFrame.png` |
| `Images/resource/icon_lava.png` | `Runtime/Art/LavaRushLavaBlock.png` |

## External and shared production dependencies

The production prefabs also reference project scripts, shared UI wrappers, fonts, localization, Addressables, and other project assets. Those dependencies have no package counterpart because they are not redistribution-safe or package-neutral. `LavaRushScreenView`, Unity UGUI, runtime-localized `Text`, `LavaRushUIThemeAsset`, and existing service callbacks replace those presentation dependencies without referencing `Assembly-CSharp` or a consuming project's `Assets` folder.

Production prefab paths, Addressable keys, `.meta` files, images, fonts, audio, and scripts remain unchanged. Switching production to these package prefabs is a separate project migration and is not part of MCC-1551.
