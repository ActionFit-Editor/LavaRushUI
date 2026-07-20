# Lava Rush UI Migration Coverage

MCC-1551 requires one-to-one production coverage. Every source role below has an additive package copy; no AI-generated, neutral, consolidated, or substituted visual is accepted as coverage.

## Prefab roles (14/14)

| Production source | Package copy |
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

`Runtime/Prefabs/Main/UI_LavaRush.prefab` composes the eight state prefabs as package-owned nested instances. `Runtime/Prefabs/LavaRushPresentation.prefab` and `Runtime/Prefabs/LavaRushDemo.prefab` retain their published paths and GUIDs.

## Original Lava Rush images (56/56)

Every target is below `Runtime/Art` with the same relative path and filename as the source below `Assets/_Project/Content/LavaRush/Images`.

| Production source | Package copy |
| --- | --- |
| `Images/Colorcode/001_1_C.png` | `Runtime/Art/Colorcode/001_1_C.png` |
| `Images/Colorcode/001_2_C.png` | `Runtime/Art/Colorcode/001_2_C.png` |
| `Images/Colorcode/001_3_C.png` | `Runtime/Art/Colorcode/001_3_C.png` |
| `Images/Colorcode/002_C.png` | `Runtime/Art/Colorcode/002_C.png` |
| `Images/Colorcode/003_1_C.png` | `Runtime/Art/Colorcode/003_1_C.png` |
| `Images/Colorcode/003_2_C.png` | `Runtime/Art/Colorcode/003_2_C.png` |
| `Images/Colorcode/004_C.png` | `Runtime/Art/Colorcode/004_C.png` |
| `Images/DP/001_1.png` | `Runtime/Art/DP/001_1.png` |
| `Images/DP/001_2.png` | `Runtime/Art/DP/001_2.png` |
| `Images/DP/001_3.png` | `Runtime/Art/DP/001_3.png` |
| `Images/DP/002_1.png` | `Runtime/Art/DP/002_1.png` |
| `Images/DP/002_2.png` | `Runtime/Art/DP/002_2.png` |
| `Images/DP/003_1.png` | `Runtime/Art/DP/003_1.png` |
| `Images/DP/003_2.png` | `Runtime/Art/DP/003_2.png` |
| `Images/DP/004.png` | `Runtime/Art/DP/004.png` |
| `Images/resource/BG.png` | `Runtime/Art/resource/BG.png` |
| `Images/resource/Badge.png` | `Runtime/Art/resource/Badge.png` |
| `Images/resource/Bottom_board.png` | `Runtime/Art/resource/Bottom_board.png` |
| `Images/resource/Box_final.png` | `Runtime/Art/resource/Box_final.png` |
| `Images/resource/Bridge.png` | `Runtime/Art/resource/Bridge.png` |
| `Images/resource/Bridge_shadow.png` | `Runtime/Art/resource/Bridge_shadow.png` |
| `Images/resource/Btn_green.png` | `Runtime/Art/resource/Btn_green.png` |
| `Images/resource/Btn_yellow.png` | `Runtime/Art/resource/Btn_yellow.png` |
| `Images/resource/Cat_person.png` | `Runtime/Art/resource/Cat_person.png` |
| `Images/resource/Chest_easy.png` | `Runtime/Art/resource/Chest_easy.png` |
| `Images/resource/Chest_hard.png` | `Runtime/Art/resource/Chest_hard.png` |
| `Images/resource/Chest_normal.png` | `Runtime/Art/resource/Chest_normal.png` |
| `Images/resource/Grand_board.png` | `Runtime/Art/resource/Grand_board.png` |
| `Images/resource/Jewel.png` | `Runtime/Art/resource/Jewel.png` |
| `Images/resource/Lava_block.png` | `Runtime/Art/resource/Lava_block.png` |
| `Images/resource/Level_Select.png` | `Runtime/Art/resource/Level_Select.png` |
| `Images/resource/Level_board.png` | `Runtime/Art/resource/Level_board.png` |
| `Images/resource/Level_difficulty_easy.png` | `Runtime/Art/resource/Level_difficulty_easy.png` |
| `Images/resource/Level_difficulty_hard.png` | `Runtime/Art/resource/Level_difficulty_hard.png` |
| `Images/resource/Level_difficulty_normal.png` | `Runtime/Art/resource/Level_difficulty_normal.png` |
| `Images/resource/Main_icon.png` | `Runtime/Art/resource/Main_icon.png` |
| `Images/resource/Popup_image_A.png` | `Runtime/Art/resource/Popup_image_A.png` |
| `Images/resource/Popup_image_B.png` | `Runtime/Art/resource/Popup_image_B.png` |
| `Images/resource/Popup_textboard.png` | `Runtime/Art/resource/Popup_textboard.png` |
| `Images/resource/Reward_box_B.png` | `Runtime/Art/resource/Reward_box_B.png` |
| `Images/resource/Reward_box_F.png` | `Runtime/Art/resource/Reward_box_F.png` |
| `Images/resource/Reward_box_combined.png` | `Runtime/Art/resource/Reward_box_combined.png` |
| `Images/resource/Stack_bar.png` | `Runtime/Art/resource/Stack_bar.png` |
| `Images/resource/Stack_in.png` | `Runtime/Art/resource/Stack_in.png` |
| `Images/resource/Title_CN.png` | `Runtime/Art/resource/Title_CN.png` |
| `Images/resource/Title_EN.png` | `Runtime/Art/resource/Title_EN.png` |
| `Images/resource/Title_JP.png` | `Runtime/Art/resource/Title_JP.png` |
| `Images/resource/Title_KR.png` | `Runtime/Art/resource/Title_KR.png` |
| `Images/resource/Title_TW.png` | `Runtime/Art/resource/Title_TW.png` |
| `Images/resource/Top_title.png` | `Runtime/Art/resource/Top_title.png` |
| `Images/resource/Tutorial_box.png` | `Runtime/Art/resource/Tutorial_box.png` |
| `Images/resource/Tutorial_cha.png` | `Runtime/Art/resource/Tutorial_cha.png` |
| `Images/resource/Ui_timer.png` | `Runtime/Art/resource/Ui_timer.png` |
| `Images/resource/btn_i.png` | `Runtime/Art/resource/btn_i.png` |
| `Images/resource/icon_i.png` | `Runtime/Art/resource/icon_i.png` |
| `Images/resource/icon_lava.png` | `Runtime/Art/resource/icon_lava.png` |

## Shared visual dependencies

Fonts, materials, animation controllers, and shared images used by the production hierarchy are copied under `Runtime/ProductionDependencies` using their source-relative paths. Every serialized reference in the package prefabs must resolve within `Packages/com.actionfit.lava-rush.ui` or a declared package dependency; none may point back into `Assets`.

The copied TMP shader set also includes `TMPro.cginc`, `TMPro_Mobile.cginc`, `TMPro_Properties.cginc`, and `TMPro_Surface.cginc` from `Assets/TextMesh Pro/Shaders`. These text resources remain byte-identical to the production source so relative `#include` directives compile from the package path.

Validation must prove source/package SHA-256 equality for all 56 PNGs, TextureImporter parity, no missing scripts, package-only visual dependencies, one active state view per model, callback routing, and rendered parity for the eight screen states.
