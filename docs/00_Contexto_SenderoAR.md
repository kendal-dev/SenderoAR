# 00 · Contexto maestro — Sendero AR

> Destilado de `00_Contexto_SenderoAR.md`, `SESSION_HANDOFF.md` y `PROJECT_PROGRESS.md`
> (versiones del 21–28/05/2026). Contrastado contra el repo el 16/10/2026.
> Leyenda de marcas: ver [`README.md`](README.md).
>
> **El estado vivo y las fechas de entrega están en [`CLAUDE.md`](../CLAUDE.md).**
> Este documento guarda el *porqué* de las decisiones, no el avance.

---

## 1. Identidad

| Atributo | Valor |
|---|---|
| Nombre | Sendero AR |
| Bundle ID | `com.kendallab.senderoar` |
| Autor | Kevin Daniel Lozano · KENDAL Lab |
| Tutor | Ing. Jorge Gustavo Méndez Ayala |
| Institución | Universidad Privada del Valle (Univalle) — Ingeniería de Sistemas Informáticos |
| Modalidad | Proyecto de grado (licenciatura) |
| Caso de estudio | Conjunto Misional de San José de Chiquitos, Santa Cruz, Bolivia (UNESCO 1990) |
| Hardware baseline | **Samsung Galaxy A52s 5G** (Snapdragon 778G + Adreno 642L) |
| Hardware secundario | iPhone 17 Pro Max (planificado como respaldo en la defensa) |
| Shell del autor | PowerShell en Windows |

---

## 2. Visión

App móvil (Android/iOS) con dos módulos:

- **AR offline** — image tracking sobre un marcador 2D, modelo 3D superpuesto con
  animación, **texto flotante orbital** con datos históricos y narración en ES/EN/PT.
- **Chatbot online** — guía histórico ("Padre Felipe") sobre Gemini vía Firebase AI Logic,
  con cache local para responder sin red.

El texto orbital figura en el perfil aprobado: **no es opcional**.

### 2.1. Fachada real vs marcador impreso — 🔴 decisión abierta

En mayo se congeló el **enfoque diorama**: una base MDF de 60×60 cm con un vinilo mate que
lleva el marcador 2D, y encima una miniatura de resina de ~15 cm. El tracking real ocurre
sobre el vinilo; la miniatura es escenografía. Detalle en
[`03_AR_Tracking_Specs.md`](03_AR_Tracking_Specs.md) §2.

`CLAUDE.md` §7 paso 1 **reabre esa decisión** el 04/09/2026 (memoria
`sendero-ar-fachada-vs-marcador`). Mientras no se cierre, todo lo que dependa del tamaño o
del material del marcador es 📐.

Lo único ✅: el pipeline de tracking funciona en el A52s con **1 marcador impreso de 20 cm**
(placeholder, imagen de la Misión de San Javier en hoja oficio).

---

## 3. Stack

### 3.1. Motor y AR — ✅ en el repo

| Componente | Versión | Nota |
|---|---|---|
| Unity | **2022.3.62f3 LTS** | CI falla si `ProjectVersion.txt` cambia |
| AR Foundation | **5.2.0** | 🔁 Se probó 6.0.7 el 19/05 y chocó con `com.unity.ugui 1.0.0`; 6.x apunta a Unity 6. Se volvió a 5.2 |
| ARCore / ARKit XR Plugin | 5.2.0 | Android API 24+ · iOS 14+ · ARM64 · IL2CPP |
| XR Management | 4.4.1 | |
| Input System | 1.7.0 | |
| TextMeshPro | 3.0.9 | |
| URP | 14.0.11 | Asset `URP-Performant` reutilizado de la plantilla AR Mobile |
| C# | .NET Standard 2.1 | Sin `dynamic` |

**`URP-Performant`**: HDR off · Cast Shadows off · MSAA 2x · Render Scale 1.0 · sin luces
adicionales (1 Directional) · post-processing off.

### 3.2. Backend e IA — 📐 diseño (no hay dependencias Firebase en `manifest.json`)

Ver [`02_AI_Architecture.md`](02_AI_Architecture.md). Resumen: Firebase AI Logic → Vertex AI
en `southamerica-east1`, modelo leído de Remote Config, App Check enforced, 5 RPM por
usuario, system prompt en español.

### 3.3. Narración — 📐 diseño

Ver [`05_TTS_Pipeline.md`](05_TTS_Pipeline.md). ES con Azure (`es-BO-MarceloNeural`), EN y
PT con ElevenLabs. 5 monumentos × 3 idiomas = **15 narraciones**, empaquetadas con
Addressables; pt-BR se descarga bajo demanda.

### 3.4. Patrón — 🟡 parcial

MVVM puro sin frameworks. **Hoy existen** contratos, modelos y Composition Root;
**no existen** ViewModels, Views, infraestructura reactiva ni `.asmdef`. Ver
[`04_MVVM_Patterns.md`](04_MVVM_Patterns.md).

### 3.5. Presupuesto de rendimiento — objetivos de diseño

| Métrica | Umbral |
|---|---|
| Frame rate | `Application.targetFrameRate = 30` ✅ (lo fija `AppBootstrapper`) |
| Draw calls | ≤ 100 |
| Triángulos en escena | ≤ 150 000 |
| Triángulos por monumento | ≤ 30 000 |
| Materiales por malla | 1, sin transparencia alfa |
| Texturas | ASTC 6×6, máx. 2048×2048 |

---

## 4. Estructura del repositorio — ✅ en el repo

```
Assets/_SenderoAR/                 ← única ruta de trabajo del proyecto
├── AR/Libraries/                  SenderoTrackingLibrary.asset (versionada)
├── Art/                           EXCLUIDA de Git...
│   └── Tracking/                  ...MENOS esta: la librería guarda GUIDs, no píxeles
├── Core/
│   ├── AR/                        ImageTrackingController, TrackingLimitedTimeoutWatcher
│   ├── Contracts/                 5 interfaces, ninguna implementada
│   ├── Diagnostics/               FPSDiagnosticsHUD (descartable)
│   ├── Infrastructure/            AppBootstrapper, AppContext, SceneNames
│   └── Materials/                 DebugCubeMat (URP/Lit)
├── Models/                        POCOs, enums, MonumentDataSO, catálogo, 5 .asset
├── Scenes/                        _App_Boot (índice 0), _Main (índice 1)
├── ViewModels/                    vacío
└── Views/                         vacío
Assets/Editor/                     MonumentDataValidator, FindMissingScripts
Tools/validate_project.py          integridad del repo sin Unity
docs/                              esta documentación
```

**Prohibido**: escenas en `Assets/Scenes/`; Composition Root fuera de
`Core/Infrastructure/`; librerías de tracking con prefijo `Sprint*_`.

---

## 5. Decisiones consolidadas

Las 45 "decisiones inmutables" de mayo, deduplicadas y con su estado real. Las que son
lecciones de depuración están en [`09_Lecciones_Unity.md`](09_Lecciones_Unity.md).

### 5.1. Arquitectura y código

| # | Decisión | Estado |
|---|---|---|
| A1 | Todo el código propio vive en `Assets/_SenderoAR/`; escenas solo en `Assets/_SenderoAR/Scenes/` | ✅ |
| A2 | MVVM puro: sin UniRx, Zenject, R3 ni MVVMLight | 📐 (no hay MVVM todavía) |
| A3 | ViewModels son POCOs, nunca `MonoBehaviour`; solo exponen `IReadOnlyReactiveProperty<T>` e `ICommand` | 📐 |
| A4 | Contratos antes que implementaciones; cada contrato documenta su modelo de hilos en `<remarks>` | ✅ (5 contratos) |
| A5 | Enums type-safe en vez de strings mágicos (`LanguageCode`, `MonumentTrackingPhase`, `ChatbotErrorKind`, `PlaybackEventKind`) | ✅ |
| A6 | `readonly struct` para eventos de alta frecuencia (`TrackedMonumentEvent`, `AudioCue`) | ✅ |
| A7 | `LanguageCode` en un único archivo, sin duplicados inline | ✅ 🔁 ver nota |
| A8 | Lookups con patrón `TryGetById(string id, out T result)`: sin `null` de retorno ni `throw` | ✅ (`MonumentCatalogSO`) |
| A9 | Todo `ScriptableObject` de datos declara `OnValidate()` defensivo dentro de `#if UNITY_EDITOR` | ✅ (`MonumentDataSO`) |
| A10 | Todo `ScriptableObject` de datos tiene un validador de menú en `Assets/Editor/` | ✅ (`Sendero AR > Validate Monument Data`) |
| A11 | `MonumentId` = nombre del `.asset`; `PhysicalSize` con piso 0.60 m; `TrackingConfidenceThreshold` en [0..1], default 0.4 | ✅ |
| A12 | Ningún segmento final de namespace puede llamarse `Debug`, `Object`, `Random` o `Time` | ✅ (`Core.Diagnostics`) |
| A13 | Watchers previos a MVVM solo **observan**; mutar la jerarquía es trabajo de los servicios | ✅ (`TrackingLimitedTimeoutWatcher`) |
| A14 | Modo demo con patrón Decorator + `#if DEMO_MODE`, sin tocar contratos | 📐 |

> 🔁 **A7 — `LanguageCode`.** El handoff del 28/05 pedía valores
> `SpanishBolivia/EnglishUS/PortugueseBR` y un método `ToCode()`. **El código real** tiene
> `EsBO = 0`, `EnUS = 1`, `PtBR = 2` y **no tiene `ToCode()`**. La duplicación inline en
> `AddressableAudioReference.cs` ya no existe. Cuando haga falta el código ISO
> (`"es-BO"`), se agrega como método de extensión en `Models/`, sin renombrar los valores:
> los `.asset` los guardan serializados como enteros.

### 5.2. AR y render

| # | Decisión | Estado |
|---|---|---|
| R1 | AR Foundation 5.2.0, no 6.x | ✅ |
| R2 | `XRReferenceImageLibrary` compilada en build-time, no en runtime | ✅ |
| R3 | `requestedMaxNumberOfMovingImages = 1` | ✅ 🔁 (el doc original decía `maxNumberOfMovingImages`, obsoleta) |
| R4 | `_App_Boot` sin componentes AR físicos; solo `ARSession.CheckAvailability()` estático | ✅ |
| R5 | Una `ARSession` por escena AR, nunca compartida con `DontDestroyOnLoad` | ✅ |
| R6 | El Renderer de URP declara `AR Background Renderer Feature` | ✅ |
| R7 | Materiales de objetos creados en runtime se inyectan con `[SerializeField]`, nunca `Shader.Find()` | ✅ |
| R8 | Color en URP sobre `_BaseColor`, nunca `.color` (apunta a `_Color`, de Built-in). Hoy se aplica con `MaterialPropertyBlock` para no instanciar materiales | ✅ 🔁 (S1 usaba `renderer.material.SetColor`, que filtraba un `Material` por cubo; corregido en `db36f3f`) |
| R9 | `ARPlaneManager` y `ARRaycastManager` desactivados en `_Main` salvo fallback | ✅ (commit `cc0d8f9`) |
| R10 | Cero *missing scripts* en escenas y prefabs antes de cualquier build | ✅ (lo verifica `validate_project.py`) |
| R11 | Score `arcoreimg` ≥ 85 por marcador | 📐 (no hay marcadores reales) |
| R12 | Plan B: tap-to-place sobre `ARPlane` si la luz rompe el image tracking | 📐 |

### 5.3. Producto y contenido

| # | Decisión | Estado |
|---|---|---|
| P1 | 5 monumentos en código; `mon_05` es la **Pila Bautismal**, no Santa Cruz la Vieja | ✅ (los 5 `.asset` existen) |
| P2 | 3 miniaturas físicas en la defensa (Templo, Torre, Pila) — "Camino A" | 📐 (`IsPhysicalInDefense` ya marcado en los `.asset`) |
| P3 | 3 idiomas: ES + EN + PT | 🟡 (solo hay descripciones ES) |
| P4 | Texto orbital obligatorio, con `TextMeshPro` 3D (no UGUI en World Space) | 📐 |
| P5 | Animación por monumento: rotación lenta + animación contextual + texto orbital | 📐 |

### 5.4. Proceso

| # | Decisión | Estado |
|---|---|---|
| G1 | Conventional Commits, cuerpo en español explicando el porqué | ✅ |
| G2 | Trunk-Based: ramas `feat/…` efímeras, merge `--no-ff` | 🔁 Las ramas iban a durar < 2 días; Sprint 2 duró 100 días sin integrar. Regla vigente: **≤ 2 semanas** (`CLAUDE.md` §5 regla 3) |
| G3 | "5 commits por día" | ❌ Retirada. Llevó a 8 commits vacíos el 26–30/05. Regla vigente: **ningún commit vacío** |
| G4 | `git diff` antes de `git add`; `File > Save Project` antes de `git diff` | ✅ |
| G5 | Mover assets solo dentro del Project window de Unity | ✅ |
| G6 | Nunca subir `Library/`, `Temp/`, binarios de herramientas (`arcoreimg.exe`) ni `Art/` (salvo `Art/Tracking/`) | ✅ |
| G7 | `python Tools/validate_project.py` con 0 errores antes de cada push | ✅ |

---

## 6. Roadmap original (mayo) — ⚠️ vencido

Planificado en Sprint 0. **No sirve para prometer fechas**: el proyecto está en Sprint 2.
Se conserva porque fija el **orden de dependencias**, que sigue valiendo.

| Sprint | Fechas plan | Objetivo |
|---|---|---|
| S0 | 18–22 May | Cimentación, 5 contratos, POCOs |
| S1 | 25 May – 5 Jun | Image tracking PoC (placeholder) |
| S2 | 8–19 Jun | `ScriptableObject`s + fotos/marcadores reales |
| S3 | 22 Jun – 3 Jul | MVVM base + primer test NUnit |
| S4 | 6–17 Jul | Localización trilingüe |
| S4.5 | 20–24 Jul | Firebase + GCP + smoke test de Vertex AI |
| S5 | 27 Jul – 7 Ago | UI core (Views) |
| S6 | 10–21 Ago | Modelos 3D + animaciones + texto orbital |
| S7 | 24 Ago – 4 Sep | Narración (alta en ElevenLabs recién acá) |
| S8 | 7–18 Sep | Bases MDF + impresión de miniaturas |
| S9 | 21 Sep – 2 Oct | Chatbot + cache Levenshtein |
| S10 | 5–16 Oct | Pulido UX + decoradores de demo + 30 Q&A |
| S11 | 19–30 Oct | Testing, QA, video de respaldo |
| S12 | 2–6 Nov | Ensayos y contingencias |

Las fechas académicas que mandan (H1, H2, H3 = 09/11/2026) están en `CLAUDE.md` §3.

---

## 7. Historial de sprints (resumen)

| Sprint | Cierre | Hash | Qué dejó |
|---|---|---|---|
| S0 | 21/05/2026 | `c5cecf8` | Proyecto Unity + AR Foundation 5.2, Player Settings (IL2CPP, ARM64), XR Plug-in Management, `AppBootstrapper` + `AppContext`, 5 contratos, POCOs, primer APK en el A52s |
| S1 | 25/05/2026 | `266aa64` | `SenderoTrackingLibrary` con 1 placeholder, escenas `_App_Boot` → `_Main`, `ImageTrackingController` URP-safe, `FPSDiagnosticsHUD`, `TrackingLimitedTimeoutWatcher`. Validado en A52s: cubo cian (tracking) / ámbar (limited) / oculto, 30 FPS |
| S2 | en curso | merge `e630d00` | `MonumentDataSO`, `MonumentCatalogSO`, catálogo con los 5 monumentos, descripciones ES, `MonumentDataValidator`, `OnValidate`. **Falta**: los 5 marcadores reales |
| — | 04/09/2026 | `db36f3f`…`3f908cf` | Auditoría: fuga de `Material` cortada, imagen fuente de tracking versionada, `ARPlaneManager` off, validador de CI, README y `CLAUDE.md` alineados a la realidad |

> 🔁 `PROJECT_PROGRESS.md` afirmaba que en S1 se había "restaurado" `AppContext` como
> Service Locator completo (`Register<T>`, `Resolve<T>`, `TryResolve<T>`). **En el código,
> `AppContext` es solo una compuerta de inicialización** (`IsReady`, `Initialize()`,
> `Reset()`, `EnsureReady()`). El Service Locator está por construir — ver
> [`04_MVVM_Patterns.md`](04_MVVM_Patterns.md) §3.
