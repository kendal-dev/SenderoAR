# 📚 Documentación técnica — Sendero AR

> Importada el **16/10/2026** desde el paquete externo `SenderoAR_Docs` (15 archivos,
> escritos entre el 21 y el 28/05/2026) y **contrastada contra el código** del repo en esa
> fecha. Hasta hoy el README decía que estos documentos vivían fuera del repositorio; ahora
> viven acá, y las referencias que el código ya hacía (`03_AR_Tracking_Specs.md §10`,
> `02_AI_Architecture.md §6`, `04_MVVM_Patterns.md`) apuntan a archivos reales.

---

## Cómo leer estos documentos

Cada sección lleva una de estas marcas. **La marca manda sobre el tono del texto**: un
párrafo puede sonar a decisión cerrada y seguir marcado 📐 si nadie lo implementó.

| Marca | Significado |
|---|---|
| ✅ **En el repo** | Existe en el código y se verificó contra él el 16/10/2026 |
| 📐 **Diseño** | Decidido en mayo de 2026, **sin implementar**. Es el plan, no el estado |
| ⚠️ **Por verificar** | Dato o afirmación salida de los reportes *Deep Research* (Gemini) sin fuente primaria con nombre y fecha. **No se cita en la tesis ni en el chatbot** hasta contrastarlo (regla 4 de `CLAUDE.md`) |
| 🔁 **Corregido** | El documento original decía otra cosa y el código (o el compilador) demostró que estaba mal. Se anota qué cambió |

Para el **estado real** del proyecto (qué corre, qué falta, fechas de entrega) la fuente
es [`CLAUDE.md`](../CLAUDE.md) en la raíz, no estos documentos.

---

## Índice

| Archivo | Qué contiene | Cuándo leerlo |
|---|---|---|
| [`00_Contexto_SenderoAR.md`](00_Contexto_SenderoAR.md) | Identidad, stack, decisiones consolidadas con su estado, roadmap original, historial de sprints | Al empezar cualquier sesión que toque arquitectura o alcance |
| [`01_Monuments_Database.md`](01_Monuments_Database.md) | Los 5 monumentos: IDs, datos históricos, specs de tracking, modelo 3D y animaciones | Al tocar `MonumentDataSO`, los `.asset`, el catálogo o los textos |
| [`02_AI_Architecture.md`](02_AI_Architecture.md) | Chatbot: modelo, backend, Remote Config, App Check, manejo de errores, system prompt | Antes de implementar `IChatbotClient` |
| [`03_AR_Tracking_Specs.md`](03_AR_Tracking_Specs.md) | Image tracking: marcador físico, librería de referencia, `arcoreimg`, estados, presupuesto de rendimiento | Al tocar `Core/AR/`, la librería de tracking o los marcadores |
| [`04_MVVM_Patterns.md`](04_MVVM_Patterns.md) | MVVM puro, assembly definitions, Composition Root, binding, convenciones de código | Antes de escribir el primer ViewModel o `.asmdef` |
| [`05_TTS_Pipeline.md`](05_TTS_Pipeline.md) | Narración trilingüe: proveedores, pipeline batch, Addressables, importer de audio | Antes de implementar `IAudioPlaybackService` |
| [`06_3D_Pipeline.md`](06_3D_Pipeline.md) | Fotogrametría, retopología, importación a Unity, shaders, texto orbital (las miniaturas SLA quedaron fuera del MVP) | Al reemplazar el cubo de debug por contenido real |
| [`07_CICD_Pipeline.md`](07_CICD_Pipeline.md) | Lo que CI hace hoy y lo que estaba planificado (tests, distribución, firma) | Al tocar `.github/workflows/` |
| [`08_Demo_Mode_Defense.md`](08_Demo_Mode_Defense.md) | Modo demo: decoradores, cache offline Levenshtein, HUD, checklist de defensa | Sprint de pulido y preparación de la defensa |
| [`09_Lecciones_Unity.md`](09_Lecciones_Unity.md) | Trampas de Unity / URP / AR Foundation / Git en Windows ya pagadas con tiempo | **Antes de depurar cualquier cosa rara.** Probablemente ya pasó |

---

## Qué NO se importó, y por qué

| Origen | Decisión | Motivo |
|---|---|---|
| `Perfil_Proyecto_04_06_Corregido.md` | ❌ No importado | Es el documento académico, no documentación de código. Su hogar es el vault (`31_CARRERA/02_PROYECTO/`). Además contiene la encuesta de 31 respuestas, el Anexo A y el Anexo B, registrados como no verificables (ver `CLAUDE.md` §6). Copiarlo a un repo público los volvería a publicar |
| `sendero-ar-estrategia-ig-bitacora-visual.md` | ❌ No importado | Estrategia de Instagram. No tiene relación con el código |
| `PROJECT_INSTRUCTIONS.md` | ❌ No importado | Instrucciones de rol para un Project de Claude.ai ("Chief Software Architect", formato Raw Spec). Las reglas de trabajo vigentes están en `CLAUDE.md` |
| `SESSION_HANDOFF.md` / `PROJECT_PROGRESS.md` | 🔀 Destilados | Lo durable (decisiones, lecciones, historia de sprints) pasó a `00_` y `09_`. El "prompt de arranque" y los planes día a día de mayo están vencidos |
| `gitignore.md` | ❌ No importado | Copia vieja y genérica del `.gitignore`. El real, en la raíz, es más completo y es el que manda |
| Datos de facturación GCP (tarjeta, país y dirección de facturación) | ❌ No importados | Datos financieros personales. No van en un repositorio público |
| Teléfonos y WhatsApp de proveedores | ❌ No importados | Datos de contacto de terceros. Quedan los nombres; los contactos siguen en los documentos originales, fuera del repo |
| En `08_`: latencia artificial "para no levantar sospecha", "nunca mencionar *cache* ni *fallback* delante del tribunal", frase de excusa preparada | ❌ Eliminados | Buscaban que el tribunal no supiera qué estaba viendo. El cache offline es una **funcionalidad legítima** del diseño y se presenta como tal. Ver `08_` §1 |
| Porcentajes de "probabilidad de tracking" por monumento (88–95 %) | ❌ Eliminados | Eran estimaciones sin medición. Quedó el umbral exigible (`arcoreimg` ≥ 85) como requisito, no como dato |

---

## Cómo mantener esto

1. **Si el código contradice un documento, el código gana** y el documento se corrige en el
   mismo commit, con una marca 🔁 que diga qué cambió.
2. Cuando algo 📐 se implementa, pasa a ✅ **en el mismo commit que lo implementa**.
3. Un dato ⚠️ pasa a ✅ solo cuando se le agrega la fuente (autor, obra o institución, fecha).
4. Estos archivos están **fuera de `Assets/`**: Unity no los importa, no necesitan `.meta`
   y no entran al build.
