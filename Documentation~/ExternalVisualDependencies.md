# External Visual Dependencies

The copied production prefabs retain their original visual-effect components. The normal Lava Rush Installer flow must install every dependency below before the UI package is considered production-ready.

| Package | Immutable Git dependency | Purpose |
|---|---|---|
| `com.coffee.ui-effect@5.10.8` | `https://github.com/mob-sakai/UIEffect.git?path=Packages/src#5.10.8` | Original UI filter/effect components |
| `com.coffee.ui-particle@4.12.1` | `https://github.com/mob-sakai/ParticleEffectForUGUI.git#4.12.1` | Original UI particle attractor components |
| `com.coffee.softmask-for-ugui@3.5.0` | `https://github.com/mob-sakai/SoftMaskForUGUI.git?path=Packages/src#3.5.0` | Original soft-mask components |
| `com.actionfit.uilighteffector@1.0.0` | `https://github.com/HuiSungz/UILightingEffect-ReShade.git#7dab46ec2378209bd1e524c8336b976eccb3df05` | Original lighting effect components; repository has no version tag, so the full commit is mandatory |
| `jp.hadashikick.vcontainer@1.16.8` | `https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.16.8` | Runtime dependency of UILighting |

These are bundle-level Git dependencies because UPM does not support declaring nested Git URLs in this package's `package.json`. Direct UI-only installation must add the exact top-level manifest entries above. Do not strip components, replace their visuals, move to a floating branch, or shorten the commit to make installation easier. A dependency change requires an explicit production-parity review.

UIEffect, ParticleEffectForUGUI, SoftMaskForUGUI, and VContainer identify their upstream packages as MIT licensed. UILighting is an ActionFit-authored dependency but publishes no version tag at this revision. This package references those installed assemblies; it does not copy their source code.
