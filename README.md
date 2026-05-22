<div align="center">

# 🌿 Sendero AR

### Realidad Aumentada para el Patrimonio Misional de Chiquitos

[![Unity](https://img.shields.io/badge/Unity-2022.3.62f3_LTS-000000?style=for-the-badge&logo=unity)](https://unity.com/releases/editor/whats-new/2022.3.62)
[![AR Foundation](https://img.shields.io/badge/AR_Foundation-5.2.0-success?style=for-the-badge)](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.2/manual/index.html)
[![C#](https://img.shields.io/badge/C%23-.NET_Standard_2.1-239120?style=for-the-badge&logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Firebase](https://img.shields.io/badge/Firebase-AI_Logic-FFCA28?style=for-the-badge&logo=firebase)](https://firebase.google.com/docs/ai-logic)
[![Gemini](https://img.shields.io/badge/Gemini-3.1_Flash--Lite-4285F4?style=for-the-badge&logo=google)](https://ai.google.dev/gemini-api/docs/models/gemini)

[![License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Android%20%7C%20iOS-lightgrey?style=flat-square)](#)
[![Status](https://img.shields.io/badge/status-MVP_in_development-orange?style=flat-square)](#)

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

**Sendero AR** democratiza el acceso al patrimonio cultural boliviano mediante experiencias inmersivas de Realidad Aumentada. La aplicación reconoce las fachadas de cinco monumentos históricos en San José de Chiquitos, superpone modelos 3D reconstruidos digitalmente y narra su historia en tres idiomas (Español, Inglés, Portugués) — todo funcionando offline.

Para profundizar en la experiencia, un chatbot histórico impulsado por **Gemini 3.1 Flash-Lite** responde preguntas académicas sobre las misiones jesuíticas, la arquitectura Barroco Mestizo y el legado de figuras como Martin Schmid y Hans Roth.

---

## ✨ Características Clave

| Feature | Modo | Estado |
|---|---|---|
| 🎯 Image Tracking de 5 monumentos | Offline | 🟡 Sprint 1-2 |
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
| **Vertex AI Gemini** | 3.1 Flash-Lite | Modelo de lenguaje |
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

- ✅ **Testabilidad suprema** — ViewModels son POCOs ejecutables con NUnit sin entorno Unity
- ✅ **Zero allocation por frame** — Binding manual con delegados precompilados
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
│   │   └── ci.yml                    # Pipeline GitHub Actions
│   ├── ISSUE_TEMPLATE/
│   └── PULL_REQUEST_TEMPLATE.md
│
├── Assets/
│   └── _SenderoAR/                   # Todo el código del proyecto
│       ├── Core/                     # Infraestructura MVVM + Servicios
│       │   ├── Infrastructure/       # BaseViewModel, ReactiveProperty
│       │   └── Services/             # FirebaseAI, Audio, Localization
│       ├── Models/                   # POCOs + ScriptableObjects
│       ├── ViewModels/               # Lógica de presentación reactiva
│       ├── Views/                    # MonoBehaviours + Prefabs
│       └── Tests/                    # NUnit unit tests
│
│   └── _SenderoAR/Art/               # ⚠️ EXCLUIDO del repo (gitignore)
│       ├── Models3D/                 # FBX de los 5 monumentos
│       ├── Audio/{ES,EN,PT}/         # Narraciones OGG Vorbis
│       └── ReferenceLibrary/         # XRReferenceImageLibrary
│
├── Packages/
│   └── manifest.json                 # AR Foundation 5.2.0
│
├── docs/                             # Documentación técnica
│
├── .gitignore
├── LICENSE
└── README.md                         # Este archivo
```

---

## 🗓️ Roadmap

Proyecto estructurado en **12 sprints de 2 semanas** (24 semanas totales), siguiendo metodología **Scrum** con Trunk-Based Development.

| Sprint | Periodo | Objetivo |
|---|---|---|
| **S0** | 18 – 22 May 2026 | Cimentación: setup repo + AR Foundation 5.2 |
| **S1** | 25 May – 5 Jun | Image Tracking PoC de 1 monumento |
| **S2** | 8 – 19 Jun | Modelo de datos + ScriptableObjects |
| **S3** | 22 Jun – 3 Jul | Arquitectura MVVM base |
| **S4** | 6 – 17 Jul | Sistema de localización trilingüe |
| **S5** | 20 – 31 Jul | UI Core (Views) |
| **S6** | 3 – 14 Ago | Integración de modelos 3D |
| **S7** | 17 – 28 Ago | Audio narration |
| **S8** | 31 Ago – 11 Sep | Firebase Setup + App Check |
| **S9** | 14 – 25 Sep | Chatbot Gemini |
| **S10** | 28 Sep – 9 Oct | Pulido UX + performance |
| **S11** | 12 – 23 Oct | Testing + QA |
| **S12** | 26 Oct – 6 Nov | Validación con usuarios reales |
| **Buffer** | 9 – 13 Nov | Defensa privada UPV |

---

## 🚀 Empezando

### Requisitos Previos

- Unity Hub con licencia Personal activa
- Unity Editor **2022.3.62f3** instalado
- Android Build Support + iOS Build Support
- Git con LFS configurado
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

---

## 📚 Documentación

Toda la documentación técnica vive en `/docs/`. Documentos clave:

- 📄 **Contexto del Proyecto** — Visión, stack y decisiones inmutables
- 🏛️ **Database de Monumentos** — Specs de los 5 monumentos
- 🤖 **Arquitectura de IA** — Gemini + Vertex AI + App Check
- 🎯 **Especificaciones AR** — Image Tracking detallado
- 🧬 **Patrones MVVM** — Arquitectura de software
- 🎙️ **Pipeline TTS** — Generación de narraciones
- 🗿 **Pipeline 3D** — Producción de modelos
- ⚙️ **Pipeline CI/CD** — GitHub Actions

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
