# 01 · Los 5 monumentos

> Destilado de `01_Monuments_Database.md` v2.0 (21/05/2026). Contrastado contra los `.asset`
> de `Assets/_SenderoAR/Models/Monuments/` el 16/10/2026.
> Leyenda de marcas: ver [`README.md`](README.md).

---

## ⚠️ Antes de usar cualquier dato histórico de este documento

Los datos históricos (fechas, nombres, cifras) salieron de los reportes *Deep Research* de
mayo de 2026, generados con Gemini, **sin fuente primaria citada**. Ya están copiados en las
descripciones ES de los 5 `.asset` y en el system prompt del chatbot
([`02_AI_Architecture.md`](02_AI_Architecture.md) §3).

Por la regla 4 de `CLAUDE.md`, **ninguno se cita en la tesis ni se le enseña al chatbot como
hecho** hasta tener fuente (Plan Misiones, bibliografía académica, inscripción fotografiada
en el lugar) con autor y fecha.

### 🔴 El dato más urgente de verificar: la Pila Bautismal "c. 1652"

`mon_05_pila` se eligió en mayo justamente por su "anomalía cronológica": una pila fechada
c. 1652, casi un siglo anterior al templo. El problema:

- Según fuentes generales, las misiones jesuíticas de Chiquitos empiezan en 1691 (San Javier)
  y San José se funda hacia 1697. *(Estas dos fechas también hay que confirmarlas con fuente.)*
- Si eso es correcto, una pila tallada en 1652 sería **anterior a toda presencia jesuítica
  en la región**, no solo al templo.

Eso no la descarta (pudo traerse de otro sitio), pero la convierte en el dato con más riesgo
del proyecto: es el argumento central de un monumento entero, figura en `FoundationYear`
del `.asset`, en el texto orbital y en una de las preguntas preparadas para la defensa. Si
resulta falso, hay que reescribir `mon_05` antes de H2.

---

## Resumen — ✅ en el repo

Valores leídos de los `.asset` el 16/10/2026.

| ID (`MonumentId`) | Nombre | `FoundationYear` | `GPSCoordinates` (lat, lon) | `AnchorImageName` | `IsPhysicalInDefense` |
|---|---|---|---|---|---|
| `mon_01_templo` | Templo de San José de Chiquitos | 1745 | -17.8483, -60.7425 | `templo_san_jose` | ✅ |
| `mon_02_capilla` | Capilla Mortuoria (Miserere) | 1750 | -17.8483, -60.7428 | `capilla_mortuoria` | — |
| `mon_03_torre` | Torre del Campanario | 1748 | -17.8481, -60.7422 | `torre_campanario` | ✅ |
| `mon_04_boveda` | La Bóveda (Colegio / Museo) | 1754 | -17.8481, -60.7419 | `la_boveda` | — |
| `mon_05_pila` | Pila Bautismal de Piedra | 1652 ⚠️ | -17.8483, -60.7425 (interior del templo) | `pila_bautismal` | ✅ |

- Los 5 tienen `PhysicalSize = 0.6 × 0.6` y `TrackingConfidenceThreshold = 0.4`.
- ⚠️ Las coordenadas GPS son aproximadas y sin fuente. Verificarlas antes de construir un mapa.
- 🔴 Ninguno de los 5 `AnchorImageName` existe todavía en `SenderoTrackingLibrary.asset`:
  son los 5 avisos que da `Tools/validate_project.py`. Ningún monumento se dispara hasta
  que se agreguen sus marcadores.
- Las descripciones ES están pobladas; EN y PT están vacías.

### Por qué `mon_05` es la Pila y no Santa Cruz la Vieja

La versión anterior usaba `mon_05_scv` (Parque Histórico Santa Cruz la Vieja). Se descartó
en mayo porque son ruinas planas, malas para tracking, y están lejos del conjunto. La Pila
quedó por: estar dentro del templo (no suma recorrido), ser un sólido de revolución fácil de
imprimir en resina y por la fecha de arriba. **Nunca volver a referirse a `mon_05_scv`.**

---

## Ficha por monumento

Las secciones "Datos históricos" son ⚠️ en su totalidad. Las de modelo 3D y animación son 📐.

### mon_01_templo — Templo de San José de Chiquitos

**Datos históricos ⚠️**
- Construcción hacia 1745–1760, dirigida por el P. Bartolomé de Mora (rector desde 1745).
- Mano de obra: "más de 5 000 indígenas" de la etnia Penoca.
- Restauración: Hans Roth (1988–2003).
- Singularidad declarada: único templo jesuítico de Chiquitos en piedra y cal (los demás en
  adobe y madera). Es también el argumento técnico para el tracking: la piedra laja da
  textura irregular con muchos keypoints.
- Planta basilical de tres naves, fachada de piedra laja ocre/rojiza con mortero de cal,
  portadas talladas con relieves geométricos y florales, patrón ajedrezado en pilares y
  friso, horcones interiores de madera de cuchi.

**Tracking 📐** — Anclaje 2D: portada principal tallada (ilustración técnica + textura
grunge). Distancia operativa 30 cm – 1.5 m.

**Modelo 3D 📐** — ~18 500 tris · albedo 2048² + normal 1024², ASTC 6×6 · URP Simple Lit.
Animación: rotación 0.5 RPM + cruces translúcidas flotantes con fechas (mezcla aditiva) +
texto orbital *"Templo de San José · 1745-1760"*.

### mon_02_capilla — Capilla Mortuoria (Miserere)

**Datos históricos ⚠️**
- Parte del complejo, c. 1750. Velatorio y ritos fúnebres.
- El nombre viene del salmo *Miserere mei, Deus*.
- Capilla anexa al templo: óculo central, friso decorativo continuo, bóveda de cañón,
  pinturas murales restauradas por Hans Roth.

**Tracking 📐** — Anclaje 2D: óculo + friso (alta densidad de features). Solo en código, sin
miniatura física.

**Modelo 3D 📐** — ~14 000 tris. Animación: rotación 0.5 RPM + luz tenue interior simulada
(Shader Graph) + texto orbital *"Capilla Mortuoria · c. 1750"*.

### mon_03_torre — Torre del Campanario

**Datos históricos ⚠️**
- c. 1748. Estructura exenta, separada del templo (rasgo chiquitano).
- Volumen piramidal trunco de tres cuerpos, base de piedra labrada, aberturas en arco,
  cubierta de teja con cruz de remate. Restaurada por Hans Roth.

**Tracking 📐** — Anclaje 2D: base + primer cuerpo ajedrezado. **Riesgo**: ambigüedad de pose
por la simetría vertical; se mitiga con un bloque de texto asimétrico en el marcador.

**Modelo 3D 📐** — ~30 000 tris (el máximo del presupuesto). Animación: rotación 0.7 RPM +
campanas pendulares ±15° a 0.5 Hz (C# directo sobre `Transform.localRotation`, sin
Mecanim) + texto orbital *"Torre del Campanario · c. 1748"*.

### mon_04_boveda — La Bóveda (Colegio Misional / Museo)

**Datos históricos ⚠️**
- Terminada en 1754 (inscripción en el portal). Residencia jesuita, colegio para hijos de
  caciques, talleres.
- Murales pintados por Gregorio Villarroel en 1810; Hans Roth rescató "más de 2 500 m²".
- Hoy Espacio Cultural Chiquitos, inaugurado en 2011.
- Una planta, muros muy gruesos, 6 salas + 2 corredores abovedados, doble galería de arcos.

**Tracking 📐** — Anclaje 2D: inscripción "1754" + arcadas. Solo en código.

**Modelo 3D 📐** — ~22 000 tris. Animación: rotación 0.5 RPM + iluminación cálida interior
+ texto orbital *"La Bóveda · 1754"*.

### mon_05_pila — Pila Bautismal de Piedra

**Datos históricos ⚠️** — ver el aviso 🔴 al inicio.
- Fecha "c. 1652" tallada en la pieza. Hipótesis de mayo: perteneció a una capilla anterior y
  se reincorporó al templo.
- ~1.2 m de alto, piedra calcárea local, motivos geométricos (lacerías, soles), cuenco
  cóncavo. En uso litúrgico.

**Tracking 📐** — Ilustración técnica de la pila + textura lítica. Diseño del marcador en
[`03_AR_Tracking_Specs.md`](03_AR_Tracking_Specs.md) §4 (el ejemplo trabajado es este).

**Modelo 3D 📐** — ~12 000 tris · albedo 1024² + normal 512². Animación: rotación 0.5 RPM +
plano de "agua bendita" con Shader Graph (desplazamiento de vértices con ruido Perlin +
senoidales, sin partículas) + texto orbital *"Pila Bautismal · 1652"*.

**Miniatura física 📐** — MSLA en resina, pared de 2–2.5 mm, 2–3 drenajes de 2.5 mm, ~8 h de
impresión. Detalle en [`06_3D_Pipeline.md`](06_3D_Pipeline.md) §10.

---

## Esquema de `MonumentDataSO` — ✅ en el repo

`Assets/_SenderoAR/Models/MonumentDataSO.cs`, `namespace SenderoAR.Models`:

| Sección (`[Header]`) | Campo | Tipo |
|---|---|---|
| Identity | `MonumentId`, `FoundationYear` | `string`, `int` |
| Geolocation | `GPSCoordinates` | `Vector2` (x = lat, y = lon) |
| AR Tracking | `AnchorImageName`, `PhysicalSize`, `TrackingConfidenceThreshold` | `string`, `Vector2` (m), `float` [0..1] |
| 3D Content (Sprint 6) | `Monument3DPrefab`, `PivotOffset` | `GameObject`, `Vector3` |
| Localization & Audio | `Description`, `NarrationClips` | `LocalizedDescription`, `AddressableAudioReference[]` |
| Defense Flags | `IsPhysicalInDefense` | `bool` |

`OnValidate()` (solo editor): copia el nombre del asset a `MonumentId` si está vacío, aplica
`Clamp01` al umbral y, si un eje de `PhysicalSize` es ≤ 0, lo pone en 0.60 m (no es un mínimo: 0.20 m se respeta). Cubierto por `Tests/EditMode/MonumentDataSOValidationTests`.

`MonumentCatalogSO` agrupa los 5 en orden (`SenderoMonumentCatalog.asset`) y expone
`TryGetById(string, out MonumentDataSO)`.

`AnchorImageName` **debe ser idéntico** al nombre de la imagen en la
`XRReferenceImageLibrary`; el validador de CI lo comprueba.

---

## Recorrido real

Los 5 están en el mismo conjunto. La pila está dentro del templo, así que no suma
desplazamiento. (La estimación de mayo de "~2.5 h de recorrido" no tiene fuente.)

```
   [mon_04_boveda] ───── [mon_03_torre]
         │                     │
   [mon_02_capilla] ──── [mon_01_templo]
                               │ (interior)
                         [mon_05_pila]
```
