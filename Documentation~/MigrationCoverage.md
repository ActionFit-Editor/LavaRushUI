# Lava Rush UI Migration Coverage

MCC-1551 defines the immutable one-to-one production inventory, and MCC-1581 completes its single-owner production switch. No AI-generated, consolidated, or substituted visual is accepted as coverage. `AssetOwnership.json` records every completed ownership transfer. All 14 prefabs and 56 images are package-owned at the paths below with their original GUIDs and no local duplicates.

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

The final nine ownership units were transferred in the required order: Difficulty, EventEnd, EventStart, Match, MatchEnd, MatchLose, MatchWin, Tutorial, then Main. Each unit preserved its original project GUID, remapped the retired package-copy GUID, removed the verified local prefab and `.meta`, and passed package-dependency, missing-script, consumer-reference, and ledger hash checks before the next unit began. The canonical Main role keeps GUID `ffae8bfdd6acf4657b158ff432e5a23b`, so the project-owned `UI_LavaRush` Addressable key now resolves directly to `Runtime/Prefabs/Main/UI_LavaRush.prefab` without an Addressables entry rewrite.

The canonical Main prefab owns `LavaRushPresentation`, `LavaRushBootstrap`, and the inactive `LavaRushFlowView` queue owner. `Documentation~/StandalonePresentationEvidence.json` records the canonical prefab, package engine bootstrap source, and the EditMode complete-flow test. Cat Merge initializes the same bootstrap with its project-owned engine and services; the package remains neutral toward inventory, rewards, Addressables, analytics, localization, audio, navigation, and profile systems.

`Runtime/Prefabs/Icon/UI_LavaRush_Icon.prefab` and `Runtime/Prefabs/Icon/UI_LavaRush_Cell.prefab` are completed prefab ownership units. They preserve original GUIDs `f7a017bca31e14a2eae90bc3a60cd5e3` and `800bfcd600b24494eb593e8f6ed492b1`; Cat Merge keeps both Addressable keys and attaches its project adapters through `EventAccessRegistry`. Package-owned `LavaRushAccessIconView` and `LavaRushInGameCellView` supply the serialized production bindings, including the cell's authored `0.3` second animation duration. Version `0.1.24` intentionally changes only the Cell title's General localization ID from the retired duplicate `lava_rush_icon` entry to canonical `lavarush_title`; the ownership ledger records the resulting prefab SHA while GUIDs, fileIDs, hierarchy, visual values, and behavior remain unchanged.

`Runtime/Prefabs/Base/Img_Title Variant.prefab` is a completed visual-prefab ownership unit. It preserves original GUID `faf6d9eda0d564250be884de1760886b` plus the legacy root GameObject, RectTransform, Image, and timer `UI_Text` local file identifiers consumed by the canonical package `UI_LavaRush_Match.prefab`. The role is connected to package-owned `Runtime/Prefabs/Internal/Img_LavaRush_TitleBase.prefab` and `Txt_LavaRush_TitleBase.prefab`; these internal authoring bases add no production role and introduce no consuming-project `Assets` dependency. The title text restores its package font material, `UI_Text` localization, Outline `0.1`, and authored Underlay while the completed package BaseEvent continues to own an equivalent flattened title/timer hierarchy directly.

`Runtime/Prefabs/Base/UI_LavaRush_BaseEvent.prefab` is a completed visual-prefab ownership unit. It preserves original GUID `db969225b48c74c929a40f9143f44288` plus the 18 valid GameObject, RectTransform, Image, TMP, `UI_Text`, and `UI_Button` local file identifiers consumed by Difficulty, EventEnd, EventStart, MatchEnd, MatchLose, and MatchWin. Characterization proved that local IDs `775524696328212203`, `6923480937244319326`, and `7723706689444764598` were stale no-op targets already ignored by Unity; the package prefab intentionally does not bind them to new objects, so the six unchanged consumer YAML files retain their existing behavior.

`Runtime/Prefabs/Icon/UI_LavaRush_Icon.prefab` and `UI_LavaRush_Cell.prefab` restore the production `UI_Text` components on the icon timer and all five cell text roles. The flattened cell preserves the original nested-prefab overrides for `Txt_ReaminCount` outline and `Txt_RemainTitle` localization/outline, uses the UI Foundation assembly identifier, and has no duplicate legacy `LocalizeStringEvent`. The cell Indicator keeps its GameObject, Transform, active-state behavior, and component file identifier while using UI Foundation `ScalePulse`; the obsolete Animator controller remains an unreferenced historical asset and is not part of the runtime contract.

`Runtime/Prefabs/Base/Content_LavaBlock.prefab` is a completed package ownership unit. It preserves original GUID `8107a7b8fccd249f4947f08aca662f01` and original root component file identifier `6810643454494422369`, now backed by package-owned `LavaRushBlockView`. Characterization verified the flattened package hierarchy against the former production prefab's transforms, active states, images, sprite rectangles, text, fonts, and mask. The View owns serialized visual references and the reward-info callback only; Cat Merge keeps item sprite resolution, amount formatting, collection navigation, and runtime profile groups in project adapters. The canonical package Match prefab consumes the same GUID/file identifier while the local prefab and former `Content_LavaBlock` script are absent.

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

`Runtime/Art/DP` contains full-screen design previews retained for source parity. Production prefabs must not reference these images as Sprites; reusable description panels use the original `Runtime/Art/resource/Popup_textboard.png`.

## Shared visual dependencies

Fonts, materials, animation controllers, and shared images used by the production hierarchy are copied under `Runtime/ProductionDependencies` using their source-relative paths. Every serialized reference in the package prefabs must resolve within `Packages/com.actionfit.lava-rush.ui` or a declared package dependency; none may point back into `Assets`.

The copied TMP shader set also includes `TMPro.cginc`, `TMPro_Mobile.cginc`, `TMPro_Properties.cginc`, and `TMPro_Surface.cginc` from `Assets/TextMesh Pro/Shaders`. These text resources remain byte-identical to the production source so relative `#include` directives compile from the package path.

Validation must prove recorded original GUID/SHA-256 evidence for all 56 single-owner PNGs, no missing scripts, package-only visual dependencies, one active state view per model, callback routing, and rendered parity for the eight screen states.
