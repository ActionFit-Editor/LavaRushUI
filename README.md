# ActionFit Lava Rush UI (`com.actionfit.lava-rush.ui`)

`com.actionfit.lava-rush`를 위한 프로젝트 중립 UGUI 프레젠테이션입니다. 14개 실사용 역할에 대응하는 모듈형 패키지 프리팹, ActionFit 소유 Lava Rush 이미지, 교체 가능한 theme asset, 독립 실행형 PlayerPrefs bootstrap을 포함합니다. 깨끗한 Unity 프로젝트에서도 이벤트 시작, 난이도 선택, 튜토리얼, 시간 제한 스테이지, 승패, 진행도, 보상과 이벤트 종료까지 확인할 수 있습니다.

## 설치

공개 패키지가 publish된 후 Git 패키지를 `Packages/manifest.json`에 추가합니다.

```json
{
  "dependencies": {
    "com.actionfit.content-core": "https://github.com/ActionFit-Editor/ContentCore.git#0.2.3",
    "com.actionfit.time": "https://github.com/ActionFit-Editor/Time.git#1.0.4",
    "com.actionfit.lava-rush": "https://github.com/ActionFit-Editor/LavaRush.git#0.1.6",
    "com.actionfit.lava-rush.ui": "https://github.com/ActionFit-Editor/LavaRushUI.git#0.1.8"
  }
}
```

패키지는 `com.unity.ugui@2.0.0`도 선언합니다.

## 빠른 시작

1. `Tools > Package > ActionFit Lava Rush UI > Create Demo`를 선택합니다. 이 메뉴는 `Runtime/Prefabs/LavaRushDemo.prefab`을 생성하며, 데모는 `LavaRushPresentation.prefab`을 사용합니다.
2. Play Mode에 진입합니다.
3. 이벤트를 시작하고 난이도를 선택한 뒤 튜토리얼을 확인하고 각 스테이지를 실행합니다.
4. **+ Progress** 또는 **Resolve Timer**는 독립 실행 데모에서만 사용합니다. 두 동작 모두 공개 `LavaRushEngine` 명령을 호출합니다.

`LavaRushBootstrap`은 `LavaRushEngine`을 Content Core PlayerPrefs 기본값, 결정론적 월요일 데모 시계, 하루 일정과 패키지 소유 데모 카탈로그로 구성합니다. 운영 환경에서는 프로젝트 소유 엔진을 주입합니다.

기본 경로는 package-authored `Runtime/Prefabs/LavaRushPresentation.prefab`입니다. 이 호환 composition root는 `Runtime/Prefabs/Main/UI_LavaRush.prefab`과 8개 상태 화면을 조합하고, `LavaRushScreenView`가 immutable view model을 현재 화면에 바인딩합니다. prefab의 Inspector reference가 완전하면 런타임 계층을 새로 만들지 않습니다. prefab이 없거나 필수 reference가 끊긴 경우에만 `LavaRushPresentation`이 기존 단색 UGUI overlay를 생성합니다. 패키지 로컬 전환 curve를 사용하며 UI Foundation, DOTween, UniTask, Addressables, 프로젝트 폰트, localization table 또는 오디오 의존성이 없습니다.

## 모듈형 프리팹 세트

- `Runtime/Prefabs/Base`: lava block, title variant, event background
- `Runtime/Prefabs/Icon`: reusable cell and icon
- `Runtime/Prefabs/Main/UI_LavaRush.prefab`: canonical nested composition
- `Runtime/Prefabs/UI`: difficulty, event start/end, tutorial, match, win, lose, match end
- `Runtime/Prefabs/LavaRushPresentation.prefab`: 기존 공개 경로/GUID를 보존하는 compatibility root
- `Runtime/Prefabs/LavaRushDemo.prefab`: standalone bootstrap

14개 production prefab 역할과 56개 image 역할의 대응표는 `Documentation~/MigrationCoverage.md`에 있습니다. 대응표는 역할 인벤토리이며 production 파일 복사를 의미하지 않습니다.

## 프로젝트별 UI 편집

1. installer로 전체 Lava Rush bundle을 정상 설치합니다.
2. Custom Package Manager에서 **`com.actionfit.lava-rush.ui`만** `Embed for Edit`합니다.
3. `Runtime/Prefabs/Main`, `Runtime/Prefabs/UI`, `Runtime/Prefabs/Base`, `Runtime/Prefabs/Icon`, `Runtime/Art/*.png`, `Runtime/Themes/LavaRushNeutralTheme.asset`을 프로젝트 스타일에 맞게 편집합니다. 공개 compatibility root는 유지하고 nested prefab을 바꾸는 방식을 권장합니다.
4. engine, Content Core, Time과 installer/manager는 downloaded 상태로 유지합니다.

Embed는 사용자가 명시적으로 실행하는 편집 전환입니다. installer는 자동으로 UI를 embed하거나 기존 embedded 파일을 덮어쓰지 않습니다. 요구 버전과 호환되는 embedded UI는 설치/복구/해제에서 보존되며, 더 오래된 embedded UI는 자동 교체 대신 충돌로 보고됩니다. package update를 적용하려면 embedded 변경을 먼저 별도 branch에서 비교·병합해야 합니다.

## CatDetective Starter

`Samples~/CatDetective Starter`는 `AF_CatDetective` Unity `6000.3.9f1`을 위한 opt-in 프로젝트 소유 consumer bridge입니다. sample popup prefab은 package-authored `LavaRushPresentation.prefab`을 참조하고, 가져온 프로젝트 adapter가 CatDetective 엔진과 서비스를 주입합니다. 패키지 안에서는 비활성 상태이며 `Assets/Contents/LavaRush` 아래로 가져온 뒤에만 CatDetective `Assembly-CSharp` API를 참조할 수 있습니다.

1. `Tools > Package > ActionFit Lava Rush UI > Preview CatDetective Starter`를 선택합니다.
2. 모든 정확한 버전 의존성 문제와 파일 충돌을 해결합니다.
3. `Install CatDetective Starter`를 선택하고 새 파일 생성 전용 plan을 승인합니다.
4. 가져온 `CatDetectiveLavaRushSettings.asset`을 설정합니다.
5. 가져온 CatDetective Addressables 등록 메뉴를 미리 보고 별도로 승인합니다.

installer는 내용이 다른 대상 파일을 덮어쓰지 않습니다. 같은 버전을 다시 가져오면 변경하지 않으며 Addressables 충돌이 있으면 직렬화 설정을 바꾸지 않고 등록을 차단합니다. 영속 상태, 보상, 시간, 팝업, 업그레이드, 제거 및 알려진 원자성 세부사항은 가져온 sample README를 확인하세요.

## 운영 연동

- 프로젝트 소유 영속 상태, 보상, 시계, 카탈로그, 접근, 일정, curve, 난수 및 분석 adapter로 `LavaRushEngine`을 구성합니다.
- 기존 `LavaRushPresentation`과 함께 `LavaRushBootstrap.Initialize`에 전달하거나, bootstrap을 프로젝트 composition root의 참조 동작 router로만 사용합니다.
- `ILavaRushUILocalizer`, `ILavaRushUIAudio`, `ILavaRushUIRewardRenderer`, `ILavaRushUIProfileProvider`, `ILavaRushUIViewHost`는 사용하는 프로젝트 경계에서만 구현합니다.
- 초기화 전에 `LavaRushUIThemeAsset`, inline theme을 적용하거나 `ApplyThemeOverride`를 호출합니다. 선택형 `com.actionfit.lava-rush.theme.catmerge` 패키지는 재배포 가능한 프리셋을 제공합니다.
- 프로젝트 order 및 merge adapter가 진행도를 소유한다면 커스텀 프레젠테이션 config에서 데모 동작 button을 비활성화합니다.

프레젠테이션은 불변 엔진 상태를 읽고 `LavaRushUIAction` 요청을 발생시킵니다. 패키지 JSON을 쓰거나 인벤토리를 직접 지급하거나 일정을 처리하거나 스테이지 및 보상 규칙을 복제하지 않습니다.

## 런타임 API

- `LavaRushBootstrap`: 독립 실행 composition root 및 동작 router입니다.
- `LavaRushPresentation`: 제한된 화면/진행 hook을 제공하는 자동 생성 또는 프리팹 기반 UGUI presenter입니다.
- `LavaRushScreenView`: 모듈형 상태 프리팹의 serialized reference와 표시·버튼 callback을 소유하는 얇은 binder입니다.
- `LavaRushUIViewModel`: 불변 프레젠테이션 스냅샷입니다.
- `LavaRushUITheme`, `LavaRushUIThemeAsset`, `LavaRushUIConfig`: Inspector에서 작성하는 외형 및 동작 입력입니다.
- `ILavaRushUILocalizer`, `ILavaRushUIAudio`, `ILavaRushUIRewardRenderer`, `ILavaRushUIProfileProvider`, `ILavaRushUIViewHost`: 좁은 프로젝트 서비스 경계입니다.

## 에셋 경계

이 패키지는 ActionFit이 MCC-1551에서 독립 제작한 Lava Rush 배경·explorer·reward 이미지와 재현 가능한 UI geometry, package-authored prefab/theme만 포함합니다. `Assets/_Project/Content/LavaRush`에서 복사한 파일, 서드파티 아트, 프로젝트 폰트 또는 오디오는 없습니다. 기존 Cat Merge 프리팹, Addressable key, 스크립트, `.meta` GUID와 바이너리 리소스는 변경하지 않습니다. 상세 출처는 `Documentation~/AssetProvenance.md`를 확인하세요. 이후 기존 에셋을 이동하려면 별도 권리 검토와 GUID/참조 마이그레이션 승인이 필요합니다.

## 어셈블리 및 테스트

- Runtime: `com.actionfit.lava-rush.ui`
- Editor: `com.actionfit.lava-rush.ui.Editor`
- EditMode tests: `com.actionfit.lava-rush.ui.Editor.Tests`

릴리스 전달 전에 패키지 계약 검증, UI 및 엔진 EditMode 테스트와 격리 Unity 검증을 실행합니다.

## 배포

저장소 공개 범위 메타데이터는 Public입니다. 저장소 생성, Git push, 태그 생성, 카탈로그 등록과 publish는 Custom Package Manager에서 수동으로 실행합니다.
