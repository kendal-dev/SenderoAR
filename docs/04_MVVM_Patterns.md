# 04 · Arquitectura MVVM y convenciones de código

> Destilado de `04_MVVM_Patterns.md` v2.0 (21/05/2026) y de las decisiones de S0–S2.
> Contrastado contra el código el 16/10/2026.
> Leyenda de marcas: ver [`README.md`](README.md).

---

## 0. Qué existe hoy

| Pieza | Estado |
|---|---|
| 5 contratos en `Core/Contracts/` | ✅ Escritos y documentados. **Cero implementaciones** |
| Modelos, enums y `ScriptableObject`s en `Models/` | ✅ |
| Composition Root (`AppBootstrapper`) | ✅ Valida AR, fija 30 FPS y portrait, inicializa `AppContext`, carga `_Main` |
| `AppContext` | 🟡 **Solo una compuerta**: `IsReady`, `Initialize()`, `Reset()`, `EnsureReady()`. **No registra ni resuelve servicios** |
| `BaseViewModel`, `ReactiveProperty<T>`, `RelayCommand`, `CompositeDisposable` | 📐 No existen |
| ViewModels / Views | 📐 Carpetas vacías (solo `.gitkeep`) |
| Assembly definitions (`.asmdef`) | ✅ `SenderoAR.Models`, `SenderoAR.Core`, `SenderoAR.Tests.EditMode` (16/10/2026). Ver §2 |
| Tests | ✅ 29 tests EditMode en `Tests/EditMode/`, verdes en batchmode el 16/10/2026. Ver §2.2 |

---

## 1. Principios

1. **MVVM puro, sin frameworks**: nada de UniRx, Zenject, R3 ni MVVMLight.
2. **Los ViewModels son POCOs**, nunca `MonoBehaviour`. Así se prueban con NUnit sin escena.
3. **El ViewModel no conoce a la Vista.** Expone `IReadOnlyReactiveProperty<T>` e `ICommand`;
   la Vista se suscribe.
4. **Cero asignaciones de memoria por frame.** El binding usa delegados creados una vez,
   no reflexión.
5. **`[CallerMemberName]`** en `OnPropertyChanged()`: sin strings mágicos.
6. **Inyección de dependencias híbrida**: Service Locator (`AppContext`) en los puntos de
   entrada (`MonoBehaviour`s), inyección por constructor en todo lo demás.
7. **Toda suscripción se registra en un `CompositeDisposable`** y se libera en `Dispose()`.
8. **Contratos antes que implementaciones**, y cada contrato documenta su modelo de hilos
   en `<remarks>`.

---

## 2. Assembly definitions

### 2.1. Lo que existe — ✅ en el repo (16/10/2026)

Se eligió la **opción (a)** de la nota de §2.3 para no mover ningún archivo: un `.asmdef`
por carpeta existente, con referencias a UnityEngine. Los `.cs.meta` no se tocaron, así que
las escenas siguen encontrando sus scripts por GUID.

| Assembly | Carpeta | Referencias | Notas |
|---|---|---|---|
| `SenderoAR.Models` | `Models/` | (ninguna) | Con UnityEngine: `MonumentDataSO` y `MonumentCatalogSO` son `ScriptableObject` |
| `SenderoAR.Core` | `Core/` | `SenderoAR.Models`, `Unity.XR.ARFoundation`, `Unity.XR.ARSubsystems`, `Unity.XR.CoreUtils` | Contratos, AR, Composition Root, diagnósticos |
| `SenderoAR.Tests.EditMode` | `Tests/EditMode/` | `SenderoAR.Models`, `SenderoAR.Core`, Test Runner, `nunit.framework.dll` | Solo Editor, `defineConstraints: UNITY_INCLUDE_TESTS`, `autoReferenced: false` |

`Assets/Editor/` (los validadores de menú) sigue en `Assembly-CSharp-Editor`, que ve a
`SenderoAR.Models` y `SenderoAR.Core` porque ambos son `autoReferenced`.

**Al agregar código nuevo**: va dentro de una de esas carpetas (o de una nueva con su
propio `.asmdef`). Lo que quede fuera compila en `Assembly-CSharp`, y **ningún `.asmdef`
puede referenciar `Assembly-CSharp`**: los tests no lo verían.

### 2.2. Tests — ✅ en el repo

| Archivo | Qué cubre |
|---|---|
| `MonumentCatalogSOTests` | `TryGetById`: acierto, ID desconocido, `null`/vacío, catálogo vacío, slots `null`, mayúsculas |
| `MonumentDataSOValidationTests` | `OnValidate`: ID vacío toma el nombre del asset, umbral en [0..1], `PhysicalSize` ≤ 0 → 0.60 m, 0.20 m se respeta |
| `MonumentCatalogAssetTests` | Los datos **reales**: 5 monumentos en orden, ID = nombre del asset y único, datos de tracking en rango, descripción ES presente |
| `ModelContractTests` | Valores serializados estables de `LanguageCode` y `MonumentTrackingPhase`; validación del constructor de `MonumentSnapshot` |

Correrlos sin abrir el editor (~3 min; falla si el editor tiene el proyecto abierto):

```bash
"/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe" -batchmode -nographics \
  -projectPath "C:\Proyectos\SenderoAR" -runTests -testPlatform EditMode \
  -testResults resultados.xml -logFile tests.log
```

Código de salida 0 = todo verde. En el editor: *Window → General → Test Runner → EditMode*.
El CI de GitHub **no** los corre: necesita una licencia de Unity en los secrets
([`07_CICD_Pipeline.md`](07_CICD_Pipeline.md) §2).

### 2.3. Diseño a futuro — 📐

```
Assets/_SenderoAR/
├── Models/            App.Domain.asmdef               sin UnityEngine (ver nota)
├── Core/
│   ├── Infrastructure/ App.MVVM.asmdef                Reactive, Commands, Binding
│   ├── Services/       App.Services.FirebaseAI.asmdef aislado de Firebase
│   └── DemoMode/       App.DemoMode.asmdef            solo con DEMO_MODE
├── ViewModels/        App.ViewModels.asmdef
├── Views/             App.Views.asmdef                MonoBehaviours y UI
└── Tests/             App.Tests.asmdef                NUnit, solo Editor
```

> 🔁 **Choque con el código, resuelto con la opción (a).** `App.Domain` se diseñó con
> `noEngineReferences: true`, pero `Models/` contiene `ScriptableObject`s que usan
> `Vector2`, `Vector3` y `GameObject`. Las opciones eran:
> **(a)** assembly de modelos con referencias a UnityEngine — **elegida el 16/10/2026**
> (`SenderoAR.Models`), sin mover archivos; o
> **(b)** separar `Models/Data/` (POCOs puros) de `Models/Assets/` (`ScriptableObject`s).
> (b) sigue disponible si algún día hace falta probar el dominio fuera de Unity. Los nombres
> `App.*` de abajo son el plan de mayo; los assemblies nuevos siguen el prefijo
> `SenderoAR.*`, alineado con los namespaces.

Configuraciones previstas:

```jsonc
// App.Domain.asmdef
{ "name": "App.Domain", "references": [], "noEngineReferences": true }

// App.MVVM.asmdef
{ "name": "App.MVVM", "references": ["App.Domain"], "autoReferenced": true }

// App.Services.FirebaseAI.asmdef — el resto del proyecto no ve las DLL de Firebase
{
  "name": "App.Services.FirebaseAI",
  "references": ["App.Domain", "App.MVVM"],
  "overrideReferences": true,
  "precompiledReferences": ["Firebase.App.dll", "Firebase.AI.dll", "Firebase.AppCheck.dll"],
  "autoReferenced": false
}

// App.DemoMode.asmdef — solo se compila si DEMO_MODE está definido
{
  "name": "App.DemoMode",
  "references": ["App.Domain", "App.MVVM", "App.Services.FirebaseAI"],
  "defineConstraints": ["DEMO_MODE"],
  "autoReferenced": false
}

// App.Tests.asmdef
{
  "name": "App.Tests",
  "references": ["App.Domain", "App.MVVM", "App.ViewModels"],
  "optionalUnityReferences": ["TestAssemblies"],
  "includePlatforms": ["Editor"],
  "overrideReferences": true,
  "precompiledReferences": ["nunit.framework.dll"],
  "autoReferenced": false
}
```

Cada capa nueva (MVVM, Services, ViewModels, Views, DemoMode) entra con su propio
`.asmdef` en el mismo commit que su primer archivo, y se verifica corriendo los tests de
§2.2.

---

## 3. Composition Root y Service Locator

### Hoy — ✅ en el repo

- `_App_Boot.unity` (índice 0) contiene solo `AppBootstrapper`, con
  `[DefaultExecutionOrder(-1000)]` y `DontDestroyOnLoad`.
- `Start()` es una corrutina: `ARSession.CheckAvailability()` → `ARSession.Install()` si
  hace falta → aborta si `Unsupported` → `AppContext.Initialize()` →
  `SceneManager.LoadSceneAsync(SceneNames.Main)`.
- `_Main.unity` (índice 1) tiene su **propia** `ARSession`, `XR Origin` y
  `ARTrackedImageManager`.

### Pendiente — 📐 diseño

> 🔁 `PROJECT_PROGRESS.md` daba por "restaurado" un Service Locator completo en
> `AppContext`. No está en el código. La API prevista es:

```csharp
public static class AppContext
{
    public static bool IsReady { get; }
    public static void Initialize();
    public static void Reset();                 // para tests

    public static void Register<T>(T service) where T : class;   // falla si ya existe
    public static T Resolve<T>() where T : class;                // pasa por EnsureReady
    public static bool TryResolve<T>(out T service) where T : class;
}
```

Los servicios se construyen y se registran **solo en `AppBootstrapper`**. Si un servicio se
decora (modo demo), la decoración también ocurre ahí:

```csharp
// En AppBootstrapper, antes de AppContext.Initialize()
IChatbotClient chatbot = new ChiquitaniaHeritageChatService(/* deps */);
#if DEMO_MODE
chatbot = new DemoCachedChatbotDecorator(chatbot, new LevenshteinQACache(), new NetworkDetector());
#endif
AppContext.Register<IChatbotClient>(chatbot);
```

Los ViewModels nunca saben si hablan con el servicio real o con un decorador.

---

## 4. Infraestructura reactiva — 📐 implementación de referencia

```csharp
public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected readonly CompositeDisposable Disposables = new CompositeDisposable();

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public virtual void Dispose() => Disposables.Dispose();
}
```

```csharp
// Semántica de BehaviorSubject: emite el valor actual al suscribirse.
public class ReactiveProperty<T> : IReadOnlyReactiveProperty<T>
{
    private T _value;
    private Action<T> _onValueChanged;

    public T Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value)) return;
            _value = value;
            _onValueChanged?.Invoke(_value);
        }
    }

    public IDisposable Subscribe(Action<T> observer)
    {
        _onValueChanged += observer;
        observer(_value);
        return new Subscription(this, observer);   // al Dispose: _onValueChanged -= observer
    }
}
```

`RelayCommand` (con `CanExecute` y `CanExecuteChanged`) y `CompositeDisposable` siguen el
mismo estilo. Todo esto va en `Core/Infrastructure/`.

---

## 5. Binding manual sin reflexión — 📐

```csharp
public static class UnityUIBindingExtensions
{
    public static IDisposable BindTo(this IReadOnlyReactiveProperty<string> property, TMP_Text text)
        => property.Subscribe(value => text.text = value);

    public static IDisposable BindClickTo(this Button button, ICommand command)
    {
        UnityAction action = () => command.Execute();
        button.onClick.AddListener(action);

        Action refresh = () => button.interactable = command.CanExecute();
        command.CanExecuteChanged += refresh;
        refresh();

        return new ActionDisposable(() =>
        {
            button.onClick.RemoveListener(action);
            command.CanExecuteChanged -= refresh;
        });
    }

    public static T AddTo<T>(this T disposable, CompositeDisposable composite) where T : IDisposable
    {
        composite.Add(disposable);
        return disposable;
    }
}
```

Las lambdas se crean una vez al enlazar: después del setup, el binding no asigna memoria
por frame.

---

## 6. Contratos — ✅ en el repo

| Contrato | Miembros |
|---|---|
| `IARTrackingService` | `event Action<TrackedMonumentEvent> MonumentTrackingChanged` · `Task<bool> InitializeAsync(CancellationToken)` · `Task StopAsync()` · `bool IsTracking` |
| `IAudioPlaybackService` | `bool IsPlaying` · `event Action<AudioCue, PlaybackEventKind> PlaybackStateChanged` · `Task PlayAsync(AudioCue, CancellationToken)` · `PauseAsync` · `ResumeAsync` · `StopAsync` |
| `IChatbotClient : IDisposable` | `bool IsReady` · `event Action<ChatbotErrorKind> ErrorOccurred` · `InitializeAsync(CancellationToken)` · `Task<string> QueryAsync(string, LanguageCode, CancellationToken, int timeoutSeconds = 15)` · `ResetSessionAsync(CancellationToken)` |
| `ILocalizationService` | `LanguageCode CurrentLanguage` · `void SetLanguage(LanguageCode)` · `event Action<LanguageCode> LanguageChanged` |
| `IMonumentRepository` | `Task<MonumentSnapshot> GetByIdAsync(string, CancellationToken)` · `Task<IReadOnlyList<MonumentSnapshot>> GetAllAsync(CancellationToken)` · `bool IsReady` |

`IMonumentRepository` es el primero a implementar (`CLAUDE.md` §7): una clase que
lee `MonumentCatalogSO` y devuelve `MonumentSnapshot`s.

> 🔁 `MonumentSnapshot` exige un `displayName` no vacío, y `MonumentDataSO` no tenía campo de
> nombre. Desde el 16/10/2026 tiene `DisplayName` (español), completo en los 5 `.asset` y
> cubierto por `MonumentCatalogAssetTests`. Si más adelante el nombre tiene que traducirse,
> pasa a ser un `LocalizedDescription`.

---

## 7. Convenciones de código — ✅ vigentes

| Regla | Ejemplo / motivo |
|---|---|
| Namespaces `SenderoAR.<Capa>.<Área>` alineados a la carpeta | `SenderoAR.Core.Infrastructure`, `SenderoAR.Models` |
| Ningún segmento final de namespace llamado `Debug`, `Object`, `Random`, `Time` | Chocan con `UnityEngine.*`. Por eso `Core.Diagnostics` y `Core/Materials/` |
| Directivas `#if` / `#endif` **siempre en su propia línea** | Pegadas a una llave, Unity dejó de compilar `OnValidate()` sin dar error (S2) |
| Código solo de editor dentro de `#if UNITY_EDITOR` | Costo cero en el player |
| `ScriptableObject` de datos → `OnValidate()` defensivo + validador en `Assets/Editor/` | Patrón de `MonumentDataSO` + `MonumentDataValidator` |
| Lookups `TryGetX(…, out T)` | Sin `null` de retorno ni excepciones |
| Enums en vez de strings | `LanguageCode`, `MonumentTrackingPhase`, `ChatbotErrorKind`, `PlaybackEventKind` |
| `readonly struct` para eventos frecuentes | `TrackedMonumentEvent`, `AudioCue` |
| Async con `Task` + `CancellationToken` en toda operación de E/S | Contratos de S0 |
| Comentarios `<remarks>` explican el **porqué** | Ver `ImageTrackingController` |
| Archivos `.cs` en UTF-8 | Hubo mojibake en S1 por Windows-1252 |

---

## 8. Anti-patrones prohibidos

- ❌ `class XViewModel : MonoBehaviour`.
- ❌ Notificaciones de propiedad con string literal.
- ❌ `FindObjectOfType`, `GameObject.Find` para resolver dependencias → `AppContext` o constructor.
- ❌ Crear decoradores fuera de `AppBootstrapper`.
- ❌ Mezclar lógica de demo y producción en la misma clase con `if (Application.isEditor)`.
- ❌ Dejar `DEMO_MODE` definido en un build publicable.
- ❌ Un watcher o controlador provisional que modifique la jerarquía de la escena: eso es de
  los servicios.
