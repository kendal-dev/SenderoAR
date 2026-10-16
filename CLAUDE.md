# CLAUDE.md — Contrato de trabajo sobre Sendero AR

> Repositorio de código del proyecto de grado **Sendero AR** (Univalle · Kevin Daniel Lozano).
> Este archivo vive en la **raíz del repo, fuera de `Assets/`**: Unity no lo importa,
> el build no lo ve, y Claude Code lo carga solo al trabajar en `C:\Proyectos\SenderoAR`.
>
> 🔴 **Un dato, un hogar** (regla 1 de KENDAL-OS). Este archivo **no redefine** nada que
> ya viva en el vault `C:\Users\kevin\Segundo_Cerebro`. Cuando hace falta un dato de
> alcance, calendario o normativa, **se referencia su hogar**, no se copia.
>
> Última medición contra el disco: **16/10/2026** (la anterior: 04/09/2026).

---

## 1. Qué es esto y qué NO es

| | |
|---|---|
| **Es** | App Unity de AR (image tracking) para 5 monumentos de San José de Chiquitos |
| **Entrega** | Objetivo específico #4 del perfil aprobado: *construir el MVP* |
| **NO es** | El documento de tesis. El documento vive en el vault, `31_CARRERA/02_PROYECTO/` |
| **Hogar del alcance** | `31_CARRERA/02_PROYECTO/perfil/indice-perfil-proyecto.md` |
| **Hogar del calendario** | `31_CARRERA/01_UNIVERSIDAD/bitacora-taller-2.md` (defensa, mitad de 2027) · `32_SOCIAL/03_ORATORIA/temas-presentacion-final.md` (demos). 🗄️ Antes: `registro-clases-taller-2.md` §1.5, que era de Taller II |
| **Hogar de los KPIs** | `31_CARRERA/07_REGISTRO/kpis-carrera.md` — C2 |
| **Hogar del diseño técnico** | [`docs/`](docs/README.md) en este repo — importado y contrastado con el código el 16/10/2026 |

### 🟢 Alcance del MVP — decidido por Daniel el 16/10/2026

| | |
|---|---|
| **En el lugar** | La app reconoce **la fachada real** de cada monumento en San José de Chiquitos |
| **En la defensa en vivo** | Se escanean **fotos impresas** de esas mismas fachadas |
| **Queda fuera del MVP** | El enfoque *diorama* de mayo: bases MDF de 60 × 60 cm, marcadores diseñados con paleta y ruido fractal, miniaturas de resina. Ver `docs/03` §2.4 |

Lo que se sigue de esto, y que el código tiene que respetar:

1. **La imagen de referencia de cada monumento es una foto frontal de su fachada**, no un
   marcador diseñado. Es la misma imagen en el lugar y en la defensa.
2. **Dos escalas para la misma foto.** ARCore usa el `Specify Size` para calcular distancia
   y escala: la fachada mide metros y la foto impresa, centímetros. Diseño propuesto
   (`docs/03` §2.3): una librería de campo y otra de demo con las mismas texturas y tamaños
   distintos, elegidas al arrancar, y contenido escalado según `ARTrackedImage.size`.
3. **La Pila Bautismal no tiene fachada** y la Torre no es plana: hace falta decidir qué
   imagen representa a cada una (§7.1, D2).
4. **La condición (c) de la DoD para "reconoce la fachada" solo se cumple en el lugar.** En
   la defensa, la foto impresa demuestra el mecanismo; la prueba de que funciona sobre la
   fachada real es un video grabado allá. Se dice así: es la regla de `temas-presentacion-final.md` §3.
5. Hogar del alcance: el vault (`indice-perfil-proyecto.md`), congelado hasta el 31/12.
   Esta decisión vive acá hasta que Daniel la pase allá.

### Antes de escribir código, leer en `docs/`

| Al tocar… | Leer |
|---|---|
| Cualquier cosa rara al depurar | `docs/09_Lecciones_Unity.md` — probablemente ya pasó |
| **Cualquier `.cs` nuevo** | `docs/04_MVVM_Patterns.md` §2.1 — va dentro de una carpeta con `.asmdef`; si cae en `Assembly-CSharp`, los tests no lo ven |
| `Core/AR/`, la librería de tracking, marcadores | `docs/03_AR_Tracking_Specs.md` |
| `.asmdef`, ViewModels, `AppContext`, un contrato nuevo | `docs/04_MVVM_Patterns.md` |
| `MonumentDataSO`, los `.asset`, textos históricos | `docs/01_Monuments_Database.md` — 🔴 datos históricos **sin fuente** |
| Chatbot / audio / 3D / CI / defensa | `docs/02_` · `05_` · `06_` · `07_` · `08_` |

Las marcas ✅ / 📐 / ⚠️ / 🔁 de esos documentos dicen si algo **existe**, es **diseño**,
está **sin fuente** o fue **corregido**. Si el código contradice un documento, gana el
código y el documento se corrige en el mismo commit.

---

## 2. Estado real medido el 16/10/2026

### Lo que corre

| Capa | Estado |
|---|---|
| Image tracking (ARCore, AR Foundation 5.2) | 🟢 Validado en Samsung A52s con **1 marcador impreso de 20 cm** |
| Escenas `_App_Boot` → `_Main` | 🟢 Encadenadas, en Build Settings |
| Capa de datos | 🟢 `MonumentDataSO` + catálogo + **5 assets** con descripción ES |
| Contratos de servicio | 🟢 5 interfaces escritas y documentadas |
| Assembly definitions | 🟢 `SenderoAR.Models`, `SenderoAR.Core`, `SenderoAR.Tests.EditMode` (opción (a) de `docs/04` §2.3: sin mover archivos ni GUIDs) |
| Tests | 🟢 **29 tests EditMode en verde** (batchmode, 16/10), también en un clone limpio desde GitHub con importación completa. Cubren la capa de datos: catálogo, `OnValidate`, datos reales de los 5 `.asset`, enums serializados, `MonumentSnapshot` |
| Compilación | 🟢 Batchmode con 2022.3.62f3 el 16/10: **0 errores de C#** |
| Documentación técnica | 🟢 `docs/` importada y contrastada con el código |
| CI en `main` | 🟢 Validador determinístico mergeado (`b209fd6`, merge `643338d`): 0 errores también en un clone limpio. Estuvo en rojo desde `553144d` hasta ese merge. Los tests **no** corren en CI (falta licencia de Unity en los secrets) |
| Implementaciones de los contratos | 🔴 **Cero.** No hay una sola clase que implemente ninguna |
| ViewModels / Views | 🔴 Carpetas vacías (solo `.gitkeep`) |
| Audio · 3D · chatbot · UI | 🔴 Nada |

### La distancia real

**Sprint 2 de 12.** Entre el 27/05 y el 04/09 no hubo un solo commit con cambios;
entre el 04/09 y el 16/10, solo el de documentación. El 16/10 entraron los `.asmdef` y los
primeros tests: es el primer avance de código desde la auditoría.

### Ruido en Git — pendiente de Daniel

Borrar ramas y stashes lo bloquea el modo automático de Claude (es irreversible). Se
verificó el 16/10 que **todo esto ya está contenido en `main`** y se puede borrar sin
perder nada:

| Qué | Por qué sobra |
|---|---|
| `feat/sprint-2-monument-scriptable-objects` (local y remota) | Mergeada en `e630d00` |
| `ccr-f1a5b7d4-y0d83n` (remota) | Mergeada en `643338d` |
| `backup/main-pre-auditoria-2026-09-04` (local) | Su punta está en la historia de `main` |
| `stash@{0}` `849e15d` (ProjectSettings) | Idéntico a `HEAD` (`preloadedAssets: []`, que XR Management completa solo durante el build) |
| `stash@{1}` `245819c` (borra `Assets/XR/Temp.meta`) | Ya aplicado en `b209fd6`; `Assets/XR/Temp` ahora está en `.gitignore` |

```bash
git branch -d feat/sprint-2-monument-scriptable-objects backup/main-pre-auditoria-2026-09-04
git push origin --delete feat/sprint-2-monument-scriptable-objects ccr-f1a5b7d4-y0d83n
git stash drop stash@{1}
git stash drop stash@{0}
```

### Los 5 avisos que el validador da hoy

Los 5 `MonumentDataSO` apuntan a marcadores (`templo_san_jose`, `capilla_mortuoria`,
`torre_campanario`, `la_boveda`, `pila_bautismal`) que **no existen en la librería**.
Ese es el trabajo pendiente del Sprint 2, y es trabajo de cámara, no de código.

---

## 3. 🔴 Las fechas que mandan

🔁 **Cambio registrado en el vault el 22/09/2026**: Daniel dejó de ir a Taller de
Sistemas II como oyente, y **la defensa de Sendero AR es a mitad de 2027**
(hogar: `31_CARRERA/01_UNIVERSIDAD/bitacora-taller-2.md`, frontmatter y §8.1).
Los hitos H1 (~15/09), H2 (~15/10) y H3 (09/11/2026) eran de esa materia y **ya no
aplican** a este proyecto.

| Fecha | Qué | Qué exige del repo | Fuente |
|---|---|---|---|
| ⬜ sin dato | **Predefensa** | Demo que corra en vivo | `32_SOCIAL/03_ORATORIA/temas-presentacion-final.md` §5 |
| 🟡 24/10/2026 | AWS (pitch) | Opcional: demo de 3 min | ídem §1 |
| 🟡 14–15/11/2026 | NASA Space Apps | Opcional: demo de 3 min | ídem §1 |
| 🟢 mitad de 2027 | **Defensa de grado** | El MVP completo bajo la DoD | `bitacora-taller-2.md` |

🔴 **La regla de cualquier demo** (de `temas-presentacion-final.md` §3): se muestra solo lo
que corre ese día; lo que no corre se dice como "lo que falta". Hoy corre el reconocimiento
del marcador impreso de prueba (la Misión de San Javier, no uno de los 5 monumentos) con un
cubo encima. Nada más.

En la defensa se escanean fotos impresas de las fachadas (§1). Se dice así: *"en el lugar
reconoce la fachada real; acá uso una foto impresa de la misma fachada"*, y se acompaña con
el video grabado allá (D6). Sin ese video, el reconocimiento de la fachada real va en
"lo que falta".

🟢 **Lo que cambia para el trabajo**: hay meses, no semanas. Eso permite hacer las cosas
bajo la DoD desde la primera unidad (tests incluidos) en vez de correr a mostrar algo.

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

🔴 **Hoy el proyecto tiene 0 unidades cerradas bajo esta definición.**

La más cercana es la **capa de datos** (`MonumentDataSO` + catálogo): cumple (a), (b) (29
tests EditMode, incluidos los que leen los `.asset` reales) y (d). Le falta (c): **nada en
la escena la usa todavía**, así que no hay nada que ver en el teléfono. Se cierra cuando el
tracking la consuma (§7.2: C1–C3 y C6) y Daniel lo vea en el A52s con una foto impresa (D7). El tracking en sí cumple
(c) desde S1, pero no tiene tests.

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

## 7. Próximos pasos

> Revisado el 16/10/2026. Hecho ese día: CI en verde, `.asmdef`, 29 tests, docs alineadas.
> El trabajo quedó partido en dos listas: lo que **solo Daniel puede hacer** (decisiones,
> cámara, teléfono, fuentes, credenciales) y lo que **Claude hace sin él**.

### 7.1. Lo que solo puede hacer Daniel

| # | Tarea | Por qué no lo puede hacer Claude | Desbloquea |
|---|---|---|---|
| D1 | ✅ **Alcance del MVP decidido el 16/10/2026**: fachada real en el lugar, fotos impresas en la defensa (§1) | — | Todo lo de abajo |
| D2 | 🔴 **Decidir qué imagen representa a la Pila Bautismal** (es un objeto curvo dentro del templo, no una fachada) **y a la Torre** (no es plana). Opciones para la Pila: (a) foto frontal de su cara tallada, con riesgo de tracking por la curvatura; (b) otro plano fijo cercano, como una placa; (c) reemplazar `mon_05`. Para la Torre: foto del primer cuerpo, que es el tramo más plano | Es una decisión de alcance y de contenido | D3, C4 |
| D3 | 🔴 **Las 5 fotos de referencia**: frontales, sin perspectiva fuerte, luz pareja, alta resolución. Lo ideal es tomarlas en el lugar (D6); mientras tanto sirven fotos web con licencia, **anotando autor y licencia**. Validar con `arcoreimg` (≥ 85 en Linux/WSL). Dejar los PNG en `Assets/_SenderoAR/Art/Tracking/` | Hace falta cámara o criterio para elegir y licencias | C4 |
| D4 | 🔴 **El ancho real de cada fachada en metros**, medido o con fuente (autor y fecha). Es el `Specify Size` de la librería de campo, y por la regla 4 no se puede estimar | Medición en el lugar o bibliografía | C4 |
| D5 | 🔴 **Imprimir las 5 fotos para la demo** en papel **mate** (el brillo rompe el tracking), sin bordes recortados, y anotar el ancho impreso en metros | Impresora | C4 (librería de demo) |
| D6 | 🔴 **Viaje a San José de Chiquitos**: validar sobre las fachadas reales (condición (c) de la DoD), **grabar un video de cada reconocimiento** como evidencia para la defensa y tomar fotos de referencia propias, en más de un horario si la luz cambia mucho | Hay que estar ahí | DoD de "reconoce la fachada"; C5 |
| D7 | 🔴 **Probar en el Samsung A52s** cada cosa nueva, con las fotos impresas | Condición (c) de la DoD | Cerrar unidades bajo la DoD |
| D8 | 🔴 **Fuente para cada dato histórico ⚠️**, empezando por la Pila "c. 1652" (`docs/01_Monuments_Database.md`) | Bibliografía, Plan Misiones o el lugar | Audio, chatbot y textos de la tesis |
| D9 | 🟡 Limpiar Git con los 4 comandos de §2 | El modo automático bloquea borrar ramas y stashes | Nada; es orden |
| D10 | 🟡 Cargar `UNITY_LICENSE`, `UNITY_EMAIL` y `UNITY_PASSWORD` en los secrets de GitHub | Son credenciales personales | C8 |
| D11 | 🟡 Averiguar la fecha de la predefensa y la del viaje (D6) | Información de la universidad y logística propia | Planificar hacia atrás |

### 7.2. Lo que hace Claude, en orden

| # | Tarea | Depende de |
|---|---|---|
| C1 | Agregar `DisplayName` a `MonumentDataSO` y completarlo en los 5 `.asset` (lo exige `MonumentSnapshot`; ver `docs/04` §6) + test | Nada |
| C2 | Implementar `IMonumentRepository` sobre `MonumentCatalogSO` + tests | C1 |
| C3 | Completar `AppContext` como Service Locator (`Register`/`Resolve`/`TryResolve`, `docs/04` §3) + tests, y registrar el repositorio en `AppBootstrapper` | C2 |
| C4 | **Dos librerías de tracking con las mismas texturas**: campo (ancho real, D4) y demo (ancho impreso, D5), elegidas al arrancar. Extender el validador para que compruebe que las dos apuntan a las mismas imágenes versionadas. El mecanismo se puede armar ya con el placeholder; las fotos reales entran cuando lleguen | Mecanismo: nada · Fotos: D2–D5 |
| C5 | Si D6 trae más de una foto por fachada (por horario), permitir **varias imágenes por monumento** (`AnchorImageName` pasa a ser un prefijo o una lista) + tests | D6 |
| C6 | Reemplazar el cubo de debug por el **nombre del monumento** (TextMeshPro 3D) resuelto vía repositorio, **con escala proporcional a `ARTrackedImage.size`**, para que se lea igual sobre una foto de 30 cm que sobre una fachada de varios metros → primera unidad candidata a DoD | C3 · cierra con D7 (impresa) y D6 (fachada) |
| C7 | Revisar lo que quedó del enfoque diorama en el código: el valor de respaldo de 0.60 m en `MonumentDataSO.OnValidate` y el significado de `IsPhysicalInDefense` (venían de las bases MDF y las miniaturas) + tests | Nada |
| C8 | Workflow de CI que corra los tests EditMode con GameCI | D10 |
| C9 | Contenido real de 1 monumento, narración trilingüe, UI mínima, chatbot | D8 |

---

## 8. Comandos

```bash
python Tools/validate_project.py          # integridad del repo, sin Unity
gh run list --limit 5                      # estado del CI en GitHub
```

En esta máquina `python` no está en el PATH de Git Bash: usar `py -I Tools/validate_project.py`.

Compilar sin abrir el editor (verifica que el C# no rompió nada; ~2–5 min, falla si el
editor tiene el proyecto abierto):

```bash
"/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe" -batchmode -quit -nographics -projectPath "C:\Proyectos\SenderoAR" -logFile /tmp/unity.log
```

Correr los tests EditMode sin abrir el editor (~3 min; código de salida 0 = verde; sin
`-quit`, porque `-runTests` cierra solo):

```bash
"/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe" -batchmode -nographics -projectPath "C:\Proyectos\SenderoAR" -runTests -testPlatform EditMode -testResults /tmp/resultados.xml -logFile /tmp/tests.log
```

**Antes de cada commit con C#**: tests en verde. **Antes de cada push**: validador con 0 errores.

---

*Creado 04/09/2026 tras la auditoría de principio a fin · mide, no promete*
