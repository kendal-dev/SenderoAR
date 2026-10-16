# 09 · Lecciones de Unity, URP, AR Foundation y Git

> Destilado de las secciones de incidencias y aprendizajes de `PROJECT_PROGRESS.md` y
> `SESSION_HANDOFF.md` (S0–S2, mayo 2026) y de la auditoría del 04/09/2026.
> Cada fila costó tiempo real al menos una vez. **Consultar antes de depurar.**

---

## 1. Diagnóstico rápido por síntoma

| Síntoma | Causa más probable | Solución |
|---|---|---|
| **Pantalla negra** en el teléfono, pero el tracking "funciona" (logs) | Son **4 causas en paralelo**, revisarlas todas: (a) `ARTrackedImageManager.serializedLibrary` en None · (b) la cámara no tiene `ARCameraBackground` · (c) el Renderer de URP no tiene **`AR Background Renderer Feature`** · (d) GameObjects con script faltante | Revisar las cuatro, no parar en la primera |
| Objeto **magenta** brillante y FPS cayendo a ~8 | Shader faltante. En builds Android, `Shader.Find("Universal Render Pipeline/Lit")` devuelve null por **shader stripping**: Unity elimina shaders que ningún asset referencia | Inyectar un `Material` asset por `[SerializeField]` (ej. `DebugCubeMat.mat`). **Nunca `Shader.Find()` para objetos creados en runtime** |
| El color asignado no cambia nada | `renderer.material.color` escribe `_Color` (Built-in). URP/Lit usa **`_BaseColor`** | `SetColor("_BaseColor", c)`, idealmente vía `MaterialPropertyBlock` |
| La memoria sube cada vez que aparece/desaparece un marcador | `renderer.material` **instancia** un `Material` nuevo por renderer y no se libera al destruir el objeto | `sharedMaterial` + `MaterialPropertyBlock` (commit `db36f3f`) |
| El contenido queda "flotando" segundos después de sacar el marcador | ARCore sigue en `Limited` estimando la pose con giroscopio + IMU (VIO drift) | Timeout de 5 s en `Limited` ([`03_`](03_AR_Tracking_Specs.md) §10.3) |
| Con varios marcadores, el timeout se dispara con el monumento equivocado | Estado de tracking guardado globalmente | Estado **por `TrackableId`** (commit `db36f3f`) |
| `OnValidate()` no se ejecuta, sin error en consola | `#if UNITY_EDITOR` / `#endif` en la misma línea que una llave (pegado con CRLF raro) | Directivas **siempre en su propia línea** |
| Error de ambigüedad con `Debug.Log` dentro del proyecto | Namespace propio terminado en `.Debug` | Prohibido `Debug`, `Object`, `Random`, `Time` como último segmento de namespace |
| Un clone limpio compila pero **no reconoce ningún marcador** | La imagen de la librería no está en Git: la librería guarda el GUID, no los píxeles | Las fuentes de tracking van en `Art/Tracking/` (versionada). `validate_project.py` lo detecta |
| El teléfono se calienta y el FPS baja tras ~10 min | `ARPlaneManager` activo sin necesidad + calor ambiente | `ARPlaneManager` y `ARRaycastManager` desactivados en `_Main` (commit `cc0d8f9`) |
| `maxNumberOfMovingImages` marcada obsoleta | API vieja | En AR Foundation 5.x: **`requestedMaxNumberOfMovingImages`** |
| Tildes rotas (`Ã©`) en comentarios | Archivo guardado en Windows-1252 | Guardar como UTF-8. Si se usa un script de reemplazo, que sea idempotente y verificar después |

---

## 2. Ciclo de vida AR entre escenas

- **Una `ARSession` por escena AR**, nunca compartida con `DontDestroyOnLoad`.
- `_App_Boot` no tiene ningún componente AR físico: solo `AppBootstrapper`, que usa los
  métodos **estáticos** `ARSession.CheckAvailability()` y `ARSession.Install()`.
- `_Main` tiene su propia `ARSession`, `XR Origin` y `ARTrackedImageManager`.
- Build Settings: índice 0 = `_App_Boot`, índice 1 = `_Main`.

---

## 3. Manejo de assets

| Regla | Por qué |
|---|---|
| **Mover y renombrar assets solo dentro del Project window de Unity**, nunca con el explorador ni `mv` | Unity mantiene GUIDs y actualiza `EditorBuildSettings.asset`. Fuera de Unity se rompen las referencias |
| **Renombrar en Unity no cambia el `namespace`** dentro del `.cs` | Editar la línea `namespace` a mano |
| **`File > Save Project` antes de `git diff`** | Unity guarda cambios en memoria; sin esto Git no los ve |
| **Para convertir una imagen, re-exportarla** (Paint, ffmpeg, ImageMagick) | Cambiar la extensión `.jpeg → .png` no convierte nada y deja un archivo con cabecera inválida |
| Cada asset y carpeta nueva dentro de `Assets/` lleva su `.meta` en el mismo commit | Sin `.meta`, Unity genera otro GUID en cada clone. El validador lo detecta |
| Si Unity reescribe un `.cs` nuevo con la plantilla `MonoBehaviour` por defecto | `git restore --staged <archivo>`, reescribir y verificar en disco antes de volver a agregar |
| **El disco es la verdad**, no el editor | Verificar con `Get-Content` / `cat` lo que se va a commitear |
| Ante un comportamiento raro del watcher de assets | `Reimport` del asset |

---

## 4. Git en Windows

| Regla | Por qué |
|---|---|
| Respetar mayúsculas exactas en las rutas de `git add` | NTFS no distingue mayúsculas, Git sí: `MonumentS/` vs `Monuments/` deja archivos "fantasma" sin trackear |
| Nunca dejar binarios de herramientas en la raíz (`arcoreimg.exe`) | Terminan en un commit |
| Copiar a la terminal solo bloques de comandos, nunca salida | Pegar salida en PowerShell produce una cascada de `CommandNotFoundException` |
| `git diff` antes de cada `git add` | Detecta reescrituras automáticas de Unity |

### Equivalencias Bash → PowerShell

| Bash | PowerShell |
|---|---|
| `ls -la` | `Get-ChildItem -Force` |
| `cat f` | `Get-Content f` |
| `head -20` / `tail -20` | `Select-Object -First 20` / `-Last 20` |
| `rm f` | `Remove-Item f` |
| `mkdir -p d` | `New-Item -ItemType Directory -Force -Path d` |
| `grep "x" f` | `Select-String -Pattern "x" -Path f` |

---

## 5. Herramientas

```powershell
# Logs de Unity y AR desde el teléfono conectado
adb logcat -s Unity ARCore ARFoundation

# Score de un marcador (en Windows da 20–35 puntos menos)
.\arcoreimg.exe eval-img --input_image_path=<ruta.png>

# Integridad del repo, sin Unity
python Tools/validate_project.py

# Tests EditMode sin abrir el editor (salida 0 = verde; ver 04_MVVM_Patterns.md §2.2)
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Proyectos\SenderoAR" -runTests -testPlatform EditMode -testResults resultados.xml -logFile tests.log
```

Compilar sin abrir el editor: comando en `CLAUDE.md` §8.

---

## 6. Principios que salieron de estos errores

| Principio | Origen |
|---|---|
| **KISS**: la versión validada por Unity para tu LTS le gana a la más nueva | Pivote de AR Foundation 6.0.7 → 5.2.0 (19/05) |
| Reutilizar los assets de la plantilla antes de crear otros | `URP-Performant` |
| **Validar en el dispositivo físico desde el principio** | Primer APK en el A52s en Sprint 0 |
| Composition Root antes que funcionalidades; contratos antes que implementaciones | S0 |
| **El compilador es el auditor final**: si marca una API como obsoleta, el documento está desactualizado, no el código | `requestedMaxNumberOfMovingImages` |
| Una pantalla negra no es un solo bug | S1, 4 causas a la vez |
| Observar antes de mutar | `TrackingLimitedTimeoutWatcher` |
| Un dato que no se puede rastrear a una fuente no entra | Auditoría 04/09 · regla 4 de `CLAUDE.md` |
