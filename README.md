<div align="center">

# 🌿 Sendero AR

### Realidad Aumentada para el Patrimonio Misional de Chiquitos

[![Unity](https://img.shields.io/badge/Unity-2022.3.62f3_LTS-000000?style=for-the-badge&logo=unity)](https://unity.com/releases/editor/whats-new/2022.3.62)
[![AR Foundation](https://img.shields.io/badge/AR_Foundation-5.2.0-success?style=for-the-badge)](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.2/manual/index.html)
[![C#](https://img.shields.io/badge/C%23-.NET_Standard_2.1-239120?style=for-the-badge&logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Firebase](https://img.shields.io/badge/Firebase-planificado_S8-9e9e9e?style=for-the-badge&logo=firebase)](https://firebase.google.com/docs/ai-logic)
[![Gemini](https://img.shields.io/badge/Gemini-planificado_S9-9e9e9e?style=for-the-badge&logo=google)](https://ai.google.dev/gemini-api/docs/models/gemini)

[![License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Android%20%7C%20iOS-lightgrey?style=flat-square)](#)
[![Status](https://img.shields.io/badge/status-Sprint_2_en_curso-orange?style=flat-square)](#)
[![CI](https://img.shields.io/badge/CI-integridad_sin_licencia-informational?style=flat-square)](.github/workflows/ci.yml)

**Aplicación móvil que utiliza Image Tracking y un chatbot histórico online para enriquecer la experiencia turística autónoma en las Misiones Jesuíticas de San José de Chiquitos, Bolivia (Patrimonio UNESCO desde 1990).**

</div>

---

## 📖 Tabla de Contenidos

- [Visión del Proyecto](#-visión-del-proyecto)
- [Características Clave](#-características-clave)
- [Stack Tecnológico](#-stack-tecnológico)
- [Arquitectura](#-arquitectura)
- [Estructura del Repositorio](#-estructura-del-repositorio)
- [Roadmap](#-roadmap)
- [Empezando](#-empezando)
- [Documentación](#-documentación)
- [Contribuir](#-contribuir)
- [Autor](#-autor)
- [Licencia](#-licencia)

---

## 🎯 Visión del Proyecto

**Sendero AR** busca acercar el patrimonio cultural boliviano mediante Realidad Aumentada. El objetivo: que la app reconozca cinco monumentos del Conjunto Misional de San José de Chiquitos, superponga un modelo 3D de cada uno y narre su historia en tres idiomas (Español, Inglés, Portugués), sin necesidad de internet.

> **Lo que corre hoy:** el reconocimiento de **un marcador impreso de prueba** (20 cm) en un
> Samsung A52s, con un cubo encima. Si el reconocimiento final será sobre la fachada real o
> sobre un marcador impreso es una decisión todavía abierta.

Como complemento online está planificado un chatbot histórico sobre **Gemini** (vía Firebase AI Logic, con el modelo configurable por Remote Config) que responda preguntas sobre las misiones jesuíticas. Diseño en [`docs/02_AI_Architecture.md`](docs/02_AI_Architecture.md).

---

## ✨ Características Clave

| Feature | Modo | Estado |
|---|---|---|
| 🎯 Image Tracking | Offline | 🟢 PoC validado en Samsung A52s con **1 marcador impreso** · 🟡 faltan los 5 reales |
| 🗿 Modelos 3D superpuestos | Offline | 🟡 Sprint 6 |
| 🎙️ Narraciones trilingües (ES/EN/PT) | Offline | 🟡 Sprint 7 |
| 🤖 Chatbot histórico con Gemini | Online | 🟡 Sprint 9 |
| 🌐 Cambio dinámico de idioma | Híbrido | 🟡 Sprint 4 |
| 📊 Telemetría con Firebase Analytics | Online | 🟡 Sprint 11 |

---

## 🛠️ Stack Tecnológico

### Frontend Móvil

| Componente | Versión | Propósito |
|---|---|---|
| **Unity LTS** | 2022.3.62f3 | Motor de juego y AR |
| **AR Foundation** | 5.2.0 | Framework AR multiplataforma |
| **ARCore XR Plugin** | 5.2.0 | Backend Android |
| **ARKit XR Plugin** | 5.2.0 | Backend iOS |
| **Universal RP** | 14.0 | Pipeline de renderizado optimizado para móvil |
| **TextMeshPro** | 3.0 | Renderizado de tipografía |

### Backend & IA

| Componente | Versión | Propósito |
|---|---|---|
| **Firebase Unity SDK** | 13.10.0+ | Backend serverless |
| **Firebase AI Logic** | API Mayo 2026 | Proxy seguro a Gemini |
| **Vertex AI Gemini** | `gemini-3.5-flash` previsto, vía Remote Config | Modelo de lenguaje |
| **Firebase App Check** | Play Integrity / DeviceCheck | Atestación criptográfica |
| **Firebase Remote Config** | — | Feature flags + A/B testing |

### Producción de Contenido

| Componente | Propósito |
|---|---|
| **RealityCapture** | Reconstrucción 3D fotogramétrica |
| **Blender 4.2 LTS** | Retopología y baking de texturas |
| **Azure Neural TTS** | Voz en español boliviano nativo |
| **ElevenLabs v3** | Voces premium en inglés y portugués |

### DevOps

| Componente | Propósito |
|---|---|
| **GitHub Actions** | CI/CD automatizado |
| **GameCI** | Builds Unity en la nube |
| **Trunk-Based Development** | Estrategia de branching |

---

## 🏛️ Arquitectura

El proyecto implementa el patrón **Model-View-ViewModel (MVVM) puro en C#**, sin frameworks externos como UniRx o Zenject. Esto garantiza:

- 🟡 **Testabilidad** — el código propio compila en tres assembly definitions (`SenderoAR.Models`, `SenderoAR.Core`, `SenderoAR.Tests.EditMode`) y hay **29 tests EditMode** sobre la capa de datos, en verde. Corren dentro de Unity (los modelos usan `ScriptableObject`), todavía no en el CI. Ver [`docs/04_MVVM_Patterns.md`](docs/04_MVVM_Patterns.md) §2
- ✅ **Zero allocation por frame** — objetivo de diseño: el color de estado de tracking se aplica con `MaterialPropertyBlock` y las etiquetas del HUD se cachean, precisamente para no generar basura en el bucle de render
- ✅ **Desacoplamiento total** — La capa de presentación ignora la existencia de la UI
- ✅ **Inyección de dependencias híbrida** — Service Locator + Constructor Injection

```
┌─────────────────────────────────────────────────────────┐
│                     CAPA DE PRESENTACIÓN                │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Views (MonoBehaviour)                            │   │
│  │  ↕ Manual Binding (cero reflection)               │   │
│  │  ViewModels (POCOs reactivos)                     │   │
│  └──────────────────────────────────────────────────┘   │
│                          ↕                              │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Services (IChatbotClient, IAudioPlaybackService)│   │
│  │  ↕                                                │   │
│  │  Repositories (IMonumentRepository)               │   │
│  └──────────────────────────────────────────────────┘   │
│                          ↕                              │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Models (POCOs inmutables, ScriptableObjects)    │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

---

## 📁 Estructura del Repositorio

```
SenderoAR/
├── .github/                          # CI/CD y plantillas
│   ├── workflows/
│   │   ├── ci.yml                    # Integridad del repo (sin licencia Unity)
│   │   └── build-android.yml         # Build GameCI — MANUAL, requiere secrets
│   ├── ISSUE_TEMPLATE/
│   └── PULL_REQUEST_TEMPLATE.md
│
├── Tools/
│   └── validate_project.py           # Validador que corre en CI
│
├── docs/                             # Documentación técnica (ver docs/README.md)
│
├── Assets/
│   └── _SenderoAR/                   # Todo el código del proyecto
│       ├── AR/Libraries/            # XRReferenceImageLibrary (versionada)
│       ├── Core/
│       │   ├── AR/                   # ImageTrackingController, TimeoutWatcher
│       │   ├── Contracts/            # 5 interfaces de servicio (sin impl. aún)
│       │   ├── Diagnostics/          # FPSDiagnosticsHUD (descartable, S1)
│       │   ├── Infrastructure/       # AppBootstrapper, AppContext, SceneNames
│       │   └── Materials/            # DebugCubeMat (URP)
│       ├── Models/                   # POCOs + MonumentDataSO + 5 assets
│       ├── Scenes/                   # _App_Boot, _Main
│       ├── Tests/EditMode/           # Tests NUnit (Unity Test Framework)
│       ├── ViewModels/               # (vacío — solo .gitkeep)
│       └── Views/                    # (vacío — solo .gitkeep)
│
│       Assembly definitions: SenderoAR.Models (Models/), SenderoAR.Core (Core/),
│       SenderoAR.Tests.EditMode (Tests/EditMode/). Ver docs/04_MVVM_Patterns.md §2.
│
│   └── _SenderoAR/Art/
│       ├── Tracking/                 # ✅ SÍ versionado: la librería de imágenes
│       │                             #    guarda el GUID de la textura, no sus
│       │                             #    píxeles. Sin esto, un clone no trackea.
│       ├── Models3D/                 # ⚠️ excluido — FBX de los 5 monumentos
│       └── Audio/{ES,EN,PT}/         # ⚠️ excluido — narraciones OGG Vorbis
│
├── Packages/
│   └── manifest.json                 # AR Foundation 5.2.0
│
├── .gitignore
├── LICENSE
└── README.md                         # Este archivo
```

---

## 🗓️ Roadmap

Proyecto estructurado en **12 sprints de 2 semanas** (24 semanas totales), siguiendo metodología **Scrum** con Trunk-Based Development.

| Sprint | Periodo planificado | Objetivo | Estado real al 16/10/2026 |
|---|---|---|---|
| **S0** | 18 – 22 May 2026 | Cimentación: setup repo + AR Foundation 5.2 | ✅ Cerrado |
| **S1** | 25 May – 5 Jun | Image Tracking PoC de 1 monumento | ✅ Cerrado — validado en dispositivo físico |
| **S2** | 8 – 19 Jun | Modelo de datos + ScriptableObjects | 🟡 **En curso** — capa de datos lista (5 `MonumentDataSO` + catálogo); faltan los 5 marcadores reales en la librería |
| **S3** | 22 Jun – 3 Jul | Arquitectura MVVM base | 🟡 Contratos escritos y `.asmdef` de Models/Core/Tests; sin implementaciones ni infraestructura MVVM |
| **S4** | 6 – 17 Jul | Sistema de localización trilingüe | ⬜ Descripciones EN/PT vacías en los 5 assets |
| **S5** | 20 – 31 Jul | UI Core (Views) | ⬜ |
| **S6** | 3 – 14 Ago | Integración de modelos 3D | ⬜ |
| **S7** | 17 – 28 Ago | Audio narration | ⬜ |
| **S8** | 31 Ago – 11 Sep | Firebase Setup + App Check | ⬜ Sin dependencias Firebase en `manifest.json` |
| **S9** | 14 – 25 Sep | Chatbot Gemini | ⬜ |
| **S10** | 28 Sep – 9 Oct | Pulido UX + performance | ⬜ |
| **S11** | 12 – 23 Oct | Testing + QA | 🟡 29 tests EditMode de la capa de datos; nada de PlayMode ni de integración AR |
| **S12** | 26 Oct – 6 Nov | Validación con usuarios reales | ⬜ |

> 📌 **El calendario de arriba es el planificado en Sprint 0 y está vencido.**
> El proyecto está en Sprint 2, no en Sprint 8. Las fechas de S3 en adelante
> necesitan replanificación antes de usarse para prometer nada.
> La defensa de grado quedó para **mitad de 2027**; las fechas vigentes están en
> [`CLAUDE.md`](CLAUDE.md) §3.

## 🚀 Empezando

### Requisitos Previos

- Unity Hub con licencia Personal activa
- Unity Editor **2022.3.62f3** instalado
- Android Build Support + iOS Build Support
- Git (LFS todavía **no** configurado: se decide en Sprint 6, con los FBX)
- Dispositivo físico Android (API 24+) o iOS (14+) para testing AR

### Clonar el Repositorio

```bash
git clone https://github.com/kendallab/SenderoAR.git
cd SenderoAR
```

### Abrir en Unity

1. Abrí Unity Hub
2. **Open** → seleccioná la carpeta `SenderoAR`
3. Cuando pregunte la versión, elegí **2022.3.62f3**
4. Esperá la primera importación (puede tomar 15-30 min la primera vez)

### Correr los tests

En el editor: **Window → General → Test Runner → EditMode → Run All**.

Sin abrir el editor (con el proyecto cerrado; código de salida 0 = todo verde):

```bash
"/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe" -batchmode -nographics -projectPath "C:\Proyectos\SenderoAR" -runTests -testPlatform EditMode -testResults resultados.xml -logFile tests.log
```

---

## 📚 Documentación

La documentación técnica vive en [`docs/`](docs/README.md): contexto y decisiones,
base de monumentos, arquitectura del chatbot, specs de image tracking, patrones MVVM,
pipelines de audio, 3D y CI, modo demo y lecciones de depuración. Cada sección indica si
describe algo **que ya está en el repo** o **un diseño todavía sin implementar**.
Las referencias del código (`03_AR_Tracking_Specs.md` §10, `02_AI_Architecture.md` §6)
apuntan a esos archivos.

Lo que es verificable sin abrir Unity:

```bash
python Tools/validate_project.py
```

Comprueba que un clone limpio produzca la misma app: pares asset/`.meta`, GUIDs
únicos, escenas del Build Settings presentes, componentes sin script faltante y
—lo importante— que cada imagen de la `XRReferenceImageLibrary` apunte a una
textura realmente versionada.

---

## 🤝 Contribuir

Este es un proyecto académico de grado en desarrollo activo. Si querés reportar un bug o sugerir una mejora:

1. Revisá los [Issues abiertos](../../issues)
2. Usá las plantillas correspondientes (Bug Report / Feature Request)
3. Para Pull Requests, seguí la convención **Conventional Commits**

---

## 👨‍💻 Autor

**Kevin Daniel Lozano** — _KENDAL Lab_

- 🎓 Universidad Privada del Valle (UPV) — Ingeniería de Sistemas Informáticos
- 👨‍🏫 Tutor: Ing. Jorge Gustavo Méndez Ayala
- 🌐 GitHub: [@kendallab](https://github.com/kendallab)

---

## 📄 Licencia

Este proyecto se distribuye bajo la Licencia MIT. Ver [`LICENSE`](LICENSE) para más información.

---

<div align="center">

**Hecho con 🤍 en Bolivia para el patrimonio chiquitano**

</div>
