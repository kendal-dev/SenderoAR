# 03 · Image tracking

> Destilado de `03_AR_Tracking_Specs.md` v3.0 (21/05/2026) y de las lecciones de Sprint 1.
> Contrastado contra `Core/AR/` y la escena `_Main` el 16/10/2026.
> Leyenda de marcas: ver [`README.md`](README.md).
>
> La numeración de secciones se conserva: el código cita **§10**.

---

## 1. Stack — ✅ en el repo

| Componente | Valor |
|---|---|
| Unity | 2022.3.62f3 LTS |
| AR Foundation / ARCore / ARKit | **5.2.0** |
| XR Management | 4.5.0 |
| Librería de referencia | **Build-time**, estática: `Assets/_SenderoAR/AR/Libraries/SenderoTrackingLibrary.asset` |
| Android | API 24+, ARM64, IL2CPP |
| iOS | 14+ |

Hoy la librería tiene **1 imagen**: `placeholder_marker` (Misión de San Javier, impresa a
0.20 × 0.20 m). Fuente versionada en `Assets/_SenderoAR/Art/Tracking/placeholder_marker.png`.

---

## 2. Qué se trackea — ✅ decidido el 16/10/2026

**Alcance del MVP (Daniel, 16/10/2026):** en el lugar, la app reconoce **la fachada real**;
en la defensa en vivo se escanean **fotos impresas** de esas fachadas. Las tareas que se
desprenden están en `CLAUDE.md` §7 (D2–D6 y C4–C7).

### 2.1. La fachada real — 📐 diseño

- **La imagen de referencia es una foto frontal de la fachada**: tomada de frente, sin
  perspectiva fuerte, con luz pareja, en alta resolución y recortada a la parte plana y
  característica. La misma foto sirve para el campo y para la demo.
- **`Specify Size` = ancho real de la fachada en metros**, medido o con fuente (regla 4).
  ARCore y ARKit lo usan para estimar distancia y escala: si no coincide con lo que la
  cámara ve, el contenido aparece a la distancia y la escala equivocadas.
- **Monumentos que no son una fachada plana** (decisión D2 de `CLAUDE.md`):
  - *Pila Bautismal*: objeto curvo dentro del templo. Una foto de su cara tallada es la
    opción más directa, pero el image tracking supone una superficie plana ⚠️: hay que
    probarlo antes de comprometerse.
  - *Torre*: volumen de tres cuerpos. Lo más plano es el primer cuerpo.
- **Riesgos de trackear una fachada al aire libre** ⚠️ (a verificar en el lugar, D6):
  - La luz cambia en el día: sombras que se mueven sobre la piedra pueden bajar el
    reconocimiento. Si pasa, se agrega más de una foto por fachada, de distintos horarios (C5).
  - Gente, autos y árboles delante de la fachada tapan partes de la imagen.
  - La distancia: la fachada tiene que ocupar buena parte del cuadro, así que hay que estar
    lejos, y la perspectiva desde abajo deforma la imagen.

### 2.2. Las fotos impresas en la defensa — 📐 diseño

- Son **las mismas imágenes** de referencia, impresas en papel **mate**: el brillo y el
  laminado brillante generan reflejos que rompen el tracking (ver §7, *prohibido brillante*).
- Impresas planas (sin dobleces), sin recortar bordes y a un tamaño que se lea bien a
  distancia de brazo. Lo validado hasta hoy es **20 cm en el A52s**; tamaños mayores hay que
  probarlos (D5, D7).
- **El ancho impreso en metros es el `Specify Size` de la librería de demo.**

### 2.3. Dos librerías, mismas texturas — 📐 diseño (tarea C4)

| Librería | `Specify Size` | Cuándo |
|---|---|---|
| `SenderoTrackingLibrary` | Ancho real de cada fachada | En el lugar |
| `SenderoTrackingLibrary_Demo` | Ancho de cada foto impresa | Defensa y pruebas en casa |

- Mismas texturas y **mismos nombres de imagen**, así `AnchorImageName` resuelve igual en
  las dos.
- Se elige **al arrancar**: `ARTrackedImageManager.referenceLibrary` se asigna antes de que
  el manager se habilite. Cómo se elige (ajuste en la app, símbolo de compilación o un
  `ScriptableObject` de configuración) se decide al implementar.
- El contenido se escala con **`ARTrackedImage.size`**, de modo que el mismo prefab se vea
  proporcionado sobre una foto de 30 cm y sobre una fachada de varios metros.
- El validador (`Tools/validate_project.py`) se extiende para comprobar que las dos
  librerías apunten a las mismas imágenes versionadas.

### 2.4. 🗄️ Enfoque diorama — fuera del MVP desde el 16/10/2026

> Diseño de mayo para el marcador impreso, retirado al decidir la fachada real. Se conserva
> por si se retoma. Lo mismo vale para §4–§6 (diseño del marcador) y para las bases y
> miniaturas de §7 y de `06_` §7.

#### Enfoque diorama

1. **Base MDF de 12 mm, 60 × 60 cm.**
2. **Vinilo mate laminado** pegado encima, con el marcador 2D.
3. **Miniatura de resina** (~15 cm) pegada al centro.

El tracking opera sobre el vinilo (lo que ARCore y ARKit soportan igual). La app superpone
un modelo digital más detallado y animado sobre la miniatura.

#### Por qué 60 × 60 cm

| Tamaño | Veredicto |
|---|---|
| 40 × 40 | A 1.5 m la imagen ocupa < 25 % del cuadro → tracking inestable |
| **60 × 60** | 25–30 % del cuadro a 1.5 m; cabe en una mochila; estable de 30 cm a 1.5 m |
| 80 × 80 | No entra en una mesa de aula estándar |

> ⚠️ Los porcentajes de ocupación del cuadro son cálculos de mayo, no mediciones. Lo medido
> es: **20 cm funciona en el A52s** a distancia de mano.

#### Montaje para la defensa (con bases)

Mesa despejada de al menos 1.5 × 1.0 m · 3 bases en línea, separadas 10–15 cm · ≥ 300 lux,
sin contraluz de ventana · operador a 30 cm – 1.5 m del marcador. *Con fotos impresas siguen
valiendo la luz ≥ 300 lux y evitar el contraluz.*

---

## 3. `XRReferenceImageLibrary`

### Reglas — 📐 para las fotos de fachada

| Parámetro | Valor |
|---|---|
| Resolución dentro de Unity | Del orden de 1024 px de lado; ver la recomendación vigente de ARCore al cargarlas |
| Formato | PNG (o JPEG sin recompresión). Texture Importer: `Compression = None` |
| **`Specify Size`** | **Obligatorio**, en metros: ancho real de la fachada en la librería de campo, ancho impreso en la de demo (§2.3) |
| Score `arcoreimg` | **≥ 85** |
| Nombres de archivo | ASCII puro: sin tildes, espacios ni Unicode |
| Nombre de la imagen en la librería | **Igual al `AnchorImageName`** del `MonumentDataSO` (`templo_san_jose`, …) |
| Nombre del asset de librería | `SenderoTrackingLibrary.asset`. Prohibido el prefijo `Sprint*_` |

### Dónde vive la fuente — ✅ regla vigente

`Assets/_SenderoAR/Art/` está fuera de Git **excepto `Art/Tracking/`**. La librería guarda
el GUID de la textura, no los píxeles: si la imagen no está versionada, un clone limpio
compila una app que no reconoce nada. Ese error costó el Sprint 1. El validador de CI lo
comprueba.

### Nombres de archivo previstos

El archivo se llama como el `AnchorImageName` del monumento, que es también el nombre de
la imagen dentro de las dos librerías:

```
templo_san_jose.png
capilla_mortuoria.png
torre_campanario.png
la_boveda.png
pila_bautismal.png
```

Si hace falta más de una foto por fachada (por horario, tarea C5), se agrega un sufijo:
`templo_san_jose__manana.png`, `templo_san_jose__tarde.png`.

### Validación con `arcoreimg`

```bash
# Linux/macOS (referencia)
./arcoreimg eval-img --input_image_path=templo_san_jose.png

# Windows
arcoreimg.exe eval-img --input_image_path=templo_san_jose.png
```

**Sesgo de Windows:** el binario de Windows da puntajes 20–35 puntos más bajos. Caso
observado en S1: la imagen de San Javier dio **65 en Windows y trackeó bien en el A52s**.
Para dar un score por bueno de forma oficial, correrlo en Linux/macOS (WSL sirve).

Exportar a PNG en escala de grises para iterar: es lo que "ve" el algoritmo.

**Nunca** dejar `arcoreimg.exe` en la raíz del repo.

---

## 4. Diseño del marcador 2D — 🗄️ fuera del MVP

> Era para el marcador diseñado del enfoque diorama (§2.4). Con fotos de fachada no se
> diseña nada: el principio de abajo (gradientes de alto contraste repartidos, sin simetría)
> sirve igual para **elegir y recortar** la foto de referencia.

### Principio

ARCore pasa la imagen a luminancia y busca esquinas (FAST/Harris). Un buen marcador
maximiza gradientes de alto contraste repartidos por **toda** la superficie, evita la
simetría rotacional y garantiza features en cualquier sub-región, porque la miniatura tapa
el centro.

### Composición concéntrica (60 × 60 cm)

```
+-------------------------------------------------------+
|  [A] ANILLO EXTERIOR (55 % del área)                  |
|      Textura lítica de alta frecuencia (ruido fractal)|
|      + bloque tipográfico asimétrico                  |
|     +-------------------------------------------+     |
|     |  [B] ANILLO MEDIO (30 %)                  |     |
|     |      Ilustración técnica vectorial        |     |
|     |        +---------------------+            |     |
|     |        | [C] NÚCLEO 15×15 cm |            |     |
|     |        | Lo tapa la miniatura|            |     |
|     |        | Sin keypoints.      |            |     |
|     |        | Solo cruz de guía.  |            |     |
|     |        +---------------------+            |     |
|     +-------------------------------------------+     |
|                                 +--------------+      |
|                                 | PILA         |      |
|                                 | BAUTISMAL    |      |
|                                 +--------------+      |
+-------------------------------------------------------+
```

Con el teléfono en ángulo oblicuo, la miniatura tapa ~10–15 % del vinilo: los keypoints
críticos van en el anillo exterior.

### Capas (Photoshop / Affinity / GIMP)

1. **Fondo opaco** `#2B231F`.
2. **Macro-frecuencia**: ondulaciones suaves en `#8C6D46` (pistas de perspectiva a distancia).
3. **Ilustración técnica**: vectores `#BA5D38`, luces `#D1C7B4`, trazos ≥ 8–12 px a
   resolución final.
4. **Ruido fractal lítico (crítico)**: Perlin/Simplex umbralizado, manchas de 2–5 mm en el
   vinilo final, sobre toda la superficie.
5. **Bloque tipográfico asimétrico**: sans-serif geométrica pesada, titular de 4.5–5.0 cm,
   en una esquina.
6. **Máscara de oclusión** (no se imprime): baja el contraste del cuadrado central de 15 cm
   para que `arcoreimg` reparta descriptores hacia la periferia.

### Paleta anti-reflejo

| Elemento | HEX | Luminancia Y | ΔY |
|---|---|---|---|
| Fondo "Granadillo" | `#2B231F` | 36 | — |
| Ilustración "Terracota" | `#BA5D38` | 100 | +64 vs fondo |
| Ruido "Ocre" | `#8C6D46` | 112 | +76 vs fondo |
| Tipografía "Caliza" | `#D1C7B4` | 201 | +101 vs vectores |

**Reglas**: nunca blanco puro `#FFFFFF` (*bloom* en el sensor) · nunca negro absoluto
(Y < 20, refleja como espejo bajo fluorescentes) · ΔY ≥ 60 entre elementos vecinos.

### Tipografía

Montserrat Bold, Inter Black o Roboto ExtraBold. **Nada con serifa**: los remates finos
generan aliasing a 1.5 m. Titular 4.5–5.0 cm, subtítulo 2.5–3.0 cm, margen 4.5 cm desde el
borde del MDF.

---

## 5. Diferenciar los 5 marcadores — 🗄️ fuera del MVP

> Con fotos reales la diferencia la ponen las fachadas mismas. Lo que sigue valiendo: si dos
> fachadas comparten la misma piedra y el mismo patrón, ARCore puede confundirlas ⚠️. Se
> comprueba apuntando a cada foto impresa con las otras cuatro en la librería (D7).

Si comparten ruido y paleta, ARCore puede confundirlos (el modelo del Templo aparece sobre
la base de la Pila). Tres medidas combinadas:

1. **Macro-geometría distinta**: Templo → horizontales en perspectiva · Torre → verticales
   severas · Pila → círculos concéntricos · Capilla → óculo + friso curvo · Bóveda →
   arcadas repetidas + "1754".
2. **Corrimiento tonal**: Pila en Terracota (Y≈100) · Templo en gris sillar (Y≈130) · Torre
   en madera oscura (Y≈75) · Capilla y Bóveda intermedios.
3. **Posición del bloque tipográfico**: Templo abajo-derecha · Capilla arriba-izquierda ·
   Torre vertical en el borde izquierdo · Bóveda en el borde superior · Pila abajo-izquierda.

---

## 6. Si el score queda bajo 85

> Con una foto de fachada, lo primero es **otra foto o otro recorte** (más textura, menos
> cielo y menos suelo liso). Los pasos 1 y 3 eran para el marcador diseñado; el 2 (CLAHE)
> también sirve para fotos.

1. **Ruido fractal**: reescalar el patrón un 30 % hacia abajo (duplica la densidad de cruces).
2. **Ecualización local (CLAHE)**: Camera Raw → Claridad +20 %, o GIMP → Realce → CLAHE 20 %.
3. **Eliminar zonas planas**: aislar áreas lisas de la silueta y aplicar un mapa de textura
   en *multiply* al 40–50 %.

---

## 7. Impresión — 🟡 aplica en parte

> Para las fotos impresas de la demo (§2.2) valen: **acabado mate, nada brillante ni
> satinado** y no recomprimir en JPEG. El vinilo, el MDF y el laminado eran del enfoque
> diorama (🗄️).

| Parámetro | Valor |
|---|---|
| Archivo | 7323 × 7323 px, 1.0–1.5 cm de sangría para forrar el MDF |
| Formato máster | **PDF/X-4** (o TIFF LZW). **Nunca JPEG**: los bloques DCT confunden al detector de esquinas |
| Color | CMYK (FOGRA39 o US Web Coated SWOP v2) |
| Vinilo | Calandrado denso, adhesivo **acrílico permanente** (el borde del MDF absorbe) |
| Laminado | Mate, **0–10 GU** a 60°. **Prohibido** brillante, satinado o UV parcial |
| Curado | 24–48 h de desgasificado de tintas ecosolventes antes de laminar |

Costos estimados de mayo en Santa Cruz ⚠️: 130–170 Bs por marcador terminado; bases MDF
50–75 Bs c/u.

---

## 8. Límites por plataforma

| Métrica | ARCore | ARKit |
|---|---|---|
| Imágenes simultáneas | 20 | 100 |
| Tolerancia de escala | Estricta | ~10–15 % |
| Múltiples instancias de la misma imagen | No | Sí |

`Specify Size` tiene que ser **exacto** para Android.

### `requestedMaxNumberOfMovingImages = 1` — ✅ en el repo

```csharp
// AR Foundation 5.x — lo que usa ImageTrackingController
_manager.requestedMaxNumberOfMovingImages = 1;
```

> 🔁 El documento original indicaba `maxNumberOfMovingImages` como "API legacy de 5.2". El
> compilador la marcó **obsoleta** en el commit #29 de S1. La propiedad correcta en 5.x es
> `requestedMaxNumberOfMovingImages`.

Motivo: con 3 marcadores en la mesa, ARCore solo recalcula la pose del que se está mirando;
los demás quedan como anclas estáticas.

---

## 9. Plan B — 📐 diseño

Si la luz rompe el image tracking: **tap-to-place sobre un `ARPlane`**. Hoy `ARPlaneManager`
y `ARRaycastManager` están **desactivados** en `_Main` (consumen batería y calientan el
equipo); el fallback los activaría solo cuando haga falta.

```csharp
// Pseudocódigo del encadenamiento
if (await TryImageTracking(monument, timeoutSeconds: 5)) return true;
ShowHint("Toca la base para colocar el monumento");
return await TryTapToPlace(monument);   // activa ARPlaneManager recién acá
```

---

## 10. Estados de tracking

### 10.1. Enum — ✅ en el repo

`Assets/_SenderoAR/Models/MonumentTrackingPhase.cs`:

| Valor | Equivale a | Comportamiento esperado |
|---|---|---|
| `Hidden = 0` | `TrackingState.None` | Ocultar |
| `Dimmed = 1` | `TrackingState.Limited` | Pose inferida por VIO sin ver la imagen. Mostrar atenuado; **despawn a los 5 s** |
| `Active = 2` | `TrackingState.Tracking` | Mostrar normal |

> 🔁 El documento original llamaba al enum `MonumentTrackingState` con `Limited`. El código
> usa `MonumentTrackingPhase` con `Dimmed`.

### 10.2. Controlador actual — ✅ en el repo (provisional, pre-MVVM)

`Core/AR/ImageTrackingController.cs`:

- Se suscribe a `ARTrackedImageManager.trackedImagesChanged` y recorre `added`, `updated` y
  `removed` de `ARTrackedImagesChangedEventArgs`.
- Crea un cubo de debug por `TrackableId`, hijo del `ARTrackedImage`.
- Color por estado: **cian** = `Tracking`, **ámbar** = `Limited`, oculto = `None`.
- Material inyectado por `[SerializeField]` (`DebugCubeMat.mat`, URP/Lit), asignado con
  `sharedMaterial`. Color con **`MaterialPropertyBlock`** sobre `_BaseColor`: no instancia
  materiales, así que no hay fuga al aparecer y desaparecer marcadores.

Se reemplaza por `View → MonumentTrackingViewModel → IARTrackingService` cuando exista MVVM.

### 10.3. Timeout de VIO — ✅ en el repo (solo observa)

**Problema**: al sacar el marcador, ARCore sigue estimando la pose con giroscopio e IMU
(odometría visual-inercial) y deja la imagen en `Limited` varios segundos: el contenido
queda "flotando" donde ya no hay nada.

`Core/AR/TrackingLimitedTimeoutWatcher.cs` lleva el estado **por `TrackableId`** y avisa en
el log si una imagen pasa más de `_limitedTimeoutSeconds` (5 s) en `Limited`. **No
despawnea**: mutar la escena es responsabilidad de la implementación de
`IARTrackingService`.

### 10.4. Contrato del servicio — ✅ contrato, 📐 implementación

`Core/Contracts/IARTrackingService.cs`:

```csharp
event Action<TrackedMonumentEvent> MonumentTrackingChanged;
Task<bool> InitializeAsync(CancellationToken cancellationToken = default);
Task StopAsync();
bool IsTracking { get; }
```

`TrackedMonumentEvent` es un `readonly struct` porque se emite muchas veces por segundo.

---

## 11. Presupuesto de rendimiento (Samsung A52s)

| Métrica | Umbral | Estado |
|---|---|---|
| `Application.targetFrameRate` | 30 fijo, `vSyncCount = 0` | ✅ |
| `ARPlaneManager` | Desactivado salvo fallback | ✅ |
| `requestedMaxNumberOfMovingImages` | 1 | ✅ |
| Draw calls | ≤ 100 | 📐 |
| Triángulos | ≤ 150 000 escena · ≤ 30 000 por monumento | 📐 |
| Materiales | 1 por malla, sin transparencia alfa | 📐 |
| Texturas | ASTC 6×6, máx. 2048 | 📐 |
| Luces | 1 Directional, sin sombras | ✅ (URP-Performant) |

### Temperatura — 📐

Santa Cruz llega a 33–34 °C en oct–nov. Previsto ⚠️: throttling del A52s a los ~10–15 min de
sesión continua. Medidas: pausar `ARSession` al perder el foco · reducir trabajo si
`batteryLevel < 0.2` sin cargador · sugerir pausa a los 20 min · tener un segundo
dispositivo listo.

---

## 12. Probar sin dispositivo

- **XR Simulation** (Project Settings → XR Plug-in Management → Standalone): un `Quad` con
  `Simulated Tracked Image`, escalado exactamente al `Specify Size` de la librería.
- **AR Foundation Remote 2.0** (asset de pago): manda la cámara del teléfono al Editor; sirve
  para probar con la luz real sin compilar un APK cada vez.

Ninguno reemplaza la condición (c) de la Definition of Done: **dispositivo físico**.

---

## 13. Checklist antes de un build

- [ ] `python Tools/validate_project.py` → 0 errores
- [ ] Cada imagen de la librería tiene su PNG versionado en `Art/Tracking/`
- [ ] Cada imagen tiene `Specify Size` correcto en cada librería: ancho real (campo) y ancho impreso (demo)
- [ ] Cada `AnchorImageName` existe en la librería (sin avisos del validador)
- [ ] Score `arcoreimg` ≥ 85 en Linux/macOS para cada marcador real
- [ ] `requestedMaxNumberOfMovingImages = 1`
- [ ] `ARPlaneManager` desactivado (salvo fallback)
- [ ] `AR Background Renderer Feature` presente en el Renderer de URP
- [ ] `ARCameraBackground` en la cámara de `_Main`
- [ ] Ningún `Shader.Find()` en código que cree objetos en runtime
- [ ] Modelos ≤ 30 000 tris, ASTC 6×6, sin alfa, 1 luz
- [ ] Pivote ajustado con `PivotOffset`
- [ ] Probado en el A52s a 30 cm, 50 cm, 1 m y 1.5 m
