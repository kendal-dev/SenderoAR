# CLAUDE.md — Contrato de trabajo sobre Sendero AR

> Repositorio de código del proyecto de grado **Sendero AR** (Univalle · Kevin Daniel Lozano).
> Este archivo vive en la **raíz del repo, fuera de `Assets/`**: Unity no lo importa,
> el build no lo ve, y Claude Code lo carga solo al trabajar en `C:\Proyectos\SenderoAR`.
>
> 🔴 **Un dato, un hogar** (regla 1 de KENDAL-OS). Este archivo **no redefine** nada que
> ya viva en el vault `C:\Users\kevin\Segundo_Cerebro`. Cuando hace falta un dato de
> alcance, calendario o normativa, **se referencia su hogar**, no se copia.
>
> Última medición contra el disco: **04/09/2026**.

---

## 1. Qué es esto y qué NO es

| | |
|---|---|
| **Es** | App Unity de AR (image tracking) para 5 monumentos de San José de Chiquitos |
| **Entrega** | Objetivo específico #4 del perfil aprobado: *construir el MVP* |
| **NO es** | El documento de tesis. El documento vive en el vault, `31_CARRERA/02_PROYECTO/` |
| **Hogar del alcance** | `31_CARRERA/02_PROYECTO/perfil/indice-perfil-proyecto.md` |
| **Hogar del calendario** | `31_CARRERA/01_UNIVERSIDAD/registro-clases-taller-2.md` §1.5 |
| **Hogar de los KPIs** | `31_CARRERA/07_REGISTRO/kpis-carrera.md` — C2 |

---

## 2. Estado real medido el 04/09/2026

### Lo que corre

| Capa | Estado |
|---|---|
| Image tracking (ARCore, AR Foundation 5.2) | 🟢 Validado en Samsung A52s con **1 marcador impreso de 20 cm** |
| Escenas `_App_Boot` → `_Main` | 🟢 Encadenadas, en Build Settings |
| Capa de datos | 🟢 `MonumentDataSO` + catálogo + **5 assets** con descripción ES |
| Contratos de servicio | 🟢 5 interfaces escritas y documentadas |
| Implementaciones de esos contratos | 🔴 **Cero.** No hay una sola clase que implemente ninguna |
| ViewModels / Views | 🔴 Carpetas vacías |
| Audio · 3D · chatbot · UI · tests | 🔴 Nada |
| CI | 🟢 `Tools/validate_project.py` (sin licencia Unity) + build GameCI manual |

### La distancia real

**Sprint 2 de 12.** El roadmap del README planificaba Sprint 8 para esta fecha.
Entre el 27/05 y el 04/09 no hubo un solo commit con cambios.

### Los 5 avisos que el validador da hoy

Los 5 `MonumentDataSO` apuntan a marcadores (`templo_san_jose`, `capilla_mortuoria`,
`torre_campanario`, `la_boveda`, `pila_bautismal`) que **no existen en la librería**.
Ese es el trabajo pendiente del Sprint 2, y es trabajo de cámara, no de código.

---

## 3. 🔴 Las tres fechas que mandan

| Hito | Fecha | Qué exige | Fuente |
|---|---|---|---|
| **H1 · Avance de mes** | 🟡 ~15/09/2026 | Casos de uso + diagrama de clases + 1 monumento real corriendo | Clase 24/08 |
| **H2 · Parcial 1 — defensa 1 a 1** | 🟡 ~15/10/2026 | Capítulo II + 3 monumentos + trilingüe | Clase 24/08 |
| **H3 · Parcial 2** | 🟢 **09/11/2026** | *"Codificación avanzada o casi terminada"* · 30 docente + 70 defensa | Clase 24/08, fecha exacta |

🔴 **Las fechas de H1 y H2 siguen siendo "mediados de".** Sin fecha exacta no son
un dato (regla 8). Se cierran preguntando en clase.

---

## 4. Definition of Done — no se negocia

Copiada del perfil aprobado §7.3.1 (**su hogar es `indice-perfil-proyecto.md` §5**).
Una funcionalidad está terminada **solo** cuando:

```
(a) el código está implementado, comentado y sigue las guías de estilo
(b) superó las pruebas unitarias y de integración
(c) su funcionamiento fue verificado en un DISPOSITIVO FÍSICO
(d) el código fue versionado y consolidado en GitHub
```

🔴 **Hoy el proyecto tiene 0 unidades cerradas bajo esta definición** — falla (b) en
todo y (d) en lo último. La condición (c), que es la cara, **ya está probada**.

---

## 5. Reglas de trabajo en este repo

| # | Regla | Por qué |
|---|---|---|
| **1** | 🔴 **`python Tools/validate_project.py` tiene que dar 0 errores antes de cada push** | Un clone roto ya costó el Sprint 1: la librería de tracking apuntaba a una imagen excluida del repo |
| **2** | 🔴 **Ningún commit vacío.** Si no hay cambios, no hay commit | El 26–30/05 se empujaron 8 commits vacíos, 3 con mensajes descriptivos y 0 archivos |
| **3** | 🔴 **Ninguna rama de sprint dura más de 2 semanas sin mergear a `main`** | Sprint 2 estuvo 100 días completo y sin integrar |
| **4** | 🔴 **Ningún dato inventado entra al repo ni al documento.** Si un número no tiene fuente con fecha y nombre, no se escribe | Regla 8 de KENDAL-OS. Es el riesgo #1 del proyecto — ver §6 |
| **5** | Conventional Commits, cuerpo en español, explicando **por qué** | Ya es la convención del historial |
| **6** | El `README.md` describe lo que el repo **tiene**, no lo que va a tener | Declaraba `docs/`, `Tests/`, LFS y CI que no existían |
| **7** | Assets pesados (FBX, audio) **fuera de Git**; las fuentes de tracking **dentro** | La librería guarda GUIDs, no píxeles |

---

## 6. 🔴🔴 El riesgo que no es técnico

La investigación de campo del perfil —**encuesta de 31 respuestas, aliado
*Sendero Tours*, Anexo A y Anexo B**— fue registrada como inventada en el vault el
02/09/2026 (`CLAUDE.md` §6.4, pendiente #41) y retirada de la bio pública y del CV.

🔴 **La corrección no se propagó a los archivos del proyecto.** Al 04/09 siguen
figurando como 🟢 verificados en `indice-perfil-proyecto.md`, `procesos-sendero-ar.md`,
`correcciones-perfil-taller2.md`, `indice-normativa-perfil.md`, `bitacora-taller-2.md`
y `registro-clases-taller-2.md`.

De ahí salen los dos números que el perfil declara como datos duros citables —
**68 % barrera idiomática · 87 % necesita offline**— y sobre ellos se construyeron
el Ishikawa corregido, el planteamiento del problema y los objetivos 1 y 5.

El feedback del propio docente dice: *"la problemática debe ser verificable; un
perfil puede ser rechazado si el problema no se considera real"*.

🟢 **La salida no exige confesar nada y ya está escrita en el vault**: la pregunta 4
de `indice-perfil-proyecto.md` §7 —*"¿la encuesta de 2025 sigue siendo válida por
antigüedad o hay que rehacer el trabajo de campo?"*— es la vía institucional normal
para reinstrumentar. **Un instrumento de 2025 rehecho en 2026 no le llama la
atención a nadie.**

---

## 7. Próximos pasos — en orden, sin saltar

> Detalle, horas y fechas: el plan de acción del 04/09/2026.

1. 🔴 **Decidir el alcance real de la demo** (fachada real vs marcador impreso) —
   condiciona todo lo demás. Ver la memoria `sendero-ar-fachada-vs-marcador`.
2. 🔴 **Cerrar la primera unidad funcional bajo la DoD**: test mínimo + commit sobre
   image tracking. Son 2 acciones, no un sprint.
3. 🔴 **5 fotos de marcador** → librería → los 5 avisos del validador en verde.
4. 🟡 Implementar `IMonumentRepository` sobre el catálogo que ya existe.
5. 🟡 Reemplazar el cubo por contenido real de 1 monumento (H1).
6. 🟡 Narración trilingüe por TTS · UI mínima · chatbot.

---

## 8. Comandos

```bash
python Tools/validate_project.py          # integridad del repo, sin Unity
git push origin main                       # 6 commits de la auditoría del 04/09 sin subir
```

Compilar sin abrir el editor (verifica que el C# no rompió nada):

```bash
"/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe" -batchmode -quit -nographics -projectPath "C:\Proyectos\SenderoAR" -logFile /tmp/unity.log
```

---

*Creado 04/09/2026 tras la auditoría de principio a fin · mide, no promete*
