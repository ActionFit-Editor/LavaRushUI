# ActionFit Lava Rush UI (`com.actionfit.lava-rush.ui`)

`com.actionfit.lava-rush`를 위한 UGUI 프레젠테이션입니다. Cat Merge Cafe에서 사용하던 14개 원본 Lava Rush 프리팹 역할, 원본 PNG 56개, 필요한 시각 의존성을 패키지 안에 그대로 포함한 편집 가능한 베이스와 독립 실행형 PlayerPrefs bootstrap을 제공합니다. 설치 직후 원본과 같은 외형·계층·상호작용을 확인하고, UI 패키지만 Embed한 뒤 이미지 리소스를 프로젝트별로 교체할 수 있습니다.

## 설치

공개 패키지가 publish된 후 Git 패키지를 `Packages/manifest.json`에 추가합니다.

```json
{
  "dependencies": {
    "com.actionfit.content-core": "https://github.com/ActionFit-Editor/ContentCore.git#0.2.3",
    "com.actionfit.time": "https://github.com/ActionFit-Editor/Time.git#1.0.4",
    "com.actionfit.lava-rush": "https://github.com/ActionFit-Editor/LavaRush.git#0.1.11",
    "com.actionfit.fonts.maplestory": "https://github.com/ActionFit-Editor/Maplestory_Fonts.git#1.0.0",
    "com.actionfit.referencebinding": "https://github.com/ActionFit-Editor/ReferenceBinding.git#0.2.1",
    "com.actionfit.ui.foundation": "https://github.com/ActionFit-Editor/UI_Foundation.git#2.0.5",
    "com.actionfit.ui.popup": "https://github.com/ActionFit-Editor/UI_Popup.git#0.1.1",
    "com.actionfit.lava-rush.ui": "https://github.com/ActionFit-Editor/LavaRushUI.git#0.2.1",
    "com.coffee.ui-effect": "https://github.com/mob-sakai/UIEffect.git?path=Packages/src#5.10.8",
    "com.coffee.ui-particle": "https://github.com/mob-sakai/ParticleEffectForUGUI.git#4.12.1",
    "com.coffee.softmask-for-ugui": "https://github.com/mob-sakai/SoftMaskForUGUI.git?path=Packages/src#3.5.0",
    "com.actionfit.uilighteffector": "https://github.com/HuiSungz/UILightingEffect-ReShade.git#7dab46ec2378209bd1e524c8336b976eccb3df05",
    "jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.16.8"
  }
}
```

패키지는 원본 `UI_Text`/`UI_Button` 직렬화와 공용 `ScalePulse` 동작, 전체 이벤트 동안 하나의 popup queue slot을 유지하기 위해 `com.actionfit.fonts.maplestory@1.0.0`, `com.actionfit.referencebinding@0.2.1`, `com.actionfit.ui.foundation@2.0.5`, `com.actionfit.ui.popup@0.1.1`, `com.unity.ugui@2.0.0`, `com.unity.modules.animation@1.0.0`을 선언합니다. 원본 프리팹의 UIEffect, UIParticle, SoftMask, UILighting 컴포넌트는 위의 정확한 top-level Git dependency를 요구하며 정상 설치 경로인 Lava Rush Installer가 모두 설치합니다. 세부 리비전과 역할은 `Documentation~/ExternalVisualDependencies.md`에 있습니다.

## 빠른 시작

1. `Tools > Package > ActionFit Lava Rush UI > Create Demo`를 선택합니다. 이 메뉴는 원본 `UI_LavaRush` 컨트롤러 프리팹을 참조하는 `Runtime/Prefabs/LavaRushDemo.prefab`을 생성합니다.
2. Play Mode에 진입합니다.
3. 이벤트를 시작하고 난이도를 선택한 뒤 튜토리얼을 확인하고 각 스테이지를 실행합니다.
4. **+ Progress** 또는 **Resolve Timer**는 독립 실행 데모에서만 사용합니다. 두 동작 모두 공개 `LavaRushEngine` 명령을 호출합니다.

`LavaRushBootstrap`은 `LavaRushEngine`을 Content Core PlayerPrefs 기본값, 디바이스 로컬 달력 시간대, 결정론적 월요일 데모 시계, 하루 일정과 패키지 소유 데모 카탈로그로 구성한 뒤 원본 `UI_LavaRush` 컨트롤러를 초기화합니다. 운영 환경에서는 프로젝트 소유 엔진과 중립 서비스 포트를 `LavaRushControllerContext`로 주입합니다.

운영 canonical 경로는 `Runtime/Prefabs/Main/UI_LavaRush.prefab`입니다. 이 프리팹은 원본 `UI_LavaRush` 루트와 직렬화된 8개 상태 컨트롤러를 직접 조합합니다. 루트는 `LavaRushEngine`만 상태 권한으로 사용하고, 각 컨트롤러는 `LavaRushControllerSnapshot`을 표시하며 원본 `UI_Text`/`UI_Button` 콜백을 루트로 전달합니다. 런타임 생성 화면이나 대체 계층은 없습니다.

## 모듈형 프리팹 세트

- `Runtime/Prefabs/Base`: lava block, title variant, event background
- `Runtime/Prefabs/Icon`: reusable cell and icon
- `Runtime/Prefabs/Main/UI_LavaRush.prefab`: canonical nested composition
- `Runtime/Prefabs/UI`: difficulty, event start/end, tutorial, match, win, lose, match end
- `Runtime/Prefabs/LavaRushDemo.prefab`: canonical controller를 참조하는 standalone bootstrap

`0.1.16`은 매치 단계 튜토리얼 텍스트 3개의 `Maskable`만 비활성화해 프로젝트 `UI_Text` Outline과 SoftMask의 비호환 머티리얼 치환을 우회합니다. 패키지의 기존 로컬라이징 이벤트와 `UI_Text` 컴포넌트 설정, 프로젝트의 `UI_Text` 로컬라이징·Outline 설정, 부모 SoftMask, 계층, 참조와 진행 동작은 유지됩니다.

`0.1.18`은 Icon의 `Txt_Timer`와 Cell의 `Txt_Timer`, `Txt_Status`, `Text (TMP) (1)`에 누락됐던 원본 `UI_Text` 설정과 local fileID를 복구합니다. Cell `Indicator`는 고장 난 Animator 경로 대신 UI Foundation `ScalePulse`를 사용하되 기존 Indicator GameObject, Transform, 활성 규칙과 component fileID를 유지합니다. 패키지 binder는 이 구체 타입을 직접 검증하며 Cat Merge EventAccess adapter의 카운트다운 포맷과 활성/비활성 수명주기는 그대로 유지합니다.

`0.1.19`는 Cell 평탄화 과정에서 빠졌던 `Txt_ReaminCount` Outline과 `Txt_RemainTitle` Localization/Outline 인스펙터 값을 원본 nested prefab override대로 복원합니다. 두 텍스트는 UI Foundation `UI_Text` 어셈블리 식별자를 사용하며 중복 `LocalizeStringEvent`는 제거되어 한 컴포넌트만 로컬라이징을 소유합니다. Cat Merge Difficulty의 설명 보드는 DP preview 대신 패키지 원본 `Popup_textboard.png`를 참조합니다. 기존 계층, 타이머, 버튼과 상호작용은 유지됩니다.

`0.1.20`은 `Runtime/Art/DP`의 1080×1920 완성 화면을 디자인 프리뷰로 분류하고 production prefab의 Sprite 참조를 금지합니다. Cat Merge Match End, Match Win, Match Lose의 `Img_Desc`는 BaseEvent 원본 `Popup_textboard.png`를 사용하며 기존 alpha, 계층, Transform, 버튼과 상호작용은 유지됩니다. 프로젝트와 패키지의 모든 Lava Rush prefab을 검사하는 회귀 테스트가 이후 DP 참조 재발을 차단합니다.

`0.1.21`은 평탄화되어 연결이 사라졌던 `Runtime/Prefabs/Base/Img_Title Variant.prefab`을 패키지 내부 이미지·텍스트 베이스와 연결된 nested prefab으로 복구합니다. 타이틀 GUID와 Match가 소비하는 local file ID는 유지하면서 패키지 폰트 머티리얼, `UI_Text` Localization, Outline `0.1`, Underlay 색상과 오프셋을 복원합니다. 패키지 밖 `Assets` 의존성과 중복 `LocalizeStringEvent` 없이 Embed 후 원본 연결을 그대로 편집할 수 있습니다.

`0.1.22`는 `Content_LavaBlock`의 `Mask_SeatPanel` 펼침 높이를 authored RectTransform과 같은 `180`으로 복원합니다. 좌석 말풍선 reveal이 완료된 뒤 마스크가 `Img_SeatPanel`과 같은 높이로 줄어들며 이미지 상하 테두리를 자르던 현상을 막고, 원본 이미지·계층·참조와 기존 애니메이션 timing은 유지합니다.

`0.1.23`은 남아 있던 Difficulty, EventEnd, EventStart, Match, MatchEnd, MatchLose, MatchWin, Tutorial, Main 역할을 패키지 단일 소유로 전환합니다. 원본 GUID와 소비 fileID를 패키지 경로에서 보존하고 로컬 prefab 중복을 제거했으므로 기존 `UI_LavaRush` Addressable key는 canonical Main을 직접 로드합니다. `0.1.29`부터 Main은 원본 컨트롤러를 직접 조합하고, Cat Merge는 같은 프리팹에 프로젝트 엔진과 localization/audio/reward/profile adapter를 주입합니다. Inventory, analytics, Addressables, navigation과 reward 권한은 프로젝트에 남습니다.

`0.1.24`는 General 로컬라이징 표준의 canonical `lavarush_title` Shared Data ID를 Cell 프리팹에 연결합니다. 기존 `lava_rush_icon` 중복 ID만 retired하며 prefab GUID, fileID, 계층, 아트와 런타임 동작은 유지합니다.

`0.1.25`는 `Runtime/Prefabs/Icon/UI_LavaRush_Icon.prefab`의 `Txt_Timer` 기본 TMP 글자색만 불투명 흰색에서 불투명 검정색으로 변경합니다. 기존 `UI_Text` 테두리, 폰트, 머티리얼, 텍스트, 계층, 바인딩, GUID, Addressable 계약과 런타임 동작은 유지합니다.

`0.1.26`은 복원될 controller가 `Main`, `TimeProvider`, `CountdownManager`를 참조하지 않도록 `ILavaRushFrameScheduler`, `ILavaRushCountdownScheduler`, `LavaRushTimeText`와 독립 실행용 `StandaloneLavaRushTiming`을 추가합니다. Cat Merge는 `com.actionfit.cat.app` adapter로 같은 port를 교체하며 controller 전환은 후속 작업에서 수행합니다.

`0.1.27`은 프로젝트 타입을 노출하지 않는 `ILavaRushAudio`, 10개 typed cue,
`ILavaRushProfileRoster`, immutable profile snapshot, profile group view/factory와 18개 semantic
localization key/fallback을 추가합니다. Cat Merge의 실제 프로필·사운드·General table
정책은 `com.actionfit.cat.app`이 제공합니다. `0.1.29` production `LavaRushManager`가
이 서비스들을 canonical direct controller에 주입합니다.

`0.1.28`은 Main-free `ILavaRushOrderProgressSource`, `ILavaRushAccessService`,
`ILavaRushProgressView`를 추가합니다. Cat Order score/provider/effect, EventAccess 등록·slot,
Addressable key/type binding과 outer controller lifetime은 `com.actionfit.cat.app` 또는
Project Shell이 소유합니다. `0.1.29` production composition은 Cat timing, order, access,
audio, profile, localization 포트를 동일한 canonical controller context에 연결합니다.

`0.1.29`는 MCC-1630에서 원본 GUID를 유지한 12개 `UI_LavaRush*` 컨트롤러를
`Runtime/Controllers`로 단일 소유 이전하고 canonical Main/Icon/Cell 프리팹에 직접
연결합니다. `UI_LavaRush`는 8개 상태 컨트롤러와 `LavaRushEngine`을 직접 조합하며,
timing/audio/profile/localization/order/access 포트만 소비합니다. Cat Merge는 동일
Addressable key와 프로젝트 어댑터를 유지하고 로컬 컨트롤러 중복을 제거합니다.
Standalone도 같은 canonical Main을 사용하며 런타임 생성 화면은 제공하지 않습니다.
구체 Cat 프로필 프리팹 factory 선택과 production 주입은 최종 composition gate에
남기며 패키지는 중립 factory seam과 생성된 view 수명만 소유합니다.

`0.2.0`은 `LavaRushScreenView`와 `LavaRushUIViewModel` 생산 API를 제거한 direct-controller breaking line입니다. 소비자는 canonical `UI_LavaRush*` controller와 `LavaRushControllerContext`를 사용하고, Cat Merge는 `com.actionfit.cat.app@0.2.0` composition 및 Project Shell 입력을 연결합니다. 저장 key, migration marker, reward receipt, prefab GUID와 세 Addressable identity는 변경하지 않습니다. 정확한 업그레이드·검증·rollback 순서는 `Documentation~/ConsumerMigration.md`에 있습니다.

`0.2.1`은 LavaRush 전용 Maplestory SDF/material 변형을
`com.actionfit.fonts.maplestory@1.0.0`으로 GUID 보존 이전하고, atlas padding
`8`과 authored material 값을 유지한 채 canonical Bold source를 사용합니다.

14개 production prefab 역할과 56개 원본 image의 일대일 inventory는 `Documentation~/MigrationCoverage.md`에 있습니다. 모든 역할은 원본 GUID를 패키지 경로에서 보존하는 단일 소유 상태이며 `Documentation~/AssetOwnership.json`에 GUID와 SHA-256을 기록합니다. 유효한 기존 소비 fileID와 nested prefab 연결은 그대로 유지하고, Unity가 이미 무시하던 BaseEvent stale override 3개는 새 객체에 연결하지 않습니다. `Documentation~/StandalonePresentationEvidence.json`은 canonical Main, engine bootstrap, complete-flow test를 연결합니다. Cat Merge는 프로젝트 전용 리소스·내비게이션·프로필 그룹·EventAccess·보상·분석 동작을 adapter에서 유지합니다.

## 프로젝트별 UI 편집

1. installer로 전체 Lava Rush bundle을 정상 설치합니다.
2. Custom Package Manager에서 **`com.actionfit.lava-rush.ui`만** `Embed for Edit`합니다.
3. `Runtime/Prefabs/Main`, `Runtime/Prefabs/UI`, `Runtime/Prefabs/Base`, `Runtime/Prefabs/Icon`, `Runtime/Art`, `Runtime/ProductionDependencies`를 프로젝트 스타일에 맞게 편집합니다. 원본 베이스를 유지한 채 nested prefab과 이미지를 교체하고 공개 compatibility root는 유지합니다.
4. engine, Content Core, Time과 installer/manager는 downloaded 상태로 유지합니다.

Embed는 사용자가 명시적으로 실행하는 편집 전환입니다. installer는 자동으로 UI를 embed하거나 기존 embedded 파일을 덮어쓰지 않습니다. 요구 버전과 호환되는 embedded UI는 설치/복구/해제에서 보존되며, 더 오래된 embedded UI는 자동 교체 대신 충돌로 보고됩니다. package update를 적용하려면 embedded 변경을 먼저 별도 branch에서 비교·병합해야 합니다.

Cat App, engine, shared owner와 theme은 자동 Embed 대상이 아닙니다. `com.actionfit.lava-rush.theme.catmerge@0.2.0`은 별도 선택 설치 프리셋이며 mandatory bundle에는 포함되지 않습니다.

CatDetective처럼 GUID와 구현이 다른 global `UI_Image`/`UI_Text`/`UI_Button`을 이미 소유한 프로젝트는 UI Foundation의 global 타입을 동시에 auto-reference하면 컴파일할 수 없습니다. 이 경우에만 기존 스크립트·GUID를 삭제하지 말고, 먼저 타입/GUID 호환 감사를 수행한 뒤 UI Foundation도 project-local로 Embed하여 Runtime asmdef의 `autoReferenced`를 `false`로 격리합니다. Lava Rush UI와 Foundation Editor asmdef는 Foundation을 명시 참조하므로 production prefab 동작은 유지되고, 프로젝트 `Assembly-CSharp`는 기존 UI 구현만 사용합니다. 이 예외 구성은 소비 프로젝트 문서와 diff에 반드시 기록합니다.

## CatDetective Starter

`Samples~/CatDetective Starter`는 `AF_CatDetective` Unity `6000.3.9f1`을 위한 opt-in 프로젝트 소유 consumer bridge입니다. sample composition root는 package-authored canonical `UI_LavaRush`를 참조하고, 가져온 프로젝트 adapter가 CatDetective 엔진과 서비스를 주입합니다. 패키지 안에서는 비활성 상태이며 `Assets/Contents/LavaRush` 아래로 가져온 뒤에만 CatDetective `Assembly-CSharp` API를 참조할 수 있습니다.

1. `Tools > Package > ActionFit Lava Rush UI > Preview CatDetective Starter`를 선택합니다.
2. 모든 정확한 버전 의존성 문제와 파일 충돌을 해결합니다.
3. `Install CatDetective Starter`를 선택하고 새 파일 생성 전용 plan을 승인합니다.
4. 가져온 `CatDetectiveLavaRushSettings.asset`을 설정합니다.
5. 가져온 CatDetective Addressables 등록 메뉴를 미리 보고 별도로 승인합니다.

installer는 내용이 다른 대상 파일을 덮어쓰지 않습니다. 같은 버전을 다시 가져오면 변경하지 않으며 Addressables 충돌이 있으면 직렬화 설정을 바꾸지 않고 등록을 차단합니다. 영속 상태, 보상, 시간, 팝업, 업그레이드, 제거 및 알려진 원자성 세부사항은 가져온 sample README를 확인하세요.

## 운영 연동

- 프로젝트 소유 영속 상태, 보상, 시계, 카탈로그, 접근, 일정, curve, 난수 및 분석 adapter로 `LavaRushEngine`을 구성합니다.
- canonical `UI_LavaRush`에 프로젝트 소유 `LavaRushEngine`과 `LavaRushControllerContext`를 전달합니다.
- `ILavaRushFrameScheduler`, `ILavaRushCountdownScheduler`, `ILavaRushAudio`, `ILavaRushProfileRoster`, `ILavaRushUILocalizer`, `ILavaRushUIRewardRenderer`, `ILavaRushOrderProgressSource`, `ILavaRushAccessService`는 실제 프로젝트 경계에서만 구현합니다.
- 프로젝트 order 및 merge adapter가 진행도를 소유한다면 커스텀 프레젠테이션 config에서 데모 동작 button을 비활성화합니다.

컨트롤러는 엔진 스냅샷을 읽고 `LavaRushUIAction` 요청을 발생시킵니다. 패키지 JSON을 쓰거나 인벤토리를 직접 지급하거나 일정을 처리하거나 스테이지 및 보상 규칙을 복제하지 않습니다.

## 런타임 API

- `UI_LavaRush`: 엔진 상태, 8개 화면 전환, 동작 라우팅과 중립 서비스 수명주기를 소유하는 canonical controller입니다.
- `LavaRushControllerView`: 상태별 직렬화 참조, 스냅샷 표시와 버튼 callback을 소유하는 공통 베이스입니다.
- `UI_LavaRush_EventStart`, `UI_LavaRush_Difficulty`, `LavaRushTutorialView`, `UI_LavaRush_Match`, `UI_LavaRush_MatchWin`, `UI_LavaRush_MatchLose`, `UI_LavaRush_MatchEnd`, `UI_LavaRush_EventEnd`: 원본 화면 identity를 유지한 직접 컨트롤러입니다.
- `UI_LavaRush_Icon`, `UI_LavaRush_Cell`: 패키지 소유 접근/진행 표시 컨트롤러이며 Cat EventAccess 동작은 프로젝트 어댑터가 같은 프리팹에 추가합니다.
- `LavaRushBootstrap`: 동일 canonical controller를 사용하는 독립 실행 composition root입니다.
- `LavaRushBlockView`: 패키지 소유 발판 프리팹의 위치·스테이지·좌석·보상 표시와 정보 요청 callback을 제공하는 얇은 binder입니다. 아이템 리소스와 프로젝트 팝업은 소비 프로젝트 adapter가 처리합니다.
- `LavaRushControllerContext`, `LavaRushControllerSnapshot`: 프로젝트 서비스 입력과 불변 엔진 표시 스냅샷입니다.

## 에셋 경계

패키지 기본 베이스는 `Assets/_Project/Content/LavaRush`에서 가져온 원본 프리팹 14개와 PNG 56개를 가공·재생성 없이 포함합니다. 프리팹이 참조하던 공용 이미지, 폰트, material, animation 등의 시각 의존성도 `Runtime/ProductionDependencies`에 포함하고 패키지 경로로 연결했습니다. TMP shader가 사용하는 원본 `TMPro*.cginc` 네 파일도 같은 폴더에 바이트 그대로 포함해 상대 include가 패키지 안에서 완결됩니다. 프로젝트 전용 gameplay MonoBehaviour는 제거하고 엔진 callback binder로 대체했지만 외형·계층·활성 상태는 보존합니다.

AI 생성 이미지, 합성 이미지, neutral placeholder, 재그린 근사치, variant 통합, 자동 대체 리소스는 기본 베이스에 허용하지 않습니다. 이후 컨텐츠 패키징에서도 원본을 포함할 수 없는 항목이 있으면 작업을 중단하고 항목별 명시적 결정을 받아야 합니다. 새 로컬/패키지 복제본도 만들지 않습니다. 한 역할씩 원본 GUID를 패키지 경로로 이전하고 로컬 경로를 같은 단위에서 제거한 뒤 소비 prefab을 검증합니다. 상세 출처와 대응은 `Documentation~/AssetProvenance.md`, `MigrationCoverage.md`, `AssetOwnership.json`을 확인하세요.

## 어셈블리 및 테스트

- Runtime: `com.actionfit.lava-rush.ui`
- Editor: `com.actionfit.lava-rush.ui.Editor`
- EditMode tests: `com.actionfit.lava-rush.ui.Editor.Tests`

릴리스 전달 전에 패키지 계약 검증, UI 및 엔진 EditMode 테스트와 격리 Unity 검증을 실행합니다.

## 배포

저장소 공개 범위 메타데이터는 Public입니다. 저장소 생성, Git push, 태그 생성, 카탈로그 등록과 publish는 Custom Package Manager에서 수동으로 실행합니다.
