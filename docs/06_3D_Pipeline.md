# 06 · Modelos 3D y miniaturas físicas

> Destilado de `06_3D_Pipeline.md` v2.0 (21/05/2026). Contrastado contra el repo el
> 16/10/2026. Leyenda de marcas: ver [`README.md`](README.md).
>
> **Todo es 📐.** Hoy el único contenido 3D es el cubo de debug. Los FBX van fuera de Git
> (regla 7 de `CLAUDE.md`), en `Assets/_SenderoAR/Art/Models3D/`.

---

## 1. Decisiones

| Componente | Decisión | Motivo |
|---|---|---|
| Captura | **Fotogrametría mixta**: fotos propias a altura humana + fotos web para lo alto | Se descartó el mástil telescópico: riesgo de que no lo permitan en un sitio UNESCO |
| Reconstrucción | **RealityCapture** (o Agisoft Metashape Edu como respaldo) | RealityCapture es mucho más rápido en GPU NVIDIA; Metashape tolera mejor el poco solapamiento de interiores oscuros |
| Optimización | **Blender 4.2 LTS** | Retopología y bake |
| Export a Unity | **FBX**, Y-up, escala 1.0 | |
| Export a impresión | **3MF** (preferido) o STL | 3MF lleva unidades |
| Texturas | **ASTC 6×6** | 3.56 bpp, Android e iOS |
| Polígonos | ≤ 30 000 tris por monumento | Snapdragon 778G / Adreno 642L |
| Peso | ≤ 5 MB por monumento | |
| Miniaturas | **3** (Templo, Torre, Pila), resina MSLA | "Camino A": 5 en código, 3 físicas |

> El viaje de captura estaba planeado para el ~21/06/2026. `CLAUDE.md` no registra que se
> haya hecho. Mientras tanto, las fuentes disponibles son fotos web con licencia
> (Wikimedia Commons, Flickr CC) — **anotar autor y licencia de cada foto usada**.

---

## 2. Captura

### Fuentes

1. **Visita en sitio** (2 días): fachadas a altura humana de Templo, Torre, Capilla y
   Bóveda; la Pila en 360° + vista cenital del cuenco.
2. **Fotos web** con licencia compatible: techos, cumbreras, linterna de la torre.
3. RealityCapture alinea cámaras distintas si hay > 60 % de solapamiento.

### Permisos (pedir ~15 días antes)

| Permiso | Para qué |
|---|---|
| Carta de la universidad (membretada, firmada por el tutor) | Acreditación |
| Párroco | Fotografiar la Pila dentro del templo |
| Plan Misiones | Aviso institucional |
| Alcaldía | Cortesía |

### Cámara

```
Mirrorless/DSLR:  Manual · RAW · f/8–f/11 · ≥1/160 s exterior, ≥1/60 s interior con monopie
                  ISO 100 (hasta 800–1600 en interior) · balance fijo 5500 K · AF-S punto fijo
Smartphone:       Lightroom Mobile (Pro) o Halide · DNG · ISO mínimo · ≥1/200 s
                  solo lente principal (nada de gran angular ni tele)
```

### Por monumento

| Monumento | Fotos | Patrón |
|---|---|---|
| Templo (fachada) | 350–500 | Serpentina en 3 alturas, 80 % solape frontal / 60 % lateral, a 5–7 m. Primera luz de la mañana (luz rasante) |
| Torre | 250–400 | 3 órbitas a distintas alturas + 1 órbita lejana (20–30 m, 70–105 mm) para aplanar el paralaje y alcanzar la cubierta |
| Pila (interior) | 150–250 | Domo: desde abajo, a la altura del cuenco y **cenital** (sin la cenital el cuenco queda como agujero). Luz LED rasante a 45–60° |
| Capilla / Bóveda | 200–300 c/u | Solo en código |

---

## 3. Procesamiento

### Ingesta
Respaldo inmediato del material crudo · descartar fotos con peatones o sombras duras ·
**delighting** (Shadow Recovery en Camera Raw/Lightroom): Unity calcula la luz en runtime, no
se quieren sombras horneadas en la textura.

### RealityCapture
```
Alignment: max features/imagen 40 000 · max features/Mpx 10 000 · overlap HIGH
Align Images → revisar nube rala → recortar con bounding box
Reconstruction: High Detail · textura 8192 → malla de 5–20 M tris
Texturing → Export FBX + PNG 8192, Y-up
```

### Blender 4.2
```
1. Importar el high-poly
2. Retopología: Quad Remesher (~15 000 quads) o QuadriFlow (gratis, más manual)
3. Shrinkwrap del low-poly al high-poly
4. UV: Smart UV Project, margen de islas 8 px
5. Bake en Cycles (Selected to Active, extrusión 0.05 m): Normal · Albedo · AO
6. AO empaquetado en el canal alfa del albedo
7. Export FBX: Y-up, escala 1.0, Apply Modifiers
```

### La Pila: doble salida
- **Unity**: low-poly ~12 000 tris, albedo 1024², normal 512².
- **Impresión**: Solidify 2–2.5 mm (vaciado) · booleanas para 2–3 drenajes de 2.5 mm en la
  base · validar manifold · STL en mm (se reescala a ~15 cm en el slicer).

---

## 4. Importación en Unity 2022.3

```
Model:      Scale Factor 1.0 · Mesh Compression HIGH · Read/Write OFF (ahorra RAM)
            Optimize Mesh ON · Lightmap UVs OFF · BlendShapes/Cameras/Lights OFF
Rig:        Animation Type None
Materials:  External Materials (control manual)

Albedo:     Default · sRGB ON · Mipmaps ON · Max 2048 · ASTC 6×6 (Android e iOS)
Normal:     Normal Map · sRGB OFF · Mipmaps ON · Max 2048 · ASTC 6×6
```

Peso estimado por monumento ⚠️ (cálculo, no medición): geometría ~0.8 MB + dos texturas
2048² ASTC 6×6 ~3.5 MB ≈ **4.3 MB**.

---

## 5. Materiales y shaders

| Decisión | Valor |
|---|---|
| Shader base | **URP Simple Lit** (no Lit/PBR: bastante más barato en Adreno) |
| Agua de la Pila | Shader Graph con desplazamiento de vértices |
| Cruces del Templo | URP Unlit, mezcla aditiva |
| Texturas | Una por monumento, **sin atlas**: se muestra un monumento a la vez |
| Batching | **SRP Batcher ON** · Static OFF (la corrección de deriva de AR lo invalida) · Dynamic OFF |
| LOD Groups | **No**: la cámara está siempre a 0.3–1.5 m y el *popping* se nota |
| Post-proceso, SSAO, sombras | OFF · MSAA 2x |

### SRP Batcher y `MaterialPropertyBlock`

Todo shader propio declara sus propiedades dentro de `CBUFFER_START(UnityPerMaterial)`.

> 🔁 El documento original decía "NO usar `MaterialPropertyBlock` — corrompe el SRP
> Batcher". Más preciso: un renderer con property block **sale del SRP Batcher** y se dibuja
> por el camino normal; no rompe nada más. El cubo de debug **sí** usa un property block, y
> está bien: es un solo objeto y evita filtrar un `Material` por cubo. Para los
> **modelos de monumentos**, no usar property blocks: un material por monumento.

---

## 6. Animaciones

| Monumento | Global | Contextual | Implementación |
|---|---|---|---|
| Templo | Rotación 0.5 RPM (3°/s) | Cruces translúcidas con fechas | Shader Graph aditivo |
| Capilla | 0.5 RPM | Luz tenue interior | Shader Graph |
| Torre | 0.7 RPM | Campanas pendulares ±15° a 0.5 Hz | C# sobre `Transform.localRotation`, sin Mecanim |
| Bóveda | 0.5 RPM | Luz cálida | Unlit aditivo |
| Pila | 0.5 RPM | Agua ondulante | Shader Graph: ruido Perlin + senoidales en vértices, sin partículas |

### Texto flotante orbital (obligatorio por el perfil)

- **`TextMeshPro` 3D**, no `TextMeshProUGUI` en un Canvas World Space (el `Rebuild()` del
  canvas cuesta frames).
- Fuente sans-serif pesada. Tamaño ~0.02 unidades. Radio = 1.5× el bounding box.
- **Billboard** hacia la cámara, sin inclinación vertical.
- El texto se asigna **una sola vez** al enlazar (cada cambio dispara un rebuild de malla).

```csharp
// Referencia — la Vista. Los parámetros salen del ViewModel.
public sealed class MonumentView : MonoBehaviour
{
    [SerializeField] private TextMeshPro _orbitalText;
    [SerializeField] private Transform _geometryPivot;

    private Transform _camera, _textTransform;
    private float _speedDegPerSec, _radius, _angleRad;

    public void Bind(string title, float speedDegPerSec, float radius, Transform camera)
    {
        _orbitalText.text = title;          // una vez
        _speedDegPerSec = speedDegPerSec;
        _radius = radius;
        _camera = camera;
        _textTransform = _orbitalText.transform;
        enabled = true;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        _geometryPivot.Rotate(Vector3.up, _speedDegPerSec * dt, Space.Self);

        _angleRad = Mathf.Repeat(_angleRad + _speedDegPerSec * dt * Mathf.Deg2Rad, Mathf.PI * 2f);
        var p = _textTransform.localPosition;
        p.x = Mathf.Cos(_angleRad) * _radius;
        p.z = Mathf.Sin(_angleRad) * _radius;
        _textTransform.localPosition = p;

        var dir = _textTransform.position - _camera.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            _textTransform.rotation = Quaternion.LookRotation(dir);
    }
}
```

> 🔁 El ejemplo original fijaba `Application.targetFrameRate = 30` dentro de la vista. Eso ya
> lo hace `AppBootstrapper`; no repetirlo en cada componente.

---

## 7. Miniaturas físicas (MSLA)

### Proveedores ⚠️ (relevados en mayo; contactos en los documentos originales, fuera del repo)

| Proveedor | Servicio |
|---|---|
| 3D Lab Bolivia | Impresión MSLA (Anycubic Photon M7 Pro 14K), envío a Santa Cruz |
| Reality HC (Santa Cruz) | Impresión en resina, respaldo |
| Blue Laser (Santa Cruz) | Corte de bases MDF |

### Piezas

| Pieza | Tamaño | Impresión | Costo estimado ⚠️ |
|---|---|---|---|
| Templo (fachada simplificada) | ~15 × 12 × 8 cm | ~12 h | 350–400 Bs |
| Torre | ~18 cm de alto | ~10 h | 320–380 Bs |
| Pila | ~12 cm de alto | ~8 h | 280–350 Bs |

### Parámetros

```
XY 24 µm · Z 50 µm · exposición 2.5–3.0 s · 4–6 capas base a 25–30 s
Resina gris tipo ABS · pared 2–2.5 mm · 2–3 drenajes de 2.5 mm
Soportes tipo árbol ligero, contacto 0.3 mm, densidad 50 %
```

Post-proceso: lavado en IPA 99 % (5 min) → curado UV 5–10 min → quitar soportes → lijado
P400 → P800 → **imprimación gris mate** (anti-reflejo) → pintura opcional.

Bases: MDF 12 mm crudo, 60 × 60 cm, × 3. El vinilo va encima
([`03_`](03_AR_Tracking_Specs.md) §7).

---

## 8. Checklist por monumento

- [ ] FBX importado sin errores en consola
- [ ] ≤ 30 000 tris (ventana Stats)
- [ ] Read/Write OFF · Mesh Compression HIGH
- [ ] Texturas ASTC 6×6
- [ ] < 5 MB tras el build
- [ ] Pivote ajustado con `MonumentDataSO.PivotOffset`
- [ ] URP Simple Lit (sin Standard, Lit ni alfa)
- [ ] Shaders propios con `CBUFFER_START(UnityPerMaterial)`
- [ ] Probado en el A52s a 30 cm, 50 cm, 1 m y 1.5 m
- [ ] Texto orbital legible a todas esas distancias
- [ ] ≥ 28 FPS sostenidos tras 5 minutos de sesión
- [ ] Autor y licencia anotados para cada foto web usada

---

## 9. Riesgos

| Riesgo | Mitigación |
|---|---|
| RealityCapture no alinea el interior de la Pila | Más fotos del cuenco con luz rasante; Metashape de respaldo |
| Modelo > 5 MB | Bajar texturas a 1024 |
| Lluvia el día de la visita | Pasar a interiores (Pila) y reacomodar bloques |
| Niegan acceso al interior | Carta de la universidad enviada antes + mediación del Espacio Cultural Chiquitos |
| Miniatura con defectos | Segunda copia en el proveedor de respaldo |
| Sin visita en sitio | Modelo solo con fotos web: declarar esa limitación en la tesis |
